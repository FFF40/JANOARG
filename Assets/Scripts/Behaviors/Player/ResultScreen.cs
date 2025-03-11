using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.IO;

public class ResultScreen : MonoBehaviour
{
    public static ResultScreen main;

    public GameObject Flash;
    public Image FlashBackground;
    public Image ResultBackground;
    public TMP_Text ResultText;
    public TMP_Text ResultTextBig;
    [Space]
    public RectTransform ScoreHolder;
    public TMP_Text ScoreText;
    public TMP_Text RankText;
    public List<GraphicCircle> ScoreRings;
    public List<GraphicCircle> ScoreExplosionRings;
    [Space]
    public CanvasGroup BestScoreHolder;
    public RectTransform BestScoreTransform;
    [Space]
    public CanvasGroup SongInfoHolder;
    public RectTransform SongInfoTransform;
    public TMP_Text SongNameText;
    public TMP_Text SongArtistText;
    public TMP_Text SongDifficultyText;
    [Space]
    public TMP_Text BestScoreText;
    public TMP_Text ScoreDifferenceText;
    [Space]
    public CanvasGroup DetailsHolder;
    public RectTransform DetailsTransform;
    public TMP_Text PerfectCountText;
    public TMP_Text GoodCountText;
    public TMP_Text BadCountText;
    public TMP_Text MaxComboText;
    [Space]
    public CanvasGroup LeftActionsHolder;
    public RectTransform LeftActionsTransform;
    public CanvasGroup RightActionsHolder;
    public RectTransform RightActionsTransform;
    [Space]
    public AudioClip Fanfare;
    public AudioSource FanfareSource;
    public float FanfareStart;
    [Space]
    public Image RetryBackground;
    public Image RetryFlash;
    [HideInInspector]
    public bool IsAnimating;

    public PlayerSettings Settings = new();

    void Awake() 
    {
        main = this;
    }

    public void StartEndingAnim() 
    {
        StartCoroutine(EndingAnim());
    }

    IEnumerator EndingAnim()
    {
        Fanfare.LoadAudioData();
        PlayerScreen.main.PlayerHUD.SetActive(false);
        
        //Added logic to display ALL FLAWLESS or FULL COMBO
        PlayerScreen ps = PlayerScreen.main;
        Transform childTransform = ps.transform.Find("End Flash/Result");
        TMP_Text childTMP = childTransform.GetComponent<TMP_Text>();

        if (ps.TotalCombo == ps.PerfectCount) { } // ALL FLAWLESS
        else if (ps.BadCount == 0 && ps.GoodCount != 0) {
            childTMP.text = "FULL STREAK!";
        } else if (ps.GoodCount == ps.TotalCombo) {
            childTMP.text = "ALL MISALIGNED...!?";
        } else if (ps.BadCount == ps.TotalCombo) {
            childTMP.text = "ALL BROKEN...?";
        } else {
            childTMP.text = "TRACK CLEARED";
        }

        Flash.gameObject.SetActive(true);
        ResultText.gameObject.SetActive(false);
        ResultTextBig.alpha = 0;

        yield return Ease.Animate(1, (x) => {
            FlashBackground.color = new (1, 1, 1, .2f * (1 - x));
            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.y,
                Ease.Get(x, EaseFunction.Circle, EaseMode.In) * 50
            );
        });

        ResultText.gameObject.SetActive(true);
        ResultText.rectTransform.localScale = Vector3.one;
        ResultText.rectTransform.anchoredPosition = Vector2.zero;
        ResultTextBig.text = ResultText.text;
        ResultTextBig.color = PlayerScreen.CurrentChart.Palette.InterfaceColor;

        yield return Ease.Animate(4, (x) => {
            FlashBackground.color = new Color (
                1f - x * 2, 1f - x * 2, 1f - x * 2, 
                Ease.Get(Mathf.Clamp01(x * 2), EaseFunction.Circle, EaseMode.InOut) * .2f + .2f
            );

            ResultTextBig.alpha = 1 - Random.Range(
                Ease.Get(Mathf.Clamp01(x * 4), EaseFunction.Circle, EaseMode.Out),
                Ease.Get(Mathf.Clamp01(x * 2), EaseFunction.Exponential, EaseMode.Out)
            );
            float ease = Mathf.Pow(Ease.Get(x, EaseFunction.Circle, EaseMode.Out), 2);
            ResultText.characterSpacing = 15 / ease;
            ResultTextBig.characterSpacing = 25 * ease - 40;

            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.y,
                Mathf.Pow(Ease.Get(Mathf.Clamp01(x * 4), EaseFunction.Circle, EaseMode.Out), 2) * 50 + 50
            );
        });

        yield return new WaitWhile(() => PlayerScreen.main.CurrentTime < PlayerScreen.main.Music.clip.length);
        StartResultAnim();
    }

    public void StartResultAnim() 
    {
        StartCoroutine(ResultAnim());
    }

    IEnumerator ResultAnim()
    {
        PlayerScreen.main.IsPlaying = false;
        FanfareSource.clip = Fanfare;
        FanfareSource.volume = 0;
        while (!FanfareSource.isPlaying) 
        {
            FanfareSource.Play();
            FanfareSource.time = FanfareStart - 4.5f;
            Debug.Log("Trying play");
            yield return null;
        }
        FanfareSource.loop = true;

        yield return Ease.Animate(1, (x) => {
            PlayerScreen.main.SetInterfaceColor(PlayerScreen.CurrentChart.Palette.InterfaceColor * new Color(1, 1, 1, 1 - x));;
            FanfareSource.volume = (x * .3f) * Settings.BGMusicVolume;
        });

        yield return Ease.Animate(1, (x) => {
            float ease1 = Mathf.Pow(Ease.Get(x, EaseFunction.Circle, EaseMode.In), 2);
            float ease2 = Ease.Get(x, EaseFunction.Quadratic, EaseMode.In);
            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.x,
                ease1 * -10 + 100
            );
            ResultText.rectTransform.localScale = Vector3.one * (1 - ease2 * .1f);
            ResultText.rectTransform.anchoredPosition = Vector2.up * (ease1 * 40);
            FanfareSource.volume = (x * .3f + .3f) * Settings.BGMusicVolume;
        });

        ResultText.gameObject.SetActive(false);
        ScoreHolder.gameObject.SetActive(true);
        ScoreHolder.anchorMin = ScoreHolder.anchorMax = ScoreHolder.pivot = Vector2.one * .5f;
        ScoreHolder.anchoredPosition = Vector2.zero;
        ScoreText.rectTransform.anchoredPosition = new (ScoreText.rectTransform.anchoredPosition.x, -20);
        BestScoreHolder.alpha = 0;
        
        ResultText.rectTransform.localScale = Vector3.one;
        int score = Mathf.RoundToInt(PlayerScreen.main.CurrentExScore / PlayerScreen.main.TotalExScore * 1e6f);
        string rank = Helper.GetRank(score);
        string[] ranks = new [] {"1", "SSS+", "SSS", "SS+", "SS", "S+", "S", "AAA+", "AAA", "AA+", "AA", "A+", "A", "B", "C", "D", "?"};
        int rankNum = System.Array.IndexOf(ranks, rank);

        ScoreExplosionRings[0].color = ScoreExplosionRings[1].color = 
            PlayerScreen.CurrentChart.Palette.InterfaceColor * new Color(1, 1, 1, 0.5f);

        yield return Ease.Animate(2.5f, (x) => {
            float ease1 = 1 - Mathf.Pow(1 - Ease.Get(Mathf.Clamp01(x * 1.5f), EaseFunction.Circle, EaseMode.Out), 2);
            float ease2 = Ease.Get(Mathf.Clamp01(x * 1.5f), EaseFunction.Quadratic, EaseMode.Out);
            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.x,
                ease1 * -10 + 90
            );
            ScoreHolder.localScale = Vector3.one * (1.1f - ease2 * .1f);
            ScoreHolder.anchoredPosition = Vector2.down * (1 - ease1) * 40;

            ScoreText.text = Helper.PadScore((score * x).ToString("#0"));
            RankText.text = ranks[Mathf.CeilToInt(Mathf.Lerp(ranks.Length - 2, rankNum, x))];
            ScoreRings[0].FillAmount = score * Ease.Get(x, EaseFunction.Exponential, EaseMode.Out) / 1e6f;
            ScoreRings[0].SetVerticesDirty();
            for (int a = 1; a < ScoreRings.Count; a++) 
            {
                ScoreRings[a].FillAmount = ScoreRings[a - 1].FillAmount * 10 - 9;
                ScoreRings[a].SetVerticesDirty();
            }

            float ease3 = Ease.Get(x, EaseFunction.Exponential, EaseMode.In);
            ScoreExplosionRings[0].InsideRadius = 1 - ease3 - x * .01f;
            ScoreExplosionRings[0].rectTransform.sizeDelta = Vector2.one * (600 / ease1 * (1 - ease3) + 100);
            ScoreExplosionRings[0].rectTransform.position = ScoreRings[0].rectTransform.position;
            float ease4 = Ease.Get(x * 1.5f - .5f, EaseFunction.Exponential, EaseMode.In);
            ScoreExplosionRings[1].InsideRadius = 1 - ease4 - x * .01f;
            ScoreExplosionRings[1].rectTransform.sizeDelta = Vector2.one * (900 / ease1 * (1 - ease3) + 100);
            ScoreExplosionRings[1].rectTransform.position = ScoreRings[0].rectTransform.position;
            
            FanfareSource.volume = (x * .4f + .6f) * Settings.BGMusicVolume;
        });

        ScoreExplosionRings[2].rectTransform.position = ScoreRings[0].rectTransform.position;
        StartCoroutine(RankExplosionAnim());
        
        SongInfoHolder.alpha = DetailsHolder.alpha = 0;

        SongInfoHolder.gameObject.SetActive(true);
        SongNameText.text = PlayerScreen.TargetSong.SongName;
        SongArtistText.text = PlayerScreen.TargetSong.SongArtist;
        SongDifficultyText.text = Helper.FormatDifficulty(PlayerScreen.TargetChart.Data.DifficultyLevel);

        yield return Ease.Animate(1, (x) => {
            RankText.color = new Color(1 - x, 1 - x, 1 - x);
        });

        DetailsHolder.gameObject.SetActive(true);
        PerfectCountText.text = PlayerScreen.main.PerfectCount.ToString();
        GoodCountText.text = PlayerScreen.main.GoodCount.ToString();
        BadCountText.text = PlayerScreen.main.BadCount.ToString();
        MaxComboText.text = PlayerScreen.main.MaxCombo.ToString() 
            + " <size=75%><b>/ " + PlayerScreen.main.TotalCombo.ToString();

        var record = GetBestScore();
        var recordScore = record?.Score ?? 0;
        var recordDiff = score - recordScore;

        BestScoreText.text       = Helper.PadScore(recordScore.ToString("#0"));
        ScoreDifferenceText.text = (recordDiff >= 0 ? "+" : "") + Helper.PadScore(recordDiff.ToString("#0"));

        SaveScoreEntry(score);

        LeftActionsHolder.gameObject.SetActive(true);
        RightActionsHolder.gameObject.SetActive(true);

        yield return Ease.Animate(0.8f, (x) => {
            float ease1 = 1 - Mathf.Pow(1 - Ease.Get(x, EaseFunction.Circle, EaseMode.Out), 2);
            float ease2 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            ScoreHolder.anchorMin = ScoreHolder.anchorMax = ScoreHolder.pivot = new(.5f * (1 - ease1), .5f);
            ScoreHolder.anchoredPosition = Vector2.right * 50 * ease1;
            ScoreText.rectTransform.anchoredPosition = new (ScoreText.rectTransform.anchoredPosition.x, -20 * (1 - ease2));
            
            float ease3 = Ease.Get(Mathf.Clamp01(x * 2 - 1), EaseFunction.Cubic, EaseMode.Out);
            BestScoreHolder.alpha = SongInfoHolder.alpha = DetailsHolder.alpha
                = LeftActionsHolder.alpha = RightActionsHolder.alpha = ease3;
            ProfileBar.main.SetVisibilty(ease3);
            BestScoreTransform.anchoredPosition = new (-1010 + 10 * ease3, BestScoreTransform.anchoredPosition.y);
            SongInfoTransform.anchoredPosition = new (SongInfoTransform.anchoredPosition.x, 40 - 10 * ease3);
            DetailsTransform.anchoredPosition = new (DetailsTransform.anchoredPosition.x, 10 * ease3 - 50);
            LeftActionsTransform.anchoredPosition = new (-10 * (1 - ease3), LeftActionsTransform.anchoredPosition.y);
            RightActionsTransform.anchoredPosition = new (10 * (1 - ease3), RightActionsTransform.anchoredPosition.y);
        });
    }
    IEnumerator RankExplosionAnim()
    {
        yield return Ease.Animate(2.5f, (x) => {
            FlashBackground.color = new Color (0, 0, 0, .8f - .4f * x);

            float ease2 = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
            ScoreExplosionRings[0].InsideRadius = ease2;
            ScoreExplosionRings[0].rectTransform.sizeDelta = Vector2.one * (700 * ease2 + 100);
            float ease4 = Ease.Get(x * 1.5f, EaseFunction.Exponential, EaseMode.Out);
            ScoreExplosionRings[1].InsideRadius = ease4;
            ScoreExplosionRings[1].rectTransform.sizeDelta = Vector2.one * (1400 * ease4 + 100);
            float ease1 = Ease.Get(x * 1.2f - .2f, EaseFunction.Exponential, EaseMode.Out);
            ScoreExplosionRings[2].InsideRadius = ease1;
            ScoreExplosionRings[2].rectTransform.sizeDelta = Vector2.one * (1600 * ease2 + 100);
            
            FanfareSource.volume = (x * .4f + .6f) * Settings.BGMusicVolume;
        });
    }

    public void Retry() 
    {
        if (!IsAnimating) StartCoroutine(RetryAnim());
    }

    IEnumerator RetryAnim()
    {
        IsAnimating = true;

        ScoreHolder.gameObject.SetActive(false);
        RetryBackground.gameObject.SetActive(true);
        RetryBackground.color = PlayerScreen.TargetSong.BackgroundColor;

        yield return Ease.Animate(1, a => {
            float lerp = Ease.Get(a * 5, EaseFunction.Cubic, EaseMode.Out);
            ProfileBar.main.SetVisibilty(1 - lerp);
            BestScoreHolder.alpha = SongInfoHolder.alpha = DetailsHolder.alpha
                = LeftActionsHolder.alpha = RightActionsHolder.alpha = 1 - lerp;
            
            SongInfoTransform.anchoredPosition = new (SongInfoTransform.anchoredPosition.x, 30 + 10 * lerp);
            DetailsTransform.anchoredPosition = new (DetailsTransform.anchoredPosition.x, -10 * lerp - 40);
            LeftActionsTransform.anchoredPosition = new (-10 * lerp, LeftActionsTransform.anchoredPosition.y);
            RightActionsTransform.anchoredPosition = new (10 * lerp, RightActionsTransform.anchoredPosition.y);

            float lerp2 = Mathf.Pow(Ease.Get(a, EaseFunction.Circle, EaseMode.In), 2);
            RetryBackground.rectTransform.anchorMin = new(0, .5f * (1 - lerp2));
            RetryBackground.rectTransform.anchorMax = new(1, 1 - .5f * (1 - lerp2));
            RetryBackground.rectTransform.sizeDelta = new(0, ResultBackground.rectTransform.sizeDelta.y * (1 - lerp2));
            
            float lerp3 = Mathf.Pow(Ease.Get(a, EaseFunction.Exponential, EaseMode.Out), 0.5f);
            RetryFlash.color = new (1, 1, 1, 1 - lerp3);
        });

        RetryBackground.gameObject.SetActive(false);
        SongInfoHolder.gameObject.SetActive(false);
        LeftActionsHolder.gameObject.SetActive(false);
        RightActionsHolder.gameObject.SetActive(false);
        DetailsHolder.gameObject.SetActive(false);
        Flash.gameObject.SetActive(false);
        Common.main.MainCamera.backgroundColor = PlayerScreen.TargetSong.BackgroundColor;

        yield return PlayerScreen.main.InitChart();
        PlayerScreen.main.PlayerHUD.SetActive(true);
        PlayerScreen.main.BeginReadyAnim();

        IsAnimating = false;
    }

    public void Continue() 
    {
        if (!IsAnimating) StartCoroutine(ContinueAnim());
    }

    IEnumerator ContinueAnim()
    {
        IsAnimating = true;
        
        Color backColor = FlashBackground.color;

        yield return Ease.Animate(1, a => {
            float lerp = Ease.Get(a * 5, EaseFunction.Cubic, EaseMode.Out);
            ProfileBar.main.SetVisibilty(1 - lerp);
            SongInfoHolder.alpha = DetailsHolder.alpha
                = LeftActionsHolder.alpha = RightActionsHolder.alpha = 1 - lerp;
            
            SongInfoTransform.anchoredPosition = new (SongInfoTransform.anchoredPosition.x, 30 + 10 * lerp);
            DetailsTransform.anchoredPosition = new (DetailsTransform.anchoredPosition.x, -10 * lerp - 40);
            LeftActionsTransform.anchoredPosition = new (-10 * lerp, LeftActionsTransform.anchoredPosition.y);
            RightActionsTransform.anchoredPosition = new (10 * lerp, RightActionsTransform.anchoredPosition.y);
            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.x,
                80 * (1 - lerp)
            );

            float lerp2 = Mathf.Pow(Ease.Get(a, EaseFunction.Circle, EaseMode.In), 2);
            ScoreHolder.pivot = new(lerp2, .5f);
            ScoreHolder.anchorMin = ScoreHolder.anchorMax = new(-lerp2, .5f);
            
            float lerp3 = Ease.Get(a, EaseFunction.Exponential, EaseMode.InOut);
            FlashBackground.color = Color.Lerp(backColor, Common.main.MainCamera.backgroundColor, lerp3);
        });

        RetryBackground.gameObject.SetActive(false);
        SongInfoHolder.gameObject.SetActive(false);
        LeftActionsHolder.gameObject.SetActive(false);
        RightActionsHolder.gameObject.SetActive(false);
        DetailsHolder.gameObject.SetActive(false);
        Flash.gameObject.SetActive(false);

        yield return new WaitForSeconds(1);

        LoadingBar.main.Show();
        Common.Load("Song Select", () => !LoadingBar.main.IsAnimating && (SongSelectScreen.main?.IsInit == true), () => {
            LoadingBar.main.Hide();
            SongSelectScreen.main.Intro();
        }, false);
        SceneManager.UnloadSceneAsync("Player");
        Resources.UnloadUnusedAssets();

        IsAnimating = false;
    }

    void SaveScoreEntry(int score)
    {
        ScoreStoreEntry entry = new ScoreStoreEntry
        {
            SongID = Path.GetFileNameWithoutExtension(PlayerScreen.TargetSongPath),
            ChartID = PlayerScreen.TargetChartMeta.Target,

            Score = score,
            PerfectCount = PlayerScreen.main.PerfectCount,
            GoodCount = PlayerScreen.main.GoodCount,
            BadCount = PlayerScreen.main.BadCount,
            MaxCombo = PlayerScreen.main.MaxCombo
        };

        StorageManager.main.Scores.Register(entry);
        StorageManager.main.Save();
    }

    ScoreStoreEntry GetBestScore()
    {
        string songID = Path.GetFileNameWithoutExtension(PlayerScreen.TargetSongPath);
        string chartID = PlayerScreen.TargetChartMeta.Target;
        return StorageManager.main.Scores.Get(songID, chartID);
    }
}
