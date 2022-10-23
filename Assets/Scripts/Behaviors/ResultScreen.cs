
// TODO: new result screen

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultScreen : MonoBehaviour
{

    public static ResultScreen main;
    
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
    public TMP_Text ResultPerfectText;
    public TMP_Text ResultGoodText;
    public TMP_Text ResultBadText;
    public TMP_Text ResultComboText;

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
        ChartPlayer cp = ChartPlayer.main;

        ResultSongNameText.text = cp.Song.SongName;
        ResultSongArtistText.text = cp.Song.SongArtist;
        ResultDifficultyLabel.text = cp.CurrentChart.DifficultyLevel;

        ResultPerfectText.text = cp.PerfectCount.ToString("0", CultureInfo.InvariantCulture);
        ResultGoodText.text = cp.GoodCount.ToString("0", CultureInfo.InvariantCulture);
        ResultBadText.text = cp.BadCount.ToString("0", CultureInfo.InvariantCulture);
        ResultComboText.text = cp.MaxCombo.ToString("0", CultureInfo.InvariantCulture) + " / " + cp.TotalCombo.ToString("0", CultureInfo.InvariantCulture);

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
    }
}

[System.Serializable] 
public class RankLetter 
{
    public int Threshold;
    public string Letter;
}
