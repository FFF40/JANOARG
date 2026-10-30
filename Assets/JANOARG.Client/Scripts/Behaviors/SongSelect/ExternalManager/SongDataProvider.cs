using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Data.Files;
using UnityEngine;
using UnityEngine.Networking;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public class SongDataProvider : MonoBehaviour
    {
        public static SongDataProvider sMain;

        public void Awake() => sMain = this;

        public void LoadSong(string dir, Action<PlayableSong> onComplete)
        {
            StartCoroutine(LoadSongRoutine(dir, onComplete));
        }

        public void LoadAllSongs(Action<List<PlayableSong>> onComplete)
        {
            StartCoroutine(LoadAllSongsRoutine(onComplete));
        }

        public IEnumerator LoadSongRoutine(string dir, Action<PlayableSong> onComplete)
        {
            PlayableSong song = null;
            try
            {
                string japsPath = Directory.GetFiles(dir, "*.japs", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (string.IsNullOrEmpty(japsPath))
                {
                    Debug.LogWarning($"No .japs file in {dir}");
                    onComplete?.Invoke(null);
                    yield break;
                }
                song = JAPSDecoder.Decode(File.ReadAllText(japsPath));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to decode {dir}: {e}");
                onComplete?.Invoke(null);
                yield break;
            }

            // Load audio clip from the directory
            string audioPath = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                  || f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)
                                  || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(audioPath))
            {
                using (var req = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.UNKNOWN))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                        song.Clip = DownloadHandlerAudioClip.GetContent(req);
                }
            }

            onComplete?.Invoke(song);
        }

        IEnumerator LoadAllSongsRoutine(Action<List<PlayableSong>> onComplete)
        {
            var songs = new List<PlayableSong>();
            var dirs = ChartScanner.sMain != null ? new List<string>(ChartScanner.sMain.JAPSDirs.Values) : new List<string>();
            foreach (string dir in dirs)
            {
                PlayableSong loaded = null;
                yield return LoadSongRoutine(dir, s => loaded = s);
                if (loaded != null) songs.Add(loaded);
            }
            onComplete?.Invoke(songs);
        }
    }
}
