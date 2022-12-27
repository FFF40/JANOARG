
// TODO: new result screen

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResultScreen : MonoBehaviour
{

    public static ResultScreen main;
    
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
    public List<RankLetter> Ranks;
    public RectTransform RankBox;
    public Image RankBackground;
    public List<GraphicCircle> RankFill;
    public GraphicCircle RankExplosion;
    public TMP_Text RankLabel;
    [Space]
    public RectTransform ResultScoreBox;
    public LayoutGroup ResultScoreLayout;
    public TMP_Text ResultScoreText;
    public TMP_Text ResultScoreLabel;
    [Space]
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
        if (cp.Score >= cp.TotalScore) PlayStateText.text = "TRACK MASTERED!!";
        else if (cp.Combo >= cp.TotalCombo) PlayStateText.text = "FULL STREAK!";
        else if (cp.Score <= 0) PlayStateText.text = "TRACK CLEARED?";
        else PlayStateText.text = "TRACK CLEARED";

        cp.PlayInterfaceObject.SetActive(false);
        ResultInterfaceObject.SetActive(true);

        SummaryBox.anchorMin = new Vector2(0, .5f);
        SummaryBox.anchorMax = new Vector2(1, .5f);
        SummaryBox.anchoredPosition = Vector2.zero;
        PlayStateText.rectTransform.anchorMin = PlayStateText.rectTransform.anchorMax = new Vector2(.5f, .5f);
        PlayStateText.rectTransform.anchoredPosition = Vector2.zero;
        ResultSongBox.sizeDelta = new Vector2(ResultSongBox.sizeDelta.x, 0);
        DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, 0);

        RankBackground.color = RankLabel.color = Color.clear;
        foreach (GraphicCircle fill in RankFill)
        {
            fill.color = Color.clear;
        }

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
        cp.ComboText.fontSize = 120;
        PlayStateText.fontSize = 32;
        isAnimating = false;

        StartCoroutine(ResultAnimation());
    }

    public IEnumerator ResultAnimation () 
    {
        isAnimating = true;

        ChartPlayer cp = ChartPlayer.main;

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
        
        ResultAccuracyTooEarlyText.text = cp.AccuracyValues[0].ToString("0", CultureInfo.InvariantCulture);
        int early = 0;
        for (int i = 1; i <= 10; i++) early += cp.AccuracyValues[i];
        ResultAccuracyEarlyText.text = early.ToString("0", CultureInfo.InvariantCulture);
        ResultAccuracyPerfectText.text = cp.AccuracyValues[11].ToString("0", CultureInfo.InvariantCulture);
        int late = 0;
        for (int i = 12; i <= 21; i++) late += cp.AccuracyValues[i];
        ResultAccuracyLateText.text = late.ToString("0", CultureInfo.InvariantCulture);
        ResultAccuracyTooLateText.text = cp.AccuracyValues[22].ToString("0", CultureInfo.InvariantCulture);

        ResultDiscretePassText.text = cp.DiscreteValues[0].ToString("0", CultureInfo.InvariantCulture);
        ResultDiscreteFailText.text = cp.DiscreteValues[1].ToString("0", CultureInfo.InvariantCulture);

        ResultTimingAverageText.text = (cp.OffsetMean < 0 ? "−" : "+") + Mathf.Abs(cp.OffsetMean * 1e3f).ToString("0.00", CultureInfo.InvariantCulture) + "ms";
        ResultTimingDeviationText.text = (cp.Deviation * 1e3f).ToString("0.00", CultureInfo.InvariantCulture) + "ms";

        float max = Mathf.Max(cp.AccuracyValues);
        for (int a = 0; a < 23; a++) 
        {
            Image bar = Instantiate(ResultBarSample, ResultGraphBox);
            int dist = Mathf.Abs(a - 11);
            bar.color = dist == 0 ? PerfectBarColor : dist == 11 ? BadBarColor : Color.Lerp(HighGoodBarColor, LowGoodBarColor, (dist - 1) / 10f);
            bar.rectTransform.anchorMin = new Vector2(.035f * a, 0);
            bar.rectTransform.anchorMax = new Vector2(.035f * a + .03f, cp.AccuracyValues[a] / max);
            bar.rectTransform.sizeDelta = Vector2.zero;
        }

        max = Mathf.Max(cp.DiscreteValues);
        for (int a = 0; a < 2; a++) 
        {
            Image bar = Instantiate(ResultBarSample, ResultGraphBox);
            bar.color = a == 0 ? PerfectBarColor : BadBarColor;
            bar.rectTransform.anchorMin = new Vector2(.035f * a + .935f, 0);
            bar.rectTransform.anchorMax = new Vector2(.035f * a + .965f, cp.DiscreteValues[a] / max);
            bar.rectTransform.sizeDelta = Vector2.zero;
        }
        
        foreach (RankLetter rank in Ranks) 
        {
            if (cp.Score / Mathf.Max(cp.TotalScore, 1) * 1e6 >= rank.Threshold) currentRank = rank;
            else break;
        }
        RankLabel.text = currentRank.Letter;

        for (float a = 0; a < 1; a += Time.deltaTime)
        {
            float ease = Ease.Get(a, "Circle", EaseMode.In);
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 50 + (50 * ease));
            PlayStateText.fontSize = 32 * (1 - ease);
            PlayStateText.characterSpacing = 50 / Mathf.Pow(1 - ease, 1.5f);
            yield return null;
        }
        PlayStateText.fontSize = 0;

        
        ResultScoreBox.anchorMin = ResultScoreBox.anchorMax = ResultScoreBox.pivot = new Vector2(.5f, .5f);
        ResultScoreBox.anchoredPosition = new Vector2(0, 0);
        ResultScoreBox.sizeDelta = new Vector2(ResultScoreLayout.preferredWidth, 0);

        ResultScoreBox.anchorMin = ResultScoreBox.anchorMax = ResultScoreBox.pivot = new Vector2(1, .5f);
        ResultScoreBox.anchoredPosition = new Vector2(-340, 0);

        void SetScore(float a) 
        {
            float score = cp.Score / Mathf.Max(cp.TotalScore, 1) * a;
            ResultScoreText.text = ((int)(score * 1e6)).ToString("D7", CultureInfo.InvariantCulture);
            for (int i = 0; i < RankFill.Count; i++) 
            {
                RankFill[i].FillAmount = score;
                score = 10 * score - 9;
            }
        }
        for (float a = 0; a < 1; a += Time.deltaTime / 2f)
        {
            float ease = Ease.Get(a, "Circle", EaseMode.In);
            float height = 100 + 80 * Ease.Get(a, "Circle", EaseMode.Out);
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, height);
            
            float ease2 = Ease.Get(a, "Exponential", EaseMode.In);
            ResultScoreBox.anchorMin = ResultScoreBox.anchorMax = ResultScoreBox.pivot = new Vector2(.5f + .5f * ease2, .5f);
            ResultScoreBox.anchoredPosition = new Vector2(-340 * ease2, 0);
            
            SetScore(1 - Mathf.Pow(1e-7f, a));
            ResultScoreBox.localScale = Vector3.one / Mathf.Max(1 - Mathf.Pow(1 - a, 5), 1e-7f);
            for (int i = 0; i < RankFill.Count; i++) 
            {
                float prg = Mathf.Max(1 - Mathf.Pow(1 - (a * (1 + .1f * i) - .1f * i), 5), 1e-7f);
                float size = ((1000 + 30 * i) / prg * (1 - Mathf.Pow(1e-2f, (1 - a) * (1 + .5f * i))) - 4);
                RankFill[i].rectTransform.sizeDelta = Vector2.one * size;
                RankFill[i].InsideRadius = (size + 95) / (size + 100);
                RankFill[i].color = new Color(1 - ease2, 1 - ease2, 1 - ease2, prg);
            }
            yield return null;
        }
        ResultScoreBox.anchorMin = ResultScoreBox.anchorMax = ResultScoreBox.pivot = new Vector2(1, .5f);
        ResultScoreBox.anchoredPosition = new Vector2(-340, 0);
        SetScore(1);
        for (int i = 0; i < RankFill.Count; i++) 
        {
            RankFill[i].rectTransform.sizeDelta = Vector2.one * -4;
            RankFill[i].color = new Color(0, 0, 0, (i + 1f) / RankFill.Count);
            RankFill[i].InsideRadius = 89 / 94f;
        }
        ResultScoreBox.localScale = Vector3.one;
        RankBackground.color = RankLabel.color = Color.white;

        PlayStateText.fontSize = 32;
        PlayStateText.characterSpacing = 10;
        PlayStateText.rectTransform.anchorMin = PlayStateText.rectTransform.anchorMax = new Vector2(0, .5f);

        void SetEase(float a) 
        {
            float ease = Ease.Get(a, "Quintic", EaseMode.Out);
            SummaryBox.sizeDelta = new Vector2(SummaryBox.sizeDelta.x, 180 - (100 * ease));
            RankLabel.color = new Color(1 - a, 1 - a, 1 - a);
            RankExplosion.rectTransform.sizeDelta = Vector2.one * (1000 * ease);
            RankExplosion.InsideRadius = ease;
            RankExplosion.color = new Color(1, 1, 1, 1 - a);

            PlayStateText.rectTransform.anchoredPosition = new Vector2(PlayStateText.preferredWidth / 2 - 400 * (1 - ease), 12);

            ResultSongBox.sizeDelta = new Vector2(ResultSongBox.sizeDelta.x, 40 * ease);
            DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, 40 * ease);
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
        PlayStateText.color = ResultScoreText.color = ResultScoreLabel.color = Color.Lerp(Color.black, new Color(1, 1, 1, .2f), ease);
        ResultScoreBox.anchoredPosition = new Vector2(-340 + 115 * ease, 0);
        RankBox.anchoredPosition = new Vector2(-25 + 1000 * ease, 0);

        ResultSongBox.anchoredPosition = new Vector2(0, -60 + 10 * ease);

        DetailsBox.sizeDelta = new Vector2(DetailsBox.sizeDelta.x, 40 + 10 * ease);
        SimpleDetailBox.anchoredPosition = new Vector2(0, 40 * ease);
        ExtraDetailBox.anchoredPosition = new Vector2(0, -40 * (1 - ease));

        ResultGraphBox.anchorMax = new Vector2(1, ease);
    }

    IEnumerator ExtraDetailEnter()
    {
        isAnimating = true;
        for (float a = 0; a < 1; a += Time.deltaTime / .6f)
        {
            float ease = Ease.Get(a, "Quintic", EaseMode.InOut);
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
            float ease = Ease.Get(a, "Quintic", EaseMode.InOut);
            ExtraDetailEase(1 - ease);
            yield return null;
        }
        ExtraDetailEase(0);
        isAnimating = false;
    }
}

[System.Serializable] 
public class RankLetter 
{
    public int Threshold;
    public string Letter;
}
