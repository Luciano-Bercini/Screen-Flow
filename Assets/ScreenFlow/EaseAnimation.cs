using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Ease/Ease Animation")]
public class EaseAnimation : ScriptableObject
{
    public bool WithMaxRepetitions => withMaxRepetitions;
    public int MaxRepetitions => repetitions;
    public bool Fade = true;
    [ShowIf("Fade")] public bool UsePreviousStateAsDefault;
    public Loop LoopBehaviour = Loop.None;
    public bool AnimateColor = true;
    [ShowIf("AnimateColor"), Indent] public bool AnimateRGB = true;
    [ShowIf("AnimateColor"), Indent] public bool AnimateAlpha = true;
    public bool AnimateScale = true;
    public List<EaseTransition> Sequence = new List<EaseTransition>(1);
    public bool ChainAnimation;
    [ShowIf("ChainAnimation"), InlineEditor, ValidateInput("AnimationCheck")]
    public EaseAnimation ChainedAnimation;

    public enum Loop
    {
        None = 0,
        PingPong = 1,
        LoopBack = 2
    }

    [SerializeField, HideIf("LoopBehaviour", Loop.None)]
    public bool withMaxRepetitions;
    [SerializeField, Range(1, 100), HideIf("LoopBehaviour", Loop.None)]
    public int repetitions = 5;

    [System.Serializable]
    public class EaseTransition
    {
        public EaseTransitionState EaseTransitionState => _easeTransitionState;
        public Easing.Ease Ease => _ease;
        public float Duration => _duration;
        public float Delay => _delay;

        [FormerlySerializedAs("EaseTransitionState")] [SerializeField, HideLabel]
        private EaseTransitionState _easeTransitionState;
        [FormerlySerializedAs("Ease")] [SerializeField]
        private Easing.Ease _ease = Easing.Ease.Linear;
        [FormerlySerializedAs("Duration")] [SerializeField, Min(0f)]
        private float _duration = 1f;
        [FormerlySerializedAs("Delay")] [SerializeField, Min(0f)]
        private float _delay;
    }

    [System.Serializable]
    public class EaseTransitionState
    {
        public Vector3 Scale => _scale;
        public Color Color => _color;
        
        [FormerlySerializedAs("Scale"), SerializeField] private Vector3 _scale = Vector3.one;
        [FormerlySerializedAs("Color"), SerializeField] private Color _color = Color.white;
    }

#if UNITY_EDITOR
    private bool AnimationCheck()
    {
        return this != ChainedAnimation;
    }
#endif
}