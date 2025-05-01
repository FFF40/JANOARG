using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Themer : MonoBehaviour
{
    public string Key;
    public Graphic[] Targets;

    public static Dictionary<string, List<Themer>> Themers = new Dictionary<string, List<Themer>>();
    public static Dictionary<string, Color> Colors = new Dictionary<string, Color>();

    public static void SetColor(string key, Color color) {
        Colors[key] = color;
        if (!Themers.ContainsKey(key)) return;
        foreach (Themer t in Themers[key]) t.UpdateColors();
    }

    void OnEnable()
    {
        if (!Themers.ContainsKey(Key)) Themers[Key] = new List<Themer>();
        Themers[Key].Add(this);
        UpdateColors();
    }

    void OnDisable()
    {
        Themers[Key].Remove(this);
    }

    public void UpdateColors() {
        if (!Colors.ContainsKey(Key)) return;
        Color color = Colors[Key];
        foreach (Graphic g in Targets) {
            g.color = color;
        }
    }
}
