using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableSong : ScriptableObject
{
    public string SongName;
    public string SongArtist;
    public AudioClip Clip;

    public Metronome Timing = new Metronome(140);

    public List<Chart> Charts = new List<Chart>();
}
