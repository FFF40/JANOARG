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
        // This instruction does not have a time delay, it just sets the choice
        teller.ChoiceDictionary[Item.StoryFlag] = Item.Value;
        yield return null; // Yield to allow the Storyteller to update
    }
}