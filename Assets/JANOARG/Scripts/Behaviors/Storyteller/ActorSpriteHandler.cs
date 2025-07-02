using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[Serializable]
public class ActorSpriteHandler : MonoBehaviour
{
    public GameObject ImageHolder;
    public Image Sprite;
    [Space]
    public string CurrentActor;
    public string CurrentActorSprite;

    
    public void SetActor(string name)
    {
        CurrentActor = name;
    }
    public void SetActorSprite(string name = "normal")
    {
        Image Current = ImageHolder.GetComponent<Image>();
    
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
