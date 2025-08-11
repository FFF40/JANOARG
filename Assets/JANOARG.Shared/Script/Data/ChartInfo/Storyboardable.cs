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
    [SerializeReference]
    public IEaseDirective Easing = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);

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
        };
        return clone;
    }
}

public class TimestampType {
    public string ID;
    public string Name;
    public Func<Storyboardable, float> Get;
    public Action<Storyboardable, float> Set;
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

public abstract class Storyboardable
{
    public Storyboard Storyboard = new Storyboard();

    public abstract TimestampType[] TimestampTypes { get; }

    public Storyboardable Get(float time) {
        Storyboardable obj = (Storyboardable)this.MemberwiseClone();
        foreach(TimestampType tst in TimestampTypes) try {
            List<Timestamp> sb = Storyboard.FromType(tst.ID);
            float value = tst.Get(this);
            foreach (Timestamp ts in sb) 
            {
                if (time >= ts.Offset + ts.Duration) value = ts.Target;
                else if (time > ts.Offset) 
                {
                    if (!float.IsNaN(ts.From)) value = (float)ts.From;
                    value = Mathf.LerpUnclamped(value, ts.Target, ts.Easing.Get((time - ts.Offset) / ts.Duration));
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

    protected Dictionary<string, float> currentValues;
    protected float currentTime;
    public virtual void Advance (float time) 
    {
        if (currentValues == null) 
        {
            currentValues = new Dictionary<string, float>();
            foreach (TimestampType tst in TimestampTypes) 
            {
                currentValues.Add(tst.ID, tst.Get(this));
            }
        }
        foreach(TimestampType tst in TimestampTypes) try {
            float value = currentValues[tst.ID];
            while (true) 
            {
                Timestamp ts = Storyboard.Timestamps.Find(x => x.ID == tst.ID);
                if (ts == null || (time < ts.Offset && currentTime < ts.Offset)) break;
                else if (time < ts.Offset + ts.Duration)
                {
                    if (!float.IsNaN(ts.From)) currentValues[tst.ID] = value = ts.From;
                    value = Mathf.LerpUnclamped(value, ts.Target, ts.Easing.Get((time - ts.Offset) / ts.Duration));
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

public abstract class DirtyTrackedStoryboardable : Storyboardable {
    public bool IsDirty;

    public override void Advance (float time) 
    {
        if (currentValues == null) 
        {
            currentValues = new Dictionary<string, float>();
            foreach (TimestampType tst in TimestampTypes) 
            {
                currentValues.Add(tst.ID, tst.Get(this));
            }
        }
        foreach(TimestampType tst in TimestampTypes) try {
            float value = currentValues[tst.ID];
            while (true) 
            {
                Timestamp ts = Storyboard.Timestamps.Find(x => x.ID == tst.ID);
                if (ts == null || (time < ts.Offset && currentTime < ts.Offset)) break;
                else if (time < ts.Offset + ts.Duration)
                {
                    if (!float.IsNaN(ts.From)) currentValues[tst.ID] = value = ts.From;
                    value = Mathf.LerpUnclamped(value, ts.Target, ts.Easing.Get((time - ts.Offset) / ts.Duration));
                    IsDirty = true;
                    break;
                }
                else
                {
                    currentValues[tst.ID] = value = ts.Target;
                    Storyboard.Timestamps.Remove(ts);
                    IsDirty = true;
                }
            }
            tst.Set(this, value);
        } catch (Exception e) {
            Debug.LogError(this.GetType() + " " + tst.ID + "\n" + e);
        }
        currentTime = time;
    }
}
