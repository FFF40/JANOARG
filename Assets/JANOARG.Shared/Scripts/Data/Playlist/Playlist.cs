using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Playlist", menuName = "JANOARG/Playlist", order = 100)]
public class Playlist : ScriptableObject
{
    public string MapName;
    [FormerlySerializedAs("Items")]
    public PlaylistSong[] Songs;
    public PlaylistReference[] Playlists;
}

[Serializable]
public class PlaylistSong
{
    public string ID;
    [SerializeReference]
    public GameConditional[] RevealConditions;
    [SerializeReference]
    public GameConditional[] UnlockConditions;
}

[Serializable]
public class PlaylistReference
{
    public Playlist Playlist;
    [SerializeReference]
    public GameConditional[] RevealConditions;
    [SerializeReference]
    public GameConditional[] UnlockConditions;
}

