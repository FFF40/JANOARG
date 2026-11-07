using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;
using JANOARG.Shared.Utils.Animation;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public class SetBackgroundStoryInstruction : StoryInstruction
    {
        public string Id;
        public float Duration = 1.0f;

        // In-dialog
        [StoryTag("SetBackground")]
        public SetBackgroundStoryInstruction(string id, string duration = "1.0")
        {
            Id = id;
            Match match = Regex.Match(duration, @"(?<number>\d+(?:\.\d+)?)");
            if (!match.Success) throw new ArgumentException("Duration value is invalid");

            Duration = Convert.ToSingle(duration);
        }

        // Scene
        public override IEnumerator OnSceneSet(Storyteller teller)
        {
            // If no background set to iamge color black
            yield return Ease.Animate(Duration, (a) =>
            {
                float lerp = Ease.Get(1 - a, EaseFunction.Cubic, EaseMode.Out);
                if (teller.BackgroundImage.sprite == null){
                    teller.BackgroundImage.color = new Color(0f, 0f, 0f, lerp);
                } else {
                    teller.BackgroundImage.color = new Color(1f, 1f, 1f, lerp);
                }
                teller.ActorStage.CanvasGroup.alpha = 1 * lerp;
            });

            // Change Background
            if (teller.Constants.MainStoryBackgroundConstant == null)
            {
                // Black screen if there are no backgrounds set in the constants
                teller.BackgroundImage.sprite = null;
                Debug.Log($"{Id} on IEnumerator OnSceneSet is NULL");
            }
            else
            {
                Sprite bg = teller.Constants.MainStoryBackgroundConstant.GetBackground(Id);

                // If the background with the specified alias is not found, use the first background as default
                if (bg == null)
                {
                    teller.BackgroundImage.sprite = null;
                    Debug.LogWarning($"Background with alias '{Id}' not found. Using default background.");
                }
                else
                {
                    teller.BackgroundImage.sprite = bg;
                }
            }

            // Background opacity will be set to 1(fully visible) before changing the background
            yield return Ease.Animate(Duration, (a) =>
            {
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                teller.BackgroundImage.color = new Color(1f, 1f, 1f, lerp);
                teller.ActorStage.CanvasGroup.alpha = 1 * lerp;
            });
        }
    }
}