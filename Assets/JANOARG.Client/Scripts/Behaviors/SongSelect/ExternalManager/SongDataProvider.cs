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

        /// <summary>Returns the first .japs file directly inside <paramref name="dir"/>, or null.</summary>
        public static string FindJapsFile(string dir)
        {
            try
            {
                return Directory.EnumerateFiles(dir, "*.japs", SearchOption.TopDirectoryOnly).FirstOrDefault();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to enumerate {dir}: {e}");
                return null;
            }
        }

        // Resources in a .japs are relative to the chart folder (same as JAPSImporter).
        static string ResolveResourcePath(string dir, string target)
        {
            if (string.IsNullOrEmpty(target)) return null;
            target = target.Replace('\\', '/');
            while (target.StartsWith("./")) target = target.Substring(2);
            target = target.TrimStart('/');
            return Path.Combine(dir, target);
        }

        // Only for charts whose Clip entry is missing or stale.
        static string FindAudioFallback(string dir) =>
            Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                  || f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)
                                  || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase));

        public IEnumerator LoadSongRoutine(string dir, Action<PlayableSong> onComplete)
        {
            string japsPath = FindJapsFile(dir);
            if (string.IsNullOrEmpty(japsPath))
            {
                Debug.LogWarning($"No .japs file in {dir}");
                onComplete?.Invoke(null);
                yield break;
            }
            yield return LoadSongRoutine(dir, japsPath, onComplete);
        }

        /// <summary>Loads a song when the caller has already located its .japs file.</summary>
        public IEnumerator LoadSongRoutine(string dir, string japsPath, Action<PlayableSong> onComplete)
        {
            PlayableSong song = null;
            try
            {
                song = JAPSDecoder.Decode(File.ReadAllText(japsPath));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to decode {dir}: {e}");
                onComplete?.Invoke(null);
                yield break;
            }

            // Load audio clip: take the path straight from the decoded Clip entry, no directory scan.
            string audioPath = ResolveResourcePath(dir, song.ClipPath);
            if (audioPath == null || !File.Exists(audioPath))
            {
                string fallback = FindAudioFallback(dir);
                if (fallback != null)
                    Debug.LogWarning($"[SongDataProvider] Clip '{song.ClipPath}' not found in {Path.GetFileName(dir)}, using {Path.GetFileName(fallback)}");
                audioPath = fallback;
            }
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
                    string layerPath = ResolveResourcePath(dir, layer.Target);
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
            var entries = ChartScanner.sMain != null ? new List<ChartEntry>(ChartScanner.sMain.JAPSDirs.Values) : new List<ChartEntry>();
            foreach (var entry in entries)
            {
                PlayableSong loaded = null;
                yield return LoadSongRoutine(entry.Dir, entry.JapsPath, s => loaded = s);
                if (loaded != null) songs.Add(loaded);
            }
            onComplete?.Invoke(songs);
        }
    }
}
