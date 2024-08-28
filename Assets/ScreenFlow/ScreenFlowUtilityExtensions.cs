using UnityEngine;

public static class ScreenFlowUtilityExtensions
{
    /// <summary> To check for active, it uses activeInHierarchy (checking if its active scene-wise). </summary>
    public static bool IsNullOrInactive(this GameObject go)
    {
        return go == null || !go.activeInHierarchy;
    }

}
