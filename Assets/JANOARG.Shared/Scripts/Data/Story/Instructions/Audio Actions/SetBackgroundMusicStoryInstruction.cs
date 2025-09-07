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
    public class SetBackgroundMusicStoryInstruction : StoryInstruction
    {
        public string MusicName;

        [StoryTag("bgmusic")]
        public SetBackgroundMusicStoryInstruction(string name)
        {
            MusicName = name;
        }

        public override void OnMusicChange(Storyteller teller)
        {
            var musicToBePlayed = teller.AudioConstants.BackgroundMusic.Find(x => x.Name == MusicName);
            var player = teller.BackgroundMusicPlayer;
            if (musicToBePlayed != null)
            {
                player.volume = 1f;  //TODO: Add player's volume preference 
                player.clip = musicToBePlayed.BackgroundMusic;
                player.loop = true;
                player.Play();
            }
            else
            {
                player.Pause();
                player.clip = null;
            }
        }
    }
}