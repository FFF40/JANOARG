using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class PlaySFXStoryInstruction : StoryInstruction
{
    public string sfxName;

    [StoryTag("sfx")]
    public PlaySFXStoryInstruction(string name)
    {
        sfxName = name;
    }

    public override IEnumerator OnSFXPlay(Storyteller teller)
    {
        var sfxToBePlayed = teller.AudioConstants.SoundEffects.Find(x => x.Name == sfxName);
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
