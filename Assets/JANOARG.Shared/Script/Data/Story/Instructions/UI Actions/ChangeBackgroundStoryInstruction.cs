using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class ChangeBackgroundInstruction : StoryInstruction
{
    public float FadeDuration = 1f;
    public string TargetBackground = "Blank";

    [StoryTag("bgswitch")]
     public ChangeBackgroundInstruction(string alias)
    {
        TargetBackground = alias;  
    }

    public override IEnumerator OnBackgroundChange(Storyteller teller)
    {
        //Fade Out
        yield return Ease.Animate(FadeDuration, (a) =>
        {
            float lerp = Ease.Get(1-a, EaseFunction.Cubic, EaseMode.Out);
            teller.BackgroundImage.color = new Color(1f, 1f, 1f, lerp);   
        });

        if (teller.Constants.Backgrounds.Count == 0)
        {
            teller.BackgroundImage.sprite = teller.Constants.Backgrounds[0].File;   //black screen         
        }
        else
        {
            var bg = teller.Constants.Backgrounds.Find(x => x.Alias == TargetBackground);
            teller.BackgroundImage.sprite = bg.File; 
        }
        
        //Fade In
        yield return Ease.Animate(FadeDuration, (a) =>
        {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            teller.BackgroundImage.color = new Color(1f, 1f, 1f, lerp);   
        });
    }

}