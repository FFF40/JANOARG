using System;
using System.Globalization;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Shared.Data.Files
{
    /// <summary>
    /// Utility class to decode a .JAPS file into a Playable Song.
    /// </summary>
    public class JAPSDecoder
    {
        public const int FORMAT_VERSION = 2;
        public const int INDENT_SIZE = 2;

        /// <summary>
        /// Parse a .JAPS file's content.
        /// </summary>
        /// <param name="str">Content of the .JAPS file.</param>
        /// <returns>A Playable Song represent by the .JAPS file.</returns>
        /// <exception cref="Exception">Exception that's thrown when parsing encounters an error.</exception>
        public static PlayableSong Decode(string str)
        {
            PlayableSong decodingSong = new();

            decodingSong.Timing.Stops.Clear();

            var mode = "";

            object currentObject = null;

            string[] lines = str.Split("\n");
            var index = 0;

            try
            {
                foreach (string l in lines)
                {
                    string line = l.TrimStart();
                    index++;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        mode = line[1..^1];

                        if (mode == "VERSION")
                            currentObject = "version";
                        else if (mode == "METADATA")
                            currentObject = decodingSong;
                        else if (mode == "RESOURCES")
                            currentObject = decodingSong;
                        else if (mode == "COVER")
                            currentObject = decodingSong.Cover;
                        else if (mode == "COLORS")
                            currentObject = decodingSong;
                        else if (mode == "TIMING")
                            currentObject = decodingSong.Timing;
                        else if (mode == "CHARTS")
                            currentObject = decodingSong.Charts;
                        else
                            throw new Exception("The specified mode " + mode + " is not a valid mode.");
                    }
                    else if (line.StartsWith("+"))
                    {
                        string[] tokens = line.Split(' ');

                        if (tokens.Length < 2) throw new Exception("Object token expected but not found.");

                        if (tokens[1] == "Layer")
                        {
                            if (tokens.Length >= 6)
                            {
                                CoverLayer layer = new()
                                {
                                    Scale = ParseFloat(tokens[2]),
                                    Position = new Vector2(ParseFloat(tokens[3]), ParseFloat(tokens[4])),
                                    ParallaxFactor = ParseFloat(tokens[5])
                                };

                                decodingSong.Cover.Layers.Add(layer);
                                currentObject = layer;
                            }
                            else
                            {
                                throw new Exception("Not enough tokens (minimum 6, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "BPM")
                        {
                            if (tokens.Length >= 6)
                            {
                                BPMStop stop = new(ParseFloat(tokens[3]), ParseFloat(tokens[2]))
                                {
                                    Signature = ParseInt(tokens[4]),
                                    Significant = tokens[5] == "S"
                                };

                                decodingSong.Timing.Stops.Add(stop);
                                currentObject = stop;
                            }
                            else
                            {
                                throw new Exception("Not enough tokens (minimum 6, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "Chart")
                        {
                            ExternalChartMeta chart = new();

                            decodingSong.Charts.Add(chart);
                            currentObject = chart;
                        }
                        else
                        {
                            throw new Exception("The specified object " + tokens[1] + " is not a valid object.");
                        }
                    }
                    else if (line.Contains(": "))
                    {
                        int pos = line.IndexOf(": ");
                        string key = line[..pos];
                        string value = line[(pos + 2)..];

                        if (currentObject is PlayableSong song)
                        {
                            if (key == "Name") song.SongName = value;
                            if (key == "Alt Name") song.AltSongName = value;
                            if (key == "Artist") song.SongArtist = value;
                            if (key == "Alt Artist") song.AltSongArtist = value;
                            if (key == "Genre") song.Genre = value;
                            if (key == "Location") song.Location = value;
                            if (key == "Preview Range") song.PreviewRange = ParseVector(value);

                            if (key == "Clip") song.ClipPath = value;

                            if (key == "Background") song.BackgroundColor = ParseColor(value);
                            if (key == "Interface") song.InterfaceColor = ParseColor(value);
                        }
                        else if (currentObject is Cover cover)
                        {
                            if (key == "Artist") cover.ArtistName = value;
                            else if (key == "Alt Artist") cover.AltArtistName = value;
                            else if (key == "Background") cover.BackgroundColor = ParseColor(value);
                            else if (key == "Icon") cover.IconTarget = value;
                            else if (key == "Icon Center") cover.IconCenter = ParseVector(value);
                            else if (key == "Icon Size") cover.IconSize = ParseFloat(value);
                        }
                        else if (currentObject is CoverLayer layer)
                        {
                            if (key == "Target") layer.Target = value;
                        }
                        else if (currentObject is ExternalChartMeta chart)
                        {
                            if (key == "Target") chart.Target = value;
                            else if (key == "Index") chart.DifficultyIndex = ParseInt(value);
                            else if (key == "Name") chart.DifficultyName = value;
                            else if (key == "Charter") chart.CharterName = value;
                            else if (key == "Level") chart.DifficultyLevel = value;
                            else if (key == "Constant") chart.ChartConstant = ParseFloat(value);
                        }
                    }
                    else if (currentObject is CoverLayer layer)
                    {
                        if (line == "Tiling") layer.Tiling = true;
                    }
                    else if (currentObject?.ToString() == "version")
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (!int.TryParse(line, out int version)) continue;

                        if (version > FORMAT_VERSION)
                            throw new Exception(
                                "Chart version is newer than the supported format version. Please open this chart using a newer version of the Chartmaker.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    "An error occurred while trying to decode line " +
                    index +
                    ":\nContent: " +
                    lines[index - 1] +
                    "\nException: " +
                    e);
            }

            return decodingSong;
        }

        private static T ParseEnum<T>(string str) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        private static void ParseEasing(string str, out EaseFunction easing, out EaseMode easeMode)
        {
            string[] tokens = str.Split('/');

            if (tokens.Length == 2)
            {
                easing = (EaseFunction)Enum.Parse(typeof(EaseFunction), tokens[0]);
                easeMode = (EaseMode)Enum.Parse(typeof(EaseMode), tokens[1]);
            }
            else
            {
                throw new ArgumentException("The specified string is not in a valid Easing format");
            }
        }

        private static int ParseInt(string number)
        {
            return int.Parse(number, CultureInfo.InvariantCulture);
        }

        private static float ParseFloat(string number)
        {
            return float.Parse(number, CultureInfo.InvariantCulture);
        }

        private static float ParseTime(string number)
        {
            return float.Parse(number, CultureInfo.InvariantCulture);
        }

        private static Vector3 ParseVector(string str)
        {
            string[] tokens = str.Split(' ');

            return new Vector3(
                ParseFloat(tokens[0]), ParseFloat(tokens[1]),
                tokens.Length < 3 ? 0 : ParseFloat(tokens[2]));
        }

        private static Color ParseColor(string str)
        {
            string[] tokens = str.Split(' ');

            return new Color(
                ParseFloat(tokens[0]), ParseFloat(tokens[1]), ParseFloat(tokens[2]),
                ParseFloat(tokens[3]));
        }
    }
}