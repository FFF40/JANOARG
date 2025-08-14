using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecisionItemHandler : MonoBehaviour
{
    public string StoryFlag;
    public string Value;

    public TMP_Text DecisionText;
    // public Storage Storage = new Storage("save");

    public void Setup(DecisionItem item)
    {
        StoryFlag = item.StoryFlag;
        Value = item.Value;
        DecisionText.text = item.Dialog;
    }


    public void SaveDecision()
    {
        

    }
}

