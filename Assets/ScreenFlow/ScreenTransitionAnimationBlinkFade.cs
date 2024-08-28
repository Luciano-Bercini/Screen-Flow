using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Blink Fade", menuName = "Screen Transition/Blink Fade")]
public class ScreenTransitionAnimationBlinkFade : ScreenTransitionAnimation
{
    [SerializeField, Range(0f, 1f)] private float _fadeOutTime = 0.05f;
    [SerializeField] private Easing.Ease _fadeOutEase = Easing.Ease.Linear;
    [SerializeField, Range(0f, 1f)] private float _waitTimeBeforeFadeIn = 0.15f;
    [SerializeField, Range(0f, 1f)] private float _fadeInTime = 0.2f;
    [SerializeField] private Easing.Ease _fadeInEase = Easing.Ease.Linear;

    public override IEnumerator Animate(Image transitionImg, System.Action previousScreen, System.Action nextScreen)
    {
        float elapsedTime = 0f;
        while (elapsedTime < _fadeOutTime)
        {
            yield return null;
            elapsedTime += Time.unscaledDeltaTime;
            transitionImg.color = Color.Lerp(Color.clear, Color.black, Easing.Lerp(0f, 1f, elapsedTime / _fadeOutTime, _fadeOutEase));
        }
        transitionImg.color = Color.black;
        previousScreen.Invoke();
        yield return Wait.SecondsRealtime(_waitTimeBeforeFadeIn);
        nextScreen.Invoke();
        elapsedTime = 0f;
        while (elapsedTime < _fadeInTime)
        {
            yield return null;
            elapsedTime += Time.unscaledDeltaTime;
            transitionImg.color = Color.Lerp(Color.black, Color.clear, Easing.Lerp(0f, 1f, elapsedTime / _fadeInTime, _fadeInEase));
        }
        transitionImg.color = Color.clear;
    }
}