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
public class Chart : IStoryboardable, IDeepClonable<Chart>
{
    public string DifficultyName = "Normal";
    public string DifficultyLevel = "6";
    public int DifficultyIndex = 1;
    public float ChartConstant = 6;

    public List<LaneGroup> Groups = new List<LaneGroup>();
    public List<Lane> Lanes = new List<Lane>();

    public CameraController Camera = new CameraController();
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
    
    public Chart DeepClone()
    {
        Chart clone = new Chart()
        {
            DifficultyName = DifficultyName,
            DifficultyLevel = DifficultyLevel,
            DifficultyIndex = DifficultyIndex,
            ChartConstant = ChartConstant,
            Camera = Camera.DeepClone(),
            Pallete = Pallete.DeepClone(),
            Storyboard = Storyboard.DeepClone(),
            CameraPivot = new Vector3(CameraPivot.x, CameraPivot.y, CameraPivot.z),
            CameraRotation = new Vector3(CameraRotation.x, CameraRotation.y, CameraRotation.z),
        };
        foreach (LaneGroup group in Groups) clone.Groups.Add(group.DeepClone());
        foreach (Lane lane in Lanes) clone.Lanes.Add(lane.DeepClone());
        return clone;
    }
}

public class ChartManager 
{
    public PlayableSong Song;
    public Chart CurrentChart;
    public Dictionary<string, LaneGroupManager> Groups = new Dictionary<string, LaneGroupManager>();
    public List<LaneManager> Lanes = new List<LaneManager>();
    public HitMeshManager HitMeshManager = new HitMeshManager();
    public float CurrentSpeed;
    public float CurrentTime;

    public ChartManager (PlayableSong song, Chart chart, float speed, float time, float pos)
    {
        Song = song;
        CurrentChart = chart;
        CurrentSpeed = speed;
        Update(time, pos);
    }

    public void Update(float time, float pos)
    {
        for (int a = 0; a < CurrentChart.Groups.Count; a++)
        {
            LaneGroup group = (LaneGroup)CurrentChart.Groups[a].Get(pos);
            if (Groups.ContainsKey(group.Name)) Groups[group.Name].Update(group, pos, this);
            else Groups.Add(group.Name, new LaneGroupManager(group, pos, this));
        }
        foreach (KeyValuePair<string, LaneGroupManager> pair in new Dictionary<string, LaneGroupManager>(Groups))
        {
            if (pair.Value.isDirty) pair.Value.UpdatePosition(this);
            else Groups.Remove(pair.Key);
        }

        for (int a = 0; a < CurrentChart.Lanes.Count; a++)
        {
            Lane lane = (Lane)CurrentChart.Lanes[a].Get(pos);
            if (Lanes.Count <= a) Lanes.Add(new LaneManager(lane, time, pos, this));
            else Lanes[a].Update(lane, time, pos, this);
        }
        while (Lanes.Count > CurrentChart.Lanes.Count)
        {
            Lanes[CurrentChart.Lanes.Count].Dispose();
            Lanes.RemoveAt(CurrentChart.Lanes.Count);
        }
        HitMeshManager.Cleanup();
    }
    
    public void Dispose() 
    {
        foreach (LaneManager lane in Lanes) lane.Dispose();
        HitMeshManager.Dispose();
    }
}

[System.Serializable]
public class CameraController : IStoryboardable, IDeepClonable<CameraController> {
    public Vector3 CameraPivot;
    public float PivotDistance = 10;
    public Vector3 CameraRotation;
    
    public new static TimestampType[] TimestampTypes = 
    {
        new TimestampType
        {
            ID = "CameraPivot_X",
            Name = "Camera Pivot X",
            Get = (x) => ((CameraController)x).CameraPivot.x,
            Set = (x, a) => { ((CameraController)x).CameraPivot.x = a; },
        },
        new TimestampType
        {
            ID = "CameraPivot_Y",
            Name = "Camera Pivot Y",
            Get = (x) => ((CameraController)x).CameraPivot.y,
            Set = (x, a) => { ((CameraController)x).CameraPivot.y = a; },
        },
        new TimestampType
        {
            ID = "CameraPivot_Z",
            Name = "Camera Pivot Z",
            Get = (x) => ((CameraController)x).CameraPivot.z,
            Set = (x, a) => { ((CameraController)x).CameraPivot.z = a; },
        },
        new TimestampType
        {
            ID = "PivotDistance",
            Name = "Pivot Distance",
            Get = (x) => ((CameraController)x).PivotDistance,
            Set = (x, a) => { ((CameraController)x).PivotDistance = a; },
        },
        new TimestampType
        {
            ID = "CameraRotation_X",
            Name = "Camera Rotation X",
            Get = (x) => ((CameraController)x).CameraRotation.x,
            Set = (x, a) => { ((CameraController)x).CameraRotation.x = a; },
        },
        new TimestampType
        {
            ID = "CameraRotation_Y",
            Name = "Camera Rotation Y",
            Get = (x) => ((CameraController)x).CameraRotation.y,
            Set = (x, a) => { ((CameraController)x).CameraRotation.y = a; },
        },
        new TimestampType
        {
            ID = "CameraRotation_Z",
            Name = "Camera Rotation Z",
            Get = (x) => ((CameraController)x).CameraRotation.z,
            Set = (x, a) => { ((CameraController)x).CameraRotation.z = a; },
        },
    };
    
    public CameraController DeepClone()
    {
        CameraController clone = new CameraController()
        {
            Storyboard = Storyboard.DeepClone(),
            CameraPivot = new Vector3(CameraPivot.x, CameraPivot.y, CameraPivot.z),
            CameraRotation = new Vector3(CameraRotation.x, CameraRotation.y, CameraRotation.z),
        };
        return clone;
    }
}

// Style 
[System.Serializable]
public class Pallete : IStoryboardable, IDeepClonable<Pallete>  {

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

    public Pallete DeepClone()
    {
        Pallete clone = new Pallete()
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
public class LaneStyle : IStoryboardable, IDeepClonable<LaneStyle> {

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
        try 
        {
            LaneMaterial = (Material)Resources.Load("Materials/Lane/Default");
            JudgeMaterial = (Material)Resources.Load("Materials/Judge/Default");
        }
        catch (UnityException)
        {
            
        }
    }

    public LaneStyle DeepClone()
    {
        LaneStyle clone = new LaneStyle()
        {
            LaneMaterial = LaneMaterial,
            LaneColorTarget = LaneColorTarget,
            LaneColor = new Color(LaneColor.r, LaneColor.g, LaneColor.b, LaneColor.a),
            JudgeMaterial = JudgeMaterial,
            JudgeColorTarget = JudgeColorTarget,
            JudgeColor = new Color(JudgeColor.r, JudgeColor.g, JudgeColor.b, JudgeColor.a),
            Storyboard = Storyboard.DeepClone(),
        };
        return clone;
    }
}

public class LaneStyleManager {
    public Material BaseLaneMaterial;
    public Material LaneMaterial;

    public Material BaseJudgeMaterial;
    public Material JudgeMaterial;

    public LaneStyleManager(LaneStyle style) 
    {
        Update(style);
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
        MonoBehaviour.DestroyImmediate(LaneMaterial);
        MonoBehaviour.DestroyImmediate(JudgeMaterial);
    }
}

[System.Serializable]
public class HitStyle : IStoryboardable, IDeepClonable<HitStyle> {

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
        try 
        {
            MainMaterial = (Material)Resources.Load("Materials/Hit/Default");
            HoldTailMaterial = (Material)Resources.Load("Materials/Hold/Default");
        }
        catch (UnityException)
        {

        }
    }

    public HitStyle DeepClone()
    {
        HitStyle clone = new HitStyle()
        {
            MainMaterial = MainMaterial,
            MainColorTarget = MainColorTarget,
            NormalColor = new Color(NormalColor.r, NormalColor.g, NormalColor.b, NormalColor.a),
            CatchColor = new Color(CatchColor.r, CatchColor.g, CatchColor.b, CatchColor.a),
            HoldTailMaterial = HoldTailMaterial,
            HoldTailColorTarget = HoldTailColorTarget,
            HoldTailColor = new Color(HoldTailColor.r, HoldTailColor.g, HoldTailColor.b, HoldTailColor.a),
            Storyboard = Storyboard.DeepClone(),
        };
        return clone;
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
        MonoBehaviour.DestroyImmediate(NormalMaterial);
        MonoBehaviour.DestroyImmediate(CatchMaterial);
        MonoBehaviour.DestroyImmediate(HoldTailMaterial);
    }
}

[System.Serializable]
public class LaneGroup : IStoryboardable, IDeepClonable<LaneGroup> 
{
    public string Name;
    public Vector3 Position;
    public Vector3 Rotation;
    public string Group;

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
            ID = "Rotation_X",
            Name = "Rotation X",
            Get = (x) => ((LaneGroup)x).Rotation.x,
            Set = (x, a) => { ((LaneGroup)x).Rotation.x = a; },
        },
        new TimestampType
        {
            ID = "Rotation_Y",
            Name = "Rotation Y",
            Get = (x) => ((LaneGroup)x).Rotation.y,
            Set = (x, a) => { ((LaneGroup)x).Rotation.y = a; },
        },
        new TimestampType
        {
            ID = "Rotation_Z",
            Name = "Rotation Z",
            Get = (x) => ((LaneGroup)x).Rotation.z,
            Set = (x, a) => { ((LaneGroup)x).Rotation.z = a; },
        },
    };

    public LaneGroup DeepClone()
    {
        LaneGroup clone = new LaneGroup()
        {
            Name = Name,
            Position = new Vector3(Position.x, Position.y, Position.z),
            Rotation = new Vector3(Rotation.x, Rotation.y, Rotation.z),
            Storyboard = Storyboard.DeepClone(),
        };
        return clone;
    }
}

public class LaneGroupManager
{
    public LaneGroup CurrentGroup;
    public Vector3 FinalPosition;
    public Quaternion FinalRotation;
    public bool isDirty;

    public LaneGroupManager(LaneGroup init, float pos, ChartManager main)
    {
        Update(init, pos, main);
    }

    public void Update(LaneGroup data, float pos, ChartManager main)
    {
        CurrentGroup = data;
        isDirty = true;
    }

    public void Get(ref Vector3 pos, ref Quaternion rot)
    {
        pos = FinalRotation * pos + FinalPosition;
        rot = FinalRotation * rot;
    }

    public void UpdatePosition(ChartManager main, string original = null)
    {
        FinalPosition = CurrentGroup.Position;
        FinalRotation = Quaternion.Euler(CurrentGroup.Rotation);
        if (original == null) original = CurrentGroup.Group;
        if (!string.IsNullOrEmpty(CurrentGroup.Group) && main.Groups.ContainsKey(CurrentGroup.Group))
        {
            LaneGroupManager group = main.Groups[CurrentGroup.Group];
            if (group.CurrentGroup.Name == CurrentGroup.Group)
            {
                Debug.LogError("Infinite group reference loop detected: " + original);
            }
            else 
            {
                if (group.isDirty) group.UpdatePosition(main, original);
                FinalPosition = group.FinalRotation * FinalPosition + group.FinalPosition;
                FinalRotation = group.FinalRotation * FinalRotation;
            }
        }
        isDirty = false;
    }
}

[System.Serializable]
public class Lane : IStoryboardable, IDeepClonable<Lane>
{
    public List<HitObject> Objects = new List<HitObject>();
    public List<LaneStep> LaneSteps = new List<LaneStep>();
    [FormerlySerializedAs("Offset")]
    public Vector3 Position;
    [FormerlySerializedAs("OffsetRotation")]
    public Vector3 Rotation;
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

            if (time == step.Offset) return new LaneStep 
            {
                StartPos = step.StartPos,
                EndPos = step.EndPos,
                Offset = laneTime < time ? offset : float.NaN,
            };
            else if (time < step.Offset) 
            {
                if (a == 0) return new LaneStep 
                {
                    StartPos = step.StartPos,
                    EndPos = step.EndPos,
                    Offset = laneTime < time ? offset : float.NaN,
                };

                LaneStep prev = steps[a - 1];
                float p = (time - prev.Offset) / (step.Offset - prev.Offset);

                if (step.IsLinear)
                {
                    return new LaneStep 
                    {
                        StartPos = Vector2.LerpUnclamped(prev.StartPos, step.StartPos, p),
                        EndPos = Vector2.LerpUnclamped(prev.EndPos, step.EndPos, p),
                        Offset = laneTime < time ? offset + (timeT - t) * step.Speed : float.NaN,
                    };
                }
                else 
                {
                    
                    return new LaneStep 
                    {
                        StartPos = new Vector2(Mathf.LerpUnclamped(prev.StartPos.x, step.StartPos.x, Ease.Get(p, step.StartEaseX, step.StartEaseXMode)),
                            Mathf.LerpUnclamped(prev.StartPos.y, step.StartPos.y, Ease.Get(p, step.StartEaseY, step.StartEaseYMode))),
                        EndPos = new Vector2(Mathf.LerpUnclamped(prev.EndPos.x, step.EndPos.x, Ease.Get(p, step.EndEaseX, step.EndEaseXMode)),
                            Mathf.LerpUnclamped(prev.EndPos.y, step.EndPos.y, Ease.Get(p, step.EndEaseY, step.EndEaseYMode))),
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
            Name = "Position X",
            Get = (x) => ((Lane)x).Position.x,
            Set = (x, a) => { ((Lane)x).Position.x = a; },
        },
        new TimestampType
        {
            ID = "Offset_Y",
            Name = "Position Y",
            Get = (x) => ((Lane)x).Position.y,
            Set = (x, a) => { ((Lane)x).Position.y = a; },
        },
        new TimestampType
        {
            ID = "Offset_Z",
            Name = "Position Z",
            Get = (x) => ((Lane)x).Position.z,
            Set = (x, a) => { ((Lane)x).Position.z = a; },
        },
        new TimestampType
        {
            ID = "OffsetRotation_X",
            Name = "Rotation X",
            Get = (x) => ((Lane)x).Rotation.x,
            Set = (x, a) => { ((Lane)x).Rotation.x = a; },
        },
        new TimestampType
        {
            ID = "OffsetRotation_Y",
            Name = "Rotation Y",
            Get = (x) => ((Lane)x).Rotation.y,
            Set = (x, a) => { ((Lane)x).Rotation.y = a; },
        },
        new TimestampType
        {
            ID = "OffsetRotation_Z",
            Name = "Rotation Z",
            Get = (x) => ((Lane)x).Rotation.z,
            Set = (x, a) => { ((Lane)x).Rotation.z = a; },
        },
    };

    public Lane DeepClone()
    {
        Lane clone = new Lane()
        {
            Position = new Vector3(Position.x, Position.y, Position.z),
            Rotation = new Vector3(Rotation.x, Rotation.y, Rotation.z),
            Group = Group,
            StyleIndex = StyleIndex,
            Storyboard = Storyboard.DeepClone(),
        };
        foreach (HitObject obj in Objects) clone.Objects.Add(obj.DeepClone());
        foreach (LaneStep step in LaneSteps) clone.LaneSteps.Add(step.DeepClone());
        return clone;
    }
}

public class LaneManager
{
    public Lane CurrentLane;
    public List<LaneStepManager> Steps = new List<LaneStepManager>();
    public List<HitObjectManager> Objects = new List<HitObjectManager>();
    public Mesh CurrentMesh = new Mesh();

    public float CurrentSpeed;
    public float CurrentDistance;

    public Vector3 StartPos;
    public Vector3 EndPos;

    public Vector3 FinalPosition;
    public Quaternion FinalRotation;

    float lastStepCount;

    public LaneManager(Lane init, float time, float pos, ChartManager main)
    {
        Update(init, time, pos, main);
    }

    public void Update(Lane data, float time, float pos, ChartManager main)
    {
        CurrentLane = data;
        if (CurrentMesh == null) CurrentMesh = new Mesh();

        int stepCount = 0;
        bool force = main.CurrentSpeed != CurrentSpeed;
        float offset = float.NaN;
        CurrentSpeed = main.CurrentSpeed;
        for (int a = 0; a < CurrentLane.LaneSteps.Count; a++)
        {
            if (Steps.Count <= a) Steps.Add(new LaneStepManager());
            LaneStep step = (LaneStep)CurrentLane.LaneSteps[a].Get(pos);
            
            if (step.Offset != Steps[a].CurrentStep?.Offset) 
            {
                Steps[a].Offset = main.Song.Timing.ToSeconds(step.Offset);
                force = true;
            }
            if (force)
            {
                LaneStepManager prev = a < 1 ? new LaneStepManager() : Steps[a - 1];
                Steps[a].Distance = prev.Distance + CurrentSpeed * step.Speed * (Steps[a].Offset - prev.Offset);
            }

            Steps[a].CurrentStep = step;

            stepCount += float.IsNaN(offset) ? 1 : 
                Mathf.CeilToInt((offset == Steps[a].Offset ? (Steps[a].Offset > time ? 1 : 0) : Mathf.Clamp01((time - Steps[a].Offset) / (offset - Steps[a].Offset))) * (step.IsLinear ? 1 : 16));
            offset = Steps[a].Offset;
        }
        while (Steps.Count > CurrentLane.LaneSteps.Count) Steps.RemoveAt(CurrentLane.LaneSteps.Count);

        int index = 0;
        Vector3[] verts = new Vector3[stepCount * 2];
        Vector2[] uvs = new Vector2[stepCount * 2];
        LaneStepManager next = null;
        CurrentDistance = float.NaN;
        if (verts.Length > 0) for (int a = Steps.Count - 1; a >= 0; a--)
        {
            LaneStepManager curr = Steps[a];
            if (next == null)
            {
                verts[index] = (Vector3)curr.CurrentStep.StartPos + Vector3.forward * curr.Distance;
                verts[index + 1] = (Vector3)curr.CurrentStep.EndPos + Vector3.forward * curr.Distance;
                // Debug.Log(index + "/" + verts.Length + " " + verts[index] + " " + verts[index + 1]);
                index += 2;

                if (index >= verts.Length) 
                {
                    CurrentDistance = curr.Distance + curr.CurrentStep.Speed * CurrentSpeed * (time - curr.Offset);
                    break; 
                }
            }
            else if (next.CurrentStep.IsLinear)
            {
                float p = curr.Offset == next.Offset ? (curr.Offset < time ? 1 : 0) : Mathf.Clamp01((time - curr.Offset) / (next.Offset - curr.Offset));
                float dist = Mathf.Lerp(curr.Distance, next.Distance, p);
                verts[index] = Vector3.Lerp(curr.CurrentStep.StartPos, next.CurrentStep.StartPos, p) + Vector3.forward * dist;
                verts[index + 1] = Vector3.Lerp(curr.CurrentStep.EndPos, next.CurrentStep.EndPos, p) + Vector3.forward * dist;
                // Debug.Log(index + "/" + verts.Length + " " + verts[index] + " " + verts[index + 1]);
                index += 2;

                if (p > 0) 
                {
                    CurrentDistance = dist;
                    break; 
                }
                if (index >= verts.Length) 
                {
                    break; 
                }
            }
            else
            {
                float p = curr.Offset == next.Offset ? (curr.Offset < time ? 1 : 0) : Mathf.Clamp01((time - curr.Offset) / (next.Offset - curr.Offset));
                float dist = 0;
                for (int i = 15; i >= 0; i--)
                {
                    float x = Math.Max(i / 16f, p);
                    dist = Mathf.Lerp(curr.Distance, next.Distance, x);
                    verts[index] = new Vector3(Mathf.LerpUnclamped(curr.CurrentStep.StartPos.x, next.CurrentStep.StartPos.x, Ease.Get(x, next.CurrentStep.StartEaseX, next.CurrentStep.StartEaseXMode)),
                        Mathf.LerpUnclamped(curr.CurrentStep.StartPos.y, next.CurrentStep.StartPos.y, Ease.Get(x, next.CurrentStep.StartEaseY, next.CurrentStep.StartEaseYMode)), dist);
                    verts[index + 1] = new Vector3(Mathf.LerpUnclamped(curr.CurrentStep.EndPos.x, next.CurrentStep.EndPos.x, Ease.Get(x, next.CurrentStep.EndEaseX, next.CurrentStep.EndEaseXMode)),
                        Mathf.LerpUnclamped(curr.CurrentStep.EndPos.y, next.CurrentStep.EndPos.y, Ease.Get(x, next.CurrentStep.EndEaseY, next.CurrentStep.EndEaseYMode)), dist);
                    index += 2;
                    if (x == p || index >= verts.Length) break;
                }
                if (p > 0) 
                {
                    CurrentDistance = dist;
                    break; 
                }
                if (index >= verts.Length) 
                {
                    break; 
                }
            }
            next = curr;
        }
        if (float.IsNaN(CurrentDistance) && Steps.Count > 0) 
        {
            CurrentDistance = Steps[0].Distance + Steps[0].CurrentStep.Speed * CurrentSpeed * (time - Steps[0].Offset);
        }

        for (int a = 0; a < verts.Length; a++)
        {
            uvs[a] = new Vector2(a % 2, verts[a].z);
        }

        if (stepCount != lastStepCount) 
        {
            CurrentMesh.Clear();
            CurrentMesh.SetVertices(verts);
            CurrentMesh.SetUVs(0, uvs);
            RemakeMesh(stepCount);
            lastStepCount = stepCount;
        }
        else 
        {
            int[] tris = CurrentMesh.triangles;
            CurrentMesh.Clear();
            CurrentMesh.SetVertices(verts);
            CurrentMesh.SetUVs(0, uvs);
            CurrentMesh.SetTriangles(tris, 0);
        }

        FinalPosition = CurrentLane.Position;
        FinalRotation = Quaternion.Euler(CurrentLane.Rotation);
        if (!string.IsNullOrEmpty(CurrentLane.Group) && main.Groups.ContainsKey(CurrentLane.Group))
            main.Groups[CurrentLane.Group].Get(ref FinalPosition, ref FinalRotation);
        
        StartPos = verts[stepCount * 2 - 2] - Vector3.forward * CurrentDistance;
        StartPos = FinalRotation * StartPos + FinalPosition;
        EndPos = verts[stepCount * 2 - 1] - Vector3.forward * CurrentDistance;
        EndPos = FinalRotation * EndPos + FinalPosition;



        for (int a = 0; a < CurrentLane.Objects.Count; a++)
        {
            HitObject hit = (HitObject)CurrentLane.Objects[a].Get(pos);
            if (Objects.Count <= a) Objects.Add(new HitObjectManager(hit, time, this, main));
            else Objects[a].Update(hit, time, this, main);
        }
        while (Objects.Count > CurrentLane.Objects.Count)
        {
            Objects.RemoveAt(CurrentLane.Objects.Count);
        }
    }

    public LaneStep GetLaneStep(float sec, float speed = 1)
    {
        if (sec < Steps[0].Offset || Steps.Count <= 1)
        {
            return new LaneStep()
            {
                StartPos = CurrentLane.LaneSteps[0].StartPos,
                EndPos = CurrentLane.LaneSteps[0].EndPos,
                Offset = Steps[0].Distance - CurrentLane.LaneSteps[0].Speed * speed * (Steps[0].Offset - sec),
            };
        }
        else if (sec > Steps[Steps.Count - 1].Offset)
        {
            return new LaneStep()
            {
                StartPos = CurrentLane.LaneSteps[Steps.Count - 1].StartPos,
                EndPos = CurrentLane.LaneSteps[Steps.Count - 1].EndPos,
                Offset = Steps[Steps.Count - 1].Distance + CurrentLane.LaneSteps[Steps.Count - 1].Speed * speed * (sec - Steps[Steps.Count - 1].Offset),
            };
        }
        else for (int i = 1; i < Steps.Count; i++)
        {
            LaneStepManager prev = Steps[i - 1];
            LaneStep prevS = CurrentLane.LaneSteps[i - 1];
            LaneStepManager curr = Steps[i];
            LaneStep currS = CurrentLane.LaneSteps[i];
            if (sec > curr.Offset) continue;

            
            float p = (sec - prev.Offset) / (curr.Offset - prev.Offset);

            if (currS.IsLinear)
            {
                return new LaneStep 
                {
                    StartPos = Vector2.LerpUnclamped(prevS.StartPos, currS.StartPos, p),
                    EndPos = Vector2.LerpUnclamped(prevS.EndPos, currS.EndPos, p),
                    Offset = prev.Distance + currS.Speed * speed * (sec - prev.Offset),
                };
            }
            else 
            {
                
                return new LaneStep 
                {
                    StartPos = new Vector2(Mathf.LerpUnclamped(prevS.StartPos.x, currS.StartPos.x, Ease.Get(p, currS.StartEaseX, currS.StartEaseXMode)),
                        Mathf.LerpUnclamped(prevS.StartPos.y, currS.StartPos.y, Ease.Get(p, currS.StartEaseY, currS.StartEaseYMode))),
                    EndPos = new Vector2(Mathf.LerpUnclamped(prevS.EndPos.x, currS.EndPos.x, Ease.Get(p, currS.EndEaseX, currS.EndEaseXMode)),
                        Mathf.LerpUnclamped(prevS.EndPos.y, currS.EndPos.y, Ease.Get(p, currS.EndEaseY, currS.EndEaseYMode))),
                    Offset = prev.Distance + currS.Speed * speed * (sec - prev.Offset),
                };
            }
        }
        return null;
    }

    public void Dispose()
    {
        if (CurrentMesh != null) MonoBehaviour.DestroyImmediate(CurrentMesh);
    }

    public void RemakeMesh(int stepCount)
    {
        int[] tris = new int[Mathf.Max((stepCount - 1) * 6, 0)];

        for (int a = 0; a < stepCount - 1; a++)
        {
            tris[a * 6 + 0] = a * 2;
            tris[a * 6 + 1] = a * 2 + 1;
            tris[a * 6 + 2] = a * 2 + 2;

            tris[a * 6 + 3] = a * 2 + 2;
            tris[a * 6 + 4] = a * 2 + 1;
            tris[a * 6 + 5] = a * 2 + 3;
        }

        CurrentMesh.SetTriangles(tris, 0);
    }
}

[System.Serializable]
public class LaneStep : IStoryboardable, IDeepClonable<LaneStep> 
{
    public float Offset;
    public Vector2 StartPos;
    public EaseFunction StartEaseX = EaseFunction.Linear;
    public EaseMode StartEaseXMode;
    public EaseFunction StartEaseY = EaseFunction.Linear;
    public EaseMode StartEaseYMode;
    public Vector2 EndPos;
    public EaseFunction EndEaseX = EaseFunction.Linear;
    public EaseMode EndEaseXMode;
    public EaseFunction EndEaseY = EaseFunction.Linear;
    public EaseMode EndEaseYMode;
    public float Speed = 1;

    public bool IsLinear => StartEaseX == EaseFunction.Linear && StartEaseY == EaseFunction.Linear && EndEaseX == EaseFunction.Linear && EndEaseY == EaseFunction.Linear;

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
    
    public LaneStep DeepClone()
    {
        LaneStep clone = new LaneStep()
        {
            Offset = Offset,
            StartPos = new Vector2(StartPos.x, StartPos.y),
            StartEaseX = StartEaseX,
            StartEaseXMode = StartEaseXMode,
            StartEaseY = StartEaseY,
            StartEaseYMode = StartEaseYMode,
            EndPos = new Vector2(EndPos.x, EndPos.y),
            EndEaseX = EndEaseX,
            EndEaseXMode = EndEaseXMode,
            EndEaseY = EndEaseY,
            EndEaseYMode = EndEaseYMode,
            Speed = Speed,
            Storyboard = Storyboard.DeepClone(),
        };
        return clone;
    }
}

public class LaneStepManager
{
    public LaneStep CurrentStep;

    public float Offset;
    public float Distance;
}

[System.Serializable]
public class HitObject : IStoryboardable, IDeepClonable<HitObject>
{
    public HitType Type;
    public float Offset = 0;
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
    
    public HitObject DeepClone()
    {
        HitObject clone = new HitObject()
        {
            Type = Type,
            Offset = Offset,
            Position = Position,
            Length = Length,
            HoldLength = HoldLength,
            Flickable = Flickable,
            FlickDirection = FlickDirection,
            StyleIndex = StyleIndex,
            Storyboard = Storyboard.DeepClone(),
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

public class HitMeshManager
{
    public Dictionary<float, Mesh> NormalMeshes = new Dictionary<float, Mesh>();
    public Dictionary<float, int> NormalMeshCounts = new Dictionary<float, int>();
    public Dictionary<float, Mesh> CatchMeshes = new Dictionary<float, Mesh>();
    public Dictionary<float, int> CatchMeshCounts = new Dictionary<float, int>();

    public float Resolution = .001f;

    public void Cleanup()
    {
        List<float> list = new List<float>(NormalMeshes.Keys);
        foreach (float key in  list)
        {
            if (!NormalMeshCounts.ContainsKey(key)) 
            {
                MonoBehaviour.DestroyImmediate(NormalMeshes[key]);
                NormalMeshes.Remove(key);
            }
        }
        list = new List<float>(CatchMeshes.Keys);
        foreach (float key in list)
        {
            if (!CatchMeshCounts.ContainsKey(key)) 
            {
                MonoBehaviour.DestroyImmediate(CatchMeshes[key]);
                CatchMeshes.Remove(key);
            }
        }
        NormalMeshCounts.Clear();
        CatchMeshCounts.Clear();
    }
    
    public void Dispose()
    {
        foreach (float key in NormalMeshes.Keys)
        {
            MonoBehaviour.DestroyImmediate(NormalMeshes[key]);
        }
        foreach (float key in CatchMeshes.Keys)
        {
            MonoBehaviour.DestroyImmediate(CatchMeshes[key]);
        }
    }

    public Mesh GetMesh(HitObject.HitType type, float size)
    {
        size = Mathf.Floor(size / Resolution) * Resolution;

        Dictionary<float, Mesh> meshes = type == HitObject.HitType.Catch ? CatchMeshes : NormalMeshes;
        Dictionary<float, int> counts = type == HitObject.HitType.Catch ? CatchMeshCounts : NormalMeshCounts;

        if (!meshes.ContainsKey(size)) meshes[size] = MakeMesh(type, size);
        if (!counts.ContainsKey(size)) counts[size] = 0;
        counts[size]++;

        return meshes[size];
    }

    public Mesh MakeMesh(HitObject.HitType type, float size)
    {
        Vector3 startPos = Vector3.left * size / 2;
        Vector3 endPos = Vector3.right * size / 2;

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        void AddStep(Vector3 start, Vector3 end, bool addTris = true)
        {

            vertices.Add(start);
            vertices.Add(end);
            vertices.Add(start);
            vertices.Add(end);

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);

            if (addTris && vertices.Count >= 8)
            {
                tris.Add(vertices.Count - 8);
                tris.Add(vertices.Count - 3);
                tris.Add(vertices.Count - 7);

                tris.Add(vertices.Count - 3);
                tris.Add(vertices.Count - 8);
                tris.Add(vertices.Count - 4);
            }
        }
        if (type == HitObject.HitType.Normal)
        {
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .2f;
                AddStep((Vector3)startPos + Vector3.right * .2f + ofs, (Vector3)endPos - Vector3.right * .2f + ofs);
            }
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .2f;
                AddStep((Vector3)startPos + ofs, (Vector3)startPos + Vector3.right * .1f + ofs, ang != 45);
            }
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .2f;
                AddStep((Vector3)endPos - Vector3.right * .1f + ofs, (Vector3)endPos + ofs, ang != 45);
            }
        }
        else if (type == HitObject.HitType.Catch)
        {
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .1f;
                AddStep((Vector3)startPos + Vector3.right * .1f + ofs, (Vector3)endPos - Vector3.right * .1f + ofs);
            }
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .2f;
                AddStep((Vector3)startPos + ofs, (Vector3)startPos + Vector3.right * .1f + ofs, ang != 45);
            }
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .2f;
                AddStep((Vector3)endPos - Vector3.right * .1f + ofs, (Vector3)endPos + ofs, ang != 45);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}

public class HitObjectManager
{
    public HitObject CurrentHit;

    public Vector3 Position;
    public Quaternion Rotation;
    
    public Vector3 StartPos;
    public Vector3 EndPos;

    public Mesh CurrentMesh = new Mesh();

    public HitObjectManager(HitObject data, float time, LaneManager lane, ChartManager main)
    {
        Update(data, time, lane, main);
    }

    public void Update(HitObject data, float time, LaneManager lane, ChartManager main)
    {
        CurrentHit = data;
        float offset = main.Song.Timing.ToSeconds(data.Offset);

        if (time <= offset)
        {
            LaneStep step = lane.GetLaneStep(offset, main.CurrentSpeed);

            Vector3 fwd = Vector3.forward * (step.Offset - lane.CurrentDistance);
            StartPos = Vector3.LerpUnclamped(step.StartPos, step.EndPos, data.Position) + fwd;
            EndPos = Vector3.LerpUnclamped(step.StartPos, step.EndPos, data.Position + data.Length) + fwd;

            Position = lane.FinalRotation * ((StartPos + EndPos) / 2) + lane.FinalPosition;
            Rotation = lane.FinalRotation * (Quaternion.LookRotation(EndPos - StartPos) * Quaternion.Euler(0, 90, 0));

            CurrentMesh = main.HitMeshManager.GetMesh(data.Type, Vector3.Distance(StartPos, EndPos));
        }
        else
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            CurrentMesh = null;
        }
    }
}
