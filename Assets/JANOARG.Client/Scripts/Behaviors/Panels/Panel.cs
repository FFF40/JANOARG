using System.Collections;
using System.Collections.Generic;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JANOARG.Client.Behaviors.Panels
{
    public class Panel : MonoBehaviour
    {
        public static List<Panel> sPanels = new();

        public RectTransform Holder;
        public CanvasGroup   HolderGroup;

        [Space] public string SceneName;

        [Space] public bool IsAnimating;

        public void OnEnable()
        {
            sPanels.Add(this);
            HolderGroup.alpha = 0;
            HolderGroup.blocksRaycasts = false;
        }

        public void OnDisable()
        {
            sPanels.Remove(this);
        }

        public void Intro()
        {
            if (!IsAnimating) StartCoroutine(IntroAnim());
        }

        public IEnumerator IntroAnim()
        {
            IsAnimating = true;

            HolderGroup.blocksRaycasts = true;

            yield return Ease.Animate(
                .2f,
                a => { SetPanelVisibility(Ease.Get(a, EaseFunction.Cubic, EaseMode.Out)); });

            IsAnimating = false;
        }

        public void Close()
        {
            if (!IsAnimating) StartCoroutine(CloseAnim());
        }

        public IEnumerator CloseAnim()
        {
            IsAnimating = true;

            HolderGroup.blocksRaycasts = false;

            if (sPanels.Count <= 1) AudioManager.sMain.SetSceneLayerLowPassCutoff(5000, 1f);

            yield return Ease.Animate(
                .2f,
                a =>
                {
                    SetPanelVisibility(1 - Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
                });

            CommonSys.sMain.StartCoroutine(UnloadAnim());

            IsAnimating = false;
        }

        public IEnumerator UnloadAnim()
        {
            yield return SceneManager.UnloadSceneAsync(SceneName);

            if (sPanels.Count <= 1) QuickMenu.sMain.HideFromPanel();
            else
                sPanels[^2]
                    .Intro();
        }

        public void SetPanelVisibility(float a)
        {
            HolderGroup.alpha = a * a;
            Holder.anchoredPosition = new Vector2(-10 * (1 - a), Holder.anchoredPosition.y);
        }
    }
}
