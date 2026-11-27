using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Chartmaker.UI.Themeable
{
    public class Themer : MonoBehaviour
    {
        public static Themer sMain;

        public Dictionary<string, Color> Keys;

        public Theme LightTheme;
        public Theme DarkTheme;

        public bool IsDarkTheme { get; private set; }

        public void SetDarkTheme(bool darkTheme)
        {
            if (IsDarkTheme == darkTheme) return;
            IsDarkTheme = darkTheme;
            InitTheme();
            SetAllColors();
        }

        public void Awake()
        {
            sMain = this;
        }

        public void InitTheme()
        {
            Theme theme = IsDarkTheme ? DarkTheme : LightTheme;
     
            Keys = Theme.ToDict(theme.Keys);
     
            SetAllColors();
        }

        private void SetAllColors()
        {
            foreach (Themeable themeable in FindObjectsByType<Themeable>())
                themeable.SetColors();
        }
    }
}
