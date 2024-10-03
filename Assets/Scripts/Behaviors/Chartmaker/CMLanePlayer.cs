using System;
using System.Collections.Generic;
using UnityEngine;

public class CMLanePlayer : MonoBehaviour
{
    public LaneManager CurrentLane;
    public Transform Holder;
    public MeshRenderer Renderer;
    public MeshFilter Filter;
    public MeshRenderer JudgeLine;
    public MeshRenderer[] JudgeEnds;

    public List<CMHitPlayer> HitPlayers { get; private set; } = new();

    public void UpdateObjects(LaneManager lane) 
    {
        CurrentLane = lane;
        transform.SetLocalPositionAndRotation(lane.FinalPosition, lane.FinalRotation);
        Holder.localPosition = Vector3.back * lane.CurrentDistance;
        var styles = PlayerView.main.Manager.PalleteManager.LaneStyles;
        int index = lane.CurrentLane.StyleIndex;
        Renderer.enabled = index >= 0 && index < styles.Count;
        Renderer.sharedMaterial = Renderer.enabled ? styles[index].LaneMaterial : null;
        Filter.sharedMesh = Renderer.enabled ? lane.CurrentMesh : null;

        if (InformationBar.main.sec >= lane.Steps[0].Offset && InformationBar.main.sec < lane.Steps[^1].Offset)
        {
            JudgeLine.gameObject.SetActive(Renderer.enabled);
            JudgeEnds[0].gameObject.SetActive(Renderer.enabled);
            JudgeEnds[1].gameObject.SetActive(Renderer.enabled);
            JudgeLine.sharedMaterial = JudgeEnds[0].sharedMaterial = JudgeEnds[1].sharedMaterial 
                = Renderer.enabled ? styles[index].JudgeMaterial : null;
            JudgeEnds[0].transform.localPosition = lane.StartPosLocal;
            JudgeEnds[1].transform.localPosition = lane.EndPosLocal;
            JudgeLine.transform.localPosition = (lane.StartPosLocal + lane.EndPosLocal) / 2;
            JudgeLine.transform.localScale = new (Vector3.Distance(lane.StartPosLocal, lane.EndPosLocal), .05f, .05f);
            JudgeLine.transform.localEulerAngles = Vector3.back * Vector2.SignedAngle(lane.EndPosLocal - lane.StartPosLocal, Vector2.left);
        }
        else 
        {
            JudgeLine.gameObject.SetActive(false);
            JudgeEnds[0].gameObject.SetActive(false);
            JudgeEnds[1].gameObject.SetActive(false);
        }
        
        int count = 0;
        for (int a = 0; a < lane.Objects.Count; a++)
        {
            if (lane.Objects[a].TimeEnd < InformationBar.main.sec) continue;
            if (lane.Objects[a].Position.z > lane.CurrentDistance + 250) break;
            if (HitPlayers.Count <= count) HitPlayers.Add(Instantiate(PlayerView.main.HitPlayerSample, Holder));
            HitPlayers[count].UpdateObjects(lane.Objects[a]);
            count++;
        }
        while (HitPlayers.Count > count)
        {
            Destroy(HitPlayers[count].gameObject);
            HitPlayers.RemoveAt(count);
        }

    }
}