using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class FlagCheckStoryInstruction : StoryInstruction 
{
    public List<DecisionItem> Items = new List<DecisionItem>();

    public override void AddChoices(Storyteller teller)
    {
        teller.CurrentFlagChecks.Clear();
        teller.CurrentFlagChecks.AddRange(Items);
    }
}