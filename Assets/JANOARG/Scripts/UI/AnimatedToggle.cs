using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

[ExecuteAlways][RequireComponent(typeof(CanvasRenderer))]
public class AnimatedToggle : Selectable, IPointerClickHandler
{
    bool _value;

    public RectTransform OuterBox;
    public RectTransform InnerBox;
    public Graphic Checkmark;

    public UnityEvent<bool> OnValueChanged;

    public bool Value 
    {
        get 
        { 
            return _value; 
        }
        set 
        { 
            _value = value; 
            if (routine != null) StopCoroutine(routine);
            SetToggleEase(_value ? 1 : 0);
        }
    }

    public new void Start ()
    {
        base.Start();
        SetToggleEase(_value ? 1 : 0);
    }


    public void OnPointerClick (PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Toggle();
        }
    }

    public void Toggle() 
    {
        _value = !_value;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ToggleAnim(_value ? 1 : 0));
        OnValueChanged.Invoke(_value);
    }

    float progress;
    Coroutine routine;

    public IEnumerator ToggleAnim(float to) 
    {
        float from = progress;
        yield return Ease.Animate(0.2f, x => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            SetToggleEase(Mathf.Lerp(from, to, ease));
        });
        routine = null;
    }

    public void SetToggleEase(float ease)
    {
        progress = ease;
        OuterBox.anchorMin = OuterBox.anchorMax = OuterBox.pivot = new Vector2(ease, .5f);
        OuterBox.anchoredPosition = Vector2.zero;
        InnerBox.sizeDelta = (OuterBox.rect.size - new Vector2 (6, 4)) * (1 - ease);
        Checkmark.color = new (Checkmark.color.r, Checkmark.color.g, Checkmark.color.b, ease);
    }
}
