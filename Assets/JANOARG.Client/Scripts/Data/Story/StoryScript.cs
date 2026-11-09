using System;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Story.Instructions;
using UnityEngine;

namespace JANOARG.Client.Data.Story
{
    public class StoryScript : ScriptableObject
    {
        // TODO: Metadata support
        // public string ChapterCount;
        // public string ChapterName;
        // public string EpisodeCount;
        // public string EpisodeName;
        // public string SceneWriter;

        // TODO: Localization support
        // public List<StoryLocalization> Localizations = new();

        public List<StoryChunk> Chunks = new();
        
        [SerializeField]
        public Dictionary<string, int> Sections = new();
        public bool TryGetLabel(string name, out int index)
        {
            return Sections.TryGetValue(name, out index);
        }

    }

    [Serializable]
    public class StoryChunk
    {
        [SerializeReference] public List<StoryInstruction> Instructions = new();
        public bool isSilent => Instructions.Count > 0 && Instructions.All(i => i.isSilent);
        public List<StoryCondition> Conditions = new();

        public bool hasChoice => Instructions.Any(i => i is ChoiceStoryInstruction);

        public bool ShouldPlay(Storyteller t) =>
            Conditions.All(c => t.Evaluate(c.Expression) != c.Negate);
    }

    [Serializable]
    public class StoryCondition 
    { 
        public string Expression; 
        public bool Negate; 
    }

    [Serializable]
    public class ChoiceOption 
    { 
        public string Text; 
        public string Target; 
        public string Condition; 
    }

    public class StoryLocalization
    {
        public string Translator;
    }
}