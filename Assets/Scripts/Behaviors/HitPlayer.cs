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
    [Space]
    public MeshFilter FlickEmblem;

    [Header("Hold")]
    public List<float> Ticks = new List<float>();
    public List<MeshFilter> LaneMeshes;
    public List<float> Positions;
    public List<float> Times;
    public int Index;
    public int IndexShift;
    public float CurrentPos;
    public float CurrentTime;
    public Vector2 ScreenStart;
    public Vector2 ScreenEnd;

    [HideInInspector]
    public bool isHit, isPreHit, isFlicked;
    [HideInInspector]
    public float railTime;

    [HideInInspector]
    public float NoteWeight;

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
                // Debug.Log(start + " " + end);
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
                mf.GetComponent<MeshRenderer>().material = ChartPlayer.main.HitStyleManagers[hit.StyleIndex].HoldTailMaterial;
                pos = nPos;
                sec = nSec;
                LaneMeshes.Add(mf);
                Positions.Add(pos);
                Times.Add(nSec);

                if (end <= 1) break;
            }
        }
        hit.Offset = ChartPlayer.main.Song.Timing.ToSeconds(hit.Offset);

        if (hit.Flickable) 
        {
            FlickEmblem.gameObject.SetActive(true);
            NoteWeight += hit.FlickDirection < 0 ? 1 : 2;
            FlickEmblem.mesh = hit.FlickDirection < 0 ? ChartPlayer.main.FreeFlickEmblem : ChartPlayer.main.DirectionalFlickEmblem;
        }
        
        foreach (Timestamp ts in hit.Storyboard.Timestamps)
        {
            ts.Duration = ChartPlayer.main.Song.Timing.ToSeconds(ts.Time + ts.Duration);
            ts.Time = ChartPlayer.main.Song.Timing.ToSeconds(ts.Time);
            ts.Duration -= ts.Time;
        }

        foreach (MeshRenderer mr in IndicatorMeshes) 
        {
            if (hit.Type == HitObject.HitType.Catch)
                mr.material = ChartPlayer.main.HitStyleManagers[hit.StyleIndex].CatchMaterial;
            else
                mr.material = ChartPlayer.main.HitStyleManagers[hit.StyleIndex].NormalMaterial;
        }

        NoteWeight += hit.Type == HitObject.HitType.Catch ? 1 : 3;
        ChartPlayer.main.TotalScore += NoteWeight + Ticks.Count;
        ChartPlayer.main.TotalCombo += 1 + Ticks.Count;
        ChartPlayer.main.NoteCount += 1 + Ticks.Count;

        if (hit.Type == HitObject.HitType.Catch)
            ChartPlayer.main.CatchHits.Add(this);
        else 
            ChartPlayer.main.NormalHits.Add(this);

        isFlicked = !hit.Flickable;
        CurrentLane = lane;
        CurrentHit = hit;
        UpdateIndicator(CurrentHit.Offset);
    }

    void UpdateIndicator(float time) 
    {

        CurrentLane.GetPosition(time, out Vector3 start, out Vector3 end);

        Vector2 rStart = Vector2.LerpUnclamped(start, end, CurrentHit.Position);
        Vector2 rEnd = Vector2.LerpUnclamped(start, end, CurrentHit.Position + CurrentHit.Length);

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
        if (ChartPlayer.main.IsPlaying)
        {
            if (isHit && isFlicked)
            {
                float time = ChartPlayer.main.CurrentTime;
                UpdateIndicator(ChartPlayer.main.CurrentTime);

                ScreenStart = ChartPlayer.main.MainCamera.WorldToScreenPoint(IndicatorLeft.position);
                ScreenEnd = ChartPlayer.main.MainCamera.WorldToScreenPoint(IndicatorRight.position);
                Vector2 ScreenMid = (ScreenStart + ScreenEnd) / 2;
                float dist = Vector2.Distance(ScreenStart, ScreenEnd) / 2 + Screen.width / 20;

                isPreHit = ChartPlayer.main.AutoPlay;
                if (!isPreHit) foreach (Touch touch in Input.touches) 
                {
                    if (Vector2.Distance(touch.position, ScreenMid) <= dist) 
                    {
                        isPreHit = true;
                        break;
                    }
                }
                railTime = isPreHit ? 1 : railTime - Time.deltaTime * ChartPlayer.main.GoodHitWindow / 1000;
                
                if (isPreHit) while (LaneMeshes.Count > 0) 
                {
                    float t = Mathf.Min((ChartPlayer.main.CurrentTime - Times[Index]) / (Times[Index + 1] - Times[Index]), 1);
                    float m = Mathf.Min((Ticks[Ticks.Count - 1] - Times[Index]) / (Times[Index + 1] - Times[Index]), 1);
                    // Debug.Log(t + " " + m);
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
                        gameObject.SetActive(false);
                        foreach (MeshFilter mesh in LaneMeshes) Destroy(mesh.gameObject);
                        ChartPlayer.main.RemovingHits.Add(this);
                    }

                    // ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.CatchHitSound);

                    if (railTime > 0) 
                    {
                        ChartPlayer.main.AddDiscrete(true);
                        ChartPlayer.main.AddScore(1, 1, true);
                        MakeHitEffect(null, false);
                    }
                    else 
                    {
                        ChartPlayer.main.AddDiscrete(false);
                        ChartPlayer.main.AddScore(1, 0, false);
                    }
                }
            }
        }
        if (CurrentHit.Flickable)
        {
            FlickEmblem.transform.rotation = Quaternion.Euler(0, 0, 
                CurrentHit.FlickDirection < 0 
                ? transform.eulerAngles.z + 90
                : -CurrentHit.FlickDirection
            ) * ChartPlayer.main.MainCamera.transform.rotation;
        }
    }

    public void BeginHit() {
        isFlicked = isHit = true;
        if (Ticks.Count == 0) 
        {
            gameObject.SetActive(false);
            ChartPlayer.main.RemovingHits.Add(this);
        }
    }

    public static float GetAccuracy(float dist) {
        float perfect = ChartPlayer.main.PerfectHitWindow;
        float good = ChartPlayer.main.GoodHitWindow;
        float absDist = Mathf.Abs(dist) * 1000;
        if (absDist < perfect) return 0;
        return (absDist - perfect) / (good - perfect) * Mathf.Sign(dist);
    }

    public void MakeHitEffect(float? accuracy, bool precise = true) 
    {
        HitEffect hje = Instantiate(ChartPlayer.main.HitJudgeEffectSample, ChartPlayer.main.JudgeEffectCanvas);
        hje.Accuracy = accuracy;
        RectTransform rt = hje.GetComponent<RectTransform>();
        Vector2 pos = Vector2.LerpUnclamped(ScreenStart, ScreenEnd, .5f);
        rt.position = new Vector2(pos.x, pos.y);
    }
}
