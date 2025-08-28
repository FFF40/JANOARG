using System;
using System.Globalization;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Shared.Data.Files
{
    /// <summary>
    /// Utility class to encode a Chart into a .JAC file.
    /// </summary>
    public class JACEncoder
    {
        public const int FORMAT_VERSION = 1;
        public const int INDENT_SIZE    = 2;

        /// <summary>
        /// Encode a Chart into a .JAC file.
        /// </summary>
        /// <param name="chart">The Chart object.</param>
        /// <returns>A string under the .JAC file format<./returns>
        public static string Encode(Chart chart)
        {
            var str = "JANOARG Chart Format\ngithub.com/FFF40/JANOARG";

            str += "\n\n[VERSION]\n" + FORMAT_VERSION;

            str += "\n\n[METADATA]";
            str += "\nIndex: " + chart.DifficultyIndex.ToString(CultureInfo.InvariantCulture);
            str += "\nName: " + chart.DifficultyName;
            str += "\nCharter: " + chart.CharterName;

            if (!string.IsNullOrWhiteSpace(chart.AltCharterName))
                str += "\nAlt Charter: " + chart.AltCharterName;

            str += "\nLevel: " + chart.DifficultyLevel;
            str += "\nConstant: " + chart.ChartConstant.ToString(CultureInfo.InvariantCulture);

            str += "\n\n[CAMERA]";
            str += "\nPivot: " + EncodeVector(chart.Camera.CameraPivot);
            str += "\nRotation: " + EncodeVector(chart.Camera.CameraRotation);
            str += "\nDistance: " + chart.Camera.PivotDistance.ToString(CultureInfo.InvariantCulture);
            str += EncodeStoryboard(chart.Camera);

            str += "\n\n[GROUPS]";
            foreach (LaneGroup group in chart.Groups) str += EncodeLaneGroup(group);

            str += "\n\n[PALLETE]";
            str += "\nBackground: " + EncodeColor(chart.Palette.BackgroundColor);
            str += "\nInterface: " + EncodeColor(chart.Palette.InterfaceColor);
            str += EncodeStoryboard(chart.Palette);
            foreach (LaneStyle style in chart.Palette.LaneStyles) str += EncodeLaneStyle(style);

            foreach (HitStyle style in chart.Palette.HitStyles) str += EncodeHitStyle(style);

            str += "\n\n[OBJECTS]";
            foreach (Lane lane in chart.Lanes) str += EncodeLane(lane);

            return str;
        }

        public static string EncodeStoryboard(Storyboardable storyboard, int depth = 0)
        {
            return EncodeStoryboard(storyboard.Storyboard, depth);
        }

        public static string EncodeStoryboard(Storyboard storyboard, int depth = 0)
        {
            var str = "";
            string indent = new(' ', depth);

            foreach (Timestamp t in storyboard.Timestamps)
                str += "\n" +
                       indent +
                       "$ " +
                       t.ID +
                       " " +
                       t.Offset.ToString(CultureInfo.InvariantCulture) +
                       " " +
                       t.Duration.ToString(CultureInfo.InvariantCulture) +
                       " " +
                       t.Target.ToString(CultureInfo.InvariantCulture) +
                       " " +
                       (float.IsFinite(t.From) ? t.From.ToString(CultureInfo.InvariantCulture) : "_") +
                       " " +
                       EncodeEase(t.Easing);

            return str;
        }

        public static string EncodeLaneGroup(LaneGroup group, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ Group" +
                         " " +
                         EncodeVector(group.Position) +
                         " " +
                         EncodeVector(group.Rotation);

            str += "\n" + indent2 + "Name: " + group.Name;

            if (!string.IsNullOrEmpty(group.Group))
                str += "\n" + indent2 + "Group: " + group.Group;

            str += EncodeStoryboard(group, depth + INDENT_SIZE);

            return str;
        }

        public static string EncodeLaneStyle(LaneStyle style, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ LaneStyle" +
                         " " +
                         EncodeColor(style.LaneColor) +
                         " " +
                         EncodeColor(style.JudgeColor);

            if (!string.IsNullOrEmpty(style.Name)) str += "\n" + indent2 + "Name: " + style.Name;

            string lanePath = style.LaneMaterial;
            string judgePath = style.JudgeMaterial;

            if (!string.IsNullOrEmpty(lanePath) && lanePath != "Default")
                str += "\n" + indent2 + "Lane Material: " + lanePath;

            if (!string.IsNullOrEmpty(style.LaneColorTarget) && style.LaneColorTarget != "_Color")
                str += "\n" + indent2 + "Lane Target: " + style.LaneColorTarget;

            if (!string.IsNullOrEmpty(judgePath) && judgePath != "Default")
                str += "\n" + indent2 + "Judge Material: " + judgePath;

            if (!string.IsNullOrEmpty(style.JudgeColorTarget) && style.JudgeColorTarget != "_Color")
                str += "\n" + indent2 + "Judge Target: " + style.JudgeColorTarget;

            str += EncodeStoryboard(style, depth + INDENT_SIZE);

            return str;
        }

        public static string EncodeHitStyle(HitStyle style, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ HitStyle" +
                         " " +
                         EncodeColor(style.HoldTailColor) +
                         " " +
                         EncodeColor(style.NormalColor) +
                         " " +
                         EncodeColor(style.CatchColor);

            if (!string.IsNullOrEmpty(style.Name)) str += "\n" + indent2 + "Name: " + style.Name;

            string mainPath = style.MainMaterial;
            string holdPath = style.HoldTailMaterial;

            if (!string.IsNullOrEmpty(mainPath) && mainPath != "Default")
                str += "\n" + indent2 + "Main Material: " + mainPath;

            if (!string.IsNullOrEmpty(style.MainColorTarget) && style.MainColorTarget != "_Color")
                str += "\n" + indent2 + "Main Target: " + style.MainColorTarget;

            if (!string.IsNullOrEmpty(holdPath) && holdPath != "Default")
                str += "\n" + indent2 + "Hold Tail Material: " + holdPath;

            if (!string.IsNullOrEmpty(style.HoldTailColorTarget) && style.HoldTailColorTarget != "_Color")
                str += "\n" + indent2 + "Hold Tail Target: " + style.HoldTailColorTarget;

            str += EncodeStoryboard(style, depth + INDENT_SIZE);

            return str;
        }

        public static string EncodeLane(Lane lane, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ Lane" +
                         " " +
                         EncodeVector(lane.Position) +
                         " " +
                         EncodeVector(lane.Rotation) +
                         " " +
                         lane.StyleIndex.ToString(CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(lane.Name)) str += "\n" + indent2 + "Name: " + lane.Name;

            if (!string.IsNullOrEmpty(lane.Group))
                str += "\n" + indent2 + "Group: " + lane.Group;

            str += EncodeStoryboard(lane, depth + INDENT_SIZE);

            int depth2 = depth + INDENT_SIZE;
            foreach (LaneStep step in lane.LaneSteps) str += EncodeLaneStep(step, depth2);

            foreach (HitObject hit in lane.Objects) str += EncodeHitObject(hit, depth2);

            return str;
        }

        public static string EncodeLaneStep(LaneStep step, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ LaneStep" +
                         " " +
                         step.Offset.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         EncodeVector(step.StartPointPosition) +
                         " " +
                         EncodeEase(step.StartEaseX) +
                         " " +
                         EncodeEase(step.StartEaseY) +
                         " " +
                         EncodeVector(step.EndPointPosition) +
                         " " +
                         EncodeEase(step.EndEaseX) +
                         " " +
                         EncodeEase(step.EndEaseY) +
                         " " +
                         step.Speed.ToString(CultureInfo.InvariantCulture);

            str += EncodeStoryboard(step, depth + INDENT_SIZE);

            return str;
        }

        public static string EncodeHitObject(HitObject hit, int depth = 0)
        {
            string indent = new(' ', depth);
            string indent2 = new(' ', depth + INDENT_SIZE);

            string str = "\n" +
                         indent +
                         "+ Hit" +
                         " " +
                         hit.Type +
                         " " +
                         hit.Offset.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         hit.Position.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         hit.Length.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         hit.HoldLength.ToString(CultureInfo.InvariantCulture) +
                         " " +
                         (hit.Flickable
                             ? "F" +
                               (float.IsFinite(hit.FlickDirection)
                                   ? hit.FlickDirection.ToString(CultureInfo.InvariantCulture)
                                   : "")
                             : "N") +
                         " " +
                         hit.StyleIndex.ToString(CultureInfo.InvariantCulture);

            str += EncodeStoryboard(hit, depth + INDENT_SIZE);

            return str;
        }

        public static string EncodeEase(IEaseDirective ease)
        {
            if (ease is BasicEaseDirective bed)
            {
                if (bed.Function == EaseFunction.Linear) return "Linear";

                return bed.Function + "/" + bed.Mode;
            }

            if (ease is CubicBezierEaseDirective cbed)
                return "Bezier/" +
                       cbed.Point1.x.ToString(CultureInfo.InvariantCulture) +
                       ";" +
                       cbed.Point1.y.ToString(CultureInfo.InvariantCulture) +
                       ";" +
                       cbed.Point2.x.ToString(CultureInfo.InvariantCulture) +
                       ";" +
                       cbed.Point2.y.ToString(CultureInfo.InvariantCulture);

            throw new Exception("Unknown ease directive " + ease.GetType());
        }

        public static string EncodeVector(Vector2 vec)
        {
            return vec.x.ToString(CultureInfo.InvariantCulture) + " " + vec.y.ToString(CultureInfo.InvariantCulture);
        }

        public static string EncodeVector(Vector3 vec)
        {
            return vec.x.ToString(CultureInfo.InvariantCulture) +
                   " " +
                   vec.y.ToString(CultureInfo.InvariantCulture) +
                   " " +
                   vec.z.ToString(CultureInfo.InvariantCulture);
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