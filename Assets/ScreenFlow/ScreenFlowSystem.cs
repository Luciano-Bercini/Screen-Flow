using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ScreenFlowSystem : MonoBehaviour
{
    public static ScreenFlowSystem Instance { get; private set; }
    public bool IsProgramaticSelection { get; private set; }
    public PrimaryScreen CurrentMainUIScreen => _currentTransition.NextScreen;

    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private Image _transitionImg;
    [SerializeField] private Image _darkRaycastFilter;
    [SerializeField] private ScreenTransitionAnimation _defaultTransitionAnimationOnBackButton;
    [SerializeField] private EaseAnimationIdentifier _openPanelID;
    [SerializeField] private EaseAnimationIdentifier _closePanelID;
    [SerializeField] private InputActionReference _pointerDown;
    [SerializeField] private bool _transitionToScreenOnStart = true;
    [SerializeField, ShowIf("_transitionToScreenOnStart")]
    private PrimaryScreen _initialScreen;
    [SerializeField, ShowIf("_transitionToScreenOnStart")]
    private ScreenTransitionAnimation _initialScreenAnimation;
    [SerializeField] private List<UIScreen> _allScreens = new();

    private UIScreen LastUIScreen => _screensHistory[^1];
    // We use a list rather than a stack for panels because there is no assurance between push/pop order in UI events,
    // causing for example, an unwanted pop of a panel we are opening (e.g. push -> push -> pop).
    private readonly List<UIScreen> _screensHistory = new();
    private readonly Stack<PrimaryScreen> _primaryScreens = new();
    private Coroutine _previousTransitionCo, _currentTransitionCo;
    private PrimaryScreenTransition _currentTransition;
    private int _panelBackgroundShowStacks;
    private bool _isScreenTransitioning;
    private UIScreen _previousUIScreenForPersistentRaycastFilter;
    private readonly List<RaycastResult> _raycastResults = new();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        _pointerDown.action.performed += TryCloseOpenPanelsOnExternalTouch;
    }

    private void Start()
    {
        if (_transitionToScreenOnStart)
        {
            TransitionTo(_initialScreen, _initialScreenAnimation);
        }
    }

    private void Update()
    {
        HandleEscapeAction();
    }

    private void OnDisable()
    {
        _pointerDown.action.performed -= TryCloseOpenPanelsOnExternalTouch;
    }

    public void Register(UIScreen uiScreen)
    {
        _allScreens.Add(uiScreen);
    }

    public void Unregister(UIScreen uiScreen)
    {
        _allScreens.Remove(uiScreen);
    }

    private void TryOpenInstantaneous(UIScreen uiScreen)
    {
        if (!uiScreen.IsOpen)
        {
            uiScreen.OpenInstantaneous();
            SelectProperGameObject();
            if (uiScreen.HasDarkRaycastFilterPanel)
            {
                ShowDarkRaycastFilter();
            }
            SetDarkRaycasterFilterBehindCurrentSectionScreen();
        }
    }

    private void TryCloseInstantaneous(UIScreen uiScreen)
    {
        if (uiScreen.IsOpen)
        {
            uiScreen.UpdateLastSelection();
            uiScreen.CloseInstantaneous();
            if (uiScreen.HasDarkRaycastFilterPanel)
            {
                HideDarkRaycasterFilter();
                // Make sure the previous screen is back to interactable as well.
                LastUIScreen.CanvasGroup.interactable = true;
            }
            SetDarkRaycasterFilterBehindCurrentSectionScreen();
        }
    }

    /// <summary> Preferred way to select gameobjects compared to EventSystem.SetSelectedGameObject(GameObject) </summary>
    public void SelectGameObject(GameObject go)
    {
        UIScreen parentUIScreen = go.GetComponentInParent<UIScreen>();
        if (!parentUIScreen.IsOpen)
        {
            Debug.LogWarning($"Trying to select a gameobject that's under a closed screen: {parentUIScreen} {go}", go);
            return;
        }
        IsProgramaticSelection = true;
        _eventSystem.SetSelectedGameObject(go);
        IsProgramaticSelection = false;
    }

    private void SelectProperGameObject()
    {
        int lastScreenIndex = _screensHistory.Count - 1;
        for (int i = lastScreenIndex; i >= 0; i--)
        {
            UIScreen uiScreen = _screensHistory[i];
            if (uiScreen.IsOpen)
            {
                GameObject selection = uiScreen.LastSelection;
                if (selection.IsNullOrInactive() || !uiScreen.ResumeFromLastSelection)
                {
                    selection = uiScreen.DefaultSelection;
                    if (selection.IsNullOrInactive())
                    {
                        selection = uiScreen.SecondarySelection;
                    }
                }
                if (!selection.IsNullOrInactive())
                {
                    SelectGameObject(selection);
                }
                break;
            }
        }
    }

    public void TransitionTo(PrimaryScreen nextScreen, ScreenTransitionAnimation canvasTransitionAnimation, bool overridePreviousTransition = false)
    {
        int primaryScreensCount = _primaryScreens.Count;
        if (_primaryScreens.TryPeek(out var primaryScreen) && primaryScreen == nextScreen)
        {
            Debug.LogWarning("Trying to transition to the same open screen!");
            return;
        }
        // In the first transition, there may not be a previous screen, so we take the next one as our previous screen.
        PrimaryScreen previousScreen = primaryScreensCount == 0 ? nextScreen : _primaryScreens.Peek();
        _primaryScreens.Push(nextScreen);
        _screensHistory.Add(nextScreen);
        BeginTransition(new PrimaryScreenTransition(previousScreen, nextScreen, canvasTransitionAnimation), overridePreviousTransition);
    }

    public void TransitionBack(ScreenTransitionAnimation canvasTransitionAnimation, bool overridePreviousTransition = false)
    {
        if (_primaryScreens.Count <= 1)
        {
            Debug.Log("Trying to transition to a previous screen that doesn't exist!");
            return;
        }
        PrimaryScreen previousScreen = _primaryScreens.Pop();
        _screensHistory.Remove(previousScreen);
        PrimaryScreen nextScreen = _primaryScreens.Peek();
        BeginTransition(new PrimaryScreenTransition(previousScreen, nextScreen, canvasTransitionAnimation), overridePreviousTransition);
    }

    private void BeginTransition(PrimaryScreenTransition canvasTransition, bool overridePreviousTransition)
    {
        _previousTransitionCo = _currentTransitionCo;
        _currentTransitionCo = StartCoroutine(TransitionCoroutine(canvasTransition, overridePreviousTransition));
    }

    private IEnumerator TransitionCoroutine(PrimaryScreenTransition canvasTransition, bool overridePreviousTransition)
    {
        if (overridePreviousTransition)
        {
            if (_previousTransitionCo != null)
            {
                StopCoroutine(_previousTransitionCo);
            }
            TryCloseInstantaneous(canvasTransition.PreviousScreen);
        }
        // Wait its previous transition is done (if there is any).
        else if (_previousTransitionCo != null)
        {
            yield return _previousTransitionCo;
        }
        _isScreenTransitioning = true;
        _transitionImg.raycastTarget = true;
        _currentTransition = canvasTransition;
        yield return canvasTransition.Animation.Animate(_transitionImg, () => TryCloseInstantaneous(_currentTransition.PreviousScreen),
            () => TryOpenInstantaneous(_currentTransition.NextScreen));
        _transitionImg.raycastTarget = false;
        _isScreenTransitioning = false;
    }

    public void TryOpenSectionScreen(SectionScreen sectionScreen, bool animate = true)
    {
        if (sectionScreen.IsOpen)
        {
            return;
        }
        LastUIScreen.UpdateLastSelection();
        if (sectionScreen.HasDarkRaycastFilterPanel)
        {
            // Make sure the graphic raycaster is not interactable if another screen is going to open in front.
            LastUIScreen.CanvasGroup.interactable = false;
        }
        _screensHistory.Add(sectionScreen);
        TryOpenInstantaneous(sectionScreen);
        if (animate && sectionScreen.GroupAnimator != null)
        {
            sectionScreen.GroupAnimator.Stop();
            sectionScreen.GroupAnimator.PlayWithId(_openPanelID);
        }
    }

    public void TryCloseSectionScreen(SectionScreen sectionScreen, bool animate = true, System.Action callbackOnCloseAnimationEnd = null)
    {
        if (!sectionScreen.IsOpen)
        {
            return;
        }
        StartCoroutine(CloseSectionScreenCoroutine(sectionScreen, animate, callbackOnCloseAnimationEnd));
    }

    private IEnumerator CloseSectionScreenCoroutine(SectionScreen sectionScreen, bool animate, System.Action callbackOnClose)
    {
        _screensHistory.Remove(sectionScreen);
        // We don't want to be able to spam-close or interact with it as soon as we call on closing it.
        sectionScreen.ChangeInteractability(false);
        if (animate && sectionScreen.GroupAnimator != null)
        {
            sectionScreen.GroupAnimator.Stop();
            yield return sectionScreen.GroupAnimator.PlayCoroutine(_closePanelID);
            if (sectionScreen.GroupAnimator.LastPlayedIdentifier == _openPanelID)
            {
                yield break;
            }
        }
        TryCloseInstantaneous(sectionScreen);
        callbackOnClose?.Invoke();
        SelectProperGameObject();
    }

    public void ShowDarkRaycastFilter()
    {
        _panelBackgroundShowStacks++;
        if (_panelBackgroundShowStacks >= 1)
        {
            _darkRaycastFilter.enabled = true;
            _darkRaycastFilter.color = new Color(0f, 0f, 0f, 0.85f);
            _darkRaycastFilter.raycastTarget = true;
        }
        SetDarkRaycasterFilterBehindCurrentSectionScreen();
    }

    public void HideDarkRaycasterFilter()
    {
        _panelBackgroundShowStacks--;
        if (_panelBackgroundShowStacks <= 0)
        {
            _darkRaycastFilter.enabled = false;
            _darkRaycastFilter.color = Color.clear;
            _darkRaycastFilter.raycastTarget = false;
        }
    }

    private void SetDarkRaycasterFilterBehindCurrentSectionScreen()
    {
        int lastIndex = _screensHistory.Count - 1;
        for (int i = lastIndex; i >= 0; i--)
        {
            UIScreen uiScreen = _screensHistory[i];
            if (uiScreen.HasDarkRaycastFilterPanel || uiScreen is PrimaryScreen)
            {
                RectTransform raycastFilterTr = _darkRaycastFilter.rectTransform;
                Transform sectionTr = uiScreen.transform;
                if (uiScreen is SectionScreen)
                {
                    RectTransform sectionParentTr = sectionTr.parent as RectTransform;
                    raycastFilterTr.SetParent(sectionParentTr, true);
                    // The panel background is always behind the top panel.
                    // Setting a sibling to -1 sets it as the last sibling for some arcaic reasons.
                    // We first place the raycast filter as last sibling, then the panel. This avoids the use of SetSiblingByIndex() which can cause sorting issues.
                    raycastFilterTr.SetAsLastSibling();
                    sectionTr.SetAsLastSibling();
                }
                else
                {
                    // The screen has no parent, hence we set it under the screen.
                    raycastFilterTr.SetParent(sectionTr, true);
                    raycastFilterTr.SetAsFirstSibling();
                    // Make sure it fills the entire canvas after its moved.
                    if (uiScreen.TryGetComponent(out RectTransform screenRt))
                    {
                        raycastFilterTr.position = screenRt.position;
                        raycastFilterTr.sizeDelta = screenRt.sizeDelta;
                    }
                }
                // May be modified when setting as child of disabled canvas.
                raycastFilterTr.localScale = Vector3.one;
                break;
            }
        }
    }

    private void HandleEscapeAction()
    {
        // Somehow with the new input system, we also have to check for the "escape key" for the Android back button,
        // albeit it should be implicit in the cancel action.
        // Also, we don't use the UI default cancel action as it may be disabled (we always want to handle the escape action).
        var gamePad = Gamepad.current;
        var keyboard = Keyboard.current;
        bool backButtonPressed = gamePad != null && gamePad.bButton.wasPressedThisFrame || keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
        if (backButtonPressed)
        {
            int lastIndex = _screensHistory.Count - 1;
            for (int i = lastIndex; i >= 0; i--)
            {
                UIScreen uiScreen = _screensHistory[i];
                if (uiScreen.OnExitButtonAction == ScreenAction.Ignore)
                {
                    continue;
                }
                if (uiScreen is PrimaryScreen && _isScreenTransitioning)
                {
                    return;
                }
                TriggerScreenBackButtonAction(uiScreen);
                return;
            }
        }
    }

    private void TriggerScreenBackButtonAction(UIScreen uiScreen)
    {
        switch (uiScreen.OnExitButtonAction)
        {
            case ScreenAction.CloseScreen:
                if (uiScreen is PrimaryScreen)
                {
                    TransitionBack(_defaultTransitionAnimationOnBackButton);
                }
                else
                {
                    TryCloseSectionScreen(uiScreen as SectionScreen);
                }
                break;
            case ScreenAction.ExecuteButtonClick:
                _eventSystem.SimulateFullClick(uiScreen.ExecuteButtonOnClick);
                break;
        }
    }

    private void TryCloseOpenPanelsOnExternalTouch(InputAction.CallbackContext context)
    {
        if (LastUIScreen is PrimaryScreen)
        {
            return;
        }
        Vector2 position = context.ReadValue<Vector2>();
        if (LastUIScreen.OnExitButtonAction == ScreenAction.CloseScreen)
        {
            if (IsTouchingSomethingFromPanel(position)) return;
            TryCloseSectionScreen(LastUIScreen as SectionScreen);
        }
    }

    private bool IsTouchingSomethingFromPanel(Vector2 position)
    {
        PointerEventData pointerEventData = new(_eventSystem)
        {
            position = position
        };
        _eventSystem.RaycastAll(pointerEventData, _raycastResults);
        foreach (var result in _raycastResults)
        {
            if (result.isValid)
            {
                var touchedTarget = result.gameObject.transform;
                SectionScreen touchedTargetParent = touchedTarget.GetComponentInParent<SectionScreen>();
                var lastScreen = LastUIScreen;
                if (lastScreen != null)
                {
                    bool isInteractableTargetChildOfCurrentScreen = touchedTargetParent == LastUIScreen;
                    if (isInteractableTargetChildOfCurrentScreen)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private struct PrimaryScreenTransition
    {
        public PrimaryScreen PreviousScreen { get; }
        public PrimaryScreen NextScreen { get; }
        public ScreenTransitionAnimation Animation { get; }

        public PrimaryScreenTransition(PrimaryScreen previousScreen, PrimaryScreen nextScreen, ScreenTransitionAnimation animation)
        {
            PreviousScreen = previousScreen;
            NextScreen = nextScreen;
            Animation = animation;
        }
    }
}