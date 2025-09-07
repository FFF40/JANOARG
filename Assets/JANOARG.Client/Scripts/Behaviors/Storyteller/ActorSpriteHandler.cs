using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JANOARG.Shared.Data.ChartInfo;

[Serializable]
public class ActorSpriteHandler : MonoBehaviour
{
    public Canvas HandlerCanvas;
    public GameObject ImageHolder;
    [Space]
    public string CurrentActor;
    public string CurrentActorSpriteAlias;
    public float BounceHeight = 5f;
    public Vector2 SpriteSize;

    public void SetActor(string name, string spriteAlias)
    {
        CurrentActor = name;
        CurrentActorSpriteAlias = spriteAlias;
    }
    public void SetActorSprite(Sprite sprite, bool isInvisible)
    {
        // Get the Image compoent then change the sprite
        Image current = ImageHolder.GetComponentInChildren<Image>();

        if (isInvisible) current.color = new Color(0f, 0f, 0f, 0f);
        current.sprite = sprite;

        Vector2 referenceResolution = new Vector2(880, 600);
        float aspectRatio = (float)sprite.texture.width / sprite.texture.height;

        float targetHeight = Screen.height * 0.8f; // 80% of screen height
        float targetWidth = targetHeight * aspectRatio;

        SpriteSize = new Vector2(targetWidth, targetHeight);
        
        current.rectTransform.sizeDelta = SpriteSize;

        StartCoroutine(BounceSprite());
    }

    public IEnumerator BounceSprite()
    {
        RectTransform rectTransform = HandlerCanvas.GetComponent<RectTransform>();
        Vector2 originalPos = rectTransform.anchoredPosition;

        // Bounce effect (up and down)
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

    public Vector2 GetSpriteSize()
    {
        return SpriteSize;
    }

    #region  WIP
    public IEnumerator SetActorPosition(float fadeDuration)
    {
        yield return Ease.Animate(fadeDuration, (a) =>
        {
            float lerp = Ease.Get(1 - a, EaseFunction.Cubic, EaseMode.Out);
            // ImageHolder.   
        });
    }

    public void SetSpriteVisible()
    {
        Image current = ImageHolder.GetComponentInChildren<Image>();
        current.color = new Color(1f, 1f, 1f, 1f);
    }

    public IEnumerator SetSpriteVisible(float fadeDuration)
    {
        Image current = ImageHolder.GetComponentInChildren<Image>();

        yield return Ease.Animate(fadeDuration, (a) =>
        {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            current.color = new Color(1f, 1f, 1f, 1f * lerp);
        });

    }

    public IEnumerator MoveSprite(Vector2 startPos, Vector2 endPos, float fadeDuration)
    {
        RectTransform rectTransform = HandlerCanvas.GetComponent<RectTransform>();

        yield return Ease.Animate(fadeDuration, (a) =>
        {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            // current.color = new Color(1f, 1f, 1f, 1f*lerp);
        });
    }
    #endregion

}
