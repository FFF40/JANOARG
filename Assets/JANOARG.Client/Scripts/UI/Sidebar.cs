using System.Collections;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Client.UI
{
    public class Sidebar : MonoBehaviour
    {
        public RectTransform SafeArea;
        public float         Width           = 750;
        public bool          SetActiveOnHide = false;

        public bool isAnimating { get; protected set; }

        public void Show()
        {
            gameObject.SetActive(true);
            StartCoroutine(ShowAnimation());
        }

        public IEnumerator ShowAnimation()
        {
            isAnimating = true;
            var rt = GetComponent<RectTransform>();

            void f_lerpContent(float value)
            {
                float ease = Ease.Get(value, EaseFunction.Quartic, EaseMode.Out);

                rt.anchoredPosition = Vector3.left * (2000 + (Width - SafeArea.sizeDelta.y / 2) * (1 - ease));
            }

            for (float a = 0; a < 1; a += Time.deltaTime / .4f)
            {
                f_lerpContent(a);

                yield return null;
            }

            f_lerpContent(1);

            isAnimating = false;
        }

        public void Hide()
        {
            StartCoroutine(HideAnimation());
        }

        public IEnumerator HideAnimation()
        {
            isAnimating = true;
            var rt = GetComponent<RectTransform>();

            void f_lerpContent(float value)
            {
                float ease = Ease.Get(value, EaseFunction.Quartic, EaseMode.In);

                rt.anchoredPosition = Vector3.left * (2000 + (Width - SafeArea.sizeDelta.y / 2) * ease);
            }

            for (float a = 0; a < 1; a += Time.deltaTime / .3f)
            {
                f_lerpContent(a);

                yield return null;
            }

            f_lerpContent(1);

            if (SetActiveOnHide) gameObject.SetActive(false);

            isAnimating = false;
        }
    }
}