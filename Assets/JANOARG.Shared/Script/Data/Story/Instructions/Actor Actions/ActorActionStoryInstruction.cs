using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ActorActionStoryInstruction : StoryInstruction
{
    public List<string> Actors = new();
    public Sprite TargetActorSprite;
    public string TargetSpriteName;

    #region Init Sprite 
    public void InitSpriteHandler(Storyteller teller, bool isInvisible = false)
    {
        var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
        ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);

        if (TargetActorSpriteHandler == null)
        {
            ActorSpriteHandler target = UnityEngine.Object.Instantiate(teller.ActorSpriteItem, teller.ActorHolder);
            TargetActorSprite = GetSprite("normal", teller); //default sprite
            target.SetActor(actor.Alias, TargetSpriteName); //Set alias

            ChangeSprite(target, teller, TargetActorSprite,isInvisible);
            teller.Actors.Add(target);
        }
    }

    public Sprite GetSprite(string targetSprite, Storyteller teller)
    {
        var actor = GetActor(teller);

        #region Debug
        if (teller.Constants.PlaceholderActorSprite.Sprite == null) Log("Placeholder Sprite is not set", teller);
        if (teller.Constants.Actors.Find(x => x.Alias == Actors[0]) == null) Log("targetActor is not present", teller);
        if (actor.ActorSprites.Find(x => x.Alias == targetSprite) == null) Log("targetSprite is not present. Set sprite to placeholder sprite", teller);
        if (Actors == null || Actors.Count == 0) Log("Actors is null or empty", teller);
        #endregion

        if (actor.ActorSprites.Find(x => x.Alias == targetSprite) == null) return teller.Constants.PlaceholderActorSprite.Sprite;
        return actor.ActorSprites.Find(x => x.Alias == targetSprite).Sprite;
    }

    public ActorInfo GetActor(Storyteller teller)
    {
        #region Debug
        if (teller.Constants == null) Log("Constant is not set", teller);
        if (teller.Constants.Actors == null) Log("Actors in Constants is null", teller);
        if (Actors == null || Actors.Count == 0) Log("Actors is null or empty", teller);
        #endregion

        if (teller.Constants == null || teller.Constants.Actors == null || Actors == null || Actors.Count == 0)
            return null;

        return teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
    }

    public void ChangeSprite(ActorSpriteHandler TargetActorSpriteHandler, Storyteller teller, Sprite actorSprite, bool isInvisible)
    {
        #region Debug
        if (TargetActorSpriteHandler == null) Log("TargetActorSpriteHandler is null", teller);
        if (actorSprite == null) Log("actorSprite is null", teller);
        #endregion

        TargetActorSpriteHandler.SetActorSprite(actorSprite, isInvisible);
    }

    #endregion



}