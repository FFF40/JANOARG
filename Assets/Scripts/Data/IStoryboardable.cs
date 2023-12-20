using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

[Serializable]
public class Timestamp : IDeepClonable<Timestamp>
{
    [FormerlySerializedAs("Time")]
    public BeatPosition Offset;
    public float Duration;
    public string ID;
    public float From = float.NaN;
    public float Target;
    public EaseFunction Easing = EaseFunction.Linear;
    public EaseMode EaseMode;

    public Timestamp DeepClone()
    {
        Timestamp clone = new Timestamp() 
        {
            Offset = Offset,
            Duration = Duration,
            ID = ID,
            From = From,
            Target = Target,
            Easing = Easing,
            EaseMode = EaseMode,
        };
        return clone;
    }
}

public class TimestampType {
    public string ID;
    public string Name;
    public Func<IStoryboardable, float> Get;
    public Action<IStoryboardable, float> Set;
}

[Serializable]
public class Storyboard 
{
    public List<Timestamp> Timestamps = new List<Timestamp>();

    public void Add(Timestamp timestamp) {
        Timestamps.Add(timestamp);
        Timestamps.Sort((x, y) => x.Offset.CompareTo(y.Offset));
    }

    public List<Timestamp> FromType(string type) {
        return Timestamps.FindAll(x => x.ID == type);
    }

    public Storyboard DeepClone()
    {
        Storyboard clone = new Storyboard();
        foreach (Timestamp ts in Timestamps) clone.Timestamps.Add(ts.DeepClone());
        return clone;
    }
}

[Serializable]
public enum EaseMode 
{
    In, Out, InOut
}

[Serializable]
public enum EaseFunction
{
    Linear,
    Sine,
    Quadratic,
    Cubic,
    Quartic,
    Quintic,
    Exponential,
    Circle
}

[Serializable]
public class Ease 
{
    public Func<float, float> In;
    public Func<float, float> Out;
    public Func<float, float> InOut;

    public static float Get(float x, EaseFunction ease, EaseMode mode) 
    {
        Ease _ease = Eases[ease];
        var func = _ease.InOut;
        if (mode == EaseMode.In) func = _ease.In;
        if (mode == EaseMode.Out) func = _ease.Out;
        return func(Mathf.Clamp01(x));
    }

    public static Dictionary<EaseFunction, Ease> Eases = new Dictionary<EaseFunction, Ease>() {
        {EaseFunction.Linear, new Ease {
            In = (x) => x,
            Out = (x) => x,
            InOut = (x) => x,
        }},
        {EaseFunction.Sine, new Ease {
            In = (x) => 1 - Mathf.Cos((x * Mathf.PI) / 2),
            Out = (x) => Mathf.Sin((x * Mathf.PI) / 2),
            InOut = (x) => (1 - Mathf.Cos(x * Mathf.PI)) / 2,
        }},
        {EaseFunction.Quadratic, new Ease {
            In = (x) => x * x,
            Out = (x) => 1 - Mathf.Pow(1 - x, 2),
            InOut = (x) => x < 0.5f ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2,
        }},
        {EaseFunction.Cubic, new Ease {
            In = (x) => x * x * x,
            Out = (x) => 1 - Mathf.Pow(1 - x, 3),
            InOut = (x) => x < 0.5f ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2,
        }},
        {EaseFunction.Quartic, new Ease {
            In = (x) => x * x * x * x,
            Out = (x) => 1 - Mathf.Pow(1 - x, 4),
            InOut = (x) => x < 0.5f ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2,
        }},
        {EaseFunction.Quintic, new Ease {
            In = (x) => x * x * x * x * x,
            Out = (x) => 1 - Mathf.Pow(1 - x, 5),
            InOut = (x) => x < 0.5f ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2,
        }},
        {EaseFunction.Exponential, new Ease {
            In = (x) => x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10) - 0.0009765625f * (1 - x),
            Out = (x) => x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x) + 0.0009765625f * x,
            InOut = (x) => x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? Mathf.Pow(2, 20 * x - 10) / 2 - 0.0009765625f * (1 - x) : (2 - Mathf.Pow(2, -20 * x + 10)) / 2 + 0.0009765625f * x,
        }},
        {EaseFunction.Circle, new Ease {
            In = (x) => 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2)),
            Out = (x) => Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2)),
            InOut = (x) => x < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2,
        }},
    };
}

public abstract class IStoryboardable
{
    public Storyboard Storyboard = new Storyboard();

    public static TimestampType[] TimestampTypes = {};

    TimestampType[] tts;
    
    public IStoryboardable Get(float time) {
        if (tts == null) tts = (TimestampType[])this.GetType().GetField("TimestampTypes").GetValue(null);
        IStoryboardable obj = (IStoryboardable)this.MemberwiseClone();
        foreach(TimestampType tst in tts) try {
            List<Timestamp> sb = Storyboard.FromType(tst.ID);
            float value = tst.Get(this);
            foreach (Timestamp ts in sb) 
            {
                if (time >= ts.Offset + ts.Duration) value = ts.Target;
                else if (time > ts.Offset) 
                {
                    if (!float.IsNaN(ts.From)) value = (float)ts.From;
                    Ease ease = Ease.Eases[ts.Easing];
                    Func<float, float> func = ease.InOut;
                    if (ts.EaseMode == EaseMode.In) func = ease.In;
                    else if (ts.EaseMode == EaseMode.Out) func = ease.Out;
                    value = Mathf.LerpUnclamped(value, ts.Target, func((time - ts.Offset) / ts.Duration));
                    break;
                }
                else break;
            }
            tst.Set(obj, value);
        } catch (Exception e) {
            Debug.LogError(this.GetType() + " " + tst.ID + "\n" + e);
        }
        return obj;
    }

    Dictionary<string, float> currentValues;
    float currentTime;
    public void Advance (float time) 
    {
        if (tts == null) tts = (TimestampType[])this.GetType().GetField("TimestampTypes").GetValue(null);
        if (currentValues == null) 
        {
            currentValues = new Dictionary<string, float>();
            foreach (TimestampType tst in tts) 
            {
                currentValues.Add(tst.ID, tst.Get(this));
            }
        }
        foreach(TimestampType tst in tts) try {
            float value = currentValues[tst.ID];
            while (true) 
            {
                Timestamp ts = Storyboard.Timestamps.Find(x => x.ID == tst.ID);
                if (ts == null || (time < ts.Offset && currentTime < ts.Offset)) break;
                else if (time < ts.Offset + ts.Duration)
                {
                    if (!float.IsNaN(ts.From)) currentValues[tst.ID] = value = (float)ts.From;
                    Ease ease = Ease.Eases[ts.Easing];
                    Func<float, float> func = ease.InOut;
                    if (ts.EaseMode == EaseMode.In) func = ease.In;
                    else if (ts.EaseMode == EaseMode.Out) func = ease.Out;
                    value = Mathf.LerpUnclamped(value, ts.Target, func((time - ts.Offset) / ts.Duration));
                    break;
                }
                else
                {
                    currentValues[tst.ID] = value = ts.Target;
                    Storyboard.Timestamps.Remove(ts);
                }
            }
            tst.Set(this, value);
        } catch (Exception e) {
            Debug.LogError(this.GetType() + " " + tst.ID + "\n" + e);
        }
        currentTime = time;
    }
}
