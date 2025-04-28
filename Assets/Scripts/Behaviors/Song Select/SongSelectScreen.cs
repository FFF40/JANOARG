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
using Unity.VisualScripting;

public class SongSelectScreen : MonoBehaviour
{
    public static SongSelectScreen main;

    public Playlist Playlist;
    public List<PlayableSong> SongList { get; private set; } = new();
    [Space]
    public SongSelectItem ItemSample;
    public RectTransform ItemHolder;
    public CanvasGroup ItemGroup;
    public List<SongSelectItem> ItemList { get; private set; } = new();
    public Graphic ItemTrack;
    public Graphic ItemCursor;
    [Space]
    public RectTransform BackgroundHolder;
    public CanvasGroup BackgroundGroup;
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
    [Space]
    public CanvasGroup DifficultyHolder;
    public RectTransform DifficultyListHolder;
    public SongSelectDifficulty DifficultySample;
    public List<SongSelectDifficulty> DifficultyList { get; private set; } = new();

    public int SelectedDifficulty;
    public SongSelectDifficulty TargetDifficulty;
    public TMP_Text TargetDifficultyName;
    public TMP_Text TargetDifficultyScore;
    public GameObject TargetDifficultyNewIndicator;
    [Space]
    public CanvasGroup LeftActionsHolder;
    public CanvasGroup RightActionsHolder;
    [Space]
    public CanvasGroup LaunchTextHolder;
    public TMP_Text LaunchText;
    [Space]
    public SongSelectReadyScreen ReadyScreen;
    [Space]
    public float ScrollOffset;
    public float ScrollVelocity;
    public float TargetScrollOffset;
    float lastScrollOffset;
    public bool IsReady;
    public bool IsDirty;
    public bool IsPointerDown;
    public bool IsTargetSongHidden;
    public bool TargetSongHiddenTarget;
    public float TargetSongOffset;
    public bool IsAnimating;
    public bool IsInit;
    [Space]
    public AudioSource PreviewSource;
    public float PreviewVolume;
    public float PreviewVolumeMulti;
    public AudioClip CurrentPreviewClip;
    public Vector2 CurrentPreviewRange;
    [Space]
    public AudioSource SFXSource;
    public AudioClip SFXTickClip;
    public float SFXVolume;

    public Coroutine TargetSongAnim;

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

        if (IsPointerDown) 
        {
            ScrollVelocity = (lastScrollOffset - ScrollOffset) / Time.deltaTime;
            lastScrollOffset = ScrollOffset;
        }
        else 
        {
            if (Mathf.Abs(ScrollVelocity) > 10)
            {
                ScrollOffset -= ScrollVelocity * Time.deltaTime;
                ScrollVelocity *= Mathf.Pow(0.2f, Time.deltaTime);

                float minBound = ItemList[0].Position - 20; 
                float maxBound = ItemList[^1].Position + 20;
                if (ScrollOffset < minBound) 
                {
                    ScrollOffset = minBound;
                    ScrollVelocity = 0;
                }
                else if (ScrollOffset > maxBound) 
                {
                    ScrollOffset = maxBound;
                    ScrollVelocity = 0;
                }

                UpdateTarget();
                IsDirty = true;
            }
            else 
            {
                if (Mathf.Abs(ScrollOffset - TargetScrollOffset) > .1f) 
                {
                    ScrollOffset = Mathf.Lerp(ScrollOffset, TargetScrollOffset, 1 - Mathf.Pow(.001f, Time.deltaTime));
                    IsDirty = true;
                }
                TargetSongHiddenTarget = false;
            }
        }

        if (TargetSongHiddenTarget) 
        {
            ItemCursor.color += new Color(0, 0, 0, (1 - ItemCursor.color.a) * Mathf.Pow(5e-3f, Time.deltaTime));
        }
        else 
        {
            ItemCursor.color *= new Color(1, 1, 1, Mathf.Pow(.001f, Time.deltaTime));
        }

        if (!IsAnimating && TargetSongHiddenTarget != IsTargetSongHidden) 
        {
            IsTargetSongHidden = TargetSongHiddenTarget;
            if (TargetSongAnim != null) StopCoroutine(TargetSongAnim);
            if (IsTargetSongHidden) TargetSongAnim = StartCoroutine(TargetSongHideAnim());
            else TargetSongAnim = StartCoroutine(TargetSongShowAnim());
        }
        if (IsDirty) 
        {
            UpdateItems();
            IsDirty = false;
        }

        float previewVolumeSpeed = 1;
        if (ReadyScreen.IsAnimating) previewVolumeSpeed = -0.5f;
        else if (CurrentPreviewClip != PreviewSource.clip) previewVolumeSpeed = -2;
        else if (!CurrentPreviewClip) previewVolumeSpeed = -2;
        else if (CurrentPreviewClip.loadState != AudioDataLoadState.Loaded) previewVolumeSpeed = -1;
        else if (CurrentPreviewRange.y - PreviewSource.time <= PreviewVolume) previewVolumeSpeed = -1;
        else if (IsTargetSongHidden) previewVolumeSpeed = -0.4f;
        PreviewVolume = Mathf.Clamp01(PreviewVolume + previewVolumeSpeed * Time.deltaTime);
        PreviewSource.volume = PreviewVolume * PreviewVolumeMulti;
        if (PreviewVolume <= 0) 
        {
            if (CurrentPreviewClip != PreviewSource.clip)
            {
                if (!PreviewSource.clip) 
                {
                    PreviewSource.clip = CurrentPreviewClip;
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
        int index = 0;
        int pos = 0;
        foreach (string path in Playlist.ItemPaths)
        {
            ResourceRequest req = Resources.LoadAsync<ExternalPlayableSong>(path);
            yield return new WaitUntil(() => req.isDone);
            if (!req.asset) 
            {
                Debug.LogWarning("Couldn't load Playable Song at " + path);
                continue;
            }
            PlayableSong song = ((ExternalPlayableSong)req.asset).Data;
            SongList.Add(song);

            SongSelectItem item = Instantiate(ItemSample, ItemHolder);
            item.SetItem(path, song, index, pos);
            ItemList.Add(item);
            index++;
            pos += 48;
            
        }
        IsInit = true;
        
        if (!LoadingBar.main.gameObject.activeSelf) Intro();
    }

    public void UpdateItems(bool cap = true)
    {
        float scrollOfs = ScrollOffset;
        if (cap) scrollOfs = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);

        foreach (SongSelectItem item in ItemList)
        {
            RectTransform rt = (RectTransform)item.transform;
            rt.anchoredPosition = new Vector2(-.26795f, -1) * (item.Position + item.PositionOffset - scrollOfs);
        }
        ItemCursor.rectTransform.anchoredPosition = new(ItemCursor.rectTransform.anchoredPosition.x, scrollOfs - TargetScrollOffset);
        if (!IsAnimating && TargetSongAnim == null)
        {
            float offset = scrollOfs - TargetSongOffset;
            TargetSongCoverHolder.anchoredPosition = new(0, offset / 2);
        }
    }

    public void UpdateTarget() 
    {
        float lastTarget = TargetScrollOffset;
        float tsDist = float.PositiveInfinity;
        foreach (SongSelectItem item in ItemList)
        {
            if (Mathf.Abs(item.Position - ScrollOffset) < tsDist) 
            {
                TargetScrollOffset = item.Position;
                tsDist = Mathf.Abs(item.Position - ScrollOffset);
            }
            else break;
        }
        if (lastTarget != TargetScrollOffset) 
        {
            TargetSongHiddenTarget = true;
            SFXSource.PlayOneShot(SFXTickClip, SFXVolume);
        }
    }

    public void OnListPointerDown(BaseEventData data) 
    {
        lastScrollOffset = ScrollOffset;
        ScrollVelocity = 0;
        IsPointerDown = true;
    }

    public void OnListDrag(BaseEventData data) 
    {
        ScrollOffset += ((PointerEventData)data).delta.y / transform.lossyScale.x;
        UpdateTarget();
        IsDirty = true;
    }

    public void OnListPointerUp(BaseEventData data) 
    {
        if (ItemList.Count < 0) ScrollOffset = Mathf.Clamp(ScrollOffset, -20, 20);
        else ScrollOffset = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);
        IsPointerDown = false;
    }

    public IEnumerator TargetSongShowAnim()
    {
        TargetSongOffset = TargetScrollOffset;
        SongSelectItem TargetSong = ItemList.Find(item => TargetScrollOffset == item.Position);
        if (TargetSong)
        {
            TargetSongInfoName.text = TargetSong.Song.SongName;
            TargetSongInfoArtist.text = TargetSong.Song.SongArtist;
            
            float songLength = TargetSong.Song.Clip.length;
            TargetSongInfoInfo.text = Mathf.Floor(songLength / 60) + "m " + Mathf.Floor(songLength % 60) + "s";

            float minBPM = float.PositiveInfinity, maxBPM = float.NegativeInfinity;
            foreach (BPMStop stop in TargetSong.Song.Timing.Stops)
            {
                if (stop.Significant)
                {
                    minBPM = Mathf.Min(minBPM, stop.BPM);
                    maxBPM = Mathf.Max(maxBPM, stop.BPM);
                }
            }
            TargetSongInfoInfo.text += " - BPM " + (minBPM == maxBPM ? minBPM : minBPM + "~" + maxBPM);

            TargetSongInfoInfo.text += " - " + TargetSong.Song.Genre.ToUpper();
        }

        CurrentPreviewClip = TargetSong.Song.Clip;
        CurrentPreviewRange = TargetSong.Song.PreviewRange;

        string songPath = Playlist.ItemPaths[SongList.IndexOf(TargetSong.Song)];
        string songID = Path.GetFileNameWithoutExtension(songPath);
        foreach (SongSelectDifficulty diff in DifficultyList) Destroy(diff.gameObject);
        DifficultyList.Clear();
        var target = GetNearestDifficulty(TargetSong.Song.Charts);
        foreach (ExternalChartMeta chart in TargetSong.Song.Charts) 
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

        TargetSongCoverBackground.color = TargetSong.Song.Cover.BackgroundColor;
        TargetSongCoverHolder.gameObject.SetActive(false);
        if (CurrentCover != TargetSong.Song.Cover) 
        {
            CurrentCover = TargetSong.Song.Cover;
            foreach (var layer in TargetSongCoverLayers) 
            {
                Resources.UnloadAsset(layer.Image.texture);
                Destroy(layer.gameObject);
            }
            TargetSongCoverLayers.Clear();
            foreach (CoverLayer layer in CurrentCover.Layers)
            {
                string path = Path.Combine(Path.GetDirectoryName(TargetSong.SongPath), layer.Target);
                if (Path.HasExtension(path)) path = Path.ChangeExtension(path, "")[0..^1];
                var req = Resources.LoadAsync<Texture2D>(path);
                yield return new WaitUntil(() => req.isDone);
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

        yield return Ease.Animate(.9f, a => {
            float lerp = Ease.Get(a * 3 - 1, EaseFunction.Cubic, EaseMode.Out);
            LerpInfo(lerp);

            float lerp2 = Ease.Get(a * 1.5f, EaseFunction.Exponential, EaseMode.InOut)
                * Ease.Get(a, EaseFunction.Exponential, EaseMode.Out);
            LerpCover(lerp2);

            float lerp3 = Ease.Get(a * 3, EaseFunction.Cubic, EaseMode.Out);
            LerpUI(lerp3);
            
            float lerp4 = Ease.Get(a * 1.5f, EaseFunction.Circle, EaseMode.Out);
            foreach (SongSelectItem item in ItemList)
            {
                item.PositionOffset = 45 * Mathf.Clamp(item.Position - TargetSongOffset, -1, 1)
                     * (lerp2 * .5f + lerp4 * .5f);
            }
            IsDirty = true;
        });

        if (TargetSongOffset != TargetScrollOffset) 
        {
            IsTargetSongHidden = true;
            StartCoroutine(TargetSongHideAnim());
        }

        TargetSongAnim = null;
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

    public IEnumerator TargetSongHideAnim()
    {
        float lerpCoverStart = coverLerp;
        yield return Ease.Animate(.3f, a => {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            LerpInfo(1 - lerp); 
            LerpUI(1 - lerp);

            float lerp2 = Ease.Get(a, EaseFunction.Quintic, EaseMode.Out);
            LerpCover((1 - lerp2) * lerpCoverStart);
            foreach (SongSelectItem item in ItemList)
            {
                item.PositionOffset = 45 * (1 - lerp2) * Mathf.Clamp(item.Position - TargetSongOffset, -1, 1);
            }
            IsDirty = true;
        });
        TargetSongCoverHolder.gameObject.SetActive(false);

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

        foreach (var item in ItemList) 
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

    public void SetScoreInfo(SongSelectDifficulty target)
    {
        TargetDifficultyName.text = target.Chart.DifficultyName;
        TargetDifficultyNewIndicator.SetActive(target.Record == null);
        TargetDifficultyScore.text = Helper.PadScore((target.Record?.Score ?? 0).ToString("#0"))
            + "<size=60%><b>ppm";
    }

    float coverLerp = 0;
    public void LerpCover(float a) 
    {
        coverLerp = a;
        float scrollOfs = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);
        float offset = scrollOfs - TargetSongOffset;
        TargetSongCoverHolder.anchorMin = Vector2.Lerp(new(0, .5f), new(0, .5f), a);
        TargetSongCoverHolder.anchorMax = Vector2.Lerp(new(0, .5f), new(1, .5f), a);
        TargetSongCoverHolder.anchoredPosition = Vector2.Lerp(new(180 + .26795f * offset, offset), new(0, offset / 2), a);
        TargetSongCoverHolder.sizeDelta = Vector2.Lerp(new(36, 36), new(4 - SafeAreaHolder.sizeDelta.x, 128), a);
        TargetSongCoverShadow.effectDistance = new Vector2(0, -2) * a;
        TargetSongCoverLayerHolder.anchoredPosition = new (-4 * a, 0);
        UpdateCover();
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
                image.uvRect = new Rect (
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

    public IEnumerator LaunchAnim()
    {
        IsAnimating = true;

        string[] launchTextList = new []{
            "LET'S GO",
            "LET'S DO THIS",
            "HERE WE GO",
            "NOW LAUNCHING",
            "NOW APPROACHING",
            "NEXT DESTINATION",
        };
        LaunchText.text = launchTextList[Random.Range(0, launchTextList.Length)];
        
        LerpInfo(0);

        SongSelectItem TargetSong = ItemList.Find(item => TargetScrollOffset == item.Position);
        TargetSongCoverLayerHolder.gameObject.SetActive(false);
        TargetSongCoverBackground.color = TargetSong.Song.BackgroundColor;

        yield return Ease.Animate(0.8f, a => {
            float lerp = Ease.Get(a * 5, EaseFunction.Cubic, EaseMode.Out);
            LerpUI(1 - lerp);
            foreach (SongSelectItem item in ItemList) item.SetVisibilty(1 - lerp);

            float lerp2 = Mathf.Pow(Ease.Get(a, EaseFunction.Circle, EaseMode.In), 2);
            float scrollOfs = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);
            float offset = scrollOfs - TargetSongOffset;
            TargetSongCoverHolder.anchorMin = new(0, .5f * (1 - lerp2));
            TargetSongCoverHolder.anchorMax = new(1, 1 - .5f * (1 - lerp2));
            TargetSongCoverHolder.anchoredPosition = new(0, offset / 2 * (1 - lerp2));
            TargetSongCoverHolder.sizeDelta *= new Vector2Frag(y: 128 * (1 - lerp2));
            IsDirty = true;
            
            float lerp3 = Mathf.Pow(Ease.Get(a, EaseFunction.Exponential, EaseMode.Out), 0.5f);
            rt(LaunchTextHolder).sizeDelta = new(LaunchText.preferredWidth * lerp3, rt(LaunchTextHolder).sizeDelta.y);
            LaunchTextHolder.alpha = Random.Range(1, 2f) - lerp2 * 2;
            TargetSongCoverFlash.color = new (1, 1, 1, 1 - lerp3);
        });

        PlayerScreen.TargetSongPath = Playlist.ItemPaths[SongList.IndexOf(TargetSong.Song)];
        PlayerScreen.TargetSong = TargetSong.Song;
        PlayerScreen.TargetChartMeta = TargetDifficulty.Chart;
        Common.main.MainCamera.backgroundColor = TargetSong.Song.BackgroundColor;
        ReadyScreen.BeginLaunch();

        yield return new WaitForSeconds(2);

        Common.Load("Player", () => PlayerScreen.main && PlayerScreen.main.IsReady, () => {
            SongSelectReadyScreen.main.EndLaunch();
        }, false);
        SceneManager.UnloadSceneAsync("Song Select");
        Resources.UnloadUnusedAssets();
    }

    public void LerpInfo(float a)
    {
        TargetSongInfoHolder.alpha = a * a;
        TargetSongInfoHolder.blocksRaycasts = a == 1;
        rt(TargetSongInfoHolder).anchoredPosition = new (50 + 10 * a, rt(TargetSongInfoHolder).anchoredPosition.y);
    }

    public void LerpUI(float a)
    {
        LeftActionsHolder.alpha = RightActionsHolder.alpha = DifficultyHolder.alpha = a * a;
        LeftActionsHolder.blocksRaycasts = RightActionsHolder.blocksRaycasts = DifficultyHolder.blocksRaycasts = a == 1;
        rt(LeftActionsHolder).anchoredPosition = new (-10 * (1 - a), rt(LeftActionsHolder).anchoredPosition.y);
        rt(RightActionsHolder).anchoredPosition = new (10 * (1 - a), rt(RightActionsHolder).anchoredPosition.y);
        rt(DifficultyHolder).anchoredPosition = new (10 * (1 - a), rt(DifficultyHolder).anchoredPosition.y);
        if (!QuickMenu.main || !QuickMenu.main.gameObject.activeSelf) ProfileBar.main.SetVisibilty(a);
    }

    public void Intro()
    {
        if (!IsAnimating) StartCoroutine(IntroAnim());
    }

    public IEnumerator IntroAnim() 
    {
        IsAnimating = true;

        ScrollOffset = Screen.height / 2 / Common.main.CommonCanvas.localScale.x;
        UpdateItems(false);

        yield return StartCoroutine(Ease.Animate(1, x => {
            ItemGroup.alpha = x * 1e10f;
            float xPos = 120 + 60 * Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
            ItemTrack.color = new (1, 1, 1, Mathf.Clamp01(x * 3));
            BackgroundGroup.alpha = Mathf.Clamp01(x * 3 - 1);
            ItemTrack.rectTransform.anchoredPosition = new (xPos - 180, ItemTrack.rectTransform.anchoredPosition.y);
            BackgroundHolder.anchoredPosition = new (xPos, BackgroundHolder.anchoredPosition.y);

            ScrollOffset = Screen.height / 2 / Common.main.CommonCanvas.localScale.x * (Ease.Get(x, EaseFunction.Exponential, EaseMode.Out) - 1);
            UpdateItems(false);
            IsReady = x > 0.6f;
        }));

        IsAnimating = false;
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;
}