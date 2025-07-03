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
    public ChangeActorSpriteStoryInstruction(string param, List<string> actors)
    {
        TargetSpriteName = param;
        Actors = actors;
    }

    public override IEnumerator OnActorAction(Storyteller teller)
    {
        Debug.LogWarning("ONactor");
        Debug.LogWarning($"Count: {Actors.Count}");
        if (Actors.Count == 0)
        {
            // Just do nothing
        }
        else if (Actors.Count == 1)
        {
            var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
            ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);

            if (TargetActorSpriteHandler == null)
            {
                TargetActorSprite = GetSprite(TargetSpriteName, teller);
                ActorSpriteHandler target = UnityEngine.Object.Instantiate(teller.ActorSpriteItem, teller.ActorHolder);
                target.SetActor(actor.Alias, TargetSpriteName); //Set alias

                ChangeSprite(target, TargetSpriteName, teller, TargetActorSprite);
                teller.Actors.Add(target);
            }
            else
            {
                TargetActorSprite = GetSprite(TargetSpriteName, teller);
                ChangeSprite(TargetActorSpriteHandler, TargetSpriteName, teller, TargetActorSprite);
            }
        }

        yield return null;
    }

    private bool CheckIfSpriteExists(string targetSprite, Storyteller teller)
    {
        var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
        if (actor.ActorSprites.Find(x => x.Alias == targetSprite) != null)
        {
            Debug.Log("true");
            return true;
        }
        Debug.Log("false");
        return false;
    }

    private Sprite GetSprite(string targetSprite, Storyteller teller)
    {
        var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
        return actor.ActorSprites.Find(x => x.Alias == targetSprite).Sprite;
    }

    public void ChangeSprite(ActorSpriteHandler TargetActorSpriteHandler,string TargetSprite, Storyteller teller, Sprite actorSprite)
    {
        if (TargetActorSpriteHandler == null)
        {
            Debug.LogWarning("TargetActorSpriteHandler is null");
        }

        if (string.IsNullOrEmpty(TargetSprite))
        {
            Debug.LogWarning("TargetSprite is null or empty");
        }

        if (teller == null)
        {
            Debug.LogWarning("Storyteller (teller) is null");
        }

        if (actorSprite == null)
        {
            Debug.LogWarning("actorSprite is null");
        }

        if (CheckIfSpriteExists(TargetSprite, teller))
            TargetActorSpriteHandler.SetActorSprite(actorSprite);
        else
            TargetActorSpriteHandler.SetActorSprite(actorSprite);
    }
}