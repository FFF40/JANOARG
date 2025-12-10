using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JANOARG.Client.Behaviors.Common;
using Unity.VisualScripting;
using System;
using JANOARG.Client.Data.Storage;
using JANOARG.Client.Behaviors.Song_Select;
using JANOARG.Shared.Data.ChartInfo;
using System.IO;
using JetBrains.Annotations;
using JANOARG.Client.Utils;

namespace JANOARG.Client.Behaviors.Panels.Profile
{
    public class RatingBreakdownEntry : MonoBehaviour
    {
        public TMP_Text Rating;
        public TMP_Text BestScore;
        public TMP_Text SongName;
        public TMP_Text SongArtist;
        public TMP_Text ChartConstant;

        public RawImage Icon;
        public RawImage BackgroundCover;

        public GameObject FullStreakIndicator;
        public GameObject AllFlawlessIndicator;

        private List<SongSelectItem> _SongSelectItems;
        private Color _Color;

        public SongSelectItem GetSong(string songID)
        {
            _SongSelectItems = SongSelectScreen.sMain.itemList;

            var item = _SongSelectItems.Find(x =>
                x.Song.ClipPath.Substring(0, x.Song.ClipPath.Length - 4) == songID
            );

            return item;
        }
        public PlayableSong GetSongInfo(string songID)
        {
            var item = GetSong(songID);
            return item.Song;
        }

        public ExternalChartMeta GetSongChart(PlayableSong song, string difficulty)
        {
            return song.Charts.Find(x => x.Target == difficulty);
        }

        //Get ICON image for the song
        public IEnumerator GetCoverImage(string songID) {
            var item = GetSong(songID);

            string path = Path.Combine(Path.GetDirectoryName(item.SongPath)!, item.Song.Cover.IconTarget);
            if (Path.HasExtension(path)) path = Path.ChangeExtension(path, "")[0..^1];
            ResourceRequest req = Resources.LoadAsync<Texture2D>(path);

            yield return new WaitUntil(() => req.isDone);

            if (req.asset)
            {
                var tex = (Texture2D)req.asset;
                Icon.texture = tex;
                Icon.color = Color.white;
            }
            else
            {
                Icon.color = item.Song.Cover.BackgroundColor;
            }
        }

        public IEnumerator SetCoverLayerColor(string songID)
        {
            var item = GetSong(songID);
    
            //Only solid color for now since cover is using Texture
            BackgroundCover.color = item.Song.Cover.BackgroundColor;

        //    foreach (CoverLayer layer in item.Song.Cover.Layers)
        //     {
        //         string path = Path.Combine(Path.GetDirectoryName(item.SongPath)!, layer.Target);
        //         if (Path.HasExtension(path)) path = Path.ChangeExtension(path, "")[0..^1];
        //         ResourceRequest request = Resources.LoadAsync<Texture2D>(path);

        //         yield return new WaitUntil(() => request.isDone);

        //         if (request.asset)
        //         {
        //             Debug.Log("Cover Layer Found: " + path);
        //             var texture = (Texture2D)request.asset;
        //             BackgroundCover.texture = texture;
                    
        //         }
        //         else
        //         {
        //             Debug.Log("Cover Layer Not Found: " + path);
        //             BackgroundCover.color = item.Song.Cover.BackgroundColor;
        //         }
                
        //     }

            yield return null;

        }

        //Get Cover Layer for the Best Score
        public IEnumerator GetCoverLayer(ScoreStoreEntry entry)
        {
            var item = GetSong(entry.SongID);

            foreach (CoverLayer layer in item.Song.Cover.Layers)
            {
                string path = Path.Combine(Path.GetDirectoryName(item.SongPath)!, layer.Target);
                if (Path.HasExtension(path)) path = Path.ChangeExtension(path, "")[0..^1];
                ResourceRequest request = Resources.LoadAsync<Texture2D>(path);

                yield return new WaitUntil(() => request.isDone);

                if (request.asset)
                {
                    var texture = (Texture2D)request.asset;
                    ScreenshotCanvas.sMain.SetBestSongCover(texture);
                }
                else
                {
                    Debug.Log("Cover Layer Not Found: " + path);
                    ScreenshotCanvas.sMain.SetBestSongCover(BackgroundCover.color);
                }
                
            }
            if (item.Song.Cover.Layers.Count == 0)
            {
                ScreenshotCanvas.sMain.SetBestSongCover(BackgroundCover.color);   
                Debug.Log("No Cover Layer Found"); 
            }
            
            yield return new WaitUntil(() => ScreenshotCanvas.sMain.IsCoverSet);
        }

        public void SetIndicator(ScoreStoreEntry entry)
        {
            if (entry.BadCount == 0 & entry.GoodCount == 0)
            {
                AllFlawlessIndicator.SetActive(true);
            }
            else if (entry.BadCount == 0)
            {
                FullStreakIndicator.SetActive(true);
            }
        }

        public IEnumerator SetEntry(ScoreStoreEntry entry)
        {
            if (entry.IsUnityNull())
            {
                yield return null;
            }

            PlayableSong songInfo = GetSongInfo(entry.SongID);
            ExternalChartMeta chartInfo = GetSongChart(songInfo, entry.ChartID);
            _Color = CommonSys.sMain.Constants.GetDifficultyColor(chartInfo.DifficultyIndex);

            Rating.text = FormatRating(entry.Rating);
            BestScore.text = Helper.PadScore(entry.Score.ToString()) + "<size=50%><b>ppm";
            SongName.text = songInfo.SongName;
            SongArtist.text = songInfo.SongArtist;
            ChartConstant.text = Helper.FormatDifficulty(chartInfo.DifficultyLevel);
            ChartConstant.color = _Color;

            StartCoroutine(GetCoverImage(entry.SongID));
            StartCoroutine(SetCoverLayerColor(entry.SongID));
            SetIndicator(entry);

            yield return null;
        }

        public string FormatRating(float rating)
        {
            string rating_Text = rating.ToString("F1");

            return rating_Text.Substring(0, rating_Text.Length - 2) +
                    "<size=50%><b>" +
                    rating_Text.Substring(rating_Text.Length - 2);
        }
    }
}
