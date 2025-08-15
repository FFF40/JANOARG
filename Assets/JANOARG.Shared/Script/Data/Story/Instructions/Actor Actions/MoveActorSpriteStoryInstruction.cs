using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;

[Serializable]
public class MoveActorSpriteStoryInstruction : ActorActionStoryInstruction
{
    public string From;
    public string Destination;
    public float Duration;

    private Vector2 SpriteSize;

    [StoryTag("move")]
    public MoveActorSpriteStoryInstruction(string actor, string source, string destination, string duration)
    {
        Actors.Add(actor);

        Debug.Log($"MoveActorSpriteStoryInstruction: Moving actor {actor} from {source} to {destination} over {duration}s");
        From = ParseParameters(source);
        Destination = ParseParameters(destination);
        Duration = ParseDuration(duration);

        Debug.Log($"MoveActorSpriteStoryInstruction: Parsed From: {From}, Destination: {Destination}, Duration: {Duration}");
    }

    #region Parse Parameters
    string ParseParameters(string input)
    {
        return input switch
        {
            "r" or "right" => "r",
            "l" or "left" => "l",
            "c" or "center" or "middle" => "c",
            "*" or "self" => "self",
            "ol" or "outleft" => "ol",
            "or" or "outright" => "or",
            _ => throw new ArgumentException($"Input '{input}' is invalid. Expected a keyword like left, right, center, etc."),
        };
    }
    #endregion

    public override void OnTextBuild(Storyteller teller)
    {
        Debug.Log($"MoveActorSpriteStoryInstruction: Moving actor {Actors[0]} from {From} to {Destination} over {Duration}s");
        if (Actors.Count == 0) return;

        var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
        InitSpriteHandler(actor.Alias, teller);
        ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);
        SpriteSize = TargetActorSpriteHandler.GetSpriteSize();

        RectTransform rt = TargetActorSpriteHandler.ImageHolder.GetComponentInChildren<Image>().rectTransform;

        Vector2 fromScreenPos = ResolvePosition(From, rt);
        Vector2 toScreenPos = ResolvePosition(Destination, rt);

        teller.StartCoroutine(MoveSprite(rt, fromScreenPos, toScreenPos, Duration));
    }

    private Vector2 ResolvePosition(string keyword, RectTransform rt)
    {
        RectTransform parentRT = rt.parent as RectTransform;
        float screenW = parentRT.rect.width;
        float screenH = parentRT.rect.height;

        Debug.Log($"Resolving position for keyword '{keyword}' with screen size ({screenW}, {screenH})");

        rt.sizeDelta = SpriteSize;

        return keyword switch
        {
            // Left edge: half sprite width from left border
            "l" or "left" => new Vector2(-screenW / 2 + SpriteSize.x / 2, 0),
            // Right edge: half sprite width from right border
            "r" or "right" => new Vector2(screenW / 2 - SpriteSize.x / 2, 0),

            "c" or "center" or "middle" => Vector2.zero,

            // Out of bounds left
            "ol" or "outleft" => new Vector2(-screenW / 2 - SpriteSize.x, 0),
            // Out of bounds right
            "or" or "outright" => new Vector2(screenW / 2 + SpriteSize.x, 0),

            "self" => rt.anchoredPosition,
            _ => throw new ArgumentException($"Unknown position keyword: {keyword}")
        };
    }

    private IEnumerator MoveSprite(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        rt.anchoredPosition = from;

        yield return Ease.Animate(duration, (a) =>
        {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            rt.anchoredPosition = Vector2.Lerp(from, to, lerp);
        });

        rt.anchoredPosition = to;
    }
}
