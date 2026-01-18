using System.Collections;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.SongSelect;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Player
{
    public class PlayerScreenPause : MonoBehaviour
    {
        public static PlayerScreenPause sMain;

        public GameObject UIHolder;

        [Space] public Image Background;

        public RectTransform OptionHolder;
        public CanvasGroup   OptionGroup;

        [Space] public Image RetryBackground;

        public Image RetryFlash;

        public float PauseTime = -10;

        [Space] public Coroutine CurrentAnimation;

        private void Awake()
        {
            sMain = this;
        }

        private void Start()
        {
            Background.gameObject.SetActive(false);
            UIHolder.SetActive(false);
        }

        public void OnPauseButtonClick()
        {
            if (PlayerScreen.sMain.CurrentTime - PauseTime < 2)
            {
                PlayerScreen.sMain.IsPlaying = false;
                PlayerScreen.sMain.Music.Pause();

                // PlayerInputManager.main.Fingers.Clear();
                PlayerInputManager.sInstance.TouchClasses.Clear();
                Show();
            }
            else
            {
                PauseTime = PlayerScreen.sMain.CurrentTime;
            }
        }

        public void Show()
        {
            if (CurrentAnimation != null) StopCoroutine(CurrentAnimation);
            CurrentAnimation = StartCoroutine(ShowAnim());
        }

        private IEnumerator ShowAnim()
        {
            Background.gameObject.SetActive(true);
            UIHolder.SetActive(true);

            yield return Ease.Animate(
                0.2f, x =>
                {
                    float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);

                    Background.color = CommonSys.sMain.MainCamera.backgroundColor *
                                       new Color(1, 1, 1, ease * 0.8f);

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

        private IEnumerator ContinueAnim()
        {
            StartCoroutine(
                Ease.Animate(
                    0.2f, x =>
                    {
                        float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                        OptionHolder.anchoredPosition = ease * 20 * Vector2.left;
                        OptionGroup.alpha = 1 - ease;
                    }));

            PlayerScreen.sMain.CurrentTime -= 1.5f;
            PlayerScreen.sMain.Resync();
            PlayerScreen.sMain.IsPlaying = true;

            float targetVolume = PlayerScreen.sMain.Music.volume;

            yield return Ease.Animate(
                1.5f, a =>
                {
                    float ease = Ease.Get(a, EaseFunction.Cubic, EaseMode.InOut);

                    Background.color = CommonSys.sMain.MainCamera.backgroundColor *
                                       new Color(1, 1, 1, (1 - ease) * 0.8f);

                    PlayerScreen.sMain.Music.volume = a * targetVolume;
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

        private IEnumerator RetryAnim()
        {
            StartCoroutine(
                Ease.Animate(
                    0.2f, x =>
                    {
                        float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                        OptionHolder.anchoredPosition = ease * 20 * Vector2.left;
                        OptionGroup.alpha = 1 - ease;
                    }));

            RetryBackground.gameObject.SetActive(true);
            RetryBackground.color = PlayerScreen.sTargetSong.BackgroundColor;

            yield return Ease.Animate(
                1, a =>
                {
                    float lerp2 = Mathf.Pow(
                        Ease.Get(a, EaseFunction.Circle, EaseMode.In),
                        2);

                    RetryBackground.rectTransform.anchorMin =
                        new Vector2(0, .5f * (1 - lerp2));

                    RetryBackground.rectTransform.anchorMax =
                        new Vector2(1, 1 - .5f * (1 - lerp2));

                    RetryBackground.rectTransform.sizeDelta =
                        new Vector2(0, 100 * (1 - lerp2));

                    float lerp3 = Mathf.Pow(
                        Ease.Get(
                            a, EaseFunction.Exponential,
                            EaseMode.Out), 0.5f);

                    RetryFlash.color = new Color(1, 1, 1, 1 - lerp3);
                });

            yield return new WaitForSeconds(1);

            RetryBackground.gameObject.SetActive(false);
            CommonSys.sMain.MainCamera.backgroundColor = PlayerScreen.sTargetSong.BackgroundColor;

            Background.color = PlayerScreen.sTargetSong.BackgroundColor;

            yield return PlayerScreen.sMain.InitChart();

            Background.gameObject.SetActive(false);
            UIHolder.SetActive(false);
            PlayerScreen.sMain.BeginReadyAnim();

            CurrentAnimation = null;
        }

        public void Quit()
        {
            if (CurrentAnimation != null) return;

            CurrentAnimation = StartCoroutine(QuitAnim());
        }

        private IEnumerator QuitAnim()
        {
            yield return Ease.Animate(
                0.2f, x =>
                {
                    float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);

                    Background.color = CommonSys.sMain.MainCamera.backgroundColor *
                                       new Color(1, 1, 1, ease * 0.2f + 0.8f);

                    OptionHolder.anchoredPosition = ease * 20 * Vector2.left;
                    OptionGroup.alpha = 1 - ease;
                });

            yield return new WaitForSeconds(1);

            LoadingBar.sMain.Show();

            CommonSys.Load(
                "Song Select", () => !LoadingBar.sMain.IsAnimating && SongSelectScreen.sMain?.IsInit == true,
                () =>
                {
                    LoadingBar.sMain.Hide();
                    SongSelectScreen.sMain.Intro();
                }, false);

            SceneManager.UnloadSceneAsync("Player");
            Resources.UnloadUnusedAssets();
        }
    }
}
