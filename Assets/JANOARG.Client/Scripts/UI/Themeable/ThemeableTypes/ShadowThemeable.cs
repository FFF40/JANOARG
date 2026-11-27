using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Chartmaker.UI.Themeable.ThemeableTypes
{
    [RequireComponent(typeof(Shadow))]
    public class ShadowThemeable : Themeable<Shadow>
    {
        public string ID = "ShadowNormal";

        public override void SetColors()
        {
            if (Themer.sMain.Keys.TryGetValue(ID, out Color color)) 
                Target.effectColor = color;
        }
    }
}
