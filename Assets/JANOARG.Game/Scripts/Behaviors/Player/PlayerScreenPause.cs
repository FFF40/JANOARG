using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.IO;

public class PlayerScreenPause : MonoBehaviour
{
    public static PlayerScreenPause main;

    public GameObject UIHolder;
    [Space]
    public Image Background;
    public RectTransform OptionHolder;
    public CanvasGroup OptionGroup;
    [Space]
    public Image RetryBackground;
    public Image RetryFlash;
    [Space]
    public Coroutine CurrentAnimation;

    public float PauseTime = -10;

    void Awake() 
    {
        main = this;
    }

    void Start() 
    {
        Background.gameObject.SetActive(false);
        UIHolder.SetActive(false);
    }

    public void OnPauseButtonClick() 
    {
        if (PlayerScreen.main.CurrentTime - PauseTime < 2)
        {
            PlayerScreen.main.IsPlaying = false;
            PlayerScreen.main.Music.Pause();
            PlayerInputManager.main.Fingers.Clear();
            Show();
        }
        else 
        {
            PauseTime = PlayerScreen.main.CurrentTime; 
        }
    }

    public void Show() 
    {
        if (CurrentAnimation != null) StopCoroutine(CurrentAnimation);
        CurrentAnimation = StartCoroutine(ShowAnim());
    }

    IEnumerator ShowAnim() 
    {
        Background.gameObject.SetActive(true);
        UIHolder.SetActive(true);

        yield return Ease.Animate(0.2f, (x) => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            Background.color = Common.main.MainCamera.backgroundColor * new Color(1, 1, 1, ease * 0.8f);
            OptionHolder.anchoredPosition = (1 - ease) * 20 * Vector2.left;
            OptionGroup.alpha = ease;
        });

        CurrentAnimation = null;
    }

    public void Continue() 
    {
        if (CurrentAnimation != null) return;
        CurrentAnimation = StartCoroutine(ContinueAnim());
    }

    IEnumerator ContinueAnim() 
    {
        StartCoroutine(Ease.Animate(0.2f, (x) => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            OptionHolder.anchoredPosition = ease * 20 * Vector2.left;
            OptionGroup.alpha = 1 - ease;
        }));

        PlayerScreen.main.CurrentTime -= 1.5f;
        PlayerScreen.main.Resync();
        PlayerScreen.main.IsPlaying = true;

        float targetVolume = PlayerScreen.main.Music.volume;

        yield return Ease.Animate(1.5f, a => {
            float ease = Ease.Get(a, EaseFunction.Cubic, EaseMode.InOut);
            Background.color = Common.main.MainCamera.backgroundColor * new Color(1, 1, 1, (1 - ease) * 0.8f);
            PlayerScreen.main.Music.volume = a * targetVolume;
        });

        Background.gameObject.SetActive(false);
        UIHolder.SetActive(false);

        CurrentAnimation = null;
    }

    public void Retry() 
    {
        if (CurrentAnimation != null) return;
        CurrentAnimation = StartCoroutine(RetryAnim());
    }

    IEnumerator RetryAnim() 
    {
        StartCoroutine(Ease.Animate(0.2f, (x) => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            OptionHolder.anchoredPosition = ease * 20 * Vector2.left;
            OptionGroup.alpha = 1 - ease;
        }));

        RetryBackground.gameObject.SetActive(true);
        RetryBackground.color = PlayerScreen.TargetSong.BackgroundColor;

        yield return Ease.Animate(1, a => {
            float lerp2 = Mathf.Pow(Ease.Get(a, EaseFunction.Circle, EaseMode.In), 2);
            RetryBackground.rectTransform.anchorMin = new(0, .5f * (1 - lerp2));
            RetryBackground.rectTransform.anchorMax = new(1, 1 - .5f * (1 - lerp2));
            RetryBackground.rectTransform.sizeDelta = new(0, 100 * (1 - lerp2));
            
            float lerp3 = Mathf.Pow(Ease.Get(a, EaseFunction.Exponential, EaseMode.Out), 0.5f);
            RetryFlash.color = new (1, 1, 1, 1 - lerp3);
        });

        yield return new WaitForSeconds(1);

        RetryBackground.gameObject.SetActive(false);
        Common.main.MainCamera.backgroundColor = PlayerScreen.TargetSong.BackgroundColor;

        Background.color = PlayerScreen.TargetSong.BackgroundColor;
        yield return PlayerScreen.main.InitChart();
        Background.gameObject.SetActive(false);
        UIHolder.SetActive(false);
        PlayerScreen.main.BeginReadyAnim();

        CurrentAnimation = null;
    }

    public void Quit() 
    {
        if (CurrentAnimation != null) return;
        CurrentAnimation = StartCoroutine(QuitAnim());
    }

    IEnumerator QuitAnim() 
    {
        yield return Ease.Animate(0.2f, (x) => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            Background.color = Common.main.MainCamera.backgroundColor * new Color(1, 1, 1, ease * 0.2f + 0.8f);
            OptionHolder.anchoredPosition = ease * 20 * Vector2.left;
            OptionGroup.alpha = 1 - ease;
        });

        yield return new WaitForSeconds(1);

        LoadingBar.main.Show();
        Common.Load("Song Select", () => !LoadingBar.main.IsAnimating && (SongSelectScreen.main?.IsInit == true), () => {
            LoadingBar.main.Hide();
            SongSelectScreen.main.Intro();
        }, false);
        SceneManager.UnloadSceneAsync("Player");
        Resources.UnloadUnusedAssets();
    }
}
