using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HierarchyFilterItem : MonoBehaviour
{
    public LayoutElement IndentBox;
    public TMP_Text Text;
    public Image Icon;
    public Toggle InHierarchyToggle;
    public Toggle InSearchResultToggle;

    HierarchyFilterSetting setting;

    public void SetItem(HierarchyFilterSetting item)
    {
        setting = item;
        IndentBox.minWidth = item.Indent * 12;
        Icon.sprite = item.Icon;
        Text.text = item.Name;
        Reset();
    }

    public void Reset() 
    {
        InHierarchyToggle.interactable = setting.InHierarchyToggleable;
        InHierarchyToggle.graphic.GetComponent<GraphicThemeable>().ID = setting.InHierarchyToggleable ? "ControlContent" : "Content1";
        InHierarchyToggle.SetIsOnWithoutNotify(setting.InHierarchyDefault);
        InSearchResultToggle.interactable = setting.InSearchResultToggleable;
        InSearchResultToggle.graphic.GetComponent<GraphicThemeable>().ID = setting.InSearchResultToggleable ? "ControlContent" : "Content1";
        InSearchResultToggle.SetIsOnWithoutNotify(setting.InSearchResultDefault);
    }

    public void OnChange() 
    {
        HierarchyPanel.main.UpdateHolders();
    }
}
