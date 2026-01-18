using System;
using System.Collections.Generic;
using JANOARG.Client.Data.Story.Instructions;
using UnityEngine;

namespace JANOARG.Client.Data.Story
{
    public class StoryScript : ScriptableObject
    {
        public List<StoryChunk> Chunks = new();
    }

    [Serializable]
    public class StoryChunk
    {
        [SerializeReference] public List<StoryInstruction> Instructions = new();
    }
}