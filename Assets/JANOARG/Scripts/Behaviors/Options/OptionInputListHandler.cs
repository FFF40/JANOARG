using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Windows;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System.Linq;

public class OptionInputListHandler : MonoBehaviour, IInitializePotentialDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    public static OptionInputListHandler main;

    public RectTransform ListHolder;
    public OptionInputListItem ItemSample;
    public List<OptionInputListItem> Items;
    [Space]
    public float ItemHeight = 40;
    [Space]
    public Color NormalTextColor;
    public Color SelectedTextColor;

    [HideInInspector]
    public int CurrentPosition;
    [HideInInspector]
    public int OldPosition;

    [HideInInspector]
    public float ScrollOffset;
    [HideInInspector]
    public float ScrollVelocity;

    [HideInInspector]
    public bool IsPointerDown;

    public void Awake() 
    {
        main = this;
    }

    public void Update() 
    {
        if (!IsPointerDown)
        {
            float target = CurrentPosition * ItemHeight;
            if (target == ScrollOffset) 
            {
                // noop
            }
            else if (Mathf.Abs(target - ScrollOffset) > 1e-3f) 
            {
                ScrollOffset = Mathf.Lerp(target, ScrollOffset, Mathf.Pow(1e-3f, Time.deltaTime));
                ListHolder.anchoredPosition = new (ListHolder.anchoredPosition.x, ScrollOffset);
            }
            else 
            {
                ScrollOffset = target;
                ListHolder.anchoredPosition = new (ListHolder.anchoredPosition.x, ScrollOffset);
            }
        }
        if (OldPosition != CurrentPosition) 
        {
            if (OldPosition >= 0 && OldPosition < Items.Count) SetItemActive(Items[OldPosition], false);
            OldPosition = CurrentPosition;
            SetItemActive(Items[CurrentPosition], true);
        }
    }

    public void Finish() 
    {
        Items[OldPosition].OnSelect();
    }

    public void SetList<T>(ListOptionInput<T> input)
    {
        ClearList();
        CurrentPosition = -1;

        int index = 0;
        foreach (KeyValuePair<T, string> value in input.ValidValues) 
        {
            var item = Instantiate(ItemSample, ListHolder);
            int i = index;
            item.Text.text = value.Value;
            item.OnSelect = () => {
                input.Set(value.Key);
                input.UpdateValue();
            };
            item.Button.onClick.AddListener(() => {
                ScrollToItem(i);
            });
            Items.Add(item);
            if (Equals(value.Key, input.CurrentValue)) CurrentPosition = index;
            index++;
        }

        ScrollOffset = CurrentPosition * ItemHeight;
        ListHolder.anchoredPosition = new (ListHolder.anchoredPosition.x, ScrollOffset);

        OldPosition = CurrentPosition;
        SetItemActive(Items[CurrentPosition], true);
    }

    public void ClearList() 
    {
        foreach (var item in Items) Destroy(item.gameObject);
        Items.Clear();
    }

    public void SetItemActive(OptionInputListItem item, bool active) 
    {
        item.Button.interactable = !active;
        item.Text.color = active ? SelectedTextColor : NormalTextColor;
    }

    public void ScrollToItem(int index) 
    {
        IsPointerDown = false;
        CurrentPosition = index;
    }

    public void OnInitializePotentialDrag(PointerEventData data)
    {
        IsPointerDown = true;
        ScrollVelocity = 0;
    }

    public void OnDrag(PointerEventData data)
    {
        if (!IsPointerDown) return;
        float delta = data.delta.y / transform.lossyScale.x;
        ScrollOffset += delta;
        ListHolder.anchoredPosition = new (ListHolder.anchoredPosition.x, Mathf.Clamp(ScrollOffset, ItemHeight * -.4f, ItemHeight * (Items.Count - .6f)));
        CurrentPosition = Mathf.RoundToInt(ListHolder.anchoredPosition.y / ItemHeight);
    }

    public void OnEndDrag(PointerEventData data)
    {
        OnPointerUp(data);
    }

    public void OnPointerUp(PointerEventData data)
    {
        IsPointerDown = false;
        ScrollOffset = Mathf.Clamp(ScrollOffset, ItemHeight * -.4f, ItemHeight * (Items.Count - .6f));
    }
}