using System;
using System.Collections.Generic;
using System.IO;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Data.Playlist;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Data.Files;
using JANOARG.Shared.Utils.Animation;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public class CameraColourShifter : MonoBehaviour
    {
        public static CameraColourShifter sMain;

        private const float ReimportFadeDuration = 0.2f;
        private const float ReimportResumeDuration = 0.3f;

        public Camera TargetCamera;
        public float ShiftInterval = 4f;
        public float DedupThreshold = 0.18f;

        private readonly SongColourManager _Colours = new();
        private readonly List<Color> _Palette = new();
        private Camera _Camera;
        private ShifterState _State = ShifterState.Running;
        private Color _InitialColour = Color.black;
        private Color _Current = Color.black;
        private Color _FromColour = Color.black;
        private Color _TargetColour = Color.black;
        private float _Timer;
        private int _TargetIndex;
        private int _PaletteRevision = -1;
        private bool _RebuildOnBlack;
        private bool _QuickResume;

        public int ColourCount => _Colours.Colours.Count;

        public void Awake()
        {
            sMain = this;
            EnsureCamera();
            _Colours.DedupThreshold = DedupThreshold;
        }

        public void Update()
        {
            if (!EnsureCamera())
                return;

            if (_State == ShifterState.FadingToBlack)
            {
                float duration = Mathf.Max(ReimportFadeDuration, 0.01f);
                _Timer += Mathf.Min(Time.deltaTime, duration * 0.1f);
                float t = EvaluateReimportEase(_Timer / duration);
                _Current = Color.Lerp(_FromColour, Color.black, t);

                if (_Timer >= duration)
                {
                    _Current = Color.black;
                    _State = ShifterState.HoldingBlack;
                    if (_RebuildOnBlack)
                        RebuildAndResume();
                }

                ApplyColour(_Current);
                return;
            }

            if (_State == ShifterState.HoldingBlack)
            {
                ApplyColour(Color.black);
                return;
            }

            _Colours.Tick();
            SyncPalette();

            if (_Palette.Count == 0)
                return;

            float interval = _QuickResume ? ReimportResumeDuration : Mathf.Max(ShiftInterval, 0.01f);
            _Timer += Mathf.Min(Time.deltaTime, interval * 0.1f);
            while (_Timer >= interval)
            {
                _Timer -= interval;
                _QuickResume = false;
                AdvanceTarget();
                interval = Mathf.Max(ShiftInterval, 0.01f);
            }

            float progress = _QuickResume
                ? EvaluateReimportEase(_Timer / interval)
                : EvaluateEase(_Timer / interval);
            _Current = Color.Lerp(_FromColour, _TargetColour, progress);
            ApplyColour(_Current);
        }

        /// <summary>Stops applying colours so the launch flow can own the camera again.</summary>
        public void Deactivate() => enabled = false;

        public void BeginReimport() => FadeToBlack(false);

        /// <summary>
        /// Fades the camera background to black and holds it while the map scene is unloaded.
        /// </summary>
        public void FadeToBlackForLeave() => FadeToBlack(false);

        private void FadeToBlack(bool rebuildOnBlack)
        {
            if (EnsureCamera())
                _Current = _Camera.backgroundColor;

            _RebuildOnBlack = rebuildOnBlack;
            _QuickResume = false;
            _FromColour = _Current;
            _Timer = 0f;
            _State = ShifterState.FadingToBlack;
        }

        public void EndReimport()
        {
            _RebuildOnBlack = true;
            if (_State == ShifterState.HoldingBlack)
                RebuildAndResume();
        }

        public void MarkDirty()
        {
            _Colours.MarkDirty();
        }

        public void Rebuild()
        {
            if (_State != ShifterState.Running)
                return;

            _Colours.Rebuild();
            SyncPalette();
            _Timer = 0f;
        }

        private bool EnsureCamera()
        {
            if (_Camera != null)
                return true;

            if (TargetCamera != null)
                _Camera = TargetCamera;
            else if (TryGetComponent(out Camera attachedCamera))
                _Camera = attachedCamera;
            else if (CommonSys.sMain != null)
                _Camera = CommonSys.sMain.MainCamera;

            if (_Camera == null)
                return false;

            _InitialColour = _Camera.backgroundColor;
            _Current = _InitialColour;
            _FromColour = _InitialColour;
            _TargetColour = _InitialColour;
            return true;
        }

        private void SyncPalette()
        {
            if (_PaletteRevision == _Colours.Revision)
                return;

            _PaletteRevision = _Colours.Revision;

            if (_Colours.Colours.Count == 0)
                return;

            _Palette.Clear();
            _Palette.AddRange(_Colours.Colours);
            Shuffle(_Palette);

            _TargetIndex = 0;
            _FromColour = _Current;
            _TargetColour = _Palette.Count > 0 ? _Palette[0] : _FromColour;
            _Timer = 0f;
        }

        private void AdvanceTarget()
        {
            if (_Palette.Count == 0)
                return;

            _FromColour = _TargetColour;
            _TargetIndex++;

            if (_TargetIndex >= _Palette.Count)
            {
                Shuffle(_Palette);
                _TargetIndex = 0;
            }

            _TargetColour = _Palette[_TargetIndex];
        }

        private void RebuildAndResume()
        {
            _RebuildOnBlack = false;

            try
            {
                _Colours.Rebuild();
                SyncPalette();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CameraColourShifter] Palette rebuild failed: " + e);
            }
            finally
            {
                _FromColour = Color.black;
                _Current = Color.black;
                _TargetColour = _Palette.Count > 0 ? _Palette[0] : Color.black;
                _Timer = 0f;
                _QuickResume = _Palette.Count > 0;
                _State = ShifterState.Running;
                ApplyColour(Color.black);
            }
        }

        private void ApplyColour(Color colour)
        {
            _Camera.backgroundColor = colour;
            RenderSettings.fogColor = colour;
        }

        private static float EvaluateEase(float t)
        {
            return Ease.Get(Mathf.Clamp01(t), EaseFunction.Cubic, EaseMode.InOut);
        }

        private static float EvaluateReimportEase(float t)
        {
            return Ease.Get(Mathf.Clamp01(t), EaseFunction.Cubic, EaseMode.Out);
        }

        private static void Shuffle(List<Color> colours)
        {
            for (int i = colours.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                Color temp = colours[i];
                colours[i] = colours[j];
                colours[j] = temp;
            }
        }

        private enum ShifterState
        {
            Running,
            FadingToBlack,
            HoldingBlack
        }
    }

    internal sealed class SongColourManager
    {
        public float DedupThreshold = 0.18f;

        private readonly List<Color> _Colours = new();
        private string _LastPlaylistKey;
        private int _LastSongCount = -1;
        private bool _Dirty = true;

        public IReadOnlyList<Color> Colours => _Colours;

        public int Revision { get; private set; }

        public void Tick()
        {
            SongSelectScreen screen = SongSelectScreen.sMain;
            if (screen == null)
                return;

            int songCount = screen.PlayableSongByID != null ? screen.PlayableSongByID.Count : 0;
            if (songCount != _LastSongCount)
            {
                _LastSongCount = songCount;
                _Dirty = true;
            }
            if (_Colours.Count == 0 && songCount > 0)
                _Dirty = true;

            string key = screen.Playlist != null ? screen.Playlist.name : null;
            if (!_Dirty && key == _LastPlaylistKey)
                return;

            _LastPlaylistKey = key;
            _Dirty = false;
            Rebuild(screen);
        }

        public void MarkDirty()
        {
            _Dirty = true;
        }

        public void Rebuild()
        {
            SongSelectScreen screen = SongSelectScreen.sMain;
            if (screen == null)
                return;

            Rebuild(screen);
        }

        private void Rebuild(SongSelectScreen screen)
        {
            _LastPlaylistKey = screen.Playlist != null ? screen.Playlist.name : null;
            _LastSongCount = screen.PlayableSongByID != null ? screen.PlayableSongByID.Count : 0;
            _Dirty = false;

            _Colours.Clear();

            if (screen.PlayableSongByID == null)
                return;

            foreach (KeyValuePair<string, PlayableSong> kv in screen.PlayableSongByID)
            {
                try
                {
                    string songID = kv.Key;
                    PlayableSong song = kv.Value;

                    if (song == null)
                        continue;

                    if (song.Charts != null && song.Charts.Count > 0)
                    {
                        foreach (ExternalChartMeta chartMeta in song.Charts)
                            _Colours.Add(TryGetChartBackgroundColour(songID, song, chartMeta));
                    }
                    else
                    {
                        _Colours.Add(song.BackgroundColor);
                    }

                    _Colours.Add(SampleCover(song));
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[SongColourManager] Failed processing song: " + e);
                }
            }

            List<Color> deduplicated = Deduplicate(_Colours, DedupThreshold);
            _Colours.Clear();
            foreach (Color colour in deduplicated)
                _Colours.Add(TuneBrightness(colour));

            Revision++;
        }

        private static Color TryGetChartBackgroundColour(string songID, PlayableSong song, ExternalChartMeta chartMeta)
        {
            try
            {
                Chart chart = LoadChart(songID, chartMeta);
                if (chart != null)
                    return GetStoryboardedPalette(chart.Palette).BackgroundColor;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SongColourManager] Failed to read chart colour for '{songID}': {e}");
            }
            return song.BackgroundColor;
        }

        // Gameplay advances the palette each frame. For the menu palette, use the colour the
        // storyboard settles on rather than the base value it starts from.
        private static Palette GetStoryboardedPalette(Palette source)
        {
            float time = 0f;

            foreach (Timestamp timestamp in source.Storyboard.Timestamps)
            {
                switch (timestamp.ID)
                {
                    case TimestampIDs.BackgroundColor_R:
                    case TimestampIDs.BackgroundColor_G:
                    case TimestampIDs.BackgroundColor_B:
                        time = Mathf.Max(time, (float)timestamp.Offset + timestamp.Duration);
                        break;
                }
            }

            return (Palette)source.GetStoryboardableObject(time);
        }

        private static Chart LoadChart(string songID, ExternalChartMeta chartMeta)
        {
            if (chartMeta == null || string.IsNullOrEmpty(chartMeta.Target))
                return null;

            bool isExternal = SongSelectScreen.sMain != null
                && SongSelectScreen.sMain.PlaylistSongByID.TryGetValue(songID, out PlaylistSong playlistSong)
                && playlistSong.IsExternal;

            if (isExternal)
            {
                string externalDir = Path.Combine(ChartDirProvider.GetChartsDirectoryPath(), songID);
                string path = Path.Combine(externalDir, chartMeta.Target);
                if (!File.Exists(path) && !path.EndsWith(".jac", StringComparison.OrdinalIgnoreCase))
                    path += ".jac";
                if (!File.Exists(path))
                    return null;

                return JACDecoder.Decode(File.ReadAllText(path));
            }

            ExternalChart asset = Resources.Load<ExternalChart>("Songs/" + songID + "/" + chartMeta.Target);
            if (asset != null && asset.Data != null)
                return asset.Data;

            return null;
        }

        private static Color SampleCover(PlayableSong song)
        {
            if (song == null || song.Cover == null)
                return Color.gray;

            Texture2D texture = null;
            if (song.Cover.IconCover != null)
                texture = song.Cover.IconCover.texture;

            if (texture == null && song.Cover.Layers != null)
            {
                foreach (CoverLayer layer in song.Cover.Layers)
                {
                    if (layer != null && layer.Texture != null)
                    {
                        texture = layer.Texture;
                        break;
                    }
                }
            }

            if (texture != null)
                return AverageColour(texture);

            return song.Cover.BackgroundColor.a > 0 ? song.Cover.BackgroundColor : song.BackgroundColor;
        }

        private static Color AverageColour(Texture2D texture)
        {
            try
            {
                Color32[] pixels = texture.GetPixels32();
                long r = 0;
                long g = 0;
                long b = 0;
                long a = 0;
                long count = 0;
                foreach (Color32 p in pixels)
                {
                    if (p.a == 0)
                        continue;
                    r += p.r;
                    g += p.g;
                    b += p.b;
                    a += p.a;
                    count++;
                }
                if (count == 0)
                    return Color.gray;
                return new Color(
                    (r / (float)count) / 255f,
                    (g / (float)count) / 255f,
                    (b / (float)count) / 255f,
                    (a / (float)count) / 255f);
            }
            catch
            {
                return Color.gray;
            }
        }

        public static Color TuneBrightness(Color colour, float maxBrightness = 0.65f)
        {
            Color.RGBToHSV(colour, out float h, out float sat, out float val);
            if (val > maxBrightness)
                val = maxBrightness;
            if (sat < 0.2f)
                sat = 0.2f;
            return Color.HSVToRGB(h, sat, val);
        }

        public static List<Color> Deduplicate(List<Color> input, float threshold)
        {
            List<Color> result = new List<Color>();
            if (input == null)
                return result;

            foreach (Color c in input)
            {
                bool duplicate = false;
                foreach (Color r in result)
                {
                    if (ColourDistance(r, c) < threshold)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate)
                    result.Add(c);
            }
            return result;
        }

        private static float ColourDistance(Color a, Color b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return Mathf.Sqrt(dr * dr + dg * dg + db * db);
        }
    }
}
