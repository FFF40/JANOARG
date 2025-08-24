using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Shared.Data.ChartInfo
{
    /// <summary>
    /// Contains all metadata and chart data for a playable song.
    /// </summary>
    [Serializable]
    public class PlayableSong
    {
        /// <summary>
        /// The main display name of the song.
        /// </summary>
        public string SongName;
        /// <summary>
        /// An alternative display name for the song (e.g., in another language).
        /// </summary>
        public string AltSongName;
        /// <summary>
        /// The main artist of the song.
        /// </summary>
        public string SongArtist;
        /// <summary>
        /// An alternative artist name (e.g., in another language).
        /// </summary>
        public string AltSongArtist;
        /// <summary>
        /// The location or source of the song (e.g., album, game, etc.).
        /// </summary>
        public string Location = "Prototype";
        /// <summary>
        /// The genre of the song.
        /// </summary>
        public string Genre    = "Genreless";

        /// <summary>
        /// Path to the audio clip file.
        /// </summary>
        public string    ClipPath;
        /// <summary>
        /// The loaded audio clip.
        /// </summary>
        public AudioClip Clip;
        /// <summary>
        /// The preview range (in seconds) for the song.
        /// </summary>
        public Vector2   PreviewRange = new(15, 30);

        /// <summary>
        /// The background color for the song.
        /// </summary>
        public Color BackgroundColor = Color.black;
        /// <summary>
        /// The interface color for the song.
        /// </summary>
        public Color InterfaceColor  = Color.white;

        /// <summary>
        /// The metronome/timing information for the song.
        /// </summary>
        public Metronome Timing = new(140);
        /// <summary>
        /// The cover art and related metadata for the song.
        /// </summary>
        public Cover     Cover  = new();

        /// <summary>
        /// [Obsolete] List of embedded charts (legacy).
        /// </summary>
        [FormerlySerializedAs("Charts")] public List<Chart> ChartsOld = new();

        /// <summary>
        /// List of external chart metadata for this song.
        /// </summary>
        public List<ExternalChartMeta> Charts = new();
    }

    /// <summary>
    /// Contains cover art and related metadata for a song.
    /// </summary>
    [Serializable]
    public class Cover
    {
        /// <summary>
        /// The main artist name for the cover.
        /// </summary>
        public string ArtistName    = "";
        /// <summary>
        /// Alternate artist name for the cover.
        /// </summary>
        public string AltArtistName = "";

        /// <summary>
        /// The file name or path for the cover icon.
        /// </summary>
        public string  IconTarget = "icon.png";
        /// <summary>
        /// The loaded cover icon sprite.
        /// </summary>
        public Sprite  IconCover;
        /// <summary>
        /// The center point of the icon (for alignment).
        /// </summary>
        public Vector2 IconCenter;
        /// <summary>
        /// The size of the icon.
        /// </summary>
        public float   IconSize = 24;

        /// <summary>
        /// The background color for the cover.
        /// </summary>
        public Color BackgroundColor;

        /// <summary>
        /// List of additional cover layers.
        /// </summary>
        public List<CoverLayer> Layers = new();
    }

    /// <summary>
    /// Represents a single layer in a cover image.
    /// </summary>
    [Serializable]
    public class CoverLayer
    {
        /// <summary>
        /// The file name or path for this layer's image.
        /// </summary>
        public string    Target;
        /// <summary>
        /// The loaded texture for this layer.
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// The scale factor for this layer.
        /// </summary>
        public float   Scale = 1;
        /// <summary>
        /// The position offset for this layer.
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The parallax factor for this layer.
        /// </summary>
        public float   ParallaxFactor;
        /// <summary>
        /// Whether this layer should tile.
        /// </summary>
        public bool    Tiling;
    }
}