using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Shared.Data.ChartInfo;

namespace JANOARG.Shared.Data.Story.Instructions
{
    //Sets actor names in the Dialogue
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
                string finalDisplayNames = "";
                for (int i = 0; i < Actors.Count; i++)
                {
                    // Check if the actor name ends with '?', indicating to hide the name of the actor
                    string targetActor = Actors[i];
                    bool isUnknown = targetActor.EndsWith("?");
                    string trimmedAlias = targetActor.TrimEnd('?');

                    // Find the actor in the constants
                    Debug.Log($"Setting actor: {trimmedAlias} (Unknown: {isUnknown})");
                    var actor = teller.Constants.Actors.Find(x => x.Alias == trimmedAlias);
                    teller.CurrentActors.Add(actor);

                    // Make the display name as ??? if the actor has a '?' on the end
                    string displayName = isUnknown ? "???" : actor.Name;

                    //Make the actor name stack if there are 2 or more actors
                    finalDisplayNames += displayName;

                    // Add a comma if not the last actor
                    if (i < Actors.Count - 1)
                    {
                        finalDisplayNames += ", ";
                    }

                }

                teller.SetNameLabelText(finalDisplayNames);
                teller.DialogueLabel.text += "<color=#bdf>"; //hardcoded for now 

            }
        }
    }
}