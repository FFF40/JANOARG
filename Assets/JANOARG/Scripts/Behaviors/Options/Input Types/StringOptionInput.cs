using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StringOptionInput : OptionInput<string>
{
    [Space]
    public int Limit = 16;
    public TMP_Text ValueHolder;

    public new void Start() 
    {
        UpdateValue();
    }

    public new void UpdateValue() 
    {
        base.UpdateValue();
        ValueHolder.text = CurrentValue;
    }

    public void Edit()
    {
        OptionInputHandler.main.Edit(this);
    }
}
