using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitEffect : MonoBehaviour
{
    public float? Accuracy;

    [Space]
    public Image InnerCircle;
    [Space]
    public Image SmallSquare;
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
            JudgeArc.gameObject.SetActive(false);
            Triangles.gameObject.SetActive(false);
            for (float a = 0; a < 1; a += Time.deltaTime / .4f) 
            {
                SmallSquare.rectTransform.sizeDelta = Vector2.one * (40 * Mathf.Pow(a, .2f) + 20);
                InnerCircle.rectTransform.sizeDelta = Vector2.one * (-12 * Mathf.Pow(a, .2f) + 20);
                MainGroup.alpha = 1 - Mathf.Pow(a, 5);
                yield return null;
            }
        }
        else 
        {
            SmallSquare.gameObject.SetActive(false);

            LeftArc.rectTransform.eulerAngles = Vector3.back * Mathf.Max(0, (float)Accuracy * 180);
            RightArc.rectTransform.eulerAngles = LeftArc.rectTransform.eulerAngles + Vector3.forward * 180;
            LeftArc.FillAmount = RightArc.FillAmount = (1 - Mathf.Abs((float)Accuracy)) * .5f;
            for (float a = 0; a < 1; a += Time.deltaTime / .4f) 
            {
                Triangles.sizeDelta = JudgeArc.sizeDelta = Vector2.one * (80 * Mathf.Pow(a, .1f) + 40);
                Triangles.localEulerAngles = Vector3.forward * ((float)Accuracy * -180 * Mathf.Pow(a, .1f));
                InnerCircle.rectTransform.sizeDelta = Vector2.one * (-32 * Mathf.Pow(a, .2f) + 40);
                LeftArc.InsideRadius = RightArc.InsideRadius = FullArc.InsideRadius = 1 - Mathf.Pow(1 - a, 15) - (1 - a) * .1f;
                MainGroup.alpha = 1 - Mathf.Pow(a, 5);
                yield return null;
            }
        }
        Destroy(gameObject);
    }

    void SetColor() 
    {
        InnerCircle.color = FullArc.color = LeftArc.color = RightArc.color = TopTriangle.color = BottomTriangle.color = 
            SmallSquare.color = ChartPlayer.main.CurrentChart.Pallete.InterfaceColor;
    }
}
