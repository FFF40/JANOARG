using System;
using System.Collections.Generic;
using JANOARG.Shared.Scripts.Data.Story.Instructions;
using UnityEngine;

namespace JANOARG.Shared.Scripts.Data.Story
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