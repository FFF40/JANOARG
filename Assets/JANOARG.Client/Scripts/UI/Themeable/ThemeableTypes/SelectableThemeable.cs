using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Chartmaker.UI.Themeable.ThemeableTypes
{
    [RequireComponent(typeof(Selectable))]
    public class SelectableThemeable : Themeable<Selectable>
    {
        public string NormalID      = "ControlNormal";
        public string HighlightedID = "ControlHighlighted";
        public string SelectedID;
        public string PressedID = "ControlPressed";
        public string DisabledID;

        public override void SetColors()
        {
            ColorBlock colors = Target.colors;
        
            { if (Themer.sMain.Keys.TryGetValue(NormalID,      out Color color)) colors.normalColor      = color; }
            { if (Themer.sMain.Keys.TryGetValue(HighlightedID, out Color color)) colors.highlightedColor = color; }
            { if (Themer.sMain.Keys.TryGetValue(SelectedID,    out Color color)) colors.selectedColor    = color; }
            { if (Themer.sMain.Keys.TryGetValue(PressedID,     out Color color)) colors.pressedColor     = color; }
            { if (Themer.sMain.Keys.TryGetValue(DisabledID,    out Color color)) colors.disabledColor    = color; }
        
            Target.colors = colors;
        }
    }
}
