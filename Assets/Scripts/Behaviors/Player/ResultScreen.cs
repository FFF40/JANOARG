using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultScreen : MonoBehaviour
{
    public static ResultScreen main;

    public GameObject Flash;
    public Image FlashBackground;
    public Image ResultBackground;
    public TMP_Text ResultText;
    [Space]
    public RectTransform ScoreHolder;
    public TMP_Text ScoreText;
    public TMP_Text RankText;
    public List<GraphicCircle> ScoreRings;
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
    public CanvasGroup DetailsHolder;
    public RectTransform DetailsTransform;

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
        PlayerScreen.main.PlayerHUD.SetActive(false);
        Flash.gameObject.SetActive(true);
        ResultText.gameObject.SetActive(false);
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

        yield return Ease.Animate(8, (x) => {
            FlashBackground.color = new Color (
                1f - x * 2, 1f - x * 2, 1f - x * 2, 
                Ease.Get(Mathf.Clamp01(x * 4), EaseFunction.Circle, EaseMode.InOut) * .2f + .2f
            );

            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.y,
                Mathf.Pow(Ease.Get(Mathf.Clamp01(x * 8), EaseFunction.Circle, EaseMode.Out), 2) * 50 + 50
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
        yield return Ease.Animate(1, (x) => {
            float ease1 = Mathf.Pow(Ease.Get(x, EaseFunction.Circle, EaseMode.In), 2);
            float ease2 = Ease.Get(x, EaseFunction.Quadratic, EaseMode.In);
            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.y,
                ease1 * -10 + 100
            );
            ResultText.rectTransform.localScale = Vector3.one * (1 - ease2 * .1f);
            ResultText.rectTransform.anchoredPosition = Vector2.up * (ease1 * 40);
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
        string[] ranks = new [] {"1", "SSS+", "SSS", "SS+", "SS", "S+", "S", "AAA", "AA", "A", "B", "C", "D", "?"};
        int rankNum = System.Array.IndexOf(ranks, rank);

        yield return Ease.Animate(1.5f, (x) => {
            float ease1 = 1 - Mathf.Pow(1 - Ease.Get(Mathf.Clamp01(x * 1.5f), EaseFunction.Circle, EaseMode.Out), 2);
            float ease2 = Ease.Get(Mathf.Clamp01(x * 1.5f), EaseFunction.Quadratic, EaseMode.Out);
            ResultBackground.rectTransform.sizeDelta = new (
                ResultBackground.rectTransform.sizeDelta.y,
                ease1 * -10 + 90
            );
            ScoreHolder.localScale = Vector3.one * (1.1f - ease2 * .1f);
            ScoreHolder.anchoredPosition = Vector2.down * (1 - ease1) * 40;

            ScoreText.text = PadAlpha((score * x).ToString("######0"), '0', 7);
            RankText.text = ranks[Mathf.CeilToInt(Mathf.Lerp(ranks.Length - 2, rankNum, x))];
            ScoreRings[0].FillAmount = score * Ease.Get(x, EaseFunction.Exponential, EaseMode.Out) / 1e6f;
            ScoreRings[0].SetVerticesDirty();
            for (int a = 1; a < ScoreRings.Count; a++) 
            {
                ScoreRings[a].FillAmount = ScoreRings[a - 1].FillAmount * 10 - 9;
                ScoreRings[a].SetVerticesDirty();
            }
        });
        
        SongInfoHolder.alpha = DetailsHolder.alpha = 0;
        SongInfoHolder.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        DetailsHolder.gameObject.SetActive(true);

        yield return Ease.Animate(0.8f, (x) => {
            float ease1 = 1 - Mathf.Pow(1 - Ease.Get(x, EaseFunction.Circle, EaseMode.Out), 2);
            float ease2 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            ScoreHolder.anchorMin = ScoreHolder.anchorMax = ScoreHolder.pivot = new(.5f * (1 - ease1), .5f);
            ScoreHolder.anchoredPosition = Vector2.right * 50 * ease1;
            ScoreText.rectTransform.anchoredPosition = new (ScoreText.rectTransform.anchoredPosition.x, -20 * (1 - ease2));
            
            float ease3 = Ease.Get(Mathf.Clamp01(x * 2 - 1), EaseFunction.Cubic, EaseMode.Out);
            BestScoreHolder.alpha = SongInfoHolder.alpha = DetailsHolder.alpha = ease3;
            ProfileBar.main.SetVisibilty(ease3);
            BestScoreTransform.anchoredPosition = new (-1010 + 10 * ease3, BestScoreTransform.anchoredPosition.y);
            SongInfoTransform.anchoredPosition = new (SongInfoTransform.anchoredPosition.x, 25 - 10 * ease3);
            DetailsTransform.anchoredPosition = new (DetailsTransform.anchoredPosition.x, 10 * ease3 - 35);
        });
    }
    string PadAlpha(string source, char pad, int length)
    {
        if (source.Length >= length) return source;
        return "<alpha=#80>" + new string(pad, length - source.Length) + "<alpha=#ff>" + source;
    }
}
