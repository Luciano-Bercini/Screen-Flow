using UnityEngine;

// For visualization refer to: https://easings.net/
public static class Easing
{
    private const float PI = Mathf.PI;
    private const float HalfPI = Mathf.PI / 2f;

    public enum Ease
    {
        Linear,
        QuadraticEaseIn,
        QuadraticEaseOut,
        QuadraticEaseInOut,
        CubicEaseIn,
        CubicEaseOut,
        CubicEaseInOut,
        QuarticEaseIn,
        QuarticEaseOut,
        QuarticEaseInOut,
        QuinticEaseIn,
        QuinticEaseOut,
        QuinticEaseInOut,
        SineEaseIn,
        SineEaseOut,
        SineEaseInOut,
        CircularEaseIn,
        CircularEaseOut,
        CircularEaseInOut,
        ExponentialEaseIn,
        ExponentialEaseOut,
        ExponentialEaseInOut,
        ElasticEaseIn,
        ElasticEaseOut,
        ElasticEaseInOut,
        BackEaseIn,
        BackEaseOut,
        BackEaseInOut,
        BounceEaseIn,
        BounceEaseOut,
        BounceEaseInOut
    }

    /// <summary> Returns the interpolated value from 0 to 1 using the specified ease function. Internally clamps the progress to [0, 1]. </summary>
    public static float Lerp01(float progress, Ease easeFunction)
    {
        progress = Mathf.Clamp01(progress);
        progress = GetEase(progress, easeFunction);
        return Interpolate(0f, 1f, progress);
    }

    /// <summary> Returns the interpolated value using the specified ease function. Internally clamps the progress to [0, 1]. </summary>
    public static float Lerp(float start, float end, float progress, Ease easeFunction)
    {
        progress = Mathf.Clamp01(progress);
        progress = GetEase(progress, easeFunction);
        return Interpolate(start, end, progress);
    }

    private static float Linear(float p)
    {
        return p;
    }

    private static float QuadraticEaseIn(float p)
    {
        return p * p;
    }

    private static float QuadraticEaseOut(float p)
    {
        return 1 - (1 - p) * (1 - p);
    }

    private static float QuadraticEaseInOut(float p)
    {
        return p < 0.5f ? 2 * p * p : (-2 * p * p) + (4 * p) - 1;
    }

    private static float CubicEaseIn(float p)
    {
        return p * p * p;
    }

    private static float CubicEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f + 1;
    }

    private static float CubicEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 4 * p * p * p;
        }
        else
        {
            float f = ((2 * p) - 2);
            return 0.5f * f * f * f + 1;
        }
    }

    private static float QuarticEaseIn(float p)
    {
        return p * p * p * p;
    }

    private static float QuarticEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f * (1 - p) + 1;
    }

    private static float QuarticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 8 * p * p * p * p;
        }
        else
        {
            float f = (p - 1);
            return -8 * f * f * f * f + 1;
        }
    }

    private static float QuinticEaseIn(float p)
    {
        return p * p * p * p * p;
    }

    private static float QuinticEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f * f * f + 1;
    }

    private static float QuinticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 16 * p * p * p * p * p;
        }
        else
        {
            float f = ((2 * p) - 2);
            return 0.5f * f * f * f * f * f + 1;
        }
    }

    private static float SineEaseIn(float p)
    {
        return Mathf.Sin((p - 1) * HalfPI) + 1;
    }

    private static float SineEaseOut(float p)
    {
        return Mathf.Sin(p * HalfPI);
    }

    private static float SineEaseInOut(float p)
    {
        return 0.5f * (1 - Mathf.Cos(p * PI));
    }

    private static float CircularEaseIn(float p)
    {
        return 1 - Mathf.Sqrt(1 - (p * p));
    }

    private static float CircularEaseOut(float p)
    {
        return Mathf.Sqrt((2 - p) * p);
    }

    private static float CircularEaseInOut(float p)
    {
        return p < 0.5f ? 0.5f * (1 - Mathf.Sqrt(1 - 4 * (p * p))) : 0.5f * (Mathf.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
    }

    private static float ExponentialEaseIn(float p)
    {
        return p == 0f ? p : Mathf.Pow(2, 10 * (p - 1));
    }

    private static float ExponentialEaseOut(float p)
    {
        return p == 1f ? p : 1 - Mathf.Pow(2, -10 * p);
    }

    private static float ExponentialEaseInOut(float p)
    {
        if (p == 0.0 || p == 1.0) return p;

        if (p < 0.5f)
        {
            return 0.5f * Mathf.Pow(2, (20 * p) - 10);
        }
        else
        {
            return -0.5f * Mathf.Pow(2, (-20 * p) + 10) + 1;
        }
    }

    private static float ElasticEaseIn(float p)
    {
        return Mathf.Sin(13 * HalfPI * p) * Mathf.Pow(2, 10 * (p - 1));
    }

    private static float ElasticEaseOut(float p)
    {
        return Mathf.Sin(-13 * HalfPI * (p + 1)) * Mathf.Pow(2, -10 * p) + 1;
    }

    private static float ElasticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * Mathf.Sin(13 * HalfPI * (2 * p)) * Mathf.Pow(2, 10 * ((2 * p) - 1));
        }
        return 0.5f * (Mathf.Sin(-13 * HalfPI * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
    }

    private static float BackEaseIn(float p)
    {
        return p * p * p - p * Mathf.Sin(p * PI);
    }

    private static float BackEaseOut(float p)
    {
        float f = (1 - p);
        return 1 - (f * f * f - f * Mathf.Sin(f * PI));
    }

    private static float BackEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            float f = 2 * p;
            return 0.5f * (f * f * f - f * Mathf.Sin(f * PI));
        }
        else
        {
            float f = (1 - (2 * p - 1));
            return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * PI))) + 0.5f;
        }
    }

    private static float BounceEaseIn(float p)
    {
        return 1 - BounceEaseOut(1 - p);
    }

    private static float BounceEaseOut(float p)
    {
        if (p < 4 / 11.0f)
        {
            return (121 * p * p) / 16.0f;
        }
        else if (p < 8 / 11.0f)
        {
            return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
        }
        else if (p < 9 / 10.0f)
        {
            return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
        }
        return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
    }

    private static float BounceEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * BounceEaseIn(p * 2);
        }
        return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
    }

    /// <summary> Maps each enum with the corresponding ease function. </summary>
    private static float GetEase(float progress, Ease easeFunction)
    {
        progress = easeFunction switch
        {
            Ease.QuadraticEaseOut => QuadraticEaseOut(progress),
            Ease.QuadraticEaseIn => QuadraticEaseIn(progress),
            Ease.QuadraticEaseInOut => QuadraticEaseInOut(progress),
            Ease.CubicEaseIn => CubicEaseIn(progress),
            Ease.CubicEaseOut => CubicEaseOut(progress),
            Ease.CubicEaseInOut => CubicEaseInOut(progress),
            Ease.QuarticEaseIn => QuarticEaseIn(progress),
            Ease.QuarticEaseOut => QuarticEaseOut(progress),
            Ease.QuarticEaseInOut => QuarticEaseInOut(progress),
            Ease.QuinticEaseIn => QuinticEaseIn(progress),
            Ease.QuinticEaseOut => QuinticEaseOut(progress),
            Ease.QuinticEaseInOut => QuinticEaseInOut(progress),
            Ease.SineEaseIn => SineEaseIn(progress),
            Ease.SineEaseOut => SineEaseOut(progress),
            Ease.SineEaseInOut => SineEaseInOut(progress),
            Ease.CircularEaseIn => CircularEaseIn(progress),
            Ease.CircularEaseOut => CircularEaseOut(progress),
            Ease.CircularEaseInOut => CircularEaseInOut(progress),
            Ease.ExponentialEaseIn => ExponentialEaseIn(progress),
            Ease.ExponentialEaseOut => ExponentialEaseOut(progress),
            Ease.ExponentialEaseInOut => ExponentialEaseInOut(progress),
            Ease.ElasticEaseIn => ElasticEaseIn(progress),
            Ease.ElasticEaseOut => ElasticEaseOut(progress),
            Ease.ElasticEaseInOut => ElasticEaseInOut(progress),
            Ease.BackEaseIn => BackEaseIn(progress),
            Ease.BackEaseOut => BackEaseOut(progress),
            Ease.BackEaseInOut => BackEaseInOut(progress),
            Ease.BounceEaseIn => BounceEaseIn(progress),
            Ease.BounceEaseOut => BounceEaseOut(progress),
            Ease.BounceEaseInOut => BounceEaseInOut(progress),
            _ => Linear(progress),
        };
        return progress;
    }

    private static float Interpolate(float start, float end, float progress) => start + (end - start) * progress;
}