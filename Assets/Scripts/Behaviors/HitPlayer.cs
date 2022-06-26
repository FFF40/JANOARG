using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPlayer : MonoBehaviour
{
    public LanePlayer CurrentLane;
    public HitObject CurrentHit;
    
    [Header("Data")]
    public Transform Indicator;
    public float Thickness;
    [Space]
    public Transform IndicatorLeft;
    public Transform IndicatorRight;
    public float IndicatorSize;
    [Space]
    public List<MeshRenderer> IndicatorMeshes;

    [Header("Hold")]
    public List<float> Ticks = new List<float>();
    public List<MeshFilter> LaneMeshes;
    public List<float> Positions;
    public List<float> Times;
    public int Index;
    public int IndexShift;
    public float CurrentPos;
    public float CurrentTime;

    bool isHit;

    public void SetHit(LanePlayer lane, HitObject hit)
    {
        if (hit.HoldLength > 0)
        {
            for (float a = .5f; a < hit.HoldLength; a += .5f) 
            {
                Ticks.Add(ChartPlayer.main.Song.Timing.ToSeconds(hit.Offset + a));
            }
            Ticks.Add(ChartPlayer.main.Song.Timing.ToSeconds(hit.Offset + hit.HoldLength));

            float sec = ChartPlayer.main.Song.Timing.ToSeconds(hit.Offset);
            float pos = lane.Positions[0];
            for (int a = 1; a < lane.CurrentLane.LaneSteps.Count; a++) 
            {
                LaneStep prev = lane.CurrentLane.LaneSteps[a - 1];
                float pSec = lane.Times[a - 1];
                LaneStep step = lane.CurrentLane.LaneSteps[a];
                float nSec = lane.Times[a];
                float nPos = pos + prev.Speed * (nSec - pSec);
                
                float start = (sec - pSec) / (nSec - pSec);
                float end = (Ticks[Ticks.Count - 1] - pSec) / (nSec - pSec);
                Debug.Log(start + " " + end);
                if (start >= 1) 
                {
                    IndexShift++;
                    pos = nPos;
                    continue;
                }
                if (Times.Count < 1) 
                {
                    Times.Add(pSec);
                    Positions.Add(pos);
                }

                MeshFilter mf = Instantiate(ChartPlayer.main.LaneMeshSample, lane.Container);
                mf.mesh = LanePlayer.MakeHoldMesh(hit, prev, step, pos * ChartPlayer.main.ScrollSpeed, nPos * ChartPlayer.main.ScrollSpeed, Mathf.Clamp01(start), Mathf.Clamp01(end));
                mf.GetComponent<MeshRenderer>().material = ChartPlayer.main.CurrentChart.HoldMaterial;
                pos = nPos;
                sec = nSec;
                LaneMeshes.Add(mf);
                Positions.Add(pos);
                Times.Add(nSec);

                if (end <= 1) break;
            }
        }
        hit.Offset = ChartPlayer.main.Song.Timing.ToSeconds(hit.Offset);
        
        foreach (Timestamp ts in hit.Storyboard.Timestamps)
        {
            ts.Duration = ChartPlayer.main.Song.Timing.ToSeconds(ts.Time + ts.Duration);
            ts.Time = ChartPlayer.main.Song.Timing.ToSeconds(ts.Time);
            ts.Duration -= ts.Time;
        }

        foreach (MeshRenderer mr in IndicatorMeshes) 
        {
            mr.material = ChartPlayer.main.CurrentChart.HitMaterial;
        }

        ChartPlayer.main.TotalScore += (hit.Type == HitObject.HitType.Catch ? 1 : 3) + Ticks.Count;
        ChartPlayer.main.TotalCombo += 1 + Ticks.Count;
        ChartPlayer.main.NoteCount += 1 + Ticks.Count;

        CurrentLane = lane;
        CurrentHit = hit;
        UpdateIndicator(CurrentHit.Offset);
    }

    void UpdateIndicator(float time) 
    {

        CurrentLane.GetPosition(time, out Vector3 start, out Vector3 end);

        Vector2 rStart = Vector2.Lerp(start, end, CurrentHit.Position);
        Vector2 rEnd = Vector2.Lerp(start, end, CurrentHit.Position + CurrentHit.Length);

        transform.localPosition = (Vector3)(rStart + rEnd) / 2 + Vector3.forward * start.z * ChartPlayer.main.ScrollSpeed;
        Indicator.localScale = new Vector3(Vector3.Distance(rStart, rEnd) - IndicatorSize, Thickness, Thickness);
        Indicator.localRotation = Quaternion.Euler(Vector3.forward * Vector2.SignedAngle(Vector2.right, rEnd - rStart));
        if (IndicatorLeft) 
        {
            IndicatorLeft.localPosition = Indicator.localRotation * Vector3.left * (Indicator.localScale.x / 2 + .2f);
            IndicatorLeft.localRotation = Indicator.localRotation;
            IndicatorLeft.localScale = new Vector3(.1f, Thickness, Thickness);
        }
        if (IndicatorRight) 
        {
            IndicatorRight.localPosition = Indicator.localRotation * Vector3.right * (Indicator.localScale.x / 2 + .2f);
            IndicatorRight.localRotation = Indicator.localRotation;
            IndicatorRight.localScale = new Vector3(.1f, Thickness, Thickness);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float time = ChartPlayer.main.AudioPlayer.isPlaying ? ChartPlayer.main.AudioPlayer.time : ChartPlayer.main.CurrentTime;
        if (!isHit) 
        {
            if (ChartPlayer.main.AutoPlay && time > CurrentHit.Offset)
            {
                if (Ticks.Count == 0) 
                {
                    Destroy(gameObject);
                    Indicator.gameObject.SetActive(false);
                }
                if (CurrentHit.Type == HitObject.HitType.Normal) 
                {
                    float acc = 0; //GetAccuracy(time - CurrentHit.Offset - Time.deltaTime / 2);
                    MakeHitEffect(acc);
                    ChartPlayer.main.AddScore((1 - Mathf.Abs(acc)) * 3, true);
                    ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.NormalHitSound);
                }
                else if (CurrentHit.Type == HitObject.HitType.Catch) 
                {
                    MakeHitEffect(null);
                    ChartPlayer.main.AddScore(1, true);
                    ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.CatchHitSound);
                }
                isHit = true;
            }
            else if (time > CurrentHit.Offset + .2f)
            {
                if (Ticks.Count == 0) 
                {
                    Destroy(gameObject);
                    Indicator.gameObject.SetActive(false);
                }
                ChartPlayer.main.AddScore(0, false);
                isHit = true;
            }
        }
        else 
        {
            UpdateIndicator(ChartPlayer.main.CurrentTime);
            
            while (LaneMeshes.Count > 0) 
            {
                float t = Mathf.Min((ChartPlayer.main.CurrentTime - Times[Index]) / (Times[Index + 1] - Times[Index]), 1);
                float m = Mathf.Min((Ticks[Ticks.Count - 1] - Times[Index]) / (Times[Index + 1] - Times[Index]), 1);
                Debug.Log(t + " " + m);
                if (t < m)
                {
                    CurrentPos = Mathf.LerpUnclamped(Positions[Index], Positions[Index + 1], t);
                    if (t > 0) LaneMeshes[0].mesh = LanePlayer.MakeHoldMesh(CurrentHit, 
                        CurrentLane.CurrentLane.LaneSteps[Index + IndexShift], CurrentLane.CurrentLane.LaneSteps[Index + IndexShift + 1], 
                        Positions[Index] * ChartPlayer.main.ScrollSpeed, Positions[Index + 1] * ChartPlayer.main.ScrollSpeed, 
                        Mathf.Clamp01(t), Mathf.Clamp01(m));
                    break;
                }
                else
                {
                    Destroy(LaneMeshes[0].gameObject);
                    LaneMeshes.RemoveAt(0);
                    Index++;
                }
            }

            while (Ticks.Count > 0 && time > Ticks[0])
            {
                Ticks.RemoveAt(0);
                if (Ticks.Count == 0)
                {
                    Destroy(gameObject);
                    Indicator.gameObject.SetActive(false);
                }
                // ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.CatchHitSound);
                ChartPlayer.main.AddScore(1, true);
                MakeHitEffect(null);
            }
        }
    }

    float GetAccuracy(float dist) {
        float absDist = Mathf.Abs(dist) * 1000;
        if (absDist < 35) return 0;
        else return (absDist) / 200 * Mathf.Sign(dist);
    }

    void MakeHitEffect(float? accuracy) 
    {
        HitEffect hje = Instantiate(ChartPlayer.main.HitJudgeEffectSample, ChartPlayer.main.JudgeEffectCanvas);
        hje.Accuracy = accuracy;
        RectTransform rt = hje.GetComponent<RectTransform>();
        Vector2 pos = ChartPlayer.main.MainCamera.WorldToScreenPoint(transform.position + Vector3.back * transform.position.z);
        rt.position = new Vector2(pos.x, pos.y);
    }
}
