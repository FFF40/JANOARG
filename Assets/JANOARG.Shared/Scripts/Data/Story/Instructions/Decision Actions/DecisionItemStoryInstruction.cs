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
    public class DecisionItemStoryInstruction : StoryInstruction
    {
        public List<DecisionItem> Items = new List<DecisionItem>();

        public override void AddChoices(Storyteller teller)
        {
            teller.CurrentDecisionItems.Clear();
            teller.CurrentDecisionItems.AddRange(Items);
        }
    }
}