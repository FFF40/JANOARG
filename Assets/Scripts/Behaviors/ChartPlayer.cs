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
    public bool AutoPlay;
    public float CurrentTime;
    public float Score;
    public float TotalScore;
    public int Combo;
    public int MaxCombo;
    public int TotalCombo;
    public int NoteCount;

    [Header("Settings")]
    public float ScrollSpeed = 120;
    public float SyncThreshold = .05f;

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
    public AudioClip NormalHitSound;
    public AudioClip CatchHitSound;

    [Header("Interface")]
    public AudioSource AudioPlayer;
    public GameObject SideObject;
    public GameObject CenterObject;
    public TMP_Text SongNameLabel;
    public TMP_Text SongSeparatorLabel;
    public TMP_Text SongArtistLabel;
    public TMP_Text DifficultyNameLabel;
    public List<Image> DifficultyIndicators;
    public TMP_Text DifficultyLabel;
    public TMP_Text ScoreText;
    public List<TMP_Text> ScoreDigits;
    public TMP_Text ScoreUnitText;
    public Slider SongProgressSlider;
    public Image SongProgressFill;
    public TMP_Text ComboLabel;
    public TMP_Text ComboText;

    [Header("Result Screen")]
    public RectTransform SummaryBox;
    public TMP_Text PlayStateText;
    public TMP_Text RecordLabelText;
    public TMP_Text RecordText;
    public TMP_Text RecordDifferenceText;
    public RectTransform ResultScoreBox;
    public TMP_Text ResultScoreText;
    public RectTransform ResultSongBox;
    public TMP_Text ResultSongNameText;
    public RectTransform DetailsBox;
    public List<TMP_Text> ResultBasicInfoLabelText;
    public List<TMP_Text> ResultBasicInfoText;
    public TMP_Text ResultComboLabelText;
    public TMP_Text ResultComboInfoText;

    [HideInInspector]
    public Chart CurrentChart;
    [HideInInspector]
    public List<LaneStyleManager> LaneStyleManagers = new List<LaneStyleManager>();
    [HideInInspector]
    public List<HitStyleManager> HitStyleManagers = new List<HitStyleManager>();

    bool isAnimating;

    public void Awake()
    {
        main = this;
    }

    public void NormalizeTimestamp(Timestamp ts)
    {
        ts.Duration = Song.Timing.ToSeconds(ts.Time + ts.Duration);
        ts.Time = Song.Timing.ToSeconds(ts.Time);
        ts.Duration -= ts.Time;
    }

    public void Start()
    {
        // Clone chart

        CurrentChart = Instantiate(Song).Charts[ChartPosition];

        // Init chart data
        
        foreach (Timestamp ts in CurrentChart.Storyboard.Timestamps)
            NormalizeTimestamp(ts);
        
        foreach (Timestamp ts in CurrentChart.Pallete.Storyboard.Timestamps)
            NormalizeTimestamp(ts);

        foreach (LaneStyle style in CurrentChart.Pallete.LaneStyles)
        {
            LaneStyleManagers.Add(new LaneStyleManager(style));
            foreach (Timestamp ts in style.Storyboard.Timestamps)
                NormalizeTimestamp(ts);
        }

        foreach (HitStyle style in CurrentChart.Pallete.HitStyles)
        {
            HitStyleManagers.Add(new HitStyleManager(style));
            foreach (Timestamp ts in style.Storyboard.Timestamps)
                NormalizeTimestamp(ts);
        }

        foreach (Lane lane in CurrentChart.Lanes) {
            LanePlayer lp = Instantiate(LanePlayerSample, transform);
            lp.SetLane(lane);
        }

        SongNameLabel.text = Song.SongName;
        SongArtistLabel.text = Song.SongArtist;
        DifficultyNameLabel.text = CurrentChart.DifficultyName;
        DifficultyLabel.text = CurrentChart.DifficultyLevel;
        AudioPlayer.clip = Song.Clip;

        RenderSettings.fogColor = MainCamera.backgroundColor = CurrentChart.Pallete.BackgroundColor;
        SetInterfaceColor(CurrentChart.Pallete.InterfaceColor);

        float camRatio = Mathf.Min(1, (3f * Screen.width) / (2f * Screen.height));
        MainCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;

        UpdateScore();
    }

    public void SetInterfaceColor(Color color)
    {
        SongNameLabel.color = SongSeparatorLabel.color = SongArtistLabel.color = DifficultyNameLabel.color =
        DifficultyLabel.color = ScoreText.color = ScoreUnitText.color = ComboLabel.color = ComboText.color = SongProgressFill.color = color;
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

        SideObject.SetActive(NoteCount > 0);

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

    public void AddScore(float weight, bool combo)
    {
        Score += weight;
        Combo = combo ? Combo + 1 : 0;
        MaxCombo = Mathf.Max(MaxCombo, Combo);
        NoteCount--;
        
        StopCoroutine(ComboPop());
        if (combo)
        {
            StartCoroutine(ComboPop());
        }

        if (NoteCount <= 0) StartCoroutine(SplashAnimation());
        UpdateScore();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        CurrentChart.Advance(CurrentTime);
        if (CurrentTime > 0 && CurrentTime < AudioPlayer.clip.length) 
        {
            if (!AudioPlayer.isPlaying) AudioPlayer.Play();
            if (Mathf.Abs(AudioPlayer.time - CurrentTime) > SyncThreshold) AudioPlayer.time = CurrentTime;
            else CurrentTime = AudioPlayer.time;
        }
        SongProgressSlider.value = CurrentTime / Song.Clip.length;

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
        if (Score >= TotalScore) PlayStateText.text = "TRACK MASTERED";
        else if (Combo >= MaxCombo) PlayStateText.text = "FULL STREAK";
        else PlayStateText.text = "TRACK CLEARED";

        PlayStateText.rectTransform.anchorMin = PlayStateText.rectTransform.anchorMax = new Vector2(.5f, .5f);
        PlayStateText.rectTransform.anchoredPosition = Vector2.zero;
        RecordLabelText.margin = RecordText.margin = RecordDifferenceText.margin = new Vector4(0, 50, 10, 0);
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
        for (int x = 0; x < ResultBasicInfoText.Count; x++) 
        {
            ResultBasicInfoText[x].text = "0";
        }
        ResultBasicInfoText[2].text = TotalCombo.ToString("0", CultureInfo.InvariantCulture);
        ResultComboInfoText.text = MaxCombo.ToString("0", CultureInfo.InvariantCulture) + " / " + TotalCombo.ToString("0", CultureInfo.InvariantCulture);

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
            for (int x = 0; x < ResultBasicInfoText.Count; x++) 
            {
                ResultBasicInfoLabelText[x].margin = new Vector4(0, -60 * (1 - Ease.Get(Mathf.Clamp01(a - (3 + x * .1f)), "Quintic", EaseMode.Out)), 8, 0);
                ResultBasicInfoText[x].margin = new Vector4(0, -60 * (1 - Ease.Get(Mathf.Clamp01(a - (3.05f + x * .1f)), "Quintic", EaseMode.Out)), 25, 0);
            }
            ResultComboLabelText.margin = new Vector4(25, -60 * (1 - Ease.Get(Mathf.Clamp01(a - (3.5f)), "Quintic", EaseMode.Out)), 8, 0);
            ResultComboInfoText.margin = new Vector4(0, -60 * (1 - Ease.Get(Mathf.Clamp01(a - (3.55f)), "Quintic", EaseMode.Out)), 0, 0);
        }
        for (float a = 0; a < 1; a += Time.deltaTime / 6f)
        {
            SetEase(a * 6);
            yield return null;
        }
        SetEase(6);
    }
}
