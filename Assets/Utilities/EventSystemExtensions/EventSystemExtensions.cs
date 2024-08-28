using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> Utility class for input offering additional utility methods. </summary>
public static class EventSystemExtensions
{
    /// <summary> Execute the PointerDown, the PointerClick and the PointerUp events on a given target. </summary>
    public static void SimulateFullClick(this EventSystem eventSystem, GameObject target)
    {
        if (!IsTargetActive(target))
        {
            return;
        }
        PointerEventData eventData = new(eventSystem);
        ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerClickHandler);
        ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerUpHandler);
    }

    /// <summary> Execute the PointerClick event on a given target. </summary>
    public static void SimulateClick(this EventSystem eventSystem, GameObject target)
    {
        if (!IsTargetActive(target))
        {
            return;
        }
        PointerEventData eventData = new(eventSystem);
        ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerClickHandler);
    }

    /// <summary> Execute the Move event on a given target. </summary>
    public static void SimulateMove(this EventSystem eventSystem, GameObject target, MoveDirection moveDirection)
    {
        if (!IsTargetActive(target))
        {
            return;
        }
        AxisEventData eventData = new(eventSystem)
        {
            moveDir = moveDirection
        };
        ExecuteEvents.Execute(target, eventData, ExecuteEvents.moveHandler);
    }

    private static bool IsTargetActive(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("The target is null!");
            return false;
        }
        if (!target.activeInHierarchy)
        {
            Debug.LogWarning($"The target {target} is inactive!", target);
            return false;
        }
        return true;
    }
}