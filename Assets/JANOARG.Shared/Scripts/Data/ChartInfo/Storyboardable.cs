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

        public float  Duration;
        public string ID;
        public float  From = float.NaN;
        public float  Target;

        [SerializeReference]
        public IEaseDirective Easing = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);

        public Timestamp DeepClone()
        {
            var clone = new Timestamp
            {
                Offset = Offset,
                Duration = Duration,
                ID = ID,
                From = From,
                Target = Target,
                Easing = Easing
            };

            return clone;
        }
    }

    public class TimestampType
    {
        public string                        ID;
        public string                        Name;
        public Func<Storyboardable, float>   StoryboardGetter;
        public Action<Storyboardable, float> StoryboardSetter;
    }

    [Serializable]
    public class Storyboard
    {
        public List<Timestamp> Timestamps = new();

        public void Add(Timestamp timestamp)
        {
            Timestamps.Add(timestamp);
            Timestamps.Sort((x, y) => x.Offset.CompareTo(y.Offset));
        }

        public List<Timestamp> FromType(string type)
        {
            return Timestamps.FindAll(x => x.ID == type);
        }

        public Storyboard DeepClone()
        {
            var clone = new Storyboard();
            foreach (Timestamp timestamp in Timestamps) clone.Timestamps.Add(timestamp.DeepClone());

            return clone;
        }
    }

    public abstract class Storyboardable
    {
        public Storyboard Storyboard = new();

        public abstract TimestampType[] TimestampTypes { get; }

        public Storyboardable GetStoryboardableObject(float time) {
        
            Storyboardable obj = (Storyboardable)MemberwiseClone();
        
            foreach(TimestampType timestampType in TimestampTypes) 
                try {
                    List<Timestamp> storyboard = Storyboard.FromType(timestampType.ID);

                    float value = timestampType.StoryboardGetter(this);

                    foreach (Timestamp timestamp in storyboard)
                        if (time >= timestamp.Offset + timestamp.Duration)
                        {
                            value = timestamp.Target;
                        }
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
                        {
                            break;
                        }

                    timestampType.StoryboardSetter(obj, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(GetType() + " " + timestampType.ID + "\n" + e);
                }

            return obj;
        }

        protected Dictionary<string, float> CurrentValues;

        protected float CurrentTime;

        public virtual void Advance(float time)
        {
        
            if (CurrentValues == null) 
            {
                CurrentValues = new Dictionary<string, float>();

                foreach (TimestampType timestampType in TimestampTypes)
                    CurrentValues.Add(timestampType.ID, timestampType.StoryboardGetter(this));
            }
        
            foreach (TimestampType timestampType in TimestampTypes)
            {
                if (!CurrentValues.ContainsKey(timestampType.ID))
                {
                    continue;
                    // Debug.LogError(this.GetType() + " " + timestampType.ID + "\n" + e);
                }
                float value = CurrentValues[timestampType.ID];
            
                while (true) 
                {
                    Timestamp timestamp = null;
                    foreach (var thisTimeStamp in Storyboard.Timestamps){
                        if (timestampType.ID == thisTimeStamp.ID)
                        {
                            timestamp = thisTimeStamp;
                            break;
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
                timestampType.StoryboardSetter(this, value);
            }
        
            CurrentTime = time;
        }
    }

    public abstract class DirtyTrackedStoryboardable : Storyboardable
    {
        public bool IsDirty;

        public override void Advance(float time)
        {
            if (CurrentValues == null) 
            {
                CurrentValues = new Dictionary<string, float>();
                foreach (TimestampType timestampType in TimestampTypes) CurrentValues.Add(timestampType.ID, timestampType.StoryboardGetter(this));
            }
            foreach(TimestampType timestampType in TimestampTypes){
                if (!CurrentValues.ContainsKey(timestampType.ID))
                {
                    continue;
                    // Debug.LogError(this.GetType() + " " + tst.ID + "\n" + e);
                }
                float value = CurrentValues[timestampType.ID];
                while (true) 
                {
                    Timestamp timestamp = null;
                    foreach (var thisTimeStamp in Storyboard.Timestamps){
                        if (thisTimeStamp.ID == timestampType.ID){
                            timestamp = thisTimeStamp;
                            break;
                        }
                    }
                    if (timestamp == null || (time < timestamp.Offset && CurrentTime < timestamp.Offset)) break;
                    else if (time < timestamp.Offset + timestamp.Duration)
                    {
                        if (!float.IsNaN(timestamp.From)) CurrentValues[timestampType.ID] = value = timestamp.From;
                        value = Mathf.LerpUnclamped(value, timestamp.Target, timestamp.Easing.Get((time - timestamp.Offset) / timestamp.Duration));
                        IsDirty = true;
                        break;
                    }
                    else
                    {
                        CurrentValues[timestampType.ID] = value = timestamp.Target;
                        Storyboard.Timestamps.Remove(timestamp);
                        IsDirty = true;
                    }
                }
                timestampType.StoryboardSetter(this, value);
            }
            CurrentTime = time;
        }
    }
}