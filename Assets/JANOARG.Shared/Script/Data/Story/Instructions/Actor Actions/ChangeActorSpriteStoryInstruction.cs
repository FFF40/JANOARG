using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ChangeActorSpriteStoryInstruction : ActorActionStoryInstruction
{

    [StoryTag("switch")]
    public ChangeActorSpriteStoryInstruction(string param)
    {
        Debug.LogWarning(param);
        TargetSprite = param;
    }

    private bool CheckIfSpriteExists(string targetSprite, Storyteller teller)
    {
        var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
        if (actor.ActorSprites.Find(x => x.Alias == targetSprite) != null) {
            Debug.Log("true");
            return true;
        }
        Debug.Log("false");
        return false;
    }

    public override void OnActorStaticAction(Storyteller teller)
    {
        Debug.LogWarning("ONactor");
        Debug.LogWarning($"Count: {Actors.Count}");
        if (Actors.Count == 0)
        {
            // Just do nothing
        }
        else if (Actors.Count == 1)
        {
            Debug.LogWarning(teller.Constants.Actors[0]);
            var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
            Debug.LogWarning(actor.Alias);

            ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);
            Debug.LogWarning(TargetActorSpriteHandler);

            if (TargetActorSpriteHandler == null)
            {
                ActorSpriteHandler target = UnityEngine.Object.Instantiate(teller.ActorSpriteItem, teller.ActorHolder);
                target.SetActor(actor.Alias);

                if (CheckIfSpriteExists(TargetSprite, teller))
                    target.SetActorSprite(TargetSprite);
                else
                    target.SetActorSprite();

                teller.Actors.Add(target);
            }
            else
            {
                if (CheckIfSpriteExists(TargetSprite, teller))
                    TargetActorSpriteHandler.SetActorSprite(TargetSprite);
                else
                    TargetActorSpriteHandler.SetActorSprite();
            }
        }
    }
}