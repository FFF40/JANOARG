using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class DecisionItemStoryInstruction : StoryInstruction
{
    public string StoryFlag;
    public string Dialog;

    public DecisionItemStoryInstruction(string flag, string text)
    {
        StoryFlag = flag;
        Dialog = text;
    }

    public override void AddChoices(Storyteller teller)
    {
        
    }
}
