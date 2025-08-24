using System;
using System.Collections.Generic;

namespace JANOARG.Shared.Data.ChartInfo
{
    /// <summary>
    /// Class for handling conversions of song time in seconds to beats and vice versa.
    /// </summary>
    [Serializable]
    public class Metronome
    {

        public List<BPMStop> Stops = new();

        public Metronome(float startBPM)
        {
            Stops.Add(new BPMStop(startBPM, 0));
        }

        public Metronome(float startBpm, float startOffset)
        {
            Stops.Add(new BPMStop(startBpm, startOffset));
        }

        /// <summary>
        /// Converts song time in seconds to beat.
        /// </summary>
        /// <param name="seconds">Song time in seconds.</param>
        /// <returns>Song time in beats.</returns>
        public float ToBeat(float seconds)
        {
            if (Stops.Count == 0) return float.NaN;

            float totalBeats = 0;

            for (int stopIndex = 0; stopIndex < Stops.Count; stopIndex++)
            {
                float currentBeatPosition = (seconds - Stops[stopIndex].Offset) / (60 / Stops[stopIndex].BPM);

                if (stopIndex + 1 < Stops.Count)
                {
                    float sectionBeats = (Stops[stopIndex + 1].Offset - Stops[stopIndex].Offset) /
                                         (60 / Stops[stopIndex].BPM);

                    if (currentBeatPosition <= sectionBeats) return totalBeats + currentBeatPosition;

                    totalBeats += sectionBeats;
                }
                else
                {
                    return totalBeats + currentBeatPosition;
                }
            }

            return totalBeats;
        }

        /// <summary>
        /// Converts song time in beats to seconds
        /// </summary>
        /// <param name="beat">Song time in beats.</param>
        /// <returns>Song time in seconds.</returns>
        public float ToSeconds(float beat)
        {
            if (Stops.Count == 0) return float.NaN;

            for (int stopIndex = 0; stopIndex < Stops.Count; stopIndex++)
            {
                BPMStop stop = Stops[stopIndex];
                float timeInSeconds = beat * (60 / Stops[stopIndex].BPM) + Stops[stopIndex].Offset;

                if (stopIndex + 1 < Stops.Count)
                {
                    float sectionBeats = (Stops[stopIndex + 1].Offset - Stops[stopIndex].Offset) /
                                         (60 / Stops[stopIndex].BPM);

                    if (beat <= sectionBeats) return timeInSeconds;

                    beat -= sectionBeats;
                }
                else
                {
                    return timeInSeconds;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get the bar position given song time in seconds and offset in beats.
        /// </summary>
        /// <param name="seconds">Song time in seconds.</param>
        /// <param name="beat">Song time offset in beats.</param>
        /// <returns>The calculated bar time.</returns>
        public float ToBar(float seconds, float beat = 0)
        {
            if (Stops.Count == 0) return float.NaN;

            float totalBars = 0;

            for (int stopIndex = 0; stopIndex < Stops.Count; stopIndex++)
            {
                float currentBarPosition =
                    ((seconds - Stops[stopIndex].Offset) / (60 / Stops[stopIndex].BPM) + beat) /
                    Stops[stopIndex].Signature;

                if (stopIndex + 1 < Stops.Count)
                {
                    float sectionBars =
                        ((Stops[stopIndex + 1].Offset - Stops[stopIndex].Offset) / (60 / Stops[stopIndex].BPM) +
                         beat) /
                        Stops[stopIndex].Signature;

                    if (currentBarPosition <= sectionBars) return totalBars + currentBarPosition;

                    totalBars += sectionBars;
                }
                else
                {
                    return totalBars + currentBarPosition;
                }
            }

            return totalBars;
        }

        /// <summary>
        /// Get the beat position in a bar given song time in seconds and offset in beats.
        /// </summary>
        /// <param name="seconds">Song time in seconds.</param>
        /// <param name="beat">Song time offset in beats.</param>
        /// <returns>The calculated beat time in the bar.</returns>
        public float ToDividedBeat(float seconds, float beat = 0)
        {
            if (Stops.Count == 0) return float.NaN;

            for (int stopIndex = 0; stopIndex < Stops.Count; stopIndex++)
            {
                float currentBeatPosition =
                    (seconds - Stops[stopIndex].Offset) / (60 / Stops[stopIndex].BPM) + beat;

                float beatInBar = (currentBeatPosition % Stops[stopIndex].Signature + Stops[stopIndex].Signature) %
                                  Stops[stopIndex].Signature;

                if (stopIndex + 1 < Stops.Count)
                {
                    float sectionBeats = (Stops[stopIndex + 1].Offset - Stops[stopIndex].Offset) /
                                         (60 / Stops[stopIndex].BPM) +
                                         beat;

                    if (currentBeatPosition <= sectionBeats) return beatInBar;
                }
                else
                {
                    return beatInBar;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get the current BPM Stop currently in effect at a given song time.
        /// </summary>
        /// <param name="seconds">Song time in seconds.</param>
        /// <param name="index">The index of the BPM Stop currently in effect.</param>
        /// <returns>The BPM Stop currently in effect.</returns>
        public BPMStop GetStop(float seconds, out int index)
        {
            index = 0;

            if (Stops.Count == 0) return null;

            while (index < Stops.Count - 1 && Stops[index + 1].Offset < seconds) index++;

            return Stops[index];
        }
        
        public static Metronome sIdentity = new(60);
    }

    /// <summary>
    /// Represent a point that the song's BPM and time signature changes.
    /// </summary>
    [Serializable]
    public class BPMStop : IDeepClonable<BPMStop>
    {
        /// <summary>
        /// The song time in seconds that this BPM stop begins to take effect.
        /// </summary>
        public float Offset;
        /// <summary>
        /// The song's speed in beats per minute at the point of this BPM stop.
        /// </summary>
        public float BPM;
        /// <summary>
        /// The song's time signature at the point of this BPM stop.
        /// </summary>
        public int   Signature = 4;

        /// <summary>
        /// Whether this BPM stop should be included in the BPM summary of the song.
        /// </summary>
        public bool Significant = true;

        public BPMStop(float bpm, float offset)
        {
            Offset = offset;
            BPM = bpm;
        }

        public BPMStop DeepClone()
        {
            BPMStop clone = new(BPM, Offset)
            {
                Signature = Signature
            };

            return clone;
        }
    }
}