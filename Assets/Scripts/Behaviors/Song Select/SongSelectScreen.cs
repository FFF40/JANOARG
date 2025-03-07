using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
    [Space]
    public CanvasGroup TargetSongInfoHolder;
    public TMP_Text TargetSongInfoName;
    public TMP_Text TargetSongInfoArtist;
    public TMP_Text TargetSongInfoInfo;
    public RectTransform TargetSongCoverHolder;
    public Image TargetSongCoverBackground;
    public Image TargetSongCoverFlash;
    [Space]
    public CanvasGroup DifficultyHolder;
    public RectTransform DifficultyListHolder;
    public SongSelectDifficulty DifficultySample;
    public List<SongSelectDifficulty> DifficultyList { get; private set; } = new();
    public int SelectedDifficulty;
    public SongSelectDifficulty TargetDifficulty;
    [Space]
    public CanvasGroup LeftActionsHolder;
    public CanvasGroup RightActionsHolder;
    [Space]
    public CanvasGroup LaunchTextHolder;
    public TMP_Text LaunchText;
    [Space]
    public SongSelectReadyScreen ReadyScreen;
    public ScoreManager ScoreManager;
    [Space]
    public float ScrollOffset;
    public float TargetScrollOffset;
    public bool IsReady;
    public bool IsDirty;
    public bool IsPointerDown;
    public bool IsTargetSongHidden;
    public bool TargetSongHiddenTarget;
    public float TargetSongOffset;
    public bool IsAnimating;
    public bool IsInit;

    public bool IsPlaylistInitialized { get; private set; } = false;
    private Coroutine initPlaylistCoroutine = null;

    public void Awake()
    {
        main = this;
        CommonScene.Load();
    }

    public void Start()
    {
        if (!IsPlaylistInitialized && initPlaylistCoroutine == null)
        {
           initPlaylistCoroutine = StartCoroutine(InitPlaylist());
        }
        
    }

    public void Update() 
    {
        if (!IsReady) return;

        if (IsPointerDown) 
        {
            ItemCursor.color += new Color(0, 0, 0, (1 - ItemCursor.color.a) * Mathf.Pow(5e-3f, Time.deltaTime));
        }
        else 
        {
            ItemCursor.color *= new Color(1, 1, 1, Mathf.Pow(.001f, Time.deltaTime));
            if (Mathf.Abs(ScrollOffset - TargetScrollOffset) > .1f) 
            {
                ScrollOffset = Mathf.Lerp(ScrollOffset, TargetScrollOffset, 1 - Mathf.Pow(.001f, Time.deltaTime));
                IsDirty = true;
            }
            TargetSongHiddenTarget = false;
        }

        if (!IsAnimating && TargetSongHiddenTarget != IsTargetSongHidden) 
        {
            IsTargetSongHidden = TargetSongHiddenTarget;
            if (IsTargetSongHidden) StartCoroutine(TargetSongHideAnim());
            else StartCoroutine(TargetSongShowAnim());
        }
        if (IsDirty) 
        {
            UpdateItems();
            IsDirty = false;
        }
    }

    public IEnumerator InitPlaylist()
    {
        if (IsPlaylistInitialized) yield break;

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
            item.SetItem(song, index, pos);
            ItemList.Add(item);
            index++;
            pos += 48;
            
        }
        IsInit = true;
        IsPlaylistInitialized = true;
        initPlaylistCoroutine = null;
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
        if (!IsAnimating)
        {
            float offset = scrollOfs - TargetSongOffset;
            TargetSongCoverHolder.anchoredPosition = new(0, offset / 2);
        }
    }

    public void OnListPointerDown(BaseEventData data) 
    {
        IsPointerDown = true;
    }

    public void OnListDrag(BaseEventData data) 
    {
        ScrollOffset += ((PointerEventData)data).delta.y / transform.lossyScale.x;
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
        if (lastTarget != TargetScrollOffset) TargetSongHiddenTarget = true;
        IsDirty = true;
    }

    public void OnListPointerUp(BaseEventData data) 
    {
        ScrollOffset = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);
        IsPointerDown = false;
    }

    public IEnumerator TargetSongShowAnim()
    {
        IsAnimating = true;

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
        TargetSongCoverHolder.gameObject.SetActive(true);

        foreach (SongSelectDifficulty diff in DifficultyList) Destroy(diff.gameObject);
        DifficultyList.Clear();
        int dist = int.MaxValue;
        foreach (ExternalChartMeta chart in TargetSong.Song.Charts) 
        {
            SongSelectDifficulty diff = Instantiate(DifficultySample, DifficultyListHolder);
            diff.SetItem(chart);
            diff.Button.onClick.AddListener(() => ChangeDiff(diff));
            DifficultyList.Add(diff);
            int chartDiff = Mathf.Abs((chart.DifficultyIndex - SelectedDifficulty + 100) % 100);
            if (dist > chartDiff)
            {
                dist = chartDiff;
                TargetDifficulty = diff;
            }
        }
        TargetDifficulty.SetSelectability(1);

        yield return Ease.Animate(.6f, a => {
            float lerp = Ease.Get(a * 2 - 1, EaseFunction.Cubic, EaseMode.Out);
            LerpInfo(lerp);

            float lerp3 = Ease.Get(a * 2, EaseFunction.Cubic, EaseMode.Out);
            LerpUI(lerp3);

            float lerp2 = Ease.Get(a, EaseFunction.Exponential, EaseMode.Out);
            LerpCover(lerp2);
            foreach (SongSelectItem item in ItemList)
            {
                item.PositionOffset = 45 * lerp2 * Mathf.Clamp(item.Position - TargetSongOffset, -1, 1);
            }
            IsDirty = true;
        });
        IsAnimating = false;

        if (TargetSongOffset != TargetScrollOffset) 
        {
            IsTargetSongHidden = true;
            StartCoroutine(TargetSongHideAnim());
        }
    }

    public IEnumerator TargetSongHideAnim()
    {
        IsAnimating = true;
        yield return Ease.Animate(.2f, a => {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            LerpInfo(1 - lerp); 
            LerpUI(1 - lerp);

            float lerp2 = Ease.Get(a, EaseFunction.Quintic, EaseMode.Out);
            LerpCover(1 - lerp2);
            foreach (SongSelectItem item in ItemList)
            {
                item.PositionOffset = 45 * (1 - lerp2) * Mathf.Clamp(item.Position - TargetSongOffset, -1, 1);
            }
            IsDirty = true;
        });
        TargetSongCoverHolder.gameObject.SetActive(false);
        IsAnimating = false;
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

        yield return Ease.Animate(.15f, a => {
            float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
            oldTarget.SetSelectability(1 - lerp);
            target.SetSelectability(lerp);
            LayoutRebuilder.MarkLayoutForRebuild(DifficultyListHolder);
        });
        IsAnimating = false;

    }

    public void LerpCover(float a) 
    {
        float scrollOfs = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);
        float offset = scrollOfs - TargetSongOffset;
        TargetSongCoverHolder.anchorMin = Vector2.Lerp(new(0, .5f), new(0, .5f), a);
        TargetSongCoverHolder.anchorMax = Vector2.Lerp(new(0, .5f), new(1, .5f), a);
        TargetSongCoverHolder.anchoredPosition = Vector2.Lerp(new(180 + .26795f * offset, offset), new(0, offset / 2), a);
        TargetSongCoverHolder.sizeDelta = Vector2.Lerp(new(36, 36), new(0, 128), a);
    }

    public void Launch() 
    {
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
            "YOU ARE ABOUT TO EXPERIENCE",
            "YOU ARE NOW EXPERIENCING",
            "NEXT DESTINATION",
        };
        LaunchText.text = launchTextList[Random.Range(0, launchTextList.Length)];
        
        LerpInfo(0);

        SongSelectItem TargetSong = ItemList.Find(item => TargetScrollOffset == item.Position);
        TargetSongCoverBackground.color = TargetSong.Song.BackgroundColor;

        yield return Ease.Animate(1, a => {
            float lerp = Ease.Get(a * 5, EaseFunction.Cubic, EaseMode.Out);
            LerpUI(1 - lerp);
            foreach (SongSelectItem item in ItemList) item.SetVisibilty(1 - lerp);

            float lerp2 = Mathf.Pow(Ease.Get(a, EaseFunction.Circle, EaseMode.In), 2);
            float scrollOfs = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);
            float offset = scrollOfs - TargetSongOffset;
            TargetSongCoverHolder.anchorMin = new(0, .5f * (1 - lerp2));
            TargetSongCoverHolder.anchorMax = new(1, 1 - .5f * (1 - lerp2));
            TargetSongCoverHolder.anchoredPosition = new(0, offset / 2 * (1 - lerp2));
            TargetSongCoverHolder.sizeDelta = new(0, 128 * (1 - lerp2));
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
        ProfileBar.main.SetVisibilty(a);
    }

    public void Intro()
    {
        if (!IsAnimating) StartCoroutine(IntroAnim());
    }

    public IEnumerator IntroAnim() 
    {
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
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;
}