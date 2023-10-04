using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public static Tooltip main;

    public TMP_Text Label;
    public CanvasGroup Group;

    public void Awake()
    {
        main = this;
    }

    public void Show(string text, RectTransform target, TooltipPositionMode positionMode)
    {
        Label.text = text;
        Group.alpha = 1;

        RectTransform rt = (RectTransform)transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        Rect curRect = rt.rect;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Rect tarRect = new(corners[0], corners[2] - corners[0]);

        if (positionMode == TooltipPositionMode.Cursor)
        {
            rt.anchoredPosition = new (
                Mathf.Clamp(Input.mousePosition.x + 18, 5, Screen.width - curRect.width - 5),
                Mathf.Clamp(Input.mousePosition.y - 18 - curRect.height, 5, Screen.height - curRect.height - 5)
            );
        }
        if (positionMode == TooltipPositionMode.Up)
        {
            rt.anchoredPosition = new (
                Mathf.Clamp(tarRect.xMin + tarRect.width / 2 - curRect.width / 2, 5, Screen.width - curRect.width - 5),
                Mathf.Clamp(tarRect.yMax + 2, 5, Screen.height - curRect.height - 5)
            );
        }
        else if (positionMode == TooltipPositionMode.Down)
        {
            rt.anchoredPosition = new (
                Mathf.Clamp(tarRect.xMin + tarRect.width / 2 - curRect.width / 2, 5, Screen.width - curRect.width - 5),
                Mathf.Clamp(tarRect.yMin - curRect.height - 2, 5, Screen.height - curRect.height - 5)
            );
        }
        else if (positionMode == TooltipPositionMode.Left)
        {
            rt.anchoredPosition = new (
                Mathf.Clamp(tarRect.xMin - curRect.height - 2, 5, Screen.width - curRect.width - 5),
                Mathf.Clamp(tarRect.yMin + tarRect.height / 2 - curRect.height / 2, 5, Screen.height - curRect.height - 5)
            );
        }
        else if (positionMode == TooltipPositionMode.Right)
        {
            rt.anchoredPosition = new (
                Mathf.Clamp(tarRect.xMax + 2, 5, Screen.width - curRect.width - 5),
                Mathf.Clamp(tarRect.yMin + tarRect.height / 2 - curRect.height / 2, 5, Screen.height - curRect.height - 5)
            );
        }
    }

    public void Hide()
    {
        Group.alpha = 0;
    }
}

public enum TooltipPositionMode
{
    Cursor, Up, Down, Left, Right
}
