using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using UnityEditor;
using System.IO;

public class JAPSDecoder
{

    public const int FormatVersion = 2;
    public const int IndentSize = 2;

    public static PlayableSong Decode(string str)
    {
        PlayableSong song = new PlayableSong();

        song.Timing.Stops.Clear();

        string mode = "";

        object currentObject = null;

        string[] lines = str.Split("\n");
        int index = 0;
        try
        {
            foreach(string l in lines) 
            {
                string line = l.TrimStart();
                index++;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    mode = line[1..^1];
                    if (mode == "VERSION")
                    {
                        currentObject = "version";
                    }
                    else if (mode == "METADATA")
                    {
                        currentObject = song;
                    }
                    else if (mode == "RESOURCES")
                    {
                        currentObject = song;
                    }
                    else if (mode == "COVER")
                    {
                        currentObject = song.Cover;
                    }
                    else if (mode == "COLORS")
                    {
                        currentObject = song;
                    }
                    else if (mode == "TIMING")
                    {
                        currentObject = song.Timing;
                    }
                    else if (mode == "CHARTS")
                    {
                        currentObject = song.Charts;
                    }
                    else 
                    {
                        throw new System.Exception("The specified mode " + mode + " is not a valid mode.");
                    }
                }
                else if (line.StartsWith("+"))
                {
                    string[] tokens = line.Split(' ');
                    if (tokens.Length < 2)
                    { 
                        throw new System.Exception("Object token expected but not found.");
                    }
                    else if (tokens[1] == "Layer")
                    {
                        if (tokens.Length >= 6)
                        {
                            CoverLayer layer = new() {
                                Scale = ParseFloat(tokens[2]),
                                Position = new Vector2(ParseFloat(tokens[3]), ParseFloat(tokens[4])),
                                ParallaxFactor = ParseFloat(tokens[5]),
                            };
                            song.Cover.Layers.Add(layer);
                            currentObject = layer;
                        }
                        else 
                        {
                            throw new System.Exception("Not enough tokens (minimum 6, got " + tokens.Length + ").");
                        }
                    }
                    else if (tokens[1] == "BPM")
                    {
                        if (tokens.Length >= 6)
                        {
                            BPMStop stop = new BPMStop(ParseFloat(tokens[3]), ParseFloat(tokens[2])) {
                                Signature = ParseInt(tokens[4]),
                                Significant = tokens[5] == "S",
                            };
                            song.Timing.Stops.Add(stop);
                            currentObject = stop;
                        }
                        else 
                        {
                            throw new System.Exception("Not enough tokens (minimum 6, got " + tokens.Length + ").");
                        }
                    }
                    else if (tokens[1] == "Chart")
                    {
                        ExternalChartMeta chart = new ExternalChartMeta();

                        song.Charts.Add(chart);
                        currentObject = chart;
                    }
                    else 
                    {
                        throw new System.Exception("The specified object " + tokens[1] + " is not a valid object.");
                    }
                }
                else if (line.Contains(": "))
                {
                    int pos = line.IndexOf(": ");
                    string key = line[..pos];
                    string value = line[(pos + 2)..];

                    if (currentObject is PlayableSong Song)
                    {
                             if (key == "Name")        song.SongName = value;
                             if (key == "Alt Name")    song.AltSongName = value;
                             if (key == "Artist")      song.SongArtist = value;
                             if (key == "Alt Artist")  song.AltSongArtist = value;
                             if (key == "Genre")       song.Genre = value;
                             if (key == "Location")    song.Location = value;
                             
                             if (key == "Clip")        song.ClipPath = value;
                             
                             if (key == "Background")  song.BackgroundColor = ParseColor(value);
                             if (key == "Interface")   song.InterfaceColor = ParseColor(value);
                    }
                    else if (currentObject is Cover cover)
                    {
                             if (key == "Background")   cover.BackgroundColor = ParseColor(value);
                        else if (key == "Icon")         cover.IconTarget = value;
                        else if (key == "Icon Center")  cover.IconCenter = ParseVector(value);
                        else if (key == "Icon Size")    cover.IconSize = ParseFloat(value);
                    }
                    else if (currentObject is CoverLayer layer)
                    {
                             if (key == "Target")  layer.Target = value;
                    }
                    else if (currentObject is ExternalChartMeta chart)
                    {
                             if (key == "Target")    chart.Target = value;
                        else if (key == "Index")     chart.DifficultyIndex = ParseInt(value);
                        else if (key == "Name")      chart.DifficultyName = value;
                        else if (key == "Charter")   chart.CharterName = value;
                        else if (key == "Level")     chart.DifficultyLevel = value;
                        else if (key == "Constant")  chart.ChartConstant = ParseFloat(value);
                    }
                }
                else if (currentObject is CoverLayer layer)
                {
                         if (line == "Tiling")  layer.Tiling = true;
                }
                else if (currentObject?.ToString() == "version")
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!int.TryParse(line, out int version)) continue;
                    if (version > FormatVersion) throw new System.Exception("Chart version is newer than the supported format version. Please open this chart using a newer version of the Chartmaker.");
                }
            }
        }
        catch (System.Exception e) 
        {
            throw new System.Exception("An error occurred while trying to decode line " + index + ":\nContent: " + lines[index - 1] + "\nException: " + e);
        }

        return song;
    }

    static T ParseEnum<T>(string str) where T : System.Enum
    {
        return (T)System.Enum.Parse(typeof(T), str);
    }

    static void ParseEasing(string str, out EaseFunction easing, out EaseMode easeMode)
    {
        string[] tokens = str.Split('/');
        if (tokens.Length == 2)
        {
            easing = (EaseFunction)System.Enum.Parse(typeof(EaseFunction), tokens[0]);
            easeMode = (EaseMode)System.Enum.Parse(typeof(EaseMode), tokens[1]);
        }
        else 
        {
            throw new System.ArgumentException("The specified string is not in a valid Easing format");
        }
    }

    static int ParseInt(string number)
    {
        return int.Parse(number, CultureInfo.InvariantCulture);
    }

    static float ParseFloat(string number)
    {
        return float.Parse(number, CultureInfo.InvariantCulture);
    }

    static float ParseTime(string number)
    {
        return float.Parse(number, CultureInfo.InvariantCulture);
    }
    static Vector3 ParseVector(string str)
    {
        string[] tokens = str.Split(' ');
        return new Vector3(ParseFloat(tokens[0]), ParseFloat(tokens[1]), tokens.Length < 3 ? 0 : ParseFloat(tokens[2]));
    }

    static Color ParseColor(string str)
    {
        string[] tokens = str.Split(' ');
        return new Color(ParseFloat(tokens[0]), ParseFloat(tokens[1]), ParseFloat(tokens[2]), ParseFloat(tokens[3]));
    }
}
