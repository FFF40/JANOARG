using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chart : IStoryboardable
{
    public string DifficultyName = "Normal";
    public string DifficultyLevel = "6";
    public int DifficultyIndex = 1;
    public int ChartConstant = 6;

    public List<LaneGroup> Groups = new List<LaneGroup>();
    public List<Lane> Lanes = new List<Lane>();

    public Color BackgroundColor = Color.black;
    public Color InterfaceColor = Color.white;
    public Material LaneMaterial;
    public Material HitMaterial;
    public Material HoldMaterial;

    public Vector3 CameraPivot;
    public Vector3 CameraRotation;

    public Pallete Pallete = new Pallete();

    public new static TimestampType[] TimestampTypes = 
    {
        new TimestampType
        {
            ID = "CameraPivot_X",
            Name = "Camera Pivot X",
            Get = (x) => ((Chart)x).CameraPivot.x,
            Set = (x, a) => { ((Chart)x).CameraPivot.x = a; },
        },
        new TimestampType
        {
            ID = "CameraPivot_Y",
            Name = "Camera Pivot Y",
            Get = (x) => ((Chart)x).CameraPivot.y,
            Set = (x, a) => { ((Chart)x).CameraPivot.y = a; },
        },
        new TimestampType
        {
            ID = "CameraPivot_Z",
            Name = "Camera Pivot Z",
            Get = (x) => ((Chart)x).CameraPivot.z,
            Set = (x, a) => { ((Chart)x).CameraPivot.z = a; },
        },
        new TimestampType
        {
            ID = "CameraRotation_X",
            Name = "Camera Rotation X",
            Get = (x) => ((Chart)x).CameraRotation.x,
            Set = (x, a) => { ((Chart)x).CameraRotation.x = a; },
        },
        new TimestampType
        {
            ID = "CameraRotation_Y",
            Name = "Camera Rotation Y",
            Get = (x) => ((Chart)x).CameraRotation.y,
            Set = (x, a) => { ((Chart)x).CameraRotation.y = a; },
        },
        new TimestampType
        {
            ID = "CameraRotation_Z",
            Name = "Camera Rotation Z",
            Get = (x) => ((Chart)x).CameraRotation.z,
            Set = (x, a) => { ((Chart)x).CameraRotation.z = a; },
        },
    };


    public Chart() 
    {
        
    }
}

// Style 
[System.Serializable]
public class Pallete : IStoryboardable  {

    public Color BackgroundColor = Color.black;
    public Color InterfaceColor = Color.white;

    public List<LaneStyle> LaneStyles = new List<LaneStyle>();
    public List<HitStyle> HitStyles = new List<HitStyle>();

    public new static TimestampType[] TimestampTypes = 
    {
        new TimestampType
        {
            ID = "BackgroundColor_R",
            Name = "Background Color R",
            Get = (x) => ((Pallete)x).BackgroundColor.r,
            Set = (x, a) => { ((Pallete)x).BackgroundColor.r = a; },
        },
        new TimestampType
        {
            ID = "BackgroundColor_G",
            Name = "Background Color G",
            Get = (x) => ((Pallete)x).BackgroundColor.g,
            Set = (x, a) => { ((Pallete)x).BackgroundColor.g = a; },
        },
        new TimestampType
        {
            ID = "BackgroundColor_B",
            Name = "Background Color B",
            Get = (x) => ((Pallete)x).BackgroundColor.b,
            Set = (x, a) => { ((Pallete)x).BackgroundColor.b = a; },
        },
        new TimestampType
        {
            ID = "InterfaceColor_R",
            Name = "Interface Color R",
            Get = (x) => ((Pallete)x).InterfaceColor.r,
            Set = (x, a) => { ((Pallete)x).InterfaceColor.r = a; },
        },
        new TimestampType
        {
            ID = "InterfaceColor_G",
            Name = "Interface Color G",
            Get = (x) => ((Pallete)x).InterfaceColor.g,
            Set = (x, a) => { ((Pallete)x).InterfaceColor.g = a; },
        },
        new TimestampType
        {
            ID = "InterfaceColor_B",
            Name = "Interface Color B",
            Get = (x) => ((Pallete)x).InterfaceColor.b,
            Set = (x, a) => { ((Pallete)x).InterfaceColor.b = a; },
        },
        new TimestampType
        {
            ID = "InterfaceColor_A",
            Name = "Interface Color A",
            Get = (x) => ((Pallete)x).InterfaceColor.a,
            Set = (x, a) => { ((Pallete)x).InterfaceColor.a = a; },
        },
    };

    public Pallete() 
    {
        LaneStyles.Add(new LaneStyle());
        HitStyles.Add(new HitStyle());
    }
}

[System.Serializable]
public class LaneStyle : IStoryboardable {

    public Material LaneMaterial;
    public string LaneColorTarget = "_Color";
    public Color LaneColor = Color.black;

    public Material JudgeMaterial;
    public string JudgeColorTarget = "_Color";
    public Color JudgeColor = Color.black;

    public new static TimestampType[] TimestampTypes = 
    {
        new TimestampType
        {
            ID = "LaneColor_R",
            Name = "Lane Color R",
            Get = (x) => ((LaneStyle)x).LaneColor.r,
            Set = (x, a) => { ((LaneStyle)x).LaneColor.r = a; },
        },
        new TimestampType
        {
            ID = "LaneColor_G",
            Name = "Lane Color G",
            Get = (x) => ((LaneStyle)x).LaneColor.g,
            Set = (x, a) => { ((LaneStyle)x).LaneColor.g = a; },
        },
        new TimestampType
        {
            ID = "LaneColor_B",
            Name = "Lane Color B",
            Get = (x) => ((LaneStyle)x).LaneColor.b,
            Set = (x, a) => { ((LaneStyle)x).LaneColor.b = a; },
        },
        new TimestampType
        {
            ID = "LaneColor_A",
            Name = "Lane Color A",
            Get = (x) => ((LaneStyle)x).LaneColor.a,
            Set = (x, a) => { ((LaneStyle)x).LaneColor.a = a; },
        },
        new TimestampType
        {
            ID = "JudgeColor_R",
            Name = "Judge Color R",
            Get = (x) => ((LaneStyle)x).JudgeColor.r,
            Set = (x, a) => { ((LaneStyle)x).JudgeColor.r = a; },
        },
        new TimestampType
        {
            ID = "JudgeColor_G",
            Name = "Judge Color G",
            Get = (x) => ((LaneStyle)x).JudgeColor.g,
            Set = (x, a) => { ((LaneStyle)x).JudgeColor.g = a; },
        },
        new TimestampType
        {
            ID = "JudgeColor_B",
            Name = "Judge Color B",
            Get = (x) => ((LaneStyle)x).JudgeColor.b,
            Set = (x, a) => { ((LaneStyle)x).JudgeColor.b = a; },
        },
        new TimestampType
        {
            ID = "JudgeColor_A",
            Name = "Judge Color A",
            Get = (x) => ((LaneStyle)x).JudgeColor.a,
            Set = (x, a) => { ((LaneStyle)x).JudgeColor.a = a; },
        },
    };

    public LaneStyle() 
    {
        LaneMaterial = (Material)Resources.Load("Materials/Default Lane");
        JudgeMaterial = (Material)Resources.Load("Materials/Default Judge");
    }
}

public class LaneStyleManager {
    public Material BaseLaneMaterial;
    public Material LaneMaterial;

    public Material BaseJudgeMaterial;
    public Material JudgeMaterial;

    public LaneStyleManager(LaneStyle style) 
    {
        
    }

    public void Update(LaneStyle style) 
    {
        if (style.LaneMaterial && BaseLaneMaterial != style.LaneMaterial) 
        {
            LaneMaterial = new Material(BaseLaneMaterial = style.LaneMaterial);
        }
        if (style.JudgeMaterial && BaseJudgeMaterial != style.JudgeMaterial) 
        {
            JudgeMaterial = new Material(BaseJudgeMaterial = style.JudgeMaterial);
        }

        if (LaneMaterial) LaneMaterial.SetColor(style.LaneColorTarget, style.LaneColor);
        if (JudgeMaterial) JudgeMaterial.SetColor(style.JudgeColorTarget, style.JudgeColor);
    }

    public void Dispose() 
    {
        GameObject.DestroyImmediate(LaneMaterial);
        GameObject.DestroyImmediate(JudgeMaterial);
    }
}

[System.Serializable]
public class HitStyle : IStoryboardable {

    public Material MainMaterial;
    public string MainColorTarget = "_Color";
    public Color NormalColor = Color.black;
    public Color CatchColor = Color.blue;

    public Material HoldTailMaterial;
    public string HoldTailColorTarget = "_Color";
    public Color HoldTailColor = Color.black;

    public new static TimestampType[] TimestampTypes = {
        new TimestampType
        {
            ID = "NormalColor_R",
            Name = "Normal Color R",
            Get = (x) => ((HitStyle)x).NormalColor.r,
            Set = (x, a) => { ((HitStyle)x).NormalColor.r = a; },
        },
        new TimestampType
        {
            ID = "NormalColor_G",
            Name = "Normal Color G",
            Get = (x) => ((HitStyle)x).NormalColor.g,
            Set = (x, a) => { ((HitStyle)x).NormalColor.g = a; },
        },
        new TimestampType
        {
            ID = "NormalColor_B",
            Name = "Normal Color B",
            Get = (x) => ((HitStyle)x).NormalColor.b,
            Set = (x, a) => { ((HitStyle)x).NormalColor.b = a; },
        },
        new TimestampType
        {
            ID = "NormalColor_A",
            Name = "Normal Color A",
            Get = (x) => ((HitStyle)x).NormalColor.a,
            Set = (x, a) => { ((HitStyle)x).NormalColor.a = a; },
        },
        new TimestampType
        {
            ID = "CatchColor_R",
            Name = "Catch Color R",
            Get = (x) => ((HitStyle)x).CatchColor.r,
            Set = (x, a) => { ((HitStyle)x).CatchColor.r = a; },
        },
        new TimestampType
        {
            ID = "CatchColor_G",
            Name = "Catch Color G",
            Get = (x) => ((HitStyle)x).CatchColor.g,
            Set = (x, a) => { ((HitStyle)x).CatchColor.g = a; },
        },
        new TimestampType
        {
            ID = "CatchColor_B",
            Name = "Catch Color B",
            Get = (x) => ((HitStyle)x).CatchColor.b,
            Set = (x, a) => { ((HitStyle)x).CatchColor.b = a; },
        },
        new TimestampType
        {
            ID = "CatchColor_A",
            Name = "Catch Color A",
            Get = (x) => ((HitStyle)x).CatchColor.a,
            Set = (x, a) => { ((HitStyle)x).CatchColor.a = a; },
        },
        new TimestampType
        {
            ID = "HoldTailColor_R",
            Name = "Hold Tail Color R",
            Get = (x) => ((HitStyle)x).HoldTailColor.r,
            Set = (x, a) => { ((HitStyle)x).HoldTailColor.r = a; },
        },
        new TimestampType
        {
            ID = "HoldTailColor_G",
            Name = "Hold Tail Color G",
            Get = (x) => ((HitStyle)x).HoldTailColor.g,
            Set = (x, a) => { ((HitStyle)x).HoldTailColor.g = a; },
        },
        new TimestampType
        {
            ID = "HoldTailColor_B",
            Name = "Hold Tail Color B",
            Get = (x) => ((HitStyle)x).HoldTailColor.b,
            Set = (x, a) => { ((HitStyle)x).HoldTailColor.b = a; },
        },
        new TimestampType
        {
            ID = "HoldTailColor_A",
            Name = "Hold Tail Color A",
            Get = (x) => ((HitStyle)x).HoldTailColor.a,
            Set = (x, a) => { ((HitStyle)x).HoldTailColor.a = a; },
        },
    };

    public HitStyle() 
    {
        MainMaterial = (Material)Resources.Load("Materials/Default Hit");
        HoldTailMaterial = (Material)Resources.Load("Materials/Default Hold");
    }
}

public class HitStyleManager {
    public Material BaseMainMaterial;
    public Material NormalMaterial;
    public Material CatchMaterial;

    public Material BaseHoldTailMaterial;
    public Material HoldTailMaterial;

    public HitStyleManager(HitStyle style) 
    {
        Update(style);
    }

    public void Update(HitStyle style) 
    {
        if (style.MainMaterial && BaseMainMaterial != style.MainMaterial) 
        {
            NormalMaterial = new Material(BaseMainMaterial = style.MainMaterial);
            CatchMaterial = new Material(BaseMainMaterial);
        }
        if (style.HoldTailMaterial && BaseHoldTailMaterial != style.HoldTailMaterial) 
        {
            HoldTailMaterial = new Material(BaseHoldTailMaterial = style.HoldTailMaterial);
        }

        if (NormalMaterial) NormalMaterial.SetColor(style.MainColorTarget, style.NormalColor);
        if (CatchMaterial) CatchMaterial.SetColor(style.MainColorTarget, style.CatchColor);
        if (HoldTailMaterial) HoldTailMaterial.SetColor(style.HoldTailColorTarget, style.HoldTailColor);
    }

    public void Dispose() 
    {
        GameObject.DestroyImmediate(NormalMaterial);
        GameObject.DestroyImmediate(CatchMaterial);
        GameObject.DestroyImmediate(HoldTailMaterial);
    }
}

// Game

[System.Serializable]
public class LaneGroup : IStoryboardable 
{
    public string Name;
    public Vector3 Position;
    public float Rotation;

    public new static TimestampType[] TimestampTypes = {
        new TimestampType
        {
            ID = "Position_X",
            Name = "Position X",
            Get = (x) => ((LaneGroup)x).Position.x,
            Set = (x, a) => { ((LaneGroup)x).Position.x = a; },
        },
        new TimestampType
        {
            ID = "Position_Y",
            Name = "Position Y",
            Get = (x) => ((LaneGroup)x).Position.y,
            Set = (x, a) => { ((LaneGroup)x).Position.y = a; },
        },
        new TimestampType
        {
            ID = "Position_Z",
            Name = "Position Z",
            Get = (x) => ((LaneGroup)x).Position.z,
            Set = (x, a) => { ((LaneGroup)x).Position.z = a; },
        },
        new TimestampType
        {
            ID = "Rotation",
            Name = "Rotation",
            Get = (x) => ((LaneGroup)x).Rotation,
            Set = (x, a) => { ((LaneGroup)x).Rotation = a; },
        },
    };
}

[System.Serializable]
public class Lane : IStoryboardable 
{
    public List<HitObject> Objects = new List<HitObject>();
    public List<LaneStep> LaneSteps = new List<LaneStep>();
    public bool ExpandToInfinity = true;
    public Vector3 Offset;
    public Vector3 OffsetRotation;
    public string Group;

    public int StyleIndex = 0;

    public LaneStep GetLaneStep(float time, float laneTime, Metronome timing) 
    {
        float offset = 0;
        float timeT = timing.ToSeconds(time);
        float laneTimeT = timing.ToSeconds(laneTime);
        float curtime = laneTimeT;
        List<LaneStep> steps = new List<LaneStep>();
        for (int a = 0; a < LaneSteps.Count; a++) 
        {
            LaneStep step = (LaneStep)LaneSteps[a].Get(laneTime);
            steps.Add(step);

            float t = timing.ToSeconds(step.Offset);
            offset += step.Speed * (Mathf.Max(t, laneTimeT) - curtime);
            curtime = Mathf.Max(t, laneTimeT);

            if (step.Offset == time) return new LaneStep 
            {
                StartPos = step.StartPos,
                EndPos = step.EndPos,
                Offset = laneTime < time ? offset : float.NaN,
            };
            else if (step.Offset > time) 
            {
                if (a == 0) return new LaneStep 
                {
                    StartPos = step.StartPos,
                    EndPos = step.EndPos,
                    Offset = laneTime < time ? offset : float.NaN,
                };

                LaneStep prev = steps[a - 1];
                float p = (time - prev.Offset) / (step.Offset - prev.Offset);

                if (step.StartEaseX == "Linear" && step.StartEaseY == "Linear" &&
                    step.EndEaseX == "Linear" && step.EndEaseY == "Linear")
                {
                    return new LaneStep 
                    {
                        StartPos = Vector2.Lerp(prev.StartPos, step.StartPos, p),
                        EndPos = Vector2.Lerp(prev.EndPos, step.EndPos, p),
                        Offset = laneTime < time ? offset + (timeT - t) * step.Speed : float.NaN,
                    };
                }
                else 
                {
                    
                    return new LaneStep 
                    {
                        StartPos = new Vector2(Mathf.Lerp(prev.StartPos.x, step.StartPos.x, Ease.Get(p, step.StartEaseX, step.StartEaseXMode)),
                            Mathf.Lerp(prev.StartPos.y, step.StartPos.y, Ease.Get(p, step.StartEaseY, step.StartEaseYMode))),
                        EndPos = new Vector2(Mathf.Lerp(prev.EndPos.x, step.EndPos.x, Ease.Get(p, step.EndEaseX, step.EndEaseXMode)),
                            Mathf.Lerp(prev.EndPos.y, step.EndPos.y, Ease.Get(p, step.EndEaseY, step.EndEaseYMode))),
                        Offset = laneTime < time ? offset + (timeT - t) * step.Speed : float.NaN,
                    };
                }
            }
            
        }
        {
            float t = timing.ToSeconds(steps[steps.Count - 1].Offset);
            return new LaneStep 
            {
                StartPos = steps[steps.Count - 1].StartPos,
                EndPos = steps[steps.Count - 1].EndPos,
                Offset = laneTime < time ? offset + (timeT - t) * LaneSteps[LaneSteps.Count - 1].Speed : float.NaN,
            };
        }
    }

    public new static TimestampType[] TimestampTypes = 
    {
        new TimestampType
        {
            ID = "Offset_X",
            Name = "Offset X",
            Get = (x) => ((Lane)x).Offset.x,
            Set = (x, a) => { ((Lane)x).Offset.x = a; },
        },
        new TimestampType
        {
            ID = "Offset_Y",
            Name = "Offset Y",
            Get = (x) => ((Lane)x).Offset.y,
            Set = (x, a) => { ((Lane)x).Offset.y = a; },
        },
        new TimestampType
        {
            ID = "Offset_Z",
            Name = "Offset Z",
            Get = (x) => ((Lane)x).Offset.z,
            Set = (x, a) => { ((Lane)x).Offset.z = a; },
        },
    };
}

[System.Serializable]
public class LaneStep : IStoryboardable 
{
    public float Offset;
    public Vector2 StartPos;
    public string StartEaseX = "Linear";
    public EaseMode StartEaseXMode;
    public string StartEaseY = "Linear";
    public EaseMode StartEaseYMode;
    public Vector2 EndPos;
    public string EndEaseX = "Linear";
    public EaseMode EndEaseXMode;
    public string EndEaseY = "Linear";
    public EaseMode EndEaseYMode;
    public float Speed = 1;

    public new static TimestampType[] TimestampTypes = 
    {
        new TimestampType
        {
            ID = "StartPos_X",
            Name = "Start Position X",
            Get = (x) => ((LaneStep)x).StartPos.x,
            Set = (x, a) => { ((LaneStep)x).StartPos.x = a; },
        },
        new TimestampType
        {
            ID = "StartPos_Y",
            Name = "Start Position Y",
            Get = (x) => ((LaneStep)x).StartPos.y,
            Set = (x, a) => { ((LaneStep)x).StartPos.y = a; },
        },
        new TimestampType
        {
            ID = "EndPos_X",
            Name = "End Position X",
            Get = (x) => ((LaneStep)x).EndPos.x,
            Set = (x, a) => { ((LaneStep)x).EndPos.x = a; },
        },
        new TimestampType
        {
            ID = "EndPos_Y",
            Name = "End Position Y",
            Get = (x) => ((LaneStep)x).EndPos.y,
            Set = (x, a) => { ((LaneStep)x).EndPos.y = a; },
        },
        new TimestampType
        {
            ID = "Speed",
            Name = "Speed",
            Get = (x) => ((LaneStep)x).Speed,
            Set = (x, a) => { ((LaneStep)x).Speed = a; },
        },
    };
}

[System.Serializable]
public class HitObject : IStoryboardable 
{
    public HitType Type;
    public float Offset = 0;
    public float Position;
    public float Length;
    public float HoldLength = 0;

    public int StyleIndex = 0;
    
    public enum HitType
    {
        Normal,
        Catch,
    }

    public List<RailTimestamp> Rail = new List<RailTimestamp>();

    public new static TimestampType[] TimestampTypes = 
    {
        new TimestampType
        {
            ID = "Position",
            Name = "Position",
            Get = (x) => ((HitObject)x).Position,
            Set = (x, a) => { ((HitObject)x).Position = a; },
        },
        new TimestampType
        {
            ID = "Length",
            Name = "Length",
            Get = (x) => ((HitObject)x).Length,
            Set = (x, a) => { ((HitObject)x).Length = a; },
        },
    };
}

[System.Serializable]
public class RailTimestamp : IStoryboardable 
{
    public int Offset = 0;
    public float Position;
    public Vector3 Velocity;
    

    public new static TimestampType[] TimestampTypes = 
    {
        new TimestampType
        {
            ID = "Position",
            Name = "Position",
            Get = (x) => ((RailTimestamp)x).Position,
            Set = (x, a) => { ((RailTimestamp)x).Position = a; },
        },
        new TimestampType
        {
            ID = "Velocity_X",
            Name = "Velocity X",
            Get = (x) => ((RailTimestamp)x).Velocity.x,
            Set = (x, a) => { ((RailTimestamp)x).Velocity.x = a; },
        },
        new TimestampType
        {
            ID = "Velocity_Y",
            Name = "Velocity Y",
            Get = (x) => ((RailTimestamp)x).Velocity.y,
            Set = (x, a) => { ((RailTimestamp)x).Velocity.y = a; },
        },
        new TimestampType
        {
            ID = "Velocity_Z",
            Name = "Velocity Z",
            Get = (x) => ((RailTimestamp)x).Velocity.z,
            Set = (x, a) => { ((RailTimestamp)x).Velocity.z = a; },
        },
    };
}

public enum CoordinateMode 
{
    Local,
    Group,
    Global,
}
