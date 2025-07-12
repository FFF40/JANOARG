using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Story Audio Constants", menuName = "JANOARG/Story Audio Constants")]
public class StoryAudioConstants : ScriptableObject
{
    public List<StoryBackgroundMusicInfo> BackgroundMusic;
    public List<StorySoundEffectsInfo> SoundEffects;
}

[Serializable]
public class StoryBackgroundMusicInfo
{
    public string Name;
    public AudioClip BackgroundMusic;
}

[Serializable]
public class StorySoundEffectsInfo
{
    public string Name;
    public AudioClip SFX;
}