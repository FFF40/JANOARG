using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FormEntryKeybind : FormEntry<Keybind>
{
    public string Category;
    public TMP_Text Field;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset()
    {
        recursionBuster = true;
        Field.text = CurrentValue.ToString();
        recursionBuster = false;
    }

    public void StartChange() 
    {
        KeyboardHandler.main.StartKeybindChange(this);
    }
}
