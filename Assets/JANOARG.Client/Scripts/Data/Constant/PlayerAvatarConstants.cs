using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Data.Storage;
using JANOARG.Client.Utils;

namespace JANOARG.Client.Data.Constant
{
    [CreateAssetMenu(fileName = "Player Avatar Constants", menuName = "JANOARG/Player Avatar Constants")]
    public class PlayerAvatarConstants : ScriptableObject
    {
        public List<Avatar> Avatars;

        public void GetAllSongAvatars()
        {
            // Get scores
            Dictionary<string, ScoreStoreEntry> entries = StorageManager.sMain.Scores.entries;
            // If score >= 800000, then unlocked (add to list)
            foreach (var entry in entries)
            {
                var score = entry.Value;
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
                        Name = songID,
                        Image = avatarSprite,
                        IsUnlocked = score.Score >= Helper.PASSING_SCORE
                    };

                    Avatars.Add(songAvatar);
                }
            }
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
        public bool IsUnlocked;
    }

    
}