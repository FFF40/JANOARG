using System;
using System.Collections.Generic;

namespace JANOARG.Shared.Data.ChartInfo
{
    [Serializable]
    public class Metronome
    {
        public static Metronome     sIdentity = new(60);
        public        List<BPMStop> Stops     = new();

        public Metronome(float startBPM)
        {
            Stops.Add(new BPMStop(startBPM, 0));
        }

        public Metronome(float startBpm, float startOffset)
        {
            Stops.Add(new BPMStop(startBpm, startOffset));
        }

        public float ToBeat(float seconds)
        {
            if (Stops.Count == 0) return float.NaN;

            float totalBeats = 0;

            for (var stopIndex = 0; stopIndex < Stops.Count; stopIndex++)
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

        public float ToSeconds(float beat)
        {
            if (Stops.Count == 0) return float.NaN;

            for (var stopIndex = 0; stopIndex < Stops.Count; stopIndex++)
            {
                BPMStop stop = Stops[stopIndex];
                float timeInSeconds = (beat * (60 / Stops[stopIndex].BPM)) + Stops[stopIndex].Offset;

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

        public float ToBar(float seconds, float beat = 0)
        {
            if (Stops.Count == 0) return float.NaN;

            float totalBars = 0;

            for (var stopIndex = 0; stopIndex < Stops.Count; stopIndex++)
            {
                float currentBarPosition =
                    (((seconds - Stops[stopIndex].Offset) / (60 / Stops[stopIndex].BPM)) + beat) /
                    Stops[stopIndex].Signature;

                if (stopIndex + 1 < Stops.Count)
                {
                    float sectionBars =
                        (((Stops[stopIndex + 1].Offset - Stops[stopIndex].Offset) / (60 / Stops[stopIndex].BPM)) +
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

        public float ToDividedBeat(float seconds, float beat = 0)
        {
            if (Stops.Count == 0) return float.NaN;

            for (var stopIndex = 0; stopIndex < Stops.Count; stopIndex++)
            {
                float currentBeatPosition =
                    ((seconds - Stops[stopIndex].Offset) / (60 / Stops[stopIndex].BPM)) + beat;

                float beatInBar = ((currentBeatPosition % Stops[stopIndex].Signature) + Stops[stopIndex].Signature) %
                                  Stops[stopIndex].Signature;

                if (stopIndex + 1 < Stops.Count)
                {
                    float sectionBeats = ((Stops[stopIndex + 1].Offset - Stops[stopIndex].Offset) /
                                         (60 / Stops[stopIndex].BPM)) +
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

        public BPMStop GetStop(float seconds, out int tag)
        {
            tag = 0;

            if (Stops.Count == 0) return null;

            while (tag < Stops.Count - 1 && Stops[tag + 1].Offset < seconds) tag++;

            return Stops[tag];
        }

        internal float ToSeconds(object offset)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class BPMStop : IDeepClonable<BPMStop>
    {
        public float Offset;
        public float BPM;
        public int   Signature = 4;

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