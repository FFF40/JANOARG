using System;
using System.Collections;
using System.Text.RegularExpressions;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public class DelayStoryInstruction : StoryInstruction
    {
        public float Duration;
        public bool  ScaleWithCharacterDuration;

        [StoryTag("wait")]
        public DelayStoryInstruction(string duration)
        {
            Match match = Regex.Match(duration, @"^(?<number>\d+(?:\.\d+)?)(?<unit>s|x|)$");

            if (!match.Success) throw new ArgumentException("Duration value is invalid");

            Duration = Convert.ToSingle(match.Groups["number"].Value);
            ScaleWithCharacterDuration = match.Groups["unit"].Value != "s";
        }

        public override IEnumerator OnTextReveal(Storyteller teller)
        {
            float realDuration = Duration;
            if (ScaleWithCharacterDuration) realDuration *= teller.CharacterDuration;

            while (teller.TimeBuffer < realDuration) yield return null;

            teller.TimeBuffer -= realDuration;
        }
    }
}