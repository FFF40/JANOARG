using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Data.Storage;
using JANOARG.Client.Utils;

namespace JANOARG.Client.Data.Player
{
    [CreateAssetMenu(fileName = "Player Avatar", menuName = "JANOARG/Player/Avatars")]
    public class PlayerAvatars : ScriptableObject
    {
        public List<Avatar> Avatars;

        public void GetAllSongAvatars()
        {
            // Get scores
            Dictionary<string, ScoreStoreEntry> entries = StorageManager.sMain.Scores.entries;
            // If score >= 950000, then unlocked (add to list)
            foreach (var entry in entries)
            {
                var score = entry.Value;
                if (score.Score < Helper.GetScoreFromRank("S")) continue; 
                string songID = score.SongID;

                // Skip duplicate songs
                if (Avatars.Any(x => x.ID == songID))
                    continue;

                string avatarPath = $"Songs/{songID}/icon";
                Sprite avatarSprite = Resources.Load<Sprite>(avatarPath);

                if (avatarSprite != null)
                {
                    SongAvatar songAvatar = new SongAvatar
                    {
                        ID = songID,
                        Name = SongInfoManager.sMain.GetSongNameByID(songID),
                        Image = avatarSprite,
                    };

                    Avatars.Add(songAvatar);
                }
            }
            Debug.Log($"Collected {Avatars.Count} song avatars.");
        }
    }

    [Serializable]
    public class Avatar
    {
        public string ID;
        public string Name;
        public Sprite Image;
    }

    public class SongAvatar: Avatar
    {
        // TODO: Make some options like have GameConditionals
        public bool IsUnlocked; 
    }

    
}