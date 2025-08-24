using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Shared.Data.ChartInfo
{
    /// <summary>
    /// Interface for deep cloning objects.
    /// </summary>
    public interface IDeepClonable<T>
    {
        /// <summary>
        /// Returns a deep clone of the object.
        /// </summary>
        public T DeepClone();
    }

    /// <summary>
    /// Represents a playable chart, containing all lanes, groups, camera, and style information for a song difficulty.
    /// </summary>
    [System.Serializable]
    public class Chart : IDeepClonable<Chart>
    {
        /// <summary>
        /// The display name of the difficulty (e.g., "Normal").
        /// </summary>
        public string DifficultyName  = "Normal";
        /// <summary>
        /// The level of the difficulty (e.g., "6").
        /// </summary>
        public string DifficultyLevel = "6";
        /// <summary>
        /// The index of the difficulty (for sorting).
        /// </summary>
        public int    DifficultyIndex = 1;
        /// <summary>
        /// The chart constant (difficulty rating used for ability rating calculation).
        /// </summary>
        public float  ChartConstant   = 6;

        /// <summary>
        /// The main name of the charter.
        /// </summary>
        public string CharterName    = "";
        /// <summary>
        /// Alternative name of the charter.
        /// </summary>
        public string AltCharterName = "";

        /// <summary>
        /// List of lane groups in this chart.
        /// </summary>
        public List<LaneGroup> Groups = new();
        /// <summary>
        /// List of lanes in this chart.
        /// </summary>
        public List<Lane>      Lanes  = new();

        /// <summary>
        /// The camera controller for this chart.
        /// </summary>
        public CameraController Camera = new();

        /// <summary>
        /// The palette (style) for this chart.
        /// </summary>
        public Palette Palette = new();

        public Chart()
        {
        }

        /// <inheritdoc/>
        public Chart DeepClone()
        {
            Chart clone = new()
            {
                DifficultyName = DifficultyName,
                DifficultyLevel = DifficultyLevel,
                DifficultyIndex = DifficultyIndex,
                ChartConstant = ChartConstant,
                Camera = Camera.DeepClone(),
                Palette = Palette.DeepClone(),
            };

            foreach (LaneGroup group in Groups)
                clone.Groups.Add(group.DeepClone());

            foreach (Lane lane in Lanes)
                clone.Lanes.Add(lane.DeepClone());

            return clone;
        }
    }

    /// <summary>
    /// Controls the camera's position, distance, and rotation for a chart.
    /// </summary>
    [System.Serializable]
    public class CameraController : Storyboardable, IDeepClonable<CameraController>
    {
        /// <summary>
        /// The pivot point of the camera.
        /// </summary>
        public Vector3 CameraPivot;
        /// <summary>
        /// The distance from the pivot point.
        /// </summary>
        public float   PivotDistance = 10;
        /// <summary>
        /// The rotation of the camera.
        /// </summary>
        public Vector3 CameraRotation;

        public static new TimestampType[] TimestampTypes =
        {
            #region Camera Pivot
            new()
            {
                ID = "CameraPivot_X",
                Name = "Camera Pivot X",
                StoryboardGetter = (x) => ((CameraController)x).CameraPivot.x,
                StoryboardSetter = (x, a) => { ((CameraController)x).CameraPivot.x = a; }
            },
            new()
            {
                ID = "CameraPivot_Y",
                Name = "Camera Pivot Y",
                StoryboardGetter = (x) => ((CameraController)x).CameraPivot.y,
                StoryboardSetter = (x, a) => { ((CameraController)x).CameraPivot.y = a; }
            },
            new()
            {
                ID = "CameraPivot_Z",
                Name = "Camera Pivot Z",
                StoryboardGetter = (x) => ((CameraController)x).CameraPivot.z,
                StoryboardSetter = (x, a) => { ((CameraController)x).CameraPivot.z = a; }
            },
            #endregion

            new()
            {
                ID = "PivotDistance",
                Name = "Pivot Distance",
                StoryboardGetter = (x) => ((CameraController)x).PivotDistance,
                StoryboardSetter = (x, a) => { ((CameraController)x).PivotDistance = a; }
            },

            #region Camera Rotation
            new()
            {
                ID = "CameraRotation_X",
                Name = "Camera Rotation X",
                StoryboardGetter = (x) => ((CameraController)x).CameraRotation.x,
                StoryboardSetter = (x, a) => { ((CameraController)x).CameraRotation.x = a; }
            },
            new()
            {
                ID = "CameraRotation_Y",
                Name = "Camera Rotation Y",
                StoryboardGetter = (x) => ((CameraController)x).CameraRotation.y,
                StoryboardSetter = (x, a) => { ((CameraController)x).CameraRotation.y = a; }
            },
            new()
            {
                ID = "CameraRotation_Z",
                Name = "Camera Rotation Z",
                StoryboardGetter = (x) => ((CameraController)x).CameraRotation.z,
                StoryboardSetter = (x, a) => { ((CameraController)x).CameraRotation.z = a; }
            },
            #endregion
        };

        public CameraController DeepClone()
        {
            CameraController clone = new()
            {
                Storyboard = Storyboard.DeepClone(),
                CameraPivot = new Vector3(CameraPivot.x, CameraPivot.y, CameraPivot.z),
                CameraRotation = new Vector3(CameraRotation.x, CameraRotation.y, CameraRotation.z)
            };

            return clone;
        }
    }

    /// <summary>
    /// Defines the color palette and style for a chart, including lane and hit styles.
    /// </summary>
    [System.Serializable]
    public class Palette : Storyboardable, IDeepClonable<Palette>
    {
        /// <summary>
        /// The color of the background.
        /// </summary>
        public Color BackgroundColor = Color.black;
        /// <summary>
        /// The interface color
        /// </summary>
        public Color InterfaceColor  = Color.white;

        /// <summary>
        /// List of lane styles.
        /// </summary>
        public List<LaneStyle> LaneStyles = new();
        /// <summary>
        /// List of hit styles.
        /// </summary>
        public List<HitStyle>  HitStyles  = new();

        public static new TimestampType[] TimestampTypes =
        {
            #region Background Color (RGB)
            new()
            {
                ID = "BackgroundColor_R",
                Name = "Background Color R",
                StoryboardGetter = (x) => ((Palette)x).BackgroundColor.r,
                StoryboardSetter = (x, a) => { ((Palette)x).BackgroundColor.r = a; }
            },
            new()
            {
                ID = "BackgroundColor_G",
                Name = "Background Color G",
                StoryboardGetter = (x) => ((Palette)x).BackgroundColor.g,
                StoryboardSetter = (x, a) => { ((Palette)x).BackgroundColor.g = a; }
            },
            new()
            {
                ID = "BackgroundColor_B",
                Name = "Background Color B",
                StoryboardGetter = (x) => ((Palette)x).BackgroundColor.b,
                StoryboardSetter = (x, a) => { ((Palette)x).BackgroundColor.b = a; }
            },
            #endregion

            #region Interface Color (RGBA)
            new()
            {
                ID = "InterfaceColor_R",
                Name = "Interface Color R",
                StoryboardGetter = (x) => ((Palette)x).InterfaceColor.r,
                StoryboardSetter = (x, a) => { ((Palette)x).InterfaceColor.r = a; }
            },
            new()
            {
                ID = "InterfaceColor_G",
                Name = "Interface Color G",
                StoryboardGetter = (x) => ((Palette)x).InterfaceColor.g,
                StoryboardSetter = (x, a) => { ((Palette)x).InterfaceColor.g = a; }
            },
            new()
            {
                ID = "InterfaceColor_B",
                Name = "Interface Color B",
                StoryboardGetter = (x) => ((Palette)x).InterfaceColor.b,
                StoryboardSetter = (x, a) => { ((Palette)x).InterfaceColor.b = a; }
            },
            new()
            {
                ID = "InterfaceColor_A",
                Name = "Interface Color A",
                StoryboardGetter = (x) => ((Palette)x).InterfaceColor.a,
                StoryboardSetter = (x, a) => { ((Palette)x).InterfaceColor.a = a; }
            },
            #endregion
        };

        public Palette DeepClone()
        {
            Palette clone = new()
            {
                BackgroundColor = new Color(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, BackgroundColor.a),
                InterfaceColor = new Color(InterfaceColor.r, InterfaceColor.g, InterfaceColor.b, InterfaceColor.a),
                Storyboard = Storyboard.DeepClone()
            };

            foreach (LaneStyle ls in LaneStyles) clone.LaneStyles.Add(ls.DeepClone());
            foreach (HitStyle hs in HitStyles) clone.HitStyles.Add(hs.DeepClone());

            return clone;
        }
    }

    /// <summary>
    /// Defines the style for a lane, including materials and colors for the lane and judgment line.
    /// </summary>
    [System.Serializable]
    public class LaneStyle : Storyboardable, IDeepClonable<LaneStyle>
    {
        /// <summary>
        /// The name of this lane style.
        /// </summary>
        public string Name;

        /// <summary>
        /// The material used for the lane.
        /// </summary>
        public string LaneMaterial    = "Default";
        /// <summary>
        /// The shader property for the lane color.
        /// </summary>
        public string LaneColorTarget = "_Color";
        /// <summary>
        /// The color of the lane.
        /// </summary>
        public Color  LaneColor       = Color.black;

        /// <summary>
        /// The material used for the judgment line.
        /// </summary>
        public string JudgeMaterial    = "Default";
        /// <summary>
        /// The shader property for the judgment line color.
        /// </summary>
        public string JudgeColorTarget = "_Color";
        /// <summary>
        /// The color of the judgment line.
        /// </summary>
        public Color  JudgeColor       = Color.black;

        public static new TimestampType[] TimestampTypes =
        {
            #region Lane Color
            new()
            {
                ID = "LaneColor_R",
                Name = "Lane Color R",
                StoryboardGetter = (x) => ((LaneStyle)x).LaneColor.r,
                StoryboardSetter = (x, a) => { ((LaneStyle)x).LaneColor.r = a; }
            },
            new()
            {
                ID = "LaneColor_G",
                Name = "Lane Color G",
                StoryboardGetter = (x) => ((LaneStyle)x).LaneColor.g,
                StoryboardSetter = (x, a) => { ((LaneStyle)x).LaneColor.g = a; }
            },
            new()
            {
                ID = "LaneColor_B",
                Name = "Lane Color B",
                StoryboardGetter = (x) => ((LaneStyle)x).LaneColor.b,
                StoryboardSetter = (x, a) => { ((LaneStyle)x).LaneColor.b = a; }
            },
            new()
            {
                ID = "LaneColor_A",
                Name = "Lane Color A",
                StoryboardGetter = (x) => ((LaneStyle)x).LaneColor.a,
                StoryboardSetter = (x, a) => { ((LaneStyle)x).LaneColor.a = a; }
            },
            #endregion

            #region Judgeline Color
            new()
            {
                ID = "JudgeColor_R",
                Name = "Judge Color R",
                StoryboardGetter = (x) => ((LaneStyle)x).JudgeColor.r,
                StoryboardSetter = (x, a) => { ((LaneStyle)x).JudgeColor.r = a; }
            },
            new()
            {
                ID = "JudgeColor_G",
                Name = "Judge Color G",
                StoryboardGetter = (x) => ((LaneStyle)x).JudgeColor.g,
                StoryboardSetter = (x, a) => { ((LaneStyle)x).JudgeColor.g = a; }
            },
            new()
            {
                ID = "JudgeColor_B",
                Name = "Judge Color B",
                StoryboardGetter = (x) => ((LaneStyle)x).JudgeColor.b,
                StoryboardSetter = (x, a) => { ((LaneStyle)x).JudgeColor.b = a; }
            },
            new()
            {
                ID = "JudgeColor_A",
                Name = "Judge Color A",
                StoryboardGetter = (x) => ((LaneStyle)x).JudgeColor.a,
                StoryboardSetter = (x, a) => { ((LaneStyle)x).JudgeColor.a = a; }
            },
            #endregion
        };

        public LaneStyle DeepClone()
        {
            LaneStyle clone = new()
            {
                LaneMaterial = LaneMaterial,
                LaneColorTarget = LaneColorTarget,
                LaneColor = new Color(LaneColor.r, LaneColor.g, LaneColor.b, LaneColor.a),
                JudgeMaterial = JudgeMaterial,
                JudgeColorTarget = JudgeColorTarget,
                JudgeColor = new Color(JudgeColor.r, JudgeColor.g, JudgeColor.b, JudgeColor.a),
                Storyboard = Storyboard.DeepClone()
            };

            return clone;
        }
    }

    /// <summary>
    /// Defines the style for hit objects, including materials and colors for hit objects and hold tails.
    /// </summary>
    [System.Serializable]
    public class HitStyle : Storyboardable, IDeepClonable<HitStyle>
    {
        /// <summary>
        /// The name of this hit style.
        /// </summary>
        public string Name;

        /// <summary>
        /// The material for the body of the hit object.
        /// </summary>
        public string MainMaterial    = "Default";
        /// <summary>
        /// The shader property for the body color.
        /// </summary>
        public string MainColorTarget = "_Color";
        /// <summary>
        /// The color for normal notes.
        /// </summary>
        public Color  NormalColor     = Color.black;
        /// <summary>
        /// The color for catch notes.
        /// </summary>
        public Color  CatchColor      = Color.blue;

        /// <summary>
        /// The material for the hold tail.
        /// </summary>
        public string HoldTailMaterial    = "Default";
        /// <summary>
        /// The shader property for the hold tail color.
        /// </summary>
        public string HoldTailColorTarget = "_Color";
        /// <summary>
        /// The color for the hold tail.
        /// </summary>
        public Color  HoldTailColor       = Color.black;

        public static new TimestampType[] TimestampTypes =
        {
            #region Tap Note Color
            new()
            {
                ID = "NormalColor_R",
                Name = "Normal Color R",
                StoryboardGetter = (x) => ((HitStyle)x).NormalColor.r,
                StoryboardSetter = (x, a) => { ((HitStyle)x).NormalColor.r = a; }
            },
            new()
            {
                ID = "NormalColor_G",
                Name = "Normal Color G",
                StoryboardGetter = (x) => ((HitStyle)x).NormalColor.g,
                StoryboardSetter = (x, a) => { ((HitStyle)x).NormalColor.g = a; }
            },
            new()
            {
                ID = "NormalColor_B",
                Name = "Normal Color B",
                StoryboardGetter = (x) => ((HitStyle)x).NormalColor.b,
                StoryboardSetter = (x, a) => { ((HitStyle)x).NormalColor.b = a; }
            },
            new()
            {
                ID = "NormalColor_A",
                Name = "Normal Color A",
                StoryboardGetter = (x) => ((HitStyle)x).NormalColor.a,
                StoryboardSetter = (x, a) => { ((HitStyle)x).NormalColor.a = a; }
            },
            #endregion

            #region Catch Note Color
            new()
            {
                ID = "CatchColor_R",
                Name = "Catch Color R",
                StoryboardGetter = (x) => ((HitStyle)x).CatchColor.r,
                StoryboardSetter = (x, a) => { ((HitStyle)x).CatchColor.r = a; }
            },
            new()
            {
                ID = "CatchColor_G",
                Name = "Catch Color G",
                StoryboardGetter = (x) => ((HitStyle)x).CatchColor.g,
                StoryboardSetter = (x, a) => { ((HitStyle)x).CatchColor.g = a; }
            },
            new()
            {
                ID = "CatchColor_B",
                Name = "Catch Color B",
                StoryboardGetter = (x) => ((HitStyle)x).CatchColor.b,
                StoryboardSetter = (x, a) => { ((HitStyle)x).CatchColor.b = a; }
            },
            new()
            {
                ID = "CatchColor_A",
                Name = "Catch Color A",
                StoryboardGetter = (x) => ((HitStyle)x).CatchColor.a,
                StoryboardSetter = (x, a) => { ((HitStyle)x).CatchColor.a = a; }
            },
            #endregion

            #region Hold Note Color (Tail)
            new()
            {
                ID = "HoldTailColor_R",
                Name = "Hold Tail Color R",
                StoryboardGetter = (x) => ((HitStyle)x).HoldTailColor.r,
                StoryboardSetter = (x, a) => { ((HitStyle)x).HoldTailColor.r = a; }
            },
            new()
            {
                ID = "HoldTailColor_G",
                Name = "Hold Tail Color G",
                StoryboardGetter = (x) => ((HitStyle)x).HoldTailColor.g,
                StoryboardSetter = (x, a) => { ((HitStyle)x).HoldTailColor.g = a; }
            },
            new()
            {
                ID = "HoldTailColor_B",
                Name = "Hold Tail Color B",
                StoryboardGetter = (x) => ((HitStyle)x).HoldTailColor.b,
                StoryboardSetter = (x, a) => { ((HitStyle)x).HoldTailColor.b = a; }
            },
            new()
            {
                ID = "HoldTailColor_A",
                Name = "Hold Tail Color A",
                StoryboardGetter = (x) => ((HitStyle)x).HoldTailColor.a,
                StoryboardSetter = (x, a) => { ((HitStyle)x).HoldTailColor.a = a; }
            },
            #endregion
        };

        public HitStyle DeepClone()
        {
            HitStyle clone = new()
            {
                MainMaterial = MainMaterial,
                MainColorTarget = MainColorTarget,
                NormalColor = new Color(NormalColor.r, NormalColor.g, NormalColor.b, NormalColor.a),
                CatchColor = new Color(CatchColor.r, CatchColor.g, CatchColor.b, CatchColor.a),
                HoldTailMaterial = HoldTailMaterial,
                HoldTailColorTarget = HoldTailColorTarget,
                HoldTailColor = new Color(HoldTailColor.r, HoldTailColor.g, HoldTailColor.b, HoldTailColor.a),
                Storyboard = Storyboard.DeepClone()
            };

            return clone;
        }
    }

    /// <summary>
    /// Represents a group for organization and bulk-transformation of lanes.
    /// </summary>
    /// <remarks>
    public class LaneGroup : Storyboardable, IDeepClonable<LaneGroup>
    {
        /// <summary>
        /// The name of this lane group.
        /// </summary>
        public string  Name;
        /// <summary>
        /// The position of this group.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// The rotation of this group.
        /// </summary>
        public Vector3 Rotation;
        /// <summary>
        /// The parent group's name.
        /// </summary>
        public string  Group;

        public static new TimestampType[] TimestampTypes =
        {
            #region Position
            new()
            {
                ID = "Position_X",
                Name = "Position X",
                StoryboardGetter = (x) => ((LaneGroup)x).Position.x,
                StoryboardSetter = (x, a) => { ((LaneGroup)x).Position.x = a; }
            },
            new()
            {
                ID = "Position_Y",
                Name = "Position Y",
                StoryboardGetter = (x) => ((LaneGroup)x).Position.y,
                StoryboardSetter = (x, a) => { ((LaneGroup)x).Position.y = a; }
            },
            new()
            {
                ID = "Position_Z",
                Name = "Position Z",
                StoryboardGetter = (x) => ((LaneGroup)x).Position.z,
                StoryboardSetter = (x, a) => { ((LaneGroup)x).Position.z = a; }
            },
            #endregion

            #region Rotation
            new()
            {
                ID = "Rotation_X",
                Name = "Rotation X",
                StoryboardGetter = (x) => ((LaneGroup)x).Rotation.x,
                StoryboardSetter = (x, a) => { ((LaneGroup)x).Rotation.x = a; }
            },
            new()
            {
                ID = "Rotation_Y",
                Name = "Rotation Y",
                StoryboardGetter = (x) => ((LaneGroup)x).Rotation.y,
                StoryboardSetter = (x, a) => { ((LaneGroup)x).Rotation.y = a; }
            },
            new()
            {
                ID = "Rotation_Z",
                Name = "Rotation Z",
                StoryboardGetter = (x) => ((LaneGroup)x).Rotation.z,
                StoryboardSetter = (x, a) => { ((LaneGroup)x).Rotation.z = a; }
            },
            #endregion
        };

        public LaneGroup DeepClone()
        {
            LaneGroup clone = new()
            {
                Name = Name,
                Position = new Vector3(Position.x, Position.y, Position.z),
                Rotation = new Vector3(Rotation.x, Rotation.y, Rotation.z),
                Storyboard = Storyboard.DeepClone(),
                Group = Group
            };

            return clone;
        }
    }

    /// <summary>
    /// Represents the position and offset of a lane at a given time.
    /// </summary>
    [System.Serializable]
    public class LanePosition
    {
        /// <summary>
        /// The start position of the lane.
        /// </summary>
        [FormerlySerializedAs("StartPos")] public Vector2 StartPosition;
        /// <summary>
        /// The end position of the lane.
        /// </summary>
        [FormerlySerializedAs("EndPos")]   public Vector2 EndPosition;
        /// <summary>
        /// The offset value for the lane.
        /// </summary>
        public                                    float   Offset;
    }

    /// <summary>
    /// Represents a lane in a chart, containing hit objects, steps, and position/rotation data.
    /// </summary>
    [System.Serializable]
    public class Lane : DirtyTrackedStoryboardable, IDeepClonable<Lane>
    {
        /// <summary>
        /// The name of this lane.
        /// </summary>
        public string Name;

        /// <summary>
        /// List of hit objects in this lane.
        /// </summary>
        public List<HitObject> Objects   = new();
        /// <summary>
        /// List of this lane's steps.
        /// </summary>
        public List<LaneStep>  LaneSteps = new();

        /// <summary>
        /// The position offset of the lane.
        /// </summary>
        [FormerlySerializedAs("Offset")]
        public Vector3 Position;

        /// <summary>
        /// The rotation offset of the lane.
        /// </summary>
        [FormerlySerializedAs("OffsetRotation")]
        public Vector3 Rotation;

        /// <summary>
        /// The name of the <see cref="LaneGroup"/> this lane belongs to.
        /// </summary>
        public string Group;

        /// <summary>
        /// The style index for this lane.
        /// </summary>
        public int StyleIndex = 0;

        public LanePosition GetLanePosition(float time, float laneTime, Metronome timing)
        {
            float offset = 0;
            float timeT = timing.ToSeconds(time);
            float laneTimeT = timing.ToSeconds(laneTime);
            float curtime = laneTimeT;
            List<LaneStep> steps = new();

            for (var a = 0; a < LaneSteps.Count; a++)
            {
                var step = (LaneStep)LaneSteps[a]
                    .GetStoryboardableObject(laneTime);

                steps.Add(step);

                float t = timing.ToSeconds(step.Offset);
                offset += step.Speed * (Mathf.Max(t, laneTimeT) - curtime);
                curtime = Mathf.Max(t, laneTimeT);

                if (Mathf.Approximately(time, step.Offset))
                {
                    return new LanePosition
                    {
                        StartPosition = step.StartPointPosition,
                        EndPosition = step.EndPointPosition,
                        Offset = laneTime < time ? offset : float.NaN
                    };
                }
                else if (time < step.Offset)
                {
                    if (a == 0)
                        return new LanePosition
                        {
                            StartPosition = step.StartPointPosition,
                            EndPosition = step.EndPointPosition,
                            Offset = laneTime < time ? offset : float.NaN
                        };

                    LaneStep previousStep = steps[a - 1];
                    float percentageDifference = (time - previousStep.Offset) / (step.Offset - previousStep.Offset);

                    if (step.isLinear)
                        return new LanePosition
                        {
                            StartPosition = Vector2.LerpUnclamped(previousStep.StartPointPosition, step.StartPointPosition, percentageDifference),
                            EndPosition = Vector2.LerpUnclamped(previousStep.EndPointPosition, step.EndPointPosition, percentageDifference),
                            Offset = laneTime < time ? offset + (timeT - t) * step.Speed : BeatPosition.NaN
                        };
                    else
                        return new LanePosition
                        {
                            StartPosition = new Vector2(Mathf.LerpUnclamped(previousStep.StartPointPosition.x, step.StartPointPosition.x, step.StartEaseX.Get(percentageDifference)),
                                Mathf.LerpUnclamped(previousStep.StartPointPosition.y, step.StartPointPosition.y, step.StartEaseY.Get(percentageDifference))),
                            EndPosition = new Vector2(Mathf.LerpUnclamped(previousStep.EndPointPosition.x, step.EndPointPosition.x, step.EndEaseX.Get(percentageDifference)),
                                Mathf.LerpUnclamped(previousStep.EndPointPosition.y, step.EndPointPosition.y, step.EndEaseY.Get(percentageDifference))),
                            Offset = laneTime < time ? offset + (timeT - t) * step.Speed : BeatPosition.NaN
                        };
                }
            }

            {
                // Array[^x] == Array[Array.Count - x]
                float t = timing.ToSeconds(steps[^1].Offset);

                return new LanePosition
                {
                    StartPosition = steps[^1].StartPointPosition,
                    EndPosition = steps[^1].EndPointPosition,
                    Offset = laneTime < time ? offset + (timeT - t) * LaneSteps[^1].Speed : float.NaN
                };
            }
        }

        // More as offset
        public static new TimestampType[] TimestampTypes =
        {
            #region Position
            new()
            {
                ID = "Offset_X",
                Name = "Position X",
                StoryboardGetter = (x) => ((Lane)x).Position.x,
                StoryboardSetter = (x, a) => { ((Lane)x).Position.x = a; }
            },
            new()
            {
                ID = "Offset_Y",
                Name = "Position Y",
                StoryboardGetter = (x) => ((Lane)x).Position.y,
                StoryboardSetter = (x, a) => { ((Lane)x).Position.y = a; }
            },
            new()
            {
                ID = "Offset_Z",
                Name = "Position Z",
                StoryboardGetter = (x) => ((Lane)x).Position.z,
                StoryboardSetter = (x, a) => { ((Lane)x).Position.z = a; }
            },
            #endregion

            #region Rotation
            new()
            {
                ID = "OffsetRotation_X",
                Name = "Rotation X",
                StoryboardGetter = (x) => ((Lane)x).Rotation.x,
                StoryboardSetter = (x, a) => { ((Lane)x).Rotation.x = a; }
            },
            new()
            {
                ID = "OffsetRotation_Y",
                Name = "Rotation Y",
                StoryboardGetter = (x) => ((Lane)x).Rotation.y,
                StoryboardSetter = (x, a) => { ((Lane)x).Rotation.y = a; }
            },
            new()
            {
                ID = "OffsetRotation_Z",
                Name = "Rotation Z",
                StoryboardGetter = (x) => ((Lane)x).Rotation.z,
                StoryboardSetter = (x, a) => { ((Lane)x).Rotation.z = a; }
            },
            #endregion
        };

        public Lane DeepClone()
        {
            Lane clone = new()
            {
                Position = new Vector3(Position.x, Position.y, Position.z),
                Rotation = new Vector3(Rotation.x, Rotation.y, Rotation.z),
                Group = Group,
                StyleIndex = StyleIndex,
                Storyboard = Storyboard.DeepClone()
            };

            foreach (HitObject obj in Objects)
                clone.Objects.Add(obj.DeepClone());

            foreach (LaneStep step in LaneSteps)
                clone.LaneSteps.Add(step.DeepClone());

            return clone;
        }
    }

    /// <summary>
    /// Represents a point of a Lane, which defines the lane's position, shape, and scroll speed.
    /// </summary>
    [System.Serializable]
    public class LaneStep : DirtyTrackedStoryboardable, IDeepClonable<LaneStep>
    {
        /// <summary>
        /// The time position of this step, in song beats.
        /// </summary>
        public BeatPosition Offset = new();

        /// <summary>
        /// The start point position of the lane at this step.
        /// </summary>
        public                      Vector2        StartPointPosition;
        /// <summary>
        /// The easing directive for the X component of the start point.
        /// </summary>
        [SerializeReference] public IEaseDirective StartEaseX = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);
        /// <summary>
        /// The easing directive for the Y component of the start point.
        /// </summary>
        [SerializeReference] public IEaseDirective StartEaseY = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);

        /// <summary>
        /// The end point position of the lane at this step.
        /// </summary>
        public                      Vector2        EndPointPosition;
        /// <summary>
        /// The easing directive for the X component of the end point.
        /// </summary>
        [SerializeReference] public IEaseDirective EndEaseX = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);
        /// <summary>
        /// The easing directive for the Y component of the end point.
        /// </summary>
        [SerializeReference] public IEaseDirective EndEaseY = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);

        /// <summary>
        /// The speed of the lane during this step.
        /// </summary>
        public float Speed = 1;

        /// <summary>
        /// Returns true if all easing is linear.
        /// </summary>
        public bool isLinear =>
            StartEaseX is BasicEaseDirective startEaseX &&
            StartEaseY is BasicEaseDirective startEaseY &&
            EndEaseX is BasicEaseDirective endEaseX &&
            EndEaseY is BasicEaseDirective endEaseY &&
            startEaseX.Function == EaseFunction.Linear &&
            startEaseY.Function == EaseFunction.Linear &&
            endEaseX.Function == EaseFunction.Linear &&
            endEaseY.Function == EaseFunction.Linear;

        public static new TimestampType[] TimestampTypes =
        {
            #region Start Position
            new()
            {
                ID = "StartPos_X",
                Name = "Start Position X",
                StoryboardGetter = (x) => ((LaneStep)x).StartPointPosition.x,
                StoryboardSetter = (x, a) => { ((LaneStep)x).StartPointPosition.x = a; }
            },
            new()
            {
                ID = "StartPos_Y",
                Name = "Start Position Y",
                StoryboardGetter = (x) => ((LaneStep)x).StartPointPosition.y,
                StoryboardSetter = (x, a) => { ((LaneStep)x).StartPointPosition.y = a; }
            },
            #endregion

            #region End Position
            new()
            {
                ID = "EndPos_X",
                Name = "End Position X",
                StoryboardGetter = (x) => ((LaneStep)x).EndPointPosition.x,
                StoryboardSetter = (x, a) => { ((LaneStep)x).EndPointPosition.x = a; }
            },
            new()
            {
                ID = "EndPos_Y",
                Name = "End Position Y",
                StoryboardGetter = (x) => ((LaneStep)x).EndPointPosition.y,
                StoryboardSetter = (x, a) => { ((LaneStep)x).EndPointPosition.y = a; }
            },
            #endregion

            new()
            {
                ID = "Speed",
                Name = "Speed",
                StoryboardGetter = (x) => ((LaneStep)x).Speed,
                StoryboardSetter = (x, a) => { ((LaneStep)x).Speed = a; }
            }
        };

        public LaneStep DeepClone()
        {
            LaneStep clone = new()
            {
                Offset = Offset,
                StartPointPosition = new Vector2(StartPointPosition.x, StartPointPosition.y),
                StartEaseX = StartEaseX,
                StartEaseY = StartEaseY,
                EndPointPosition = new Vector2(EndPointPosition.x, EndPointPosition.y),
                EndEaseX = EndEaseX,
                EndEaseY = EndEaseY,
                Speed = Speed,
                Storyboard = Storyboard.DeepClone()
            };

            return clone;
        }
    }

    /// <summary>
    /// Represents a hit object (note) on a lane.
    /// </summary>
    [System.Serializable]
    public class HitObject : DirtyTrackedStoryboardable, IDeepClonable<HitObject>
    {
        /// <summary>
        /// The type of hit object (normal or catch).
        /// </summary>
        public HitType      Type;
        /// <summary>
        /// The time position of this step, in song beats.
        /// </summary>
        public BeatPosition Offset = new();
        /// <summary>
        /// The position of the hit object (0~1 range).
        /// </summary>
        public float        Position;
        /// <summary>
        /// The length of the hit object (0~1 range).
        /// </summary>
        public float        Length;
        /// <summary>
        /// The hold duration of the hit object, in beats.
        /// </summary>
        public float        HoldLength = 0;
        /// <summary>
        /// Whether this hit object is flickable.
        /// </summary>
        public bool         Flickable;
        /// <summary>
        /// The direction of the flick (in degrees), or NaN for omni-directional flick.
        /// </summary>
        public float        FlickDirection = float.NaN;

        /// <summary>
        /// The style index for this hit object.
        /// </summary>
        public int StyleIndex = 0;

        /// <summary>
        /// The type of hit object.
        /// </summary>
        public enum HitType
        {
            /// <summary>
            /// A normal (tap) note.
            /// </summary>
            Normal,
            /// <summary>
            /// A catch note.
            /// </summary>
            Catch
        }

        public static new TimestampType[] TimestampTypes =
        {
            new()
            {
                ID = "Position",
                Name = "Position",
                StoryboardGetter = (x) => ((HitObject)x).Position,
                StoryboardSetter = (x, a) => { ((HitObject)x).Position = a; }
            },
            new()
            {
                ID = "Length",
                Name = "Length",
                StoryboardGetter = (x) => ((HitObject)x).Length,
                StoryboardSetter = (x, a) => { ((HitObject)x).Length = a; }
            }
        };

        public HitObject DeepClone()
        {
            HitObject clone = new()
            {
                Type = Type,
                Offset = Offset,
                Position = Position,
                Length = Length,
                HoldLength = HoldLength,
                Flickable = Flickable,
                FlickDirection = FlickDirection,
                StyleIndex = StyleIndex,
                Storyboard = Storyboard.DeepClone()
            };

            return clone;
        }
    }

    /// <summary>
    /// Specifies the coordinate mode for lane/group/global positioning.
    /// </summary>
    public enum CoordinateMode
    {
        /// <summary>
        /// Local coordinates (relative to lane).
        /// </summary>
        Local,
        /// <summary>
        /// Group coordinates (relative to group).
        /// </summary>
        Group,
        /// <summary>
        /// Global coordinates (absolute).
        /// </summary>
        Global
    }
}