using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "None", menuName = "Screen Transition/None")]
public class ScreenTransitionAnimationNone : ScreenTransitionAnimation
{
    public override IEnumerator Animate(Image transitionImg, System.Action previousScreen, System.Action nextScreen)
    {
        previousScreen.Invoke();
        nextScreen.Invoke();
        yield break;
    }
}
