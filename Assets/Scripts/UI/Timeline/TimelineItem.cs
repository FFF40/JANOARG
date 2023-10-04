
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimelineItem : Selectable, IPointerClickHandler
{
    public object Item;
    public Image Border;
    public Image Indicator;
    public Image SelectedBorder;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        SelectItem();
    }

    public void SetItem(object item)
    {
        Item = item;
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
            InspectorPanel.main.SetObject(Item);
        }
    }
}
