
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

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
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            IList list = (InspectorPanel.main.CurrentTimestamp?.Count ?? 0) > 0 ? InspectorPanel.main.CurrentTimestamp :
                InspectorPanel.main.CurrentObject is IList li && li.Contains(Item) ? li : null;
            if (list != null) TimelinePanel.main.BeginDragItem(list, eventData);
            else TimelinePanel.main.BeginDragItem(new List<object>() { Item }, eventData);
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (TimelinePanel.main.isDragged) return;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SelectItem();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClickItem();
        }

    }

    public void SetItem(object item, Lane lane = null)
    {
        Item = item;
        Lane = lane;
        SelectedBorder.gameObject.SetActive(InspectorPanel.main?.IsSelected(item) == true);
    }

    public void SelectItem()
    {
        if (PickerPanel.main.CurrentTimelinePickerMode == TimelinePickerMode.Delete)
        {
            Chartmaker.main.DeleteItem(Item);
        }
        else
        {
            if (Lane != null) InspectorPanel.main.SetObject(Lane);
            InspectorPanel.main.SetObject(Item);
        }
    }

    public void RightClickItem()
    {
        TimelinePanel.main.RightClickItem(this);
    }
}
