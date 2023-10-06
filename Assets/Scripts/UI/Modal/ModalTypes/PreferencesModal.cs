using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreferencesModal : Modal
{
    public static PreferencesModal main;

    public RectTransform Holder;
    public Button[] TabButtons;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    public new void Start()
    {
        base.Start();
        SetTab(0);
    }

    public void SetTab(int tab)
    {
        for (int a = 0; a < TabButtons.Length; a++) 
        {
            TabButtons[a].interactable = tab != a;
        }
    }
}
