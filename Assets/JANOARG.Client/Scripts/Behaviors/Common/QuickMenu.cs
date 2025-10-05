using System.Collections;
using JANOARG.Client.Behaviors.Panels;
using JANOARG.Client.Utils;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Common
{
    public class QuickMenu : MonoBehaviour
    {
        public static QuickMenu sMain;

        public CanvasGroup Background;
        public Graphic     MainBackground;

        [Space] public AudioMixer AudioMixer;

        [Space] public RectTransform LeftPanel;

        public CanvasGroup LeftPanelGroup;

        public bool IsAnimating;

        public void Awake()
        {
            sMain = this;
            gameObject.SetActive(false);
        }

        public void ShowLeft()
        {
            if (!IsAnimating)
            {
                gameObject.SetActive(true);
                StartCoroutine(ShowLeftAnim());
            }
        }

        public IEnumerator ShowLeftAnim()
        {
            IsAnimating = true;

            MainBackground.color = CommonSys.sMain.MainCamera.backgroundColor * new Color(1, 1, 1, 0) +
                                   new Color(0, 0, 0, 0.75f);

            AudioManager.sMain.SetSceneLayerLowPassCutoff(1000, 0.5f);

            yield return Ease.Animate(
                .2f, a =>
                {
                    ProfileBar.sMain.SetVisibility(
                        1 -
                        Ease.Get(
                            a, EaseFunction.Cubic,
                            EaseMode.Out));

                    Background.alpha = a;
                });

            LeftPanel.gameObject.SetActive(true);

            yield return Ease.Animate(
                .2f,
                a =>
                {
                    SetLeftPanelVisibility(Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
                });

            IsAnimating = false;
        }

        public void HideLeft()
        {
            if (!IsAnimating)
                StartCoroutine(HideLeftAnim());
        }

        public IEnumerator HideLeftAnim()
        {
            IsAnimating = true;

            AudioManager.sMain.SetSceneLayerLowPassCutoff(22050, 0.5f);

            yield return Ease.Animate(
                .2f,
                a =>
                {
                    SetLeftPanelVisibility(1 - Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
                });

            LeftPanel.gameObject.SetActive(false);

            yield return Ease.Animate(
                .2f, a =>
                {
                    ProfileBar.sMain.SetVisibility(
                        Ease.Get(
                            a, EaseFunction.Cubic,
                            EaseMode.Out));

                    Background.alpha = 1 - a;
                });

            IsAnimating = false;
            gameObject.SetActive(false);
        }

        public void HideFromPanel()
        {
            if (!IsAnimating) StartCoroutine(HideFromPanelAnim());
        }

        public IEnumerator HideFromPanelAnim()
        {
            IsAnimating = true;

            AudioManager.sMain.SetSceneLayerLowPassCutoff(22050, 0.5f);

            yield return Ease.Animate(
                .2f, a =>
                {
                    ProfileBar.sMain.SetVisibility(
                        Ease.Get(
                            a, EaseFunction.Cubic,
                            EaseMode.Out));

                    Background.alpha = 1 - a;
                });

            IsAnimating = false;
            gameObject.SetActive(false);
        }

        public void SetLeftPanelVisibility(float a)
        {
            LeftPanelGroup.alpha = a * a;
            LeftPanel.anchoredPosition *= new Vector2Frag(-10 * (1 - a));
        }

        public void ShowPanel(string sceneName)
        {
            if (!IsAnimating)
                StartCoroutine(ShowPanelAnim(sceneName));
        }

        public IEnumerator ShowPanelAnim(string sceneName)
        {
            IsAnimating = true;

            AudioManager.sMain.SetSceneLayerLowPassCutoff(9, 2);

            yield return Ease.Animate(
                .2f,
                a =>
                {
                    SetLeftPanelVisibility(1 - Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
                });

            LeftPanel.gameObject.SetActive(false);

            AsyncOperation sceneReq = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            float time = 0;

            while (time < .5f || (!sceneReq.isDone && time < 1))
            {
                yield return null;

                time += Time.deltaTime;
            }

            if (!sceneReq.isDone)
            {
                LoadingBar.sMain.Show();

                yield return new WaitWhile(() => LoadingBar.sMain.IsAnimating || !sceneReq.isDone);

                LoadingBar.sMain.Hide();

                yield return new WaitWhile(() => LoadingBar.sMain.IsAnimating);
            }

            if (Panel.sPanels.Count > 0)
            {
                Panel panel = Panel.sPanels[^1];
                panel.SceneName = sceneName;
                panel.Intro();
            }

            IsAnimating = false;
        }
    }
}
