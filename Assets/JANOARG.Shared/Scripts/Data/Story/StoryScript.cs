
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
