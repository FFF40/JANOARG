using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Chartmaker.UI.Themeable.ThemeableTypes
{
    [RequireComponent(typeof(Graphic))]
    public class GraphicThemeable : Themeable<Graphic>
    {
        public string ID;

        public override void SetColors()
        {
            if (Themer.sMain.Keys.TryGetValue(ID, out Color color)) 
                Target.color = color;
        }
    }
}
