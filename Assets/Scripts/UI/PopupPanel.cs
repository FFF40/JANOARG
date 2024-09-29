using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupPanel : MonoBehaviour
{
    public RectTransform HitTestRect;
    Rect worldRect;

    public void Start()
    {
        gameObject.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) 
        {
            RectTransform rt = HitTestRect ? HitTestRect : (RectTransform)transform;
            if (!RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, null))
            {
                Close();
            }
        }
    }

    bool isAnimating;

    public void Open()
    {
        gameObject.SetActive(true);
        StartCoroutine(Intro());
    }

    public void Close()
    {
        if (!isAnimating) gameObject.SetActive(false);
    }

    IEnumerator Intro()
    {
        isAnimating = true;
        RectTransform rt = (RectTransform)transform;
        rt.anchoredPosition -= new Vector2(-2, 2);
        yield return new WaitForSecondsRealtime(0.05f);
        rt.anchoredPosition += new Vector2(-2, 2);
        isAnimating = false;
    }
}
