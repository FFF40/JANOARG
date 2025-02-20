using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Sidebar : MonoBehaviour
{
    public RectTransform SafeArea;
    public float Width = 750;
    public bool SetActiveOnHide = false;

    public bool isAnimating { get; protected set; }

    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(ShowAnimation());
    }

    public IEnumerator ShowAnimation()
    {
        isAnimating = true;
        RectTransform rt = GetComponent<RectTransform>();

        void LerpContent(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Quartic, EaseMode.Out);

            rt.anchoredPosition = Vector3.left * (2000 + (Width - SafeArea.sizeDelta.y / 2) * (1 - ease));
        }
        
        for (float a = 0; a < 1; a += Time.deltaTime / .4f) 
        {
            LerpContent(a);
            yield return null;
        }
        LerpContent(1);

        isAnimating = false;
    }

    public void Hide()
    {
        StartCoroutine(HideAnimation());
    }

    public IEnumerator HideAnimation()
    {
        isAnimating = true;
        RectTransform rt = GetComponent<RectTransform>();

        void LerpContent(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Quartic, EaseMode.In);

            rt.anchoredPosition = Vector3.left * (2000 + (Width - SafeArea.sizeDelta.y / 2) * ease);
        }
        
        for (float a = 0; a < 1; a += Time.deltaTime / .3f) 
        {
            LerpContent(a);
            yield return null;
        }
        LerpContent(1);

        if (SetActiveOnHide) gameObject.SetActive(false);

        isAnimating = false;
    }
}