using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorFilterItem : MonoBehaviour
{
    public Button Button;
    public Image Icon;
    public GameObject SelectedBackground;
    public TMP_Text Text;
    public LayoutElement IndentBox;
    public Button ExpandButton;
    public RectTransform ExpandIcon;

    public HierarchyItem Target;

    public void Select()
    {
        HierarchyPanel.main.Select(Target);
    }

    public void SetItem(HierarchyItem item, int indent)
    {
        Target = item;
        Text.text = item.Name + " <alpha=#77>" + item.Subname;
        IndentBox.minWidth = 12 * indent + 24;
        UpdateExpand();
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
