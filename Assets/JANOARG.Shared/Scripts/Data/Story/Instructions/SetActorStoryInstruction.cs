using System;
using System.Collections.Generic;
using JANOARG.Client.Behaviors.Storyteller;

namespace JANOARG.Shared.Data.Story.Instructions
{
    [Serializable]
    public class SetActorStoryInstruction : StoryInstruction 
    {
        public List<string> Actors = new();

        public override void OnTextBuild(Storyteller teller)
        {
            if (Actors.Count == 0) 
            {
                var narrator = teller.Constants.Actors.Find(x => string.IsNullOrEmpty(x.Alias));
                teller.SetNameLabelText("");
                teller.DialogueLabel.text += narrator.TextPrefix;
            }
            else if (Actors.Count == 1)
            {
                var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
                teller.SetNameLabelText(actor.Name);
                teller.DialogueLabel.text += actor.TextPrefix;
            }
        }
    }
}