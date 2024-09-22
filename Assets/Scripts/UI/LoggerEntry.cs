using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoggerEntry : MonoBehaviour
{
    public Button Button;
    public TMP_Text MessageLabel;
    public TMP_Text StackTraceLabel;
    public Image Icon;
    public ContentSizeFitter Sizer;

    public RectTransform rectTransform;

    BorderlessWindow.LoggerEntry Target;

    public LoggerModal Parent;

    public void Select()
    {
        Parent.LoadEntry(Target, this);
    }

    public void SetItem(BorderlessWindow.LoggerEntry target, float offset, bool active, LoggerModal parent)
    {
        Parent = parent;
        Target = target;
        Icon.sprite = target.LogType == LogType.Log ? parent.InfoIcon : target.LogType == LogType.Warning ? parent.WarningIcon : parent.ErrorIcon;
        rectTransform.anchoredPosition = new(0, -offset);
        Button.interactable = !active;
        if (active)
        {
            MessageLabel.text = target.Message;
            StackTraceLabel.gameObject.SetActive(true);
            StackTraceLabel.text = target.StackTrace;
            MessageLabel.enableWordWrapping = true;
        }
        else 
        {
            MessageLabel.text = Regex.Match(target.Message + "\n" + target.StackTrace, @"^[^\n]*\n[^\n]*").Value;
            StackTraceLabel.gameObject.SetActive(false);
            MessageLabel.enableWordWrapping = false;
        }
        Sizer.enabled = true;
    }
}
