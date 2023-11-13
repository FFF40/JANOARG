using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class SelectableThemeable : Themeable<Selectable>
{
    public string NormalID = "ControlNormal";
    public string HighlightedID = "ControlHighlighted";
    public string SelectedID;
    public string PressedID = "ControlPressed";
    public string DisabledID;

    public override void SetColors()
    {
        ColorBlock colors = Target.colors;
        { if (Themer.main.Keys.TryGetValue(NormalID, out Color color)) colors.normalColor = color; }
        { if (Themer.main.Keys.TryGetValue(HighlightedID, out Color color)) colors.highlightedColor = color; }
        { if (Themer.main.Keys.TryGetValue(SelectedID, out Color color)) colors.selectedColor = color; }
        { if (Themer.main.Keys.TryGetValue(PressedID, out Color color)) colors.pressedColor = color; }
        { if (Themer.main.Keys.TryGetValue(DisabledID, out Color color)) colors.disabledColor = color; }
        Target.colors = colors;
    }
}
