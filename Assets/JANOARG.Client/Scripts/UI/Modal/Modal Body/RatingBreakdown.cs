using System.Collections.Generic;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.Panels.Profile;
using JANOARG.Client.Data.Playlist;
using JANOARG.Client.Data.Storage;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using TMPro;
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
        public Canvas ScreenshotCanvas;
        public TMP_Text PlayerName;
        public TMP_Text PlayerTitle;
        public TMP_Text LevelValue;
        public TMP_Text AbilityRatingValue;

        public bool IsAnimating = false;
        
        public List<RatingBreakdownEntry> RatingBreakdownEntries;
        public List<ScoreStoreEntry> ScoreStoreEntries;

        List<RatingBreakdownEntry> ScreenshotEntries;

        const float MAX_BLOOM_INTENSITY = 2.4f;
        const float MAX_BLOOM_RANGE = 0.32f;
        const float SHARE_ASPECT = 3072f / 1280f;

        RectTransform _BloomHolder;
        Material _HaloMaterial;
        Texture2D _HaloTexture;
        List<RawImage> _Halos;

        //This Playlist will be the main/root playlist so we can use the PlayableSong's metachart and cover
        public Playlist MainPlaylist;
        public Dictionary<string, PlayableSong> SongDict;

        void Awake()
        {
            // A nested canvas ignores its ScreenSpaceCamera render mode, so detach
            // the screenshot canvas to keep it a root canvas rendered by its camera.
            if (ScreenshotCanvas != null)
                ScreenshotCanvas.transform.SetParent(null, false);
        }

        void OnDestroy()
        {
            if (ScreenshotCanvas != null)
                Destroy(ScreenshotCanvas.gameObject);
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

            PlayerName.text = CommonSys.sMain.Storage.Get("INFO:Name", "JANOARG");
            PlayerTitle.text = CommonSys.sMain.Storage.Get("INFO:Title", "Perfectly Generic Player");
            LevelValue.text = CommonSys.sMain.Storage.Get("INFO:Level", 1).ToString();
            AbilityRatingValue.text = ProfileBar.sMain.AbilityRating.ToString("F2");

            ScreenshotEntries = new List<RatingBreakdownEntry>(
                ScreenshotCanvas.GetComponentsInChildren<RatingBreakdownEntry>(true)
            );
            ScreenshotEntries.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

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

            // Columns start hidden and are only revealed once populated, so a
            // failed entry can never leave a placeholder on the card.
            for (int i = 0; i < ScreenshotEntries.Count; i++)
            {
                if (ScreenshotEntries[i] != null)
                    ScreenshotEntries[i].gameObject.SetActive(false);
            }

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
                var screenshotEntry = i < ScreenshotEntries.Count ? ScreenshotEntries[i] : null;

                displayEntry.SetData(scoreEntry);
                screenshotEntry?.SetData(scoreEntry);

                if (SongDict != null && SongDict.TryGetValue(scoreEntry.SongID, out var song))
                {
                    ExternalChartMeta chart = song.Charts != null
                        ? song.Charts.Find(x => x.Target == scoreEntry.ChartID)
                        : null;

                    string songName = Truncate(song.SongName ?? "", 30);
                    string songArtist = Truncate(song.SongArtist ?? "", 30);
                    string chartConstant = chart != null ? chart.DifficultyLevel.ToString() : "--";
                    Color chartColor = CommonSys.sMain.Constants.GetDifficultyColor(scoreEntry.ChartIndex);

                    SetSongInfo(displayEntry, songName, songArtist, chartConstant, chartColor);
                    SetSongInfo(screenshotEntry, songName, songArtist, chartConstant, chartColor);

                    Texture2D iconTex = null;

                    yield return StartCoroutine(
                        GetIconImage(scoreEntry.SongID, (tex) =>
                        {
                            iconTex = tex;
                        })
                    );

                    if (iconTex != null)
                    {
                        if (displayEntry.Icon != null)
                        {
                            displayEntry.Icon.texture = iconTex;
                        }

                        if (screenshotEntry != null && screenshotEntry.Icon != null)
                        {
                            screenshotEntry.Icon.texture = iconTex;
                        }
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
                        if (displayEntry.BackgroundCover != null)
                        {
                            displayEntry.BackgroundCover.texture = coverTex;
                            displayEntry.BackgroundCover.color = Color.white;
                        }

                        if (screenshotEntry != null && screenshotEntry.BackgroundCover != null)
                        {
                            screenshotEntry.BackgroundCover.texture = coverTex;
                            screenshotEntry.BackgroundCover.color = Color.white;
                        }
                    } else
                    {
                        Debug.LogWarning($"Cover not found: {scoreEntry.SongID}");
                    }

                    if (screenshotEntry != null)
                        screenshotEntry.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"Song not found: {scoreEntry.SongID}");
                }
            }
        }

        void SetSongInfo(RatingBreakdownEntry entry, string songName, string songArtist, string chartConstant, Color chartColor)
        {
            if (entry == null)
                return;

            if (entry.SongName != null)
                entry.SongName.text = songName;

            if (entry.SongArtist != null)
                entry.SongArtist.text = songArtist;

            if (entry.ChartConstant != null)
            {
                entry.ChartConstant.text = chartConstant;
                entry.ChartConstant.color = chartColor;
            }
        }

        void ApplyCoverCrops()
        {
            int count = Mathf.Min(ScoreStoreEntries.Count, ScreenshotEntries.Count);

            for (int i = 0; i < count; i++)
            {
                ScoreStoreEntry scoreEntry = ScoreStoreEntries[i];

                if (scoreEntry == null || SongDict == null)
                    continue;

                if (SongDict.TryGetValue(scoreEntry.SongID, out PlayableSong song))
                    ApplyCoverCrop(ScreenshotEntries[i], song);
            }
        }

        void ApplyCoverCrop(RatingBreakdownEntry entry, PlayableSong song)
        {
            if (entry == null || entry.BackgroundCover == null || song == null || song.Cover == null)
                return;

            Texture texture = entry.BackgroundCover.texture;

            if (texture == null)
                return;

            Rect rect = entry.BackgroundCover.rectTransform.rect;

            if (rect.width <= 0 || rect.height <= 0)
                return;

            float textureAspect = (float)texture.width / texture.height;
            float rectAspect = rect.width / rect.height;

            float width = 1;
            float height = 1;

            if (textureAspect > rectAspect)
                width = rectAspect / textureAspect;
            else
                height = textureAspect / rectAspect;

            Vector2 uvCenter = new(0.5f, 0.5f);

            if (song.Cover.Layers.Count > 0)
            {
                CoverLayer layer = song.Cover.Layers[0];

                if (Mathf.Abs(layer.Scale) > 0.0001f)
                {
                    Vector2 position = layer.Position + song.Cover.IconCenter * layer.ParallaxFactor;
                    Vector2 size = 880 * layer.Scale * new Vector2(1, (float)texture.height / texture.width);

                    if (Mathf.Abs(size.x) > 0.0001f) uvCenter.x = 0.5f - position.x / size.x;
                    if (Mathf.Abs(size.y) > 0.0001f) uvCenter.y = 0.5f - position.y / size.y;
                }
            }

            float x = Mathf.Clamp(uvCenter.x - width * 0.5f, 0, 1 - width);
            float y = Mathf.Clamp(uvCenter.y - height * 0.5f, 0, 1 - height);

            entry.BackgroundCover.uvRect = new Rect(x, y, width, height);
        }

        void GenerateBloom()
        {
            if (ScreenshotCanvas == null || ScoreStoreEntries == null || ScreenshotEntries == null)
                return;

            RectTransform canvasRect = (RectTransform)ScreenshotCanvas.transform;
            int layer = ScreenshotCanvas.gameObject.layer;

            if (_BloomHolder == null)
            {
                GameObject holder = new("Bloom", typeof(RectTransform));
                holder.layer = layer;

                _BloomHolder = (RectTransform)holder.transform;
                _BloomHolder.SetParent(canvasRect, false);
                _BloomHolder.anchorMin = Vector2.zero;
                _BloomHolder.anchorMax = Vector2.one;
                _BloomHolder.offsetMin = Vector2.zero;
                _BloomHolder.offsetMax = Vector2.zero;
                _BloomHolder.SetAsFirstSibling();

                GameObject background = new("Background", typeof(RawImage));
                background.layer = layer;

                RawImage backgroundImage = background.GetComponent<RawImage>();
                backgroundImage.color = Color.black;
                backgroundImage.raycastTarget = false;

                RectTransform backgroundRect = (RectTransform)background.transform;
                backgroundRect.SetParent(_BloomHolder, false);
                backgroundRect.anchorMin = Vector2.zero;
                backgroundRect.anchorMax = Vector2.one;
                backgroundRect.offsetMin = Vector2.zero;
                backgroundRect.offsetMax = Vector2.zero;
            }

            if (_HaloMaterial == null)
            {
                Shader haloShader = Shader.Find("UI/Halo");

                if (haloShader == null)
                    return;

                _HaloTexture = BuildHaloTexture(128);
                _HaloMaterial = new Material(haloShader);
            }

            int count = Mathf.Min(ScoreStoreEntries.Count, ScreenshotEntries.Count);

            List<Color> colors = new();
            List<float> intensities = new();
            List<float> ranges = new();

            for (int i = 0; i < count; i++)
            {
                ScoreStoreEntry scoreEntry = ScoreStoreEntries[i];
                RatingBreakdownEntry entry = ScreenshotEntries[i];

                if (scoreEntry == null || entry == null || entry.BackgroundCover == null)
                    continue;

                Color color = SampleCoverColor(entry.BackgroundCover.texture);

                if (color.a <= 0)
                    continue;

                int judged = scoreEntry.PerfectCount + scoreEntry.GoodCount + scoreEntry.BadCount;
                float comboNorm = judged > 0 ? Mathf.Clamp01((float)scoreEntry.MaxCombo / judged) : 0;
                float scoreNorm = Mathf.Clamp01(scoreEntry.Score / 1000000f);

                colors.Add(color);
                intensities.Add(MAX_BLOOM_INTENSITY * comboNorm);
                ranges.Add(MAX_BLOOM_RANGE * scoreNorm);
            }

            List<Vector2> positions = ScatterPositions(ranges);

            _Halos ??= new List<RawImage>();

            while (_Halos.Count < colors.Count)
            {
                GameObject haloObject = new("Halo", typeof(RawImage));
                haloObject.layer = layer;

                RawImage halo = haloObject.GetComponent<RawImage>();
                halo.texture = _HaloTexture;
                halo.material = _HaloMaterial;
                halo.raycastTarget = false;
                ((RectTransform)haloObject.transform).SetParent(_BloomHolder, false);

                _Halos.Add(halo);
            }

            for (int i = colors.Count; i < _Halos.Count; i++)
                _Halos[i].gameObject.SetActive(false);

            int[] order = new int[colors.Count];

            for (int i = 0; i < order.Length; i++)
                order[i] = i;

            System.Array.Sort(order, (a, b) => {
                int compare = intensities[b].CompareTo(intensities[a]);
                return compare != 0 ? compare : ranges[b].CompareTo(ranges[a]);
            });

            // Rank the halos by their config, then hand out the sampled colours
            // (first sampled first) so both ranks correlate.
            for (int rank = 0; rank < order.Length; rank++)
            {
                int index = order[rank];
                Color color = colors[rank];
                float intensity = intensities[index];
                RawImage halo = _Halos[rank];

                RectTransform haloRect = halo.rectTransform;
                Vector2 position = positions[index];
                float halfHeight = ranges[index];
                float halfWidth = halfHeight / SHARE_ASPECT;

                halo.color = new Color(color.r * intensity, color.g * intensity, color.b * intensity, 1);
                haloRect.anchorMin = new Vector2(position.x - halfWidth, position.y - halfHeight);
                haloRect.anchorMax = new Vector2(position.x + halfWidth, position.y + halfHeight);
                haloRect.anchoredPosition = Vector2.zero;
                haloRect.sizeDelta = Vector2.zero;
                halo.transform.SetSiblingIndex(rank + 1);
                halo.gameObject.SetActive(true);
            }
        }

        List<Vector2> ScatterPositions(List<float> ranges)
        {
            List<Vector2> positions = new();

            const float EDGE_INSET = 0.5f;
            const float BASE_INSET = 0.03f;
            const float CLUMP_FACTOR = 0.35f;
            const int MAX_ATTEMPTS = 24;

            foreach (float range in ranges)
            {
                float insetX = EDGE_INSET * range / SHARE_ASPECT + BASE_INSET;
                float insetY = EDGE_INSET * range + BASE_INSET;

                float minX = insetX;
                float maxX = 1 - insetX;
                float minY = insetY;
                float maxY = 1 - insetY;

                if (minX > maxX)
                    minX = maxX = 0.5f;

                if (minY > maxY)
                    minY = maxY = 0.5f;

                Vector2 position = new(Random.Range(minX, maxX), Random.Range(minY, maxY));

                for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
                {
                    Vector2 candidate = new(Random.Range(minX, maxX), Random.Range(minY, maxY));
                    bool clear = true;

                    for (int i = 0; i < positions.Count; i++)
                    {
                        float wanted = (range + ranges[i]) * CLUMP_FACTOR;
                        Vector2 delta = new(
                            (candidate.x - positions[i].x) * SHARE_ASPECT,
                            candidate.y - positions[i].y
                        );

                        if (delta.magnitude < wanted)
                        {
                            clear = false;
                            break;
                        }
                    }

                    if (clear)
                    {
                        position = candidate;
                        break;
                    }
                }

                positions.Add(position);
            }

            return positions;
        }

        Texture2D BuildHaloTexture(int size)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                float falloff = Mathf.Clamp01(1 - distance);
                falloff *= falloff;

                texture.SetPixel(x, y, new Color(1, 1, 1, falloff));
            }

            texture.Apply();

            return texture;
        }

        Color SampleCoverColor(Texture cover)
        {
            if (cover == null)
                return Color.clear;

            RenderTexture renderTexture = RenderTexture.GetTemporary(64, 64, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(cover, renderTexture);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;

            Texture2D readable = new(64, 64, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
            readable.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);

            Color pick = Color.clear;
            Color whitePick = Color.clear;
            int candidates = 0;
            int whiteCandidates = 0;

            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                Color color = readable.GetPixel(x, y);

                if (color.a < 0.9f)
                    continue;

                float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
                float min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));

                if (max < 0.08f)
                    continue;

                float saturation = max > 0.0001f ? (max - min) / max : 0;

                if (max > 0.85f && saturation < 0.15f)
                {
                    whiteCandidates++;

                    if (Random.Range(0, whiteCandidates) == 0)
                        whitePick = color;
                }
                else
                {
                    candidates++;

                    if (Random.Range(0, candidates) == 0)
                        pick = color;
                }
            }

            Destroy(readable);

            return candidates > 0 ? pick : whitePick;
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
            Canvas.ForceUpdateCanvases();

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

            ApplyCoverCrops();
            GenerateBloom();

            Vector2Int size = CommonSys.GetShareSize(3072f / 1280f);
            Texture2D  image = Screenshot(size.x, size.y);

            yield return Share(image);

            IsAnimating = false;
        }

        public IEnumerator Share(Texture2D image)
        {
            string path = Application.persistentDataPath + "/screenshot.png";
            Task task = File.WriteAllBytesAsync(path, image.EncodeToPNG());

            yield return new WaitUntil(() => task.IsCompleted);

            CommonSys.ShareFile(path);
        }
    }
}