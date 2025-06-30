using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeActorSpriteStoryInstruction : StoryInstruction 
{
    public List<string> Actors = new();

    public override void OnTextBuild(Storyteller teller)
    {
        if (Actors.Count == 0)
        {
            // Just do nothing
        }
        else if (Actors.Count == 1)
        {
            var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);

            //Spawn a sprite holder if there is no holder responisble for the actor
            if (true) {
                
            }

            //Change sprite


        }
    }
}