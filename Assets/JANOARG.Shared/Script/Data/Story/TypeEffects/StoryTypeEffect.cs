using System;
using System.Collections;
using JANOARG.Client.Scripts.Behaviors.Storyteller;
using TMPro;

namespace JANOARG.Shared.Script.Data.Story.TypeEffects
{
    [Serializable]
    public abstract class StoryTypeEffect
    {
        public virtual IEnumerator OnCharacterReveal(Storyteller teller, TMP_Text text, int index, float startTime) 
        { yield return null; }
    }
}