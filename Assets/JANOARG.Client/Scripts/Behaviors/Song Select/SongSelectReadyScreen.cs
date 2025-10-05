using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class SongSelectReadyScreen : MonoBehaviour
{
    public static SongSelectReadyScreen main;

    public bool IsAnimating;
    [Space]
    public RectTransform DifficultyInfoHolder;
    public TMP_Text DifficultyLevelText;
    public TMP_Text DifficultyNameText;
    [Space]
    public RectTransform SongInfoHolder;
    public TMP_Text SongNameText;
    public TMP_Text SongArtistText;
    [Space]
    public RectTransform ExtraInfoBackground;
    public RectTransform ExtraInfoBackground2;
    public RectTransform ExtraInfoBackground3;
    public TMP_Text CoverArtistNameLabel;
    public TMP_Text CoverArtistNameText;
    public TMP_Text CharterNameLabel;
    public TMP_Text CharterNameText;

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
        SongInfoHolder.anchoredPosition += new Vector2(1000, 0);
        SongArtistText.text = PlayerScreen.TargetSong.SongArtist + " <alpha=#77>»";
        SongNameText.text = PlayerScreen.TargetSong.SongName;
        CharterNameText.text = PlayerScreen.TargetChartMeta.CharterName;
        CoverArtistNameText.text = PlayerScreen.TargetSong.Cover.ArtistName;

        // Song info
        StartCoroutine(Ease.AnimateText(SongArtistText, 2, 1 / 500f, (info, x) => {
            var meshInfo = SongArtistText.textInfo.meshInfo[info.materialReferenceIndex];
            int index = info.vertexIndex;
            float ease = Ease.Get(x * 2, EaseFunction.Exponential, EaseMode.Out) + .05f - .05f * Ease.Get(x * 1.4f - 0.4f, EaseFunction.Bounce, EaseMode.Out);
            Vector3 offset = 400 * (1 - ease) * new Vector3(-.26795f, -1);
            meshInfo.vertices[index] += offset;
            meshInfo.vertices[index + 1] += offset;
            meshInfo.vertices[index + 2] += offset;
            meshInfo.vertices[index + 3] += offset;
        }));
        StartCoroutine(Ease.AnimateText(SongNameText, 1.5f, 1 / 400f, (info2, x) => {
            var meshInfo = SongNameText.textInfo.meshInfo[info2.materialReferenceIndex];
            int index = info2.vertexIndex;
            float ease = 1 - Mathf.Pow(Ease.Get(1 - x, EaseFunction.Circle, EaseMode.In), 3.5f);
            Vector3 offset = new (1000 * (1 - ease), 0);
            meshInfo.vertices[index] += offset;
            meshInfo.vertices[index + 1] += offset;
            meshInfo.vertices[index + 2] += offset;
            meshInfo.vertices[index + 3] += offset;
        }));
        
        // Difficulty info
        StartCoroutine(Ease.Animate(1, x => {
            DifficultyInfoHolder.anchoredPosition = new (
                -912 + 1000 * Ease.Get(x, EaseFunction.Exponential, EaseMode.Out),
                DifficultyInfoHolder.anchoredPosition.y);
        }));
        
        // Extra info
        StartCoroutine(Ease.Animate(1.3f, x => {
            ExtraInfoBackground.sizeDelta = new (ExtraInfoBackground.sizeDelta.x, 
                100 * Ease.Get(x * 1.3f, EaseFunction.Exponential, EaseMode.Out));
            ExtraInfoBackground2.sizeDelta = new (ExtraInfoBackground2.sizeDelta.x, 
                100 * Ease.Get(x * 1.3f - .2f, EaseFunction.Exponential, EaseMode.Out));
            ExtraInfoBackground3.sizeDelta = new (ExtraInfoBackground3.sizeDelta.x, 
                96 * Ease.Get(x * 1.3f - .3f, EaseFunction.Exponential, EaseMode.Out));

            CoverArtistNameLabel.rectTransform.anchoredPosition = new(CoverArtistNameLabel.rectTransform.anchoredPosition.x, 
                -200 + 200 * Ease.Get(x * 1.4f, EaseFunction.Exponential, EaseMode.Out));
            CoverArtistNameText.rectTransform.anchoredPosition = new(CoverArtistNameText.rectTransform.anchoredPosition.x, 
                -210 + 200 * Ease.Get(x * 1.3f, EaseFunction.Exponential, EaseMode.Out));
            CharterNameLabel.rectTransform.anchoredPosition = new(CharterNameLabel.rectTransform.anchoredPosition.x, 
                -200 + 200 * Ease.Get(x * 1.2f, EaseFunction.Exponential, EaseMode.Out));
            CharterNameText.rectTransform.anchoredPosition = new(CharterNameText.rectTransform.anchoredPosition.x, 
                -210 + 200 * Ease.Get(x * 1.1f, EaseFunction.Exponential, EaseMode.Out));
        }));
        
        yield return null;
        SongInfoHolder.anchoredPosition -= new Vector2(1000, 0);
        yield return new WaitForSeconds(4);

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

        // Song info
        StartCoroutine(Ease.AnimateText(SongArtistText, 0.75f, 1 / 800f, (info, x) => {
            var meshInfo = SongArtistText.textInfo.meshInfo[info.materialReferenceIndex];
            int index = info.vertexIndex;
            float ease = Ease.Get(x, EaseFunction.Exponential, EaseMode.In);
            Vector3 offset = -400 * ease * new Vector3(-.26795f, -1);
            meshInfo.vertices[index] += offset;
            meshInfo.vertices[index + 1] += offset;
            meshInfo.vertices[index + 2] += offset;
            meshInfo.vertices[index + 3] += offset;
        }));
        StartCoroutine(Ease.AnimateText(SongNameText, 0.5f, 1 / 500f, (info, x) => {
            var meshInfo = SongNameText.textInfo.meshInfo[info.materialReferenceIndex];
            int index = info.vertexIndex;
            float ease = Ease.Get(x, EaseFunction.Exponential, EaseMode.In);
            Vector3 offset = -1000 * ease * Vector2.right;
            meshInfo.vertices[index] += offset;
            meshInfo.vertices[index + 1] += offset;
            meshInfo.vertices[index + 2] += offset;
            meshInfo.vertices[index + 3] += offset;
        }));
        
        // Difficulty info
        StartCoroutine(Ease.Animate(1, x => {
            DifficultyInfoHolder.anchoredPosition = new ( 
                88 + 1000 * Ease.Get(x, EaseFunction.Exponential, EaseMode.In),
                DifficultyInfoHolder.anchoredPosition.y);
        }));

        // Extra info
        StartCoroutine(Ease.Animate(1f, x => {
            ExtraInfoBackground.sizeDelta = new (ExtraInfoBackground.sizeDelta.x, 
                100 * (1 - Ease.Get(x * 1.3f - .3f, EaseFunction.Exponential, EaseMode.In)));
            ExtraInfoBackground2.sizeDelta = new (ExtraInfoBackground2.sizeDelta.x, 
                100 * (1 - Ease.Get(x * 1.3f - .1f, EaseFunction.Exponential, EaseMode.In)));
            ExtraInfoBackground3.sizeDelta = new (ExtraInfoBackground3.sizeDelta.x, 
                96 * (1 - Ease.Get(x * 1.3f, EaseFunction.Exponential, EaseMode.In)));

            CoverArtistNameLabel.rectTransform.anchoredPosition = new(CoverArtistNameLabel.rectTransform.anchoredPosition.x, 
                -0 + 200 * Ease.Get(x * 1.1f - .1f, EaseFunction.Exponential, EaseMode.In));
            CoverArtistNameText.rectTransform.anchoredPosition = new(CoverArtistNameText.rectTransform.anchoredPosition.x, 
                -10 + 200 * Ease.Get(x * 1.2f - .2f, EaseFunction.Exponential, EaseMode.In));
            CharterNameLabel.rectTransform.anchoredPosition = new(CharterNameLabel.rectTransform.anchoredPosition.x, 
                -0 + 200 * Ease.Get(x * 1.3f - .3f, EaseFunction.Exponential, EaseMode.In));
            CharterNameText.rectTransform.anchoredPosition = new(CharterNameText.rectTransform.anchoredPosition.x, 
                -10 + 200 * Ease.Get(x * 1.4f - .4f, EaseFunction.Exponential, EaseMode.In));
        }));
        
        yield return new WaitForSeconds(2);
        
        PlayerScreen.main.BeginReadyAnim();
        Destroy(gameObject);

        IsAnimating = false;
    }
}