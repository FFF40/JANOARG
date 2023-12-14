using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpEntry : MonoBehaviour
{
    public Button Button;
    public TMP_Text Text;
    public HelpModal Parent;
    public LayoutElement IndentBox;
    public Button ExpandButton;
    public RectTransform ExpandIcon;
    [Space]
    public string Target;
    public int Indent;
    public bool Expanded;

    public void Select()
    {
        Parent.LoadEntry(Target);
    }

    public void SetItem(string name, string path, int indent)
    {
        Target = path;
        Indent = indent;
        Text.text = name;
        IndentBox.minWidth = 15 * indent + 24;
    }

    public void ToggleExpand()
    {
        Expanded = !Expanded;
        ExpandIcon.localEulerAngles = Vector3.forward * (Expanded ? 180 : -90);
        Parent.UpdateEntries();
    }
}
