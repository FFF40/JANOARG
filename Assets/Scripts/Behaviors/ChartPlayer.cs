using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChartPlayer : MonoBehaviour
{
    public static ChartPlayer main;

    [Header("Data")]
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
    [Space]
    public int Combo;
    public int MaxCombo;
    public int TotalCombo;
    public int NoteCount;

    [Header("Settings")]
    public float ScrollSpeed = 120;
    public float SyncThreshold = .05f;
    [Space]
    public float PerfectHitWindow = 35;
    public float GoodHitWindow = 200;

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
    [Space]
    public GameObject SideObject;
    public GameObject CenterObject;
    [Space]
    public TMP_Text SongNameLabel;
    public TMP_Text SongArtistLabel;
    public TMP_Text DifficultyNameLabel;
    public List<Image> DifficultyIndicators;
    public Image DifficultyBox;
    public TMP_Text DifficultyLabel;
    [Space]
    public TMP_Text ScoreText;
    public List<TMP_Text> ScoreDigits;
    public TMP_Text ScoreUnitText;
    [Space]
    public Slider SongProgressSlider;
    public Image SongProgressFill;
    [Space]
    public Image GaugeBox;
    public TMP_Text GaugeText;
    public Slider GaugeSlider;
    public Image GaugeFill;
    public Image GaugeUpper;
    public Image GaugeLower;
    [Space]
    public TMP_Text ComboLabel;
    public TMP_Text ComboText;

    [Header("Result Interface")]
    public GameObject ResultInterfaceObject;
    [Space]
    public RectTransform SummaryBox;
    public TMP_Text PlayStateText;
    [Space]
    public RectTransform ResultSongBox;
    public TMP_Text ResultSongNameText;
    public TMP_Text ResultSongArtistText;
    public TMP_Text ResultDifficultyLabel;
    public List<Image> ResultDifficultyIndicators;
    [Space]
    public RectTransform ResultScoreBox;
    public TMP_Text ResultScoreText;
    [Space]
    public TMP_Text RecordLabelText;
    public TMP_Text RecordText;
    public TMP_Text RecordDifferenceText;
    [Space]
    public RectTransform DetailsBox;
    public TMP_Text ResultPerfectText;
    public TMP_Text ResultGoodText;
    public TMP_Text ResultBadText;
    public TMP_Text ResultComboText;

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

    public void Awake()
    {
        main = this;
        Application.targetFrameRate = 60;
        var configuration = AudioSettings.GetConfiguration();
        configuration.dspBufferSize = 2048;
        AudioSettings.Reset(configuration);
    }

    public void NormalizeTimestamp(Timestamp ts)
    {
        ts.Duration = Song.Timing.ToSeconds(ts.Time + ts.Duration);
        ts.Time = Song.Timing.ToSeconds(ts.Time);
        ts.Duration -= ts.Time;
    }

    public void Start()
    {
        StartCoroutine(InitChart());

        float camRatio = Mathf.Min(1, (3f * Screen.width) / (2f * Screen.height));
        MainCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;

        UpdateScore();
    }

    public IEnumerator InitChart()
    {
        CurrentChart = Instantiate(Song).Charts[ChartPosition];

        SongNameLabel.text = Song.SongName;
        SongArtistLabel.text = Song.SongArtist;
        DifficultyNameLabel.text = CurrentChart.DifficultyName;
        DifficultyLabel.text = CurrentChart.DifficultyLevel;
        AudioPlayer.clip = Song.Clip;

        RenderSettings.fogColor = MainCamera.backgroundColor = CurrentChart.Pallete.BackgroundColor;
        SetInterfaceColor(CurrentChart.Pallete.InterfaceColor);

        if (!FreeFlickEmblem) FreeFlickEmblem = MakeFreeFlickEmblem();
        if (!DirectionalFlickEmblem) DirectionalFlickEmblem = MakeDirectionalFlickEmblem();

        long time = System.DateTime.Now.Ticks;
        long t;

        foreach (Timestamp ts in CurrentChart.Storyboard.Timestamps)
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
        foreach (Lane lane in CurrentChart.Lanes) {
            LanePlayer lp = Instantiate(LanePlayerSample, transform);
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
        ResultInterfaceObject.SetActive(false);

        IsPlaying = true;
    }

    public void SetInterfaceColor(Color color)
    {
        SongNameLabel.color = SongArtistLabel.color = DifficultyNameLabel.color = DifficultyBox.color = DifficultyLabel.color = 
        ScoreText.color = ScoreUnitText.color = GaugeBox.color = GaugeText.color = GaugeFill.color = GaugeUpper.color = GaugeLower.color =
        ComboLabel.color = ComboText.color = SongProgressFill.color = color;
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
        ComboText.text = Combo.ToString("#", CultureInfo.InvariantCulture);

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
        Combo = combo ? Combo + 1 : 0;
        MaxCombo = Mathf.Max(MaxCombo, Combo);
        NoteCount--;

        if (combo)
        {
            Gauge = Mathf.Min(1, Gauge + weight * acc / CurrentChart.ChartConstant / 35);
            if (acc >= 1) PerfectCount++;
            else GoodCount++;
            StartCoroutine(ComboPop());
        }
        else
        {
            BadCount++;
            Gauge = Mathf.Max(0, Gauge - weight / 35);
        }

        if (NoteCount <= 0) StartCoroutine(SplashAnimation());
        UpdateScore();
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

            CurrentChart.Advance(CurrentTime);
            CurrentChart.Pallete.Advance(CurrentTime);
            RenderSettings.fogColor = MainCamera.backgroundColor = CurrentChart.Pallete.BackgroundColor;
            SetInterfaceColor(CurrentChart.Pallete.InterfaceColor);

            for (int a = 0; a < LaneStyleManagers.Count; a++)
            {
                CurrentChart.Pallete.LaneStyles[a].Advance(CurrentTime);
                LaneStyleManagers[a].Update(CurrentChart.Pallete.LaneStyles[a]);
            }
            for (int a = 0; a < HitStyleManagers.Count; a++)
            {
                CurrentChart.Pallete.HitStyles[a].Advance(CurrentTime);
                HitStyleManagers[a].Update(CurrentChart.Pallete.HitStyles[a]);
            }
            
            MainCamera.transform.position = CurrentChart.CameraPivot;
            MainCamera.transform.eulerAngles = CurrentChart.CameraRotation;
            MainCamera.transform.Translate(Vector3.back * 10);
            
            Dictionary<Touch, HitPlayer> Touches = new Dictionary<Touch, HitPlayer>();

            if (!ChartPlayer.main.AutoPlay)
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

            float time = CurrentTime;

            foreach (HitPlayer obj in RemovingHits)
            {
                if (obj.CurrentHit.Type == HitObject.HitType.Normal) NormalHits.Remove(obj);
                else CatchHits.Remove(obj);
            }

            foreach (HitPlayer obj in NormalHits)
            {
                if (obj.CurrentHit.Offset > time + .2f) break;
                if (!obj.isHit) 
                {
                    if (ChartPlayer.main.AutoPlay && time > obj.CurrentHit.Offset)
                    {
                        obj.MakeHitEffect(0);
                        ChartPlayer.main.AddScore(obj.NoteWeight, 1, true);
                        ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.NormalHitSound);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset + .2f)
                    {
                        ChartPlayer.main.AddScore(obj.NoteWeight, 0, false);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset - .2f)
                    {
                        Vector2 ScreenMid = (obj.ScreenStart + obj.ScreenEnd) / 2;
                        float dist = Vector2.Distance(obj.ScreenStart, obj.ScreenEnd) + Screen.width / 20;
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
                    if (time > obj.CurrentHit.Offset + .2f)
                    {
                        ChartPlayer.main.AddScore(obj.NoteWeight, 0, false);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset - .2f)
                    {
                        foreach (FlickHandle flick in FlickDirections.Values) 
                        {
                            Vector2 ScreenMid = (obj.ScreenStart + obj.ScreenEnd) / 2;
                            float dist = Vector2.Distance(obj.ScreenStart, obj.ScreenEnd) + Screen.width / 20;
                            if (!flick.Handled && Vector2.Distance(flick.Position, ScreenMid) <= dist 
                                && (obj.CurrentHit.FlickDirection < 0 || Mathf.DeltaAngle(flick.Direction, obj.CurrentHit.FlickDirection) < 40)) 
                            {
                                flick.Handled = true;
                                obj.isFlicked = true;
                                obj.MakeHitEffect(null);
                                ChartPlayer.main.AddScore(obj.NoteWeight, 1, true);
                                ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.NormalHitSound);
                                obj.BeginHit();
                                break;
                            }
                        }
                    }
                }
            }

            foreach (Touch touch in Touches.Keys)
            {
                if (Touches[touch] != null)
                {
                    HitPlayer obj = Touches[touch];
                    if (obj.CurrentHit.Flickable)
                    {
                        obj.isHit = true;
                    }
                    else 
                    {
                        float acc = HitPlayer.GetAccuracy(time - obj.CurrentHit.Offset);
                        obj.MakeHitEffect(acc);
                        ChartPlayer.main.AddScore(3, (1 - Mathf.Abs(acc)), true);
                        ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.NormalHitSound);
                        obj.BeginHit();
                    }
                }
            }

            foreach (HitPlayer obj in CatchHits)
            {
                if (obj.CurrentHit.Offset > time + .2f) break;
                if (!obj.isHit) 
                {
                    if (ChartPlayer.main.AutoPlay && time > obj.CurrentHit.Offset)
                    {
                        obj.MakeHitEffect(null);
                        ChartPlayer.main.AddScore(obj.NoteWeight, 1, true);
                        ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.CatchHitSound);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset + .2f)
                    {
                        ChartPlayer.main.AddScore(obj.NoteWeight, 0, false);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset - .2f)
                    {
                        Vector2 ScreenMid = (obj.ScreenStart + obj.ScreenEnd) / 2;
                        float dist = Vector2.Distance(obj.ScreenStart, obj.ScreenEnd) + Screen.width / 20;
                        foreach (Touch touch in Input.touches) 
                        {
                            if (Vector2.Distance(touch.position, ScreenMid) <= dist) 
                            {
                                if (obj.isFlicked) obj.isPreHit = true;
                                else obj.isHit = true;
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
                                ChartPlayer.main.AddScore(1, 1, true);
                                ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.CatchHitSound);
                                
                                if (obj.Ticks.Count != 0) obj.railTime = 1;
                                obj.BeginHit();
                            }
                        }
                    }
                }
                else if (!obj.isFlicked && obj.CurrentHit.Flickable)
                {
                    if (time > obj.CurrentHit.Offset + .2f)
                    {
                        ChartPlayer.main.AddScore(obj.NoteWeight, 0, false);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset - .2f)
                    {
                        foreach (FlickHandle flick in FlickDirections.Values) 
                        {
                            Vector2 ScreenMid = (obj.ScreenStart + obj.ScreenEnd) / 2;
                            float dist = Vector2.Distance(obj.ScreenStart, obj.ScreenEnd) + Screen.width / 20;
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
                            obj.isFlicked = true;
                            obj.MakeHitEffect(null);
                            ChartPlayer.main.AddScore(obj.NoteWeight, 1, true);
                            ChartPlayer.main.AudioPlayer.PlayOneShot(ChartPlayer.main.CatchHitSound);
                            obj.BeginHit();
                        }
                    }
                }
            }
        }
    }

    IEnumerator ComboPop () 
    {
        for (float a = 0; a < 1; a += Time.deltaTime / .1f)
        {
            ComboText.rectTransform.anchoredPosition = Vector3.up * 5 * Mathf.Cos(a * Mathf.PI / 2);
            yield return null;
        }
        ComboText.rectTransform.anchoredPosition = Vector2.zero;
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

    IEnumerator SplashAnimation () 
    {
        isAnimating = true;
        if (Score >= TotalScore) PlayStateText.text = "TRACK MASTERED!!";
        else if (Combo >= TotalCombo) PlayStateText.text = "FULL STREAK!";
        else if (Score <= 0) PlayStateText.text = "TRACK CLEARED?";
        else PlayStateText.text = "TRACK CLEARED";

        PlayInterfaceObject.SetActive(false);
        ResultInterfaceObject.SetActive(true);

        SummaryBox.anchorMin = new Vector2(0, .5f);
        SummaryBox.anchorMax = new Vector2(1, .5f);
        SummaryBox.anchoredPosition = Vector2.zero;
        PlayStateText.rectTransform.anchorMin = PlayStateText.rectTransform.anchorMax = new Vector2(.5f, .5f);
        PlayStateText.rectTransform.anchoredPosition = Vector2.zero;
        ResultSongBox.sizeDelta = new Vector2(ResultSongBox.sizeDelta.x, 0);
        DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, 0);


        ResultScoreBox.anchoredPosition = new Vector2(22500, 0);

        for (float a = 0; a < 1; a += Time.deltaTime / 4f)
        {
            float ease = Ease.Get(a, "Circle", EaseMode.Out);
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 50 + (50 * (1 - ease)));
            PlayStateText.fontSize = 32 + (15 / ease - 15);
            PlayStateText.characterSpacing = 50 - (110 * (1 - ease));
            yield return null;
        }
        SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 50);
        ComboText.fontSize = 120;
        PlayStateText.fontSize = 32;
        isAnimating = false;

        StartCoroutine(ResultAnimation());
    }

    IEnumerator ResultAnimation () 
    {

        ResultSongNameText.text = Song.SongName;
        ResultSongArtistText.text = Song.SongArtist;
        ResultDifficultyLabel.text = CurrentChart.DifficultyLevel;

        ResultPerfectText.text = PerfectCount.ToString("0", CultureInfo.InvariantCulture);
        ResultGoodText.text = GoodCount.ToString("0", CultureInfo.InvariantCulture);
        ResultBadText.text = BadCount.ToString("0", CultureInfo.InvariantCulture);
        ResultComboText.text = MaxCombo.ToString("0", CultureInfo.InvariantCulture) + " / " + TotalCombo.ToString("0", CultureInfo.InvariantCulture);

        void SetEase(float a) 
        {
            float ease = a < 1 ? Mathf.Pow(2 * Mathf.Clamp01(a / 2), 8) / 2 : 1 - Mathf.Pow(-2 * Mathf.Clamp01(a / 2) + 2, 8) / 2;
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 50 + (30 * ease));
            PlayStateText.characterSpacing = 50 - (40 * ease);
            PlayStateText.rectTransform.anchorMin = PlayStateText.rectTransform.anchorMax = new Vector2(.5f * (1 - ease), .5f);
            PlayStateText.rectTransform.anchoredPosition = new Vector2(PlayStateText.preferredWidth / 2, 12) * ease;

            RecordLabelText.margin = new Vector4(0, 50 * (1 - Ease.Get(Mathf.Clamp01(a - 1), "Quintic", EaseMode.Out)), 10, 0);
            RecordText.margin = new Vector4(0, 50 * (1 - Ease.Get(Mathf.Clamp01(a - 1.1f), "Quintic", EaseMode.Out)), 10, 0);
            RecordDifferenceText.margin = new Vector4(0, 50 * (1 - Ease.Get(Mathf.Clamp01(a - 1.2f), "Quintic", EaseMode.Out)), 10, 0);

            ResultScoreBox.anchoredPosition = new Vector2((10 / Mathf.Clamp01(a - 1) - 10), 0);
            ResultScoreText.text = ((int)(Score / Mathf.Max(TotalScore, 1) * 1e6 * (a > 3 ? 1 : 1 - Mathf.Pow(1e-7f, Mathf.Clamp01(a / 2 - .5f))))).ToString("D7", CultureInfo.InvariantCulture);

            ResultSongBox.sizeDelta = new Vector2(ResultSongBox.sizeDelta.x, 40 * Ease.Get(Mathf.Clamp01(a / 2 - 1.5f), "Quintic", EaseMode.Out));
            DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, 40 * Ease.Get(Mathf.Clamp01(a / 2 - 1.5f), "Quintic", EaseMode.Out));
        }
        for (float a = 0; a < 1; a += Time.deltaTime / 6f)
        {
            SetEase(a * 6);
            yield return null;
        }
        SetEase(6);
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
