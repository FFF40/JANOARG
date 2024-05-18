using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatOptionInput : OptionInput<float>
{
    [Space]
    public int Min = 0;
    public int Max = 100;
    public int Step = 1;
    public string Unit;
    [Space]
    public TMP_Text ValueHolder;
    public TMP_Text UnitLabel;

    public new void Start() 
    {
        UpdateValue();
    }

    public new void UpdateValue() 
    {
        base.UpdateValue();
        ValueHolder.text = CurrentValue.ToString();
        UnitLabel.text = "<alpha=#77>" + Unit;
    }

    public void Edit()
    {
        OptionInputHandler.main.Edit(this);
    }
}
