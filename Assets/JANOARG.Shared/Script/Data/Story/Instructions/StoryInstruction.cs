
using System;
using System.Collections;

[Serializable]
public abstract class StoryInstruction
{
    public virtual void OnTextBuild(Storyteller teller) { }
    public virtual IEnumerator OnTextReveal(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnBackgroundChange(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnActorAction(Storyteller teller) { yield return null; }

}