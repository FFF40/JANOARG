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

            yield return Ease.Animate(.4f, EaseFunction.Quartic, EaseMode.Out, ease =>
            {
                rt.anchoredPosition = Vector3.left * (2000 + (Width - SafeArea.sizeDelta.y / 2) * (1 - ease));
            });

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

            yield return Ease.Animate(.3f, EaseFunction.Quartic, EaseMode.In, ease =>
            {
                rt.anchoredPosition = Vector3.left * (2000 + (Width - SafeArea.sizeDelta.y / 2) * ease);
            });

            if (SetActiveOnHide) gameObject.SetActive(false);

            isAnimating = false;
        }
    }
}