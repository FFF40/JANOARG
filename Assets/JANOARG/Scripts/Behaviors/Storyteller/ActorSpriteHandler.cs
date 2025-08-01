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
    public void SetActorSprite(Sprite sprite, bool isInvisible)
    {
        Image Current = ImageHolder.GetComponentInChildren<Image>();
        if (isInvisible) Current.color = new Color(0f, 0f, 0f, 0f);

        // // Get sprite size
        // Vector2 spriteSize = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y);
        // Debug.Log(spriteSize);

        // // Then scale the sprite based on the screen res
        // // 

        //Change the sprite
        Current.sprite = sprite;

        //I need to find a way to make the image scale based on safe area stuff
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

    public void SetSpriteVisible()
    {
        Image Current = ImageHolder.GetComponentInChildren<Image>();
        Current.color = new Color(1f, 1f, 1f, 1f);
    }

    public IEnumerator SetSpriteVisible(float FadeDuration)
    {
        Image Current = ImageHolder.GetComponentInChildren<Image>();

        yield return Ease.Animate(FadeDuration, (a) =>
        {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            Current.color = new Color(1f, 1f, 1f, 1f*lerp);
        });
        
    }

    public IEnumerator MoveSprite(Vector2 startPos,Vector2 endPos, float FadeDuration)
    {
        RectTransform rectTransform = HandlerCanvas.GetComponent<RectTransform>();
        
        yield return Ease.Animate(FadeDuration, (a) =>
        {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            // Current.color = new Color(1f, 1f, 1f, 1f*lerp);
        });
    }
}
