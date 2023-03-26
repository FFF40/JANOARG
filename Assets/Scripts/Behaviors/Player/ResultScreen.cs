
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class ResultScreen : MonoBehaviour
{

    public static ResultScreen main;
    
    public RectTransform Canvas;
    public GameObject ResultInterfaceObject;
    [Space]
    public bool ExtraDetailMode;
    [Space]
    public RectTransform SummaryBox;
    public Image SummaryImage;
    public TMP_Text PlayStateText;
    [Space]
    public RectTransform ResultSongBox;
    public TMP_Text ResultSongNameText;
    public TMP_Text ResultSongArtistText;
    public TMP_Text ResultDifficultyLabel;
    public List<Image> ResultDifficultyIndicators;
    [Space]
    public RectTransform RankBox;
    public Image RankBackground;
    public List<GraphicCircle> RankFill;
    public GraphicCircle RankExplosion;
    public TMP_Text RankLabel;
    public RectTransform RankPlayStateBox;
    public TMP_Text RankPlayStateText;
    [Space]
    public RectTransform ResultScoreBox;
    public LayoutGroup ResultScoreLayout;
    public TMP_Text ResultScoreText;
    public TMP_Text ResultScoreLabel;
    [Space]
    public RectTransform RecordBox;
    public TMP_Text RecordLabelText;
    public TMP_Text RecordText;
    public TMP_Text RecordDifferenceText;
    [Space]
    public RectTransform DetailsBox;
    public RectTransform SimpleDetailBox;
    public TMP_Text ResultPerfectText;
    public TMP_Text ResultGoodText;
    public TMP_Text ResultBadText;
    public TMP_Text ResultComboText;
    [Space]
    public RectTransform ExtraDetailBox;
    public TMP_Text ResultAccuracyTooEarlyText;
    public TMP_Text ResultAccuracyEarlyText;
    public TMP_Text ResultAccuracyPerfectText;
    public TMP_Text ResultAccuracyLateText;
    public TMP_Text ResultAccuracyTooLateText;
    public TMP_Text ResultDiscretePassText;
    public TMP_Text ResultDiscreteFailText;
    public TMP_Text ResultTimingAverageText;
    public TMP_Text ResultTimingDeviationText;
    public RectTransform ResultGraphBox;
    public List<Image> ResultGraphBars;
    public Image ResultBarSample;
    public Color PerfectBarColor;
    public Color HighGoodBarColor;
    public Color LowGoodBarColor;
    public Color BadBarColor;
    [Space]
    public RectTransform ActionBar;

    RankLetter currentRank;

    bool isAnimating = false;

    void Awake()
    {
        Debug.Log("set main");
        main = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SplashAnimation () 
    {
        ChartPlayer cp = ChartPlayer.main;

        isAnimating = true;

        RankPlayStateText.text = "";

        if (cp.Score >= cp.TotalScore)             RankPlayStateText.text = PlayStateText.text = "ALL FLAWLESS!!!";
        else if (cp.GoodCount >= cp.TotalCombo)    RankPlayStateText.text = PlayStateText.text = "ALL MISALIGNED...!?";
        else if (cp.Combo >= cp.TotalCombo)        RankPlayStateText.text = PlayStateText.text = "FULL STREAK!";
        else if (cp.Score <= 0)                    RankPlayStateText.text = PlayStateText.text = "ALL BROKEN...?";
        else                                                                PlayStateText.text = "TRACK CLEARED";
        
        RankPlayStateBox.sizeDelta = new Vector2(0, RankPlayStateBox.sizeDelta.y);

        cp.PlayInterfaceObject.SetActive(false);
        ResultInterfaceObject.SetActive(true);

        SummaryBox.anchorMin = new Vector2(0, .5f);
        SummaryBox.anchorMax = new Vector2(1, .5f);
        SummaryBox.anchoredPosition = Vector2.zero;
        PlayStateText.rectTransform.anchorMin = PlayStateText.rectTransform.anchorMax = new Vector2(.5f, .5f);
        PlayStateText.rectTransform.anchoredPosition = Vector2.zero;
        ResultSongBox.sizeDelta = new Vector2(ResultSongBox.sizeDelta.x, 0);
        DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, 0);
        PlayStateText.alpha = 1;

        RankBackground.color = RankLabel.color = Color.clear;
        foreach (GraphicCircle fill in RankFill)
        {
            fill.color = Color.clear;
        }

        ResultScoreBox.anchoredPosition = RecordBox.anchoredPosition = new Vector2(22500, 0);

        for (float a = 0; a < 1; a += Time.deltaTime / 4f)
        {
            float ease = Ease.Get(a, EaseFunction.Circle, EaseMode.Out);
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 50 + (50 * (1 - ease)));
            PlayStateText.fontSize = 32 + (15 / ease - 15);
            PlayStateText.characterSpacing = 50 - (110 * (1 - ease));
            yield return null;
        }
        SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 50);
        cp.ComboText.fontSize = 120;
        PlayStateText.fontSize = 32;
        isAnimating = false;

        StartCoroutine(ResultAnimation());
    }

    public IEnumerator ResultAnimation () 
    {
        isAnimating = true;

        ChartPlayer cp = ChartPlayer.main;

        // ---- Information

        SimpleDetailBox.anchoredPosition = new Vector2(0, 0);
        ExtraDetailBox.anchoredPosition = new Vector2(0, -40);

        ResultSongNameText.text = cp.Song.SongName;
        ResultSongArtistText.text = cp.Song.SongArtist;
        ResultDifficultyLabel.text = cp.CurrentChart.DifficultyLevel;
        for (int a = 0; a < ResultDifficultyIndicators.Count; a++) 
        {
            ResultDifficultyIndicators[a].gameObject.SetActive(cp.CurrentChart.DifficultyIndex >= a);
        }

        ResultPerfectText.text = cp.PerfectCount.ToString("0", CultureInfo.InvariantCulture);
        ResultGoodText.text = cp.GoodCount.ToString("0", CultureInfo.InvariantCulture);
        ResultBadText.text = cp.BadCount.ToString("0", CultureInfo.InvariantCulture);
        ResultComboText.text = cp.MaxCombo.ToString("0", CultureInfo.InvariantCulture) + " / " + cp.TotalCombo.ToString("0", CultureInfo.InvariantCulture);
        
        ResultAccuracyTooEarlyText.text = cp.AccuracyCounts[0].ToString("0", CultureInfo.InvariantCulture);
        ResultAccuracyEarlyText.text = cp.AccuracyCounts[1].ToString("0", CultureInfo.InvariantCulture);
        ResultAccuracyPerfectText.text = cp.AccuracyCounts[2].ToString("0", CultureInfo.InvariantCulture);
        ResultAccuracyLateText.text = cp.AccuracyCounts[3].ToString("0", CultureInfo.InvariantCulture);
        ResultAccuracyTooLateText.text = cp.AccuracyCounts[4].ToString("0", CultureInfo.InvariantCulture);

        ResultDiscretePassText.text = cp.DiscreteCounts[0].ToString("0", CultureInfo.InvariantCulture);
        ResultDiscreteFailText.text = cp.DiscreteCounts[1].ToString("0", CultureInfo.InvariantCulture);

        ResultTimingAverageText.text = (cp.OffsetMean < 0 ? "−" : "+") + Mathf.Abs(cp.OffsetMean * 1e3f).ToString("0.00", CultureInfo.InvariantCulture) + "ms";
        ResultTimingDeviationText.text = (cp.Deviation * 1e3f).ToString("0.00", CultureInfo.InvariantCulture) + "ms";

        // ---- Bars

        float max = Mathf.Max(cp.AccuracyValues);
        for (int a = 0; a < cp.AccuracyValues.Length; a++) 
        {
            Image bar = Instantiate(ResultBarSample, ResultGraphBox);
            float dist = Mathf.Abs(a - cp.AccuracyValues.Length / 2) * cp.GoodHitWindow / (cp.AccuracyValues.Length / 2);
            Debug.Log(a + " " + dist + " " + cp.PerfectHitWindow);
            bar.color = dist < cp.PerfectHitWindow ? PerfectBarColor : 
                Color.Lerp(HighGoodBarColor, LowGoodBarColor, (dist - cp.PerfectHitWindow) / (cp.GoodHitWindow - cp.PerfectHitWindow));
            bar.rectTransform.pivot = new Vector2(a / (cp.AccuracyValues.Length - 1f), .5f);
            bar.rectTransform.anchorMin = new Vector2(a / (cp.AccuracyValues.Length - 1f), 0);
            bar.rectTransform.anchorMax = new Vector2(a / (cp.AccuracyValues.Length - 1f), cp.AccuracyValues[a] / max);
            bar.rectTransform.sizeDelta = new Vector2(4, 0);
        }

        max = cp.DiscreteCounts[0] + cp.DiscreteCounts[1];
        float sum = 0;
        for (int a = 0; a < 2; a++) 
        {
            Image bar = Instantiate(ResultBarSample, ResultGraphBox);
            bar.color = a == 0 ? PerfectBarColor : BadBarColor;
            bar.rectTransform.pivot = new Vector2(0, 1);
            bar.rectTransform.anchorMin = new Vector2(sum, 0);
            bar.rectTransform.anchorMax = new Vector2(sum + cp.DiscreteCounts[a] / max, 0);
            bar.rectTransform.sizeDelta = new Vector2(0, 4);
            bar.rectTransform.anchoredPosition = new Vector3(0, -4);
            sum = bar.rectTransform.anchorMax.x;
        }

        // ---- Animation
        
        RankLabel.text = Helper.GetRank(cp.Score / Mathf.Max(cp.TotalScore, 1) * 1e6f);

        for (float a = 0; a < 1; a += Time.deltaTime)
        {
            float ease = Ease.Get(a, EaseFunction.Circle, EaseMode.In);
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 50 + (50 * ease));
            PlayStateText.fontSize = 32 * (1 - ease);
            PlayStateText.characterSpacing = 50 / Mathf.Pow(1 - ease, 1.5f);
            yield return null;
        }
        
        PlayStateText.rectTransform.anchoredPosition = new Vector2(0, 200);

        void SetScore(float a) 
        {
            float score = cp.Score / Mathf.Max(cp.TotalScore, 1) * a * 1e6f;
            ResultScoreText.text = ((int)(score)).ToString("D7", CultureInfo.InvariantCulture);
            RecordDifferenceText.text = "+" + ((int)(score)).ToString("D7", CultureInfo.InvariantCulture);
            for (int i = 0; i < RankFill.Count; i++) 
            {
                RankFill[i].FillAmount = score / 1e6f;
                score = (score - 9e5f) * 10;
            }
        }
        for (float a = 0; a < 1; a += Time.deltaTime / 2f)
        {
            float ease = Ease.Get(a, EaseFunction.Circle, EaseMode.In);
            float height = 100 + 80 * Ease.Get(a, EaseFunction.Circle, EaseMode.Out);
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, height);
            
            float ease2 = Ease.Get(a, EaseFunction.Exponential, EaseMode.Out);
            ResultScoreBox.anchoredPosition = new Vector2(-800 * (1 - ease2) - 50, 0);
            RecordBox.anchoredPosition = new Vector2(800 * (1 - ease2) + 50, 0);
            
            SetScore(1 - Mathf.Pow(1e-7f, a));
            for (int i = 0; i < RankFill.Count; i++) 
            {
                float prg = Mathf.Max(1 - Mathf.Pow(1 - (a * (1 + .1f * i) - .1f * i), 15), 1e-7f);
                float size = ((750 + 10 * i) / prg * (1 - Mathf.Pow(1e-2f, (1 - a) * (1 + .5f * i))) - 4);
                RankFill[i].rectTransform.sizeDelta = Vector2.one * size;
                RankFill[i].InsideRadius = (size + 112) / (size + 120);
                RankFill[i].color = cp.CurrentChart.Pallete.InterfaceColor * new Color(1 - ease, 1 - ease, 1 - ease, prg);
            }
            yield return null;
        }

        ResultScoreBox.anchoredPosition = new Vector2(-80, 0);
        RecordBox.anchoredPosition = new Vector2(80, 0);
        SetScore(1);

        for (int i = 0; i < RankFill.Count; i++) 
        {
            RankFill[i].rectTransform.sizeDelta = Vector2.one * -4;
            RankFill[i].color = new Color(0, 0, 0, (i + 1f) / RankFill.Count);
            RankFill[i].InsideRadius = 106 / 114f;
        }
        RankBackground.color = RankLabel.color = Color.white;
    
        RankPlayStateBox.anchoredPosition = new Vector2(-RankPlayStateText.preferredWidth / 2, RankPlayStateBox.anchoredPosition.y);

        void SetEase(float a) 
        {
            float ease = Ease.Get(a, EaseFunction.Quintic, EaseMode.Out);
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 180 - (100 * ease));
            RankLabel.color = new Color(1 - a, 1 - a, 1 - a);
            RankExplosion.rectTransform.sizeDelta = Vector2.one * (750 * ease);
            RankExplosion.InsideRadius = ease;
            RankExplosion.color = cp.CurrentChart.Pallete.InterfaceColor * new Color(1, 1, 1, 1 - a);

            ResultSongBox.sizeDelta = new Vector2(ResultSongBox.sizeDelta.x, 40 * ease);
            DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, 40 * ease);
        
            RankPlayStateBox.sizeDelta = new Vector2(RankPlayStateText.preferredWidth * ease, RankPlayStateBox.sizeDelta.y);

            ResultScoreBox.anchoredPosition = new Vector2(-30 * ease - 50, 0);
            RecordBox.anchoredPosition = new Vector2(30 * ease + 50, 0);

            float ease2 = Ease.Get(a * 5 - 4, EaseFunction.Quintic, EaseMode.Out);
            ProfileBar.main.self.anchoredPosition = new Vector2(0, -40 * ease2);
            ActionBar.anchoredPosition = new Vector2(0, 40 * ease2);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / 2f)
        {
            SetEase(a);
            yield return null;
        }
        SetEase(1);
        RankExplosion.rectTransform.sizeDelta = Vector2.zero;

        isAnimating = false;
    }

    public void ToggleExtraDetailMode()
    {
        Debug.Log(isAnimating + " " + ExtraDetailMode);
        if (!isAnimating)
        {
            ExtraDetailMode = !ExtraDetailMode;
            StartCoroutine(ExtraDetailMode ? ExtraDetailEnter() : ExtraDetailExit());
        }
    }

    void ExtraDetailEase(float ease)
    {
        SummaryBox.anchorMin = new Vector2(0, .5f - .35f * ease);
        SummaryBox.anchorMax = new Vector2(1, .5f + .35f * ease);
        SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 80 - 190 * ease);
        SummaryBox.anchoredPosition = new Vector2(0, 5 * ease);
        SummaryImage.color = Color.Lerp(Color.white, Color.black, ease);
        PlayStateText.color = Color.Lerp(new Color(0, 0, 0, .2f), new Color(1, 1, 1, .2f), ease);
        ResultScoreBox.anchoredPosition = new Vector2(-80 - 1000 * ease, 0);
        RecordBox.anchoredPosition = new Vector2(80 + 1000 * ease, 0);

        RankBox.sizeDelta = Vector2.one * (120 + 680 * ease);
        RankLabel.rectTransform.anchoredPosition = new Vector2(0, (Canvas.rect.height / 2 + 100) * ease);
        RankBackground.color = new Color(1, 1, 1, 1 - ease);
        for (int i = 0; i < RankFill.Count; i++) 
        {
            RankFill[i].color = Color.Lerp(new Color(0, 0, 0, (i + 1f) / RankFill.Count), new Color(1, 1, 1, .2f), ease);
            RankFill[i].InsideRadius = (RankBox.sizeDelta.x - 12) / (RankBox.sizeDelta.x - 4);
        }
        RankPlayStateBox.anchoredPosition = new Vector2(RankPlayStateBox.anchoredPosition.x, -(Canvas.rect.height / 2 + 100) * ease);

        ResultSongBox.anchoredPosition = new Vector2(0, -60 + 10 * ease);

        DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, 40 + 10 * ease);
        SimpleDetailBox.anchoredPosition = new Vector2(0, 40 * ease);
        ExtraDetailBox.anchoredPosition = new Vector2(0, -40 * (1 - ease));

        ResultGraphBox.anchoredPosition = new Vector2(0, 12 * ease);
        ResultGraphBox.anchorMax = new Vector2(1, ease);
    }

    IEnumerator ExtraDetailEnter()
    {
        isAnimating = true;
        for (float a = 0; a < 1; a += Time.deltaTime / .6f)
        {
            float ease = Ease.Get(a, EaseFunction.Quintic, EaseMode.InOut);
            ExtraDetailEase(ease);
            yield return null;
        }
        ExtraDetailEase(1);
        isAnimating = false;
    }

    IEnumerator ExtraDetailExit()
    {
        isAnimating = true;
        for (float a = 0; a < 1; a += Time.deltaTime / .6f)
        {
            float ease = Ease.Get(a, EaseFunction.Quintic, EaseMode.InOut);
            ExtraDetailEase(1 - ease);
            yield return null;
        }
        ExtraDetailEase(0);
        isAnimating = false;
    }

    public void Continue()
    {
        StartCoroutine(ContinueAnim());
    }

    IEnumerator ContinueAnim()
    {
        void Lerp(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Exponential, EaseMode.In);
            float ease2 = 1 - Ease.Get(value * 2, EaseFunction.Quintic, EaseMode.Out);

            ExtraDetailEase(ExtraDetailMode ? ease2 : 0);

            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, SummaryBox.sizeDelta.y * (1 - ease));
            ResultScoreBox.anchoredPosition -= new Vector2((Canvas.rect.width + 100) * ease, 0);
            RecordBox.anchoredPosition -= new Vector2((Canvas.rect.width + 100) * ease, 0);
            RankBox.anchoredPosition = new Vector2((Canvas.rect.width + 100) * ease * 2, 0);

            ProfileBar.main.self.anchoredPosition = new Vector2(0, -40 * ease2);
            ActionBar.anchoredPosition = new Vector2(0, 40 * ease2);
            ResultSongBox.sizeDelta = new Vector2(ResultSongBox.sizeDelta.x, 40 * ease2);
            DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, (ExtraDetailMode ? 50 : 40) * ease2);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / 1.2f)
        {
            Lerp(a);
            yield return null;
        }
        Lerp(1);

        ChartPlayer.MetaSongPath = "";

        LoadingBar.main.Show();

        AsyncOperation op = SceneManager.LoadSceneAsync("Song Select", LoadSceneMode.Additive);
        yield return new WaitUntil(() => op.isDone);
        yield return new WaitUntil(() => PlaylistScroll.main.IsReady);
        
        LoadingBar.main.Hide();
        
        SceneManager.UnloadSceneAsync("Player");
        Resources.UnloadUnusedAssets();

    }
}

[System.Serializable] 
public class RankLetter 
{
    public int Threshold;
    public string Letter;
}
