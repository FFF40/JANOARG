using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.Panels;
using JANOARG.Client.Data.Storage;
using JANOARG.Client.UI.Modal;
using JANOARG.Client.Utils;
using JANOARG.Shared.Data.ChartInfo;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ANOARG.Client.Behaviors.Panels
{
    public class ProfilePanel : MonoBehaviour
    {
        public TMP_Text PlayerName;

        public TMP_Text PlayerTitle;
        public TMP_Text LevelContent;
        public TMP_Text LevelProgress;
        public TMP_Text AbilityRatingContent;
        public TMP_Text AllFlawlessCount;
        public TMP_Text FullStreakCount;
        public TMP_Text ClearedCount;
        public TMP_Text UnlockedCount;

        public GameObject RatingBreakdownModalBody;
        public Sprite CameraIcon;
        public Panel Panel;

        public bool isAnimating { get; private set; }

        public void Awake()
        {
            Storage storage = CommonSys.sMain.Storage;

            PlayerName.text = storage.Get("INFO:Name", "JANOARG");
            PlayerTitle.text = storage.Get("INFO:Title", "Perfectly Generic Player");

            // TODO: Leveling Stuff
            int level = CommonSys.sMain.Storage.Get("INFO:Level", 1);
            LevelContent.text = level.ToString();

            LevelProgress.text = Helper.FormatCurrency(CommonSys.sMain.Storage.Get("INFO:LevelProgress", 0L)) +
                                 " / " +
                                 Helper.FormatCurrency(Helper.GetLevelGoal(level));

            AbilityRatingContent.text = ProfileBar.sMain.AbilityRating.ToString("F2");

            int[] trackStatusCount = TrackStatus("all");

            AllFlawlessCount.text = trackStatusCount[0]
                .ToString();

            FullStreakCount.text = trackStatusCount[1]
                .ToString();

            ClearedCount.text = trackStatusCount[2]
                .ToString();

            UnlockedCount.text = trackStatusCount[3]
                .ToString();
        }

        // Function that gets numbers of AF,FL,CLR and UNL for given player.
        // will return [AF,FL,CLR,UNL]
        public int[] TrackStatus(string difficulty)
        {
            var trackCount = new int[4];
            ScoreStore scores = new();
            scores.Load();

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

                if (record.ChartID == difficulty || difficulty == "all")
                {
                    int[] trackStat = CountStatus(record, difficulty);

                    for (var i = 0; i < trackCount.Length; i++) trackCount[i] += trackStat[i];
                }
            }

            return trackCount;
        }

        public int[] CountStatus(ScoreStoreEntry record, string diff)
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
            ModalManager.sInstance.Spawn(
                "Rating Breakdown",
                RatingBreakdownModalBody,
                new ModalAction[] {
                    new () {
                        Name = "Close",
                        Icon = ProfileBar.sMain.ArrowLeftIcon,
                    }
                }
                // Sharing is not yet implement
                // new ModalAction[] {
                //     new () {
                //         Name = "Share",
                //         Icon = CameraIcon,
                //         // function
                //     }
                // }
            );

            Panel.Close();
        
        }

    }
}