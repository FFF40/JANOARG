using System;
using System.Collections;
using System.Collections.Generic;
using JANOARG.Client.Data.Story;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Client.Data.Playlist
{
    [CreateAssetMenu(fileName = "New Playlist", menuName = "JANOARG/Playlist", order = 100)]
    public class Playlist : ScriptableObject
    {
    [Header("Metadata")]
    public string ID;
    public string MapName;
    public Color BackgroundColor = Color.black;

        [Header("Items")]
        [FormerlySerializedAs("Items")]
        public PlaylistSong[] Songs;
        public PlaylistReference[] Playlists;
        public PlaylistStory[] Stories;
        [Header("Background Music")]
        public AudioClip BackgroundMusicInit;
        public AudioClip BackgroundMusicLoop;
    }

    [Serializable]
    public class PlaylistSong
    {
        [JAPSPicker]
        public string ID;

        [NonSerialized]
        public bool IsExternal;
        [SerializeReference]
        public GameConditional[] RevealConditions;
        [SerializeReference]
        public GameConditional[] UnlockConditions;
    }

    [Serializable]
    public class PlaylistStory
    {
        public StoryScript ID;
        [SerializeReference]
        public GameConditional[] RevealConditions;
        [SerializeReference]
        public GameConditional[] UnlockConditions;
    }

    [Serializable]
    public class PlaylistStory
    {
        public StoryScript ID;
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

    public class JAPSPickerAttribute : PropertyAttribute { }
    public class PlaylistPickerAttribute : PropertyAttribute { }
    public class StoryPickerAttribute : PropertyAttribute { }
}
