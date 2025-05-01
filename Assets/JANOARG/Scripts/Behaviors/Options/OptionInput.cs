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

public enum MultiValueType
{
    PerJudgment,
    PerHitType,
}

public class MultiValueFieldData 
{
    public string Name;
    public Color Color;
    

    public static Dictionary<MultiValueType, List<MultiValueFieldData>> Info = new()
    {
        {MultiValueType.PerJudgment, new () {
            new () { Name = "Flawless", Color = new (1, 1, .6f) },
            new () { Name = "Misaligned", Color = new (.6f, .7f, 1) },
            new () { Name = "Broken", Color = new (.6f, .6f, .6f) },
        }},
        {MultiValueType.PerHitType, new () {
            new () { Name = "Normal", Color = new (.8f, .9f, 1) },
            new () { Name = "Catch", Color = new (1, 1, .8f) },
        }},
    };
}
