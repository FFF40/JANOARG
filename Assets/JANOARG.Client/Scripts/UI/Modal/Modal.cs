using System;
using System.Collections;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.UI.Modal
{
    public class Modal : MonoBehaviour
    {
        public RectTransform TitleBarHolder;
        public CanvasGroup TitleBarGroup;
        public TMP_Text TitleLabel;
        public Graphic ColorBackground;
        public Graphic DarkBackground;
        public Graphic StripedBackground;
        public RectTransform BodyBackground;
        public RectTransform InnerBodyBackground;
        public RectTransform BodyHolder;
        public RectTransform LeftActionsHolder;
        public CanvasGroup LeftActionsGroup;
        public RectTransform RightActionsHolder;
        public CanvasGroup RightActionsGroup;

        Coroutine _CurrentAnimation = null;






        public void Start()
        {
            _CurrentAnimation = StartCoroutine(IntroAnim());
        }

        public void Close()
        {
            if (_CurrentAnimation != null && !IsAnimating)
                StopCoroutine(_CurrentAnimation);
            if (!IsAnimating)
                _CurrentAnimation = StartCoroutine(OutroAnim());
        }



        private void SetButtonVisibility(float t)
        {
            LeftActionsGroup.alpha = RightActionsGroup.alpha = TitleBarGroup.alpha = t * t;
            LeftActionsHolder.anchoredPosition *= new Vector2Frag(x: -10 * (1 - t));
            RightActionsHolder.anchoredPosition *= new Vector2Frag(x: -LeftActionsHolder.anchoredPosition.x);
            TitleBarHolder.anchoredPosition *= new Vector2Frag(y: 10 * (1 - t));
        }

        private IEnumerator IntroAnim()
        {
            ColorBackground.color = CommonSys.sMain.MainCamera.backgroundColor;

            // Button
            var buttonRoutine = StartCoroutine(Ease.Animate(0.3f, (t) =>
            {
                SetButtonVisibility(Ease.Get(t, EaseFunction.Cubic, EaseMode.Out));
            }));

            yield return Ease.Animate(0.8f, (t) =>
            {
                // Background
                DarkBackground.color *= new ColorFrag(a: Ease.Get(t * 2f, EaseFunction.Cubic, EaseMode.Out) * 0.75f);
                ColorBackground.color *= new ColorFrag(a: Ease.Get(t * 1.8f, EaseFunction.Cubic, EaseMode.Out) * 0.75f);
                StripedBackground.color *= new ColorFrag(a: Ease.Get(t * 1.6f - 0.2f, EaseFunction.Cubic, EaseMode.Out));

                // Body
                float ease1 = Ease.Get(t * 1.1f, EaseFunction.Exponential, EaseMode.Out);
                BodyBackground.anchorMin *= new Vector2Frag(y: .5f - .3f * Mathf.Pow(ease1, .5f));
                BodyBackground.anchorMax *= new Vector2Frag(y: .5f + .3f * Mathf.Pow(ease1, .5f));
                float ease2 = Ease.Get(t * 1.1f - 0.1f, EaseFunction.Exponential, EaseMode.Out);
                InnerBodyBackground.anchorMin *= new Vector2Frag(y: .5f - .5f * ease2);
                InnerBodyBackground.anchorMax *= new Vector2Frag(y: .5f + .5f * ease2);
                float ease3 = Ease.Get(t, EaseFunction.Exponential, EaseMode.Out);
                BodyHolder.anchoredPosition *= new Vector2Frag(x: 150 * (1 - ease3));
            });

            yield return buttonRoutine;

            _CurrentAnimation = null;
        }

        private IEnumerator OutroAnim()
        {

            // Button
            var buttonRoutine = StartCoroutine(Ease.Animate(0.3f, (t) =>
            {
                SetButtonVisibility(1 - Ease.Get(t, EaseFunction.Cubic, EaseMode.Out));
            }));

            
            yield return Ease.Animate(0.4f, (t) =>
            {
                IsAnimating = true;
                
                // Background
                DarkBackground.color *= new ColorFrag(a: (1 - Ease.Get(t * 1.5f, EaseFunction.Cubic, EaseMode.Out)) * 0.75f);
                ColorBackground.color *= new ColorFrag(a: (1 - Ease.Get(t * 1.2f, EaseFunction.Cubic, EaseMode.Out)) * 0.75f);
                StripedBackground.color *= new ColorFrag(a: (1 - Ease.Get(t * 1.2f - 0.2f, EaseFunction.Cubic, EaseMode.Out)));

                // Body
                float ease1 = Ease.Get(t * 1.5f, EaseFunction.Quadratic, EaseMode.Out);
                BodyBackground.anchorMin *= new Vector2Frag(y: .2f + .3f * ease1);
                BodyBackground.anchorMax *= new Vector2Frag(y: .8f - .3f * ease1);
                float ease2 = Ease.Get(t * 1.5f, EaseFunction.Cubic, EaseMode.Out);
                InnerBodyBackground.anchorMin *= new Vector2Frag(y: .5f * ease2);
                InnerBodyBackground.anchorMax *= new Vector2Frag(y: 1 - .5f * ease2);
                float ease3 = Ease.Get(t, EaseFunction.Exponential, EaseMode.In);
                BodyHolder.anchoredPosition *= new Vector2Frag(x: -1000 * ease3);
            });

            yield return buttonRoutine;

            _CurrentAnimation = null;
            Destroy(gameObject);
        }

        public bool IsAnimating = false;
    }
}