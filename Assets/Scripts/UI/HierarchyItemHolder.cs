using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HierarchyItemHolder : MonoBehaviour
{
    public Button Button;
    public Image Icon;
    public GameObject SelectedBackground;
    public TMP_InputField NameField;
    public TMP_Text Text;
    public LayoutElement IndentBox;
    public Button ExpandButton;
    public RectTransform ExpandIcon;

    public HierarchyItem Target;

    bool isNameFieldDirty;

    public void Select()
    {
        HierarchyPanel.main.Select(Target);
    }

    public void RightClickSelect()
    {
        HierarchyPanel.main.RightClickSelect(Target, this);
    }

    public void SetItem(HierarchyItem item, int indent)
    {
        Target = item;
        Text.text = item.Name + " <alpha=#77>" + item.Subname;
        IndentBox.minWidth = 12 * indent + 24;
        UpdateExpand();
    }

    public void Rename()
    {
        switch (Target.Target)
        {
            case LaneStyle: 
                NameField.text = ((LaneStyle)Target.Target).Name;
                break;
            case HitStyle: 
                NameField.text = ((HitStyle)Target.Target).Name;
                break;
            case LaneGroup: 
                NameField.text = ((LaneGroup)Target.Target).Name;
                break;
            case Lane: 
                NameField.text = ((Lane)Target.Target).Name;
                break;
            default: 
                return;
        }
        NameField.gameObject.SetActive(true);
        NameField.Select();
        isNameFieldDirty = false;
    }

    public void SetNameFieldDirty()
    {
        isNameFieldDirty = true;
    }

    public void DoneRename()
    {
        NameField.gameObject.SetActive(false);
        if (!isNameFieldDirty) return;
        switch (Target.Target)
        {
            case LaneGroup:
                if (string.IsNullOrEmpty(NameField.text)) return;
                var action = new ChartmakerGroupRenameAction() {
                    From = ((LaneGroup)Target.Target).Name,
                    To = InspectorPanel.main.GetNewGroupName(NameField.text.Trim(), ((LaneGroup)Target.Target)),
                };
                action.Redo();
                Chartmaker.main.History.AddAction(action);
                Chartmaker.main.OnHistoryUpdate();
                break;
            case HitStyle: case LaneStyle: case Lane: 
                Chartmaker.main.SetItem(Target.Target, "Name", NameField.text.Trim());
                break;
            default: 
                return;
        }
        HierarchyPanel.main.UpdateHierarchy();
    }

    public void ToggleExpand()
    {
        HierarchyPanel.main.ToggleExpand(Target);
    }

    public void UpdateExpand()
    {
        ExpandIcon.localEulerAngles = Vector3.forward * (Target.Expanded ? 180 : -90);
    }
}
