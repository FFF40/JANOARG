using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using JANOARG.Client.Behaviors.Storyteller;
using static JANOARG.Shared.Data.Files.StoryDecoder;

namespace JANOARG.Shared.Data.Story.Instructions
{

    [Serializable]
    public class PlaySFXStoryInstruction : StoryInstruction
    {
        public string SFXName;

        [StoryTag("sfx")]
        public PlaySFXStoryInstruction(string name)
        {
            SFXName = name;
        }

        public override IEnumerator OnSFXPlay(Storyteller teller)
        {
            var sfxToBePlayed = teller.AudioConstants.SoundEffects.Find(x => x.Name == SFXName);
            var player = teller.SoundEffectsPlayer;
            if (sfxToBePlayed != null)
            {
                player.volume = 1f; // TODO: Make the volume scale with teller.MaxVolume;
                player.clip = sfxToBePlayed.SFX;
                player.loop = false;
                player.Play();

                yield return new WaitWhile(() => player.isPlaying);
            }
            else
            {
                player.Pause();
                player.clip = null;
            }
        }
    }
}