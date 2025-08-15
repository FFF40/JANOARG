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
    public Storyboard Storyboard = new();

    public abstract TimestampType[] TimestampTypes { get; }

    protected TimestampType[] TimestampTypesP;
    
    public Storyboardable GetStoryboardableObject(float time) {
        
        if (TimestampTypesP == null) 
            TimestampTypesP = TimestampTypes;
        
        Storyboardable obj = (Storyboardable)MemberwiseClone();
        
        foreach(TimestampType timestampType in TimestampTypesP) 
        {
            List<Timestamp> storyboard = Storyboard.FromType(timestampType.ID);

            if (storyboard.Count == 0)
            {
                // Debug.LogError(this.GetType() + " " + tst.ID + "\n" + "TimestampType not found in storyboard.");
                continue;
            }

            float value = timestampType.Get(this);

            foreach (Timestamp timestamp in storyboard) 
            {
                if (time >= timestamp.Offset + timestamp.Duration) 
                    value = timestamp.Target;
                else if (time > timestamp.Offset) 
                {
                    if (!float.IsNaN(timestamp.From)) 
                        value = timestamp.From;

                    value = Mathf.LerpUnclamped(
                        value, 
                        timestamp.Target, 
                        timestamp.Easing.Get((time - timestamp.Offset) / timestamp.Duration)
                        );

                    break;
                }
                else 
                    break;
            }
            timestampType.Set(obj, value);
        }
        return obj;
    }

    protected Dictionary<string, float> CurrentValues;
    
    protected float CurrentTime;
    
    public virtual void Advance (float time) 
    {
        if (TimestampTypesP == null) 
            TimestampTypesP = TimestampTypes;
        
        if (CurrentValues == null) 
        {
            CurrentValues = new Dictionary<string, float>();
            
            foreach (TimestampType timestampType in TimestampTypesP) 
                CurrentValues.Add(timestampType.ID, timestampType.Get(this));
        }
        
        foreach(TimestampType timestampType in TimestampTypesP)
        {
            if (!CurrentValues.ContainsKey(timestampType))
            {
                Debug.LogError(this.GetType() + " " + tst.ID + "\n" + "TimestampType not found in currentValues.");
                continue;
            }
            float value = CurrentValues[timestampType.ID];
            
            while (true) 
            {
                Timestamp timestamp = null;
                var storyboardTimestamps = Storyboard.Timestamps;
                for (int i = 0; i < storyboardTimestamps.Count; i++)
                {
                    if (storyboardTimestamps[i].ID == timestampType.ID)
                    {
                        timestamp = storyboardTimestamps[i];
                    }
                }
                
                if (timestamp == null || (time < timestamp.Offset && CurrentTime < timestamp.Offset))
                    break;

                if (time < timestamp.Offset + timestamp.Duration)
                {
                    if (!float.IsNaN(timestamp.From))
                        CurrentValues[timestampType.ID] = value = timestamp.From;
                    
                    value = Mathf.LerpUnclamped(value, timestamp.Target, timestamp.Easing.Get((time - timestamp.Offset) / timestamp.Duration));
                    
                    break;
                }
                else
                {
                    CurrentValues[timestampType.ID] = value = timestamp.Target;
                    Storyboard.Timestamps.Remove(timestamp);
                }
            }
            timestampType.Set(this, value);
        }
        
        CurrentTime = time;
    }
}

public abstract class DirtyTrackedStoryboardable : Storyboardable 
{
    public bool IsDirty;

    public override void Advance (float time) 
    {
        if (TimestampTypesP == null) TimestampTypesP = TimestampTypes;
        if (CurrentValues == null) 
        {
            CurrentValues = new Dictionary<string, float>();
            foreach (TimestampType tst in TimestampTypesP) 
            {
                CurrentValues.Add(tst.ID, tst.Get(this));
            }
        }
        foreach(TimestampType tst in TimestampTypesP){
            if (!CurrentValues.ContainsKey(tst))
            {
                Debug.LogError(this.GetType() + " " + tst.ID + "\n" + "TimestampType not found in currentValues.");
                continue;
            }
            float value = CurrentValues[tst.ID];
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
                if (ts == null || (time < ts.Offset && CurrentTime < ts.Offset)) break;
                else if (time < ts.Offset + ts.Duration)
                {
                    if (!float.IsNaN(ts.From)) CurrentValues[tst.ID] = value = ts.From;
                    value = Mathf.LerpUnclamped(value, ts.Target, ts.Easing.Get((time - ts.Offset) / ts.Duration));
                    IsDirty = true;
                    break;
                }
                else
                {
                    CurrentValues[tst.ID] = value = ts.Target;
                    Storyboard.Timestamps.Remove(ts);
                    IsDirty = true;
                }
            }
            tst.Set(this, value);
        }
        CurrentTime = time;
    }
}
