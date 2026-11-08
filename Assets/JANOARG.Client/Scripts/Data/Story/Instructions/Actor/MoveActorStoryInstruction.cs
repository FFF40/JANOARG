using System;
using System.Collections;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public class MoveActorStoryInstruction : StoryInstruction
    {
        public string Alias;
        public ActorPosition Destination;
        public float Duration;

        [StoryTag("MoveActor")]
        public MoveActorStoryInstruction(string id , string destination, string duration)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
               throw new ArgumentNullException("Actor Show Instruction: Alias is null or empty."); 
            }
            Alias = id;

            if (!Single.TryParse(duration, out Duration))
            {
               throw new ArgumentNullException("Actor Show Instruction: SpriteId is null or empty."); 
            }

            if (!Enum.TryParse(destination, true, out Destination))
            {
                throw new ArgumentException($"Invalid actor position: {destination}");
            }
        }

        public override IEnumerator OnBeforeTextReveal(Storyteller teller)
        {
            yield return teller.ActorStage.MoveActor(Alias,Destination,Duration);
        }

    }
}