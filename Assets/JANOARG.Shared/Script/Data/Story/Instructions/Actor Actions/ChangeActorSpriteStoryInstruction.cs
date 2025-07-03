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
    public ChangeActorSpriteStoryInstruction(string spriteAlias, string actor)
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

            //Get actor and its sprite handler
            var actor = GetActor(teller);
            Debug.Log($"{actor.Alias}");
            ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);

            //Check if its exists
            if (TargetActorSpriteHandler == null)
            {
                //Get its sprite, make an instance of a sprite handler, and set the actor string to the actor's alias
                TargetActorSprite = GetSprite(TargetSpriteName, teller);
                Debug.Log($"{TargetActorSprite}");
                ActorSpriteHandler target = UnityEngine.Object.Instantiate(teller.ActorSpriteItem, teller.ActorHolder);
                Debug.Log($"{target}: ");
                Debug.Log($"{actor.Alias}");
                Debug.Log($" {TargetSpriteName}");
                Debug.Log($"{target}: {actor.Alias}, {TargetSpriteName}");
                target.SetActor(actor.Alias, TargetSpriteName); //Set alias

                //Change the sprite to the target sprite and add the sprite handler to the Storyteller
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
        var actor = GetActor(teller);
        if (actor.ActorSprites.Find(x => x.Alias == targetSprite) != null)
        {
            return true;
        }
        return false;
    }

    private Sprite GetSprite(string targetSprite, Storyteller teller)
    {
        var actor = GetActor(teller);

        #region Debug
        if (teller.Constants.PlaceholderActorSprite.Sprite == null)                 Log("Placeholder Sprite is not set", teller);
        if (teller.Constants.Actors.Find(x => x.Alias == Actors[0]) == null)        Log("targetActor is not present",teller);
        if (actor.ActorSprites.Find(x => x.Alias == targetSprite) == null)          Log("targetSprite is not present",teller);
        if (teller.Constants.Actors.Find(x => x.Alias == Actors[0]) == null)        Log("targetActor is not present",teller);
        if (Actors == null || Actors.Count == 0)                                    Log("Actors is null or empty",teller);
        #endregion

        
        if (actor.ActorSprites.Find(x => x.Alias == targetSprite)== null) return teller.Constants.PlaceholderActorSprite.Sprite;
        return actor.ActorSprites.Find(x => x.Alias == targetSprite).Sprite;
    }

    public ActorInfo GetActor(Storyteller teller)
    {
        #region Debug
        if (teller.Constants == null)               Log("Constant is not set",teller);
        if (teller.Constants.Actors == null)        Log("Actors in Constants is null",teller);
        if (Actors == null || Actors.Count == 0)    Log("Actors is null or empty",teller);
        #endregion
        
        if (teller.Constants == null || teller.Constants.Actors == null || Actors == null || Actors.Count == 0)
            return null;

        return teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
    }

    public void ChangeSprite(ActorSpriteHandler TargetActorSpriteHandler, string TargetSprite, Storyteller teller, Sprite actorSprite)
    {
        #region Debug
        if (TargetActorSpriteHandler == null)       Log("TargetActorSpriteHandler is null",teller);
        if (string.IsNullOrEmpty(TargetSprite))     Log("TargetSprite is null or empty",teller);
        if (actorSprite == null)                    Log("actorSprite is null",teller);
        #endregion

        if (CheckIfSpriteExists(TargetSprite, teller))
            TargetActorSpriteHandler.SetActorSprite(actorSprite);
        else
            TargetActorSpriteHandler.SetActorSprite(actorSprite);
    }

    

    
}