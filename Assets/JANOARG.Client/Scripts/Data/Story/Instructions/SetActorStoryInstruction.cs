using System;
using System.Collections.Generic;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Constant;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public class SetActorStoryInstruction : StoryInstruction
    {
        public List<string> Actors = new();

        public override void OnTextBuild(Storyteller teller)
        {
            if (Actors.Count == 0)
            {
                ActorInfo narrator = teller.Constants.Actors.Find(x => string.IsNullOrEmpty(x.Alias));
                teller.SetNameLabelText("");
                teller.DialogueLabel.text += narrator.TextPrefix;
            }
            else if (Actors.Count == 1)
            {
                ActorInfo actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
                teller.SetNameLabelText(actor.Name);
                teller.DialogueLabel.text += actor.TextPrefix;
            }
        }
    }
}