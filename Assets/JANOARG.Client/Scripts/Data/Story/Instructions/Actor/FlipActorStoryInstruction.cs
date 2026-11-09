using System;
using UnityEngine;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;
using System.Collections;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public class FlipActorStoryInstruction : StoryInstruction
    {
        public string Alias;

        [StoryTag("FlipActor")]
        public FlipActorStoryInstruction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
               throw new ArgumentNullException("Actor Show Instruction: Alias is null or empty."); 
            }
            Alias = id;
        }

        public override IEnumerator OnBeforeTextReveal(Storyteller teller)
        {
            yield return teller.ActorStage.Flip(Alias);
        }
    }
}