using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Shared.Data.ChartInfo
{
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

        public static TimestampType[] TimestampTypes = {};

        protected TimestampType[] TimestampTypesP;
    
        public Storyboardable GetStoryboardableObject(float time) {
        
            if (TimestampTypesP == null) 
                TimestampTypesP = (TimestampType[])this.GetType().GetField("TimestampTypes").GetValue(null);
        
            Storyboardable obj = (Storyboardable)MemberwiseClone();
        
            foreach(TimestampType timestampType in TimestampTypesP) 
                try {
                    List<Timestamp> storyboard = Storyboard.FromType(timestampType.ID);
                
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
                catch (Exception e) 
                {
                    Debug.LogError(this.GetType() + " " + timestampType.ID + "\n" + e);
                }
            return obj;
        }

        protected Dictionary<string, float> CurrentValues;
    
        protected float CurrentTime;
    
        public virtual void Advance (float time) 
        {
            if (TimestampTypesP == null) 
                TimestampTypesP = (TimestampType[])GetType().GetField("TimestampTypes").GetValue(null);
        
            if (CurrentValues == null) 
            {
                CurrentValues = new Dictionary<string, float>();
            
                foreach (TimestampType timestampType in TimestampTypesP) 
                    CurrentValues.Add(timestampType.ID, timestampType.Get(this));
            }
        
            foreach(TimestampType timestampType in TimestampTypesP)
                try {
                    float value = CurrentValues[timestampType.ID];
                
                    while (true) 
                    {
                        Timestamp timestamp = Storyboard.Timestamps.Find(storyboardTimestamps => storyboardTimestamps.ID == timestampType.ID);
                    
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
                catch (Exception e) 
                {
                    Debug.LogError(this.GetType() + " " + timestampType.ID + "\n" + e);
                }
        
            CurrentTime = time;
        }
    }

    public abstract class DirtyTrackedStoryboardable : Storyboardable 
    {
        public bool IsDirty;

        public override void Advance (float time) 
        {
            if (TimestampTypesP == null) TimestampTypesP = (TimestampType[])this.GetType().GetField("TimestampTypes").GetValue(null);
            if (CurrentValues == null) 
            {
                CurrentValues = new Dictionary<string, float>();
                foreach (TimestampType tst in TimestampTypesP) 
                {
                    CurrentValues.Add(tst.ID, tst.Get(this));
                }
            }
            foreach(TimestampType tst in TimestampTypesP) try {
                float value = CurrentValues[tst.ID];
                while (true) 
                {
                    Timestamp ts = Storyboard.Timestamps.Find(x => x.ID == tst.ID);
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
            } catch (Exception e) {
                Debug.LogError(this.GetType() + " " + tst.ID + "\n" + e);
            }
            CurrentTime = time;
        }
    }
}