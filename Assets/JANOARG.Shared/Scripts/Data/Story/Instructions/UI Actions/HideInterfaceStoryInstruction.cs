using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class HideInterfaceStoryInstruction : StoryInstruction
{
    public bool IsHideInterface;
    public float FadeDuration = 1f;

    [StoryTag("hideInterface")]
    public HideInterfaceStoryInstruction(string isHide, string duration)
    {
        IsHideInterface = ParseBoolean(isHide);
        FadeDuration = ParseDuration(duration);
    }

    public override IEnumerator OnInterfaceChange(Storyteller teller)
    {
        if (IsHideInterface == true)
        {
            // Hides in the interface for the duration 
            yield return Ease.Animate(FadeDuration, (a) =>
            {
                float lerp = Ease.Get(1 - a, EaseFunction.Cubic, EaseMode.Out);
                teller.InterfaceGroup.alpha = lerp;
            });
        }
        else
        {
            // Shows in the interface for the duration
            yield return Ease.Animate(FadeDuration, (a) =>
            {
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                teller.InterfaceGroup.alpha = lerp;
            });

        }
        
    }

}