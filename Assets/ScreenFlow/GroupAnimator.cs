using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupAnimator : SimpleEaseAnimator
{
    [SerializeField] private bool _beginPlayingOnEnable = true;
    [SerializeField] private EaseAnimationIdentifier _defaultAnimIdentifier;
    [SerializeField] private List<EaseAnimator> _animators = new();

    private void OnEnable()
    {
        if (_beginPlayingOnEnable)
        {
            PlayWithId(_defaultAnimIdentifier);
        }
    }

    public override void Play()
    {
        if (_defaultAnimIdentifier == null)
        {
            Debug.LogWarning($"No default animatation ID assigned to {gameObject.name}!");
            return;
        }
        PlayWithId(_defaultAnimIdentifier);
    }

    public override void PlayWithId(EaseAnimationIdentifier anim)
    {
        foreach (EaseAnimator animator in _animators)
        {
            animator.PlayWithId(anim);
        }
    }

    public override IEnumerator PlayCoroutine(EaseAnimationIdentifier anim)
    {
        PlayWithId(anim);
        yield return WaitAllCoroutine(anim);
    }

    public override void Stop()
    {
        foreach (EaseAnimator animator in _animators)
        {
            animator.Stop();
        }
    }

    private IEnumerator WaitAllCoroutine(EaseAnimationIdentifier animID)
    {
        foreach (EaseAnimator animator in _animators)
        {
            if (IsPlayingWithID(animID, animator))
            {
                yield return Wait.Until(() => !IsPlayingWithID(animID, animator));
            }
        }
    }

    private bool IsPlayingWithID(EaseAnimationIdentifier animID, EaseAnimator animator)
    {
        return animator.IsPlaying && animator.CurrentAnimationPlayingID == animID;
    }

    [Button("Add children animators to the list")]
    private void GetChildrenAnimators()
    {
        EaseAnimator[] childrenEaseAnimators = GetComponentsInChildren<EaseAnimator>(true);
        foreach (EaseAnimator animator in childrenEaseAnimators)
        {
            if (!_animators.Contains(animator))
            {
                _animators.Add(animator);
            }
        }
    }
}