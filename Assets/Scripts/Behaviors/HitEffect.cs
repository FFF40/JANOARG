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
    public Image FullArc;
    public Image LeftArc;
    public Image RightArc;
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
            LeftArc.fillClockwise = RightArc.fillClockwise = Accuracy < 0;
            LeftArc.fillAmount = RightArc.fillAmount = 1 - Mathf.Abs((float)Accuracy);
            for (float a = 0; a < 1; a += Time.deltaTime / .5f) 
            {
                Triangles.sizeDelta = JudgeArc.sizeDelta = Vector2.one * (80 * Mathf.Pow(a, .2f) + 40);
                Triangles.localEulerAngles = Vector3.forward * ((float)Accuracy * -180 * Mathf.Pow(a, .1f));
                InnerCircle.rectTransform.sizeDelta = Vector2.one * (-32 * Mathf.Pow(a, .2f) + 40);
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
