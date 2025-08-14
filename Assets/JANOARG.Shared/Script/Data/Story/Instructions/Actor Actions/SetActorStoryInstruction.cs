using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SetActorStoryInstruction : ActorActionStoryInstruction 
{

    public override void OnTextBuild(Storyteller teller)
    {
        // Narrator
        if (Actors.Count == 0)
        {
            var narrator = teller.Constants.Actors.Find(x => string.IsNullOrEmpty(x.Alias));
            teller.SetNameLabelText(" ");
            teller.DialogueLabel.text += narrator.TextPrefix;
        }
        // Actors
        else
        {
            Debug.Log($"Setting actors: {string.Join(", ", Actors)}");
            for (int i = 0; i < Actors.Count; i++)
            {
                string targetActor = Actors[i];
                bool isUnknown = targetActor.EndsWith("?");
                string trimmedAlias = targetActor.TrimEnd('?');

                Debug.Log($"Setting actor: {trimmedAlias} (Unknown: {isUnknown})");
                var actor = teller.Constants.Actors.Find(x => x.Alias == trimmedAlias);
                teller.CurrentActors.Add(actor);

                //TODO: Make the actor name stack if there are 2 or more actors

                string displayName = isUnknown ? "???" : actor.Name;
                Debug.Log($"Display name for actor '{trimmedAlias}': {displayName}");
                teller.SetNameLabelText(displayName);
                teller.DialogueLabel.text += actor.TextPrefix;
            }

        }
    }
}