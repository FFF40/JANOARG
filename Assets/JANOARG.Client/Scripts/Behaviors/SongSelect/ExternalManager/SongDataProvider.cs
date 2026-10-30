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

        static Texture2D LoadTexture(string path)
        {
            try
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(File.ReadAllBytes(path)))
                {
                    RuntimeExternalManager.ExternalTextures.Add(tex);
                    return tex;
                }
            }
            catch (Exception e) { Debug.LogWarning($"Failed to load texture {path}: {e}"); }
            return null;
        }

        static Sprite LoadSprite(string path)
        {
            var tex = LoadTexture(path);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

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
                    {
                        song.Clip = DownloadHandlerAudioClip.GetContent(req);
                        if (song.Clip != null) RuntimeExternalManager.ExternalClips.Add(song.Clip);
                    }
                }
            }

            // Load cover/icon images
            string iconPath = Path.Combine(dir, song.Cover?.IconTarget ?? "icon.png");
            if (File.Exists(iconPath))
            {
                song.Cover.IconCover = LoadSprite(iconPath);
                if (song.Cover.IconCover != null)
                    Debug.Log($"[SongDataProvider] Loaded cover for {Path.GetFileName(dir)}");
                else
                    Debug.LogWarning($"[SongDataProvider] Failed to load cover image {iconPath}");
            }
            else
            {
                Debug.LogWarning($"[SongDataProvider] Cover file not found: {iconPath}");
            }

            if (song.Cover?.Layers != null)
            {
                foreach (var layer in song.Cover.Layers)
                {
                    if (string.IsNullOrEmpty(layer.Target)) continue;
                    string target = layer.Target.Replace('\\', '/');
                    while (target.StartsWith("./")) target = target.Substring(2);
                    target = target.TrimStart('/');
                    string layerPath = Path.Combine(dir, target);
                    if (File.Exists(layerPath))
                    {
                        var tex = LoadTexture(layerPath);
                        if (tex != null) layer.Texture = tex;
                    }
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
