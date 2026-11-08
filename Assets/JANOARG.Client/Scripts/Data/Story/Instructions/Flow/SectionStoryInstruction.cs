using System;
using System.Collections;
using System.Text.RegularExpressions;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public class SectionStoryInstruction : StoryInstruction
    {
        public string Name;
        public int Index;

        
        public SectionStoryInstruction(string name, int index)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("Section Instruction: Name is null or empty.");
            }
            Name = name;
            Index = index;
        }
    }
}