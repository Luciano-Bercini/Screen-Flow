using System.Collections;
using UnityEngine;

public abstract class SimpleEaseAnimator : MonoBehaviour
{
    public EaseAnimationIdentifier LastPlayedIdentifier => _lastPlayedIdentifier;
    
    private EaseAnimationIdentifier _lastPlayedIdentifier;
    
    public abstract void Play();

    protected void Play(EaseAnimationIdentifier anim)
    {
        _lastPlayedIdentifier = anim;
        PlayWithId(anim);
    }
    
    public abstract void PlayWithId(EaseAnimationIdentifier anim);

    public abstract void Stop();

    public abstract IEnumerator PlayCoroutine(EaseAnimationIdentifier anim);
}
