using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class SectionScreenTransitionButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField, Required] private SectionScreen _panel;
    [SerializeField] private bool _shouldOpen = true;

    private ScreenFlowSystem _screenFlowSystem;

    private void Start()
    {
        _screenFlowSystem = ScreenFlowSystem.Instance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Transition();
    }

    private void Transition()
    {
        if (_shouldOpen)
        {
            _screenFlowSystem.TryOpenSectionScreen(_panel);
        }
        else
        {
            _screenFlowSystem.TryCloseSectionScreen(_panel);
        }
    }
}