using System.Globalization;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Shared.Data.Files
{
    /// <summary>
    /// Utility class to encode a Playable Song into a .JAPS file.
    /// </summary>
    public class JAPSEncoder
    {
        public const int FORMAT_VERSION = 2;
        public const int INDENT_SIZE    = 2;

        /// <summary>
        /// Encode a Playable Song into a .JAPS file.
        /// </summary>
        /// <param name="song">The Playable Song object.</param>
        /// <param name="clipName">Name of the song audio file.</param>
        /// <returns>A string under the .JAPS file format.</returns>
        public static string Encode(PlayableSong song, string clipName)
        {
            var str = "JANOARG Playable Song Format\ngithub.com/FFF40/JANOARG";

            str += "\n\n[VERSION]\n" + FORMAT_VERSION;

            str += "\n\n[METADATA]";

            str += "\nName: " + song.SongName;

            if (!string.IsNullOrWhiteSpace(song.AltSongName))
                str += "\nAlt Name: " + song.AltSongName;

            str += "\nArtist: " + song.SongArtist;

            if (!string.IsNullOrWhiteSpace(song.AltSongArtist))
                str += "\nAlt Artist: " + song.AltSongArtist;

            str += "\nGenre: " + song.Genre;
            str += "\nLocation: " + song.Location;

            str += "\nPreview Range: " + EncodeVector(song.PreviewRange);

            str += "\n\n[RESOURCES]";
            str += "\nClip: " + clipName;

            str += "\n\n[COVER]";
            str += "\nArtist: " + song.Cover.ArtistName;

            if (!string.IsNullOrWhiteSpace(song.Cover.AltArtistName))
                str += "\nAlt Artist: " + song.Cover.AltArtistName;

            str += "\nBackground: " + EncodeColor(song.Cover.BackgroundColor);
            str += "\nIcon: " + song.Cover.IconTarget;
            str += "\nIcon Center: " + EncodeVector(song.Cover.IconCenter);
            str += "\nIcon Size: " + song.Cover.IconSize.ToString(CultureInfo.InvariantCulture);
            foreach (CoverLayer layer in song.Cover.Layers) str += EncodeCoverLayer(layer);

            str += "\n\n[COLORS]";
            str += "\nBackground: " + EncodeColor(song.BackgroundColor);
            str += "\nInterface: " + EncodeColor(song.InterfaceColor);

            str += "\n\n[TIMING]";
            foreach (BPMStop stop in song.Timing.Stops) str += EncodeBPMStop(stop);

            str += "\n\n[CHARTS]";
            foreach (ExternalChartMeta chart in song.Charts) str += EncodeExternalChartMeta(chart);


            return str;
        }

        public static string EncodeCoverLayer(CoverLayer layer, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ Layer" +
                         " " +
                         layer.Scale.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         EncodeVector(layer.Position) +
                         " " +
                         layer.ParallaxFactor.ToString(CultureInfo.InvariantCulture) +
                         "\n" +
                         indent2 +
                         "Target: " +
                         layer.Target +
                         (layer.Tiling ? "\n" + indent2 + "Tiling" : "");

            return str;
        }

        public static string EncodeBPMStop(BPMStop stop, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ BPM" +
                         " " +
                         stop.Offset.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         stop.BPM.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         stop.Signature.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         (stop.Significant ? "S" : "_");

            return str;
        }

        public static string EncodeExternalChartMeta(ExternalChartMeta chart, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ Chart" +
                         "\n" +
                         indent2 +
                         "Target: " +
                         chart.Target +
                         "\n" +
                         indent2 +
                         "Index: " +
                         chart.DifficultyIndex.ToString(CultureInfo.InvariantCulture) +
                         "\n" +
                         indent2 +
                         "Name: " +
                         chart.DifficultyName +
                         "\n" +
                         indent2 +
                         "Charter: " +
                         chart.CharterName +
                         "\n" +
                         indent2 +
                         "Level: " +
                         chart.DifficultyLevel +
                         "\n" +
                         indent2 +
                         "Constant: " +
                         chart.ChartConstant.ToString(CultureInfo.InvariantCulture);

            return str;
        }

        public static string EncodeVector(Vector2 vec)
        {
            return vec.x.ToString(CultureInfo.InvariantCulture) + " " + vec.y.ToString(CultureInfo.InvariantCulture);
        }

        public static string EncodeColor(Color col)
        {
            return col.r.ToString(CultureInfo.InvariantCulture) +
                   " " +
                   col.g.ToString(CultureInfo.InvariantCulture) +
                   " " +
                   col.b.ToString(CultureInfo.InvariantCulture) +
                   " " +
                   col.a.ToString(CultureInfo.InvariantCulture);
        }
    }
}