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
        else if (Actors.Count == 1)
        {
            Debug.Log(Actors);

            //Init
            InitSpriteHandler(teller);
            var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
            ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);

            //Change
            TargetActorSprite = GetSprite(TargetSpriteName, teller);
            ChangeSprite(TargetActorSpriteHandler, teller, TargetActorSprite,false);

        }

        yield return null;
    }

    
}