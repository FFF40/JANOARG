using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class IntroScreen : MonoBehaviour
{
    [Header("Disclaimer")]
    public TMP_Text DisclaimerTitle;
    public TMP_Text[] DisclaimerParagraphs;
    public TMP_Text DisclaimerActionLabel;

    [Header("Cover")]
    public RectTransform CoverTop;
    public RectTransform CoverBottom;
    public RectTransform CoverMiddle;
    public RectTransform CoverMiddleInfo;
    public Graphic CoverMiddleImage;

    [Header("Title Screen")]
    public Image TitleLogo;
    public TMP_Text TitleActionLabel;
    public TMP_Text TitleFooter;
    public TMP_Text PostTitleLabel;
    public RectTransform PostTitleCover1;
    public RectTransform PostTitleCover2;

    [Header("Data")]
    public bool isFirstStart = true;

    public void Start() 
    {
        DisclaimerTitle.alpha = DisclaimerActionLabel.alpha = 0;
        foreach (TMP_Text p in DisclaimerParagraphs) p.alpha = 0;
        CoverMiddle.anchorMax = new(0, CoverMiddle.anchorMax.y);
        PostTitleCover1.anchorMin = PostTitleCover2.anchorMin = new(0, 0);
        PostTitleCover1.anchorMax = PostTitleCover2.anchorMax = new(1, 0);
        CoverMiddleImage.color = CoverMiddleImage.color * new Color(1, 1, 1, 0) + new Color(0, 0, 0, 1);
        StartCoroutine(IntroRoutine());
    }

    public IEnumerator IntroRoutine()
    {
        yield return DisclaimerRoutine();
        yield return IntroEnterRoutine();
        yield return IntroExitRoutine();
    }

    bool ScreenTouchedThisFrame() 
    {
        return Touchscreen.current?.primaryTouch?.phase.value == UnityEngine.InputSystem.TouchPhase.Began;
    }
    
    public IEnumerator DisclaimerRoutine()
    {
        // ------------------ Photoepilepsy disclaimer sequence
        bool isTouched = false;

        if (isFirstStart)
        {
            for (float a = 0; a < 1; a += Time.deltaTime * 5)
            {
                if (ScreenTouchedThisFrame()) isTouched = true;
                if (isTouched && !isFirstStart) break;
                DisclaimerTitle.alpha = Mathf.Min(a, 1);
                yield return null;
            }
            foreach (TMP_Text p in DisclaimerParagraphs)
            {
                for (float a = 0; a < 1; a += Time.deltaTime * 5)
                {
                    if (ScreenTouchedThisFrame()) isTouched = true;
                    if (isTouched && !isFirstStart) break;
                    p.alpha = Mathf.Min(a, 1);
                    yield return null;
                }
            }

            isTouched = false;
            for (float a = 0; a < 1; a += Time.deltaTime * 5)
            {
                if (ScreenTouchedThisFrame()) isTouched = true;
                if (isTouched) break;
                DisclaimerActionLabel.alpha = Mathf.Min(a, 1);
                yield return null;
            }

            while (!isTouched) 
            {
                if (ScreenTouchedThisFrame()) isTouched = true;
                yield return null;
            }
        }
        else
        {
            for (float a = 0; a < 1; a += Time.deltaTime * 2)
            {
                if (ScreenTouchedThisFrame()) isTouched = true;
                if (isTouched && !isFirstStart) break;
                DisclaimerTitle.alpha = Mathf.Min(a, 1);
                foreach (TMP_Text p in DisclaimerParagraphs) p.alpha = Mathf.Min(a, 1);
                yield return null;
            }

            for (float a = 0; a < 1; a += Time.deltaTime / 2)
            {
                if (ScreenTouchedThisFrame()) isTouched = true;
                if (isTouched) break;
                yield return null;
            }
        }
        
        for (float speed = 1;; speed = isFirstStart ? 3 : isTouched ? 5 : 2)
        {
            if (ScreenTouchedThisFrame()) isTouched = true;
            float subSpeed = Time.deltaTime * speed / 3;
            float max = DisclaimerTitle.alpha -= subSpeed;
            foreach (TMP_Text p in DisclaimerParagraphs) max = Mathf.Max(max, p.alpha -= subSpeed);
            max = Mathf.Max(max, DisclaimerActionLabel.alpha -= subSpeed * 8);
            if (max <= 0) break;
            yield return null;
        }

        yield return new WaitForSeconds(.3f);
    }

    public IEnumerator IntroEnterRoutine()
    {
        CoverMiddleInfo.anchoredPosition += Vector2.left * 20;
        while (CoverMiddle.anchorMax.x <= 1)
        {
            float goal = 1.005f;
            CoverMiddle.anchorMax = new(
                Mathf.Lerp(CoverMiddle.anchorMax.x, goal, 1 - Mathf.Pow(0.05f, Time.deltaTime)), 
                CoverMiddle.anchorMax.y
            );
            CoverMiddleInfo.anchoredPosition = new(
                CoverMiddleInfo.anchoredPosition.x * Mathf.Pow(0.02f, Time.deltaTime),
                CoverMiddleInfo.anchoredPosition.y
            );
            yield return null;
        }
        CoverMiddle.anchorMax = new(1, CoverMiddle.anchorMax.y);

        Color CenterColor = CoverMiddleImage.color;
        float CenterSize = CoverMiddle.sizeDelta.y;
        yield return Ease.Animate(1, (a) => {
            float lerp = Mathf.Pow(Ease.Get(a, EaseFunction.Circle, EaseMode.In), 2);
            CoverMiddle.sizeDelta = new(CoverMiddle.sizeDelta.x, CenterSize * (1 - lerp));
            CoverMiddle.anchorMax = (CoverTop.anchorMin = new(0, .5f + .5f * lerp)) + Vector2.right;
            CoverMiddle.anchorMin = (CoverBottom.anchorMax = new(1, .5f - .5f * lerp)) + Vector2.left;
            CoverTop.sizeDelta = CoverBottom.sizeDelta = new(0, -.5f * CenterSize * (1 - lerp));

            CoverMiddleInfo.anchorMin = CoverMiddle.anchorMin = new(
                Mathf.Pow(Ease.Get(a * 1.05f, EaseFunction.Exponential, EaseMode.Out), 2), 
                CoverMiddle.anchorMin.y
            );

            TitleLogo.rectTransform.anchoredPosition = new (TitleLogo.rectTransform.anchoredPosition.x, lerp * 10);
            TitleActionLabel.alpha = TitleFooter.alpha = 0;
        });

        bool isTouched = false;
        void Animate(float a)
        {
            float lerp = Mathf.Pow(Ease.Get(a * 2, EaseFunction.Exponential, EaseMode.Out), 2);
            TitleLogo.rectTransform.anchoredPosition = new (TitleLogo.rectTransform.anchoredPosition.x, 10 + lerp * 10);
            float lerp2 = Ease.Get(a * 1.5f, EaseFunction.Quintic, EaseMode.Out);
            TitleActionLabel.alpha = 1 - Mathf.Pow(1 - Mathf.Clamp01(a * 1.5f), 2);
            TitleActionLabel.rectTransform.anchoredPosition = new (TitleActionLabel.rectTransform.anchoredPosition.x, lerp2 * -36);
            float lerp3 = Ease.Get(a * 1.2f - .2f, EaseFunction.Quintic, EaseMode.Out);
            TitleFooter.alpha = (1 - Mathf.Pow(1 - Mathf.Clamp01(a * 1.2f - .2f), 2)) * .5f;
            TitleFooter.rectTransform.anchoredPosition = new (TitleFooter.rectTransform.anchoredPosition.x, 80 - lerp3 * 30);
        }
        for (float a = 0; a < 1; a += Time.deltaTime / 2)
        {
            if (ScreenTouchedThisFrame()) isTouched = true;
            if (isTouched) break;
            Animate(a);
            yield return null;
        }
        if (!isTouched) Animate(1);
        float waitTime = 0;
        while (!isTouched)
        {
            if (ScreenTouchedThisFrame()) isTouched = true;
            waitTime += Time.deltaTime;
            TitleActionLabel.rectTransform.anchoredPosition = new (TitleActionLabel.rectTransform.anchoredPosition.x, -33 - 3 * Mathf.Cos(waitTime));
            yield return null;
        }
    }

    public IEnumerator IntroExitRoutine()
    {
        float yPos = TitleActionLabel.rectTransform.anchoredPosition.y / ((RectTransform)TitleActionLabel.rectTransform.parent).rect.height + 0.5f;
        PostTitleLabel.rectTransform.anchorMin = new (PostTitleLabel.rectTransform.anchorMin.x, yPos);
        PostTitleLabel.rectTransform.anchorMax = new (PostTitleLabel.rectTransform.anchorMax.x, yPos);
        Debug.Log(TitleActionLabel.rectTransform.rect);
        yield return Ease.Animate(1f, (a) => {
            float lerp = Mathf.Pow(Ease.Get(a * 1.5f, EaseFunction.Circle, EaseMode.Out), 2);
            PostTitleCover1.anchorMin = new (PostTitleCover1.anchorMin.x, Mathf.Lerp(yPos, 0, lerp));
            PostTitleCover1.anchorMax = new (PostTitleCover1.anchorMax.x, Mathf.Lerp(yPos, 1, lerp));
            float lerp2 = Mathf.Pow(Ease.Get(a * 1.5f - 0.5f, EaseFunction.Circle, EaseMode.In), 2);
            PostTitleCover2.anchorMin = new (PostTitleCover2.anchorMin.x, Mathf.Lerp(yPos, 0, lerp2));
            PostTitleCover2.anchorMax = new (PostTitleCover2.anchorMax.x, Mathf.Lerp(yPos, 1, lerp2));
        });
        yield return new WaitForSecondsRealtime(1);

        Common.main.MainCamera.backgroundColor = Color.black;

        LoadingBar.main.Show();
        Common.Load("Song Select", () => !LoadingBar.main.IsAnimating && (SongSelectScreen.main?.IsInit == true), () => {
            LoadingBar.main.Hide();
            SongSelectScreen.main.Intro();
        }, false);
        SceneManager.UnloadSceneAsync("Intro");
        Resources.UnloadUnusedAssets();
    }
}