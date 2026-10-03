using System.Collections;
using System.Collections.Generic;
using System.IO;
using JANOARG.Client.Data.Playlist;
using JANOARG.Client.Data.Storage;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Common
{
    public class SongInfoManager : MonoBehaviour
    {
        public static SongInfoManager sMain;

        //This Playlist will be the main/root playlist so we can use the PlayableSong's metachart and cover
        public Playlist MainPlaylist;
        public Dictionary<string, PlayableSong> SongDict;
        private bool _IsFinished;

        public void Awake()
        {
            sMain = this;
            StartCoroutine(BuildSongList());
        }
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

            _IsFinished = true;
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
            yield return new WaitUntil(() => sMain._IsFinished);
            if (song == null || song.Cover == null || song.Cover.Layers == null || song.Cover.Layers.Count == 0)
            {
                onDone?.Invoke(null);
                yield break;
            }

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
            yield return new WaitUntil(() => sMain._IsFinished);
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

        public string GetSongNameByID(string songID)
        {
            if (SongDict != null && SongDict.TryGetValue(songID, out var song))
            {
                return song.SongName;
            }
            return songID; // Fallback to ID if not found
        }

        public string GetDifficultyByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return "Simple";
                case 1:
                    return "Normal";
                case 2:
                    return "Complex";
                case 3:
                    return "Overdrive";
                default:
                    return "Special";

            }
        }

        public bool IsEntryFullStreak(ScoreStoreEntry entry)
        {
            if (entry.BadCount == 0 && entry.GoodCount > 0)
            {
                return true;
            }
            return false;
        }

        public bool IsEntryAllFlawless(ScoreStoreEntry entry)
        {
            if (entry.BadCount == 0 && entry.GoodCount == 0)
            {
                return true;
            }
            return false;
        }
    }
}