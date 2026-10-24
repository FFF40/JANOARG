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
    public class PlaySoundEffectStoryInstruction : StoryInstruction
    {
        public string Id;

        // In-dialog
        [StoryTag("PlaySound")]
        public PlaySoundEffectStoryInstruction(string id)
        {
            Id = id;
        }

        // Scene
        public override IEnumerator OnSceneSet(Storyteller teller)
        {

            if (teller.Constants.MainStoryAudioConstant == null)
            {
                Debug.Log($"MainStoryAudioConstant is NULL");
            }
            else
            {
                AudioClip clip = teller.Constants.MainStoryAudioConstant.GetSound(Id);

                if (clip == null)
                {
                    teller.SoundEffectsPlayer.enabled = false;
                    teller.SoundEffectsPlayer.clip = null;
                    Debug.LogWarning($"Music with alias '{Id}' not found.");
                }
                else
                {
                    teller.SoundEffectsPlayer.enabled = false;
                    teller.SoundEffectsPlayer.clip = clip;
                    teller.SoundEffectsPlayer.enabled = true;
                }
            }
            Debug.Log("Play Sound");
            yield return null;
        }
    }
}