using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormEntryBool : FormEntry<bool>
{
    public Toggle Toggle;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset()
    {
        recursionBuster = true;
        Toggle.isOn = CurrentValue;
        recursionBuster = false;
    }
}
