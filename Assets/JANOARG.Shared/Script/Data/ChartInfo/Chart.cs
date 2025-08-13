using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public interface IDeepClonable<T>
{
    public T DeepClone();
}

[System.Serializable]
public class Chart : IDeepClonable<Chart>
{
    public string DifficultyName = "Normal";
    public string DifficultyLevel = "6";
    public int DifficultyIndex = 1;
    public float ChartConstant = 6;

    public string CharterName = "";
    public string AltCharterName = "";

    public List<LaneGroup> Groups = new();
    public List<Lane> Lanes = new();

    public CameraController Camera = new();
    public Vector3 CameraPivot;
    public Vector3 CameraRotation;

    public Palette Palette = new();

    public Chart() 
    {
        
    }
    
    public Chart DeepClone()
    {
        Chart clone = new()
        {
            DifficultyName  = DifficultyName,
            DifficultyLevel = DifficultyLevel,
            DifficultyIndex = DifficultyIndex,
            ChartConstant   = ChartConstant,
            Camera          = Camera.DeepClone(),
            Palette         = Palette.DeepClone(),
            CameraPivot     = new Vector3(CameraPivot.x, CameraPivot.y, CameraPivot.z),
            CameraRotation  = new Vector3(CameraRotation.x, CameraRotation.y, CameraRotation.z),
        };
        
        foreach (LaneGroup group in Groups) 
            clone.Groups.Add(group.DeepClone());
        
        foreach (Lane lane in Lanes) 
            clone.Lanes.Add(lane.DeepClone());
        
        return clone;
    }
}

[System.Serializable]
public class CameraController : Storyboardable, IDeepClonable<CameraController> {
    public Vector3 CameraPivot;
    public float PivotDistance = 10;
    public Vector3 CameraRotation;
    
    public new static TimestampType[] TimestampTypes = 
    {
        #region Camera Pivot
        new() 
        {
            ID   = "CameraPivot_X",
            Name = "Camera Pivot X",
            Get  = (x) => ((CameraController)x).CameraPivot.x,
            Set  = (x, a) => { ((CameraController)x).CameraPivot.x = a; },
        },
        new() 
        {
            ID   = "CameraPivot_Y",
            Name = "Camera Pivot Y",
            Get  = (x) => ((CameraController)x).CameraPivot.y,
            Set  = (x, a) => { ((CameraController)x).CameraPivot.y = a; },
        },
        new() 
        {
            ID   = "CameraPivot_Z",
            Name = "Camera Pivot Z",
            Get  = (x) => ((CameraController)x).CameraPivot.z,
            Set  = (x, a) => { ((CameraController)x).CameraPivot.z = a; },
        },
        #endregion
        
        new()
        {
            ID   = "PivotDistance",
            Name = "Pivot Distance",
            Get  = (x) => ((CameraController)x).PivotDistance,
            Set  = (x, a) => { ((CameraController)x).PivotDistance = a; },
        },
        
        #region Camera Rotation
        new() 
        {
            ID   = "CameraRotation_X",
            Name = "Camera Rotation X",
            Get  = (x) => ((CameraController)x).CameraRotation.x,
            Set  = (x, a) => { ((CameraController)x).CameraRotation.x = a; },
        },
        new() 
        {
            ID   = "CameraRotation_Y",
            Name = "Camera Rotation Y",
            Get  = (x) => ((CameraController)x).CameraRotation.y,
            Set  = (x, a) => { ((CameraController)x).CameraRotation.y = a; },
        },
        new() 
        {
            ID   = "CameraRotation_Z",
            Name = "Camera Rotation Z",
            Get  = (x) => ((CameraController)x).CameraRotation.z,
            Set  = (x, a) => { ((CameraController)x).CameraRotation.z = a; },
        },
        #endregion
    };
    
    public CameraController DeepClone()
    {
        CameraController clone = new()
        {
            Storyboard     = Storyboard.DeepClone(),
            CameraPivot    = new Vector3(CameraPivot.x, CameraPivot.y, CameraPivot.z),
            CameraRotation = new Vector3(CameraRotation.x, CameraRotation.y, CameraRotation.z),
        };
        return clone;
    }
}

// Style 
[System.Serializable]
public class Palette : Storyboardable, IDeepClonable<Palette>  {

    public Color BackgroundColor = Color.black;
    public Color InterfaceColor = Color.white;

    public List<LaneStyle> LaneStyles = new();
    public List<HitStyle> HitStyles = new();

    public new static TimestampType[] TimestampTypes = 
    {
        #region Background Color (RGB)
        new() 
        {
            ID   = "BackgroundColor_R",
            Name = "Background Color R",
            Get  = (x) => ((Palette)x).BackgroundColor.r,
            Set  = (x, a) => { ((Palette)x).BackgroundColor.r = a; },
        },
        new() 
        {
            ID   = "BackgroundColor_G",
            Name = "Background Color G",
            Get  = (x) => ((Palette)x).BackgroundColor.g,
            Set  = (x, a) => { ((Palette)x).BackgroundColor.g = a; },
        },
        new()
        {
            ID   = "BackgroundColor_B",
            Name = "Background Color B",
            Get  = (x) => ((Palette)x).BackgroundColor.b,
            Set  = (x, a) => { ((Palette)x).BackgroundColor.b = a; },
        },
        #endregion
        
        #region Interface Color (RGBA)
        new()
        {
            ID   = "InterfaceColor_R",
            Name = "Interface Color R",
            Get  = (x) => ((Palette)x).InterfaceColor.r,
            Set  = (x, a) => { ((Palette)x).InterfaceColor.r = a; },
        },
        new() 
        {
            ID   = "InterfaceColor_G",
            Name = "Interface Color G",
            Get  = (x) => ((Palette)x).InterfaceColor.g,
            Set  = (x, a) => { ((Palette)x).InterfaceColor.g = a; },
        },
        new() 
        {
            ID   = "InterfaceColor_B",
            Name = "Interface Color B",
            Get  = (x) => ((Palette)x).InterfaceColor.b,
            Set  = (x, a) => { ((Palette)x).InterfaceColor.b = a; },
        },
        new() 
        {
            ID   = "InterfaceColor_A",
            Name = "Interface Color A",
            Get  = (x) => ((Palette)x).InterfaceColor.a,
            Set  = (x, a) => { ((Palette)x).InterfaceColor.a = a; },
        },
        #endregion
    };

    public Palette DeepClone()
    {
        Palette clone = new()
        {
            BackgroundColor = new Color(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, BackgroundColor.a),
            InterfaceColor = new Color(InterfaceColor.r, InterfaceColor.g, InterfaceColor.b, InterfaceColor.a),
            Storyboard = Storyboard.DeepClone(),
        };
        foreach (LaneStyle ls in LaneStyles) clone.LaneStyles.Add(ls.DeepClone());
        foreach (HitStyle hs in HitStyles) clone.HitStyles.Add(hs.DeepClone());
        return clone;
    }
}

[System.Serializable]
public class LaneStyle : Storyboardable, IDeepClonable<LaneStyle> 
{
    public string Name;

    public string LaneMaterial = "Default";
    public string LaneColorTarget = "_Color";
    public Color LaneColor = Color.black;

    public string JudgeMaterial = "Default";
    public string JudgeColorTarget = "_Color";
    public Color JudgeColor = Color.black;

    public new static TimestampType[] TimestampTypes = 
    {
        #region Lane Color
        new() 
        {
            ID   = "LaneColor_R",
            Name = "Lane Color R",
            Get  = (x) => ((LaneStyle)x).LaneColor.r,
            Set  = (x, a) => { ((LaneStyle)x).LaneColor.r = a; },
        },
        new() 
        {
            ID   = "LaneColor_G",
            Name = "Lane Color G",
            Get  = (x) => ((LaneStyle)x).LaneColor.g,
            Set  = (x, a) => { ((LaneStyle)x).LaneColor.g = a; },
        },
        new() 
        {
            ID   = "LaneColor_B",
            Name = "Lane Color B",
            Get  = (x) => ((LaneStyle)x).LaneColor.b,
            Set  = (x, a) => { ((LaneStyle)x).LaneColor.b = a; },
        },
        new()
        {
            ID   = "LaneColor_A",
            Name = "Lane Color A",
            Get  = (x) => ((LaneStyle)x).LaneColor.a,
            Set  = (x, a) => { ((LaneStyle)x).LaneColor.a = a; },
        },
        #endregion
        
        #region Judgeline Color
        new() 
        {
            ID   = "JudgeColor_R",
            Name = "Judge Color R",
            Get  = (x) => ((LaneStyle)x).JudgeColor.r,
            Set  = (x, a) => { ((LaneStyle)x).JudgeColor.r = a; },
        },
        new() 
        {
            ID   = "JudgeColor_G",
            Name = "Judge Color G",
            Get  = (x) => ((LaneStyle)x).JudgeColor.g,
            Set  = (x, a) => { ((LaneStyle)x).JudgeColor.g = a; },
        },
        new()
        {
            ID   = "JudgeColor_B",
            Name = "Judge Color B",
            Get  = (x) => ((LaneStyle)x).JudgeColor.b,
            Set  = (x, a) => { ((LaneStyle)x).JudgeColor.b = a; },
        },
        new()
        {
            ID   = "JudgeColor_A",
            Name = "Judge Color A",
            Get  = (x) => ((LaneStyle)x).JudgeColor.a,
            Set  = (x, a) => { ((LaneStyle)x).JudgeColor.a = a; },
        },
        #endregion
    };

    public LaneStyle DeepClone()
    {
        LaneStyle clone = new()
        {
            LaneMaterial     = LaneMaterial,
            LaneColorTarget  = LaneColorTarget,
            LaneColor        = new Color(LaneColor.r, LaneColor.g, LaneColor.b, LaneColor.a),
            JudgeMaterial    = JudgeMaterial,
            JudgeColorTarget = JudgeColorTarget,
            JudgeColor       = new Color(JudgeColor.r, JudgeColor.g, JudgeColor.b, JudgeColor.a),
            Storyboard       = Storyboard.DeepClone(),
        };
        return clone;
    }
}

[System.Serializable]
public class HitStyle : Storyboardable, IDeepClonable<HitStyle> {

    public string Name;

    public string MainMaterial = "Default";
    public string MainColorTarget = "_Color";
    public Color NormalColor = Color.black;
    public Color CatchColor = Color.blue;

    public string HoldTailMaterial = "Default";
    public string HoldTailColorTarget = "_Color";
    public Color HoldTailColor = Color.black;

    public new static TimestampType[] TimestampTypes = {
        #region Tap Note Color
        new() 
        {
            ID   = "NormalColor_R",
            Name = "Normal Color R",
            Get  = (x) => ((HitStyle)x).NormalColor.r,
            Set  = (x, a) => { ((HitStyle)x).NormalColor.r = a; },
        },
        new()
        {
            ID   = "NormalColor_G",
            Name = "Normal Color G",
            Get  = (x) => ((HitStyle)x).NormalColor.g,
            Set  = (x, a) => { ((HitStyle)x).NormalColor.g = a; },
        },
        new() {
            
            ID   = "NormalColor_B",
            Name = "Normal Color B",
            Get  = (x) => ((HitStyle)x).NormalColor.b,
            Set  = (x, a) => { ((HitStyle)x).NormalColor.b = a; },
        },
        new() 
        {
            ID   = "NormalColor_A",
            Name = "Normal Color A",
            Get  = (x) => ((HitStyle)x).NormalColor.a,
            Set  = (x, a) => { ((HitStyle)x).NormalColor.a = a; },
        },
        #endregion
        
        #region Catch Note Color
        new()
        {
            ID   = "CatchColor_R",
            Name = "Catch Color R",
            Get  = (x) => ((HitStyle)x).CatchColor.r,
            Set  = (x, a) => { ((HitStyle)x).CatchColor.r = a; },
        },
        new() 
        {
            ID   = "CatchColor_G",
            Name = "Catch Color G",
            Get  = (x) => ((HitStyle)x).CatchColor.g,
            Set  = (x, a) => { ((HitStyle)x).CatchColor.g = a; },
        },
        new() 
        {
            ID   = "CatchColor_B",
            Name = "Catch Color B",
            Get  = (x) => ((HitStyle)x).CatchColor.b,
            Set  = (x, a) => { ((HitStyle)x).CatchColor.b = a; },
        },
        new() 
        {
            ID   = "CatchColor_A",
            Name = "Catch Color A",
            Get  = (x) => ((HitStyle)x).CatchColor.a,
            Set  = (x, a) => { ((HitStyle)x).CatchColor.a = a; },
        },
        #endregion
        
        #region Hold Note Color (Tail)
        new() 
        {
            ID   = "HoldTailColor_R",
            Name = "Hold Tail Color R",
            Get  = (x) => ((HitStyle)x).HoldTailColor.r,
            Set  = (x, a) => { ((HitStyle)x).HoldTailColor.r = a; },
        },
        new() 
        {
            ID   = "HoldTailColor_G",
            Name = "Hold Tail Color G",
            Get  = (x) => ((HitStyle)x).HoldTailColor.g,
            Set  = (x, a) => { ((HitStyle)x).HoldTailColor.g = a; },
        },
        new() 
        {
            ID   = "HoldTailColor_B",
            Name = "Hold Tail Color B",
            Get  = (x) => ((HitStyle)x).HoldTailColor.b,
            Set  = (x, a) => { ((HitStyle)x).HoldTailColor.b = a; },
        },
        new() 
        {
            ID   = "HoldTailColor_A",
            Name = "Hold Tail Color A",
            Get  = (x) => ((HitStyle)x).HoldTailColor.a,
            Set  = (x, a) => { ((HitStyle)x).HoldTailColor.a = a; },
        },
        #endregion
    };

    public HitStyle DeepClone()
    {
        HitStyle clone = new()
        {
            MainMaterial        = MainMaterial,
            MainColorTarget     = MainColorTarget,
            NormalColor         = new Color(NormalColor.r, NormalColor.g, NormalColor.b, NormalColor.a),
            CatchColor          = new Color(CatchColor.r, CatchColor.g, CatchColor.b, CatchColor.a),
            HoldTailMaterial    = HoldTailMaterial,
            HoldTailColorTarget = HoldTailColorTarget,
            HoldTailColor       = new Color(HoldTailColor.r, HoldTailColor.g, HoldTailColor.b, HoldTailColor.a),
            Storyboard          = Storyboard.DeepClone(),
        };
        return clone;
    }
}

[System.Serializable]
public class LaneGroup : Storyboardable, IDeepClonable<LaneGroup> 
{
    public string Name;
    public Vector3 Position;
    public Vector3 Rotation;
    public string Group;

    public new static TimestampType[] TimestampTypes = {
        #region Position
        new() {
            ID = "Position_X",
            Name = "Position X",
            Get = (x) => ((LaneGroup)x).Position.x,
            Set = (x, a) => { ((LaneGroup)x).Position.x = a; },
        },
        new() {
            ID = "Position_Y",
            Name = "Position Y",
            Get = (x) => ((LaneGroup)x).Position.y,
            Set = (x, a) => { ((LaneGroup)x).Position.y = a; },
        },
        new() {
            ID = "Position_Z",
            Name = "Position Z",
            Get = (x) => ((LaneGroup)x).Position.z,
            Set = (x, a) => { ((LaneGroup)x).Position.z = a; },
        },
        #endregion
        
        #region Rotation
        new() {
            ID = "Rotation_X",
            Name = "Rotation X",
            Get = (x) => ((LaneGroup)x).Rotation.x,
            Set = (x, a) => { ((LaneGroup)x).Rotation.x = a; },
        },
        new() {
            ID = "Rotation_Y",
            Name = "Rotation Y",
            Get = (x) => ((LaneGroup)x).Rotation.y,
            Set = (x, a) => { ((LaneGroup)x).Rotation.y = a; },
        },
        new() {
            ID = "Rotation_Z",
            Name = "Rotation Z",
            Get = (x) => ((LaneGroup)x).Rotation.z,
            Set = (x, a) => { ((LaneGroup)x).Rotation.z = a; },
        },
        #endregion
    };

    public LaneGroup DeepClone()
    {
        LaneGroup clone = new()
        {
            Name       = Name,
            Position   = new Vector3(Position.x, Position.y, Position.z),
            Rotation   = new Vector3(Rotation.x, Rotation.y, Rotation.z),
            Storyboard = Storyboard.DeepClone(),
            Group      = Group
        };
        return clone;
    }
}

[System.Serializable]
public class LanePosition
{
    [FormerlySerializedAs("StartPos")] public Vector2 StartPosition;
    [FormerlySerializedAs("EndPos")]   public Vector2 EndPosition;
    public float Offset;
}

[System.Serializable]
public class Lane : DirtyTrackedStoryboardable, IDeepClonable<Lane>
{
    public string Name;
    
    public List<HitObject> Objects = new();
    public List<LaneStep> LaneSteps = new();
    [FormerlySerializedAs("Offset")]
    public Vector3 Position;
    [FormerlySerializedAs("OffsetRotation")]
    public Vector3 Rotation;
    public string Group;

    public int StyleIndex = 0;

    public LanePosition GetLanePosition(float time, float laneTime, Metronome timing) 
    {
        float offset = 0;
        float timeT = timing.ToSeconds(time);
        float laneTimeT = timing.ToSeconds(laneTime);
        float curtime = laneTimeT;
        List<LaneStep> steps = new();
        for (int a = 0; a < LaneSteps.Count; a++) 
        {
            LaneStep step = (LaneStep)LaneSteps[a].GetStoryboardableObject(laneTime);
            steps.Add(step);

            float t = timing.ToSeconds(step.Offset);
            offset += step.Speed * (Mathf.Max(t, laneTimeT) - curtime);
            curtime = Mathf.Max(t, laneTimeT);

            if (Mathf.Approximately(time, step.Offset)) return new LanePosition 
            {
                StartPosition = step.StartPos,
                EndPosition   = step.EndPos,
                Offset   = laneTime < time ? offset : float.NaN,
            };
            else if (time < step.Offset) 
            {
                if (a == 0) return new LanePosition 
                {
                    StartPosition = step.StartPos,
                    EndPosition   = step.EndPos,
                    Offset   = laneTime < time ? offset : float.NaN,
                };

                LaneStep previousStep = steps[a - 1];
                float percentageDifference = (time - previousStep.Offset) / (step.Offset - previousStep.Offset);

                if (step.IsLinear)
                    return new LanePosition 
                    {
                        StartPosition = Vector2.LerpUnclamped(previousStep.StartPos, step.StartPos, percentageDifference),
                        EndPosition   = Vector2.LerpUnclamped(previousStep.EndPos, step.EndPos, percentageDifference),
                        Offset   = laneTime < time ? offset + (timeT - t) * step.Speed : BeatPosition.NaN,
                    };
                else 
                    return new LanePosition 
                    {
                        StartPosition = new Vector2(Mathf.LerpUnclamped(previousStep.StartPos.x, step.StartPos.x, step.StartEaseX.Get(percentageDifference)),
                            Mathf.LerpUnclamped(previousStep.StartPos.y, step.StartPos.y, step.StartEaseY.Get(percentageDifference))),
                        EndPosition = new Vector2(Mathf.LerpUnclamped(previousStep.EndPos.x, step.EndPos.x, step.EndEaseX.Get(percentageDifference)),
                            Mathf.LerpUnclamped(previousStep.EndPos.y, step.EndPos.y, step.EndEaseY.Get(percentageDifference))),
                        Offset = laneTime < time ? offset + (timeT - t) * step.Speed : BeatPosition.NaN,
                    };
            }
            
        }
        {
            // Array[^x] == Array[Array.Count - x]
            float t = timing.ToSeconds(steps[^1].Offset);
            return new LanePosition 
            {
                StartPosition = steps[^1].StartPos,
                EndPosition = steps[^1].EndPos,
                Offset = laneTime < time ? offset + (timeT - t) * LaneSteps[^1].Speed : float.NaN,
            };
        }
    }
    
    // More as offset
    public new static TimestampType[] TimestampTypes = 
    {
        #region Position
        new()
        {
            ID   = "Offset_X",
            Name = "Position X",
            Get  = (x) => ((Lane)x).Position.x,
            Set  = (x, a) => { ((Lane)x).Position.x = a; },
        },
        new() 
        {
            ID   = "Offset_Y",
            Name = "Position Y",
            Get  = (x) => ((Lane)x).Position.y,
            Set  = (x, a) => { ((Lane)x).Position.y = a; },
        },
        new() 
        {
            ID   = "Offset_Z",
            Name = "Position Z",
            Get  = (x) => ((Lane)x).Position.z,
            Set  = (x, a) => { ((Lane)x).Position.z = a; },
        },
        #endregion
        
        #region Rotation
        new()
        {
            ID   = "OffsetRotation_X",
            Name = "Rotation X",
            Get  = (x) => ((Lane)x).Rotation.x,
            Set  = (x, a) => { ((Lane)x).Rotation.x = a; },
        },
        new() 
        {
            ID   = "OffsetRotation_Y",
            Name = "Rotation Y",
            Get  = (x) => ((Lane)x).Rotation.y,
            Set  = (x, a) => { ((Lane)x).Rotation.y = a; },
        },
        new() 
        {
            ID   = "OffsetRotation_Z",
            Name = "Rotation Z",
            Get  = (x) => ((Lane)x).Rotation.z,
            Set  = (x, a) => { ((Lane)x).Rotation.z = a; },
        },
        #endregion
    };

    public Lane DeepClone()
    {
        Lane clone = new()
        {
            Position   = new Vector3(Position.x, Position.y, Position.z),
            Rotation   = new Vector3(Rotation.x, Rotation.y, Rotation.z),
            Group      = Group,
            StyleIndex = StyleIndex,
            Storyboard = Storyboard.DeepClone(),
        };
        
        foreach (HitObject obj in Objects)   
            clone.Objects.Add(obj.DeepClone());
        
        foreach (LaneStep step in LaneSteps) 
            clone.LaneSteps.Add(step.DeepClone());
        
        return clone;
    }
}

[System.Serializable]
public class LaneStep : DirtyTrackedStoryboardable, IDeepClonable<LaneStep> 
{
    public BeatPosition Offset = new();
    
    public Vector2 StartPos;
    [SerializeReference] public IEaseDirective StartEaseX = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);
    [SerializeReference] public IEaseDirective StartEaseY = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);
    
    public Vector2 EndPos;
    [SerializeReference] public IEaseDirective EndEaseX   = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);
    [SerializeReference] public IEaseDirective EndEaseY   = new BasicEaseDirective(EaseFunction.Linear, EaseMode.In);
    
    public float Speed = 1;

    public bool IsLinear => 
        StartEaseX is BasicEaseDirective startEaseX && 
        StartEaseY is BasicEaseDirective startEaseY && 
        
        EndEaseX   is BasicEaseDirective endEaseX && 
        EndEaseY   is BasicEaseDirective endEaseY &&
        
        startEaseX.Function == EaseFunction.Linear && 
        startEaseY.Function == EaseFunction.Linear && 
        
        endEaseX.Function   == EaseFunction.Linear && 
        endEaseY.Function   == EaseFunction.Linear;

    public new static TimestampType[] TimestampTypes = 
    {
        #region Start Position
        new() 
        {
            ID   = "StartPos_X",
            Name = "Start Position X",
            Get  = (x) => ((LaneStep)x).StartPos.x,
            Set  = (x, a) => { ((LaneStep)x).StartPos.x = a; },
        },
        new() 
        {
            ID   = "StartPos_Y",
            Name = "Start Position Y",
            Get  = (x) => ((LaneStep)x).StartPos.y,
            Set  = (x, a) => { ((LaneStep)x).StartPos.y = a; },
        },
        #endregion
        
        #region End Position
        new()
        {
            ID   = "EndPos_X",
            Name = "End Position X",
            Get  = (x) => ((LaneStep)x).EndPos.x,
            Set  = (x, a) => { ((LaneStep)x).EndPos.x = a; },
        },
        new()
        {
            ID   = "EndPos_Y",
            Name = "End Position Y",
            Get  = (x) => ((LaneStep)x).EndPos.y,
            Set  = (x, a) => { ((LaneStep)x).EndPos.y = a; },
        },
        #endregion
        
        new() 
        {
            ID   = "Speed",
            Name = "Speed",
            Get  = (x) => ((LaneStep)x).Speed,
            Set  = (x, a) => { ((LaneStep)x).Speed = a; },
        },
    };
    
    public LaneStep DeepClone()
    {
        LaneStep clone = new()
        {
            Offset     = Offset,
            StartPos   = new Vector2(StartPos.x, StartPos.y),
            StartEaseX = StartEaseX,
            StartEaseY = StartEaseY,
            EndPos     = new Vector2(EndPos.x, EndPos.y),
            EndEaseX   = EndEaseX,
            EndEaseY   = EndEaseY,
            Speed      = Speed,
            Storyboard = Storyboard.DeepClone(),
        };
        return clone;
    }
}

[System.Serializable]
public class HitObject : DirtyTrackedStoryboardable, IDeepClonable<HitObject>
{
    public HitType Type;
    public BeatPosition Offset = new();
    public float Position;
    public float Length;
    public float HoldLength = 0;
    public bool Flickable;
    public float FlickDirection = -1;

    public int StyleIndex = 0;
    
    public enum HitType
    {
        Normal,
        Catch,
    }

    public new static TimestampType[] TimestampTypes = 
    {
        new() {
            ID   = "Position",
            Name = "Position",
            Get  = (x) => ((HitObject)x).Position,
            Set  = (x, a) => { ((HitObject)x).Position = a; },
        },
        new() {
            ID   = "Length",
            Name = "Length",
            Get  = (x) => ((HitObject)x).Length,
            Set  = (x, a) => { ((HitObject)x).Length = a; },
        },
    };
    
    public HitObject DeepClone()
    {
        HitObject clone = new()
        {
            Type           = Type,
            Offset         = Offset,
            Position       = Position,
            Length         = Length,
            HoldLength     = HoldLength,
            Flickable      = Flickable,
            FlickDirection = FlickDirection,
            StyleIndex     = StyleIndex,
            Storyboard     = Storyboard.DeepClone(),
        };
        return clone;
    }
}

public enum CoordinateMode 
{
    Local,
    Group,
    Global,
}
