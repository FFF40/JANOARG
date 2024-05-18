using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OptionItem : MonoBehaviour
{
    public TMP_Text TitleLabel;
}

public class OptionInput<T> : OptionItem
{
    public T CurrentValue;

    public Func<T> OnGet;
    public Action<T> OnSet;

    public void Set(T value)
    {
        OnSet(value);
    }

    public void UpdateValue() 
    {
        CurrentValue = OnGet();
    }

    public void Start() 
    {
        UpdateValue();
    }
}

public enum MultiValueType {
    None,
    PerJudgment,
    PerHitType,
}
