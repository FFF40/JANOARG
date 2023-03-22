using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitEffect : MonoBehaviour
{
    public float? Accuracy;
    public int AccIndex = 0;

    [Space]
    public Image InnerCircle;
    [Space]
    public RectTransform JudgeArc;
    public GraphicCircle FullArc;
    public GraphicCircle LeftArc;
    public GraphicCircle RightArc;
    [Space]
    public RectTransform Triangles;
    public Image TopTriangle;
    public Image BottomTriangle;
    [Space]
    public CanvasGroup MainGroup;

    IEnumerator Start()
    {
        SetColor();
        if (Accuracy == null)
        {
            Triangles.gameObject.SetActive(false);
            FullArc.Resolution = LeftArc.Resolution = RightArc.Resolution = 4;
            LeftArc.FillAmount = RightArc.FillAmount = .5f;
            for (float a = 0; a < 1; a += Time.deltaTime / .4f) 
            {
                Triangles.sizeDelta = JudgeArc.sizeDelta = Vector2.one * (75 * Mathf.Pow(a, .1f) + 75) * ChartPlayer.main.HitEffectSize[AccIndex];
                LeftArc.InsideRadius = RightArc.InsideRadius = FullArc.InsideRadius = 1 - Mathf.Pow(1 - a, 15) - (1 - a) * .05f;
                InnerCircle.rectTransform.sizeDelta = Vector2.one * (-12 * Mathf.Pow(a, .2f) + 20);
                MainGroup.alpha = (1 - Mathf.Pow(a, 5)) * ChartPlayer.main.HitEffectAlpha[AccIndex];
                yield return null;
            }
        }
        else 
        {
            AccIndex = Accuracy == 0 ? 0 : Mathf.Abs((float)Accuracy) < 1 ? 1 : 2;
            LeftArc.rectTransform.eulerAngles = Vector3.back * Mathf.Max(0, (float)Accuracy * 180);
            RightArc.rectTransform.eulerAngles = LeftArc.rectTransform.eulerAngles + Vector3.forward * 180;
            LeftArc.FillAmount = RightArc.FillAmount = (1 - Mathf.Abs((float)Accuracy)) * .5f;
            for (float a = 0; a < 1; a += Time.deltaTime / .4f) 
            {
                Triangles.sizeDelta = JudgeArc.sizeDelta = Vector2.one * (100 * Mathf.Pow(a, .1f) + 100) * ChartPlayer.main.HitEffectSize[AccIndex];
                Triangles.localEulerAngles = Vector3.forward * ((float)Accuracy * -180 * Mathf.Pow(a, .1f));
                InnerCircle.rectTransform.sizeDelta = Vector2.one * (-32 * Mathf.Pow(a, .2f) + 40);
                LeftArc.InsideRadius = RightArc.InsideRadius = FullArc.InsideRadius = 1 - Mathf.Pow(1 - a, 15) - (1 - a) * .05f;
                MainGroup.alpha = (1 - Mathf.Pow(a, 5)) * ChartPlayer.main.HitEffectAlpha[AccIndex];
                yield return null;
            }
        }
        Destroy(gameObject);
    }

    void SetColor() 
    {
        InnerCircle.color = FullArc.color = LeftArc.color = RightArc.color = TopTriangle.color = BottomTriangle.color = 
            ChartPlayer.main.CurrentChart.Pallete.InterfaceColor;
    }
}
