using Sirenix.OdinInspector;
using UnityEngine;

public class PrimaryScreen : UIScreen
{
#if UNITY_EDITOR
    [PropertySpace, Button("Show in editor", ButtonHeight = 25)]
    private void ShowInTheEditor()
    {
        PrimaryScreen[] mainScreens = FindObjectsByType<PrimaryScreen>(FindObjectsSortMode.None);
        // Hide all other screens and show only this one.
        foreach (PrimaryScreen mainScreen in mainScreens)
        {
            if (mainScreen == this)
            {
                mainScreen.ChangeVisibilityInEditor(true);
            }
            else if (mainScreen.StartClosed)
            {
                mainScreen.ChangeVisibilityInEditor(false);
                // Hide it's children panels as well.
                SectionScreen[] sectionScreens = mainScreen.GetComponentsInChildren<SectionScreen>();
                foreach (SectionScreen sectionScreen in sectionScreens)
                {
                    sectionScreen.ChangeVisibilityInEditor(false);
                }
            }
        }
    }
#endif
}
