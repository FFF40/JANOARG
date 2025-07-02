using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ActorSpriteHandler : MonoBehaviour
{
    public GameObject ImageHolder;
    [Space]
    public string CurrentActor;
    public string CurrentActorSpriteAlias;


    public void SetActor(string name, string spriteAlias)
    {
        CurrentActor = name;
        CurrentActorSpriteAlias = spriteAlias;
    }
    public void SetActorSprite(Sprite sprite)
    {
        Image Current = ImageHolder.GetComponentInChildren<Image>();
        Current.sprite = sprite;
        Current.SetNativeSize();
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
