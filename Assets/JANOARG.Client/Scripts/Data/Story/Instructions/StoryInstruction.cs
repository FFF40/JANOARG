using System;
using System.Collections;
using JANOARG.Client.Behaviors.Storyteller;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public abstract class StoryInstruction
    {
        public virtual bool isSilent  => false;
        // Anything after this must go in a new chunk
        public virtual bool endsChunk => false;

        public virtual void OnTextBuild(Storyteller teller) {}
        // Doesn't show anything to the player
        public virtual IEnumerator OnSceneSet(Storyteller teller) => null;
        public virtual IEnumerator OnBeforeTextReveal(Storyteller teller) => null;
        public virtual IEnumerator OnTextReveal(Storyteller teller) => null;
        public virtual IEnumerator OnAfterTextReveal(Storyteller teller) => null;
    }
}