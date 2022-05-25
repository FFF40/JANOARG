using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPlayer : MonoBehaviour
{
    public LanePlayer CurrentLane;
    public HitObject CurrentHit;

    public Transform Indicator;
    public MeshRenderer IndicatorMesh;

    public void SetHit(LanePlayer lane, HitObject hit)
    {
        
        hit.Offset = ChartPlayer.main.Song.Timing.ToSeconds(hit.Offset);
        
        foreach (Timestamp ts in hit.Storyboard.Timestamps)
        {
            ts.Duration = ChartPlayer.main.Song.Timing.ToSeconds(ts.Time + ts.Duration);
            ts.Time = ChartPlayer.main.Song.Timing.ToSeconds(ts.Time);
            ts.Duration -= ts.Time;
        }

        lane.GetPosition(hit.Offset, out Vector3 start, out Vector3 end);
        Debug.Log(start + " " + end);

        Vector2 rStart = Vector2.Lerp(start, end, hit.Position);
        Vector2 rEnd = Vector2.Lerp(start, end, hit.Position + hit.Length);

        transform.localPosition = (Vector3)(rStart + rEnd) / 2 + Vector3.forward * start.z * 120;
        Indicator.localScale = new Vector3(Vector3.Distance(rStart, rEnd), .2f, .2f);
        Indicator.localRotation = Quaternion.Euler(Vector3.forward * Vector2.SignedAngle(Vector2.right, rEnd - rStart));
        IndicatorMesh.material = ChartPlayer.main.CurrentChart.HitMaterial;

        ChartPlayer.main.MaxScore += 3;

        CurrentLane = lane;
        CurrentHit = hit;
    }

    // Update is called once per frame
    void Update()
    {
        float time = ChartPlayer.main.AudioPlayer.isPlaying ? ChartPlayer.main.AudioPlayer.time : ChartPlayer.main.CurrentTime;
        if (ChartPlayer.main.AutoPlay && time > CurrentHit.Offset)
        {
            Destroy(gameObject);
            Indicator.gameObject.SetActive(false);
            ChartPlayer.main.AddScore(3, true);
        }
        else if (time > CurrentHit.Offset + .2f)
        {
            Destroy(gameObject);
            Indicator.gameObject.SetActive(false);
            ChartPlayer.main.AddScore(0, false);
        }
    }
}
