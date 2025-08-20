using System;
using System.Collections.Generic;
using JANOARG.Shared.Script.Data.Story.Instructions;
using UnityEngine;

namespace JANOARG.Shared.Script.Data.Story
{
    public class StoryScript : ScriptableObject
    {
        public List<StoryChunk> Chunks = new();
    }

    [Serializable]
    public class StoryChunk
    {
        [SerializeReference]
        public List<StoryInstruction> Instructions = new();
    }
}