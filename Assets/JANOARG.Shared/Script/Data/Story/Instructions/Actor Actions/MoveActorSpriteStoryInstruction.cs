using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

[Serializable]
public class MoveActorSpriteStoryInstruction : ActorActionStoryInstruction
{
    string Mode;
    string From;
    Vector2 FromPos;
    string Destination;
    Vector2 DestinationPos;

    [StoryTag("move")]
    public MoveActorSpriteStoryInstruction(string actor, string mode, string source, string destination)
    {
        Actors.Add(actor);

        if (mode == "rel" || mode == "relative")                Mode = "rel";
        else if (mode == "abs" || mode == "absolute")           Mode = "abs";
        else { throw new ArgumentException($"Mode({mode}) is invalid"); }

        if      (source == "r" || source == "right")            From = "r";
        else if (source == "l" || source == "left")             From = "l";
        else if (source == "c" || source == "center")           From = "c";
        else if (source == "*" )                                From = "self";
        else if (source == "ol" || source == "outleft")         From = "ol"; //Out of Bounds position on left of the visible screen
        else if (source == "or" || source == "outright")        From = "or"; //Out of Bounds position on right of the visible screen
        else if (TryParsePosition(source))                      {From = "pos";   FromPos = ParsePosition(source);}
        else { throw new ArgumentException($"source({source}) is invalid"); }

        if      (destination == "r" || destination == "right")              Destination = "r";
        else if (destination == "l" || destination == "left")               Destination = "l";
        else if (destination == "c" || destination == "center")             Destination = "c";
        else if (source == "*" )                                From = "self";
        else if (destination == "ol" || destination == "outleft") Destination = "ol"; //Out of Bounds position on left of the visible screen
        else if (destination == "or" || destination == "outright") Destination = "or"; //Out of Bounds position on right of the visible screen
        else if (TryParsePosition(destination)) { Destination = "pos"; DestinationPos = ParsePosition(destination); }
        else { throw new ArgumentException($"destination({destination}) is invalid"); }
    }

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

    public override void OnTextBuild(Storyteller teller)
    {
        if (Actors.Count == 0)
        {
            // Just do nothing
        }
        else if (Actors.Count == 1)
        {
            //Init
            InitSpriteHandler(teller);
            var actor = teller.Constants.Actors.Find(x => x.Alias == Actors[0]);
            ActorSpriteHandler TargetActorSpriteHandler = teller.Actors.Find(x => x.CurrentActor == actor.Alias);

            //Parse



        }
    }
}