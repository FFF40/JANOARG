using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[System.Serializable]
public class Metronome
{
    public List<BPMStop> Stops = new List<BPMStop>();

    public Metronome(float startBPM) {
        Stops.Add(new BPMStop(startBPM, 0));
    }
    public Metronome(float startBPM, float startOffset) {
        Stops.Add(new BPMStop(startBPM, startOffset));
    }

    public float ToBeat(float seconds) {
        float beat = 0;
        for (int a = 0; a < Stops.Count; a++) 
        {
            float b = (seconds - Stops[a].Offset) / (60 / Stops[a].BPM);
            if (a + 1 < Stops.Count) 
            {
                float c = (Stops[a+1].Offset - Stops[a].Offset) / (60 / Stops[a].BPM);
                if (b <= c) return beat + b;
                beat += c;
            }
            else 
            {
                return beat + b;
            }
        }
        return beat;
    }

    public float ToSeconds(float beat) {
        for (int a = 0; a < Stops.Count; a++) 
        {
            BPMStop stop = Stops[a];
            float b = beat * (60 / Stops[a].BPM) + Stops[a].Offset;
            if (a + 1 < Stops.Count) 
            {
                float c = (Stops[a+1].Offset - Stops[a].Offset) / (60 / Stops[a].BPM);
                if (beat <= c) return b;
                beat -= c;
            }
            else 
            {
                return b;
            }
        }
        return 0;
    }

    public float ToBar(float seconds, float beat = 0) {
        float bar = 0;
        for (int a = 0; a < Stops.Count; a++) 
        {
            float b = ((seconds - Stops[a].Offset) / (60 / Stops[a].BPM) + beat) / Stops[a].Signature;
            if (a + 1 < Stops.Count) 
            {
                float c = ((Stops[a+1].Offset - Stops[a].Offset) / (60 / Stops[a].BPM) + beat) / Stops[a].Signature;
                if (b <= c) return bar + b;
                bar += Mathf.Ceil(c);
            }
            else 
            {
                return bar + b;
            }
        }
        return bar;
    }

    public float ToDividedBeat(float seconds, float beat = 0) {
        for (int a = 0; a < Stops.Count; a++) 
        {
            float b = (seconds - Stops[a].Offset) / (60 / Stops[a].BPM) + beat;
            float bb = b % Stops[a].Signature;
            if (a + 1 < Stops.Count) 
            {
                float c = (Stops[a+1].Offset - Stops[a].Offset) / (60 / Stops[a].BPM) + beat;
                if (b <= c) return bb;
            }
            else 
            {
                return bb;
            }
        }
        return 0;
    }

    public BPMStop GetStop(float seconds, out int tag) {
        tag = 0;
        while (tag < Stops.Count - 1 && Stops[tag + 1].Offset < seconds) tag++;
        return Stops[tag];
    }
}

[System.Serializable]
public class BPMStop
{
    public float Offset;
    public float BPM;
    public int Signature = 4;

    public BPMStop(float bpm, float offset) {
        Offset = offset;
        BPM = bpm;
    }
}
