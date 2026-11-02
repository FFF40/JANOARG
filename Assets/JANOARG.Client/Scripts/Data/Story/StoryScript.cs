using System;
using System.Collections.Generic;
using JANOARG.Client.Data.Story.Instructions;
using UnityEngine;

namespace JANOARG.Client.Data.Story
{
    public class StoryScript : ScriptableObject
    {
        public string ChapterCount;
        public string ChapterName;
        public string EpisodeCount;
        public string EpisodeName;
        public List<StoryChunk> Chunks = new();
        public List<StoryLocalization> Localizations = new();
    }

    [Serializable]
    public class StoryChunk
    {
        [SerializeReference] public List<StoryInstruction> Instructions = new();
    }

    public class StoryLocalization
    {
        
    }
}