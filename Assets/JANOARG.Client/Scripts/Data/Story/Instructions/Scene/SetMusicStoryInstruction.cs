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
    public class SetMusicStoryInstruction : StoryInstruction
    {
        public string Id;
        public float FadeDuration = 2.0f;

        // In-dialog
        [StoryTag("SetMusic")]
        public SetMusicStoryInstruction(string id, string duration = "2.0")
        {
            Id = id;
            Match match = Regex.Match(duration, @"(?<number>\d+(?:\.\d+)?)");
            if (!match.Success) throw new ArgumentException("Fade Duration value is invalid");

            FadeDuration = Convert.ToSingle(duration);
        }

        // Scene
        public override IEnumerator OnSceneSet(Storyteller teller)
        {
            if (teller.BackgroundMusicPlayer.clip != null)
            {
                yield return Ease.Animate(FadeDuration, (a) =>
                {
                    float lerp = Ease.Get(1 - a, EaseFunction.Cubic, EaseMode.In);
                    teller.BackgroundMusicPlayer.volume = lerp;
                });
            }
            teller.BackgroundMusicPlayer.enabled = false;

            if (teller.Constants.MainStoryAudioConstant == null)
            {
                Debug.Log($"MainStoryAudioConstant is NULL");
            }
            else
            {
                AudioClip clip = teller.Constants.MainStoryAudioConstant.GetMusic(Id);

                if (clip == null)
                {
                    teller.BackgroundMusicPlayer.clip = null;
                    Debug.LogWarning($"Music with alias '{Id}' not found.");
                }
                else
                {
                    teller.BackgroundMusicPlayer.clip = clip;
                }
            }

            teller.BackgroundMusicPlayer.enabled = true;
            yield return Ease.Animate(FadeDuration, (a) =>
            {
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                teller.BackgroundMusicPlayer.volume = lerp;
            });
        }
    }
}