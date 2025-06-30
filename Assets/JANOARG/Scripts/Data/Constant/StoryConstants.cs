
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Story Constants", menuName = "JANOARG/Story Constants")]
public class StoryConstants : ScriptableObject
{
    public List<ActorInfo> Actors;
    public List<BackgroundInfo> Backgrounds;
}

[Serializable]
public class ActorInfo
{
    public string Name;
    public string Alias;
    public string TextPrefix;
}

[Serializable]
public class BackgroundInfo
{
    public string Name;
    public string Alias;
    public Sprite File;
}