using System;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Data.Constant.Story
{
    [CreateAssetMenu(fileName = "Story Constants", menuName = "JANOARG/Story Constants")]
    public class StoryConstants : ScriptableObject
    {
        public List<ActorInfo> Actors;
        public List<BackgroundInfo> Backgrounds;

        public ActorSprite PlaceholderActorSprite;
    }
}