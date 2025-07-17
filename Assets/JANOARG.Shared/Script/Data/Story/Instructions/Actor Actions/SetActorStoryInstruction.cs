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
        else
        {
            for (int i = 0; i < Actors.Count; i++)
            {
                var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[i]);
                teller.CurrentActors.Add(actor);

                //TODO: Make the actor name stack if there are 2 or more actors
                teller.SetNameLabelText(actor.Name);
                teller.DialogueLabel.text += actor.TextPrefix;
            }

        }
    }
}