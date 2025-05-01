
using System;
using System.Collections;
using TMPro;

[Serializable]
public abstract class StoryTypeEffect
{
    public virtual IEnumerator OnCharacterReveal(Storyteller teller, TMP_Text text, int index, float startTime) 
    { yield return null; }
}