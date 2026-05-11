using System.Collections.Generic;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.Panels.Profile;
using JANOARG.Client.Data.Playlist;
using JANOARG.Client.Data.Storage;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Playables;
using System.Threading.Tasks;
using JANOARG.Client.Data.Constant;

namespace JANOARG.Client.UI
{
    public class RatingBreakdownModalBody : MonoBehaviour
    {
        public ScrollRect ScrollRect;
        public Camera ScreenshotCamera;

        public bool IsAnimating = false;
        
        public List<RatingBreakdownEntry> RatingBreakdownEntries;
        public List<ScoreStoreEntry> ScoreStoreEntries;

        //This Playlist will be the main/root playlist so we can use the PlayableSong's metachart and cover
        public Playlist MainPlaylist;
        public Dictionary<string, PlayableSong> SongDict;

        public IEnumerator BuildSongList()
        {
            SongDict = new Dictionary<string, PlayableSong>();

            if (MainPlaylist == null)
            {
                Debug.LogWarning("MainPlaylist is null.");
                yield break;
            }

            HashSet<Playlist> visited = new HashSet<Playlist>();

            yield return StartCoroutine(
                CollectSongsRecursive(MainPlaylist, SongDict, visited)
            );
        }

        public IEnumerator CollectSongsRecursive(
            Playlist playlist,
            Dictionary<string, PlayableSong> dict,
            HashSet<Playlist> visited)
        {
            if (playlist == null || visited.Contains(playlist))
                yield break;

            visited.Add(playlist);

            // 1. Load songs
            if (playlist.Songs != null)
            {
                foreach (var song in playlist.Songs)
                {
                    if (song == null) continue;

                    string path = $"Songs/{song.ID}/{song.ID}";
                    ResourceRequest req = Resources.LoadAsync<ExternalPlayableSong>(path);

                    yield return req;

                    if (req.asset == null)
                    {
                        Debug.LogWarning("Couldn't load Playable Song at " + path);
                        continue;
                    }

                    PlayableSong playable = ((ExternalPlayableSong)req.asset).Data;

                    if (!dict.ContainsKey(song.ID))
                    {
                        dict.Add(song.ID, playable);
                    }
                }
            }

            // 2. Traverse sub-playlists
            if (playlist.Playlists != null)
            {
                foreach (var sub in playlist.Playlists)
                {
                    if (sub?.Playlist == null) continue;

                    yield return StartCoroutine(
                        CollectSongsRecursive(sub.Playlist, dict, visited)
                    );
                }
            }
        }

        public IEnumerator GetCoverImage(PlayableSong song, string id, System.Action<Texture2D> onDone)
        {
            string imagePath = song.Cover.Layers[0].Target;
            string path = $"Songs/{id}/{imagePath}";

            if (Path.HasExtension(path))
                path = Path.ChangeExtension(path, "").TrimEnd('.');

            ResourceRequest req = Resources.LoadAsync<Texture2D>(path);
            yield return req;

            if (req.asset == null)
            {
                Debug.LogWarning("Couldn't load texture at " + path);
                onDone?.Invoke(null);
                yield break;
            }

            onDone?.Invoke((Texture2D)req.asset);
        }
        public IEnumerator GetIconImage(string id, System.Action<Texture2D> onDone)
        {
            string path = $"Songs/{id}/icon";

            ResourceRequest req = Resources.LoadAsync<Texture2D>(path);
            yield return req;

            if (req.asset == null)
            {
                Debug.LogWarning("Couldn't load texture at " + path);
                onDone?.Invoke(null);
                yield break;
            }

            onDone?.Invoke((Texture2D)req.asset);
        }


        private IEnumerator Start()
        {
            ScrollRect.verticalNormalizedPosition = 1f;

            ScoreStoreEntries = StorageManager.sMain.Scores.GetBestEntries();
            if (ScoreStoreEntries == null || RatingBreakdownEntries == null)
            {
                Debug.LogError("ScoreStoreEntries or RatingBreakdownEntries is null.");
                yield return null;
            }

            if (MainPlaylist == null)
            {
                Debug.LogError("MainPlaylist is null. Set it on RatingBreakdownModalBody.");
                yield return null;
            }

            int count = Mathf.Min(ScoreStoreEntries.Count, RatingBreakdownEntries.Count);

            yield return StartCoroutine(BuildSongList());
         
            Dictionary<string, PlayableSong> songLookup = new Dictionary<string, PlayableSong>();

            for (int i = 0; i < count; i++)
            {
                if (RatingBreakdownEntries[i] == null)
                    continue;

                if (ScoreStoreEntries[i] == null)
                    continue;

                var scoreEntry = ScoreStoreEntries[i];
                var displayEntry = RatingBreakdownEntries[i];

                displayEntry.SetData(scoreEntry);

                if (SongDict != null && SongDict.TryGetValue(scoreEntry.SongID, out var song))
                {
                    displayEntry.SongName.text = Truncate(song.SongName,30);
                    displayEntry.SongArtist.text = Truncate(song.SongArtist,30);
                    displayEntry.ChartConstant.text = song.Charts.Find(x => x.Target == scoreEntry.ChartID).DifficultyLevel.ToString();
                    displayEntry.ChartConstant.color = CommonSys.sMain.Constants.GetDifficultyColor(scoreEntry.ChartIndex);
                    
                    Texture2D iconTex = null;

                    yield return StartCoroutine(
                        GetIconImage(scoreEntry.SongID, (tex) =>
                        {
                            iconTex = tex;
                        })
                    );

                    if (iconTex != null)
                    {
                        displayEntry.Icon.texture = iconTex;
                    } 

                    Texture2D coverTex = null;

                    yield return StartCoroutine(
                        GetCoverImage(song, scoreEntry.SongID, (tex) =>
                        {
                            coverTex = tex;
                        })
                    );

                    if (coverTex != null)
                    {
                        displayEntry.BackgroundCover.texture = coverTex;
                        displayEntry.BackgroundCover.color = Color.white;
                    } else
                    {
                        Debug.LogWarning($"Cover not found: {scoreEntry.SongID}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Song not found: {scoreEntry.SongID}");
                }
            }
        }
        
        string Truncate(string text, int maxLength)
        {
            return text.Length > maxLength
                ? text.Substring(0, maxLength) + "..."
                : text;
        }

        public Texture2D Screenshot(int width, int height)
        {
            RenderTexture rTex = new(width, height, 16, RenderTextureFormat.ARGB32);
            rTex.Create();

            ScreenshotCamera.targetTexture = rTex;
            ScreenshotCamera.Render();

            Texture2D tex2D = new(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = rTex;
            tex2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex2D.Apply();

            ScreenshotCamera.targetTexture = null;
            rTex.Release();

            return tex2D;
        }

        public void ScreenshotRatingBreakdown()
        {
            if (!IsAnimating) StartCoroutine(ScreenshotRatingBreakdownAnim());
        }

        public IEnumerator ScreenshotRatingBreakdownAnim()
        {
            IsAnimating = true;
            Texture2D image = Screenshot(3072, 1280);

            yield return Share(image);

            IsAnimating = false;
        }

        public IEnumerator Share(Texture2D image)
        {
            Task task = File.WriteAllBytesAsync(
                Application.persistentDataPath + "/screenshot.png",
                image.EncodeToPNG());

            yield return new WaitUntil(() => task.IsCompleted);
        }
    }
}