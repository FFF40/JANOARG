using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public string Text;
    [Space]
    public float Delay = 1;
    public TooltipPositionMode PositionMode;

    public IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(Delay);
        Tooltip.main.Show(Text, (RectTransform)transform, PositionMode);
    }

    public Coroutine CurrentRoutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        CurrentRoutine = StartCoroutine(ShowRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopCoroutine(CurrentRoutine);
        Tooltip.main.Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopCoroutine(CurrentRoutine);
        Tooltip.main.Hide();
    }
}
