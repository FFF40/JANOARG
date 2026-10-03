using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Data.Storage;
using JANOARG.Client.Utils;
using JANOARG.Client.Data.Playlist;

namespace JANOARG.Client.Data.Player
{
    [CreateAssetMenu(fileName = "Player Title", menuName = "JANOARG/Player/Titles")]
    public class PlayerTitles : ScriptableObject
    {
        public List<Title> Titles;
        public List<TitleRarity> TitleRarities;
        public void GetAllSongTitles()
        {
            // Get scores
            Dictionary<string, ScoreStoreEntry> entries = StorageManager.sMain.Scores.entries;
            // If entry is FULL STREAK or ALL FLAWLESS, then add to list
            foreach (var entry in entries)
            {
                var score = entry.Value;
                bool isAllFlawless = SongInfoManager.sMain.IsEntryAllFlawless(score);
                bool isFullStreak = SongInfoManager.sMain.IsEntryFullStreak(score); 

                string titlePrefix = "";
                string titleSuffix = "";

                // Skip Special charts for now
                if (SongInfoManager.sMain.GetDifficultyByIndex(score.ChartIndex) == "Special") continue;
                if (isAllFlawless || isFullStreak)
                {
                    string songID = score.SongID;

                    // Skip duplicate songs
                    if (Titles.Any(x => x.ID == songID))
                        continue;

                    titlePrefix = isAllFlawless ? "[ALL FLAWLESS] " : "[FULL STREAK] ";
                    int rarityIndex = score.ChartIndex + (isAllFlawless ? 2 : 1);
                    rarityIndex = Mathf.Clamp(rarityIndex, 0, TitleRarities.Count - 1);

                    TitleRarity titleRarity = TitleRarities[rarityIndex];
                    titleSuffix = " - " + SongInfoManager.sMain.GetDifficultyByIndex(score.ChartIndex);

                    SongTitle songTitle = new SongTitle
                    {
                        ID = songID,
                        Name = titlePrefix + SongInfoManager.sMain.GetSongNameByID(songID) + titleSuffix,
                        Rarity = titleRarity
                    };

                    Titles.Add(songTitle);
                    
                } continue; 
                
            }
            Debug.Log($"Collected {Titles.Count} song avatars.");
        }
    }

    [Serializable]
    public class Title
    {
        public string ID;
        public string Name;
        public TitleRarity Rarity;

        // TODO: Refactor/ Add a OptionInputListItem that can display this but not save this a option 
        // [SerializeReference]
        // public GameConditional[] RevealConditions;
        [SerializeReference]
        public GameConditional[] UnlockConditions;
    }

    public class SongTitle: Title
    {
        // TODO: Make some options like have GameConditionals
        public bool IsUnlocked; 
    }

    
}