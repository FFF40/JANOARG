using JANOARG.Client.UI;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Player
{
    /// <summary>
    /// Visual effect shown on each note hit.
    /// Animation is driven externally by <see cref="JudgeScreenManager.Update"/> — no coroutines,
    /// no per-hit heap allocation.
    /// </summary>
    public class JudgeScreenEffect : MonoBehaviour
    {
        public CanvasGroup      Group;
        public GraphicCircleGPU RingBackground;
        public GraphicCircleGPU RingFill1;
        public GraphicCircleGPU RingFill2;
        public GraphicCircleGPU CircleFill;

        public float Size = 120;

        public void SetAccuracy(float? acc)
        {
            if (acc == null)
            {
                RingFill1.fillAmount = RingFill2.fillAmount = 1;
                RingBackground.sides = RingFill1.sides = RingFill2.sides = 4;
                Size = 60;
            }
            else
            {
                Size = 120;
                RingBackground.sides = RingFill1.sides = RingFill2.sides = 0;
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

        /// <summary>
        /// Advance the animation by one frame. Called by <see cref="JudgeScreenManager"/> each Update.
        /// <paramref name="x"/> is linear progress in [0, 1].
        /// </summary>
        public void Tick(float x)
        {
            float expEased = Ease.Get(1 - x, EaseFunction.Exponential, EaseMode.In);
            float ease = 1 - expEased * expEased;

            RingBackground.rectTransform.sizeDelta = Vector2.one * (40 + (Size * ease) + (x * 10));
            CircleFill.rectTransform.sizeDelta = Vector2.one * (40 - (30 * ease));

            float circleEased = Ease.Get(x, EaseFunction.Circle, EaseMode.In);
            Group.alpha = EaseUtils.ToZero(1, circleEased);

            float ease3 = ease * .96f + x * .04f;
            RingBackground.insideRadius = RingFill1.insideRadius = RingFill2.insideRadius = ease3;
        }
    }
}
