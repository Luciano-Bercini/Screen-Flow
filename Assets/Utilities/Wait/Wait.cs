using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this class when you need to wait inside coroutines.
/// It provides the optimization of caching the Wait objects and further features.
/// </summary>
public static class Wait
{
    public static readonly WaitForFixedUpdate FixedUpdate;
    public static readonly WaitForEndOfFrame EndOfFrame;

    // Until() and While() methods are here for consistency (they use the default implementation).
    public static WaitUntil Until(System.Func<bool> predicate) => new(predicate);

    public static WaitWhile While(System.Func<bool> predicate) => new(predicate);

    private static readonly Dictionary<float, WaitForSeconds> TimeIntervals;

    static Wait() // A static constructor works well with domain reload disabled as well.
    {
        FixedUpdate = new WaitForFixedUpdate();
        EndOfFrame = new WaitForEndOfFrame();
        TimeIntervals = new Dictionary<float, WaitForSeconds>(32);
    }

    public static WaitForSeconds Seconds(float seconds)
    {
        if (TimeIntervals.TryGetValue(seconds, out WaitForSeconds value))
        {
            return value;
        }
        value = new WaitForSeconds(seconds);
        TimeIntervals[seconds] = value;
        return value;
    }

    /// <summary> Min [inclusive], max [inclusive]. </summary>
    public static WaitForSeconds SecondsRandom(int min, int max)
    {
        // We use the pool since integers are likely to be cached/re-used.
        int randomSeconds = Random.Range(min, max + 1);
        return Seconds(randomSeconds);
    }

    /// <summary> Min [inclusive], max [inclusive]. Does not used cached values. </summary>
    public static WaitForSeconds SecondsRandom(float min, float max) => new(Random.Range(min, max));

    /// <summary> By default WaitForSecondsRealtime is not cached and results in a new object everytime it's invoked.
    /// Remember that in case you want to cache WaitForSecondsRealtime objects,
    /// make sure they are not used concurrently (shared between more) but they are sequential with each other. </summary>
    public static WaitForSecondsRealtime SecondsRealtime(float seconds) => new(seconds);

    /// <summary> Waits between scaled and unscaled seconds based on the provided boolean value. </summary>
    public static IEnumerator SecondsGeneric(float seconds, bool unscaled)
    {
        if (unscaled)
        {
            yield return SecondsRealtime(seconds);
        }
        else
        {
            yield return Seconds(seconds);
        }
    }

    private static float GenericDeltaTime(bool unscaled)
    {
        return unscaled ? Time.unscaledTime : Time.deltaTime;
    }

    /// <summary> Allows to yield for a dynamic amount of time (so you can update the remaining seconds while it waits). </summary>
    public class DynamicWait
    {
        public float RemainingSeconds { get; set; }

        public DynamicWait(float seconds)
        {
            RemainingSeconds = seconds;
        }

        public IEnumerator DynamicYield(bool unscaledDeltaTime)
        {
            while (RemainingSeconds > 0f)
            {
                yield return null;
                RemainingSeconds -= GenericDeltaTime(unscaledDeltaTime);
            }
        }
    }
}