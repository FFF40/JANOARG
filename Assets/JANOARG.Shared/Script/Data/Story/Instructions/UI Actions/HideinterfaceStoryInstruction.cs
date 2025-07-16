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
    public bool isShow;
    public float FadeDuration = 1f;

    [StoryTag("hideInterface")]
    public HideInterfaceStoryInstruction(string isHide, string duration)
    {
        isShow = ParseBoolean(isHide);
        FadeDuration = ParseDuration(duration);
    }

    public override IEnumerator OnInterfaceChange(Storyteller teller)
    {
        Debug.Log(isShow);
        if (isShow == true)
        {
            Debug.Log("opacity to 0");
            //Fade Out
            yield return Ease.Animate(FadeDuration, (a) =>
            {
                float lerp = Ease.Get(1 - a, EaseFunction.Cubic, EaseMode.Out);
                teller.InterfaceGroup.alpha = lerp;
            });
        }
        else
        {
            Debug.Log("opacity to 1");
            //Fade Out
            yield return Ease.Animate(FadeDuration, (a) =>
            {
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                teller.InterfaceGroup.alpha = lerp;
            });

        }
        
    }

}