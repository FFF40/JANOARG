using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class DecisionItem
{
    public string StoryFlag;
    public string Value;
    public string Dialog;
    public DecisionComparisionType ComparisionType = DecisionComparisionType.Equal;


    
    public DecisionItem(string flag, string value, string text)
    {
        StoryFlag = flag;
        Value = value;
        Dialog = text;
    }

    public DecisionItem(string flag, string value, string text, DecisionComparisionType comparisionType)
    {
        StoryFlag = flag;
        Value = value;
        Dialog = text;
        ComparisionType = comparisionType;
    }



}

public enum DecisionComparisionType
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}