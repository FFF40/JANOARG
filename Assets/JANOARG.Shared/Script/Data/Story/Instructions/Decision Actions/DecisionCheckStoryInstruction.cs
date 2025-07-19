using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class DecisionItemStoryInstruction : StoryInstruction 
{
    public List<DecisionItem> Items = new List<DecisionItem>();

    public override void AddChoices(Storyteller teller)
    {
        // 
    }
}