using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Serialization;

public class ChartManager 
{
    public PlayableSong Song;
    public Chart CurrentChart;
    
    public Dictionary<string, LaneGroupManager> Groups = new ();
    public List<LaneManager> Lanes = new ();
    public HitMeshManager HitMeshManager = new ();
    public PalleteManager PalleteManager = new ();
    public CameraController Camera;

    public float CurrentSpeed;
    public float CurrentTime;
    public int[] HitObjectsRemaining;
    public int FlicksRemaining;

    public int ActiveLaneCount;
    public int ActiveHitCount;
    public int ActiveLaneVerts;
    public int ActiveLaneTris;

    public ChartManager (PlayableSong song, Chart chart, float speed, float time, float pos)
    {
        Song = song;
        CurrentChart = chart;
        CurrentSpeed = speed;
        Update(time, pos);
    }

    public void Update(float time, float pos)
    {
        PalleteManager.Update(CurrentChart.Pallete, pos);
        Camera = (CameraController)CurrentChart.Camera.Get(pos);
        HitObjectsRemaining = new [] { 0, 0 };
        FlicksRemaining = 0;
        ActiveLaneCount = ActiveHitCount = ActiveLaneVerts = ActiveLaneTris = 0;

        for (int a = 0; a < CurrentChart.Groups.Count; a++)
        {
            LaneGroup group = (LaneGroup)CurrentChart.Groups[a].Get(pos);
            if (Groups.ContainsKey(group.Name)) Groups[group.Name].Update(group, pos, this);
            else Groups.Add(group.Name, new LaneGroupManager(group, pos, this));
            Groups[group.Name].isTouched = true;
        }
        foreach (KeyValuePair<string, LaneGroupManager> pair in new Dictionary<string, LaneGroupManager>(Groups))
        {
            if (pair.Value.isDirty) pair.Value.UpdatePosition(this);
            else if (!pair.Value.isTouched) Groups.Remove(pair.Key);
            else pair.Value.isTouched = false;
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


public class PalleteManager
{
    public Pallete CurrentPallete;

    public List<LaneStyleManager> LaneStyles = new ();
    public List<HitStyleManager> HitStyles = new ();

    public void Update(Pallete pallete, float pos)
    {
        CurrentPallete = pallete = (Pallete)pallete.Get(pos);

        for (int a = 0; a < pallete.LaneStyles.Count; a++)
        {
            LaneStyle style = (LaneStyle)pallete.LaneStyles[a].Get(pos);
            if (LaneStyles.Count <= a) LaneStyles.Add(new LaneStyleManager(style));
            else LaneStyles[a].Update(style);
        }
        while (LaneStyles.Count > pallete.LaneStyles.Count)
        {
            LaneStyles[pallete.LaneStyles.Count].Dispose();
            LaneStyles.RemoveAt(pallete.LaneStyles.Count);
        }

        for (int a = 0; a < pallete.HitStyles.Count; a++)
        {
            HitStyle style = (HitStyle)pallete.HitStyles[a].Get(pos);
            if (HitStyles.Count <= a) HitStyles.Add(new HitStyleManager(style));
            else HitStyles[a].Update(style);
        }
        while (HitStyles.Count > pallete.HitStyles.Count)
        {
            HitStyles[pallete.HitStyles.Count].Dispose();
            HitStyles.RemoveAt(pallete.HitStyles.Count);
        }
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
        // Debug.Log(style.LaneMaterial);

        if (BaseLaneMaterial?.name != style.LaneMaterial) 
        {
            LaneMaterial = new Material(BaseLaneMaterial = Resources.Load<Material>("Materials/Lane/" + style.LaneMaterial));
        }
        if (BaseJudgeMaterial?.name != style.JudgeMaterial) 
        {
            JudgeMaterial = new Material(BaseJudgeMaterial = Resources.Load<Material>("Materials/Judge/" + style.LaneMaterial));
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
        if (BaseMainMaterial?.name != style.MainMaterial) 
        {
            NormalMaterial = new Material(BaseMainMaterial = Resources.Load<Material>("Materials/Hit/" + style.MainMaterial));
            CatchMaterial = new Material(BaseMainMaterial);
        }
        if (BaseHoldTailMaterial?.name != style.HoldTailMaterial) 
        {
            HoldTailMaterial = new Material(BaseHoldTailMaterial = Resources.Load<Material>("Materials/Hold/" + style.HoldTailMaterial));
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


public class LaneGroupManager
{
    public LaneGroup CurrentGroup;
    public Vector3 FinalPosition;
    public Quaternion FinalRotation;
    public bool isDirty;
    public bool isTouched;

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
        original ??= CurrentGroup.Group;
        if (!string.IsNullOrEmpty(CurrentGroup.Group) && main.Groups.ContainsKey(CurrentGroup.Group))
        {
            LaneGroupManager group = main.Groups[CurrentGroup.Group];
            if (original == group.CurrentGroup.Group)
            {
                Debug.LogError("Cyclical Lane group reference detected: " + original);
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


public class LaneManager
{
    public Lane CurrentLane;
    public List<LaneStepManager> Steps = new List<LaneStepManager>();
    public List<HitObjectManager> Objects = new List<HitObjectManager>();
    public Mesh CurrentMesh = new Mesh();

    public float CurrentSpeed;
    public float CurrentDistance;

    public Vector3 StartPosLocal;
    public Vector3 EndPosLocal;

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
            if (step.Speed != Steps[a].CurrentStep?.Speed) 
            {
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
                    verts[index] = new Vector3(Mathf.LerpUnclamped(curr.CurrentStep.StartPos.x, next.CurrentStep.StartPos.x, next.CurrentStep.StartEaseX.Get(x)),
                        Mathf.LerpUnclamped(curr.CurrentStep.StartPos.y, next.CurrentStep.StartPos.y, next.CurrentStep.StartEaseY.Get(x)), dist);
                    verts[index + 1] = new Vector3(Mathf.LerpUnclamped(curr.CurrentStep.EndPos.x, next.CurrentStep.EndPos.x, next.CurrentStep.EndEaseX.Get(x)),
                        Mathf.LerpUnclamped(curr.CurrentStep.EndPos.y, next.CurrentStep.EndPos.y, next.CurrentStep.EndEaseY.Get(x)), dist);
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
            RemakeMesh(CurrentMesh, stepCount);
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

        main.ActiveLaneCount++;
        main.ActiveLaneVerts += verts.Length;
        main.ActiveLaneTris += CurrentMesh.triangles.Length;

        FinalPosition = CurrentLane.Position;
        FinalRotation = Quaternion.Euler(CurrentLane.Rotation);
        if (!string.IsNullOrEmpty(CurrentLane.Group) && main.Groups.ContainsKey(CurrentLane.Group))
            main.Groups[CurrentLane.Group].Get(ref FinalPosition, ref FinalRotation);
        
        StartPosLocal = StartPos = verts[stepCount * 2 - 2] - Vector3.forward * CurrentDistance;
        StartPos = FinalRotation * StartPos + FinalPosition;
        EndPosLocal = EndPos = verts[stepCount * 2 - 1] - Vector3.forward * CurrentDistance;
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
    
    public Mesh GetPartOfLane(float timeStart, float timeEnd, float xPos, float xLength)
    {

        List <Vector3> verts = new();
        List <Vector2> uvs = new();
        for (int a = Steps.Count - 1; a >= 1; a--)
        {
            LaneStepManager next = Steps[a];
            LaneStepManager curr = Steps[a - 1];
            
            float pStart = curr.Offset == next.Offset ? (curr.Offset < timeStart ? 1 : 0) : Mathf.Clamp01((timeStart - curr.Offset) / (next.Offset - curr.Offset));
            float pEnd = curr.Offset == next.Offset ? (curr.Offset < timeEnd ? 1 : 0) : Mathf.Clamp01((timeEnd - curr.Offset) / (next.Offset - curr.Offset));

            if (curr.Offset > timeEnd) continue;

            if (verts.Count < 1)
            {
                if (next.CurrentStep.IsLinear)
                {
                    float dist = Mathf.Lerp(curr.Distance, next.Distance, pEnd);
                    Vector3 currStart = Vector3.LerpUnclamped(curr.CurrentStep.StartPos, curr.CurrentStep.EndPos, xPos);
                    Vector3 currEnd = Vector3.LerpUnclamped(curr.CurrentStep.StartPos, curr.CurrentStep.EndPos, xPos + xLength);
                    Vector3 nextStart = Vector3.LerpUnclamped(next.CurrentStep.StartPos, next.CurrentStep.EndPos, xPos);
                    Vector3 nextEnd = Vector3.LerpUnclamped(next.CurrentStep.StartPos, next.CurrentStep.EndPos, xPos + xLength);
                    verts.Add(Vector3.Lerp(currStart, nextStart, pEnd) + Vector3.forward * dist);
                    verts.Add(Vector3.Lerp(currEnd, nextEnd, pEnd) + Vector3.forward * dist);
                    // Debug.Log(index + "/" + verts.Length + " " + verts[index] + " " + verts[index + 1]);
                }
                else
                {
                    float dist = 0;
                    Vector3 currStart = Vector3.LerpUnclamped(curr.CurrentStep.StartPos, curr.CurrentStep.EndPos, xPos);
                    Vector3 currEnd = Vector3.LerpUnclamped(curr.CurrentStep.StartPos, curr.CurrentStep.EndPos, xPos + xLength);
                    Vector3 nextStart = Vector3.LerpUnclamped(next.CurrentStep.StartPos, next.CurrentStep.EndPos, xPos);
                    Vector3 nextEnd = Vector3.LerpUnclamped(next.CurrentStep.StartPos, next.CurrentStep.EndPos, xPos + xLength);
                    float x = pEnd;
                    dist = Mathf.Lerp(curr.Distance, next.Distance, x);
                    verts.Add(new Vector3(Mathf.LerpUnclamped(currStart.x, nextStart.x, next.CurrentStep.StartEaseX.Get(x)),
                        Mathf.LerpUnclamped(currStart.y, nextStart.y, next.CurrentStep.StartEaseY.Get(x)), dist));
                    verts.Add(new Vector3(Mathf.LerpUnclamped(currEnd.x, nextEnd.x, next.CurrentStep.EndEaseX.Get(x)),
                        Mathf.LerpUnclamped(currEnd.y, nextEnd.y, next.CurrentStep.EndEaseY.Get(x)), dist));
                }
            }

            if (next.CurrentStep.IsLinear)
            {
                float dist = Mathf.Lerp(curr.Distance, next.Distance, pStart);
                Vector3 currStart = Vector3.LerpUnclamped(curr.CurrentStep.StartPos, curr.CurrentStep.EndPos, xPos);
                Vector3 currEnd = Vector3.LerpUnclamped(curr.CurrentStep.StartPos, curr.CurrentStep.EndPos, xPos + xLength);
                Vector3 nextStart = Vector3.LerpUnclamped(next.CurrentStep.StartPos, next.CurrentStep.EndPos, xPos);
                Vector3 nextEnd = Vector3.LerpUnclamped(next.CurrentStep.StartPos, next.CurrentStep.EndPos, xPos + xLength);
                verts.Add(Vector3.Lerp(currStart, nextStart, pStart) + Vector3.forward * dist);
                verts.Add(Vector3.Lerp(currEnd, nextEnd, pStart) + Vector3.forward * dist);
                // Debug.Log(index + "/" + verts.Length + " " + verts[index] + " " + verts[index + 1]);
            }
            else
            {
                float dist = 0;
                Vector3 currStart = Vector3.LerpUnclamped(curr.CurrentStep.StartPos, curr.CurrentStep.EndPos, xPos);
                Vector3 currEnd = Vector3.LerpUnclamped(curr.CurrentStep.StartPos, curr.CurrentStep.EndPos, xPos + xLength);
                Vector3 nextStart = Vector3.LerpUnclamped(next.CurrentStep.StartPos, next.CurrentStep.EndPos, xPos);
                Vector3 nextEnd = Vector3.LerpUnclamped(next.CurrentStep.StartPos, next.CurrentStep.EndPos, xPos + xLength);
                for (int i = Mathf.FloorToInt(pEnd * 16); i >= 0; i--)
                {
                    float x = Math.Max(i / 16f, pStart);
                    dist = Mathf.Lerp(curr.Distance, next.Distance, x);
                    verts.Add(new Vector3(Mathf.LerpUnclamped(currStart.x, nextStart.x, next.CurrentStep.StartEaseX.Get(x)),
                        Mathf.LerpUnclamped(currStart.y, nextStart.y, next.CurrentStep.StartEaseY.Get(x)), dist));
                    verts.Add(new Vector3(Mathf.LerpUnclamped(currEnd.x, nextEnd.x, next.CurrentStep.EndEaseX.Get(x)),
                        Mathf.LerpUnclamped(currEnd.y, nextEnd.y, next.CurrentStep.EndEaseY.Get(x)), dist));
                    if (x == pStart) break;
                }
            }

            if (pStart > 0) 
            {
                break; 
            }
    }

        for (int a = 0; a < verts.Count; a++)
        {
            uvs.Add(new Vector2(a % 2, verts[a].z));
        }

        Mesh mesh = new();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        RemakeMesh(mesh, verts.Count / 2);

        return mesh;
    }

    public LanePosition GetLanePosition(float sec, float speed = 1)
    {
        if (sec < Steps[0].Offset || Steps.Count <= 1)
        {
            return new LanePosition()
            {
                StartPos = CurrentLane.LaneSteps[0].StartPos,
                EndPos = CurrentLane.LaneSteps[0].EndPos,
                Offset = Steps[0].Distance - Steps[0].CurrentStep.Speed * speed * (Steps[0].Offset - sec),
            };
        }
        else if (sec > Steps[Steps.Count - 1].Offset)
        {
            return new LanePosition()
            {
                StartPos = CurrentLane.LaneSteps[Steps.Count - 1].StartPos,
                EndPos = CurrentLane.LaneSteps[Steps.Count - 1].EndPos,
                Offset = Steps[Steps.Count - 1].Distance + Steps[Steps.Count - 1].CurrentStep.Speed * speed * (sec - Steps[Steps.Count - 1].Offset),
            };
        }
        else for (int i = 1; i < Steps.Count; i++)
        {
            LaneStepManager prev = Steps[i - 1];
            LaneStep prevS = prev.CurrentStep;
            LaneStepManager curr = Steps[i];
            LaneStep currS = curr.CurrentStep;
            if (sec > curr.Offset) continue;

            
            float p = (sec - prev.Offset) / (curr.Offset - prev.Offset);

            if (currS.IsLinear)
            {
                return new LanePosition 
                {
                    StartPos = Vector2.LerpUnclamped(prevS.StartPos, currS.StartPos, p),
                    EndPos = Vector2.LerpUnclamped(prevS.EndPos, currS.EndPos, p),
                    Offset = prev.Distance + currS.Speed * speed * (sec - prev.Offset),
                };
            }
            else 
            {
                
                return new LanePosition 
                {
                    StartPos = new Vector2(Mathf.LerpUnclamped(prevS.StartPos.x, currS.StartPos.x, currS.StartEaseX.Get(p)),
                        Mathf.LerpUnclamped(prevS.StartPos.y, currS.StartPos.y, currS.StartEaseY.Get(p))),
                    EndPos = new Vector2(Mathf.LerpUnclamped(prevS.EndPos.x, currS.EndPos.x, currS.EndEaseX.Get(p)),
                        Mathf.LerpUnclamped(prevS.EndPos.y, currS.EndPos.y, currS.EndEaseY.Get(p))),
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

    public void RemakeMesh(Mesh mesh, int stepCount)
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

        mesh.SetTriangles(tris, 0);
    }
}


public class LaneStepManager
{
    public LaneStep CurrentStep;

    public float Offset;
    public float Distance;
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
    public float TimeStart;
    public float TimeEnd;

    public Vector3 Position;
    public Quaternion Rotation;
    public float Length;
    
    public Vector3 StartPos;
    public Vector3 EndPos;

    public Mesh HoldMesh;

    public HitObjectManager(HitObject data, float time, LaneManager lane, ChartManager main)
    {
        Update(data, time, lane, main);
    }

    public void Update(HitObject data, float time, LaneManager lane, ChartManager main)
    {
        CurrentHit = data;
        TimeStart = main.Song.Timing.ToSeconds(data.Offset);
        TimeEnd = data.HoldLength > 0 ? main.Song.Timing.ToSeconds(data.Offset + data.HoldLength) : TimeStart;
        
        if (HoldMesh) MonoBehaviour.DestroyImmediate(HoldMesh);
        
        if (time <= TimeStart) 
        {
            main.HitObjectsRemaining[(int)data.Type]++;
            if (data.Flickable) main.FlicksRemaining++;
        }

        if (time <= TimeEnd)
        {
            LanePosition pos = lane.GetLanePosition(Mathf.Max(TimeStart, time), main.CurrentSpeed);

            Vector3 fwd = Vector3.forward * pos.Offset;
            StartPos = Vector3.LerpUnclamped(pos.StartPos, pos.EndPos, data.Position) + fwd;
            EndPos = Vector3.LerpUnclamped(pos.StartPos, pos.EndPos, data.Position + data.Length) + fwd;

            Position = (StartPos + EndPos) / 2;
            Rotation = Quaternion.LookRotation(EndPos - StartPos) * Quaternion.Euler(0, 90, 0);
            Length = Vector3.Distance(StartPos, EndPos);

            HoldMesh = pos.Offset < lane.CurrentDistance + 250 && TimeStart < TimeEnd ? lane.GetPartOfLane(Mathf.Max(TimeStart, time), TimeEnd, data.Position, data.Length) : null;

            if (pos.Offset < lane.CurrentDistance + 250) main.ActiveHitCount++;
            main.ActiveLaneVerts += HoldMesh?.vertices.Length ?? 0;
            main.ActiveLaneTris += HoldMesh?.triangles.Length ?? 0;
        } else {
            HoldMesh = null;
        }
    }
}
