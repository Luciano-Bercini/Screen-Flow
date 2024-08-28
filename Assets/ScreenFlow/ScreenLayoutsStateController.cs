using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Gets all performance intensive UI components in a list and turns them off/on based on the screen's state. </summary>
[DisallowMultipleComponent, RequireComponent(typeof(PrimaryScreen))]
public class ScreenLayoutsStateController : MonoBehaviour
{
    [SerializeField, Required] private PrimaryScreen _uiScreen;

    private readonly List<MonoBehaviour> _componentsToDisable = new();
    private readonly List<RectMask2D> _rectMasks = new();
    private readonly List<ScrollRect> _scrollRects = new();
    private readonly List<LayoutGroup> _layoutGroups = new();

    private void Start()
    {
        GetComponentsInChildren(true, _rectMasks);
        GetComponentsInChildren(true, _scrollRects);
        GetComponentsInChildren(true, _layoutGroups);
        _componentsToDisable.AddRange(_rectMasks);
        _componentsToDisable.AddRange(_scrollRects);
        _componentsToDisable.AddRange(_layoutGroups);
        ControlStates();
    }

    private void OnDestroy()
    {
        _uiScreen.OnStateChanged -= ChangeEnabledStatus;
    }

    private void ControlStates()
    {
        ChangeEnabledStatus(_uiScreen.IsOpen);
        // Turn the objects on/off depending on the screen state.
        _uiScreen.OnStateChanged += ChangeEnabledStatus;
    }

    private void ChangeEnabledStatus(bool open)
    {
        foreach (MonoBehaviour mono in _componentsToDisable)
        {
            if (mono.enabled != open)
            {
                mono.enabled = open;
            }
        }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        _uiScreen = GetComponent<PrimaryScreen>();
    }
#endif
}