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
                    RuntimeExternalManager.sExternalTextures.Add(tex);
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

        /// <summary>
        /// Resolves a song's audio file from its decoded Clip entry, falling back to a directory scan
        /// only if that entry is missing or stale. Returns null if no audio file can be found.
        /// </summary>
        public static string ResolveClipPath(string dir, PlayableSong song)
        {
            string audioPath = ResolveResourcePath(dir, song.ClipPath);
            if (audioPath != null && File.Exists(audioPath)) return audioPath;

            if (!Directory.Exists(dir)) return null;
            string fallback = FindAudioFallback(dir);
            if (fallback != null)
                Debug.LogWarning($"[SongDataProvider] Clip '{song.ClipPath}' not found in {Path.GetFileName(dir)}, using {Path.GetFileName(fallback)}");
            return fallback;
        }

        /// <summary>
        /// Loads an audio file. <paramref name="stream"/> = true opens it as a stream (cheap, used for song select
        /// previews); false decodes it fully into memory (what gameplay needs). Calls back with null on failure.
        /// </summary>
        public static IEnumerator LoadClipRoutine(string audioPath, bool stream, Action<AudioClip> onComplete)
        {
            using (var req = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.UNKNOWN))
            {
                if (stream && req.downloadHandler is DownloadHandlerAudioClip handler)
                    handler.streamAudio = true;

                yield return req.SendWebRequest();

                AudioClip clip = null;
                if (req.result == UnityWebRequest.Result.Success)
                    clip = DownloadHandlerAudioClip.GetContent(req);
                onComplete?.Invoke(clip);
            }
        }

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

            // Streamed for song select: a full decode blocks the main thread for ~0.8s per song.
            // PlayerScreen swaps in a fully decoded clip when the song is actually played.
            string audioPath = ResolveClipPath(dir, song);
            if (!string.IsNullOrEmpty(audioPath))
            {
                yield return LoadClipRoutine(audioPath, true, clip => song.Clip = clip);
                if (song.Clip != null) RuntimeExternalManager.sExternalClips.Add(song.Clip);
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
