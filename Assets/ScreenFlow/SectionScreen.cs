using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class SectionScreen : UIScreen
{
    public PrimaryScreen ParentPrimaryScreen => _parentMainUIScreen;
    public SimpleEaseAnimator GroupAnimator => _groupAnimator;

    [SerializeField] private SimpleEaseAnimator _groupAnimator;
    [SerializeField, ValidateInput("IsParentScreenValid", "The parent screen is not correct!", InfoMessageType.Warning)]
    private PrimaryScreen _parentMainUIScreen;

    [FormerlySerializedAs("_autoCloseOnParentFrameClose")] [SerializeField] private bool _autoCloseOnParentScreenClose = true;
    [FormerlySerializedAs("_hasDynamicMainFrame")] [SerializeField, Tooltip("Allows the section screen move from a screen to another as it is opened.")]
    private bool _hasDynamicPrimaryScreen;

    protected override void Start()
    {
        base.Start();
        // The component might be on a prefab; in such case its not always possible to manually assign a parent screen.
        if (_parentMainUIScreen == null)
        {
            _parentMainUIScreen = GetComponentInParent<PrimaryScreen>(true);
            if (_parentMainUIScreen == null)
            {
                GameObject go = gameObject;
                Debug.LogWarning($"Parent screen from section screen {go} cannot be found!", go);
                return;
            }
        }
        _parentMainUIScreen.OnStateChanged += ModifyStateOnParentUIScreenStateChange;
    }

    public override void OpenInstantaneous()
    {
        if (_hasDynamicPrimaryScreen)
        {
            DynamicallyChangeParentPrimaryScreen();
        }
        base.OpenInstantaneous();
    }

    private void DynamicallyChangeParentPrimaryScreen()
    {
        // Remove handler from previous parent, if any.
        _parentMainUIScreen.OnStateChanged -= ModifyStateOnParentUIScreenStateChange;
        // Change to the new screen.
        _parentMainUIScreen = ScreenFlowSystem.Instance.CurrentMainUIScreen;
        if (TryGetComponent(out RectTransform sectionScreenRt))
        {
            sectionScreenRt.SetParent(_parentMainUIScreen.transform);
            sectionScreenRt.SetAsLastSibling();
            _parentMainUIScreen.OnStateChanged += ModifyStateOnParentUIScreenStateChange;
        }
    }

    private void ModifyStateOnParentUIScreenStateChange(bool state)
    {
        if (_autoCloseOnParentScreenClose)
        {
            if (!state)
            {
                ScreenFlowSystem.Instance.TryCloseSectionScreen(this, false);
            }
        }
        // We don't want the graphic raycaster to stay enabled if the parent screen is closed, as we can potentially still interact with it.
        // We want to bring it back to enabled in case the panel is still open as we're back to that screen.
        if (IsOpen)
        {
            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = state;
            }
            _canvasGroup.interactable = state;
        }
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        _parentMainUIScreen = GetComponentInParent<PrimaryScreen>();
    }

    // ReSharper disable once UnusedMember.Local
    private bool IsParentScreenValid()
    {
        if (_parentMainUIScreen == null)
        {
            return true;
        }
        var actualParentScreen = GetComponentInParent<PrimaryScreen>(true);
        return _parentMainUIScreen == actualParentScreen;
    }

    [PropertySpace, Button("Show in editor", ButtonHeight = 25)]
    private void ShowInTheEditor()
    {
        ChangeVisibilityInEditor(_canvasGroup.alpha == 0f);
    }
#endif
}