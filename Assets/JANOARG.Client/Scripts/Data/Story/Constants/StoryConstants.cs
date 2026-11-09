using System;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Data.Constant
{
    [CreateAssetMenu(fileName = "Story Constants", menuName = "JANOARG/Story Constants")]
    public class StoryConstants : ScriptableObject
    {
        public StoryActorConstants MainStoryActorConstant;
        public StoryAudioConstants MainStoryAudioConstant;
        public StoryBackgroundConstants MainStoryBackgroundConstant;
    }

    
}