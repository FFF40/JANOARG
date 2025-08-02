using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using Random = UnityEngine.Random;

public class SongSelectScreen : MonoBehaviour
{
    public static SongSelectScreen main;

    public Playlist Playlist;
    public Dictionary<string, PlayableSong> SongList { get; private set; } = new();

    [Header("List View")]
    public SongSelectListView ListView;

    [Header("Map View")]
    public MapManager MapManager;
    public RectTransform MapLerpItemHolder;
    [Space]
    public CanvasGroup MapUIGroup;
    public Image MapCover;
    
    [Header("Song Selection")]
    public RectTransform SafeAreaHolder;
    [Space]
    public CanvasGroup TargetSongInfoHolder;
    public TMP_Text TargetSongInfoName;
    public TMP_Text TargetSongInfoArtist;
    public TMP_Text TargetSongInfoInfo;
    [Space]
    public RectTransform TargetSongCoverHolder;
    public Image TargetSongCoverBackground;
    public Shadow TargetSongCoverShadow;
    public Image TargetSongCoverFlash;
    public CoverLayerImage CoverLayerSample;
    public RectTransform TargetSongCoverLayerHolder;
    public List<CoverLayerImage> TargetSongCoverLayers;

    [Header("Difficulty Selection")]
    public CanvasGroup DifficultyHolder;
    public RectTransform DifficultyListHolder;
    public SongSelectDifficulty DifficultySample;
    public List<SongSelectDifficulty> DifficultyList { get; private set; } = new();

    public int SelectedDifficulty;
    public SongSelectDifficulty TargetDifficulty;
    public TMP_Text TargetDifficultyName;
    public TMP_Text TargetDifficultyScore;
    public GameObject TargetDifficultyNewIndicator;

    [Header("Actions")]
    public CanvasGroup LeftActionsHolder;
    public Button BackButton;
    [Space]
    public CanvasGroup RightActionsHolder;
    public Button MapViewButton;
    public Button ListViewButton;
    public Button SortButton;
    public Button LaunchButton;

    [Header("Launch")]
    public CanvasGroup LaunchTextHolder;
    public TMP_Text LaunchText;
    [Space]
    public SongSelectReadyScreen ReadyScreen;

    [Header("Audio")]
    public AudioSource PreviewSource;
    public float PreviewVolume;
    public float PreviewVolumeMulti;
    public AudioClip CurrentPreviewClip;
    public Vector2 CurrentPreviewRange;
    [Space]
    public AudioSource SFXSource;
    public AudioClip SFXTickClip;
    public float SFXVolume;


    [Header("Data")]
    public bool IsReady;
    public bool IsAnimating;
    public bool IsInit;
    public bool IsMapView = true;
    public Coroutine TargetSongAnim;

    [NonSerialized] public MapItem TargetMapItem;
    [NonSerialized] public PlayableSong TargetSong;
    [NonSerialized] public string TargetSongID;
    [NonSerialized] public Cover CurrentCover;

    public void Awake()
    {
        main = this;
        CommonScene.Load();
    }

    public void Start()
    {
        OnSettingDirty();
        Common.main.Storage.OnSave.AddListener(OnSave);
        StartCoroutine(InitPlaylist());
    }

    public void OnDestroy()
    {
        Common.main.Storage.OnSave.RemoveListener(OnSave);
    }

    public void OnSave() 
    {
        OnSettingDirty();
    }

    public void OnSettingDirty() 
    {
        PreviewVolumeMulti = Common.main.Preferences.Get("GENR:UIMusicVolume", 100f) / 100f;
        SFXVolume = Common.main.Preferences.Get("GENR:UISFXVolume", 100f) / 100f;
    }
            
    public void Update()
    {
        if (!IsReady) return;

        if (ListView) ListView.HandleUpdate();

        // Handle song preview
        float previewVolumeSpeed = 1;
        if (ReadyScreen.IsAnimating) previewVolumeSpeed = -0.5f;
        else if (CurrentPreviewClip != PreviewSource.clip) previewVolumeSpeed = -2;
        else if (!CurrentPreviewClip) previewVolumeSpeed = -2;
        else if (CurrentPreviewClip.loadState != AudioDataLoadState.Loaded) previewVolumeSpeed = -1;
        else if (CurrentPreviewRange.y - PreviewSource.time <= PreviewVolume) previewVolumeSpeed = -1;
        else if (ListView != null && ListView.IsTargetSongHidden) previewVolumeSpeed = -0.4f;
        PreviewVolume = Mathf.Clamp01(PreviewVolume + previewVolumeSpeed * Time.deltaTime);
        PreviewSource.volume = PreviewVolume * PreviewVolumeMulti;
        if (PreviewVolume <= 0) 
        {
            if (CurrentPreviewClip != PreviewSource.clip)
            {
                if (!PreviewSource.clip) 
                {
                    PreviewSource.clip = CurrentPreviewClip;
                    PreviewSource.clip.LoadAudioData();
                    PreviewSource.Play();
                    PreviewSource.time = CurrentPreviewRange.x;
                }
                else if (PreviewSource.clip.loadState == AudioDataLoadState.Loaded)
                {
                    PreviewSource.clip.UnloadAudioData();
                    PreviewSource.clip = CurrentPreviewClip;
                    PreviewSource.Play();
                    PreviewSource.time = CurrentPreviewRange.x;
                }
            }
            else if (PreviewSource.time > CurrentPreviewRange.y) 
            {
                PreviewSource.time -= CurrentPreviewRange.y - CurrentPreviewRange.x;
            }
        }
    }

    public IEnumerator InitPlaylist()
    {
        // Before load
        MapCover.color = Common.main.MainCamera.backgroundColor * new ColorFrag(a: 1);
        Common.main.MainCamera.backgroundColor = RenderSettings.fogColor = Playlist.BackgroundColor;

        int index = 0;
        int pos = 0;
        MapManager.LoadMap();
        foreach (PlaylistSong songInfo in Playlist.Songs)
        {
            string path = $"Songs/{songInfo.ID}/{songInfo.ID}";
            ResourceRequest req = Resources.LoadAsync<ExternalPlayableSong>(path);
            yield return new WaitUntil(() => req.isDone);
            if (!req.asset)
            {
                Debug.LogWarning("Couldn't load Playable Song at " + path);
                continue;
            }
            PlayableSong song = ((ExternalPlayableSong)req.asset).Data;
            SongList.Add(songInfo.ID, song);

            SongSelectListItem item = Instantiate(ListView.ItemSample, ListView.ItemHolder);
            item.SetItem(songInfo, song, index, pos);
            ListView.ItemList.Add(item);
            index++;
            pos += 48;
        }

        yield return new WaitUntil(() => MapManager.IsReady);
        IsInit = true;
        
        if (!LoadingBar.main.gameObject.activeSelf) Intro();
    }

    public void UpdateListItems(bool cap = true)
    {
        ListView.UpdateListItems(this, cap);
    }

    public void UpdateListTarget() 
    {
        ListView.UpdateListTarget();
    }

    public void UpdateButtons()
    {
        BackButton.gameObject.SetActive(IsMapView && TargetMapItem is SongMapItem);
        ListViewButton.gameObject.SetActive(IsMapView && !TargetMapItem);

        MapViewButton.gameObject.SetActive(!IsMapView);
        SortButton.gameObject.SetActive(!IsMapView);
        LaunchButton.gameObject.SetActive(!IsMapView || TargetMapItem is SongMapItem);
    }

    public void ToggleView()
    {
        if (IsAnimating) return;
        IsMapView = !IsMapView;
        StartCoroutine(ToggleViewAnim());
    }

    IEnumerator ToggleViewAnim()
    {
        IsAnimating = true;
        ListView.ItemGroup.blocksRaycasts = !IsMapView;
        if (TargetSongAnim != null) StopCoroutine(TargetSongAnim);

        // Animate map icons to list icons
        var mapListItems = MapManager.main.GetMapToListItems(ListView.ItemList);
        float lerpFrom = IsMapView ? 1 : 0;
        float lerpTo = 1 - lerpFrom;
        IEnumerator MapCoroutine()
        {
            foreach ((SongMapItemUI mapItem, SongSelectListItem listItem) in mapListItems)
            {
                listItem.CoverBorder.gameObject.SetActive(false);
                mapItem.transform.SetParent(MapLerpItemHolder);
            }
            yield return Ease.Animate(0.5f, (x) =>
            {
                float ease1 = Mathf.Lerp(lerpFrom, lerpTo, Ease.Get(x, EaseFunction.Quartic, EaseMode.Out));
                foreach ((SongMapItemUI mapItem, SongSelectListItem listItem) in mapListItems)
                {
                    mapItem.UpdatePosition();
                    mapItem.transform.position = Vector3.Lerp(mapItem.transform.position, listItem.CoverImage.transform.position, ease1);
                }
            });
            foreach ((SongMapItemUI mapItem, SongSelectListItem listItem) in mapListItems)
            {
                listItem.CoverBorder.gameObject.SetActive(true);
                mapItem.transform.SetParent(MapManager.ItemUIHolder);
            }
        }
        Coroutine mapCoroutine = StartCoroutine(MapCoroutine());
        
        // Animate bottom buttons
        Coroutine navCoroutine = StartCoroutine(NavUpdateAnim());
        
        // Animate the cover
        IEnumerator CoverCoroutine()
        {
            SongSelectListItem targetSong = ListView.ItemList.Find(item => ListView.TargetScrollOffset == item.Position);
            if (!MapManager.SongMapItemUIsByID.TryGetValue(targetSong.MapInfo.ID, out SongMapItemUI target)) yield break;
            target.CoverImage.gameObject.SetActive(false);
            TargetSongCoverHolder.gameObject.SetActive(true);
            yield return Ease.Animate(lerpFrom == 0 ? 0.6f : 0.5f, (x) =>
            {
                float ease1 = lerpFrom == 0
                    ? Ease.Get(Mathf.Pow(x, .5f), EaseFunction.Exponential, EaseMode.InOut)
                    : 1 - Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
                LerpCover(ease1, target.CoverImage.transform as RectTransform);
            });
            TargetSongCoverHolder.gameObject.SetActive(lerpTo > 0);
            target.CoverImage.gameObject.SetActive(true);
        }
        Coroutine coverCoroutine = null;

        if (IsMapView)
        {
            // In map view 

            CurrentPreviewClip = null;
            coverCoroutine = StartCoroutine(CoverCoroutine());

            // Animate
            yield return Ease.Animate(0.3f, (x) =>
            {
                float ease1 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                MapUIGroup.alpha = ease1;
                ListView.LerpListView(1 - ease1);
                LerpInfo(1 - ease1);
                LerpDifficulty(1 - ease1);
            });
        }
        else
        {
            // In list view 

            // Update list item positions
            ListView.TargetSongOffset = ListView.TargetScrollOffset;
            SongSelectListItem targetSong = ListView.ItemList.Find(item => ListView.TargetScrollOffset == item.Position);
            if (targetSong)
            {
                SetTargetSong(targetSong.MapInfo.ID, targetSong.Song);
                var songItem = targetSong.MapInfo;
                yield return SetCover(songItem, targetSong.Song);
                coverCoroutine = StartCoroutine(CoverCoroutine());
            }

            // Update list item positions
            ListView.TargetSongHiddenTarget = ListView.IsTargetSongHidden = false;
            coverLerp = 1;
            foreach (SongSelectListItem item in ListView.ItemList)
            {
                item.PositionOffset = 45 * Mathf.Clamp(item.Position - ListView.TargetSongOffset, -1, 1);
            }
            ListView.UpdateListItems(this);

            // Animate
            yield return Ease.Animate(0.6f, (x) =>
            {
                float ease1 = Ease.Get(x * 3, EaseFunction.Cubic, EaseMode.Out);
                MapUIGroup.alpha = 1 - ease1;
                ListView.LerpListView(ease1);

                float ease2 = Ease.Get(x * 3 - 1, EaseFunction.Cubic, EaseMode.Out);
                LerpDifficulty(ease2);
                LerpInfo(ease2);
            });
        }

        yield return mapCoroutine;
        yield return navCoroutine;
        TargetSongAnim = coverCoroutine;

        IsAnimating = false;
    }

    public void SetTargetSong(string songID, PlayableSong targetSong)
    {
        TargetSongID = songID;
        TargetSong = targetSong;
        TargetSongInfoName.text = targetSong.SongName;
        TargetSongInfoArtist.text = targetSong.SongArtist;

        float songLength = targetSong.Clip.length;
        TargetSongInfoInfo.text = Mathf.Floor(songLength / 60) + "m " + Mathf.Floor(songLength % 60) + "s";

        float minBPM = float.PositiveInfinity, maxBPM = float.NegativeInfinity;
        foreach (BPMStop stop in targetSong.Timing.Stops)
        {
            if (stop.Significant)
            {
                minBPM = Mathf.Min(minBPM, stop.BPM);
                maxBPM = Mathf.Max(maxBPM, stop.BPM);
            }
        }
        TargetSongInfoInfo.text += " - BPM " + (minBPM == maxBPM ? minBPM : minBPM + "~" + maxBPM);

        TargetSongInfoInfo.text += " - " + targetSong.Genre.ToUpper();

        CurrentPreviewClip = targetSong.Clip;
        CurrentPreviewRange = targetSong.PreviewRange;

        string songPath = $"Songs/{songID}/{songID}";
        foreach (SongSelectDifficulty diff in DifficultyList) Destroy(diff.gameObject);
        DifficultyList.Clear();
        var target = GetNearestDifficulty(targetSong.Charts);
        foreach (ExternalChartMeta chart in targetSong.Charts)
        {
            string chartID = Path.GetFileNameWithoutExtension(chart.Target);
            var record = StorageManager.main.Scores.Get(songID, chartID);

            SongSelectDifficulty diff = Instantiate(DifficultySample, DifficultyListHolder);
            diff.SetItem(chart, record, Common.main.Constants.GetDifficultyColor(chart.DifficultyIndex));
            diff.Button.onClick.AddListener(() => ChangeDiff(diff));
            DifficultyList.Add(diff);
            if (chart == target) TargetDifficulty = diff;
        }
        TargetDifficulty.SetSelectability(1);
        rt(TargetDifficulty.Holder).anchoredPosition = new(0, 5);
        SetScoreInfo(TargetDifficulty);
        LayoutRebuilder.MarkLayoutForRebuild(rt(DifficultyHolder.transform));
    }

    public IEnumerator ListTargetSongShowAnim()
    {
        ListView.TargetSongOffset = ListView.TargetScrollOffset;
        SongSelectListItem targetSong = ListView.ItemList.Find(item => ListView.TargetScrollOffset == item.Position);
        if (targetSong)
        {
            SetTargetSong(targetSong.MapInfo.ID, targetSong.Song);
            var songItem = targetSong.MapInfo;
            yield return SetCover(songItem, targetSong.Song);
        }

        yield return Ease.Animate(.9f, a =>
        {
            float lerp = Ease.Get(a * 3 - 1, EaseFunction.Cubic, EaseMode.Out);
            LerpInfo(lerp);

            float lerp2 = Ease.Get(a * 1.5f, EaseFunction.Exponential, EaseMode.InOut)
                * Ease.Get(a, EaseFunction.Exponential, EaseMode.Out);
            LerpCoverList(lerp2);

            float lerp3 = Ease.Get(a * 3, EaseFunction.Cubic, EaseMode.Out);
            LerpUI(lerp3);

            float lerp4 = Ease.Get(a * 1.5f, EaseFunction.Circle, EaseMode.Out);
            foreach (SongSelectListItem item in ListView.ItemList)
            {
                item.PositionOffset = 45 * Mathf.Clamp(item.Position - ListView.TargetSongOffset, -1, 1)
                     * (lerp2 * .5f + lerp4 * .5f);
            }
            ListView.IsDirty = true;
        });

        if (ListView.TargetSongOffset != ListView.TargetScrollOffset)
        {
            ListView.IsTargetSongHidden = true;
            StartCoroutine(ListTargetSongHideAnim());
        }

        TargetSongAnim = null;
    }

    public IEnumerator SetCover(PlaylistSong songItem, PlayableSong targetSong)
    {
        TargetSongCoverBackground.color = targetSong.Cover.BackgroundColor;
        TargetSongCoverHolder.gameObject.SetActive(false);
        if (CurrentCover != targetSong.Cover) 
        {
            CurrentCover = targetSong.Cover;
            foreach (var layer in TargetSongCoverLayers) 
            {
                Resources.UnloadAsset(layer.Image.texture);
                Destroy(layer.gameObject);
            }
            TargetSongCoverLayers.Clear();
            foreach (CoverLayer layer in CurrentCover.Layers)
            {
                string path = Path.Combine($"Songs/{songItem.ID}", layer.Target);
                if (Path.HasExtension(path)) path = Path.ChangeExtension(path, "")[0..^1];
                var req = Resources.LoadAsync<Texture2D>(path);
                yield return req;
                if (req.asset)
                {
                    Texture2D tex = (Texture2D)req.asset;
                    var layerImage = Instantiate(CoverLayerSample, TargetSongCoverLayerHolder);
                    layerImage.Layer = layer;
                    layerImage.Image.texture = tex;
                    TargetSongCoverLayers.Add(layerImage);
                }
            }
        }
        TargetSongCoverHolder.gameObject.SetActive(true);
    }

    public ExternalChartMeta GetNearestDifficulty(List<ExternalChartMeta> charts)
    {
        int dist = int.MaxValue;
        ExternalChartMeta target = null;
        foreach (var chart in charts)
        {
            int chartDiff = Mathf.Abs((chart.DifficultyIndex - SelectedDifficulty + 100) % 100);
            if (dist > chartDiff)
            {
                dist = chartDiff;
                target = chart;
            }

        }
        return target;
    }

    public IEnumerator ListTargetSongHideAnim()
    {
        float lerpCoverStart = coverLerp;
        yield return Ease.Animate(.3f, a => {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            LerpInfo(1 - lerp); 
            LerpUI(1 - lerp);

            float lerp2 = Ease.Get(a, EaseFunction.Quintic, EaseMode.Out);
            LerpCoverList((1 - lerp2) * lerpCoverStart);
            foreach (SongSelectListItem item in ListView.ItemList)
            {
                item.PositionOffset = 45 * (1 - lerp2) * Mathf.Clamp(item.Position - ListView.TargetSongOffset, -1, 1);
            }
            ListView.IsDirty = true;
        });
        TargetSongCoverHolder.gameObject.SetActive(false);

        TargetSongAnim = null;
    }

    public IEnumerator MapTargetSongShowAnim(SongMapItem target)
    {
        TargetMapItem = target;
        PlayableSong song = SongList[target.TargetID];
        SetTargetSong(target.TargetID, song);
        LerpCover(0, target.ItemUI.CoverImage.rectTransform);
        yield return SetCover(target.Target, song);
        target.ItemUI.CoverImage.gameObject.SetActive(false);

        Coroutine navCoroutine = StartCoroutine(NavUpdateAnim());

        MapUIGroup.blocksRaycasts = MapUIGroup.interactable = false;

        yield return Ease.Animate(.9f, a =>
        {
            float lerp = Ease.Get(a * 3 - 1, EaseFunction.Cubic, EaseMode.Out);
            LerpInfo(lerp);
            LerpDifficulty(lerp);

            float lerp2 = Ease.Get(a * 1.5f, EaseFunction.Exponential, EaseMode.InOut)
                * Ease.Get(a, EaseFunction.Exponential, EaseMode.Out);
            LerpCover(lerp2, target.ItemUI.CoverImage.rectTransform);

            float lerp3 = Ease.Get(a * 3, EaseFunction.Cubic, EaseMode.InOut);
            Common.main.MainCamera.transform.position *= new Vector3Frag(z: -lerp3 - 10);
            MapUIGroup.alpha = 1 - lerp3;
            MapUIGroup.transform.localScale = Vector3.one * (1 - lerp3 * 0.1f);
        });

        yield return navCoroutine;

        TargetSongAnim = null;
    }

    public IEnumerator MapTargetSongHideAnim()
    {
        SongMapItem target = (SongMapItem)TargetMapItem;
        var coverTarget = target.ItemUI.CoverImage.rectTransform;
        TargetMapItem = null;
        CurrentPreviewClip = null;

        Coroutine navCoroutine = StartCoroutine(NavUpdateAnim());

        yield return Ease.Animate(.6f, a =>
        {
            float lerp = Ease.Get(a * 2, EaseFunction.Cubic, EaseMode.Out);
            LerpInfo(1 - lerp);
            LerpDifficulty(1 - lerp);

            float lerp2 = Ease.Get(a, EaseFunction.Quintic, EaseMode.Out);
            LerpCover(1 - lerp2, coverTarget);

            float lerp3 = Ease.Get(a * 2, EaseFunction.Cubic, EaseMode.InOut);
            Common.main.MainCamera.transform.position *= new Vector3Frag(z: lerp3 - 11);
            MapUIGroup.alpha = lerp3;
            MapUIGroup.transform.localScale = Vector3.one * (1 - (1 - lerp3) * 0.1f);
        });

        yield return navCoroutine;

        TargetSongCoverHolder.gameObject.SetActive(false);
        target.ItemUI.CoverImage.gameObject.SetActive(true);
        MapUIGroup.blocksRaycasts = MapUIGroup.interactable = true;

        TargetSongAnim = null;
    }


    public void ChangeDiff(SongSelectDifficulty target)
    {
        if (IsAnimating || TargetDifficulty == target) return;
        SelectedDifficulty = target.Chart.DifficultyIndex;
        StartCoroutine(ChangeDiffAnim(target));
    }
    public IEnumerator ChangeDiffAnim(SongSelectDifficulty target)
    {
        IsAnimating = true;

        SongSelectDifficulty oldTarget = TargetDifficulty;
        TargetDifficulty = target;

        foreach (var item in ListView.ItemList)
        {
            item.SetDifficulty(GetNearestDifficulty(item.Song.Charts));
        }

        SetScoreInfo(target);

        StartCoroutine(Ease.Animate(.1f, a => {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            oldTarget.SetSelectability(1 - lerp);
            target.SetSelectability(lerp);
            rt(oldTarget.Holder).anchoredPosition = new(0, 5 * (1 - lerp));
        }));
        yield return Ease.Animate(.15f, a => {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            rt(target.Holder).anchoredPosition = new(0, 7 - 2 * lerp);
            rt(target.Holder).localEulerAngles = 10 * (1 - lerp) * Vector3.back;
        });
        IsAnimating = false;

    }

    IEnumerator NavUpdateAnim()
    {
        yield return Ease.Animate(0.2f, (x) =>
        {
            float ease1 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            LerpActions(1 - ease1);
        });
        UpdateButtons();
        yield return Ease.Animate(0.2f, (x) =>
        {
            float ease1 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            LerpActions(ease1);
        });
    }

    public void SetScoreInfo(SongSelectDifficulty target)
    {
        TargetDifficultyName.text = target.Chart.DifficultyName;
        TargetDifficultyNewIndicator.SetActive(target.Record == null);
        TargetDifficultyScore.text = Helper.PadScore((target.Record?.Score ?? 0).ToString("#0"))
            + "<size=60%><b>ppm";
    }

    float coverLerp = 0;
    public void LerpCoverList(float a) 
    {
        coverLerp = a;
        float scrollOfs = Mathf.Clamp(ListView.ScrollOffset, ListView.ItemList[0].Position - 20, ListView.ItemList[^1].Position + 20);
        float offset = scrollOfs - ListView.TargetSongOffset;
        TargetSongCoverHolder.anchorMin = Vector2.Lerp(new(0, .5f), new(0, .5f), a);
        TargetSongCoverHolder.anchorMax = Vector2.Lerp(new(0, .5f), new(1, .5f), a);
        TargetSongCoverHolder.anchoredPosition = Vector2.Lerp(new(180 + .26795f * offset, offset), new(0, offset / 2), a);
        TargetSongCoverHolder.sizeDelta = Vector2.Lerp(new(36, 36), new(4 - SafeAreaHolder.sizeDelta.x, 128), a);
        TargetSongCoverShadow.effectDistance = new Vector2(0, -2) * a;
        TargetSongCoverLayerHolder.anchoredPosition = new (-4 * a, 0);
        UpdateCover();
    }
    
    public void LerpCover(float a, RectTransform icon) 
    {
        coverLerp = a;
        TargetSongCoverHolder.anchorMin = Vector2.Lerp(new(0, .5f), new(0, .5f), a);
        TargetSongCoverHolder.anchorMax = Vector2.Lerp(new(0, .5f), new(1, .5f), a);
        TargetSongCoverHolder.anchoredPosition = Vector2.zero;
        TargetSongCoverHolder.position = Vector2.Lerp(icon.position, TargetSongCoverHolder.position, a);
        TargetSongCoverHolder.sizeDelta = Vector2.Lerp(new(36, 36), new(4 - SafeAreaHolder.sizeDelta.x, 128), a);
        TargetSongCoverShadow.effectDistance = new Vector2(0, -2) * a;
        TargetSongCoverLayerHolder.anchoredPosition = new (-4 * a, 0);
        if (CurrentCover != null) UpdateCover();
    }

    public void UpdateCover()
    {
        Vector2 parallaxOffset = CurrentCover.IconCenter * (1 - coverLerp);

        float coverScale = TargetSongCoverLayerHolder.rect.width
            / Mathf.Lerp(CurrentCover.IconSize, 880, coverLerp);

        foreach (var layer in TargetSongCoverLayers)
        {
            RawImage image = layer.Image;

            Vector2 position = layer.Layer.Position + parallaxOffset * layer.Layer.ParallaxFactor;
            Vector2 size = 880 * layer.Layer.Scale * new Vector2(1, (float)image.texture.height / image.texture.width);

            if (layer.Layer.Tiling)
            {
                Vector2 coverSize = TargetSongCoverLayerHolder.rect.size;
                image.rectTransform.sizeDelta = coverSize;
                image.rectTransform.anchoredPosition = Vector2.zero;
                image.rectTransform.localScale = Vector3.one;
                image.uvRect = new Rect(
                    ((size - coverSize / coverScale) / 2 - position) / size,
                    coverSize / coverScale / size
                );
            }
            else
            {
                image.rectTransform.sizeDelta = size;
                image.rectTransform.anchoredPosition = position * coverScale;
                image.rectTransform.localScale = Vector3.one * coverScale;
                image.uvRect = new Rect(0, 0, 1, 1);
            }
        }
    }

    public void Launch() 
    {
        if (TargetSongAnim != null) StopCoroutine(TargetSongAnim);
        StartCoroutine(LaunchAnim());
    }
    public void Return()
    {
        if (TargetMapItem is SongMapItem)
        {
            if (TargetSongAnim != null) StopCoroutine(TargetSongAnim);
            TargetSongAnim = StartCoroutine(MapTargetSongHideAnim());
        }
    }

    public IEnumerator LaunchAnim()
    {
        IsAnimating = true;

        string[] launchTextList = new[]{
            "LET'S GO",
            "LET'S DO THIS",
            "HERE WE GO",
            "NOW LAUNCHING",
            "NOW APPROACHING",
            "NEXT DESTINATION",
        };
        LaunchText.text = launchTextList[Random.Range(0, launchTextList.Length)];

        LerpInfo(0);

        TargetSongCoverLayerHolder.gameObject.SetActive(false);
        TargetSongCoverBackground.color = TargetSong.BackgroundColor;

        float currentCameraZ = 0;

        yield return Ease.Animate(0.8f, a =>
        {
            float lerp = Ease.Get(a * 5, EaseFunction.Cubic, EaseMode.Out);
            LerpUI(1 - lerp);
            if (!IsMapView) ListView.LerpListView(1 - lerp);

            float lerp2 = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            float targetCameraZ = lerp2 * 5;
            Common.main.MainCamera.transform.Translate(Vector3.back * (targetCameraZ - currentCameraZ));
            currentCameraZ = targetCameraZ;

            float lerp3 = Mathf.Pow(Ease.Get(a, EaseFunction.Circle, EaseMode.In), 2);
            float scrollOfs = Mathf.Clamp(ListView.ScrollOffset, ListView.ItemList[0].Position - 20, ListView.ItemList[^1].Position + 20);
            float offset = scrollOfs - ListView.TargetSongOffset;
            TargetSongCoverHolder.anchorMin = new(0, .5f * (1 - lerp3));
            TargetSongCoverHolder.anchorMax = new(1, 1 - .5f * (1 - lerp3));
            TargetSongCoverHolder.anchoredPosition = new(0, offset / 2 * (1 - lerp3));
            TargetSongCoverHolder.sizeDelta *= new Vector2Frag(y: (128 - Mathf.Pow(40 * lerp2, 0.5f)) * (1 - lerp3));
            Common.main.MainCamera.fieldOfView = 60 + 60 * lerp3;
            ListView.IsDirty = true;

            float lerp4 = Mathf.Pow(Ease.Get(a, EaseFunction.Exponential, EaseMode.Out), 0.5f);
            rt(LaunchTextHolder).sizeDelta = new(LaunchText.preferredWidth * lerp4, rt(LaunchTextHolder).sizeDelta.y);
            LaunchTextHolder.alpha = Random.Range(1, 2f) - lerp3 * 2;
            TargetSongCoverFlash.color = new(1, 1, 1, 1 - lerp4);
        });

        PlayerScreen.TargetSongPath = $"Songs/{TargetSongID}/{TargetSongID}";
        PlayerScreen.TargetSong = TargetSong;
        PlayerScreen.TargetChartMeta = TargetDifficulty.Chart;
        Common.main.MainCamera.backgroundColor = TargetSong.BackgroundColor;
        ReadyScreen.BeginLaunch();

        yield return new WaitForSeconds(2);

        Common.Load("Player", () => PlayerScreen.main && PlayerScreen.main.IsReady, () =>
        {
            SongSelectReadyScreen.main.EndLaunch();
        }, false);
        SceneManager.UnloadSceneAsync(MapManager.MapScene);
        SceneManager.UnloadSceneAsync("Song Select");
        Resources.UnloadUnusedAssets();
    }

    public void LerpInfo(float a)
    {
        TargetSongInfoHolder.alpha = a * a;
        TargetSongInfoHolder.blocksRaycasts = a == 1;
        rt(TargetSongInfoHolder).anchoredPosition = new(50 + 10 * a, rt(TargetSongInfoHolder).anchoredPosition.y);
    }

    public void LerpUI(float a)
    {
        LerpActions(a);
        LerpDifficulty(a);
        if (!QuickMenu.main || !QuickMenu.main.gameObject.activeSelf) ProfileBar.main.SetVisibility(a);
    }

    public void LerpActions(float a)
    {
        LeftActionsHolder.alpha = RightActionsHolder.alpha = a * a;
        LeftActionsHolder.blocksRaycasts = RightActionsHolder.blocksRaycasts = a == 1;
        rt(LeftActionsHolder).anchoredPosition = new (-10 * (1 - a), rt(LeftActionsHolder).anchoredPosition.y);
        rt(RightActionsHolder).anchoredPosition = new (10 * (1 - a), rt(RightActionsHolder).anchoredPosition.y);
    }

    public void LerpDifficulty(float a)
    {
        DifficultyHolder.alpha = a * a;
        DifficultyHolder.blocksRaycasts = a == 1;
        rt(DifficultyHolder).anchoredPosition = new (10 * (1 - a), rt(DifficultyHolder).anchoredPosition.y);
    }


    public void Intro()
    {
        if (!IsAnimating) StartCoroutine(IntroAnim());
    }

    public IEnumerator IntroAnim() 
    {
        IsAnimating = true;

        ListView.ScrollOffset = Screen.height / 2 / Common.main.CommonCanvas.localScale.x;
        UpdateListItems(false);
        UpdateButtons();

        Transform cameraTransform = Common.main.MainCamera.transform;

        cameraTransform.rotation = Quaternion.identity;
        cameraTransform.position = Vector3.zero;
        cameraTransform.position *= new Vector3Frag(z: -100);

        yield return StartCoroutine(Ease.Animate(1, x =>
        {
            float lerp1 = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
            cameraTransform.position *= new Vector3Frag(z: -100 + 90 * lerp1);
            Common.main.MainCamera.fieldOfView = 120 - 60 * lerp1;
            MapCover.color *= new ColorFrag(a: 1 - lerp1);

            if (IsMapView)
            {
                MapUIGroup.alpha = x * 1e10f;
                MapUIGroup.transform.localScale = Vector3.one * lerp1;
                MapManager.UpdateAllPositions();
            }
            else
            {
                ListView.ItemGroup.alpha = x * 1e10f;
                float xPos = 120 + 60 * lerp1;
                ListView.ItemTrack.color = new(1, 1, 1, Mathf.Clamp01(x * 3));
                ListView.BackgroundGroup.alpha = Mathf.Clamp01(x * 3 - 1);
                ListView.ItemTrack.rectTransform.anchoredPosition = new(xPos - 180, ListView.ItemTrack.rectTransform.anchoredPosition.y);
                ListView.BackgroundHolder.anchoredPosition = new(xPos, ListView.BackgroundHolder.anchoredPosition.y);
            }


            ListView.ScrollOffset = Screen.height / 2 / Common.main.CommonCanvas.localScale.x * (Ease.Get(x, EaseFunction.Exponential, EaseMode.Out) - 1);
            UpdateListItems(false);
            if (!IsReady && x > 0.6f)
            {
                IsReady = true;
                if (IsMapView) StartCoroutine(IntroShowMapUIAnim());
                else ListView.ItemGroup.interactable = ListView.ItemGroup.blocksRaycasts = ListView.IsTargetSongHidden = true;
            }
        }));

        IsAnimating = false;
    }

    IEnumerator IntroShowMapUIAnim()
    {
        yield return Ease.Animate(.2f, (x) =>
        {
            float ease1 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            LerpActions(ease1);
            ProfileBar.main.SetVisibility(ease1);
        });
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;
}