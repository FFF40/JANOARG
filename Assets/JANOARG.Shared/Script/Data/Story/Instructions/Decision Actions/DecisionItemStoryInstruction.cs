using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class DecisionItemStoryInstruction : StoryInstruction 
{
    public List<DecisionItem> Items = new List<DecisionItem>();

    public override void AddChoices(Storyteller teller)
    {
        teller.CurrentDecisionItems.Clear();
        teller.CurrentDecisionItems.AddRange(Items);
    }
}