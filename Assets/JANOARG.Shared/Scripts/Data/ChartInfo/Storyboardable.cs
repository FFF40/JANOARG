using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// TODO: REWRITE THIS ASAP

namespace JANOARG.Shared.Data.ChartInfo
{
    [Serializable]
    public class Timestamp : IDeepClonable<Timestamp>
    {
        [FormerlySerializedAs("Time")]
        public BeatPosition Offset;

        public float  Duration;
        public TimestampIDs ID;
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
        public TimestampIDs                        ID;
        public string                        Name;
        public Func<Storyboardable, float>   StoryboardGetter;
        public Action<Storyboardable, float> StoryboardSetter;
    }

    [Serializable]
    public class Storyboard
    {
        public  List<Timestamp>                 Timestamps = new();
        private Dictionary<string, Timestamp[]> _TypeCache = new();


        public void Add(Timestamp timestamp)
        {
            Timestamps.Add(timestamp);
            Timestamps.Sort((x, y) => x.Offset.CompareTo(y.Offset)); // Probably not a big deal; only called on file import
            
            _TypeCache.Clear(); // Invalidate cache when timestamps change
        }

        public Timestamp[] FromType(TimestampIDs type)
        {
            if (!_TypeCache.TryGetValue(type.ToString(), out Timestamp[] array))
            {
                array = Timestamps.Where(x => x.ID == type).ToArray();
                _TypeCache[type.ToString()] = array;
            }
            return array;

        }
    
        public Storyboard SelfReference()
        {
            return this;
        }
    }

    public abstract class Storyboardable
    {
        public Storyboard Storyboard = new();

        public abstract TimestampType[] timestampTypes { get; }

        public Storyboardable GetStoryboardableObject(float time) 
        {
            Storyboardable obj = (Storyboardable)MemberwiseClone();
        
            foreach(TimestampType timestampType in timestampTypes) 
                try 
                {
                    Timestamp[] storyboard = Storyboard.FromType(timestampType.ID);

                    float value = timestampType.StoryboardGetter(this);

                    foreach (Timestamp timestamp in storyboard)
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

                    timestampType.StoryboardSetter(obj, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(GetType() + " " + timestampType.ID + "\n" + e);
                }

            return obj;
        }

        protected float[] CurrentValues;

        protected float CurrentTime;

        public virtual void Advance(float time)
        {
            // Initialize current value of each timestamp type if they don't exist
            if (CurrentValues == null) 
            {
                // Initialize array with size equal to number of enum values
                CurrentValues = new float[Enum.GetValues(typeof(TimestampIDs)).Length];
            
                foreach (TimestampType timestampType in timestampTypes)
                    CurrentValues[(int)timestampType.ID] = timestampType.StoryboardGetter(this);
            }

            // Loop through each timestamp type
            foreach (TimestampType timestampType in timestampTypes)
            {
                // Skip if there isn't any timestamp of the given type, otherwise assign to value
                float value = CurrentValues[(int)timestampType.ID];

                // Navigate forward
                while (true) 
                {
                    // Get the next timestamp in the list
                    Timestamp timestamp = null;

                    timestamp = Storyboard.Timestamps.Find(storyboardTimestamp => storyboardTimestamp.ID == timestampType.ID);
                    
                    /*foreach (Timestamp storyboardTimestamp in Storyboard.Timestamps)
                    {
                        if (timestampType.ID == storyboardTimestamp.ID)
                        {
                            timestamp = storyboardTimestamp;
                            break;
                        }
                    }*/
                
                    // Skip if there's no timestamp or it's not yet the start of the next timestamp
                    if (timestamp == null || (time < timestamp.Offset && CurrentTime < timestamp.Offset))
                        break;

                    // If the timestamp is in progress
                    if (time < timestamp.Offset + timestamp.Duration)
                    {
                        // NaN means lerp from the previous value
                        if (!float.IsNaN(timestamp.From))
                            CurrentValues[(int)timestampType.ID] = value = timestamp.From;
                    
                        // Get the current value
                        value = Mathf.LerpUnclamped(value, timestamp.Target, timestamp.Easing.Get((time - timestamp.Offset) / timestamp.Duration));
                    
                        break;
                    }
                    else
                    {
                        // Set value to destination and pop the timestamp off the list
                        CurrentValues[(int)timestampType.ID] = value = timestamp.Target;
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
                // Initialize array with size equal to number of enum values
                CurrentValues = new float[Enum.GetValues(typeof(TimestampIDs)).Length];
            
                foreach (TimestampType timestampType in timestampTypes)
                    CurrentValues[(int)timestampType.ID] = timestampType.StoryboardGetter(this);
            }
            
            foreach(TimestampType timestampType in timestampTypes)
            {
                
                float value = CurrentValues[(int)timestampType.ID];
                
                while (true) 
                {
                    Timestamp timestamp = Storyboard.Timestamps.Find(storyboardTimestamp => storyboardTimestamp.ID == timestampType.ID);
                    
                    /*
                    // Look for matching timestamp (LINEAR SEARCH AAAAA)
                    foreach (Timestamp storyboardTimestamp in Storyboard.Timestamps)
                    {
                        if (storyboardTimestamp.ID == timestampType.ID)
                        {
                            timestamp = storyboardTimestamp;
                            break;
                        */
                    
                    // If there's no timestamp or it's not yet the start of the next timestamp
                    if (timestamp == null || (time < timestamp.Offset && CurrentTime < timestamp.Offset)) 
                        break;
                    
                    // Otherwise
                    if (time < timestamp.Offset + timestamp.Duration)
                    {
                        // Lerp from previous value if it's not NaN
                        if (!float.IsNaN(timestamp.From)) 
                            CurrentValues[(int)timestampType.ID] = value = timestamp.From;
                        
                        // Lerp to target value
                        value = Mathf.LerpUnclamped(value, timestamp.Target, timestamp.Easing.Get((time - timestamp.Offset) / timestamp.Duration));
                        IsDirty = true;
                        break;
                    }
                    else
                    {
                        // Set value to target and pop the timestamp off the list
                        CurrentValues[(int)timestampType.ID] = value = timestamp.Target;
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