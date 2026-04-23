using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Client.Data.Playlist
{
    [CreateAssetMenu(fileName = "New External Playlist", menuName = "JANOARG/ExternalPlaylist", order = 200)]
    public class ExternalPlaylist : Playlist
    {
        public List<PlaylistSong> Songlist = new List<PlaylistSong>();
        
        public void ArrayToList()
        {
            Songlist = new List<PlaylistSong>(Songs);
        }

        public void ListToArray()
        {
            Songs = Songlist.ToArray();
        }

        public void AddSong(PlaylistSong song)
        {   
            ArrayToList();
            Songlist.Add(song);
            ListToArray();
        }

        public void RemoveSong(PlaylistSong song)
        {
            ArrayToList();
            Songlist.Remove(song);
            ListToArray();
        }

        public void ClearSongs()
        {
            ArrayToList();
            Songlist.Clear();
            ListToArray();
        }
    }

}
