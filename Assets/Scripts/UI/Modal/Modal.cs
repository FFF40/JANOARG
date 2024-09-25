using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Modal : MonoBehaviour, IPointerDownHandler
{
    public void Start()
    {
       StartCoroutine(Intro());
    }

    IEnumerator Intro()
    {
        RectTransform rt = (RectTransform)transform;
        rt.anchoredPosition -= new Vector2(-2, 2);
        yield return new WaitForSecondsRealtime(0.05f);
        rt.anchoredPosition += new Vector2(-2, 2);
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
    }

    Vector2 dragStart = new();
    Vector2 posStart = new();

    public void OnHeaderDragStart(BaseEventData eventData) => OnHeaderDragStart((PointerEventData)eventData);
    public void OnHeaderDragStart(PointerEventData eventData)
    {
        RectTransform rt = (RectTransform)transform;
        dragStart = eventData.position;
        posStart = rt.anchoredPosition;
        rt.SetAsLastSibling();
    }

    public void OnHeaderDrag(BaseEventData eventData) => OnHeaderDrag((PointerEventData)eventData);
    public void OnHeaderDrag(PointerEventData eventData)
    {
        RectTransform rt = (RectTransform)transform;
        RectTransform parent = (RectTransform)rt.parent;
        rt.anchoredPosition = posStart + (eventData.position - dragStart);
        Rect rect = rt.rect;
        rect.position += rt.anchoredPosition;
        if (rect.xMin < parent.rect.width / -2) rt.anchoredPosition += Vector2.right * (-rect.xMin - parent.rect.width / 2);
        if (rect.xMax > parent.rect.width / 2) rt.anchoredPosition += Vector2.left * (rect.xMax - parent.rect.width / 2);
        if (rect.yMin < parent.rect.height / -2) rt.anchoredPosition += Vector2.up * (-rect.yMin - parent.rect.height / 2);
        if (rect.yMax > parent.rect.height / 2) rt.anchoredPosition += Vector2.down * (rect.yMax - parent.rect.height / 2);
    }
}
