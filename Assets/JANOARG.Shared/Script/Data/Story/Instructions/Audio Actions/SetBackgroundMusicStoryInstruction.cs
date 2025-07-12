using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class SetBackgroundMusicStoryInstruction : StoryInstruction 
{
    public string musicName;

    [StoryTag("bgmusic")]
    public SetBackgroundMusicStoryInstruction (string name) 
    {
        musicName = name;
    }

    public override void OnMusicChange(Storyteller teller)
    {
        var musicToBePlayed = teller.AudioConstants.BackgroundMusic.Find(x => x.Name == musicName);
        var player = teller.BackgroundMusicPlayer; 
        if (musicToBePlayed != null)
        {
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