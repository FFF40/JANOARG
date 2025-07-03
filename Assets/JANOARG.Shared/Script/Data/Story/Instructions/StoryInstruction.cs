
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public abstract class StoryInstruction
{
    public virtual void OnTextBuild(Storyteller teller) { }
    public virtual IEnumerator OnTextReveal(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnBackgroundChange(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnActorAction(Storyteller teller) { yield return null; }

    //To reduce manual labor
    public void Log(string message, Storyteller teller,[CallerMemberName] string caller = "")
    {
        Debug.LogWarning($"Chunk {teller.CurrentChunkIndex}\nClass :{GetType().Name} \nFunction:{message} in {caller}");
    }

}