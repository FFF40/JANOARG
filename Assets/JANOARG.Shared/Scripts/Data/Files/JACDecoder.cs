using System;
using System.Globalization;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Shared.Data.Files
{
    public class JACDecoder
    {
        public const int FORMAT_VERSION = 1;
        public const int INDENT_SIZE    = 2;

        public static Chart Decode(string str)
        {
            Chart chart = new();

            chart.Palette.LaneStyles.Clear();
            chart.Palette.HitStyles.Clear();

            var mode = "";

            Lane currentLane = null;
            Text currentText = null;
            object currentObject = null;
            Storyboard currentStoryboard = null;

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
                        {
                            currentObject = "version";
                        }
                        else if (mode == "METADATA")
                        {
                            currentObject = chart;
                        }
                        else if (mode == "CAMERA")
                        {
                            currentObject = chart.Camera;
                            currentStoryboard = chart.Camera.Storyboard;
                        }
                        else if (mode == "PALLETE")
                        {
                            currentObject = chart.Palette;
                            currentStoryboard = chart.Palette.Storyboard;
                        }
                        else if (mode == "GROUPS")
                        {
                            currentObject = chart.Groups;
                            currentStoryboard = null;
                        }
                        else if (mode == "OBJECTS")
                        {
                            currentObject = chart.Lanes;
                            currentStoryboard = null;
                        }
                        else if (mode == "EXTRAS")
                        {
                            currentObject = chart.Texts;
                            currentStoryboard = null;
                        }
                        else
                        {
                            throw new Exception("The specified mode " + mode + " is not a valid mode.");
                        }
                    }
                    else if (line.StartsWith("$"))
                    {
                        string[] tokens = line.Split(' ');

                        if (tokens.Length >= 6)
                        {
                            Timestamp ts = new()
                            {
                                ID = tokens[1],
                                Offset = ParseTime(tokens[2]),
                                Duration = ParseFloat(tokens[3]),
                                Target = ParseFloat(tokens[4]),
                                From = tokens[5] == "_" ? float.NaN : ParseFloat(tokens[5]),
                                Easing = ParseEasing(tokens[6])
                            };

                            currentStoryboard.Add(ts);
                        }
                        else
                        {
                            throw new Exception("Not enough tokens (minimum 6, got " + tokens.Length + ").");
                        }
                    }
                    else if (line.StartsWith("+"))
                    {
                        string[] tokens = line.Split(' ');

                        if (tokens.Length < 2) throw new Exception("Object token expected but not found.");

                        if (tokens[1] == "Group")
                        {
                            if (tokens.Length >= 8)
                            {
                                LaneGroup group = new()
                                {
                                    Position = new Vector3(
                                        ParseFloat(tokens[2]), ParseFloat(tokens[3]),
                                        ParseFloat(tokens[4])),
                                    Rotation = new Vector3(
                                        ParseFloat(tokens[5]), ParseFloat(tokens[6]),
                                        ParseFloat(tokens[7]))
                                };

                                currentObject = group;
                                currentStoryboard = group.Storyboard;
                                chart.Groups.Add(group);
                            }
                            else
                            {
                                throw new Exception("Not enough tokens (minimum 8, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "LaneStyle")
                        {
                            if (tokens.Length >= 10)
                            {
                                LaneStyle style = new()
                                {
                                    LaneColor = new Color(
                                        ParseFloat(tokens[2]), ParseFloat(tokens[3]),
                                        ParseFloat(tokens[4]), ParseFloat(tokens[5])),
                                    JudgeColor = new Color(
                                        ParseFloat(tokens[6]), ParseFloat(tokens[7]),
                                        ParseFloat(tokens[8]), ParseFloat(tokens[9]))
                                };

                                currentObject = style;
                                currentStoryboard = style.Storyboard;
                                chart.Palette.LaneStyles.Add(style);
                            }
                            else
                            {
                                throw new Exception("Not enough tokens (minimum 10, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "HitStyle")
                        {
                            if (tokens.Length >= 14)
                            {
                                HitStyle style = new()
                                {
                                    HoldTailColor = new Color(
                                        ParseFloat(tokens[2]), ParseFloat(tokens[3]),
                                        ParseFloat(tokens[4]), ParseFloat(tokens[5])),
                                    NormalColor = new Color(
                                        ParseFloat(tokens[6]), ParseFloat(tokens[7]),
                                        ParseFloat(tokens[8]), ParseFloat(tokens[9])),
                                    CatchColor = new Color(
                                        ParseFloat(tokens[10]), ParseFloat(tokens[11]),
                                        ParseFloat(tokens[12]), ParseFloat(tokens[13]))
                                };

                                currentObject = style;
                                currentStoryboard = style.Storyboard;
                                chart.Palette.HitStyles.Add(style);
                            }
                            else
                            {
                                throw new Exception("Not enough tokens (minimum 14, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "Lane")
                        {
                            if (tokens.Length >= 9)
                            {
                                Lane lane = new()
                                {
                                    Position = new Vector3(
                                        ParseFloat(tokens[2]), ParseFloat(tokens[3]),
                                        ParseFloat(tokens[4])),
                                    Rotation = new Vector3(
                                        ParseFloat(tokens[5]), ParseFloat(tokens[6]),
                                        ParseFloat(tokens[7])),
                                    StyleIndex = ParseInt(tokens[8])
                                };

                                currentObject = currentLane = lane;
                                currentStoryboard = lane.Storyboard;
                                chart.Lanes.Add(lane);
                            }
                            else
                            {
                                throw new Exception("Not enough tokens (minimum 9, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "LaneStep")
                        {
                            if (tokens.Length >= 12)
                            {
                                LaneStep step = new()
                                {
                                    Offset = ParseTime(tokens[2]),
                                    StartPointPosition =
                                        new Vector2(ParseFloat(tokens[3]), ParseFloat(tokens[4])),
                                    StartEaseX = ParseEasing(tokens[5]),
                                    StartEaseY = ParseEasing(tokens[6]),
                                    EndPointPosition = new Vector2(ParseFloat(tokens[7]), ParseFloat(tokens[8])),
                                    EndEaseX = ParseEasing(tokens[9]),
                                    EndEaseY = ParseEasing(tokens[10]),
                                    Speed = ParseFloat(tokens[11])
                                };

                                currentObject = step;
                                currentStoryboard = step.Storyboard;
                                currentLane.LaneSteps.Add(step);
                            }
                            else
                            {
                                throw new Exception("Not enough tokens (minimum 12, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "Hit")
                        {
                            if (tokens.Length >= 9)
                            {
                                HitObject hit = new()
                                {
                                    Type = ParseEnum<HitObject.HitType>(tokens[2]),
                                    Offset = ParseTime(tokens[3]),
                                    Position = ParseFloat(tokens[4]),
                                    Length = ParseFloat(tokens[5]),
                                    HoldLength = ParseFloat(tokens[6]),
                                    Flickable = tokens[7][0] == 'F',
                                    FlickDirection = tokens[7].Length > 1 ? ParseFloat(tokens[7][1..]) : float.NaN,
                                    StyleIndex = ParseInt(tokens[8])
                                };

                                currentObject = hit;
                                currentStoryboard = hit.Storyboard;
                                currentLane.Objects.Add(hit);
                            }
                            else
                            {
                                throw new Exception("Not enough tokens (minimum 9, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "Text")
                        {
                            if (tokens.Length >= 8)
                            {
                                Text text_r = new Text
                                {
                                    Position = new Vector3(ParseFloat(tokens[2]), ParseFloat(tokens[3]), ParseFloat(tokens[4])),
                                    Rotation = new Vector3(ParseFloat(tokens[5]), ParseFloat(tokens[6]), ParseFloat(tokens[7])),
                                    TextFont = ParseEnum<FontFamily>(tokens[8]),
                                };
                                currentObject = currentText = text_r;
                                currentStoryboard = text_r.Storyboard;
                                chart.Texts.Add(text_r);
                            }
                            else
                            {
                                throw new System.Exception("Not enough tokens (minimum 8, got " + tokens.Length + ").");
                            }
                        }
                        else if (tokens[1] == "TextStep")
                        {
                            Debug.Log(tokens.Length);
                            if (tokens.Length >= 3)
                            {
                                TextStep step = new TextStep
                                {
                                    Offset = ParseTime(tokens[2]),
                                    TextChange = string.Join(" ", tokens[3..]),
                                };
                                currentObject = step;

                                currentText.TextSteps.Add(step);
                            }
                            else
                            {
                                throw new System.Exception("Not enough tokens (minimum 3, got " + tokens.Length + ").");
                            }
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

                        if (currentObject is Chart)
                        {
                            if (key == "Index") chart.DifficultyIndex = ParseInt(value);
                            else if (key == "Name") chart.DifficultyName = value;
                            else if (key == "Charter") chart.CharterName = value;
                            else if (key == "Alt Charter") chart.AltCharterName = value;
                            else if (key == "Level") chart.DifficultyLevel = value;
                            else if (key == "Constant") chart.ChartConstant = ParseFloat(value);
                        }
                        else if (currentObject is CameraController camera)
                        {
                            if (key == "Pivot") camera.CameraPivot = ParseVector(value);
                            else if (key == "Rotation") camera.CameraRotation = ParseVector(value);
                            else if (key == "Distance") camera.PivotDistance = ParseFloat(value);
                        }
                        else if (currentObject is Palette pallete)
                        {
                            if (key == "Background") pallete.BackgroundColor = ParseColor(value);
                            else if (key == "Interface") pallete.InterfaceColor = ParseColor(value);
                        }
                        else if (currentObject is LaneGroup group)
                        {
                            if (key == "Name") group.Name = value;
                            else if (key == "Group") group.Group = value;
                        }
                        else if (currentObject is LaneStyle laneStyle)
                        {
                            if (key == "Name") laneStyle.Name = value;
                            else if (key == "Lane Material") laneStyle.LaneMaterial = value;
                            else if (key == "Lane Target") laneStyle.LaneColorTarget = value;
                            else if (key == "Judge Material") laneStyle.JudgeMaterial = value;
                            else if (key == "Judge Target") laneStyle.JudgeColorTarget = value;
                        }
                        else if (currentObject is HitStyle hitStyle)
                        {
                            if (key == "Name") hitStyle.Name = value;
                            else if (key == "Main Material") hitStyle.MainMaterial = value;
                            else if (key == "Main Target") hitStyle.MainColorTarget = value;
                            else if (key == "Hold Tail Material") hitStyle.HoldTailMaterial = value;
                            else if (key == "Hold Tail Target") hitStyle.HoldTailColorTarget = value;
                        }
                        else if (currentObject is Lane lane)
                        {
                            if (key == "Name") lane.Name = value;
                            else if (key == "Group") lane.Group = value;
                        }
                        else if (currentObject is Text text)
                        {
                            if (key == "Name") text.Name = value;
                            else if (key == "Display") text.DisplayText = value;
                        }
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

            return chart;
        }

        private static T ParseEnum<T>(string str) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        private static IEaseDirective ParseEasing(string str)
        {
            Debug.Log(str);

            if (str == "Linear") return new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);

            string[] tokens = str.Split('/');

            if (tokens.Length == 2)
            {
                if (tokens[0] == "Bezier")
                {
                    string[] nums = tokens[1]
                        .Split(";");

                    return new CubicBezierEaseDirective(
                        new Vector2(ParseFloat(nums[0]), ParseFloat(nums[1])),
                        new Vector2(ParseFloat(nums[2]), ParseFloat(nums[3]))
                    );
                }

                return new BasicEaseDirective(
                    (EaseFunction)Enum.Parse(typeof(EaseFunction), tokens[0]),
                    (EaseMode)Enum.Parse(typeof(EaseMode), tokens[1])
                );
            }

            throw new ArgumentException("The specified string is not in a valid Easing format");
        }

        private static int ParseInt(string number)
        {
            return int.Parse(number, CultureInfo.InvariantCulture);
        }

        private static float ParseFloat(string number)
        {
            return float.Parse(number, CultureInfo.InvariantCulture);
        }

        private static BeatPosition ParseTime(string number)
        {
            int slashPos = number.IndexOf('/');

            if (slashPos >= 0)
            {
                int bPos = number.IndexOf('b');

                return new BeatPosition(
                    ParseInt(number[..bPos]),
                    ParseInt(number[(bPos + 1)..slashPos]),
                    ParseInt(number[(slashPos + 1)..])
                );
            }

            return (BeatPosition)ParseFloat(number.Replace('b', '.'));
        }

        private static Vector3 ParseVector(string str)
        {
            string[] tokens = str.Split(' ');

            return new Vector3(ParseFloat(tokens[0]), ParseFloat(tokens[1]), ParseFloat(tokens[2]));
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