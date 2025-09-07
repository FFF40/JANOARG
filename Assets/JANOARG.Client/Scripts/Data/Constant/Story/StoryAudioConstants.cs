using System;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Data.Constant.Story
{
    [CreateAssetMenu(fileName = "Story Audio Constants", menuName = "JANOARG/Story Audio Constants")]
    public class StoryAudioConstants : ScriptableObject
    {
        public List<StoryBackgroundMusicInfo> BackgroundMusic;
        public List<StorySoundEffectsInfo> SoundEffects;
    }
}