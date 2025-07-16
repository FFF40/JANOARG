using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class DelayStoryInstruction : StoryInstruction 
{
    public float Duration;
    public bool ScaleWithCharacterDuration;

    [StoryTag("wait")]
    public DelayStoryInstruction (string duration) 
    {
        Duration = ParseDuration(duration);
        ScaleWithCharacterDuration = isDurationScaleable(duration);
    }

    public override IEnumerator OnTextReveal(Storyteller teller)
    {
        float realDuration = Duration;
        if (ScaleWithCharacterDuration) realDuration *= teller.CharacterDuration;
        while (teller.TimeBuffer < realDuration) yield return null;
        teller.TimeBuffer -= realDuration;
    }
}