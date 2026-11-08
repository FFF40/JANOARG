using System;
using System.Collections;
using System.Text.RegularExpressions;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public class JumpStoryInstruction : StoryInstruction
    {
        public string TargetSection;

        [StoryTag("Jump")]
        public JumpStoryInstruction(string targetSection)
        {
            if (string.IsNullOrWhiteSpace(targetSection))
            {
                throw new ArgumentNullException("Jump Instruction: TargetSection is null or empty.");
            }
            TargetSection = targetSection;
        }

        public override IEnumerator OnAfterTextReveal(Storyteller teller)
        {
            teller._PendingSectionJump = TargetSection;
            yield break;
        }
    }
}