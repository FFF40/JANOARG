using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LanePlayer : MonoBehaviour
{
    public Lane Original;
    public Lane Current;
    [Space]
    public Transform Holder;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;
    public LaneGroupPlayer Group;
    [Space]
    public MeshRenderer JudgeLine;
    public MeshRenderer JudgeLeft;
    public MeshRenderer JudgeRight;

    public List<float> Positions = new();
    public List<float> Times = new();
    public float CurrentPosition;

    public List<HitPlayer> HitObjects = new();
    public List<HitScreenCoord> HitCoords = new();

    public bool LaneStepDirty = false;

    public void Init() 
    {
        var met = PlayerScreen.TargetSong.Timing;
        foreach (LaneStep step in Current.LaneSteps)
        {
            Times.Add(met.ToSeconds(step.Offset));
        }
        if (Current.StyleIndex >= 0 && Current.StyleIndex < PlayerScreen.main.LaneStyles.Count)
        {
            LaneStyleManager style = PlayerScreen.main.LaneStyles[Current.StyleIndex];
            MeshRenderer.sharedMaterial = style.LaneMaterial;
            JudgeLine.sharedMaterial = JudgeLeft.sharedMaterial = JudgeRight.sharedMaterial
                = style.JudgeMaterial;
        }
        else 
        {
            MeshRenderer.enabled = false;
            JudgeLine.gameObject.SetActive(false);
            JudgeLeft.gameObject.SetActive(false);
            JudgeRight.gameObject.SetActive(false);
        }
    }

    public void UpdateSelf(float time, float beat)
    {
        if (Current != null) Current.Advance(beat);
        else Current = (Lane)Original.Get(beat);

        UpdateMesh(time, beat);
        transform.localPosition = Current.Position;
        transform.localEulerAngles = Current.Rotation;
        Holder.localPosition = Vector3.back * CurrentPosition;
        if (CurrentPosition - Positions[0] > -200) UpdateHitObjects(time, beat);
    }

    public void UpdateMesh(float time, float beat, float maxDistance = 200)
    {
        Mesh mesh = MeshFilter.mesh ?? new Mesh();
        List<Vector3> verts = new();
        List<int> tris = new();
        
        void AddLine(Vector3 start, Vector3 end) 
        {
            verts.AddRange(new [] {start, end});
            if (verts.Count > 2) tris.AddRange(new [] {
                verts.Count - 4, verts.Count - 2, verts.Count - 3,
                verts.Count - 2, verts.Count - 1, verts.Count - 3
            });
        }

        while (Times.Count > 2 && Times[1] < time) 
        {
            Times.RemoveAt(0);
            Positions.RemoveAt(0);
            Current.LaneSteps.RemoveAt(0);
        }

        if (Current.LaneSteps.Count < 1) 
        {
            if (Times[0] < time)
            {
                Destroy(mesh);
                Destroy(gameObject);
            }
            return;
        }
        
        Current.LaneSteps[0].Advance(beat);
        if (Current.LaneSteps.Count > 1) Current.LaneSteps[1].Advance(beat);

        CurrentPosition = (Times.Count <= 1 || Times[0] > time) ? time * Current.LaneSteps[0].Speed * PlayerScreen.main.Speed : 
            (time - Times[0]) * Current.LaneSteps[1].Speed * PlayerScreen.main.Speed + Positions[0];
        
        float progress = Times.Count <= 1 ? 0 : Mathf.InverseLerp(Times[0], Times[1], time);

        if (Positions.Count <= 1)
        {
            Positions.Add(Times[0] * Current.LaneSteps[0].Speed * PlayerScreen.main.Speed);
        }
        if (Times.Count <= 1)
        {
            return;
        }
        if (Positions.Count <= 2)
        {
            Positions.Add((Times[1] - Times[0]) * Current.LaneSteps[1].Speed * PlayerScreen.main.Speed + Positions[0]);
        }
        else 
        {
            Positions[1] = Positions[0] + (Times[1] - Times[0]) * Current.LaneSteps[1].Speed * PlayerScreen.main.Speed;
        }

        {

            float position = Mathf.Lerp(Positions[0], Positions[1], progress);
            var cur = Current.LaneSteps[1];
            Vector3 start, end;

            if (cur.IsLinear)
            {
                start = Vector3.Lerp(Current.LaneSteps[0].StartPos, Current.LaneSteps[1].StartPos, progress) + Vector3.forward * position;
                end = Vector3.Lerp(Current.LaneSteps[0].EndPos, Current.LaneSteps[1].EndPos, progress) + Vector3.forward * position;
            }
            else 
            {
                start = new Vector3(
                    Mathf.LerpUnclamped(Current.LaneSteps[0].StartPos.x, Current.LaneSteps[1].StartPos.x, cur.StartEaseX.Get(progress)), 
                    Mathf.LerpUnclamped(Current.LaneSteps[0].StartPos.y, Current.LaneSteps[1].StartPos.y, cur.StartEaseY.Get(progress)),
                    position
                );
                end = new Vector3(
                    Mathf.LerpUnclamped(Current.LaneSteps[0].EndPos.x, Current.LaneSteps[1].EndPos.x, cur.EndEaseX.Get(progress)), 
                    Mathf.LerpUnclamped(Current.LaneSteps[0].EndPos.y, Current.LaneSteps[1].EndPos.y, cur.EndEaseY.Get(progress)),
                    position
                );
            }

            AddLine(start, end);
            JudgeLine.enabled = JudgeLeft.enabled = JudgeRight.enabled = Times.Count >= 2 && time >= Times[0] && time < Times[1];
            if (JudgeLine.enabled && JudgeLine.gameObject.activeSelf) 
            {
                JudgeLine.transform.localPosition = (start + end) / 2;
                JudgeLine.transform.localScale = new(Vector2.Distance(start, end), .05f, .05f);
                JudgeLine.transform.localEulerAngles = Vector3.forward * Vector2.SignedAngle(Vector2.right, end - start);
                JudgeLeft.transform.localPosition = start;
                JudgeRight.transform.localPosition = end;
            }
        }


        if (Current.LaneSteps[0].IsDirty) 
        {
            LaneStepDirty = true;
            Current.LaneSteps[0].IsDirty = false;
        }
        if (Current.LaneSteps[1].IsDirty) 
        {
            LaneStepDirty = true;
            Current.LaneSteps[1].IsDirty = false;
        }

        for (int a = 1; a < Times.Count; a++) 
        {
            var cur = Current.LaneSteps[a];
            if (a > 1) 
            {
                cur.Advance(beat);
                if (cur.IsDirty) 
                {
                    LaneStepDirty = true;
                    cur.IsDirty = false;
                }
            }
            float pos = Positions[a - 1] + (Times[a] - Times[a - 1]) * cur.Speed * PlayerScreen.main.Speed;
            if (Positions.Count <= a) Positions.Add(pos); else Positions[a] = pos;
            if (cur.IsLinear)
            {
                AddLine(
                    (Vector3)cur.StartPos + Vector3.forward * pos, 
                    (Vector3)cur.EndPos + Vector3.forward * pos
                );
            }
            else 
            {
                var pre = Current.LaneSteps[a - 1];
                for (float x = Mathf.Floor(progress * 16 + 1.01f) / 16; x <= 1; x = Mathf.Floor(x * 16 + 1.01f) / 16)
                {
                    AddLine(
                        new Vector3(
                            Mathf.LerpUnclamped(pre.StartPos.x, cur.StartPos.x, cur.StartEaseX.Get(x)), 
                            Mathf.LerpUnclamped(pre.StartPos.y, cur.StartPos.y, cur.StartEaseY.Get(x)),
                        Mathf.Lerp(Positions[a - 1], pos, x)), 
                        new Vector3(
                            Mathf.LerpUnclamped(pre.EndPos.x, cur.EndPos.x, cur.EndEaseX.Get(x)), 
                            Mathf.LerpUnclamped(pre.EndPos.y, cur.EndPos.y, cur.EndEaseY.Get(x)),
                        Mathf.Lerp(Positions[a - 1], pos, x))
                    );
                }
            }
            progress = 0;
            if (a >= Positions.Count && pos - CurrentPosition > 200) break;
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        MeshFilter.mesh = mesh;
    }

    float hoTime = float.NaN;
    int hoOffset = 0;

    public void UpdateHitObjects(float time, float beat, float maxDistance = 200) 
    {
        while (Current.Objects.Count > 0) 
        {
            HitObject hit = Current.Objects[0];
            if (float.IsNaN(hoTime)) hoTime = PlayerScreen.TargetSong.Timing.ToSeconds(hit.Offset);
            if (GetZPosition(hoTime) <= CurrentPosition + maxDistance) 
            {
                HitPlayer player = Instantiate(PlayerScreen.main.HitSample, Holder);
               
                player.Original = Original.Objects[hoOffset];
                player.Current = Current.Objects[0];
                
                player.Time = hoTime;
                player.EndTime = player.Current.HoldLength > 0 ? PlayerScreen.TargetSong.Timing.ToSeconds(hit.Offset + hit.HoldLength) : hoTime;
                player.HitCoord = HitCoords[0];
                if (player.Current.HoldLength > 0)
                {
                    for (float a = 0.5f; a < player.Current.HoldLength; a += 0.5f) player.HoldTicks.Add(PlayerScreen.TargetSong.Timing.ToSeconds(hit.Offset + a));
                    player.HoldTicks.Add(player.EndTime);
                    UpdateHoldMesh(player);
                }
                
                player.Lane = this;
                HitObjects.Add(player);
                PlayerInputManager.main.AddToQueue(player);
                player.Init();

                Current.Objects.RemoveAt(0);
                HitCoords.RemoveAt(0);
                hoTime = float.NaN;
                hoOffset++;
            }
            else 
            {
                break;
            }
        }
        bool active = true;
        for (int a = 0; a < HitObjects.Count; a++) 
        {
            if (active) HitObjects[a].UpdateSelf(time, beat, LaneStepDirty);
            if (active && HitObjects[a].CurrentPosition > CurrentPosition + 200) active = false;
            HitObjects[a].gameObject.SetActive(active);
            if (HitObjects[a].HoldMesh) HitObjects[a].HoldMesh.gameObject.SetActive(active);
        }
        LaneStepDirty = false;
    }

    public float GetZPosition(float time) 
    {
        int index = Times.FindIndex(x => x >= time);
        if (index < 0) return Positions[^1] + (time - Times[Positions.Count - 1]) * Current.LaneSteps[Positions.Count - 1].Speed * PlayerScreen.main.Speed; 
        index = Mathf.Min(index, Positions.Count - 1);
        return Positions[index] + (time - Times[index]) * Current.LaneSteps[index].Speed * PlayerScreen.main.Speed; 
    }

    public void GetStartEndPosition(float time, out Vector2 start, out Vector2 end) 
    {
        int index = Times.FindIndex(x => x >= time);
        if (index < 0)
        {
            start = Current.LaneSteps[^1].StartPos;
            end = Current.LaneSteps[^1].EndPos;
        }
        else if (index == 0)
        {
            start = Current.LaneSteps[0].StartPos;
            end = Current.LaneSteps[0].EndPos;
        }
        else 
        {
            var cur = Current.LaneSteps[index];
            var pre = Current.LaneSteps[index - 1];
            float progress = Mathf.InverseLerp(Times[index - 1], Times[index], time);
            if (cur.IsLinear) 
            {
                start = Vector2.Lerp(pre.StartPos, cur.StartPos, progress);
                end = Vector2.Lerp(pre.EndPos, cur.EndPos, progress);
            }
            else 
            {
                start = new Vector2(
                    Mathf.LerpUnclamped(pre.StartPos.x, cur.StartPos.x, cur.StartEaseX.Get(progress)), 
                    Mathf.LerpUnclamped(pre.StartPos.y, cur.StartPos.y, cur.StartEaseY.Get(progress)));
                end = new Vector2(
                    Mathf.LerpUnclamped(pre.EndPos.x, cur.EndPos.x, cur.EndEaseX.Get(progress)), 
                    Mathf.LerpUnclamped(pre.EndPos.y, cur.EndPos.y, cur.EndEaseY.Get(progress)));
            }
        }
    }

    public void UpdateHoldMesh(HitPlayer hit)
    {
        if (hit.HoldRenderer == null)
        {
            hit.HoldRenderer = Instantiate(PlayerScreen.main.HoldSample, Holder);
            hit.HoldMesh = hit.HoldRenderer.GetComponent<MeshFilter>();
        }
        if (hit.HoldMesh.mesh == null)
        {
            hit.HoldMesh.mesh = new Mesh();
        }
        Mesh mesh = hit.HoldMesh.mesh;
        List<Vector3> verts = new();
        List<int> tris = new();
        
        void AddLine(Vector3 start, Vector3 end) 
        {
            verts.AddRange(new [] {start, end});
            if (verts.Count > 2) tris.AddRange(new [] {
                verts.Count - 4, verts.Count - 2, verts.Count - 3,
                verts.Count - 2, verts.Count - 1, verts.Count - 3
            });
        }

        float time = Mathf.Max(PlayerScreen.main.CurrentTime + PlayerScreen.main.Settings.VisualOffset, hit.Time);

        int index = Times.FindIndex(x => x > time);
        if (index <= 0 || Positions.Count <= index) return;
        index = Mathf.Max(index, 1);
        
        float progress = Times.Count <= 1 ? 0 : Mathf.InverseLerp(Times[index - 1], Times[index], time);
        Vector3 preStartPos, preEndPos, curStartPos, curEndPos;
        
        {
            float position = Mathf.Lerp(Positions[index - 1], Positions[index], progress);
            var pre = Current.LaneSteps[index - 1];
            var cur = Current.LaneSteps[index];

            preStartPos = Vector3.LerpUnclamped(pre.StartPos, pre.EndPos, hit.Current.Position);
            preEndPos = Vector3.LerpUnclamped(pre.StartPos, pre.EndPos, hit.Current.Position + hit.Current.Length);
            curStartPos = Vector3.LerpUnclamped(cur.StartPos, cur.EndPos, hit.Current.Position);
            curEndPos = Vector3.LerpUnclamped(cur.StartPos, cur.EndPos, hit.Current.Position + hit.Current.Length);

            if (cur.IsLinear)
            {
                AddLine(
                    Vector3.Lerp(preStartPos, curStartPos, progress) + Vector3.forward * position, 
                    Vector3.Lerp(preEndPos, curEndPos, progress) + Vector3.forward * position);
            }
            else 
            {
                AddLine(
                    new Vector3(
                        Mathf.LerpUnclamped(preStartPos.x, curStartPos.x, cur.StartEaseX.Get(progress)), 
                        Mathf.LerpUnclamped(preStartPos.y, curStartPos.y, cur.StartEaseY.Get(progress)),
                    position), 
                    new Vector3(
                        Mathf.LerpUnclamped(preEndPos.x, curEndPos.x, cur.EndEaseX.Get(progress)), 
                        Mathf.LerpUnclamped(preEndPos.y, curEndPos.y, cur.EndEaseY.Get(progress)),
                    position));
            }
        }

        int ai = 0;
        for (; index < Mathf.Min(Positions.Count, Times.Count); index++) 
        {
            float endProg = InverseLerpUnclamped(Times[index - 1], Times[index], hit.EndTime);
            float endPos = Mathf.Lerp(Positions[index - 1], Positions[index], endProg);
            var cur = Current.LaneSteps[index];

            curStartPos = Vector3.LerpUnclamped(cur.StartPos, cur.EndPos, hit.Current.Position);
            curEndPos = Vector3.LerpUnclamped(cur.StartPos, cur.EndPos, hit.Current.Position + hit.Current.Length);

            if (cur.IsLinear)
            {
                AddLine(
                    Vector3.Lerp(preStartPos, curStartPos, endProg) + Vector3.forward * endPos, 
                    Vector3.Lerp(preEndPos, curEndPos, endProg) + Vector3.forward * endPos
                );
            }
            else 
            {
                var pre = Current.LaneSteps[index - 1];
                for (float x = Mathf.Floor(progress * 16 + 1.01f) / 16;; x = Mathf.Min(endProg, Mathf.Floor(x * 16 + 1.01f) / 16))
                {
                    AddLine(
                        new Vector3(
                            Mathf.LerpUnclamped(preStartPos.x, curStartPos.x, cur.StartEaseX.Get(x)), 
                            Mathf.LerpUnclamped(preStartPos.y, curStartPos.y, cur.StartEaseY.Get(x)),
                        Mathf.Lerp(Positions[index - 1], Positions[index], x)), 
                        new Vector3(
                            Mathf.LerpUnclamped(preEndPos.x, curEndPos.x, cur.EndEaseX.Get(x)), 
                            Mathf.LerpUnclamped(preEndPos.y, curEndPos.y, cur.EndEaseY.Get(x)),
                        Mathf.Lerp(Positions[index - 1], Positions[index], x))
                    );
                    if (x >= Mathf.Min(endProg, 1)) break;
                }
            }
            progress = 0;

            ai++;
            if (endProg <= 1 || ai > 1000) break;

            preStartPos = curStartPos;
            preEndPos = curEndPos;
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        hit.HoldMesh.mesh = mesh;
    }

    float InverseLerpUnclamped(float start, float end, float val) => (val - start) / (end - start);
}

[System.Serializable]
public struct HitScreenCoord {
    public Vector2 Position;
    public float Radius;
}