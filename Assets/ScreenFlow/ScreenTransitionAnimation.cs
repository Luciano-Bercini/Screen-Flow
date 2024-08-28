using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class ScreenTransitionAnimation : ScriptableObject
{
    public abstract IEnumerator Animate(Image transitionImg, System.Action previousScreen, System.Action nextScreen);
}
