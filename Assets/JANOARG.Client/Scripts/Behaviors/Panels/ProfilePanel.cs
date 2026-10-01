using System.Collections.Generic;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.Panels;
using JANOARG.Client.Data.Storage;
using JANOARG.Client.UI;
using JANOARG.Client.UI.Modal;
using JANOARG.Client.Utils;
using JANOARG.Shared.Data.ChartInfo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Panels
{
    public class ProfilePanel : MonoBehaviour
    {
        public TMP_Text PlayerName;
        public TMP_Text PlayerTitle;
        public Image PlayerAvatar;
        public TMP_Text LevelContent;
        public TMP_Text LevelProgress;
        public TMP_Text AbilityRatingContent;
        public TMP_Text TrackStatusCategory;
        public TMP_Text AllFlawlessCount;
        public TMP_Text FullStreakCount;
        public TMP_Text ClearedCount;
        public TMP_Text UnlockedCount;
        private int _CurrentDifficulty = 5;
        public GameObject RatingBreakdownModalBody;
        public Sprite CameraIcon;
        public Panel Panel;

        public void Awake()
        {
            Storage storage = CommonSys.sMain.Storage;

            PlayerName.text = storage.Get("INFO:Name", "JANOARG");
            PlayerTitle.text = storage.Get("INFO:PlayerTitle", "Perfectly Generic Player");
            string playerIcon = CommonSys.sMain.Storage.Get("INFO:PlayerIcon", "none");

            PlayerAvatar.color = playerIcon == "none" ? Color.black : Color.white;

            PlayerAvatar.sprite = playerIcon switch
            {
                "none" => null,
                _ => Sprite.Create(
                    Resources.Load<Texture2D>($"Songs/{playerIcon}/icon"),
                    new Rect(
                        0,
                        0,
                        Resources.Load<Texture2D>($"Songs/{playerIcon}/icon").width,
                        Resources.Load<Texture2D>($"Songs/{playerIcon}/icon").height
                    ),
                    new Vector2(0.5f, 0.5f)
                )
            };
            
            // TODO: Leveling Stuff
            int level = CommonSys.sMain.Storage.Get("INFO:Level", 1);
            LevelContent.text = level.ToString();

            LevelProgress.text = Helper.FormatCurrency(CommonSys.sMain.Storage.Get("INFO:LevelProgress", 0L)) +
                                 " / " +
                                 Helper.FormatCurrency(Helper.GetLevelGoal(level));

            AbilityRatingContent.text = ProfileBar.sMain.AbilityRating.ToString("F2");

            DisplayTrackStatus(_CurrentDifficulty);
        }

        public void UpdateTrackStatus()
        {
            _CurrentDifficulty++;
            if (_CurrentDifficulty > 5) _CurrentDifficulty = 0;
            DisplayTrackStatus(_CurrentDifficulty);
        }

        public void DisplayTrackStatus(int currentDifficulty)
        {
            int[] trackStatus = TrackStatus(currentDifficulty);
            switch (currentDifficulty)
            {
                case 0:
                    TrackStatusCategory.text = "SIMPLE";
                    break;
                case 1:
                    TrackStatusCategory.text = "NORMAL";
                    break;
                case 2:
                    TrackStatusCategory.text = "COMPLEX";
                    break;
                case 3:
                    TrackStatusCategory.text = "OVERDRIVE";
                    break;
                case 4:
                    TrackStatusCategory.text = "SPECIAL";
                    break;
                case 5:
                    TrackStatusCategory.text = "ALL";
                    break;
            }
            AllFlawlessCount.text = trackStatus[0].ToString();
            FullStreakCount.text = trackStatus[1].ToString();
            ClearedCount.text = trackStatus[2].ToString();
            UnlockedCount.text = trackStatus[3].ToString();
        }

        // Function that gets numbers of AF,FL,CLR and UNL for given player.
        // will return [AF,FL,CLR,UNL]
        // TODO: Actually implement to count all unlocked songs, not just cleared ones. (Currently, UNL = CLR)
        public int[] TrackStatus(int difficulty)
        {
            var trackCount = new int[4];
            ScoreStore scores = new();
            scores.Load();

            // TODO: Refactor this later to use root playlist then compare with the scores, instead of looping all scores.
            // Loop all scores 
            foreach (KeyValuePair<string, ScoreStoreEntry> entry in scores.entries)
            {
                string key = entry.Key;
                int slashIndex = key.LastIndexOf('/');

                string songID = key.Substring(0, slashIndex);
                string chartID = key.Substring(slashIndex + 1);

                ScoreStoreEntry record = scores.Get(songID, chartID);

                if (record == null)
                {
                    Debug.LogWarning("Record of " + key + " is missing!");

                    continue;
                }

                // Check if the record's chart index matches the specified difficulty
                switch (difficulty)
                {
                    case 0: // Simple
                        if (record.ChartIndex != 0) continue;
                        IncrementTrackStatus(trackCount, record);
                        break;
                    case 1: // Normal
                        if (record.ChartIndex != 1) continue;
                        IncrementTrackStatus(trackCount, record);
                        break;
                    case 2: // Complex
                        if (record.ChartIndex != 2) continue;
                        IncrementTrackStatus(trackCount, record);
                        break;
                    case 3: // Overdrive
                        if (record.ChartIndex != 3) continue;
                        IncrementTrackStatus(trackCount, record);
                        break;
                    case 4: // Special
                        if (record.ChartIndex >= 0 && record.ChartIndex <= 3) continue;
                        IncrementTrackStatus(trackCount, record);
                        break;
                    case 5: // All
                        IncrementTrackStatus(trackCount, record);
                        break;
                }
            }

            return trackCount;
        }

        public int[] IncrementTrackStatus(int[] trackCount, ScoreStoreEntry record)
        {
            int[] trackStat = CountStatus(record);
            for (var i = 0; i < trackCount.Length; i++) trackCount[i] += trackStat[i];
            return trackCount;
        }
        
        public int[] CountStatus(ScoreStoreEntry record)
        {
            var allFlawlessCount = 0;
            var fullStreakCount = 0;
            var clearedCount = 0;
            var unlockedCount = 0;

            if (record.PerfectCount == record.MaxCombo)
            {
                allFlawlessCount++;
                fullStreakCount++;
            }
            else if (record.BadCount == 0)
            {
                fullStreakCount++;
            }

            clearedCount++;
            unlockedCount++;

            return new[] { allFlawlessCount, fullStreakCount, clearedCount, unlockedCount };
        }

        public void OpenRatingBreakdownModal()
        {
            RatingBreakdownModalBody breakdown = null;

            Modal modal = ModalManager.sInstance.Spawn(
                "Rating Breakdown",
                RatingBreakdownModalBody,
                new ModalAction[] {
                    new () {
                        Name = "Close",
                        Icon = ProfileBar.sMain.ArrowLeftIcon,
                    }
                },
                new ModalAction[] {
                    new () {
                        Name = "Share",
                        Icon = CameraIcon,
                        Action = () => breakdown.ScreenshotRatingBreakdown(),
                        ClosesModal = false,
                    }
                }
            );

            breakdown = modal.BodyHolder.GetComponentInChildren<RatingBreakdownModalBody>();

            Panel.Close();
        }
    }
}