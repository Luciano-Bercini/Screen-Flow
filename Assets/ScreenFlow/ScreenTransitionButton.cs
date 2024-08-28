using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class ScreenTransitionButton : MonoBehaviour, IPointerClickHandler
{
    [FormerlySerializedAs("_toPreviousFrame")] [SerializeField] private bool _toPreviousScreen;
    [FormerlySerializedAs("_nextFrame")] [HideIf("_toPreviousScreen"), SerializeField]
    private PrimaryScreen _nextUIScreen;
    [SerializeField] private ScreenTransitionAnimation _transitionAnim;

    private ScreenFlowSystem _screenFlowSystem;

    private void Start()
    {
        _screenFlowSystem = ScreenFlowSystem.Instance;
        if (!_toPreviousScreen && _nextUIScreen == null)
        {
            Debug.LogWarning($"The next screen has not been set up on the transition button named: " +
                             $"{gameObject.name} under the parent {transform.parent.name}!");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Transition();
    }

    private void Transition()
    {
        if (_toPreviousScreen)
        {
            _screenFlowSystem.TransitionBack(_transitionAnim);
        }
        else
        {
            _screenFlowSystem.TransitionTo(_nextUIScreen, _transitionAnim);
        }
    }
}