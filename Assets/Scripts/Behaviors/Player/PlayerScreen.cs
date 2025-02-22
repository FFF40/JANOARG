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
    public Camera Pseudocamera;
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
    public RectTransform JudgeScreenHolder;
    [Space]
    public TMP_Text PauseLabel;
    [Space]
    [Header("Samples")]
    public LaneGroupPlayer LaneGroupSample;
    public LanePlayer LaneSample;
    public HitPlayer HitSample;
    public MeshRenderer HoldSample;
    public JudgeScreenEffect JudgeScreenSample;
    [Space]
    public Mesh FreeFlickIndicator;
    public Mesh ArrowFlickIndicator;
    [Header("Values")]
    public float Speed = 121;
    [Space]
    public float PerfectWindow = 0.05f;
    public float GoodWindow = 0.15f;
    public float BadWindow = 0.2f;
    public float PassWindow = 0.1f;
    [Space]
    public float SyncThreshold = 0.05f;
    [Space]
    [Header("Data")]
    public bool IsReady;
    public bool IsPlaying;
    public bool HasPlayedBefore;
    public float CurrentTime = -5;
    [Space]
    public float TotalExScore = 0;
    public float CurrentExScore = 0;
    [Space]
    public int PerfectCount = 0;
    public int GoodCount = 0;
    public int BadCount = 0;
    [Space]
    public int Combo = 0;
    public int MaxCombo = 0;
    public int TotalCombo = 0;
    [Space]
    public int HitsRemaining = 0;
    
    [HideInInspector]
    public List<LaneStyleManager> LaneStyles = new ();
    [HideInInspector]
    public List<HitStyleManager> HitStyles = new ();
    [HideInInspector]
    public List<LaneGroupPlayer> LaneGroups = new ();
    [HideInInspector]
    public List<LanePlayer> Lanes = new ();

    double lastDSPTime;

    public PlayerSettings Settings = new();

    public void Awake() 
    {
        main = this;
        CommonScene.Load();
    }

    public void Start()
    {
        InitFlickMeshes();
        SetInterfaceColor(Color.clear);
        SongProgress.value = 0;
        StartCoroutine(LoadChart());
    }

    public IEnumerator LoadChart()
    {
        SongNameLabel.text = TargetSong.SongName;
        SongArtistLabel.text = TargetSong.SongArtist;
        DifficultyNameLabel.text = TargetChartMeta.DifficultyName;
        DifficultyLabel.text = TargetChartMeta.DifficultyLevel;

        string path = Path.Combine(Path.GetDirectoryName(TargetSongPath), TargetChartMeta.Target);
        Debug.Log(path);
        ResourceRequest thing = Resources.LoadAsync<ExternalChart>(path);
        
        TargetSong.Clip.LoadAudioData();

        yield return new WaitUntil(() => thing.isDone && TargetSong.Clip.loadState != AudioDataLoadState.Loading);

        TargetChart = thing.asset as ExternalChart;

        yield return InitChart();
    }

    public IEnumerator InitChart()
    {
        CurrentChart = TargetChart.Data.DeepClone();

        if (HasPlayedBefore) 
        {
            TotalExScore = CurrentExScore = 0;
            PerfectCount = GoodCount = BadCount = Combo = MaxCombo = TotalCombo = HitsRemaining = 0;
            ScoreCounter.SetNumber(0);
            SongProgress.value = 0;

            for (int a = 0; a < LaneStyles.Count; a++) LaneStyles[a].Update(CurrentChart.Palette.LaneStyles[a]);
            for (int a = 0; a < HitStyles.Count; a++) HitStyles[a].Update(CurrentChart.Palette.HitStyles[a]);
            for (int a = 0; a < TargetChart.Data.Groups.Count; a++) LaneGroups[a].Current = CurrentChart.Groups[a];
            foreach (LanePlayer lane in Lanes) Destroy(lane.gameObject);
            Lanes.Clear();
        } 
        else 
        {
            foreach (LaneStyle style in CurrentChart.Palette.LaneStyles) LaneStyles.Add(new(style));
            foreach (HitStyle style in CurrentChart.Palette.HitStyles) HitStyles.Add(new(style));

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
                if (string.IsNullOrEmpty(player.Current.Group)) continue;
                player.Parent = LaneGroups.Find(x => x.Current.Name == player.Current.Group);
                player.transform.SetParent(player.Parent.transform);
            }

        }

        float dpi = (Screen.dpi == 0 ? 100 : Screen.dpi);

        for (int a = 0; a < TargetChart.Data.Lanes.Count; a++)
        {
            LanePlayer player = Instantiate(LaneSample, Holder);
            player.Original = TargetChart.Data.Lanes[a];
            player.Current = CurrentChart.Lanes[a];
            LaneGroupPlayer group = null;
            if (!string.IsNullOrEmpty(player.Current.Group)) 
            {
                group = LaneGroups.Find(x => x.Current.Name == player.Current.Group);
                player.transform.SetParent(group.transform);
                player.Group = group;
            }
            player.Init();
            Lanes.Add(player);

            float time = float.NaN;
            Vector3 startPos = Vector3.zero, endPos = Vector3.zero;
            foreach (HitObject hit in player.Original.Objects)
            {
                TotalExScore += hit.Type == HitObject.HitType.Normal ? 3 : 1;
                TotalExScore += Mathf.Ceil(hit.HoldLength / 0.5f);
                if (hit.Flickable)
                {
                    TotalExScore += 1;
                    if (!float.IsNaN(hit.FlickDirection)) TotalExScore += 1;
                }

                if (time != hit.Offset) 
                {
                    CameraController camera = (CameraController)TargetChart.Data.Camera.Get(hit.Offset);
                    Pseudocamera.transform.position = camera.CameraPivot;
                    Pseudocamera.transform.eulerAngles = camera.CameraRotation;
                    Pseudocamera.transform.Translate(Vector3.back * camera.PivotDistance);

                    Lane lane = (Lane)player.Original.Get(hit.Offset);
                    LanePosition step = lane.GetLanePosition(hit.Offset, hit.Offset, TargetSong.Timing);
                    startPos = Quaternion.Euler(lane.Rotation) * step.StartPos + lane.Position;
                    endPos = Quaternion.Euler(lane.Rotation) * step.EndPos + lane.Position;
                    LaneGroupPlayer gp = group;
                    while (gp) 
                    {
                        LaneGroup laneGroup = (LaneGroup)gp.Original.Get(hit.Offset);
                        startPos = Quaternion.Euler(laneGroup.Rotation) * startPos + laneGroup.Position;
                        endPos = Quaternion.Euler(laneGroup.Rotation) * endPos + laneGroup.Position;
                        gp = gp.Parent;
                    }
                }

                HitObject h = (HitObject)hit.Get(hit.Offset);
                Vector2 hitStart = Pseudocamera.WorldToScreenPoint(Vector3.Lerp(startPos, endPos, h.Position));
                Vector2 hitEnd = Pseudocamera.WorldToScreenPoint(Vector3.Lerp(startPos, endPos, h.Position + hit.Length));
                player.HitCoords.Add(new HitScreenCoord {
                    Position = (hitStart + hitEnd) / 2,
                    Radius = Vector2.Distance(hitStart, hitEnd) / 2 + dpi * .2f,
                });
            }

            HitsRemaining += player.Original.Objects.Count;

            yield return new WaitForEndOfFrame();
        }

        Music.clip = TargetSong.Clip;
        CurrentTime = -5;
        IsReady = true;
        HasPlayedBefore = true;
        lastDSPTime = AudioSettings.dspTime;
    }

    public void BeginReadyAnim() 
    {
        StartCoroutine(ReadyAnim());
    }

    public IEnumerator ReadyAnim() 
    {
        for (int a = 0; a < ScoreCounter.Digits.Count; a++)
        {
            ScoreCounter.Digits[a].List.Clear();
            ScoreCounter.Digits[a].Speed = 3;
            for (int b = 9 - a; b <= 10; b++) 
            {
                ScoreCounter.Digits[a].List.Add((b % 10).ToString());
            }
        }
        yield return Ease.Animate(1.5f, (x) => {
            float ease = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
            SetInterfaceColor(TargetSong.InterfaceColor * new Color(1, 1, 1, ease));
            PlayerHUD.transform.localScale = Vector3.one * (ease * .05f + .95f);
        });
        for (int a = 0; a < ScoreCounter.Digits.Count; a++)
        {
            ScoreCounter.Digits[a].Speed = 9;
        }
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
            
            // Check hit objects
            CheckHitObjects();
            
            // Update song progress slider
            SongProgress.value = CurrentTime / Music.clip.length;

            float visualTime = CurrentTime + Settings.VisualOffset;
            float visualBeat = TargetSong.Timing.ToBeat(visualTime);
            
            // Update palette
            CurrentChart.Palette.Advance(visualBeat);
            if (Common.main.MainCamera.backgroundColor != CurrentChart.Palette.BackgroundColor) SetBackgroundColor(CurrentChart.Palette.BackgroundColor);
            if (SongNameLabel.color != CurrentChart.Palette.InterfaceColor) SetInterfaceColor(CurrentChart.Palette.InterfaceColor);
            for (int a = 0; a < LaneStyles.Count; a++)
            {
                CurrentChart.Palette.LaneStyles[a].Advance(visualBeat);
                LaneStyles[a].Update(CurrentChart.Palette.LaneStyles[a]);
            }
            for (int a = 0; a < HitStyles.Count; a++)
            {
                CurrentChart.Palette.HitStyles[a].Advance(visualBeat);
                HitStyles[a].Update(CurrentChart.Palette.HitStyles[a]);
            }

            // Update camera
            CurrentChart.Camera.Advance(visualBeat);
            Camera camera = Common.main.MainCamera;
            camera.transform.position = CurrentChart.Camera.CameraPivot;
            camera.transform.eulerAngles = CurrentChart.Camera.CameraRotation;
            camera.transform.Translate(Vector3.back * CurrentChart.Camera.PivotDistance);

            // Update scene
            foreach (LaneGroupPlayer group in LaneGroups) group.UpdateSelf(visualTime, visualBeat);
            foreach (LanePlayer lane in Lanes) lane.UpdateSelf(visualTime, visualBeat);
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
        }
        PlayerInputManager.main.UpdateTouches();
    }

    public void AddScore(float score, float? acc)
    {
        CurrentExScore += score;
        ScoreCounter.SetNumber(Mathf.RoundToInt(CurrentExScore / TotalExScore * 1e6f));

        if (score > 0) 
        {
            Combo++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);
            if (acc == null || acc == 0) PerfectCount++;
            else GoodCount++;
        }
        else 
        {
            Combo = 0;
            BadCount++;
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

    public void Hit(HitPlayer hit, float offset, bool spawnEffect = true)
    {
        int score = hit.Current.Type == HitObject.HitType.Normal ? 3 : 1;
        if (hit.Current.Flickable)
        {
            score += 1;
            if (!float.IsNaN(hit.Current.FlickDirection)) score += 1;
        }
        
        float offsetAbs = Mathf.Abs(offset);
        float? acc = null;
        if (hit.Current.Flickable || hit.Current.Type == HitObject.HitType.Catch)
        {
            if (offsetAbs <= PassWindow) AddScore(score, null);
            else AddScore(0, null);
        }
        else 
        {
            acc = 0;
            if (offsetAbs > GoodWindow) acc = Mathf.Sign(offset);
            else if (offsetAbs > PerfectWindow) acc = Mathf.Sign(offset) * Mathf.InverseLerp(PerfectWindow, GoodWindow, offsetAbs);
            AddScore(score * (1 - (float)acc), acc);
        }

        if (spawnEffect)
        {
            var effect = Instantiate(JudgeScreenSample, JudgeScreenHolder);
            effect.SetAccuracy(acc);
            effect.SetColor(CurrentChart.Palette.InterfaceColor);
            var rt = (RectTransform)effect.transform;
            rt.position = hit.HitCoord.Position;
        }

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
            SongProgressTip.color = SongProgressGlow.color = JudgmentLabel.color = ComboLabel.color = 
            PauseLabel.color = color;
        foreach (Graphic g in ScoreDigits) g.color = color;
        SongProgressBody.color = color * new Color (1, 1, 1, .5f);
    }

    public void InitFlickMeshes() 
    {
        if (!FreeFlickIndicator) 
        {
            Mesh mesh = new();
            List<Vector3> verts = new();
            List<int> tris = new();

            verts.AddRange(new Vector3[] { new(-1, 0), new(0, 2), new(0, -.5f), new(1, 0), new(0, -2), new(0, .5f) });
            tris.AddRange(new [] {0, 1, 2, 3, 4, 5});

            mesh.SetVertices(verts);
            mesh.SetUVs(0, verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            FreeFlickIndicator = mesh;
        }
        if (!ArrowFlickIndicator) 
        {
            Mesh mesh = new();
            List<Vector3> verts = new();
            List<int> tris = new();

            verts.AddRange(new Vector3[] { new(-1, 0), new(0, 2.2f), new(1, 0), new(.71f, -.71f), new(0, -1), new(-.71f, -.71f) });
            tris.AddRange(new [] {0, 1, 2, 2, 3, 0, 3, 4, 0, 4, 5, 0});

            mesh.SetVertices(verts);
            mesh.SetUVs(0, verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            ArrowFlickIndicator = mesh;
        }
    }
}

public class PlayerSettings 
{
    public float JudgmentOffset;
    public float VisualOffset;

    public PlayerSettings ()
    {
        Storage prefs = Common.main.Preferences;
        JudgmentOffset = prefs.Get("PLYR_JudgmentOffset", 0f) / 1000;
        VisualOffset = prefs.Get("PLYR_VisualOffset", 0f) / 1000;
    }
}