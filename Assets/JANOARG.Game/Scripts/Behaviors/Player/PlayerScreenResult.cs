using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class PlayerScreenResult : MonoBehaviour
{
    public static PlayerScreenResult main;

    public GameObject Flash;
    public Image FlashBackground;
    public Image ResultBackground;
    public TMP_Text ResultText;
    public TMP_Text ResultTextBig;
    [Space]
    public RectTransform ScoreHolder;
    public TMP_Text ScoreText;
    public RectTransform RankHolder;
    public TMP_Text RankText;
    public List<Image> ScoreRings;
    public List<GraphicCircle> ScoreExplosionRings;
    public RectTransform ScoreBarHolder;
    public RectTransform ScoreBarFill;
    public List<RectTransform> ScoreBarMarks;
    [Space]
    public PlayerScreenResultDetails Details;
    [Space]
    public CanvasGroup BestScoreHolder;
    public RectTransform BestScoreTransform;
    [Space]
    public CanvasGroup SongInfoHolder;
    public RectTransform SongInfoTransform;
    public TMP_Text SongNameText;
    public TMP_Text SongArtistText;
    public TMP_Text SongDifficultyText;
    public RawImage SongCoverImage;
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
    [HideInInspector]
    public bool IsDetailsShowing;

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
        
        PlayerScreen ps = PlayerScreen.main;

        if (ps.TotalCombo == ps.PerfectCount) {
            ResultText.text = "ALL FLAWLESS!!!";
        } else if (ps.BadCount == 0 && ps.GoodCount != 0) {
            ResultText.text = "FULL STREAK!";
        } else if (ps.GoodCount == ps.TotalCombo) {
            ResultText.text = "ALL MISALIGNED...!?";
        } else if (ps.BadCount == ps.TotalCombo) {
            ResultText.text = "ALL BROKEN...?";
        } else {
            ResultText.text = "TRACK CLEARED";
        }

        Flash.SetActive(true);
        ResultText.gameObject.SetActive(false);
        ResultText.fontSize = 50;
        ResultTextBig.text = ResultText.text;
        ResultTextBig.alpha = 0;

        foreach (var ring in ScoreExplosionRings) ring.rectTransform.localPosition = Vector2.zero;
        ScoreExplosionRings[0].color = ScoreExplosionRings[1].color = 
            PlayerScreen.CurrentChart.Palette.InterfaceColor * new Color(1, 1, 1, 0.5f);

        StartCoroutine(Ease.Animate(2, x => {
            FlashBackground.color = new (1, 1, 1, .2f * (1 - x));
            ScoreExplosionRings[0].InsideRadius = 0.9f * Ease.Get(x, EaseFunction.Quintic, EaseMode.Out);
            ScoreExplosionRings[0].rectTransform.sizeDelta = Vector2.one * (200 * Ease.Get(x, EaseFunction.Circle, EaseMode.Out));
            ScoreExplosionRings[0].rectTransform.localEulerAngles = Vector3.forward * (55 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));
        }));

        yield return Ease.Animate(1, (x) => {

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

            ScoreExplosionRings[1].InsideRadius = 0.95f * Ease.Get(x * 2f, EaseFunction.Quintic, EaseMode.Out);
            ScoreExplosionRings[1].rectTransform.sizeDelta = Vector2.one * (500 * Ease.Get(x * 2f, EaseFunction.Circle, EaseMode.Out));
            ScoreExplosionRings[1].rectTransform.localEulerAngles = Vector3.forward * (-90 + 360 * Ease.Get(x * 1.5f, EaseFunction.Cubic, EaseMode.Out));
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
            PlayerScreen.main.SetInterfaceColor(PlayerScreen.CurrentChart.Palette.InterfaceColor * new Color(1, 1, 1, 1 - x));
            FanfareSource.volume = (x * .3f) * Settings.BGMusicVolume;
        });

        Details.gameObject.SetActive(true);
        Details.Reset();
        IsDetailsShowing = false;

        yield return Ease.Animate(1, (x) => {
            float ease1 = Mathf.Pow(Ease.Get(x, EaseFunction.Circle, EaseMode.InOut), 2);
            float ease2 = Ease.Get(x, EaseFunction.Quadratic, EaseMode.In);
            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.x,
                ease1 * -10 + 100
            );
            ResultText.fontSize = 50 * (1 - ease1);
            ResultText.characterSpacing = 15 / Mathf.Pow(1 - ease1, 3);
            FanfareSource.volume = (x * .3f + .3f) * Settings.BGMusicVolume;
            Details.Container.rectTransform.localScale = new (1, .5f * ease2, 1);

            ScoreExplosionRings[0].rectTransform.sizeDelta = Vector2.one * (200 / (1 - Ease.Get(x, EaseFunction.Exponential, EaseMode.In)));
            ScoreExplosionRings[0].rectTransform.localEulerAngles = Vector3.forward * (55 + 360 * Ease.Get(x, EaseFunction.Cubic, EaseMode.In));
            ScoreExplosionRings[1].rectTransform.sizeDelta = Vector2.one * (500 / (1 - Ease.Get(x, EaseFunction.Circle, EaseMode.In)));
        });

        ResultText.gameObject.SetActive(false);
        ScoreHolder.gameObject.SetActive(true);
        ScoreHolder.anchoredPosition = Vector2.zero;
        ScoreText.rectTransform.anchoredPosition *= new Vector2Frag(x: 0);
        ScoreBarHolder.anchoredPosition *= new Vector2Frag(x: 100);
        SongInfoTransform.anchoredPosition *= new Vector2Frag(x: 150);
        RankHolder.localScale = Vector2.zero;
        BestScoreHolder.alpha = 0;
        
        ResultText.rectTransform.localScale = Vector3.one;
        int score = Mathf.RoundToInt(PlayerScreen.main.CurrentExScore / PlayerScreen.main.TotalExScore * 1e6f);
        string rank = Helper.GetRank(score);

        ScoreExplosionRings[0].color = ScoreExplosionRings[1].color = 
            PlayerScreen.CurrentChart.Palette.InterfaceColor * new Color(1, 1, 1, 0.5f);
        
        int markIndex = 0;
        foreach (var mark in ScoreBarMarks) mark.gameObject.SetActive(false);

        yield return Ease.Animate(2.5f, (x) => {
            float ease1 = 1 - Mathf.Pow(1 - Ease.Get(Mathf.Clamp01(x * 1.5f), EaseFunction.Circle, EaseMode.Out), 2);
            float ease2 = Ease.Get(Mathf.Clamp01(x * 1.5f), EaseFunction.Quadratic, EaseMode.Out);
            float ease3 = Ease.Get(x, EaseFunction.Quadratic, EaseMode.Out);
            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.x,
                ease1 * -10 + 90
            );
            ScoreHolder.anchoredPosition = new (50 - 550 * (1 - ease1) * (1 - ease1), 0);
            ScoreText.rectTransform.anchoredPosition *= new Vector2Frag(x: 150 * ease3 - 100);
            Details.SpawnPins(ease2);
            Details.Container.rectTransform.anchoredPosition *= new Vector2Frag(x: 500 * (1 - ease1));
            Details.Container.rectTransform.localScale = new (1, .5f + .5f * ease1, 1);
            
            float scoreLerp = score * Ease.Get(x, EaseFunction.Quintic, EaseMode.Out);
            ScoreText.text = Helper.PadScore(scoreLerp.ToString("#0"));
            scoreLerp /= 1e6f;
            ScoreRings[0].fillAmount = scoreLerp;
            for (int a = 1; a < ScoreRings.Count; a++) 
            {
                ScoreRings[a].fillAmount = ScoreRings[a - 1].fillAmount * 10 - 9;
            }
            ScoreBarHolder.anchorMin *= new Vector2Frag(x: -4 * Mathf.Pow(scoreLerp, 6));
            ScoreBarFill.anchorMax *= new Vector2Frag(x: scoreLerp);

            float ease4 = Ease.Get(x, EaseFunction.Exponential, EaseMode.In);
            ScoreExplosionRings[0].InsideRadius = 1 - ease4 - x * .01f;
            ScoreExplosionRings[0].rectTransform.sizeDelta = Vector2.one * (600 / ease1 * (1 - ease4) + 100);
            ScoreExplosionRings[0].rectTransform.position = ScoreRings[0].rectTransform.position;
            ScoreExplosionRings[0].rectTransform.localEulerAngles = Vector3.forward * (55 + 360 * Ease.Get(x * 1.2f, EaseFunction.Cubic, EaseMode.Out));
            float ease5 = Ease.Get(x * 1.5f - .5f, EaseFunction.Exponential, EaseMode.In);
            ScoreExplosionRings[1].InsideRadius = 1 - ease5 - x * .01f;
            ScoreExplosionRings[1].rectTransform.sizeDelta = Vector2.one * (900 / ease1 * (1 - ease4) + 100);
            ScoreExplosionRings[1].rectTransform.position = ScoreRings[0].rectTransform.position;
            ScoreExplosionRings[1].rectTransform.localEulerAngles = Vector3.forward * (55 + 360 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));

            if (markIndex < ScoreBarMarks.Count && scoreLerp >= ScoreBarMarks[markIndex].anchorMin.x)
            {
                ScoreBarMarks[markIndex].gameObject.SetActive(true);
                StartCoroutine(AnimateScoreBarMark(ScoreBarMarks[markIndex]));
                markIndex++;
            }
            
            FanfareSource.volume = (x * .4f + .6f) * Settings.BGMusicVolume;
        });

        RankText.text = rank.Replace("+", "<size=70%><voffset=.5em>+");
        ScoreExplosionRings[2].rectTransform.position = ScoreRings[0].rectTransform.position;
        StartCoroutine(RankExplosionAnim());
        
        SongInfoHolder.alpha = DetailsHolder.alpha = 0;

        SongInfoHolder.gameObject.SetActive(true);
        SongNameText.text = PlayerScreen.TargetSong.SongName;
        SongArtistText.text = PlayerScreen.TargetSong.SongArtist;
        SongDifficultyText.text = Helper.FormatDifficulty(PlayerScreen.TargetChart.Data.DifficultyLevel);
        SongDifficultyText.color = Common.main.Constants.GetDifficultyColor(PlayerScreen.TargetChartMeta.DifficultyIndex);

        if (!SongCoverImage.texture) 
        {
            StartCoroutine(LoadCoverImageRoutine());
        }

        yield return Ease.Animate(1.2f, (x) => {
            float ease1 = Ease.Get(Mathf.Pow(x, .3f), EaseFunction.Back, EaseMode.Out);
            float ease2 = Ease.Get(Mathf.Pow(x, 2), EaseFunction.Exponential, EaseMode.In);
            float ease3 = Ease.Get(Mathf.Pow(x, .4f), EaseFunction.Exponential, EaseMode.Out);
            RankHolder.localScale = Vector2.one * (ease1 * 1.1f - ease2 * .1f);
            RankHolder.localEulerAngles = -(RankText.rectTransform.localEulerAngles *= new Vector3Frag(z: -10 + 90 * (1 - ease3)));
            RankText.color = new Color(1 - x, 1 - x, 1 - x);
            ScoreBarHolder.anchoredPosition *= new Vector2Frag(x: 100 - 1000 * ease2);
            ScoreText.rectTransform.anchoredPosition *= new Vector2Frag(x: 50 + 110 * ease3);
        });

        DetailsHolder.gameObject.SetActive(true);
        PerfectCountText.text = PlayerScreen.main.PerfectCount.ToString("N0");
        GoodCountText.text = PlayerScreen.main.GoodCount.ToString("N0");
        BadCountText.text = PlayerScreen.main.BadCount.ToString("N0");
        MaxComboText.text = PlayerScreen.main.MaxCombo.ToString("N0") 
            + " <size=60%><b>/ " + PlayerScreen.main.TotalCombo.ToString("N0");

        var record = GetBestScore();
        var recordScore = record?.Score ?? 0;
        var recordDiff = score - recordScore;

        BestScoreText.text       = Helper.PadScore(recordScore.ToString("#0"));
        ScoreDifferenceText.text = (recordDiff >= 0 ? "+" : "−") + Helper.PadScore(Mathf.Abs(recordDiff).ToString("#0"));

        CalculateGainsAndSave(score);

        LeftActionsHolder.gameObject.SetActive(true);
        RightActionsHolder.gameObject.SetActive(true);

        yield return Ease.Animate(0.4f, (x) => {
            float ease3 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
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

    IEnumerator AnimateScoreBarMark(RectTransform mark)
    {
        yield return Ease.Animate(0.6f, a => {
            float ease1 = Ease.Get(Mathf.Pow(a, .5f), EaseFunction.Back, EaseMode.Out);
            mark.localScale = Vector3.one * ease1;
            mark.localEulerAngles = Vector3.back * (90 * (1 - ease1));
        });
    }

    void CalculateGainsAndSave(int score)
    {
        float baseOrbs = Helper.CalculateBaseSongGain(PlayerScreen.TargetSong, PlayerScreen.CurrentChart, score);
        float baseCoins = baseOrbs / 5 + PlayerScreen.TargetSong.Clip.length * PlayerScreen.CurrentChart.ChartConstant * score / 600e6f;
        if (PlayerScreen.main.BadCount == 0) 
        {
            baseOrbs *= 1.2f;
            baseCoins *= 1.05f;
            if (PlayerScreen.main.GoodCount == 0) 
            {
                baseOrbs *= 1.2f;
                baseCoins *= 1.05f;
            }
        }
        SaveScoreEntry(score);
        ProfileBar.main.CompleteSong((long)baseOrbs, (long)baseCoins);
        StorageManager.main.Save();
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
            ScoreExplosionRings[2].rectTransform.sizeDelta = Vector2.one * (2400 * ease2 + 100);
            
            FanfareSource.volume = (x * .4f + .6f) * Settings.BGMusicVolume;
        });
    }
    private IEnumerator LoadCoverImageRoutine() 
    {
        string path = Path.Combine(Path.GetDirectoryName(PlayerScreen.TargetSongPath), PlayerScreen.TargetSong.Cover.IconTarget);
        if (Path.HasExtension(path)) path = Path.ChangeExtension(path, "")[0..^1];
        ResourceRequest req = Resources.LoadAsync<Texture2D>(path);
        yield return new WaitUntil(() => req.isDone);
        if (req.asset) 
        {
            Texture2D tex = (Texture2D)req.asset;
            SongCoverImage.texture = tex;
        }
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
        Details.gameObject.SetActive(false);
        RetryBackground.color = PlayerScreen.TargetSong.BackgroundColor;

        yield return Ease.Animate(1, a => {
            float lerp = Ease.Get(a * 5, EaseFunction.Cubic, EaseMode.Out);
            ProfileBar.main.SetVisibilty(1 - lerp);
            BestScoreHolder.alpha = SongInfoHolder.alpha = DetailsHolder.alpha
                = LeftActionsHolder.alpha = RightActionsHolder.alpha = 1 - lerp;

            FanfareSource.volume = (1 - a) * Settings.BGMusicVolume;
            
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

        yield return new WaitForSeconds(1);

        RetryBackground.gameObject.SetActive(false);
        SongInfoHolder.gameObject.SetActive(false);
        LeftActionsHolder.gameObject.SetActive(false);
        RightActionsHolder.gameObject.SetActive(false);
        DetailsHolder.gameObject.SetActive(false);
        Flash.SetActive(false);
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
        if (IsDetailsShowing) StartCoroutine(Ease.Animate(0.4f, (x) => {
            float ease1 = Ease.Get(Mathf.Pow(x, .4f), EaseFunction.Exponential, EaseMode.Out);
            Details.LerpDetailed(ease1);
        }));

        yield return Ease.Animate(1, a => {
            float lerp = Ease.Get(a * 5, EaseFunction.Cubic, EaseMode.Out);
            ProfileBar.main.SetVisibilty(1 - lerp);
            SongInfoHolder.alpha = DetailsHolder.alpha
                = LeftActionsHolder.alpha = RightActionsHolder.alpha = 1 - lerp;

            FanfareSource.volume = (1 - a) * Settings.BGMusicVolume;
            
            SongInfoTransform.anchoredPosition *= new Vector2Frag(null, 30 + 10 * lerp);
            DetailsTransform.anchoredPosition *= new Vector2Frag(null, -10 * lerp - 40);
            LeftActionsTransform.anchoredPosition *= new Vector2Frag(-10 * lerp, null);
            RightActionsTransform.anchoredPosition *= new Vector2Frag(10 * lerp, null);
            ResultBackground.rectTransform.sizeDelta *= new Vector2Frag(null, 80 * (1 - lerp));

            float lerp2 = Mathf.Pow(Ease.Get(a, EaseFunction.Circle, EaseMode.In), 2);
            ScoreHolder.pivot = new(lerp2, .5f);
            ScoreHolder.anchorMin = ScoreHolder.anchorMax = new(-lerp2, .5f);
            Details.Container.rectTransform.localScale = new (1, 1 - lerp2, 1);
            Details.Container.rectTransform.anchoredPosition *= new Vector2Frag(x: -500 * lerp2);
            
            float lerp3 = Ease.Get(a, EaseFunction.Exponential, EaseMode.InOut);
            FlashBackground.color = Color.Lerp(backColor, Common.main.MainCamera.backgroundColor, lerp3);
            IsAnimating = true;
        });

        Details.gameObject.SetActive(false);
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

    public void ToggleDetails() 
    {
        if (!IsAnimating) StartCoroutine(ToggleDetailAnim());
    }

    IEnumerator ToggleDetailAnim()
    {
        IsAnimating = true;

        IsDetailsShowing = !IsDetailsShowing;
        float target = IsDetailsShowing ? 0 : 1;

        if (target == 0) Details.UpdateLabels();

        yield return Ease.Animate(0.4f, (x) => {
            float ease1 = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
            float prog1 = 1 - Mathf.Abs(target - 1 + ease1);
            SongInfoTransform.anchoredPosition *= new Vector2Frag(x: 150 - 1150 * prog1);
            ScoreHolder.anchoredPosition *= new Vector2Frag(x: 50 - 1150 * prog1);

            Details.LerpDetailed(target - 1 + (target == 0 ? x : ease1));
        });

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
            MaxCombo = PlayerScreen.main.MaxCombo,
            Rating = Helper.GetRating(PlayerScreen.TargetChartMeta.ChartConstant, score),
        };
        StorageManager.main.Scores.Register(entry);
    }

    ScoreStoreEntry GetBestScore()
    {
        string songID = Path.GetFileNameWithoutExtension(PlayerScreen.TargetSongPath);
        string chartID = PlayerScreen.TargetChartMeta.Target;
        return StorageManager.main.Scores.Get(songID, chartID);
    }
}
