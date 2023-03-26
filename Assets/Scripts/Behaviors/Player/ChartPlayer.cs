using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChartPlayer : MonoBehaviour
{
    public static ChartPlayer main;

    public static string MetaSongPath;
    public static int MetaChartPosition;

    [Header("Data")]
    public string SongPath;
    [HideInInspector]
    public PlayableSong Song;
    public int ChartPosition;
    [Space]
    public bool IsPlaying;
    public bool AutoPlay;
    public float CurrentTime;
    [Space]
    public float Score;
    public float TotalScore;
    public float Gauge;
    [Space]
    public int PerfectCount;
    public int GoodCount;
    public int BadCount;
    public int[] AccuracyCounts = new int[5];
    public int[] DiscreteCounts = new int[2];
    [Space]
    public int Combo;
    public int MaxCombo;
    public int TotalCombo;
    public int NoteCount;
    [Space]
    public float OffsetMean;
    public float Deviation;
    public int[] AccuracyValues = new int[65];

    [Header("Settings")]
    public float ScrollSpeed = 120;
    public float SyncThreshold = .05f;
    [Space]
    public float PerfectHitWindow = 35;
    public float GoodHitWindow = 200;
    public float BadHitWindow = 250;

    [Header("Objects")]
    public LanePlayer LanePlayerSample;
    public MeshFilter LaneMeshSample;
    public HitPlayer NormalHitSample;
    public HitPlayer CatchHitSample;
    public HitEffect HitJudgeEffectSample;
    [Space]
    public Camera MainCamera;
    public Transform JudgeEffectCanvas;

    [Header("Sounds")]
    public AudioSource AudioPlayer;
    public AudioClip NormalHitSound;
    public AudioClip CatchHitSound;

    [Header("Play Interface")]
    public GameObject PlayInterfaceObject;
    public CanvasGroup PlayInterfaceGroup;
    [Space]
    public GameObject SideObject;
    public CanvasGroup SideGroup;
    public GameObject CenterObject;
    public CanvasGroup CenterGroup;
    [Space]
    public TMP_Text SongNameLabel;
    public TMP_Text SongArtistLabel;
    public TMP_Text DifficultyNameLabel;
    public List<Image> DifficultyIndicators;
    public TMP_Text DifficultyLabel;
    [Space]
    public TMP_Text ScoreText;
    public List<TMP_Text> ScoreDigits;
    public TMP_Text ScoreUnitText;
    [Space]
    public Slider SongProgressSlider;
    public Image SongProgressFill;
    public Image SongProgressUpper;
    public Image SongProgressLower;
    [Space]
    public TMP_Text GaugeText;
    public Slider GaugeSlider;
    public Image GaugeFill;
    public Image GaugeUpper;
    public Image GaugeLower;
    [Space]
    public CanvasGroup ComboGroup;
    public TMP_Text ComboLabel;
    public TMP_Text ComboText;
    [Space]
    public Sidebar PauseSidebar;
    public TMP_Text PauseLabel;
    [Space]
    public Image AbortBackground;
    [Space]
    public float AudioOffset;
    public float VisualOffset;
    public float[] HitVolume;
    public float[] HitSize;
    public float[] HitEffectAlpha;
    public float[] HitEffectSize;


    [HideInInspector]
    public Chart CurrentChart;
    [HideInInspector]
    public List<LaneStyleManager> LaneStyleManagers = new List<LaneStyleManager>();
    [HideInInspector]
    public List<HitStyleManager> HitStyleManagers = new List<HitStyleManager>();

    
    [HideInInspector]
    public List<HitPlayer> NormalHits = new List<HitPlayer>();
    [HideInInspector]
    public List<HitPlayer> CatchHits = new List<HitPlayer>();
    [HideInInspector]
    public List<HitPlayer> RemovingHits = new List<HitPlayer>();
    [HideInInspector]
    public Dictionary<int, FlickHandle> FlickDirections = new Dictionary<int, FlickHandle>();


    [HideInInspector]
    public float InitProgress = 0;

    [HideInInspector]
    public Mesh FreeFlickEmblem;
    [HideInInspector]
    public Mesh DirectionalFlickEmblem;

    bool isAnimating;
    float precTime;
    int normalHitCount;

    float PauseTime;

    public void Awake()
    {
        main = this;
        CommonScene.Load();
    }

    void OnDestroy()
    {
        main = main == this ? null : main;
    }

    public void NormalizeTimestamp(Timestamp ts)
    {
        ts.Duration = Song.Timing.ToSeconds(ts.Offset + ts.Duration);
        ts.Offset = Song.Timing.ToSeconds(ts.Offset);
        ts.Duration -= ts.Offset;
    }

    public void Start()
    {
        if (!MainCamera) MainCamera = Camera.main;

        if (!string.IsNullOrWhiteSpace(MetaSongPath))
        {
            SongPath = MetaSongPath;
            ChartPosition = MetaChartPosition;
        }

        StartCoroutine(InitChart());

        float camRatio = Mathf.Min(1, (3f * Screen.width) / (2f * Screen.height));
        MainCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;

        UpdateScore();
    }

    public IEnumerator InitChart()
    {
        long time = System.DateTime.Now.Ticks;
        long t;
        ResourceRequest req;

        AudioOffset = Common.main.Storage.Get("PREF:AudioOffset", 0f) / 1000;
        VisualOffset = Common.main.Storage.Get("PREF:VisualOffset", 0f) / 1000;
        AudioPlayer.volume = Common.main.Storage.Get("PREF:MusicVolume", 100f) / 100;
        SideGroup.alpha = Common.main.Storage.Get("PREF:PlayerSideAlpha", 100f) / 100;
        CenterGroup.alpha = Common.main.Storage.Get("PREF:PlayerCenterAlpha", 50f) / 100;

        HitSize = Common.main.Storage.Get("PREF:NoteSize", new [] { 1f });
        if (HitSize.Length < 2) HitSize = new []{ HitSize[0], HitSize[0] };

        HitVolume = Common.main.Storage.Get("PREF:HitVolume", new [] { 80f });
        if (HitVolume.Length < 3) HitVolume = new []{ HitVolume[0], HitVolume[0], HitVolume[0] };
        for (int a = 0; a < HitVolume.Length; a++) HitVolume[a] /= 100;
        HitEffectAlpha = Common.main.Storage.Get("PREF:PlayerHitAlpha", new [] { 100f });
        if (HitEffectAlpha.Length < 3) HitEffectAlpha = new []{ HitEffectAlpha[0], HitEffectAlpha[0], HitEffectAlpha[0] };
        for (int a = 0; a < HitEffectAlpha.Length; a++) HitEffectAlpha[a] /= 100;
        HitEffectSize = Common.main.Storage.Get("PREF:PlayerHitFactor", new [] { 1f });
        if (HitEffectSize.Length < 3) HitEffectSize = new []{ HitEffectSize[0], HitEffectSize[0], HitEffectSize[0] };

        req = Resources.LoadAsync(SongPath);
        yield return new WaitUntil(() => req.isDone);
        Song = Instantiate((PlayableSong)req.asset);

        req = Resources.LoadAsync((System.IO.Path.GetDirectoryName(SongPath).Replace('\\', '/') + "/" + Song.Charts[ChartPosition].Target));
        yield return new WaitUntil(() => req.isDone);
        CurrentChart = Instantiate(((ExternalChart)req.asset)).Data;

        SongNameLabel.text = Song.SongName;
        SongArtistLabel.text = Song.SongArtist;
        DifficultyNameLabel.text = CurrentChart.DifficultyName;
        DifficultyLabel.text = CurrentChart.DifficultyLevel;
        if (DifficultyLabel.text.EndsWith("*")) DifficultyLabel.text = 
            DifficultyLabel.text.Remove(DifficultyLabel.text.Length - 1) + "<sup>*";
        AudioPlayer.clip = Song.Clip;

        RenderSettings.fogColor = MainCamera.backgroundColor = CurrentChart.Pallete.BackgroundColor;
        SetInterfaceColor(CurrentChart.Pallete.InterfaceColor);

        if (!FreeFlickEmblem) FreeFlickEmblem = MakeFreeFlickEmblem();
        if (!DirectionalFlickEmblem) DirectionalFlickEmblem = MakeDirectionalFlickEmblem();

        foreach (Timestamp ts in CurrentChart.Camera.Storyboard.Timestamps)
        {
            NormalizeTimestamp(ts);
            t = System.DateTime.Now.Ticks;
            if (t - time > 33e4)
            {
                time = t;
                yield return null;
            }
        }
        
        foreach (Timestamp ts in CurrentChart.Pallete.Storyboard.Timestamps)
        {
            NormalizeTimestamp(ts);
            t = System.DateTime.Now.Ticks;
            if (t - time > 33e4)
            {
                time = t;
                yield return null;
            }
        }

        foreach (LaneStyle style in CurrentChart.Pallete.LaneStyles)
        {
            LaneStyleManagers.Add(new LaneStyleManager(style));
            foreach (Timestamp ts in style.Storyboard.Timestamps)
                NormalizeTimestamp(ts);
            t = System.DateTime.Now.Ticks;
            if (t - time > 33e4)
            {
                time = t;
                yield return null;
            }
        }

        foreach (HitStyle style in CurrentChart.Pallete.HitStyles)
        {
            HitStyleManagers.Add(new HitStyleManager(style));
            foreach (Timestamp ts in style.Storyboard.Timestamps)
                NormalizeTimestamp(ts);
            t = System.DateTime.Now.Ticks;
            if (t - time > 33e4)
            {
                time = t;
                yield return null;
            }
        }

        SongProgressSlider.value = 0;
        Dictionary<string, LaneGroupPlayer> groups = new Dictionary<string, LaneGroupPlayer>();
        foreach (LaneGroup group in CurrentChart.Groups) {
            LaneGroupPlayer gp = new GameObject(group.Name).AddComponent<LaneGroupPlayer>();
            gp.transform.parent = transform;
            gp.SetGroup(group);
            groups.Add(group.Name, gp);
            t = System.DateTime.Now.Ticks;
            if (t - time > 33e4)
            {
                time = t;
                yield return null;
            }
            SongProgressSlider.value += 1 / CurrentChart.Groups.Count;
        }
        foreach (KeyValuePair<string, LaneGroupPlayer> pair in groups) {
            string parent = pair.Value.CurrentGroup.Group;
            if (!string.IsNullOrEmpty(parent))
            {
                pair.Value.transform.SetParent(groups[parent].transform);
                pair.Value.ParentGroup = groups[parent];
            }
            t = System.DateTime.Now.Ticks;
            if (t - time > 33e4)
            {
                time = t;
                yield return null;
            }
        }

        Debug.Log(string.Join(", ", groups.Keys));

        SongProgressSlider.value = 0;
        foreach (Lane lane in CurrentChart.Lanes) {
            LanePlayer lp = Instantiate(LanePlayerSample, transform);
            if (!string.IsNullOrEmpty(lane.Group))
            {
                lp.transform.SetParent(groups[lane.Group].transform);
                lp.ParentGroup = groups[lane.Group];
            }
            lp.SetLane(lane);
            if (lp.Ready) 
            {
                t = System.DateTime.Now.Ticks;
                if (t - time > 33e4)
                {
                    time = t;
                    yield return null;
                }
            }
            else 
            {
                yield return new WaitUntil(() => lp.Ready);
            }
            SongProgressSlider.value += 1 / CurrentChart.Lanes.Count;
        }
        
        NormalHits.Sort((x, y) => x.CurrentHit.Offset.CompareTo(y.CurrentHit.Offset));
        t = System.DateTime.Now.Ticks;
        if (t - time > 33e4)
        {
            time = t;
            yield return null;
        }
        
        CatchHits.Sort((x, y) => x.CurrentHit.Offset.CompareTo(y.CurrentHit.Offset));
        t = System.DateTime.Now.Ticks;
        if (t - time > 33e4)
        {
            time = t;
            yield return null;
        }

        PlayInterfaceObject.SetActive(true);
        Debug.Log(ResultScreen.main);
        ResultScreen.main.ResultInterfaceObject.SetActive(false);
        normalHitCount = NormalHits.Count;

        UpdateScore();
        AbortBackground.color = Color.clear;
        PlayInterfaceGroup.alpha = 1;

        IsPlaying = true;
    }

    public void SetInterfaceColor(Color color)
    {
        SongNameLabel.color = SongArtistLabel.color = DifficultyNameLabel.color = DifficultyLabel.color = 
        ScoreText.color = ScoreUnitText.color = GaugeText.color = GaugeFill.color = GaugeUpper.color = GaugeLower.color =
        ComboLabel.color = ComboText.color = SongProgressFill.color = SongProgressUpper.color = SongProgressLower.color = 
        PauseLabel.color = color;
        for (int a = 0; a < DifficultyIndicators.Count; a++) 
        {
            DifficultyIndicators[a].gameObject.SetActive(CurrentChart.DifficultyIndex >= a);
            DifficultyIndicators[a].color = new Color(color.r, color.g, color.b, color.a * (20 - CurrentChart.DifficultyIndex + a) / 20) * Mathf.Min(CurrentChart.DifficultyIndex - a + 1, 1);
        }
        for (int a = 0; a < ScoreDigits.Count; a++) 
        {
            ScoreDigits[a].color = color;
        }
    }

    void UpdateScore() 
    {

        GaugeText.text = (Gauge * 10).ToString("F" + (Gauge == 1 ? "0" : "1"), CultureInfo.InvariantCulture);
        GaugeSlider.value = Gauge;

        string score = ((int)(Score / Mathf.Max(TotalScore, 1) * 1e6)).ToString("D7", CultureInfo.InvariantCulture);
        bool d = false;
        for (int a = ScoreDigits.Count - 1; a >= 0; a--) 
        {
            if (d || ScoreDigits[a].text != score[score.Length - a - 1].ToString())
            {
                ScoreDigits[a].text = score[score.Length - a - 1].ToString();
                StopCoroutine(ScorePop(a));
                StartCoroutine(ScorePop(a));
                d = true;
            }
        }
    }

    public void AddScore(float weight, float acc, bool combo)
    {
        Score += weight * acc;
        int LastCombo = Combo;
        Combo = combo ? Combo + 1 : 0;
        MaxCombo = Mathf.Max(MaxCombo, Combo);
        NoteCount--;

        if (combo)
        {
            Gauge = Mathf.Min(1, Gauge + weight * acc / CurrentChart.ChartConstant / 35);
            if (acc >= 1) PerfectCount++;
            else GoodCount++;
            if (!isAnimating) StartCoroutine(ComboPop());
        }
        else
        {
            BadCount++;
            Gauge = Mathf.Max(0, Gauge - weight / 100);
            if (!isAnimating && LastCombo > 0) StartCoroutine(ComboDrop());
        }

        if (NoteCount <= 0) 
        {
            OffsetMean /= normalHitCount;
            Deviation /= normalHitCount;
            StartCoroutine(ResultScreen.main.SplashAnimation());
        }

        UpdateScore();
    }

    public void AddAccuracy(float time) 
    {
        Debug.Log(time);

        if (Mathf.Abs(time) <= GoodHitWindow / 1000) 
        {
            int center = AccuracyValues.Length / 2;
            int pos = Mathf.RoundToInt(time / GoodHitWindow * 1000 * center) + center;
            AccuracyValues[Mathf.Clamp(pos, 0, AccuracyValues.Length - 1)]++;
        }

        if (Mathf.Abs(time) <= PerfectHitWindow / 1000) AccuracyCounts[2]++;
        else if (Mathf.Abs(time) <= GoodHitWindow / 1000) AccuracyCounts[time < 0 ? 1 : 3]++;
        else AccuracyCounts[time < 0 ? 0 : 4]++;
        
    }
    public void AddDiscrete(bool hit) 
    {
        DiscreteCounts[hit ? 0 : 1]++;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPlaying)
        {
            CurrentTime += Time.deltaTime;
            if (CurrentTime > 0 && CurrentTime < AudioPlayer.clip.length) 
            {
                if (!AudioPlayer.isPlaying) AudioPlayer.Play();
                float preciseTime = AudioPlayer.timeSamples / (float)Song.Clip.frequency;
                if (Mathf.Abs(AudioPlayer.time - CurrentTime) > SyncThreshold) AudioPlayer.time = CurrentTime;
                else if (precTime != preciseTime) 
                {
                    CurrentTime = preciseTime;
                    precTime = preciseTime;
                } 
                    
            }
            SongProgressSlider.value = CurrentTime / Song.Clip.length;

            CurrentChart.Camera.Advance(CurrentTime);
            CurrentChart.Pallete.Advance(CurrentTime);
            RenderSettings.fogColor = MainCamera.backgroundColor = CurrentChart.Pallete.BackgroundColor;
            SetInterfaceColor(CurrentChart.Pallete.InterfaceColor);

            for (int a = 0; a < LaneStyleManagers.Count; a++)
            {
                CurrentChart.Pallete.LaneStyles[a].Advance(CurrentTime + VisualOffset);
                LaneStyleManagers[a].Update(CurrentChart.Pallete.LaneStyles[a]);
            }
            for (int a = 0; a < HitStyleManagers.Count; a++)
            {
                CurrentChart.Pallete.HitStyles[a].Advance(CurrentTime + VisualOffset);
                HitStyleManagers[a].Update(CurrentChart.Pallete.HitStyles[a]);
            }
            
            MainCamera.transform.position = CurrentChart.Camera.CameraPivot;
            MainCamera.transform.eulerAngles = CurrentChart.Camera.CameraRotation;
            MainCamera.transform.Translate(Vector3.back * CurrentChart.Camera.PivotDistance);
            
            Dictionary<Touch, HitPlayer> Touches = new Dictionary<Touch, HitPlayer>();

            if (!AutoPlay)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        Touches.Add(touch, null);
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        if (Vector2.SqrMagnitude(touch.deltaPosition) < 100) continue;
                        float dir = Mathf.Atan2(touch.deltaPosition.x, touch.deltaPosition.y) * Mathf.Rad2Deg;

                        if (FlickDirections.ContainsKey(touch.fingerId)
                            && FlickDirections[touch.fingerId].Handled
                            && Mathf.DeltaAngle(FlickDirections[touch.fingerId].Direction, dir) < 60) continue;

                        FlickDirections[touch.fingerId] = new FlickHandle()
                        {
                            Position = touch.position,
                            Direction = dir,
                        };
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        if (FlickDirections.ContainsKey(touch.fingerId)) FlickDirections.Remove(touch.fingerId);
                    }
                }
            }

            float time = CurrentTime + AudioOffset;

            foreach (HitPlayer obj in RemovingHits)
            {
                if (!obj) continue;
                if (obj.CurrentHit.Type == HitObject.HitType.Normal) NormalHits.Remove(obj);
                else CatchHits.Remove(obj);
                Destroy(obj.gameObject);
            }
            RemovingHits.Clear();

            foreach (HitPlayer obj in NormalHits)
            {
                if (obj.CurrentHit.Offset > time + BadHitWindow / 1000) break;
                if (!obj.isHit) 
                {
                    if (AutoPlay && time - AudioOffset > obj.CurrentHit.Offset)
                    {
                        obj.MakeHitEffect(0);
                        if (!obj.CurrentHit.Flickable) AddAccuracy(0);
                        else AddDiscrete(true);
                        AddScore(obj.NoteWeight, 1, true);
                        AudioPlayer.PlayOneShot(NormalHitSound, HitVolume[0]);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset + GoodHitWindow / 1000)
                    {
                        Deviation += GoodHitWindow / 1000;
                        if (!obj.CurrentHit.Flickable) AddAccuracy(float.PositiveInfinity);
                        else AddDiscrete(false);
                        AddScore(obj.NoteWeight, 0, false);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset - BadHitWindow / 1000)
                    {
                        Vector2 ScreenMid = (obj.ScreenStart + obj.ScreenEnd) / 2;
                        float dist = Vector2.Distance(obj.ScreenStart, obj.ScreenEnd) / 2 + Screen.width / 20;
                        foreach (Touch touch in Touches.Keys) 
                        {
                            if (Vector2.Distance(touch.position, ScreenMid) <= dist 
                                && (Touches[touch] == null
                                || obj.CurrentHit.Offset < Touches[touch].CurrentHit.Offset)) 
                            {
                                Touches[touch] = obj;
                                break;
                            }
                        }
                    }
                }
                else if (!obj.isFlicked && obj.CurrentHit.Flickable)
                {
                    if (time > obj.CurrentHit.Offset + GoodHitWindow / 1000)
                    {
                        AddDiscrete(false);
                        AddScore(obj.NoteWeight, 0, false);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset - GoodHitWindow / 1000)
                    {
                        foreach (FlickHandle flick in FlickDirections.Values) 
                        {
                            Vector2 ScreenMid = (obj.ScreenStart + obj.ScreenEnd) / 2;
                            float dist = Vector2.Distance(obj.ScreenStart, obj.ScreenEnd) / 2 + Screen.width / 20;
                            if (!flick.Handled && Vector2.Distance(flick.Position, ScreenMid) <= dist 
                                && (obj.CurrentHit.FlickDirection < 0 || Mathf.DeltaAngle(flick.Direction, obj.CurrentHit.FlickDirection) < 40)) 
                            {
                                flick.Handled = true;
                                obj.isFlicked = true;
                                obj.MakeHitEffect(null);
                                AddDiscrete(true);
                                AddScore(obj.NoteWeight, 1, true);
                                AudioPlayer.PlayOneShot(NormalHitSound, HitVolume[0]);
                                obj.BeginHit();
                                break;
                            }
                        }
                    }
                }
            }

            foreach (HitPlayer obj in CatchHits)
            {
                if (obj.CurrentHit.Offset > time + GoodHitWindow / 1000) break;
                if (!obj.isHit) 
                {
                    if (AutoPlay && time - AudioOffset > obj.CurrentHit.Offset)
                    {
                        obj.MakeHitEffect(null);
                        AddDiscrete(true);
                        AddScore(obj.NoteWeight, 1, true);
                        AudioPlayer.PlayOneShot(CatchHitSound, HitVolume[0]);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset + GoodHitWindow / 1000)
                    {
                        AddDiscrete(false);
                        AddScore(obj.NoteWeight, 0, false);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset - GoodHitWindow / 1000)
                    {
                        Vector2 ScreenMid = (obj.ScreenStart + obj.ScreenEnd) / 2;
                        float dist = Vector2.Distance(obj.ScreenStart, obj.ScreenEnd) / 2 + Screen.width / 20;
                        foreach (Touch touch in Input.touches) 
                        {
                            if (Vector2.Distance(touch.position, ScreenMid) <= dist) 
                            {
                                if (obj.isFlicked) obj.isPreHit = true;
                                else obj.isHit = true;
                            }
                            if (Touches.ContainsKey(touch) && Touches[touch] != null && Touches[touch].CurrentHit.Offset > obj.CurrentHit.Offset)
                            {
                                Touches.Remove(touch);
                            }
                        }
                        if (obj.isPreHit && time > obj.CurrentHit.Offset)
                        {
                            if (obj.CurrentHit.Flickable) 
                            {
                                obj.isHit = true;
                            }
                            else 
                            {
                                obj.MakeHitEffect(null);
                                AddDiscrete(true);
                                AddScore(1, 1, true);
                                AudioPlayer.PlayOneShot(CatchHitSound, HitVolume[0]);
                                
                                if (obj.Ticks.Count != 0) obj.railTime = 1;
                                obj.BeginHit();
                            }
                        }
                    }
                }
                else if (!obj.isFlicked && obj.CurrentHit.Flickable)
                {
                    if (time > obj.CurrentHit.Offset + GoodHitWindow / 1000)
                    {
                        AddScore(obj.NoteWeight, 0, false);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset - GoodHitWindow / 1000)
                    {
                        foreach (FlickHandle flick in FlickDirections.Values) 
                        {
                            Vector2 ScreenMid = (obj.ScreenStart + obj.ScreenEnd) / 2;
                            float dist = Vector2.Distance(obj.ScreenStart, obj.ScreenEnd) / 2 + Screen.width / 20;
                            if (!flick.Handled && Vector2.Distance(flick.Position, ScreenMid) <= dist 
                                && (obj.CurrentHit.FlickDirection < 0 || Mathf.DeltaAngle(flick.Direction, obj.CurrentHit.FlickDirection) < 40)) 
                            {
                                flick.Handled = true;
                                obj.isPreHit = true;
                                break;
                            }
                        }
                        if (obj.isPreHit && time > obj.CurrentHit.Offset)
                        {
                            obj.MakeHitEffect(null);
                            AddDiscrete(true);
                            AddScore(obj.NoteWeight, 1, true);
                            AudioPlayer.PlayOneShot(CatchHitSound, HitVolume[0]);
                            obj.BeginHit();
                        }
                    }
                }
            }

            foreach (Touch touch in Touches.Keys)
            {
                if (Touches[touch] != null)
                {
                    HitPlayer obj = Touches[touch];
                    float ofs = time - obj.CurrentHit.Offset;
                    if (obj.CurrentHit.Flickable)
                    {
                        if (ofs >= -GoodHitWindow / 1000)
                        {
                            obj.isHit = true;
                        }
                    }
                    else 
                    {
                        OffsetMean += ofs;
                        Deviation += Mathf.Abs(ofs);
                        float acc = ofs < -GoodHitWindow / 1000 ? -1 : HitPlayer.GetAccuracy(ofs);
                        
                        obj.MakeHitEffect(acc);
                        if (acc > -1) AddAccuracy(ofs);
                        acc = Mathf.Abs(acc);
                        AddScore(3, (1 - acc), acc < 1);
                        AudioPlayer.PlayOneShot(NormalHitSound, acc == 0 ? HitVolume[0] : acc < 1 ? HitVolume[1] : HitVolume[2]);
                        obj.BeginHit();
                    }
                }
            }
        }
    }

    IEnumerator ComboPop () 
    {
        ComboGroup.alpha = 1;
        ComboText.text = Combo.ToString("#", CultureInfo.InvariantCulture);
        for (float a = 0; a < 1; a += Time.deltaTime / .1f)
        {
            ComboText.rectTransform.anchoredPosition = Vector3.up * 5 * Mathf.Cos(a * Mathf.PI / 2);
            yield return null;
        }
        ComboText.rectTransform.anchoredPosition = Vector2.zero;
    }

    IEnumerator ComboDrop () 
    {
        isAnimating = true;
        for (float a = 0; a < 1; a += Time.deltaTime / .2f)
        {
            ComboText.rectTransform.anchoredPosition = Vector3.down * 20 * a * a;
            ComboGroup.alpha = 1 - a;
            yield return null;
        }
        ComboGroup.alpha = 0;
        if (Combo > 0) StartCoroutine(ComboPop());
        isAnimating = false;
    }

    IEnumerator ScorePop (int digit) 
    {
        for (float a = 0; a < 1; a += Time.deltaTime / .1f)
        {
            ScoreDigits[digit].rectTransform.anchoredPosition = new Vector2(ScoreDigits[digit].rectTransform.anchoredPosition.x, 2 * Mathf.Cos(a * Mathf.PI / 2));
            yield return null;
        }
        ScoreDigits[digit].rectTransform.anchoredPosition = new Vector2(ScoreDigits[digit].rectTransform.anchoredPosition.x, 0);
    }

    public void Pause()
    {
        if (PauseTime < CurrentTime)
        {
            if (CurrentTime - PauseTime < .2f)
            {
                IsPlaying = false;
                AudioPlayer.Pause();
                PauseSidebar.Show();
            }
            PauseTime = CurrentTime;
        }
    }

    public void Continue()
    {
        if (!isAnimating && !PauseSidebar.isAnimating)
        {
            StartCoroutine(ContinueAnimation());
        }
    }

    public IEnumerator ContinueAnimation()
    {
        isAnimating = true;

        PauseSidebar.Hide();

        float time = AudioPlayer.time;
        float vol = AudioPlayer.volume;

        for (float timer = 3.6f; timer > 0; timer -= Time.deltaTime)
        {
            if (!AudioPlayer.isPlaying && time - timer >= 0)
            {
                AudioPlayer.Play();
                AudioPlayer.time = time - timer;
            }
            AudioPlayer.volume = (1 - timer / 3.6f) * vol;
            yield return null;
        }

        AudioPlayer.volume = vol;

        IsPlaying = true;
        isAnimating = false;
    } 

    public void Restart()
    {
        if (!isAnimating && !PauseSidebar.isAnimating)
        {
            StartCoroutine(RestartAnimation());
        }
    }

    public void LerpAbort(float value)
    {
        float ease = Ease.Get(value, EaseFunction.Exponential, EaseMode.In);

        MainCamera.transform.position = CurrentChart.Camera.CameraPivot + MainCamera.transform.rotation * Vector3.back * (ease * 100 + CurrentChart.Camera.PivotDistance);
        SideGroup.alpha = Common.main.Storage.Get("PREF:PlayerSideAlpha", 100f) / 100;
        CenterGroup.alpha = Common.main.Storage.Get("PREF:PlayerCenterAlpha", 50f) / 100;
        
        float ease2 = Ease.Get(value, EaseFunction.Cubic, EaseMode.In);

        AbortBackground.color = CurrentChart.Pallete.BackgroundColor * new Color(1, 1, 1, ease2);
        
        float ease3 = Ease.Get(value / .4f, EaseFunction.Cubic, EaseMode.Out);

        PlayInterfaceGroup.alpha = 1 - ease3;
    }

    public IEnumerator RestartAnimation()
    {
        isAnimating = true;

        PauseSidebar.Hide();

        for (float a = 0; a < 1; a += Time.deltaTime) 
        {
            LerpAbort(a);
            yield return null;
        }
        LerpAbort(1);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        
        CurrentTime = -5;
        Score = TotalScore = Gauge = OffsetMean = Deviation = PauseTime = 
            Combo = MaxCombo = TotalCombo = NoteCount = PerfectCount = GoodCount = BadCount = 0;
            
        AccuracyCounts = new int[AccuracyCounts.Length];
        DiscreteCounts = new int[DiscreteCounts.Length];
        AccuracyValues = new int[AccuracyValues.Length];
        
        foreach (LaneStyleManager man in LaneStyleManagers) man.Dispose();
        LaneStyleManagers = new List<LaneStyleManager>();
        foreach (HitStyleManager man in HitStyleManagers) man.Dispose();
        HitStyleManagers = new List<HitStyleManager>();

        NormalHits = new List<HitPlayer>();
        CatchHits = new List<HitPlayer>();

        StartCoroutine(InitChart());

        isAnimating = false;
    } 

    public void Abort()
    {
        if (!isAnimating && !PauseSidebar.isAnimating)
        {
            StartCoroutine(AbortAnimation());
        }
    }


    public IEnumerator AbortAnimation()
    {
        isAnimating = true;

        PauseSidebar.Hide();

        for (float a = 0; a < 1; a += Time.deltaTime) 
        {
            LerpAbort(a);
            yield return null;
        }
        LerpAbort(1);

        AsyncOperation op = SceneManager.LoadSceneAsync("Song Select", LoadSceneMode.Additive);
        yield return new WaitUntil(() => op.isDone);
        yield return new WaitUntil(() => PlaylistScroll.main.IsReady);
        
        SceneManager.UnloadSceneAsync("Player");
        Resources.UnloadUnusedAssets();

        isAnimating = false;
    } 
    
    public Mesh MakeFreeFlickEmblem()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        vertices.Add(new Vector3(0, 2));
        vertices.Add(new Vector3(1, 0));
        vertices.Add(new Vector3(0, -1));
        vertices.Add(new Vector3(0, -2));
        vertices.Add(new Vector3(-1, 0));
        vertices.Add(new Vector3(0, 1));

        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);

        tris.Add(0);
        tris.Add(1);
        tris.Add(2);

        tris.Add(3);
        tris.Add(4);
        tris.Add(5);

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public Mesh MakeDirectionalFlickEmblem()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        vertices.Add(new Vector3(0, 3));
        vertices.Add(new Vector3(1, 0));
        vertices.Add(new Vector3(0, -1));
        vertices.Add(new Vector3(-1, 0));

        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);

        tris.Add(0);
        tris.Add(2);
        tris.Add(3);

        tris.Add(0);
        tris.Add(1);
        tris.Add(2);

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}

public class FlickHandle 
{
    public bool Handled;
    public Vector2 Position;
    public float Direction;
}
