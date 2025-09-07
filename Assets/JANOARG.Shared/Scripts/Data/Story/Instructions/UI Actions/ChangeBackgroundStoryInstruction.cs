using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using JANOARG.Shared.Data.ChartInfo;

using JANOARG.Client.Behaviors.Storyteller;

namespace JANOARG.Shared.Data.Story.Instructions
{
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
            // Background opacity will be set to 0 before changing the background
            yield return Ease.Animate(FadeDuration, (a) =>
            {
                float lerp = Ease.Get(1 - a, EaseFunction.Cubic, EaseMode.Out);
                teller.BackgroundImage.color = new Color(1f, 1f, 1f, lerp);
            });

            // Change Background
            if (teller.Constants.Backgrounds.Count == 0)
            {
                // Black screen if there are no backgrounds set in the constants
                teller.BackgroundImage.sprite = teller.Constants.Backgrounds[0].File;
            }
            else
            {
                var bg = teller.Constants.Backgrounds.Find(x => x.Alias == TargetBackground);

                // If the background with the specified alias is not found, use the first background as default
                if (bg == null)
                {
                    Debug.LogWarning($"Background with alias '{TargetBackground}' not found. Using default background.");
                    bg = teller.Constants.Backgrounds[0]; // Fallback to the first background
                }
                else
                {
                    teller.BackgroundImage.sprite = bg.File;
                }
            }

            // Background opacity will be set to 1(fully visible) before changing the background
            yield return Ease.Animate(FadeDuration, (a) =>
            {
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                teller.BackgroundImage.color = new Color(1f, 1f, 1f, lerp);
            });
        }

    }
}