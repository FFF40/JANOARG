using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FormEntryVector2 : FormEntry<Vector2>
{
    public TMP_InputField FieldX;
    public TMP_InputField FieldY;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset()
    {
        recursionBuster = true;
        FieldX.text = CurrentValue.x.ToString();
        FieldY.text = CurrentValue.y.ToString();
        recursionBuster = false;
    }
    
    public void SetValue(int index, string value)
    {
        if (float.TryParse(value, out float v)) 
        {
            CurrentValue[index] = v;
            SetValue(CurrentValue);
        }
    }
    public void SetX(string value) => SetValue(0, value);
    public void SetY(string value) => SetValue(1, value);
}
