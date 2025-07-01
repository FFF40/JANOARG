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
    public IEnumerator SetActorPosition(float FadeDuration)
    {
        yield return Ease.Animate(FadeDuration, (a) =>
        {
            float lerp = Ease.Get(1-a, EaseFunction.Cubic, EaseMode.Out);
            // ImageHolder.   
        });
    }
    

    
}
