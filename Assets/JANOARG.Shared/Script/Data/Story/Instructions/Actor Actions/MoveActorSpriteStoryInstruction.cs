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
    string From;
    Vector2 FromPos;
    string Destination;
    Vector2 DestinationPos;
    float Duration;

    [StoryTag("move")]
    public MoveActorSpriteStoryInstruction(string actor, string source, string destination, string duration)
    {
        Actors.Add(actor);

        (From, FromPos) = ParseParameters(source);
        (Destination, DestinationPos) = ParseParameters(destination);

        var match = Regex.Match(duration, @"^(?<number>\d+(?:\.\d+)?)(?<unit>s|x|)$");
        if (!match.Success) throw new ArgumentException("Duration value is invalid");
        Duration = Convert.ToSingle(match.Groups["number"].Value);
    }

    #region Parse Position
    bool TryParsePosition(string pos)
    {
        var match = Regex.Match(pos, @"<\s*(-?\d+)\s*,\s*(-?\d+)\s*>");
        if (match.Success) return true;
        return false;
    }

    Vector2 ParsePosition(string pos) {
        var match = Regex.Match(pos, @"<\s*(-?\d+)\s*,\s*(-?\d+)\s*>");
        int x = int.Parse(match.Groups[1].Value);
        int y = int.Parse(match.Groups[2].Value);
        return new Vector2(x, y);
    }

    (string,Vector2) ParseParameters(string input)
    {
        Vector2 rtPos = Vector2.zero;

        if (TryParsePosition(input))
        {
            rtPos = ParsePosition(input);
            return ("pos", rtPos);
        }

        return input switch
        {
            "r" or "right"                      => ("r", rtPos),
            "l" or "left"                       => ("l", rtPos),
            "c" or "center" or "middle"         => ("c", rtPos),
            "*" or "self"                       => ("self", rtPos),
            "ol" or "outleft"                   => ("ol", rtPos),  //Out of Bounds position on left of the visible screen
            "or" or "outright"                  => ("or", rtPos),    //Out of Bounds position on right of the visible screen
            _ => throw new ArgumentException($"Input '{input}' is invalid. Expected a direction or position like <x,y>."),
        };
    }
    #endregion
    public override void OnTextBuild(Storyteller teller)
    {
        if (Actors.Count == 0)
        {
            // Just do nothing
        }
        else if (Actors.Count == 1)
        {
            //Init Sprite Handler
            InitSpriteHandler(teller);
            var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
            ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);

            if (From == "pos") {
                
            }




        }
    }
}