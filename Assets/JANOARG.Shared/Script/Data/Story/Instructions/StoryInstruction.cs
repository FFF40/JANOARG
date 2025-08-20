
using System;
using System.Collections;
using JANOARG.Scripts.Behaviors.Storyteller;

[Serializable]
public abstract class StoryInstruction 
{
    public virtual void OnTextBuild(Storyteller teller) {}
    public virtual IEnumerator OnTextReveal(Storyteller teller) { yield return null; }
}