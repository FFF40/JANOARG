using System;
using System.Collections;
using JANOARG.Client.Scripts.Behaviors.Storyteller;

namespace JANOARG.Shared.Scripts.Data.Story.Instructions
{
    [Serializable]
    public abstract class StoryInstruction 
    {
        public virtual void OnTextBuild(Storyteller teller) {}
        public virtual IEnumerator OnTextReveal(Storyteller teller) { yield return null; }
    }
}