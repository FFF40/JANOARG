using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SongSelectReadyScreen : MonoBehaviour
{
    public static SongSelectReadyScreen main;

    public bool IsAnimating;
    [Space]
    public CanvasGroup DifficultyGroup;
    public RectTransform DifficultyLayout;
    public TMP_Text DifficultyLevelText;
    public TMP_Text DifficultyNameText;
    [Space]
    public CanvasGroup SongGroup;
    public RectTransform SongLayout;
    public TMP_Text SongNameText;
    public TMP_Text SongArtistText;
    [Space]
    public CanvasGroup InfoGroup;
    public RectTransform InfoLayout;
    public TMP_Text CharterNameText;
    public TMP_Text CoverArtistNameText;

    public void Awake()
    {
        main = this;
    }

    public void BeginLaunch() 
    {
        gameObject.SetActive(true);

        RectTransform rt = (RectTransform)transform;
        rt.SetParent(Common.main.CommonCanvas);

        StartCoroutine(BeginLaunchAnim());
    }

    public IEnumerator BeginLaunchAnim() 
    {
        IsAnimating = true;

        DifficultyLevelText.text = Helper.FormatDifficulty(PlayerScreen.TargetChartMeta.DifficultyLevel);
        DifficultyNameText.text = PlayerScreen.TargetChartMeta.DifficultyName.ToUpper();
        SongArtistText.text = PlayerScreen.TargetSong.SongArtist;
        SongNameText.text = PlayerScreen.TargetSong.SongName;
        CharterNameText.text = PlayerScreen.TargetChartMeta.CharterName;

        Vector2 diffStart = DifficultyLayout.anchoredPosition;
        Vector2 songStart = SongLayout.anchoredPosition;
        Vector2 infoStart = InfoLayout.anchoredPosition;

        static float lerp(float x) => Random.Range(Ease.Get(x, EaseFunction.Exponential, EaseMode.Out), Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));
        
        yield return Ease.Animate(1.5f, x => {
            DifficultyLayout.anchoredPosition = diffStart + Vector2.right * (50 * (1 - Ease.Get(x * 1.2f,        EaseFunction.Exponential, EaseMode.Out)));
            SongLayout.anchoredPosition =       songStart + Vector2.right * (50 * (1 - Ease.Get(x * 1.2f - .05f, EaseFunction.Exponential, EaseMode.Out)));
            InfoLayout.anchoredPosition =       infoStart + Vector2.right * (50 * (1 - Ease.Get(x * 1.2f - .1f,  EaseFunction.Exponential, EaseMode.Out)));

            DifficultyGroup.alpha = lerp(x * 2.4f);
            SongGroup.alpha =       lerp(x * 2.4f - .1f);
            InfoGroup.alpha =       lerp(x * 2.4f - .2f);
        });
        
        yield return new WaitForSeconds(2);

        IsAnimating = false;
    }

    public void EndLaunch() 
    {
        StartCoroutine(EndLaunchAnim());
    }
    public IEnumerator EndLaunchAnim() 
    {
        yield return new WaitUntil(() => !IsAnimating);

        IsAnimating = true;

        Vector2 songStart = SongLayout.anchoredPosition;
        Vector2 infoStart = InfoLayout.anchoredPosition;

        static float lerp(float x) => Random.Range(Ease.Get(x, EaseFunction.Exponential, EaseMode.Out), Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));

        yield return Ease.Animate(.5f, x => {
            SongLayout.anchoredPosition = songStart + Vector2.left * (50 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));
            InfoLayout.anchoredPosition = infoStart + Vector2.left * (50 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));

            SongGroup.alpha = 1 - lerp(x);
            InfoGroup.alpha = 1 - lerp(x);
            
        });
        
        PlayerScreen.main.BeginReadyAnim();
        Destroy(gameObject);

        IsAnimating = false;
    }
}