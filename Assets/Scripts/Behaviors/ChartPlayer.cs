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
    [Space]
    public int Combo;
    public int MaxCombo;
    public int TotalCombo;
    public int NoteCount;
    [Space]
    public float OffsetMean;
    public float Deviation;
    public int[] AccuracyValues = new int[23];
    public int[] DiscreteValues = new int[2];

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
    public CanvasGroup ComboGroup;
    public TMP_Text ComboLabel;
    public TMP_Text ComboText;



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

    public void Awake()
    {
        main = this;
        Application.targetFrameRate = 60;
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
        long time = System.DateTime.Now.Ticks;
        long t;

        Song = Resources.Load<PlayableSong>(SongPath);
        t = System.DateTime.Now.Ticks;
        if (t - time > 33e4)
        {
            time = t;
            yield return null;
        }
        CurrentChart = Instantiate(Resources.Load<ExternalChart>(System.IO.Path.GetDirectoryName(SongPath).Replace('\\', '/') + "/" + Song.Charts[ChartPosition].Target)).Data;
        t = System.DateTime.Now.Ticks;
        if (t - time > 33e4)
        {
            time = t;
            yield return null;
        }

        SongNameLabel.text = Song.SongName;
        SongArtistLabel.text = Song.SongArtist;
        DifficultyNameLabel.text = CurrentChart.DifficultyName;
        DifficultyLabel.text = CurrentChart.DifficultyLevel;
        AudioPlayer.clip = Song.Clip;

        RenderSettings.fogColor = MainCamera.backgroundColor = CurrentChart.Pallete.BackgroundColor;
        SetInterfaceColor(CurrentChart.Pallete.InterfaceColor);

        if (!FreeFlickEmblem) FreeFlickEmblem = MakeFreeFlickEmblem();
        if (!DirectionalFlickEmblem) DirectionalFlickEmblem = MakeDirectionalFlickEmblem();

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
        Debug.Log(ResultScreen.main);
        ResultScreen.main.ResultInterfaceObject.SetActive(false);
        normalHitCount = NormalHits.Count;

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

    public void AddAccuracy(float acc) 
    {
        int pos = (int)(Mathf.Ceil(Mathf.Abs(acc) * 10) * Mathf.Sign(acc)) + 11;
        if (acc == -1) pos = 0;
        else if (acc == 1) pos = 22;
        AccuracyValues[pos]++;
    }
    public void AddDiscrete(bool hit) 
    {
        DiscreteValues[hit ? 0 : 1]++;
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

            float time = CurrentTime;

            foreach (HitPlayer obj in RemovingHits)
            {
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
                    if (AutoPlay && time > obj.CurrentHit.Offset)
                    {
                        obj.MakeHitEffect(0);
                        if (obj.isFlicked) AddAccuracy(0);
                        else AddDiscrete(true);
                        AddScore(obj.NoteWeight, 1, true);
                        AudioPlayer.PlayOneShot(NormalHitSound);
                        obj.BeginHit();
                    }
                    else if (time > obj.CurrentHit.Offset + GoodHitWindow / 1000)
                    {
                        Deviation += GoodHitWindow / 1000;
                        if (obj.isFlicked) AddAccuracy(1);
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
                                AudioPlayer.PlayOneShot(NormalHitSound);
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
                    if (AutoPlay && time > obj.CurrentHit.Offset)
                    {
                        obj.MakeHitEffect(null);
                        AddDiscrete(true);
                        AddScore(obj.NoteWeight, 1, true);
                        AudioPlayer.PlayOneShot(CatchHitSound);
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
                                AudioPlayer.PlayOneShot(CatchHitSound);
                                
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
                            AudioPlayer.PlayOneShot(CatchHitSound);
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
                        AddAccuracy(acc);
                        AddScore(3, (1 - Mathf.Abs(acc)), acc != -1);
                        AudioPlayer.PlayOneShot(NormalHitSound);
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
