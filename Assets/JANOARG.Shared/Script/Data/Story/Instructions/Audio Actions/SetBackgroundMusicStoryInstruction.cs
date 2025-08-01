using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SetBackgroundMusicStoryInstruction : StoryInstruction
{
    public string musicName;

    [StoryTag("bgmusic")]
    public SetBackgroundMusicStoryInstruction(string name)
    {
        musicName = name;
    }

    //TODO: Add player's volume preference 
    public override void OnMusicChange(Storyteller teller)
    {
        var musicToBePlayed = teller.AudioConstants.BackgroundMusic.Find(x => x.Name == musicName);
        var player = teller.BackgroundMusicPlayer;
        if (musicToBePlayed != null)
        {
            player.volume = 1f;
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

//TODO: Make it fade also this StoryInstruction is not yet implemented
[Serializable]
public class FadeBackgroundMusicStoryInstruction : StoryInstruction 
{
    public string musicName;

    [StoryTag("bgmusicfade")]
    public FadeBackgroundMusicStoryInstruction(string name)
    {
        musicName = name;
    }

    public override IEnumerator OnMusicPlay(Storyteller teller)
    {
        var musicToBePlayed = teller.AudioConstants.BackgroundMusic.Find(x => x.Name == musicName);
        var player = teller.BackgroundMusicPlayer;
        if (musicToBePlayed != null)
        {
            player.volume = 0f;
            player.clip = musicToBePlayed.BackgroundMusic;
            player.loop = true;
            player.Play();
            
            yield return Ease.Animate(3f, (a) =>
            {
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                player.volume = lerp * teller.MaxVolume;
            });
        }
        else
        {
            yield return Ease.Animate(3f, (a) =>
            {
                float lerp = Ease.Get(1-a, EaseFunction.Cubic, EaseMode.Out);
                player.volume = lerp * teller.MaxVolume;
            });

            player.Pause();
            player.clip = null;
        }
    }
}