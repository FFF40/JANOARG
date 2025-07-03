using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ActorSpriteHandler : MonoBehaviour
{
    public Canvas HandlerCanvas;
    public GameObject ImageHolder;
    [Space]
    public string CurrentActor;
    public string CurrentActorSpriteAlias;
    public float BounceHeigth = 5f;


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

        StartCoroutine(BounceSprite());
    }

    public IEnumerator BounceSprite()
    {
        RectTransform rectTransform = HandlerCanvas.GetComponent<RectTransform>();
        Vector2 originalPos = rectTransform.anchoredPosition;
        yield return Ease.Animate(0.25f, (a) =>
        {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            rectTransform.anchoredPosition = originalPos + new Vector2(0f, 10 * lerp);
        });

        yield return Ease.Animate(0.25f, (a) =>
        {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            rectTransform.anchoredPosition = originalPos + new Vector2(0f, 10 * (1 - lerp));
        });
        rectTransform.anchoredPosition = originalPos;
    }
    public IEnumerator SetActorPosition(float FadeDuration)
    {
        yield return Ease.Animate(FadeDuration, (a) =>
        {
            float lerp = Ease.Get(1 - a, EaseFunction.Cubic, EaseMode.Out);
            // ImageHolder.   
        });
    }
    

    
}
