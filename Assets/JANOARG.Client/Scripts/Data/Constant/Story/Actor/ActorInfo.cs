using System;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Data.Constant.Story
{
    [Serializable]
    public class ActorInfo
    {
        public string Name;
        public string Alias;
        public string TextPrefix;
        public List<ActorSprite> ActorSprites;
    }
}