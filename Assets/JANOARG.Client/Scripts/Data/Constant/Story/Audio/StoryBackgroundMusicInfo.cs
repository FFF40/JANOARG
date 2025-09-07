using System;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Data.Constant.Story
{
    [Serializable]
    public class StoryBackgroundMusicInfo
    {
        public string Name;
        public string Artist = "";
        public AudioClip BackgroundMusic;
    }
}