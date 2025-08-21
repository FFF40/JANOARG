using System.Collections;
using System.IO;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Data.Storage;
using JANOARG.Client.Utils;
using JANOARG.Shared.Data.ChartInfo;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Panels.Panel_Types
{
    public class ProfilePanel : MonoBehaviour
    {
        public Camera ScreenshotCamera;
        [Space]
        public TMP_Text PlayerName;
        public TMP_Text PlayerTitle;
        public TMP_Text LevelContent;
        public TMP_Text LevelProgress;
        public TMP_Text AbilityRatingContent;
        public TMP_Text AllFlawlessCount;
        public TMP_Text FullStreakCount;
        public TMP_Text ClearedCount;
        public TMP_Text UnlockedCount;

        public void Awake()
        {
            Storage Storage = CommonSys.main.Storage;

            PlayerName.text = Storage.Get("INFO:Name", Storage.Get("INFO:Name", "JANOARG"));
            PlayerTitle.text = Storage.Get("INFO:Title", "Perfectly Generic Player");

            // TODO: Leveling Stuff
            int level = CommonSys.main.Storage.Get("INFO:Level", 1);
            LevelContent.text = level.ToString();
            LevelProgress.text = Helper.FormatCurrency(CommonSys.main.Storage.Get("INFO:LevelProgress", 0L)) 
                                 + " / " + Helper.FormatCurrency(Helper.GetLevelGoal(level));

            AbilityRatingContent.text = ProfileBar.main.AbilityRating.ToString("F2");

            int[] trackStatusCount = TrackStatus("all");

            AllFlawlessCount.text = trackStatusCount[0].ToString();
            FullStreakCount.text = trackStatusCount[1].ToString();
            ClearedCount.text = trackStatusCount[2].ToString();
            UnlockedCount.text = trackStatusCount[3].ToString();
        }

        // Function that gets no of AF,FL,CLR and UNL
        // will return [AF,FL,CLR,UNL]
        public int[] TrackStatus(string diff)
        {
            int[] trackCount = new int[4];
            ScoreStore scores = new ScoreStore();
            scores.Load();

            foreach (var entry in scores.Entries)
            {
                string key = entry.Key;
                int slashIndex = key.LastIndexOf('/');
                string SongID = key.Substring(0, slashIndex);
                string ChartID = key.Substring(slashIndex + 1);

                var record = scores.Get(SongID, ChartID);

                if (record == null)
                {
                    Debug.LogWarning("Record of " + key + " is missing!");
                    continue;
                }

                if (record.ChartID == diff || diff == "all")
                {
                    int[] trackStat = CountStatus(record, diff);

                    for (int i = 0; i < trackCount.Length; i++)
                    {
                        trackCount[i] += trackStat[i];
                    }
                }
            }
            return trackCount;
        }

        public int[] CountStatus(ScoreStoreEntry record, string diff)
        {
            int AllFlawlessCount = 0;
            int FullStreakCount = 0;
            int ClearedCount = 0;
            int UnlockedCount = 0;

            ClearedCount++;
            UnlockedCount++;
            if (record.PerfectCount == record.MaxCombo)
            {
                AllFlawlessCount++;
            }
            if (record.BadCount == 0)
            {
                FullStreakCount++;              
            }


            return new int[] { AllFlawlessCount, FullStreakCount, ClearedCount, UnlockedCount };
        
        }

        public bool IsAnimating { get; private set; }

        /**
            <summary>
                Create a screenshot with given <c>width</c> and <c>height</c> in pixels.
            </summary>
        */
        public Texture2D Screenshot(int width, int height)
        {
            RenderTexture rTex = new (width, height, 16, RenderTextureFormat.ARGB32);
            rTex.Create();

            ScreenshotCamera.targetTexture = rTex;
            ScreenshotCamera.Render();

            Texture2D tex2D = new (width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = rTex;
            tex2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex2D.Apply();

            ScreenshotCamera.targetTexture = null;
            rTex.Release();

            return tex2D;
        }

        /**
            <summary>
                Create a screenshot of the rating breakdown and invoke sharing mechanisms.
            </summary>
        */
        public void ScreenshotRatingBreakdown()
        {
            if (!IsAnimating) StartCoroutine(ScreenshotRatingBreakdownRoutine());
        }

        /**
            <summary>
                Coroutine of <c>ScreenshotRatingBreakdown</c>.
            </summary>
        */
        IEnumerator ScreenshotRatingBreakdownRoutine()
        {
            IsAnimating = true;
            Texture2D image = Screenshot(3072, 1280);
            yield return Share(image);
            IsAnimating = false;
        }

        // TODO implement actual share logic
        public IEnumerator Share(Texture2D image)
        {
            var task = File.WriteAllBytesAsync(Application.persistentDataPath + "/screenshot.png", ImageConversion.EncodeToPNG(image));
            yield return new WaitUntil(() => task.IsCompleted);
        }
    }
}
