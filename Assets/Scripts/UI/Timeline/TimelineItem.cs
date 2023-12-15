
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimelineItem : Selectable, IPointerDownHandler, IPointerClickHandler
{
    public object Item;
    public Lane Lane;
    public Image Border;
    public Image Indicator;
    public Image SelectedBorder;

    public virtual new void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        TimelinePanel.main.BeginDragItem(new List<object>() { Item }, eventData);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (TimelinePanel.main.isDragged) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        SelectItem();
    }

    public void SetItem(object item, Lane lane = null)
    {
        Item = item;
        Lane = lane;
        SelectedBorder.gameObject.SetActive(InspectorPanel.main?.IsSelected(item) == true);
    }

    public void SelectItem()
    {
        if (PickerPanel.main.CurrentMode == PickerMode.Delete)
        {
            Chartmaker.main.DeleteItem(Item);
        }
        else
        {
            if (Lane != null) InspectorPanel.main.SetObject(Lane);
            InspectorPanel.main.SetObject(Item);
        }
    }
}
