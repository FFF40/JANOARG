using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Data.Story.Instructions;

[Serializable]
public class MoveActorSpriteStoryInstruction : ActorActionStoryInstruction
{
    public string From;
    public string Destination;
    public float Duration;

    private Vector2 _SpriteSize;

    [StoryTag("move")]
    public MoveActorSpriteStoryInstruction(string actor, string source, string destination, string duration)
    {
        Actors.Add(actor);

        From = ParseParameters(source);
        Destination = ParseParameters(destination);
        Duration = ParseDuration(duration);

    }

    string ParseParameters(string input)
    {
        return input switch
        {
            "r" or "right"                  => "r",
            "l" or "left"                   => "l",
            "c" or "center" or "middle"     => "c",
            "*" or "self"                   => "self",
            "ol" or "outleft"               => "ol",
            "or" or "outright"              => "or",
            _                               => throw new ArgumentException
                                                    ($"Input '{input}' is invalid. Expected a keyword like left, right, center, etc."),
        };
    }

    public override void OnTextBuild(Storyteller teller)
    {
        //Do nothing if there are no actors
        if (Actors.Count == 0) return;

        //Find the actor in the constants list and initialize its sprite handler
        var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
        InitSpriteHandler(actor.Alias, teller);
        ActorSpriteHandler targetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);
        _SpriteSize = targetActorSpriteHandler.GetSpriteSize();

        //Get the RectTransform of the image holder
        RectTransform rt = targetActorSpriteHandler.ImageHolder.GetComponentInChildren<Image>().rectTransform;

        Vector2 fromScreenPos = ResolvePosition(From, rt);
        Vector2 toScreenPos = ResolvePosition(Destination, rt);

        teller.StartCoroutine(MoveSprite(rt, fromScreenPos, toScreenPos, Duration));
    }

    // Resolves position keywords to actual screen positions
    private Vector2 ResolvePosition(string keyword, RectTransform rt)
    {
        RectTransform parentRT = rt.parent as RectTransform;
        float screenW = parentRT.rect.width;
        float screenH = parentRT.rect.height;

        // Ensure the sprite size is set
        rt.sizeDelta = _SpriteSize;

        return keyword switch
        {
            // Left edge: half sprite width from left border
            "l" or "left" => new Vector2(-screenW / 2 + _SpriteSize.x / 2, 0),
            
            // Right edge: half sprite width from right border
            "r" or "right" => new Vector2(screenW / 2 - _SpriteSize.x / 2, 0),

            "c" or "center" or "middle" => Vector2.zero,

            // Out of bounds left
            "ol" or "outleft" => new Vector2(-screenW / 2 - _SpriteSize.x, 0),
            // Out of bounds right
            "or" or "outright" => new Vector2(screenW / 2 + _SpriteSize.x, 0),

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
