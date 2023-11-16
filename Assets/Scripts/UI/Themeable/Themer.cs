using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Themer : MonoBehaviour
{
    public static Themer main;

    public Dictionary<string, Color> Keys;

    public List<Theme> Themes;

    public void InitTheme()
    {
        main = this;
        string name = Chartmaker.Preferences.Theme;
        Theme theme = Themes.Find(x => x.name == name);
        if (!theme) theme = Themes[0];
        Keys = Theme.ToDict(theme.Keys);
        SetAllColors();
    }

    public void SetAllColors()
    {
        foreach (Themeable themeable in FindObjectsOfType<Themeable>())
        {
            themeable.SetColors();
        }
    }
}
