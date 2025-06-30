using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[Serializable]
public class ActorSpriteHandler : MonoBehaviour
{
    public GameObject ImageHolder;
    [Space]
    public string CurrentActor;
    public string CurrentActorSprite;

    
    public void SetActor(string name)
    {
        CurrentActor = name;
    }
    public void SetActorSprite(string name = "normal")
    {
        CurrentActorSprite = name;
    }
    public void SetActorPosition(Vector2 posDelta)
    {

    }
    

    
}
