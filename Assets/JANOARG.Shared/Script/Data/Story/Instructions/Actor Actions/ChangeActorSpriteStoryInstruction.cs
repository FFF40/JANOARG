using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ChangeActorSpriteStoryInstruction : ActorActionStoryInstruction
{
    [StoryTag("switch")]
    public ChangeActorSpriteStoryInstruction(string actor, string spriteAlias)
    {
        TargetSpriteName = spriteAlias;
        Actors.Add(actor);
    }

    public override IEnumerator OnActorAction(Storyteller teller)
    {
        if (Actors.Count == 0)
        {
            // Just do nothing
        }
        else
        {
            Debug.Log(Actors);

            for (int i = 0; i < Actors.Count; i++)
            {
                //Init
                var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[i]);
                InitSpriteHandler(actor.Alias,teller);
                ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);

                //Change
                TargetActorSprite = GetSprite(actor.Alias,TargetSpriteName, teller);
                ChangeSprite(TargetActorSpriteHandler, teller, TargetActorSprite,false);
            }
        }

        yield return null;
    }

    
}