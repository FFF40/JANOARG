using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormEntryRange : FormEntry<float>
{
    public Slider Range;
    public TMP_InputField Field;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void RangeSet()
    {
        SetValue(Range.value);
        Field.text = CurrentValue.ToString();
    }

    public void FieldSet()
    {
        if (float.TryParse(Field.text, out float value))
        {
            Range.value = value;
            SetValue(Range.value);
        }
    }

    public void Reset()
    {
        Range.value = CurrentValue;
        Field.text = CurrentValue.ToString();
    }
}
