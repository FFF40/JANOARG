using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Theme", menuName = "JANOARG/Theme")]
public class Theme : ScriptableObject
{
    public List<ThemeKey> Keys;

    public static List<ThemeKey> FromDict(Dictionary<string, Color> source) 
    {
        List<ThemeKey> list = new();
        foreach (KeyValuePair<string, Color> item in source)
            list.Add(new ThemeKey { Key = item.Key, Value = item.Value });
        return list;
    }

    public static Dictionary<string, Color> ToDict(List<ThemeKey> source) 
    {
        Dictionary<string, Color> dict = new();
        foreach (ThemeKey item in source)
            dict.Add(item.Key, item.Value);
        return dict;
    }
}

[System.Serializable]
public struct ThemeKey
{
    public string Key;
    public Color Value;
}