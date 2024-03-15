using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class PlayerScreen : MonoBehaviour
{
    public static PlayerScreen main;

    public static string TargetSongPath;
    public static PlayableSong TargetSong;
    public static ExternalChartMeta TargetChartMeta;
    public static ExternalChart TargetChart;

    public static Chart CurrentChart;

    [Space]
    public AudioSource Music;
    [Space]
    public Transform Holder;
    public GameObject PlayerHUD;
    [Space]
    public TMP_Text SongNameLabel;
    public TMP_Text SongArtistLabel;
    public TMP_Text DifficultyNameLabel;
    public TMP_Text DifficultyLabel;
    [Space]
    public Slider SongProgress;
    public Graphic SongProgressBody;
    public Graphic SongProgressTip;
    public Graphic SongProgressGlow;
    [Space]
    public ScrollingCounter ScoreCounter;
    public List<Graphic> ScoreDigits;
    [Space]
    public TMP_Text JudgmentLabel;
    public TMP_Text ComboLabel;
    [Space]
    [Header("Samples")]
    public LaneGroupPlayer LaneGroupSample;
    public LanePlayer LaneSample;
    public HitPlayer HitSample;
    public MeshRenderer HoldSample;
    [Space]
    [Header("Values")]
    public float Speed = 121;
    [Space]
    public float PerfectWindow = 0.05f;
    public float GoodWindow = 0.2f;
    public float PassWindow = 0.1f;
    [Space]
    public float SyncThreshold = 0.05f;
    [Space]
    [Header("Data")]
    public bool IsReady;
    public bool IsPlaying;
    public float CurrentTime = -5;
    [Space]
    public float TotalExScore = 0;
    public float CurrentExScore = 0;
    [Space]
    public int Combo = 0;
    public int MaxCombo = 0;
    public int TotalCombo = 0;
    [Space]
    public float HitsRemaining = 0;
    
    [HideInInspector]
    public List<LaneStyleManager> LaneStyles = new ();
    [HideInInspector]
    public List<HitStyleManager> HitStyles = new ();
    [HideInInspector]
    public List<LaneGroupPlayer> LaneGroups = new ();
    [HideInInspector]
    public List<LanePlayer> Lanes = new ();

    double lastDSPTime;

    public void Awake() 
    {
        main = this;
        CommonScene.Load();
    }

    public void Start()
    {
        SetInterfaceColor(Color.clear);
        SongProgress.value = 0;
        StartCoroutine(InitChart());
    }

    public IEnumerator InitChart()
    {
        SongNameLabel.text = TargetSong.SongName;
        SongArtistLabel.text = TargetSong.SongArtist;
        DifficultyNameLabel.text = TargetChartMeta.DifficultyName;
        DifficultyLabel.text = TargetChartMeta.DifficultyLevel;

        string path = Path.Combine(Path.GetDirectoryName(TargetSongPath), TargetChartMeta.Target);
        Debug.Log(path);
        ResourceRequest thing = Resources.LoadAsync<ExternalChart>(path);
        
        Music.clip = TargetSong.Clip;
        Music.clip.LoadAudioData();

        yield return new WaitUntil(() => thing.isDone && Music.clip.loadState != AudioDataLoadState.Loading);

        TargetChart = thing.asset as ExternalChart;
        CurrentChart = TargetChart.Data.DeepClone();

        foreach (LaneStyle style in CurrentChart.Pallete.LaneStyles) LaneStyles.Add(new(style));
        foreach (HitStyle style in CurrentChart.Pallete.HitStyles) HitStyles.Add(new(style));

        for (int a = 0; a < TargetChart.Data.Groups.Count; a++)
        {
            LaneGroupPlayer player = Instantiate(LaneGroupSample, Holder);
            player.Original = TargetChart.Data.Groups[a];
            player.Current = CurrentChart.Groups[a];
            player.gameObject.name = player.Current.Name;
            LaneGroups.Add(player);
            yield return new WaitForEndOfFrame();
        }
        for (int a = 0; a < LaneGroups.Count; a++)
        {
            LaneGroupPlayer player = LaneGroups[a];
            if (!string.IsNullOrEmpty(player.Current.Group))
            player.transform.SetParent(LaneGroups.Find(x => x.Current.Name == player.Current.Group).transform);
        }

        for (int a = 0; a < TargetChart.Data.Lanes.Count; a++)
        {
            LanePlayer player = Instantiate(LaneSample, Holder);
            player.Original = TargetChart.Data.Lanes[a];
            player.Current = CurrentChart.Lanes[a];
            if (!string.IsNullOrEmpty(player.Current.Group)) 
                player.transform.SetParent(LaneGroups.Find(x => x.Current.Name == player.Current.Group).transform);
            player.Init();
            Lanes.Add(player);
            foreach (HitObject hit in player.Original.Objects)
            {
                TotalExScore += (hit.Type == HitObject.HitType.Normal ? 3 : 1);
                TotalExScore += Mathf.Ceil(hit.HoldLength / 0.5f);
                if (hit.Flickable)
                {
                    TotalExScore += 1;
                    if (!float.IsNaN(hit.FlickDirection)) TotalExScore += 1;
                }
            }
            HitsRemaining += player.Original.Objects.Count;
            yield return new WaitForEndOfFrame();
        }

        IsReady = true;
        lastDSPTime = AudioSettings.dspTime;
    }

    public void BeginReadyAnim() 
    {
        StartCoroutine(ReadyAnim());
    }

    public IEnumerator ReadyAnim() 
    {
        yield return Ease.Animate(1, (x) => {
            float ease = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
            SetInterfaceColor(TargetSong.InterfaceColor * new Color(1, 1, 1, ease));
            PlayerHUD.transform.localScale = Vector3.one * (ease * .1f + .9f);
        });
        IsPlaying = true;
        lastDSPTime = AudioSettings.dspTime;
    }

    public void Update()
    {
        if (IsPlaying)
        {
            double delta = System.Math.Min(AudioSettings.dspTime - lastDSPTime, PerfectWindow);
            CurrentTime += (float)delta;
            lastDSPTime += delta;
            
            // Audio sync
            if (CurrentTime >= 0 && CurrentTime < Music.clip.length)
            {
                if (Music.isPlaying) 
                {
                    if (Mathf.Abs(CurrentTime - (float)Music.timeSamples / Music.clip.frequency) > SyncThreshold)
                    {
                        Music.time = CurrentTime;
                    }
                    else 
                    {
                        CurrentTime = (float)Music.timeSamples / Music.clip.frequency;
                    }
                }
                else
                {
                    Music.Play();
                    Music.time = CurrentTime;
                }
            }
            else 
            {
                if (Music.isPlaying) Music.Pause();
            }
            
            // Update song progress slider
            SongProgress.value = CurrentTime / Music.clip.length;

            float beat = TargetSong.Timing.ToBeat(CurrentTime);
            
            // Update palette
            CurrentChart.Pallete.Advance(beat);
            if (Common.main.MainCamera.backgroundColor != CurrentChart.Pallete.BackgroundColor) SetBackgroundColor(CurrentChart.Pallete.BackgroundColor);
            if (SongNameLabel.color != CurrentChart.Pallete.InterfaceColor) SetInterfaceColor(CurrentChart.Pallete.InterfaceColor);
            for (int a = 0; a < LaneStyles.Count; a++)
            {
                CurrentChart.Pallete.LaneStyles[a].Advance(beat);
                LaneStyles[a].Update(CurrentChart.Pallete.LaneStyles[a]);
            }
            for (int a = 0; a < HitStyles.Count; a++)
            {
                CurrentChart.Pallete.HitStyles[a].Advance(beat);
                HitStyles[a].Update(CurrentChart.Pallete.HitStyles[a]);
            }

            // Update camera
            CurrentChart.Camera.Advance(beat);
            Camera camera = Common.main.MainCamera;
            camera.transform.position = CurrentChart.Camera.CameraPivot;
            camera.transform.eulerAngles = CurrentChart.Camera.CameraRotation;
            camera.transform.Translate(Vector3.back * CurrentChart.Camera.PivotDistance);

            // Update scene
            foreach (LaneGroupPlayer group in LaneGroups) group.UpdateSelf(CurrentTime, beat);
            foreach (LanePlayer lane in Lanes) lane.UpdateSelf(CurrentTime, beat);
            
            // Check hit objects
            CheckHitObjects();
        }
    }

    public void CheckHitObjects() 
    {
        foreach (LanePlayer lane in Lanes)
        {
            for (int a = 0; a < lane.HitObjects.Count; a++)
            {
                HitPlayer hit = lane.HitObjects[a];
                if (hit.HoldMesh) 
                {
                    lane.UpdateHoldMesh(hit);
                }
            }
            for (int a = 0; a < lane.HitObjects.Count; a++)
            {
                HitPlayer hit = lane.HitObjects[a];
                if (hit.IsHit)
                {
                    while (hit.HoldTicks.Count > 0 && hit.HoldTicks[0] <= CurrentTime) 
                    {
                        AddScore(1, null);
                        hit.HoldTicks.RemoveAt(0);
                    }
                    if (hit.HoldTicks.Count <= 0)
                    {
                        RemoveHitPlayer(hit);
                        a--;
                    }
                }
                else if (hit.Time <= CurrentTime)
                {
                    Hit(hit, 0);
                    a--;
                }
                else 
                {
                    break;
                }
            }
        }
    }

    public void AddScore(float score, float? acc)
    {
        CurrentExScore += score;
        ScoreCounter.SetNumber(Mathf.RoundToInt(CurrentExScore / TotalExScore * 1e6f));

        if (score > 0) 
        {
            Combo++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);
        }
        else 
        {
            Combo = 0;
        }
        TotalCombo++;
        ComboLabel.text = Combo.ToString("0000");
    }

    public void RemoveHitPlayer(HitPlayer hit) 
    {
        if (hit.HoldMesh)
        {
            Destroy(hit.HoldMesh.mesh);
            Destroy(hit.HoldMesh.gameObject);
        }
        Destroy(hit.gameObject);
        hit.Lane.HitObjects.Remove(hit);

        HitsRemaining--;
        if (HitsRemaining <= 0) 
        {
            ResultScreen.main.StartEndingAnim();
        }
    }

    public void Hit(HitPlayer hit, float offset)
    {
        int score = hit.Current.Type == HitObject.HitType.Normal ? 3 : 1;
        if (hit.Current.Flickable)
        {
            score += 1;
            if (!float.IsNaN(hit.Current.FlickDirection))   score += 1;
        }
        
        AddScore(score, 0);

        if (hit.Time == hit.EndTime)
        {
            RemoveHitPlayer(hit);
        }
        else 
        {
            hit.IsHit = true;
        }
    }
    public void SetBackgroundColor(Color color) 
    {
        Common.main.MainCamera.backgroundColor = color;
        RenderSettings.fogColor = color;
    }

    public void SetInterfaceColor(Color color) 
    {
        SongNameLabel.color = SongArtistLabel.color = DifficultyLabel.color = DifficultyNameLabel.color = 
            SongProgressTip.color = SongProgressGlow.color = JudgmentLabel.color = ComboLabel.color = color;
        foreach (Graphic g in ScoreDigits) g.color = color;
        SongProgressBody.color = color * new Color (1, 1, 1, .5f);
    }
}