using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldThemeable : Themeable<TMP_InputField>
{
    public string NormalID = "BorderNormal";
    public string HighlightedID = "BorderHighlighted";
    public string SelectedID = "BorderSelected";
    public string PressedID = "BorderPressed";
    public string DisabledID;
    public string SelectionID = "FieldSelection";

    public override void SetColors()
    {
        ColorBlock colors = Target.colors;
        { if (Themer.main.Keys.TryGetValue(NormalID, out Color color)) colors.normalColor = color; }
        { if (Themer.main.Keys.TryGetValue(HighlightedID, out Color color)) colors.highlightedColor = color; }
        { if (Themer.main.Keys.TryGetValue(SelectedID, out Color color)) colors.selectedColor = color; }
        { if (Themer.main.Keys.TryGetValue(PressedID, out Color color)) colors.pressedColor = color; }
        { if (Themer.main.Keys.TryGetValue(DisabledID, out Color color)) colors.disabledColor = color; }
        { if (Themer.main.Keys.TryGetValue(SelectionID, out Color color)) Target.selectionColor = color; }
        Target.colors = colors;
    }
}
