using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(-1), RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
public abstract class UIScreen : MonoBehaviour
{
    public event System.Action<bool> OnStateChanged;
    public GameObject DefaultSelection
    {
        get => _defaultSelection;
        set => _defaultSelection = value;
    }
    [ShowInInspector, ReadOnly] public GameObject LastSelection { get; private set; }
    public bool IsOpen { get; private set; }
    public GameObject SecondarySelection => _secondarySelection;
    public bool ResumeFromLastSelection => _resumeFromLastSelection;
    public CanvasGroup CanvasGroup => _canvasGroup;
    public bool StartClosed => _startClosed;
    public bool HasDarkRaycastFilterPanel => _hasDarkRaycastFilterPanel;
    public ScreenAction OnExitButtonAction => _onExitButtonAction;
    public GameObject ExecuteButtonOnClick => _executeButtonOnClick;

    [SerializeField, Required] protected Canvas _canvas;
    [SerializeField, Required] protected CanvasGroup _canvasGroup;
    [SerializeField] protected EventSystem _eventSystem;
    [SerializeField] protected GraphicRaycaster _graphicRaycaster;
    [SerializeField] private GameObject _defaultSelection;
    [SerializeField, Tooltip("Useful when the default selection is destroyed or inactive")]
    private GameObject _secondarySelection;
    [SerializeField] private bool _resumeFromLastSelection = true;
    [SerializeField] private bool _startClosed = true;
    [SerializeField] private bool _hasDarkRaycastFilterPanel;
    [SerializeField] private ScreenAction _onExitButtonAction = ScreenAction.CloseScreen;
    [SerializeField, ShowIf("_onExitButtonAction", ScreenAction.ExecuteButtonClick)]
    private GameObject _executeButtonOnClick;

    private ScreenFlowSystem _screenFlowSystem;

    protected virtual void Start()
    {
        _screenFlowSystem = ScreenFlowSystem.Instance;
        _screenFlowSystem.Register(this);
        if (_eventSystem == null)
        {
            _eventSystem = EventSystem.current;
        }
        // It's fine to close or open here as they don't have to go in the history stack.
        if (_startClosed)
        {
            CloseInstantaneous();
        }
        else
        {
            OpenInstantaneous();
        }
    }

    private void OnDestroy()
    {
        if (didStart) // Might happen on inactive gameobjects.
        {
            _screenFlowSystem.Unregister(this);
        }
    }

    public void UpdateLastSelection()
    {
        // If the screen itself doesn't have a default selection, then we don't want to set a last selection.
        // Usually a screen has no default selection when its only visual and has no buttons (e.g. gameplay screen).
        if (_defaultSelection != null || _secondarySelection != null)
        {
            GameObject currentSelection = _eventSystem.currentSelectedGameObject;
            if (currentSelection != null)
            {
                var parentScreen = currentSelection.GetComponentInParent<UIScreen>();
                // Make sure to not update LastSelection on gameobjects that are not even under the screen.
                if (parentScreen != null && parentScreen == this)
                {
                    LastSelection = currentSelection;
                }
            }
        }
    }

    public void CloseInstantaneous()
    {
        ChangeState(false);
    }

    public virtual void OpenInstantaneous()
    {
        ChangeState(true);
    }

    public void ChangeInteractability(bool openState)
    {
        _canvasGroup.interactable = openState;
        // Making sure the screen cannot be interacted with when its closed (helps in performance as well as its excluded from raycasts target).
        // The screen might be visual only, in that case the graphic raycaster would be missing, hence the check.
        if (_graphicRaycaster)
        {
            _graphicRaycaster.enabled = openState;
        }
    }

    private void ChangeState(bool openState)
    {
        IsOpen = openState;
        _canvas.enabled = openState;
        _canvasGroup.alpha = openState ? 1f : 0f;
        ChangeInteractability(openState);
        OnStateChanged?.Invoke(openState);
    }

#if UNITY_EDITOR
    protected virtual void Reset()
    {
        _canvas = GetComponent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _graphicRaycaster = GetComponent<GraphicRaycaster>();
        _eventSystem = FindFirstObjectByType<EventSystem>();
    }

    // A method that serves the purpose to preview a screen in the Editor by changing the canvas group's alpha.
    // We use this simpler method to not mess with runtime data (the canvas group's alpha is reset at Start() so it's not an issue).
    public void ChangeVisibilityInEditor(bool isVisible)
    {
        // The canvas is not enabled/disabled as it leads to calculations issues (from the layouts) on game-start.
        _canvasGroup.alpha = isVisible ? 1f : 0f;
    }
#endif
}

public enum ScreenAction
{
    Ignore = -1,
    CloseScreen = 0,
    ExecuteButtonClick = 1,
    Nothing = 2
}