using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EaseAnimator : SimpleEaseAnimator
{
    public event System.Action<EaseAnimator, EaseAnimationIdentifier> OnSequenceEnd;
    public bool IsPlaying { get; private set; }
    public EaseAnimationIdentifier CurrentAnimationPlayingID { get; private set; }

    [SerializeField] private bool _beginPlayingOnEnable;
    [SerializeField] private bool _beginWithDefaultState;
    [SerializeField, ShowIf("_beginWithDefaultState")]
    private bool _animateScale = true;
    [SerializeField, ShowIf("@_beginWithDefaultState && _animateScale")]
    private Vector3 _initialScale = Vector3.one;
    [SerializeField, ShowIf("_beginWithDefaultState")]
    private bool _animateColor = true;
    [SerializeField, ShowIf("@_beginWithDefaultState && _animateColor")]
    private Color _initialColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private bool _useUnscaledTime;
    [SerializeField] private EaseAnimation _default;
    [SerializeField] private List<NamedEaseAnimation> _namedEaseAnimations = new();

    private EaseAnimation.EaseTransition _currentTransition;
    private Color _fromColor;
    private Vector3 _fromScale;
    private EaseAnimation.EaseTransition _lastTransition;
    private Coroutine _animCo;
    private Transform _tr;

    protected abstract Color GetColor();

    protected abstract void SetColor(Color color);

    private void Awake()
    {
        _tr = transform;
    }

    private void OnEnable()
    {
        if (_beginWithDefaultState)
        {
            SetToDefaultState();
        }
        if (_beginPlayingOnEnable)
        {
            Play();
        }
    }

    private void SetToDefaultState()
    {
        if (_animateColor)
        {
            SetColor(_initialColor);
        }
        if (_animateScale)
        {
            _tr.localScale = _initialScale;
        }
    }

    [Button("Play", ButtonHeight = 25), PropertySpace, DisableInEditorMode]
    public override void Play()
    {
        if (_default == null)
        {
            GameObject go = gameObject;
            Debug.LogWarning($"There is no default animation assigned to gameobject {go.name}!", go);
            return;
        }
        Stop();
        Play(_default);
    }

    public override void PlayWithId(EaseAnimationIdentifier id)
    {
        CurrentAnimationPlayingID = id;
        var namedAnimation = GetNamedEaseAnimation(id);
        if (namedAnimation != null)
        {
            Stop();
            Play(namedAnimation._animation);
        }
    }

    private NamedEaseAnimation GetNamedEaseAnimation(EaseAnimationIdentifier id)
    {
        NamedEaseAnimation namedAnimation = null;
        for (int i = 0; i < _namedEaseAnimations.Count; i++)
        {
            if (id == _namedEaseAnimations[i]._id)
            {
                namedAnimation = _namedEaseAnimations[i];
            }
        }
        return namedAnimation;
    }

    public void Play(EaseAnimation anim)
    {
        if (anim.Sequence.Count == 0)
        {
            Debug.LogWarning("There are no transitions to play!");
            return;
        }
        GameObject go = gameObject;
        if (!go.activeInHierarchy)
        {
            Debug.LogWarning($"The game object {go.name} is inactive, as such it cannot start a coroutine!", go);
            return;
        }
        Stop();
        _animCo = StartCoroutine(PlayCoroutine(anim));
    }

    public override void Stop()
    {
        IsPlaying = false;
        if (_animCo != null)
        {
            StopCoroutine(_animCo);
        }
    }

    public override IEnumerator PlayCoroutine(EaseAnimationIdentifier id)
    {
        var namedAnimation = GetNamedEaseAnimation(id);
        if (namedAnimation != null)
        {
            yield return PlayCoroutine(namedAnimation._animation);
        }
    }

    public IEnumerator PlayCoroutine(EaseAnimation anim)
    {
        IsPlaying = true;
        bool succession = true;
        int initialIndex = 0;
        int repetitions = 0;
        do
        {
            if (succession)
            {
                for (int i = initialIndex; i < anim.Sequence.Count; i++)
                {
                    yield return TransitionCoroutine(anim, i);
                }
            }
            else
            {
                for (int i = initialIndex; i >= 0; i--)
                {
                    yield return TransitionCoroutine(anim, i);
                }
            }
            if (anim.LoopBehaviour == EaseAnimation.Loop.PingPong)
            {
                initialIndex = initialIndex == 0 ? 1 : anim.Sequence.Count - 2;
                succession = !succession;
            }
            else
            {
                initialIndex = 0;
            }
            if (anim.WithMaxRepetitions)
            {
                repetitions++;
                if (repetitions >= anim.MaxRepetitions)
                {
                    break;
                }
            }
        } while (anim.LoopBehaviour != EaseAnimation.Loop.None);
        if (anim.ChainAnimation)
        {
            Play(anim.ChainedAnimation);
        }
        else
        {
            OnSequenceEnd?.Invoke(this, CurrentAnimationPlayingID);
        }
        IsPlaying = false;
    }

    private IEnumerator TransitionCoroutine(EaseAnimation anim, int transitionIndex)
    {
        _currentTransition = anim.Sequence[transitionIndex];
        _fromScale = _tr.localScale;
        _fromColor = GetColor();
        if (_currentTransition.Delay > 0f)
        {
            yield return Wait.SecondsGeneric(_currentTransition.Delay, _useUnscaledTime);
        }
        float elapsedTime = 0f;
        if (anim.UsePreviousStateAsDefault)
        {
            if (_lastTransition != null)
            {
                _fromScale = _lastTransition.EaseTransitionState.Scale;
                _fromColor = _lastTransition.EaseTransitionState.Color;
            }
        }
        if (anim.Fade)
        {
            float duration = Mathf.Max(0.00001f, _currentTransition.Duration); // Avoids division by 0 and allow the animation to live at least a frame.
            while (elapsedTime <= duration)
            {
                elapsedTime += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Easing.Lerp01(elapsedTime / duration, _currentTransition.Ease);
                if (anim.AnimateScale)
                {
                    _tr.localScale = Vector3.Lerp(_fromScale, _currentTransition.EaseTransitionState.Scale, t);
                }
                if (anim.AnimateColor)
                {
                    LerpColor(anim, _fromColor, _currentTransition.EaseTransitionState.Color, t);
                }
                yield return null;
            }
        }
        else
        {
            if (anim.AnimateScale)
                _tr.localScale = (_currentTransition.EaseTransitionState.Scale);
            if (anim.AnimateColor)
                LerpColor(anim, _fromColor, _currentTransition.EaseTransitionState.Color, 1f);
            while (elapsedTime <= _currentTransition.Duration)
            {
                elapsedTime += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }
        _lastTransition = _currentTransition;
    }

    // Lerps RGBA values according to the settings, and calls SetColor(Color).
    private void LerpColor(EaseAnimation anim, Color from, Color to, float t)
    {
        float r = from.r;
        float g = from.g;
        float b = from.b;
        float a = from.a;
        if (anim.AnimateRGB)
        {
            r = Mathf.Lerp(r, to.r, t);
            g = Mathf.Lerp(g, to.g, t);
            b = Mathf.Lerp(b, to.b, t);
        }
        if (anim.AnimateAlpha)
        {
            a = Mathf.Lerp(a, to.a, t);
        }
        Color finalColor = new Color(r, g, b, a);
        SetColor(finalColor);
    }

    [System.Serializable]
    private class NamedEaseAnimation
    {
        [HorizontalGroup, HideLabel] public EaseAnimationIdentifier _id;
        [HorizontalGroup, HideLabel] public EaseAnimation _animation;
    }
}