using System.Collections;
using System.Collections.Generic;
using System.Text;
using JANOARG.Shared.Data.ChartInfo;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Utils
{
    /// <summary>
    ///     Warms dynamic TMP font assets ahead of time. Dynamic font assets load their source
    ///     font face and rasterize glyphs on first use, which can cost over 100 ms for the CJK
    ///     fallback fonts. Doing that during Song Select / chart loading keeps it out of gameplay.
    /// </summary>
    public static class FontPrewarmer
    {
        // UI characters that are not guaranteed to appear in song metadata.
        private const string BaselineCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
            " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~×✓!?…•Δ";

        // Keep each TryAddCharacters call small enough to yield between them.
        private const int CodePointsPerChunk = 128;

        /// <summary>
        ///     Prewarms every font with the characters used by an entire song catalog.
        /// </summary>
        public static IEnumerator PrewarmSongCatalog(IEnumerable<PlayableSong> songs)
        {
            HashSet<int> codePoints = new();
            AddText(codePoints, BaselineCharacters);

            if (songs != null)
            {
                foreach (PlayableSong song in songs)
                {
                    if (song == null) continue;

                    AddText(codePoints, song.SongName);
                    AddText(codePoints, song.AltSongName);
                    AddText(codePoints, song.SongArtist);
                    AddText(codePoints, song.AltSongArtist);
                    AddText(codePoints, song.Location);
                    AddText(codePoints, song.Genre);

                    if (song.Cover != null)
                    {
                        AddText(codePoints, song.Cover.ArtistName);
                        AddText(codePoints, song.Cover.AltArtistName);
                    }

                    if (song.Charts == null) continue;

                    foreach (ExternalChartMeta chart in song.Charts)
                    {
                        if (chart == null) continue;

                        AddText(codePoints, chart.DifficultyName);
                        AddText(codePoints, chart.DifficultyLevel);
                        AddText(codePoints, chart.CharterName);
                        AddText(codePoints, chart.AltCharterName);
                    }
                }
            }

            yield return PrewarmCodePoints(codePoints);
        }

        /// <summary>
        ///     Prewarms every font with the characters used by one song/chart. Used for external
        ///     charts that are not part of the Song Select catalog.
        /// </summary>
        public static IEnumerator PrewarmSong(PlayableSong song, ExternalChartMeta chart)
        {
            HashSet<int> codePoints = new();
            AddText(codePoints, BaselineCharacters);

            if (song != null)
            {
                AddText(codePoints, song.SongName);
                AddText(codePoints, song.AltSongName);
                AddText(codePoints, song.SongArtist);
                AddText(codePoints, song.AltSongArtist);
                AddText(codePoints, song.Location);
                AddText(codePoints, song.Genre);

                if (song.Cover != null)
                {
                    AddText(codePoints, song.Cover.ArtistName);
                    AddText(codePoints, song.Cover.AltArtistName);
                }
            }

            if (chart != null)
            {
                AddText(codePoints, chart.DifficultyName);
                AddText(codePoints, chart.DifficultyLevel);
                AddText(codePoints, chart.CharterName);
                AddText(codePoints, chart.AltCharterName);
            }

            yield return PrewarmCodePoints(codePoints);
        }

        private static IEnumerator PrewarmCodePoints(HashSet<int> codePoints)
        {
            if (codePoints == null || codePoints.Count == 0) yield break;

            TMP_FontAsset[] fonts = LoadFonts();
            if (fonts.Length == 0) yield break;

            foreach (TMP_FontAsset font in fonts)
            {
                if (font == null || font.atlasPopulationMode == AtlasPopulationMode.Static) continue;

                // Song catalogs can contain thousands of CJK glyphs; let the atlas grow.
                if (!font.isMultiAtlasTexturesEnabled)
                    font.isMultiAtlasTexturesEnabled = true;

                StringBuilder chunk = new(CodePointsPerChunk);
                int count = 0;

                foreach (int codePoint in codePoints)
                {
                    chunk.Append(char.ConvertFromUtf32(codePoint));
                    count++;

                    if (count < CodePointsPerChunk) continue;

                    font.TryAddCharacters(chunk.ToString(), false);
                    chunk.Clear();
                    count = 0;
                    yield return null;
                }

                if (count > 0)
                    font.TryAddCharacters(chunk.ToString(), false);

                // Yield between fonts too, in case a face load blocked for a while.
                yield return null;
            }
        }

        private static TMP_FontAsset[] LoadFonts()
        {
            List<TMP_FontAsset> fonts = new();

            AddFont(fonts, TMP_Settings.defaultFontAsset);

            if (TMP_Settings.fallbackFontAssets != null)
                foreach (TMP_FontAsset font in TMP_Settings.fallbackFontAssets)
                    AddFont(fonts, font);

            TMP_FontAsset[] resourceFonts = Resources.LoadAll<TMP_FontAsset>("Fonts");
            if (resourceFonts != null)
                foreach (TMP_FontAsset font in resourceFonts)
                    AddFont(fonts, font);

            return fonts.ToArray();
        }

        private static void AddFont(List<TMP_FontAsset> fonts, TMP_FontAsset font)
        {
            if (font != null && !fonts.Contains(font))
                fonts.Add(font);
        }

        private static void AddText(HashSet<int> codePoints, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            for (int i = 0; i < text.Length;)
            {
                int codePoint;

                try
                {
                    codePoint = char.ConvertToUtf32(text, i);
                }
                catch (System.ArgumentException)
                {
                    i++;
                    continue;
                }

                if (!IsControl(codePoint))
                    codePoints.Add(codePoint);

                i += char.IsSurrogatePair(text, i) ? 2 : 1;
            }
        }

        private static bool IsControl(int codePoint) =>
            codePoint is >= 0 and < 32 || codePoint is >= 0x7F and <= 0x9F;
    }
}
