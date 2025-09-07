using System;
using System.Collections.Generic;
using JANOARG.Shared.Data.Story.Instructions;
using UnityEngine;

namespace JANOARG.Shared.Data.Story
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