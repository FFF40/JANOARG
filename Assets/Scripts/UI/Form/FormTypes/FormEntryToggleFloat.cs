using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormEntryToggleFloat : FormEntry<float>
{
    public Toggle Toggle;
    public TMP_InputField Field;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset()
    {
        recursionBuster = true;
        Field.gameObject.SetActive(Toggle.isOn = !float.IsNaN(CurrentValue));
        Field.text = Toggle.isOn ? CurrentValue.ToString() : "0";
        recursionBuster = false;
    }

    public void SetValue(bool value)
    {
        Field.gameObject.SetActive(value);
        SetValue(value ? Field.text : "NaN");
    }
    
    public void SetValue(string value)
    {
        if (float.TryParse(value, out float v)) SetValue(v);
    }
}
