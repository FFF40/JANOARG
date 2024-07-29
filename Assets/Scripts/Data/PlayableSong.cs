using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PlayableSong
{
    public string SongName;
    public string AltSongName;
    public string SongArtist;
    public string AltSongArtist;
    public string Location = "Prototype";
    public string Genre = "Genreless";
    
    public string ClipPath;
    public AudioClip Clip;

    public Color BackgroundColor = Color.black;
    public Color InterfaceColor = Color.white;

    public Metronome Timing = new Metronome(140);
    public Cover Cover = new();

    [FormerlySerializedAs("Charts")]
    public List<Chart> ChartsOld = new List<Chart>();

    public List<ExternalChartMeta> Charts = new List<ExternalChartMeta>();
}

[Serializable]
public class Cover
{
    public string IconTarget = "icon.png";
    public Sprite IconCover;
    public Vector2 IconCenter;
    public float IconSize = 24;

    public Color BackgroundColor;

    public List<CoverLayer> Layers = new();
}

[Serializable]
public class CoverLayer
{
    public string Target;
    public Texture2D Texture;

    public float Scale = 1;
    public Vector2 Position;
    public float ParallaxFactor = 0;
    public bool Tiling;
}