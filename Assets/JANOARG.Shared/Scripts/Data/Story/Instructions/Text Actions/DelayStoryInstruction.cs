using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using JANOARG.Client.Behaviors.Storyteller;

namespace JANOARG.Shared.Data.Story.Instructions
{

    [Serializable]
    public class DelayStoryInstruction : StoryInstruction
    {
        public float Duration;
        public bool ScaleWithCharacterDuration;

        [StoryTag("wait")]
        public DelayStoryInstruction(string duration)
        {
            Duration = ParseDuration(duration);
            ScaleWithCharacterDuration = IsDurationScaleable(duration);
        }

        public override IEnumerator OnTextReveal(Storyteller teller)
        {
            float realDuration = Duration;
            if (ScaleWithCharacterDuration)
                realDuration *= teller.CharacterDuration;
            while (teller.TimeBuffer < realDuration)
                yield return null;
            teller.TimeBuffer -= realDuration;
        }
    }
}