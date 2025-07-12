
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public abstract class StoryInstruction
{
    #region Static
    public virtual void OnTextBuild(Storyteller teller) { }
    public virtual void OnMusicChange(Storyteller teller) { }
    #endregion

    #region Dynamic
    public virtual IEnumerator OnTextReveal(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnBackgroundChange(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnActorAction(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnMusicPlay(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnSFXPlay(Storyteller teller) { yield return null; }

    #endregion

    //To reduce manual labor
    public void Log(string message, Storyteller teller, [CallerMemberName] string caller = "")
    {
        Debug.LogWarning($"Chunk {teller.CurrentChunkIndex}, Class:{GetType().Name}, Function:{message} in {caller}()");
    }

}