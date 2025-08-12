using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;
using System.Reflection;

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

    protected TimestampType[] tts;
    
    public Storyboardable Get(float time) {
        if (tts == null) tts = TimestampTypes;
        Storyboardable obj = this;
        foreach(TimestampType tst in tts){
            List<Timestamp> sb = Storyboard.FromType(tst.ID);
            if (sb.Count == 0)
            {
                // Debug.LogError(this.GetType() + " " + tst.ID + "\n" + "TimestampType not found in storyboard.");
                continue;
            }
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
        }
        return obj;
    }

    protected Dictionary<TimestampType, float> currentValues;
    protected float currentTime;
    public virtual void Advance (float time) 
    {
        if (tts == null) tts = TimestampTypes;
        if (currentValues == null) 
        {
            currentValues = new Dictionary<TimestampType, float>();
            foreach (TimestampType tst in tts) 
            {
                currentValues.Add(tst, tst.Get(this));
            }
        }
        foreach(TimestampType tst in tts){
            if (!currentValues.ContainsKey(tst))
            {
                Debug.LogError(this.GetType() + " " + tst.ID + "\n" + "TimestampType not found in currentValues.");
                continue;
            }
            float value = currentValues[tst];
            while (true) 
            {
                Timestamp ts = null;
                var timestamps = Storyboard.Timestamps;
                for (int i = 0; i < timestamps.Count; i++)
                {
                    if (timestamps[i].ID == tst.ID)
                    {
                        ts = timestamps[i];
                        break;
                    }
                }
                if (ts == null || (time < ts.Offset && currentTime < ts.Offset)) break;
                else if (time < ts.Offset + ts.Duration)
                {
                    if (!float.IsNaN(ts.From)) currentValues[tst] = value = ts.From;
                    value = Mathf.LerpUnclamped(value, ts.Target, ts.Easing.Get((time - ts.Offset) / ts.Duration));
                    break;
                }
                else
                {
                    currentValues[tst] = value = ts.Target;
                    Storyboard.Timestamps.Remove(ts);
                }
            }
            tst.Set(this, value);
        }
        currentTime = time;
    }
}

public abstract class DirtyTrackedStoryboardable : Storyboardable {
    public bool IsDirty;

    public override void Advance (float time) 
    {
        if (tts == null) tts = TimestampTypes;
        if (currentValues == null) 
        {
            currentValues = new Dictionary<TimestampType, float>();
            foreach (TimestampType tst in tts) 
            {
                currentValues.Add(tst, tst.Get(this));
            }
        }
        foreach(TimestampType tst in tts){
            if (!currentValues.ContainsKey(tst))
            {
                Debug.LogError(this.GetType() + " " + tst.ID + "\n" + "TimestampType not found in currentValues.");
                continue;
            }
            float value = currentValues[tst];
            while (true) 
            {
                Timestamp ts = null;
                var timestamps = Storyboard.Timestamps;
                for (int i = 0; i < timestamps.Count; i++)
                {
                    if (timestamps[i].ID == tst.ID)
                    {
                        ts = timestamps[i];
                        break;
                    }
                }
                if (ts == null || (time < ts.Offset && currentTime < ts.Offset)) break;
                else if (time < ts.Offset + ts.Duration)
                {
                    if (!float.IsNaN(ts.From)) currentValues[tst] = value = ts.From;
                    value = Mathf.LerpUnclamped(value, ts.Target, ts.Easing.Get((time - ts.Offset) / ts.Duration));
                    IsDirty = true;
                    break;
                }
                else
                {
                    currentValues[tst] = value = ts.Target;
                    Storyboard.Timestamps.Remove(ts);
                    IsDirty = true;
                }
            }
            tst.Set(this, value);
        }
        currentTime = time;
    }
}
