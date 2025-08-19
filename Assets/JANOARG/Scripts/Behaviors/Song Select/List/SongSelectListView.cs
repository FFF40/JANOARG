using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SongSelectListView : MonoBehaviour
{
    [Header("List View")]
    public RectTransform BackgroundHolder;
    public CanvasGroup BackgroundGroup;
    [Space]
    public RectTransform ItemHolder;
    public CanvasGroup ItemGroup;
    public Graphic ItemTrack;
    public Graphic ItemCursor;
    [Space]
    public SongSelectListSongUI SongItemSample;
    public float SongItemSize = 48;
    public SongSelectListHeaderUI SongHeaderSample;
    public float SongHeaderSize = 32;
    [Space]
    public SongSelectFilterPanel FilterPanel;

    public List<SongSelectListItem> ItemList { get; private set; } = new();
    public List<SongSelectListSongUI> SongItems { get; private set; } = new();
    public Dictionary<string, SongSelectListSongUI> SongItemsByID { get; private set; } = new();
    public Stack<SongSelectListSongUI> UnusedSongItems { get; private set; } = new();
    public List<SongSelectListHeaderUI> HeaderItems { get; private set; } = new();

    // List view state
    public float ScrollOffset;
    public float ScrollVelocity;
    public float TargetScrollOffset;
    float lastScrollOffset;
    public bool IsDirty;
    public bool IsPointerDown;
    public bool IsTargetSongHidden;
    public bool TargetSongHiddenTarget;
    public float TargetSongOffset;
    public string TargetSongID;

    Coroutine currentAnim;

    public void HandleUpdate()
    {
        SongSelectScreen screen = SongSelectScreen.main;

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

                UpdateListTarget();
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

        if (!screen.IsAnimating && TargetSongHiddenTarget != IsTargetSongHidden)
        {
            IsTargetSongHidden = TargetSongHiddenTarget;
            if (screen.TargetSongAnim != null) StopCoroutine(screen.TargetSongAnim);
            if (IsTargetSongHidden) screen.TargetSongAnim = StartCoroutine(screen.ListTargetSongHideAnim());
            else screen.TargetSongAnim = StartCoroutine(screen.ListTargetSongShowAnim());
        }
        if (IsDirty)
        {
            UpdateListItems(screen);
            IsDirty = false;
        }
    }

    public void UpdateSort()
    {
        ItemList.Clear();
        float pos = 0;
        int sortDirection = FilterPanel.SortReversed ? -1 : 1;

        bool CanAddSong(string songID)
        {
            return MapManager.SongMapItemsByID[songID].IsRevealed;
        }
        void AddSong(string songID)
        {
            ItemList.Add(new SongSelectListSong
            {
                Position = pos,
                SongID = songID
            });
            if (TargetSongID == songID) TargetSongOffset = TargetScrollOffset = ScrollOffset = pos;
            pos += SongItemSize;
        }
        void AddHeader(string header)
        {
            ItemList.Add(new SongSelectListHeader
            {
                Position = pos,
                Header = header
            });
            pos += SongHeaderSize;
        }


        switch (FilterPanel.CurrentSortCriteria)
        {
            case SongSortCriteria.Appearance:
                {
                    SongSelectScreen screen = SongSelectScreen.main;
                    IEnumerable<PlaylistSong> songs = screen.Playlist.Songs;
                    if (FilterPanel.SortReversed) songs = songs.Reverse();
                    string lastHeader = "";
                    foreach (PlaylistSong song in songs)
                    {
                        if (!CanAddSong(song.ID)) continue;
                        PlayableSong playableSong = screen.PlayableSongByID[song.ID];
                        if (lastHeader != playableSong.Location)
                        {
                            lastHeader = playableSong.Location;
                            AddHeader(playableSong.Location);
                        }
                        AddSong(song.ID);
                    }
                    break;
                }

            case SongSortCriteria.Title:
                {
                    var songs = SongSelectScreen.main.PlayableSongByID.Select(song => (
                        Key: song.Key,
                        Value: song.Value.SongName
                    )).ToArray();
                    Array.Sort(songs, (x, y) => x.Value.CompareTo(y.Value) * sortDirection);

                    string lastHeader = "";
                    foreach (var song in songs)
                    {
                        if (!CanAddSong(song.Key)) continue;
                        string firstChar = char.ToUpper(song.Value[0]).ToString();
                        if (lastHeader != firstChar)
                        {
                            lastHeader = firstChar;
                            AddHeader(firstChar);
                        }
                        AddSong(song.Key);
                    }

                    break;
                }

            case SongSortCriteria.Artist:
                {
                    var songs = SongSelectScreen.main.PlayableSongByID.Select(song => (
                        Key: song.Key,
                        Value: song.Value.SongArtist
                    )).ToArray();
                    Array.Sort(songs, (x, y) => x.Value.CompareTo(y.Value) * sortDirection);

                    string lastHeader = "";
                    foreach (var song in songs)
                    {
                        if (!CanAddSong(song.Key)) continue;
                        if (lastHeader != song.Value)
                        {
                            lastHeader = song.Value;
                            AddHeader("<i>" + song.Value);
                        }
                        AddSong(song.Key);
                    }

                    break;
                }

            case SongSortCriteria.Difficulty:
                {
                    SongSelectScreen screen = SongSelectScreen.main;
                    int GetDifficulty(string songID)
                    {
                        var diff = screen.GetNearestDifficulty(screen.PlayableSongByID[songID].Charts);
                        return Mathf.RoundToInt(diff.ChartConstant + (diff.DifficultyIndex < 0 ? 10000 : 0));
                    }
                    var songs = SongSelectScreen.main.PlayableSongByID.Select(song => (
                        Key: song.Key,
                        Value: GetDifficulty(song.Key)
                    )).ToArray();
                    Array.Sort(songs, (x, y) => x.Value.CompareTo(y.Value) * sortDirection);
                    int lastDiff = -10000;
                    foreach (var song in songs)
                    {
                        if (!CanAddSong(song.Key)) continue;
                        if (lastDiff != song.Value)
                        {
                            lastDiff = song.Value;
                            AddHeader(song.Value > 9000 ? "??" : song.Value.ToString());
                        }
                        AddSong(song.Key);
                    }
                    break;
                }

            case SongSortCriteria.Performance:
                {
                    SongSelectScreen screen = SongSelectScreen.main;
                    int? GetScore(string songID)
                    {
                        var diff = screen.GetNearestDifficulty(screen.PlayableSongByID[songID].Charts);
                        return StorageManager.main.Scores.Get(songID, diff.Target)?.Score;
                    }
                    var songs = SongSelectScreen.main.PlayableSongByID.Select(song => (
                        Key: song.Key,
                        Value: GetScore(song.Key)
                    )).ToArray();
                    Array.Sort(songs, (x, y) => (x.Value ?? -1).CompareTo(y.Value ?? -1) * sortDirection);
                    string lastHeader = "";
                    foreach (var song in songs)
                    {
                        if (!CanAddSong(song.Key)) continue;
                        string rank = song.Value != null ? "<b><i>" + Helper.GetRank(song.Value ?? 0) : "Unplayed";
                        if (lastHeader != rank)
                        {
                            lastHeader = rank;
                            AddHeader(rank);
                        }
                        AddSong(song.Key);
                    }
                    break;
                }
        }

        foreach (SongSelectListItem item in ItemList)
        {
            item.PositionOffset = 45 * Mathf.Clamp(item.Position - TargetSongOffset, -1, 1);
        }

        UpdateListItems(SongSelectScreen.main);
    }

    public void UpdateListItems(SongSelectScreen screen, bool cap = true)
    {
        if (cap && ItemList.Count > 0) ScrollOffset = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);
        float windowSize = ItemHolder.rect.height / 2 + SongItemSize * 2;
        HashSet<string> songItemsToUnload = new(SongItemsByID.Keys);
        int headerIndex = 0;

        foreach (var item in ItemList)
        {
            float pos = item.Position - ScrollOffset;
            if (pos < -windowSize) continue;
            if (pos > windowSize) break;

            if (item is SongSelectListSong song)
            {
                SongSelectListSongUI uiHolder;
                if (SongItemsByID.ContainsKey(song.SongID))
                {
                    uiHolder = SongItemsByID[song.SongID];
                    uiHolder.SetItem(song);
                }
                else if (UnusedSongItems.Count == 0)
                {
                    uiHolder = SongItemsByID[song.SongID] = Instantiate(SongItemSample, ItemHolder);
                    SongItems.Add(uiHolder);
                    uiHolder.SetItem(song);
                }
                else
                {
                    uiHolder = SongItemsByID[song.SongID] = UnusedSongItems.Pop();
                    uiHolder.gameObject.SetActive(true);
                    uiHolder.SetItem(song);
                }

                uiHolder.UpdatePosition(ScrollOffset);
                songItemsToUnload.Remove(song.SongID);
            }
            else if (item is SongSelectListHeader header)
            {
                SongSelectListHeaderUI uiHolder;
                if (headerIndex >= HeaderItems.Count)
                {
                    uiHolder = Instantiate(SongHeaderSample, ItemHolder);
                    HeaderItems.Add(uiHolder);
                }
                else
                {
                    uiHolder = HeaderItems[headerIndex];
                    uiHolder.gameObject.SetActive(true);
                }

                uiHolder.SetItem(header);
                uiHolder.UpdatePosition(ScrollOffset);
                headerIndex++;
            }
        }
        // Unload unused items
        foreach (string id in songItemsToUnload)
        {
            var item = SongItemsByID[id];
            SongItemsByID.Remove(id);
            item.SetItem(null);
            item.UnloadCoverImage();
            item.gameObject.SetActive(false);
            UnusedSongItems.Push(item);
        }
        for (int i = headerIndex; i < HeaderItems.Count; i++)
        {
            if (HeaderItems[i].gameObject.activeSelf) HeaderItems[i].gameObject.SetActive(false);
            else break;
        }

        // Update song cursor        
        float selOffset = ScrollOffset - TargetScrollOffset;
        ItemCursor.rectTransform.anchoredPosition = new(ItemCursor.rectTransform.anchoredPosition.x, selOffset);
        if (!screen.IsAnimating && screen.TargetSongAnim == null)
        {
            screen.TargetSongCoverHolder.anchoredPosition = new(0, selOffset / 2);
        }

    }

    public void UpdateListTarget()
    {
        float lastTarget = TargetScrollOffset;
        float tsDist = float.PositiveInfinity;
        foreach (SongSelectListSongUI item in SongItems)
        {
            if (item.Target == null) continue;
            if (Mathf.Abs(item.Target.Position - ScrollOffset) < tsDist)
            {
                TargetScrollOffset = item.Target.Position;
                tsDist = Mathf.Abs(item.Target.Position - ScrollOffset);
            }
        }
        if (lastTarget != TargetScrollOffset)
        {
            TargetSongHiddenTarget = true;
            SongSelectScreen screen = SongSelectScreen.main;
            screen.SFXSource.PlayOneShot(screen.SFXTickClip, screen.SFXVolume);
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
        UpdateListTarget();
        IsDirty = true;
    }

    public void OnListPointerUp(BaseEventData data)
    {
        if (ItemList.Count < 0) ScrollOffset = Mathf.Clamp(ScrollOffset, -20, 20);
        else ScrollOffset = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);
        IsPointerDown = false;
    }



    public void LerpListView(float a)
    {
        float xPos = 120 + 60 * a;
        ItemTrack.color = new(1, 1, 1, a);
        ItemGroup.alpha = BackgroundGroup.alpha = a;
        ItemTrack.rectTransform.anchoredPosition = new(xPos - 180, ItemTrack.rectTransform.anchoredPosition.y);
        BackgroundHolder.anchoredPosition = new(xPos, BackgroundHolder.anchoredPosition.y);
        ItemHolder.anchoredPosition = new(xPos, BackgroundHolder.anchoredPosition.y);
    }

    public IEnumerator SortAnim()
    {
        yield return Ease.Animate(.05f, (t) =>
        {
            float ease1 = t;  // Ease.Get(t, EaseFunction.Linear, EaseMode.In);
            float xPos = 180 - 10 * ease1;
            ItemGroup.alpha = 1 - ease1;
            ItemHolder.anchoredPosition = new(xPos, BackgroundHolder.anchoredPosition.y);
        });

        UpdateSort();

        yield return Ease.Animate(.3f, (t) =>
        {
            float ease1 = Ease.Get(t, EaseFunction.Exponential, EaseMode.Out);
            float xPos = 170 + 10 * ease1;
            ItemGroup.alpha = ease1;
            ItemHolder.anchoredPosition = new(xPos, BackgroundHolder.anchoredPosition.y);
        });

        currentAnim = null;
    }

    public void DoSortAnim()
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(SortAnim());
    }
}