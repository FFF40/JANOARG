using System;
using System.Collections.Generic;
using UnityEngine;

public class CMLanePlayer : MonoBehaviour
{
    public LaneManager CurrentLane;
    public Transform Holder;
    public MeshRenderer Renderer;
    public MeshFilter Filter;

    public List<CMHitPlayer> HitPlayers { get; private set; } = new();

    public void UpdateObjects(LaneManager lane) 
    {
        CurrentLane = lane;
        transform.localPosition = lane.FinalPosition;
        transform.localRotation = lane.FinalRotation;
        Holder.localPosition = Vector3.back * lane.CurrentDistance;
        var styles = PlayerView.main.Manager.PalleteManager.LaneStyles;
        int index = lane.CurrentLane.StyleIndex;
        Renderer.enabled = index >= 0 && index < styles.Count;
        Renderer.sharedMaterial = Renderer.enabled ? styles[index].LaneMaterial : null;
        Filter.sharedMesh = Renderer.enabled ? lane.CurrentMesh : null;
        
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