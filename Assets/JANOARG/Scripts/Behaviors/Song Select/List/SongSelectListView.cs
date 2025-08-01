using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SongSelectListView : MonoBehaviour
{
    [Header("List View")]
    public RectTransform BackgroundHolder;
    public CanvasGroup BackgroundGroup;
    [Space]
    public SongSelectListItem ItemSample;
    public RectTransform ItemHolder;
    public CanvasGroup ItemGroup;
    public List<SongSelectListItem> ItemList { get; private set; } = new();
    public Graphic ItemTrack;
    public Graphic ItemCursor;

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

    public void UpdateListItems(SongSelectScreen screen, bool cap = true)
    {
        float scrollOfs = ScrollOffset;
        if (cap) scrollOfs = Mathf.Clamp(ScrollOffset, ItemList[0].Position - 20, ItemList[^1].Position + 20);

        foreach (SongSelectListItem item in ItemList)
        {
            RectTransform rt = (RectTransform)item.transform;
            rt.anchoredPosition = new Vector2(-.26795f, -1) * (item.Position + item.PositionOffset - scrollOfs);
        }
        ItemCursor.rectTransform.anchoredPosition = new(ItemCursor.rectTransform.anchoredPosition.x, scrollOfs - TargetScrollOffset);
        if (!screen.IsAnimating && screen.TargetSongAnim == null)
        {
            float offset = scrollOfs - TargetSongOffset;
            screen.TargetSongCoverHolder.anchoredPosition = new(0, offset / 2);
        }
    }

    public void UpdateListTarget()
    {
        float lastTarget = TargetScrollOffset;
        float tsDist = float.PositiveInfinity;
        foreach (SongSelectListItem item in ItemList)
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
}