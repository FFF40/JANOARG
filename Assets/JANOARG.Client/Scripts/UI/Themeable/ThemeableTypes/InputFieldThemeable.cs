using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Chartmaker.UI.Themeable.ThemeableTypes
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldThemeable : Themeable<TMP_InputField>
    {
        public string NormalID      = "BorderNormal";
        public string HighlightedID = "BorderHighlighted";
        public string SelectedID    = "BorderSelected";
        public string PressedID     = "BorderPressed";
        public string DisabledID;
        public string SelectionID = "FieldSelection";

        public override void SetColors()
        {
            ColorBlock colors = Target.colors;
        
            { if (Themer.sMain.Keys.TryGetValue(NormalID,      out Color color)) colors.normalColor      = color; }
            { if (Themer.sMain.Keys.TryGetValue(HighlightedID, out Color color)) colors.highlightedColor = color; }
            { if (Themer.sMain.Keys.TryGetValue(SelectedID,    out Color color)) colors.selectedColor    = color; }
            { if (Themer.sMain.Keys.TryGetValue(PressedID,     out Color color)) colors.pressedColor     = color; }
            { if (Themer.sMain.Keys.TryGetValue(DisabledID,    out Color color)) colors.disabledColor    = color; }
            { if (Themer.sMain.Keys.TryGetValue(SelectionID,   out Color color)) Target.selectionColor   = color; }
        
            Target.colors = colors;
        }
    }
}
