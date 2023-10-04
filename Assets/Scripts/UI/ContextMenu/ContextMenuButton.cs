using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ContextMenuButton : Button
{
    public UnityEvent onHover;
    public float hoverDelay = 0.5f;

    Coroutine currentRoutine;

    IEnumerator hoverRoutine()
    {
        yield return new WaitForSecondsRealtime(hoverDelay);
        onHover.Invoke();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        currentRoutine = StartCoroutine(hoverRoutine());
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        StopCoroutine(currentRoutine);
    }
}
