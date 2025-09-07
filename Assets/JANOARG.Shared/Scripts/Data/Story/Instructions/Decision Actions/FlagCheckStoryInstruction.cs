using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using JANOARG.Client.Behaviors.Storyteller;

namespace JANOARG.Shared.Data.Story.Instructions
{

    // STILL IN PROGRESS <-------------------------------------------------------------
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
}