using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SetActorStoryInstruction : ActorActionStoryInstruction 
{

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

            //Check if sprite is present
            var actorSprite = teller.Actors.Find(x => x.CurrentActor == actor.Alias);
            if (actorSprite != null)
            {
                //+ Add bounce on actor sprite
                
            }
            else
            {
                //- Skip bounce
            }
            
            
        }
    }
}