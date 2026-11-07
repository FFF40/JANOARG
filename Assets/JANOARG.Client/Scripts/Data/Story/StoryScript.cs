using System;
using System.Collections.Generic;
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
        public List<StoryChunk> Chunks = new();
        
        // TODO: Localization support
        // public List<StoryLocalization> Localizations = new();
    }

    [Serializable]
    public class StoryChunk
    {
        [SerializeReference] public List<StoryInstruction> Instructions = new();
    }

    public class StoryLocalization
    {
        public string Translator;
    }
}