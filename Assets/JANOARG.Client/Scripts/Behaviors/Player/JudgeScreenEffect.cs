using System.Collections;
using JANOARG.Client.UI;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Player
{
    public class JudgeScreenEffect : MonoBehaviour
    {
        public CanvasGroup   Group;
        public GraphicCircle RingBackground;
        public GraphicCircle RingFill1;
        public GraphicCircle RingFill2;
        public GraphicCircle CircleFill;

        public float Size = 120;

        public void SetAccuracy(float? acc)
        {
            if (acc == null)
            {
                RingFill1.fillAmount = RingFill2.fillAmount = 1;
                RingBackground.resolution = RingFill1.resolution = RingFill2.resolution = 4;
                Size = 60;
            }
            else
            {
                //Debug.Log(acc);
                Size = 120;
                RingBackground.resolution = RingFill1.resolution = RingFill2.resolution = 90;
                RingFill1.fillAmount = RingFill2.fillAmount = (1 - Mathf.Abs((float)acc)) / 2;
                RingFill1.rectTransform.localEulerAngles = Vector3.back * Mathf.Max((float)acc * 180, 0);
                RingFill2.rectTransform.localEulerAngles = Vector3.forward * (RingFill1.rectTransform.localEulerAngles.z + 180);
            }
        }

        public void SetColor(Color color)
        {
            RingFill1.color = RingFill2.color = color;
            CircleFill.color = RingBackground.color = color * new Color(1, 1, 1, .3f);
        }

        public void Play() => StartCoroutine(Animate(false));
        public void PlayOneShot() => StartCoroutine(Animate(true));
        public IEnumerator Animate(bool isOneShot)
        {
            yield return Ease.Animate(0.4f, (x) => {
                float ease = 1 - Mathf.Pow(Ease.Get(1 - x, EaseFunction.Exponential, EaseMode.In), 2);
                RingBackground.rectTransform.sizeDelta = Vector2.one * (40 + (Size * ease) + (x * 10));
                CircleFill.rectTransform.sizeDelta = Vector2.one * (40 - (30 * ease));

                float ease2 = Ease.Get(x, EaseFunction.Circle, EaseMode.In);
                Group.alpha = 1 - ease2;

                float ease3 = ease * .96f + x * .04f;
                RingBackground.insideRadius = RingFill1.insideRadius = RingFill2.insideRadius = ease3;
            });

            if (isOneShot) Destroy(gameObject);
            else PlayerScreen.sMain.JudgeScreenManager.ReturnEffect(this);
        }
    }
}
