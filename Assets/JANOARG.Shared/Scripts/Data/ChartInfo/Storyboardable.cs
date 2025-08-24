using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Shared.Data.ChartInfo
{
    /// <summary>
    /// Represents a single timestamped value change for a storyboardable property.
    /// </summary>
    [Serializable]
    public class Timestamp : IDeepClonable<Timestamp>
    {
        /// <summary>
        /// The offset (in beats) at which this timestamp occurs.
        /// </summary>
        [FormerlySerializedAs("Time")]
        public BeatPosition Offset;

        /// <summary>
        /// The duration (in beats) over which the value changes.
        /// </summary>
        public float  Duration;
        /// <summary>
        /// The ID/type of the property this timestamp affects.
        /// </summary>
        public string ID;
        /// <summary>
        /// The starting value (if not NaN) for the transition.
        /// </summary>
        public float  From = float.NaN;
        /// <summary>
        /// The target value at the end of the transition.
        /// </summary>
        public float  Target;

        /// <summary>
        /// The easing directive for the transition.
        /// </summary>
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

    /// <summary>
    /// Describes a type of timestamped property for storyboardable objects.
    /// </summary>
    public class TimestampType
    {
        /// <summary>
        /// The unique ID for this property type.
        /// </summary>
        public string                        ID;
        /// <summary>
        /// The display name for this property type.
        /// </summary>
        public string                        Name;
        /// <summary>
        /// Function to get the current value from a storyboardable object.
        /// </summary>
        public Func<Storyboardable, float>   StoryboardGetter;
        /// <summary>
        /// Function to set the value on a storyboardable object.
        /// </summary>
        public Action<Storyboardable, float> StoryboardSetter;
    }

    /// <summary>
    /// Holds a list of timestamped value changes for a storyboardable object.
    /// </summary>
    [Serializable]
    public class Storyboard
    {
        /// <summary>
        /// The list of all timestamps in this storyboard.
        /// </summary>
        public List<Timestamp> Timestamps = new();

        /// <summary>
        /// Adds a timestamp and keeps the list sorted by offset.
        /// </summary>
        public void Add(Timestamp timestamp)
        {
            Timestamps.Add(timestamp);
            Timestamps.Sort((x, y) => x.Offset.CompareTo(y.Offset));
        }

        /// <summary>
        /// Returns all timestamps of a given type.
        /// </summary>
        public List<Timestamp> FromType(string type)
        {
            return Timestamps.FindAll(x => x.ID == type);
        }

        /// <summary>
        /// Deep clones this storyboard and all its timestamps.
        /// </summary>
        public Storyboard DeepClone()
        {
            var clone = new Storyboard();
            foreach (Timestamp timestamp in Timestamps) clone.Timestamps.Add(timestamp.DeepClone());

            return clone;
        }
    }

    /// <summary>
    /// Base class for objects that can be animated via a storyboard (timestamped value changes).
    /// </summary>
    public abstract class Storyboardable
    {
        /// <summary>
        /// The storyboard containing all timestamped value changes for this object.
        /// </summary>
        public Storyboard Storyboard = new();

        /// <summary>
        /// Static array of supported timestamp types for this class.
        /// </summary>
        public static TimestampType[] sTimestampTypes = Array.Empty<TimestampType>();

        /// <summary>
        /// Cached array of timestamp types for this instance.
        /// </summary>
        protected TimestampType[] TimestampTypesP;

        /// <summary>
        /// Returns a clone of this object with all properties advanced to the given time.
        /// </summary>
        public Storyboardable GetStoryboardableObject(float time)
        {
            if (TimestampTypesP == null)
                TimestampTypesP = (TimestampType[])GetType()
                    .GetField("TimestampTypes")
                    .GetValue(null);

            var obj = (Storyboardable)MemberwiseClone();

            foreach (TimestampType timestampType in TimestampTypesP)
                try
                {
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

        /// <summary>
        /// Stores the current values for each timestamp type.
        /// </summary>
        protected Dictionary<string, float> CurrentValues;

        /// <summary>
        /// The current time for this storyboardable object.
        /// </summary>
        protected float CurrentTime;

        /// <summary>
        /// Advances all properties to the given time, applying any relevant timestamps.
        /// </summary>
        /// <remarks>
        /// This operation is destructive.
        /// </remarks>
        public virtual void Advance(float time)
        {
            if (TimestampTypesP == null)
                TimestampTypesP = (TimestampType[])GetType()
                    .GetField("TimestampTypes")
                    .GetValue(null);

            if (CurrentValues == null)
            {
                CurrentValues = new Dictionary<string, float>();

                foreach (TimestampType timestampType in TimestampTypesP)
                    CurrentValues.Add(timestampType.ID, timestampType.StoryboardGetter(this));
            }

            foreach (TimestampType timestampType in TimestampTypesP)
                try
                {
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

                    timestampType.StoryboardSetter(this, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(GetType() + " " + timestampType.ID + "\n" + e);
                }

            CurrentTime = time;
        }
    }

    public abstract class DirtyTrackedStoryboardable : Storyboardable
    {
        public bool IsDirty;

        public override void Advance(float time)
        {
            if (TimestampTypesP == null)
                TimestampTypesP = (TimestampType[])GetType()
                    .GetField("TimestampTypes")
                    .GetValue(null);

            if (CurrentValues == null)
            {
                CurrentValues = new Dictionary<string, float>();
                foreach (TimestampType timestampType in TimestampTypesP) CurrentValues.Add(timestampType.ID, timestampType.StoryboardGetter(this));
            }

            foreach (TimestampType timestampType in TimestampTypesP)
                try
                {
                    float value = CurrentValues[timestampType.ID];

                    while (true)
                    {
                        Timestamp timestamp = Storyboard.Timestamps.Find(x => x.ID == timestampType.ID);

                        if (timestamp == null || (time < timestamp.Offset && CurrentTime < timestamp.Offset))
                        {
                            break;
                        }
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
                catch (Exception e)
                {
                    Debug.LogError(GetType() + " " + timestampType.ID + "\n" + e);
                }

            CurrentTime = time;
        }
    }
}