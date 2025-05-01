using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeScreenEffect : MonoBehaviour
{
    public CanvasGroup Group;

    public GraphicCircle RingBackground;
    public GraphicCircle RingFill1;
    public GraphicCircle RingFill2;
    public GraphicCircle CircleFill;

    public float Size = 120;

    public void SetAccuracy(float? acc) 
    {
        if (acc == null) 
        {
            RingFill1.FillAmount = RingFill2.FillAmount = 1;
            RingBackground.Resolution = RingFill1.Resolution = RingFill2.Resolution = 4;
            Size = 60;
        }
        else 
        {
            Debug.Log(acc);
            RingFill1.FillAmount = RingFill2.FillAmount = (1 - Mathf.Abs((float)acc)) / 2;
            RingFill1.rectTransform.localEulerAngles = Vector3.back * Mathf.Max((float)acc * 180, 0);
            RingFill2.rectTransform.localEulerAngles = Vector3.forward * (RingFill1.rectTransform.localEulerAngles.z + 180);
        }
    }

    public void SetColor(Color color) 
    {
        RingFill1.color = RingFill2.color = color;
        CircleFill.color = RingBackground.color = color * new Color(1, 1, 1, .3f);
    }

    public IEnumerator Start()
    {
        yield return Ease.Animate(0.4f, (x) => {
            float ease = 1 - Mathf.Pow(Ease.Get(1 - x, EaseFunction.Exponential, EaseMode.In), 2);
            RingBackground.rectTransform.sizeDelta = Vector2.one * (40 + Size * ease + x * 10);
            CircleFill.rectTransform.sizeDelta = Vector2.one * (40 - 30 * ease);

            float ease2 = Ease.Get(x, EaseFunction.Circle, EaseMode.In);
            Group.alpha = 1 - ease2;

            float ease3 = (1 - Mathf.Pow(Ease.Get(1 - x, EaseFunction.Exponential, EaseMode.In), 2)) * .96f + x * .04f;
            RingBackground.InsideRadius = RingFill1.InsideRadius = RingFill2.InsideRadius = ease3;
        });

        Destroy(gameObject);
    }
}


