using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ActorActionStoryInstruction : StoryInstruction
{
    /// <summary>
    /// List of actors in the scene
    /// </summary>
    public List<string> Actors = new();

    /// <summary>
    /// Sprite of the target actor
    /// </summary>
    public Sprite TargetActorSprite;

    /// <summary>
    /// Name of the target actor sprite
    /// </summary>
    public string TargetSpriteName;

    #region Init Sprite 
    
    /// <summary>
    /// Initializes the sprite handler for the target actor.
    /// This method checks if the actor's sprite handler already exists in the storyteller's actor list.
    /// If it does not exist, it creates a new instance of ActorSpriteHandler, sets the actor alias,
    /// and changes the sprite to the default sprite for that actor.
    /// 
    /// The isInvisible parameter can be used to set the visibility of the actor sprite.
    /// </summary>
    /// <param name="TargetActor"></param>
    /// <param name="teller"></param>
    /// <param name="isInvisible"></param>
    public void InitSpriteHandler(string TargetActor, Storyteller teller, bool isInvisible = false)
    {
        // Check if the actor sprite handler already exists
        var actor = teller.Constants.Actors.Find(x => x.Alias == TargetActor);
        ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);

        //Set the default sprite if the handler does not exist
        if (TargetActorSpriteHandler == null)
        {
            ActorSpriteHandler target = UnityEngine.Object.Instantiate(teller.ActorSpriteItem, teller.ActorHolder);
            TargetActorSprite = GetSprite(actor.Alias, "normal", teller); //default sprite
            target.SetActor(actor.Alias, TargetSpriteName); //Set alias

            ChangeSprite(target, teller, TargetActorSprite, isInvisible);
            teller.Actors.Add(target);
        }
    }

    /// <summary>
    /// Gets the sprite for the target actor.
    /// </summary>
    /// <param name="TargetActor"></param>
    /// <param name="TargetSprite"></param>
    /// <param name="teller"></param>
    /// <returns></returns>
    public Sprite GetSprite(string TargetActor, string TargetSprite, Storyteller teller)
    {
        var actor = GetActor(TargetActor, teller);

        #region Debug
        if (teller.Constants.PlaceholderActorSprite.Sprite == null) Log("Placeholder Sprite is not set", teller);
        if (teller.Constants.Actors.Find(x => x.Alias == Actors[0]) == null) Log("targetActor is not present", teller);
        if (actor.ActorSprites.Find(x => x.Alias == TargetSprite) == null) Log("targetSprite is not present. Set sprite to placeholder sprite", teller);
        if (Actors == null || Actors.Count == 0) Log("Actors is null or empty", teller);
        #endregion

        // If the actor or sprite is not found, return the placeholder sprite, otherwise return the sprite
        if (actor.ActorSprites.Find(x => x.Alias == TargetSprite) == null) return teller.Constants.PlaceholderActorSprite.Sprite;
        return actor.ActorSprites.Find(x => x.Alias == TargetSprite).Sprite;
    }

    /// <summary>
    /// Gets the actor information based on the target actor's alias.
    /// </summary>
    /// <param name="TargetActor"></param>
    /// <param name="teller"></param>
    /// <returns></returns>
    public ActorInfo GetActor(string TargetActor, Storyteller teller)
    {
        #region Debug
        if (teller.Constants == null) Log("Constant is not set", teller);
        if (teller.Constants.Actors == null) Log("Actors in Constants is null", teller);
        if (Actors == null || Actors.Count == 0) Log("Actors is null or empty", teller);
        #endregion

        if (teller.Constants == null || teller.Constants.Actors == null || Actors == null || Actors.Count == 0)
            return null;

        // Find the actor in the constants
        return teller.Constants.Actors.Find(x => x.Alias == TargetActor);
    }

    /// <summary>
    /// Changes the sprite of the target actor sprite handler.
    /// </summary>
    /// <param name="TargetActorSpriteHandler"></param>
    /// <param name="teller"></param>
    /// <param name="actorSprite"></param>
    /// <param name="isInvisible"></param>
    public void ChangeSprite(ActorSpriteHandler TargetActorSpriteHandler, Storyteller teller, Sprite actorSprite, bool isInvisible)
    {
        #region Debug
        if (TargetActorSpriteHandler == null) Log("TargetActorSpriteHandler is null", teller);
        if (actorSprite == null) Log("actorSprite is null", teller);
        #endregion

        //Change the sprite of the target actor sprite handler
        TargetActorSpriteHandler.SetActorSprite(actorSprite, isInvisible);
    }

    #endregion



}