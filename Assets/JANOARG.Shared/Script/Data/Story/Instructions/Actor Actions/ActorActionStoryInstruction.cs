using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ActorActionStoryInstruction : StoryInstruction 
{
    public List<string> Actors = new();
    public ActorSprite sprite = new();
    public string TargetSprite;

    
   
}