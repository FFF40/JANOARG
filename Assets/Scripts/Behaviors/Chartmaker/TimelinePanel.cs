using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimelinePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    public static TimelinePanel main;

    [Header("Data")]
    public TimelineMode CurrentMode;
    [Space]
    public Vector2 PeekRange;
    public Vector2 PeekLimit;
    [Space]
    public int ScrollOffset;
    public int SeparationFactor;
    public float VerticalScale;
    public float VerticalOffset;
    public float ResizeVelocity;

    [Header("Objects")]
    public Button StoryboardTab;
    public Button TimingTab;
    public Button LaneTab;
    public Button LaneStepTab;
    public Button HitObjectTab;
    [Space]
    public RectTransform TimeSliderHolder;
    public RectTransform CurrentTimeSlider;
    public RectTransform PeekRangeSlider;
    public RectTransform PeekStartSlider;
    public RectTransform PeekEndSlider;
    public RectTransform TicksHolder;
    public RectTransform ItemsHolder;
    public RectTransform TailsHolder;
    public RectTransform StoryboardEntryHolder;
    public RectTransform CurrentTimeTick;
    public RectTransform CurrentTimeConnector;
    public RectTransform SelectionRect;
    [Space]
    public RawImage WaveformImage;
    [Space]
    public Scrollbar VerticalScrollbar;
    public GameObject Blocker;
    public TMP_Text BlockerLabel;
    public CanvasGroup CurrentTimeCoonectorGroup;
    public CanvasGroup PeekSliderGroup;
    public CanvasGroup BlockerTextGroup;
    [Space]
    public Button UndoButton;
    public Button RedoButton;
    public RectTransform EditHistoryHolder;
    public CanvasGroup UndoButtonGroup;
    public CanvasGroup RedoButtonGroup;
    public TMP_Text ActionsBehindCounter;
    public TMP_Text ActionsAheadCounter;
    [Space]
    public Button CutButton;
    public Button CopyButton;
    public Button PasteButton;
    public CanvasGroup CutButtonGroup;
    public CanvasGroup CopyButtonGroup;
    public CanvasGroup PasteButtonGroup;
    [Space]
    public GameObject VerticalScaleHolder;
    public GameObject VerticalOffsetHolder;
    [Space]
    public TimelineOptionsPanel Options;
    [Header("Samples")]
    public TimelineTick TickSample;
    [HideInInspector]
    public List<TimelineTick> Ticks;
    public TimelineItem ItemSample;
    [HideInInspector]
    public List<TimelineItem> Items;
    public Image ItemTailSample;
    [HideInInspector]
    public List<Image> ItemTails;
    public TMP_Text StoryboardEntrySample;
    public Material StoryboardEntryMaterial;
    [HideInInspector]
    public List<TMP_Text> StoryboardEntries;

    int TimelineHeight = 8;
    int TimelineExpandHeight = 8;
    int TimelineRestoreHeight = 8;
    int ItemHeight = 0;
    bool lastPlayed;
    public IList DraggingItem;
    float DraggingItemOffset;

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
        UpdateTabs();
        UpdateScrollbar();
        Options.OnEnable();
    }

    public void UpdatePeekLimit()
    {
        PeekLimit.x = -5;
        PeekLimit.y = Chartmaker.main.CurrentSong.Clip.length + 5;
    }

    public void Update()
    {
        Vector2 limit = new(
            Mathf.Min(PeekRange.x, PeekLimit.x),
            Mathf.Max(PeekRange.y, PeekLimit.y)
        );

        float time = Chartmaker.main.SongSource.time;

        if (isDragged && (int)dragMode % 2 == 1 && dragMode != TimelineDragMode.TimelineDrag)
        {
            float density = (PeekRange.y - PeekRange.x) / TicksHolder.rect.width;
            float offset = 0;

            if (dragEnd.x < 50)
            {
                offset = -Mathf.Pow(50 - dragEnd.x, 2f) * density;
            }
            if (dragEnd.x > TicksHolder.rect.width - 50)
            {
                offset = Mathf.Pow(dragEnd.x - TicksHolder.rect.width + 50, 2f) * density;
            }
            
            if (offset != 0)
            {
                offset = Mathf.Clamp(offset * Time.deltaTime, limit.x - PeekRange.x, limit.y - PeekRange.y);
                PeekRange.x += offset;
                PeekRange.y += offset;
                OnDrag(lastDrag);
            }
        } 
        else if (Options.FollowSeekLine && Chartmaker.main.SongSource.isPlaying)
        {
            float mid = (PeekRange.x + PeekRange.y) / 2;
            float offset = Mathf.Clamp(time - mid, limit.x - PeekRange.x, limit.y - PeekRange.y);
            PeekRange.x += offset;
            PeekRange.y += offset;
        }

        CurrentTimeSlider.anchorMin = CurrentTimeSlider.anchorMax
            = new(Mathf.InverseLerp(limit.x, limit.y, time), .5f);
        PeekStartSlider.anchorMin = PeekStartSlider.anchorMax = PeekRangeSlider.anchorMin
            = new(Mathf.InverseLerp(limit.x, limit.y, PeekRange.x), .5f);
        PeekEndSlider.anchorMin = PeekEndSlider.anchorMax = PeekRangeSlider.anchorMax
            = new(Mathf.InverseLerp(limit.x, limit.y, PeekRange.y), .5f);
            
        if (PeekRange.x == PeekRange.y)
        {
            CurrentTimeTick.anchorMin = new(-1, 0);
            CurrentTimeTick.anchorMax = new(-1, 1);
            CurrentTimeConnector.anchorMin = new(-1, 0);
            CurrentTimeConnector.anchorMax = new(-1, 0);
        }
        else
        {
            float timePos = Mathf.Clamp(InverseLerpUnclamped(PeekRange.x, PeekRange.y, time), -1, 2);
            float timeCOffset = 40 / TimeSliderHolder.rect.width;
            float timeCPos = InverseLerpUnclamped(limit.x, limit.y, time) / (1 + timeCOffset) + timeCOffset / 2;

            CurrentTimeTick.anchorMin = new(timePos, 0);
            CurrentTimeTick.anchorMax = new(timePos, 1);
            CurrentTimeConnector.anchorMin = new(Mathf.Min(timePos, timeCPos), 0);
            CurrentTimeConnector.anchorMax = new(Mathf.Max(timePos, timeCPos), 0);
        }
        
        UpdateTimeline();
    }

    public void UpdateTabs()
    {

        TimingTab.gameObject.SetActive(HierarchyPanel.main.CurrentMode == HierarchyMode.PlayableSong);
        TimingTab.interactable = TimelineHeight <= 0 || CurrentMode != TimelineMode.Timing;
        
        StoryboardTab.gameObject.SetActive(HierarchyPanel.main.CurrentMode == HierarchyMode.Chart);
        StoryboardTab.interactable = TimelineHeight <= 0 || CurrentMode != TimelineMode.Storyboard;
        LaneTab.gameObject.SetActive(HierarchyPanel.main.CurrentMode == HierarchyMode.Chart);
        LaneTab.interactable = TimelineHeight <= 0 || CurrentMode != TimelineMode.Lanes;
        LaneStepTab.gameObject.SetActive(HierarchyPanel.main.CurrentMode == HierarchyMode.Chart && InspectorPanel.main.CurrentHierarchyObject is Lane);
        LaneStepTab.interactable = TimelineHeight <= 0 || CurrentMode != TimelineMode.LaneSteps;
        HitObjectTab.gameObject.SetActive(HierarchyPanel.main.CurrentMode == HierarchyMode.Chart && InspectorPanel.main.CurrentHierarchyObject is Lane);
        HitObjectTab.interactable = TimelineHeight <= 0 || CurrentMode != TimelineMode.HitObjects;
        
        VerticalScaleHolder.gameObject.SetActive(CurrentMode == TimelineMode.HitObjects);
        VerticalOffsetHolder.gameObject.SetActive(CurrentMode == TimelineMode.HitObjects);

        PickerPanel.main.UpdateButtons();
    }

    public void SetTabMode(int mode) => SetTabMode((TimelineMode)mode);

    public void SetTabMode(TimelineMode mode)
    {
        CurrentMode = mode;
        if (TimelineExpandHeight != TimelineHeight) 
        {
            ResizeTimeline(TimelineExpandHeight * 24 + 80);
        }
        else 
        {
            UpdateTabs();
            UpdateItems();
        }

    }

    public void UpdateTimeline(bool forced = false)
    {
        if (lastLimit != PeekRange || lastPlayed != Chartmaker.main.SongSource.isPlaying || forced)
        {
            lastLimit = PeekRange;
            lastPlayed = Chartmaker.main.SongSource.isPlaying;
            Metronome metronome = Chartmaker.main.CurrentSong.Timing;
            float density = (PeekRange.y - PeekRange.x) * metronome.GetStop(PeekRange.x, out _).BPM / TicksHolder.rect.width / 8;
            int count = 0;

            Color color = Themer.main.Keys["TimelineTickMain"];

            if (density != 0)
            {
                float factor = Mathf.Log(density, SeparationFactor);
                BeatPosition beat = BeatFloor(metronome.ToBeat(PeekRange.x), Mathf.FloorToInt(factor), SeparationFactor);
                BeatPosition interval = BeatInterval(Mathf.FloorToInt(factor), SeparationFactor);
                float end = metronome.ToBeat(PeekRange.y);
                while (beat < end)
                {
                    TimelineTick tick;
                    if (Ticks.Count <= count) Ticks.Add(tick = Instantiate(TickSample, TicksHolder));
                    else tick = Ticks[count];

                    float den = GetSepFactor(beat, SeparationFactor) - factor;

                    RectTransform rt = (RectTransform)tick.transform;
                    rt.anchorMin = new (
                        (metronome.ToSeconds(beat) - PeekRange.x) / (PeekRange.y - PeekRange.x),
                        0f
                    );
                    rt.anchorMax = new(rt.anchorMin.x, 1);

                    tick.Image.color = GetBeatColor(beat) * new Color(1, 1, 1, Mathf.Clamp01((Mathf.Pow(1.5f, den) - 1) / (Mathf.Pow(1.5f, 3) - 1)) * .5f);
                    tick.Label.color = color;
                    tick.Label.alpha = Mathf.Clamp01(den - 2.5f) * .5f;
                    if (tick.Label.alpha > 0) tick.Label.text = beat.ToString();

                    beat += interval;
                    count++;

                    if (count > 1000) break;
                }
            }
            while (Ticks.Count > count)
            {
                Destroy(Ticks[^1].gameObject);
                Ticks.RemoveAt(Ticks.Count - 1);
            }

            UpdateItems();
            UpdateWaveform();
        }
        else if (waveTimeouted) 
        {
            UpdateWaveform();
        }
    }

    TimelineItem GetTimelineItem(int index)
    {
        TimelineItem item;
        if (Items.Count <= index) Items.Add(item = Instantiate(ItemSample, ItemsHolder));
        else item = Items[index];
        return item;
    }
    Image GetItemTail(int index)
    {
        Image item;
        if (ItemTails.Count <= index) ItemTails.Add(item = Instantiate(ItemTailSample, TailsHolder));
        else item = ItemTails[index];
        return item;
    }
    TMP_Text GetStoryboardEntry(int index)
    {
        TMP_Text item;
        if (StoryboardEntries.Count <= index) StoryboardEntries.Add(item = Instantiate(StoryboardEntrySample, StoryboardEntryHolder));
        else item = StoryboardEntries[index];
        return item;
    }

    public void UpdateItems()
    {
        if (TimelineHeight <= 0) return;

        int count = 0;
        int tcount = 0;
        int sbcount = 0;
        Metronome metronome = Chartmaker.main.CurrentSong.Timing;
        
        float density = (PeekRange.y - PeekRange.x) / TicksHolder.rect.width;
        List<float> times = new();

        Blocker.SetActive(false);

        TimelineItem AddItem(object obj, float time)
        {
            var item = GetTimelineItem(count);
            RectTransform rt = (RectTransform)item.transform;
            rt.anchorMin = rt.anchorMax = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, time), 1);
            item.SetItem(obj);
            count++;
            return item;
        }
        int AddTime(float time, float size = 24)
        {
            int pos;
            size *= density;

            for (pos = 0; pos < times.Count; pos++)
            {
                if (times[pos] < time - size) break;
            }

            if (pos < times.Count) times[pos] = time;
            else times.Add(time);

            return pos;
        }
        TimelineItem AddItemNormal(object obj, float time, float size = 22)
        {
            TimelineItem item = null;
            int pos = AddTime(time, size + 2) - ScrollOffset;
            float dOffset = size * density / 2;
            if (time >= PeekRange.x - dOffset && time <= PeekRange.y + dOffset && pos >= -1 && pos < TimelineHeight + 1)
            {
                item = AddItem(obj, time);
                RectTransform rt = (RectTransform)item.transform;
                rt.anchoredPosition = new(0, -24 * pos - 5);
                rt.sizeDelta = new(size, 22);
            }
            return item;
        }

        if (CurrentMode == TimelineMode.Storyboard)
        {
            if (InspectorPanel.main.CurrentObject is IStoryboardable thing)
            {
                TimestampType[] types = (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null);
                Storyboard sb = thing.Storyboard;

                StoryboardEntryMaterial.SetColor("_OutlineColor", Themer.main.Keys["Background0"] + new Color(0, 0, 0, 1));

                for (int a = 0; a < types.Length; a++)
                {
                    int index = a - ScrollOffset;
                    times.Add(0);
                    if (index >= 0 && a - ScrollOffset < TimelineHeight)
                    {
                        TMP_Text label = GetStoryboardEntry(index);
                        RectTransform rt = label.rectTransform;
                        label.text = types[a].Name;
                        rt.anchoredPosition = new(0, -24 * index - 5);
                        sbcount++;
                    }
                }

                float dOffset = 4 * density;
                foreach (Timestamp ts in sb.Timestamps)
                {
                    float time = metronome.ToSeconds(ts.Offset);
                    float timeEnd = metronome.ToSeconds(ts.Offset + ts.Duration);
                    if (timeEnd < PeekRange.x - dOffset || time > PeekRange.y + dOffset) continue;

                    float index = Array.FindIndex(types, x => x.ID == ts.ID) - ScrollOffset;
                    if (index < -1 || index >= TimelineHeight + 1) continue;

                    float posX = InverseLerpUnclamped(PeekRange.x, PeekRange.y, time);
                    if (time != timeEnd)
                    {
                        var tail = GetItemTail(tcount);
                        RectTransform trt = tail.rectTransform;
                        trt.anchorMin = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, time), 1);
                        trt.anchorMax = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, timeEnd), 1);
                        trt.anchoredPosition = new(0, -24 * index - 5);
                        trt.sizeDelta = new(0, 22);
                        posX = Mathf.Max(posX, Mathf.Min(8 / ItemsHolder.rect.width, Mathf.Max(trt ? trt.anchorMax.x - 4 / ItemsHolder.rect.width : posX, posX)));
                        tcount++;
                    }

                    var item = GetTimelineItem(count);
                    RectTransform rt = (RectTransform)item.transform;
                    rt.anchorMin = rt.anchorMax = new (posX, 1);
                    rt.anchoredPosition = new(0, -24 * index - 5);
                    rt.sizeDelta = new(8, 22);
                    item.SetItem(ts);

                    count++;
                }
            }
            else if (InspectorPanel.main.CurrentObject is IList)
            {
                Blocker.SetActive(true);
                BlockerLabel.text = "Storyboard editing of multiple objects is not supported.";
            }
            else if (InspectorPanel.main.CurrentObject == null)
            {
                Blocker.SetActive(true);
                BlockerLabel.text = "No object selected - Please select an object first to view its Storyboard.";
            }
            else
            {
                Blocker.SetActive(true);
                BlockerLabel.text = "This object is not storyboardable.";
            }
        }
        else if (CurrentMode == TimelineMode.Timing)
        {
            foreach (BPMStop stop in Chartmaker.main.CurrentSong?.Timing.Stops)
            {
                AddItemNormal(stop, stop.Offset);
            }
        }
        else if (CurrentMode == TimelineMode.Lanes)
        {
            if (Chartmaker.main.CurrentChart?.Lanes != null)
            {
                float dOffset = 11 * density;
                foreach (Lane lane in Chartmaker.main.CurrentChart.Lanes)
                {
                    float time = metronome.ToSeconds(lane.LaneSteps[0].Offset);
                    float timeEnd = metronome.ToSeconds(lane.LaneSteps[^1].Offset);

                    int pos = AddTime(time, 24) - ScrollOffset;
                    times[pos + ScrollOffset] = Mathf.Max(time, timeEnd - 7 * density);

                    if (pos < -1 || pos >= TimelineHeight + 1) continue;
                    if (timeEnd < PeekRange.x - dOffset || time > PeekRange.y + dOffset) continue;

                    for (int a = 1; a < lane.LaneSteps.Count; a++)
                    {
                        LaneStep step = lane.LaneSteps[a];
                        float stepTime = metronome.ToSeconds(step.Offset);
                        if (stepTime < PeekRange.x - dOffset || stepTime > PeekRange.y + dOffset) continue;

                        float sposX = InverseLerpUnclamped(PeekRange.x, PeekRange.y, stepTime);
                        var sitem = GetTimelineItem(count);
                        RectTransform srt = (RectTransform)sitem.transform;
                        srt.anchorMin = srt.anchorMax = new (sposX, 1);
                        srt.anchoredPosition = new(0, -24 * pos - 5);
                        srt.sizeDelta = new(8, 22);
                        sitem.SetItem(step, lane);
                        
                        count++;
                    }

                    float posX = InverseLerpUnclamped(PeekRange.x, PeekRange.y, time);
                    if (time != timeEnd)
                    {
                        var tail = GetItemTail(tcount);
                        RectTransform trt = tail.rectTransform;
                        trt.anchorMin = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, time), 1);
                        trt.anchorMax = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, timeEnd), 1);
                        trt.anchoredPosition = new(0, -24 * pos - 5);
                        trt.sizeDelta = new(0, 22);
                        posX = Mathf.Max(posX, Mathf.Min(15 / ItemsHolder.rect.width, Mathf.Max(trt ? trt.anchorMax.x - 16 / ItemsHolder.rect.width : posX, posX)));
                        tcount++;
                    }

                    var item = GetTimelineItem(count);
                    RectTransform rt = (RectTransform)item.transform;
                    rt.anchorMin = rt.anchorMax = new (posX, 1);
                    rt.anchoredPosition = new(0, -24 * pos - 5);
                    rt.sizeDelta = new(22, 22);
                    item.SetItem(lane);
                    
                    count++;
                }
            }
            else
            {
                Blocker.SetActive(true);
                BlockerLabel.text = "No chart loaded - Load a chart first to view its Lanes.";
            }
        }
        else if (CurrentMode == TimelineMode.LaneSteps)
        {
            if (InspectorPanel.main.CurrentHierarchyObject is Lane lane)
            {
                foreach (LaneStep step in lane.LaneSteps)
                {
                    AddItemNormal(step, metronome.ToSeconds(step.Offset));
                }
            }
            else
            {
                Blocker.SetActive(true);
                BlockerLabel.text = "No lane selected - Select a lane first to view its Lane Steps.";
            }
        }
        else if (CurrentMode == TimelineMode.HitObjects)
        {
            if (InspectorPanel.main.CurrentHierarchyObject is Lane lane)
            {
                float height = ItemsHolder.rect.height - 8;
                float dOffset = 4 * density;

                float vpStart = .5f - VerticalScale * .5f + VerticalOffset;
                float vpEnd = .5f + VerticalScale * .5f + VerticalOffset;

                foreach (HitObject hit in lane.Objects)
                {
                    float time = metronome.ToSeconds(hit.Offset);
                    float timeEnd = metronome.ToSeconds(hit.Offset + hit.HoldLength);
                    if (timeEnd < PeekRange.x - dOffset || time > PeekRange.y + dOffset) continue;

                    float start = InverseLerpUnclamped(vpStart, vpEnd, hit.Position);
                    float end = InverseLerpUnclamped(vpStart, vpEnd, hit.Position + hit.Length);
                    float pos = Mathf.Floor(-start * height) - 2;
                    float length = Mathf.Floor((end - start) * height) + 4;

                    if (time != timeEnd)
                    {
                        var tail = GetItemTail(tcount);
                        RectTransform trt = tail.rectTransform;
                        trt.anchorMin = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, time), 1);
                        trt.anchorMax = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, timeEnd), 1);
                        trt.anchoredPosition = new Vector2(0, pos);
                        trt.sizeDelta = new Vector2(8, length);
                        tcount++;
                    }
                    if (time < PeekRange.x - dOffset || time > PeekRange.y + dOffset) continue;

                    var item = GetTimelineItem(count);
                    RectTransform rt = (RectTransform)item.transform;
                    rt.anchorMin = rt.anchorMax = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, time), 1);
                    rt.anchoredPosition = new Vector2(0, pos);
                    rt.sizeDelta = new Vector2(8, length);

                    item.SetItem(hit);

                    count++;
                }
            }
            else
            {
                Blocker.SetActive(true);
                BlockerLabel.text = "No lane selected - Select a lane first to view its Hit Objects.";
            }
        }

        while (Items.Count > count)
        {
            Destroy(Items[^1].gameObject);
            Items.RemoveAt(Items.Count - 1);
        }
        while (ItemTails.Count > tcount)
        {
            Destroy(ItemTails[^1].gameObject);
            ItemTails.RemoveAt(ItemTails.Count - 1);
        }
        while (StoryboardEntries.Count > sbcount)
        {
            Destroy(StoryboardEntries[^1].gameObject);
            StoryboardEntries.RemoveAt(StoryboardEntries.Count - 1);
        }
        
        if (ItemHeight != times.Count)
        {
            ItemHeight = times.Count;
            UpdateScrollbar();
        }
    }

    int waveOffset = 0;
    float waveTime, waveLastDensity = 0;
    bool waveTimeouted = false;
    bool[] waveBaked;

    public void UpdateWaveform()
    {
        if (
            TimelineHeight <= 0
            || PeekRange.y == PeekRange.x
            || Options.WaveformMode == 0
            || Options.WaveformIdle < (Chartmaker.main.SongSource.isPlaying ? 1 : 0)
        )
        {
            WaveformImage.enabled = false;
            return;
        }
        
        WaveformImage.enabled = true;
        Color color = Themer.main.Keys["TimelineTickMain"];

        Texture2D tex = null;
        if (WaveformImage.texture is Texture2D) tex = (Texture2D)WaveformImage.texture;
        RectTransform waveRT = WaveformImage.rectTransform;
        if (!tex || tex.width != (int)waveRT.rect.width || tex.height != (int)waveRT.rect.height)
        {
            Destroy(WaveformImage.texture);
            WaveformImage.texture = tex = new Texture2D((int)waveRT.rect.width, (int)waveRT.rect.height);
            waveBaked = new bool[(int)waveRT.rect.width];
            waveOffset = 0;
        }

        AudioClip clip = Chartmaker.main.SongSource.clip;

        float density = clip.frequency * (PeekRange.y - PeekRange.x) / tex.width;

        float step = (PeekRange.y - PeekRange.x) / tex.width;
        float sec = Mathf.Floor(PeekRange.x / step - 1) * step;
        int waveNewOffset = (int)((sec - waveTime) / step);

        if (!(Math.Abs(waveLastDensity / density - 1) < 0.0001f) || Mathf.Abs(waveOffset - waveNewOffset) >= tex.width) {
            Destroy(WaveformImage.texture);
            WaveformImage.texture = tex = new Texture2D((int)waveRT.rect.width, (int)waveRT.rect.height);
            waveBaked = new bool[(int)waveRT.rect.width];
            waveLastDensity = density;
            waveTime = sec;
            waveOffset = waveNewOffset = 0;
        }

        Color[] lineBuffer = new Color[tex.height];
        while (waveOffset < waveNewOffset) 
        {
            int sLine = (waveOffset % tex.width + tex.width) % tex.width;
            waveBaked[sLine] = false;
            tex.SetPixels(sLine, 0, 1, tex.height, lineBuffer);
            waveOffset++;
        }
        while (waveOffset > waveNewOffset) 
        {
            int sLine = (waveOffset % tex.width + tex.width) % tex.width;
            waveBaked[sLine] = false;
            tex.SetPixels(sLine, 0, 1, tex.height, lineBuffer);
            waveOffset--;
        }

        WaveformImage.uvRect = new Rect(waveOffset / (float)tex.width, 0, 1, 1);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        bool timeout() => stopwatch.ElapsedMilliseconds >= 15;
        waveTimeouted = false;

        switch (Options.WaveformMode) 
        {
            case 1: {
                float[] data = new float[Mathf.CeilToInt(Math.Min(density, 1024) / clip.channels) * clip.channels];
                float denY = 1f / tex.height * clip.channels;
                float[] lastMin = new float[clip.channels], lastMax = new float[clip.channels];
                color *= new Color(1, 1, 1, .5f);

                for (int a = 0; a < clip.channels; a++)
                {
                    lastMin[a] = -1;
                    lastMax[a] = 1;
                }

                for (int x = 0; x < tex.width; x++) 
                {
                    int sLine = ((x + waveOffset) % tex.width + tex.width) % tex.width;
                    if (waveBaked[sLine]) continue;
                    sec = waveTime + (x + waveOffset) * step;
                    float[] min = new float[clip.channels], max = new float[clip.channels];
                    int pos = (int)(sec * clip.frequency);
                    if (pos >= 0 && pos < clip.samples - data.Length)
                    {
                        clip.GetData(data, pos);
                        for (int a = 0; a < clip.channels; a++)
                        {
                            min[a] = 1;
                            max[a] = -1;
                        }
                        for (int a = 0; a < data.Length; a++)
                        {
                            int chan = (a + pos) % clip.channels;
                            min[chan] = Math.Min(min[chan], data[a]);
                            max[chan] = Math.Max(max[chan], data[a]);
                        }
                        for (int a = 0; a < clip.channels; a++)
                        {
                            float temp;
                            min[a] = Math.Min(lastMax[a], temp = min[a]) * .8f;
                            max[a] = Math.Max(lastMin[a], lastMax[a] = max[a]) * .8f;
                            lastMin[a] = temp;
                        }
                    }
                    float sPos = 0;
                    for (int y = 0; y < tex.height; y++) 
                    {
                        int channel = Mathf.FloorToInt(sPos);
                        float window = 1 - (sPos % 1) * 2f;
                        lineBuffer[y] = window >= min[channel] - denY && window <= max[channel] + denY ? color : Color.clear;
                        sPos += denY;
                    }
                    tex.SetPixels(sLine, 0, 1, tex.height, lineBuffer);
                    waveBaked[sLine] = true;
                    if (timeout())
                    {
                        waveTimeouted = true;
                        break;
                    }
                }
            } break;

            case 2: {
                int resolution = 512;

                float[] data = new float[resolution * clip.channels];
                float[][] fft = new float[clip.channels][];
                float denY = 1f / tex.height * clip.channels;

                for (int i = 0; i < clip.channels; i++) 
                {
                    fft[i] = new float[resolution];
                }

                Func<float, float> scale, unscale;

                switch (Chartmaker.Preferences.FrequencyScale)
                {
                    case FrequencyScale.Logarithmic: 
                        scale = (x) => Mathf.Log10(1 + x);
                        unscale = (x) => Mathf.Pow(10, x) - 1;
                        break;
                    case FrequencyScale.Mel: 
                        scale = (x) => 2595 * Mathf.Log10(1 + x / 700);
                        unscale = (x) => 700 * Mathf.Pow(10, x / 2595) - 700;
                        break;
                    case FrequencyScale.Bark: 
                        scale = (x) => 6 * (float)Math.Asinh(x / 600);
                        unscale = (x) => (float)Math.Sinh(x / 6) * 600;
                        break;
                    case FrequencyScale.Linear: default: 
                        scale = (x) => x;
                        unscale = (x) => x;
                        break;
                }


                float minScale = scale(Chartmaker.Preferences.FrequencyMin);
                float maxScale = scale(Chartmaker.Preferences.FrequencyMax);
                
                for (int x = 0; x < tex.width; x++) 
                {
                    int sLine = ((x + waveOffset) % tex.width + tex.width) % tex.width;
                    if (waveBaked[sLine]) continue;
                    sec = waveTime + (x + waveOffset) * step;
                    int pos = (int)(sec * clip.frequency - resolution / 2);
                    if (pos >= 0 && pos < clip.samples - data.Length)
                    {
                        clip.GetData(data, pos);
                        for (int y = 0; y < data.Length; y++) 
                        {
                            int chan = (pos + y) % clip.channels;
                            int p = y / clip.channels;
                            fft[chan][p] = data[y];
                        }
                        for (int y = 0; y < fft.Length; y++) 
                        {
                            FFT.Transform(fft[y], Chartmaker.Preferences.FFTWindow);
                        }
                    }
                    float sPos = 0;
                    for (int y = 0; y < tex.height; y++) 
                    {
                        int channel = Mathf.FloorToInt(sPos);
                        float cPos = Mathf.Clamp(unscale(Mathf.Lerp(minScale, maxScale, sPos % 1)) / clip.frequency * resolution, 0, resolution - 1);
                        float value = Mathf.Sqrt(Mathf.Lerp(fft[channel][Mathf.FloorToInt(cPos)], fft[channel][Mathf.CeilToInt(cPos)], cPos % 1) / resolution * cPos) / 4;
                        lineBuffer[y] = color * new Color(1, 1, 1, value);
                        sPos += denY;
                    }
                    tex.SetPixels(sLine, 0, 1, tex.height, lineBuffer);
                    waveBaked[sLine] = true;
                    if (timeout())
                    {
                        waveTimeouted = true;
                        break;
                    }
                }
            } break;

            case 3: {
                int resolution = 512;

                float[] data = new float[resolution * clip.channels];
                float[][] fft = new float[clip.channels][];
                float[][] chroma = new float[clip.channels][];
                float denY = 1f / tex.height * clip.channels;

                for (int i = 0; i < clip.channels; i++) 
                {
                    fft[i] = new float[resolution];
                }

                const float freqAnchor = 440;
                // Mathf.Pow(2, 1 / 12f)
                const float freqStep = 1.0594630943592953f;
                
                for (int x = 0; x < tex.width; x++) 
                {
                    int sLine = ((x + waveOffset) % tex.width + tex.width) % tex.width;
                    if (waveBaked[sLine]) continue;
                    sec = waveTime + (x + waveOffset) * step;
                    int pos = (int)(sec * clip.frequency - resolution / 2);
                    for (int y = 0; y < fft.Length; y++) chroma[y] = new float[12];
                    if (pos >= 0 && pos < clip.samples - data.Length)
                    {
                        clip.GetData(data, pos);
                        for (int y = 0; y < data.Length; y++) 
                        {
                            int chan = (pos + y) % clip.channels;
                            int p = y / clip.channels;
                            fft[chan][p] = data[y];
                        }
                        for (int y = 0; y < fft.Length; y++) 
                        {
                            FFT.Transform(fft[y], Chartmaker.Preferences.FFTWindow);
                            chroma[y] = new float[12];
                            for (int z = fft[y].Length / 128; z < fft[y].Length / 2; z++) 
                            {
                                float freq = (float)(z + 1) / resolution * clip.frequency;
                                float pitch = Mathf.Log(freq / freqAnchor, freqStep);
                                int chromaPos = (int)(pitch % 12 + 12) % 12;
                                int octave = Mathf.FloorToInt(pitch / 12);
                                float chromaOfs = (int)(pitch % 1 + 1) % 1;
                                
                                float multi = Mathf.Pow(1 / (1f - z / fft[y].Length), .25f) * Mathf.Pow(.5f, Mathf.Abs(octave - 1.5f)) * 1.418f;
                                chroma[y][chromaPos] += fft[y][z] * multi * (1 - chromaOfs) / 240;
                                chroma[y][(chromaPos + 1) % 12] += fft[y][z] * multi * chromaOfs / 240;
                            }
                        }
                    }
                    float sPos = 0;
                    for (int y = 0; y < tex.height; y++) 
                    {
                        int channel = Mathf.FloorToInt(sPos);
                        int cPos = Mathf.FloorToInt((sPos % 1 - 0.05f) * 1.1f * 12);
                        float value = cPos >= 0 && cPos < 12 ? chroma[channel][cPos] / 2 : 0;
                        lineBuffer[y] = color * new Color(1, 1, 1, value);
                        sPos += denY;
                    }
                    tex.SetPixels(sLine, 0, 1, tex.height, lineBuffer);
                    waveBaked[sLine] = true;
                    if (timeout())
                    {
                        waveTimeouted = true;
                        break;
                    }
                }
            } break;
        }

        tex.Apply();
    }

    public void DiscardWaveform() 
    {
        var waveRT = WaveformImage.rectTransform;
        Destroy(WaveformImage.texture);
        WaveformImage.texture = new Texture2D((int)waveRT.rect.width, (int)waveRT.rect.height);
        waveBaked = new bool[waveBaked.Length];
        waveTimeouted = true;
    }

    public void UpdateScrollbar()
    {
        if (ItemHeight > TimelineHeight)
        {
            if (ScrollOffset > ItemHeight - TimelineHeight)
            {
                ScrollOffset = ItemHeight - TimelineHeight;
                UpdateItems();
            }
            VerticalScrollbar.gameObject.SetActive(true);
            VerticalScrollbar.value = ScrollOffset / (float)(ItemHeight - TimelineHeight);
            VerticalScrollbar.size = TimelineHeight / (float)ItemHeight;
        }
        else
        {
            ScrollOffset = 0;
            VerticalScrollbar.gameObject.SetActive(false);
        }
    }

    public void SetScrollbar(float value)
    {
        int offset = Mathf.RoundToInt(value * (ItemHeight - TimelineHeight));
        if (ScrollOffset != offset)
        {
            ScrollOffset = offset;
            UpdateItems();
        }
        UpdateScrollbar();
    }

    public float RoundBeat(float time) 
    {
        Metronome metronome = Chartmaker.main.CurrentSong.Timing;
        float density = (PeekRange.y - PeekRange.x) * metronome.GetStop(time, out _).BPM / TicksHolder.rect.width / 8;
        float factor = Mathf.Floor(Mathf.Log(density, SeparationFactor));
        float step = Mathf.Pow(SeparationFactor, factor + 1);
        return Mathf.Round(metronome.ToBeat(time) / step) * step;
    }

    public BeatPosition ToRoundedBeat(float beat) 
    {
        Metronome metronome = Chartmaker.main.CurrentSong.Timing;
        float density = (PeekRange.y - PeekRange.x) * metronome.GetStop(beat, out _).BPM / TicksHolder.rect.width / 8;
        float factor = Mathf.Floor(Mathf.Log(density, SeparationFactor));
        float step = Mathf.Pow(SeparationFactor, -1 - factor);
        if (step > 1) 
        {
            return new BeatPosition(
                Mathf.FloorToInt(Mathf.Abs(beat)) * Math.Sign(beat),
                Mathf.RoundToInt(Mathf.Abs(beat % 1) * step) * Math.Sign(beat),
                (int)step);
        } 
        else 
        {
            return new BeatPosition((int)(Mathf.Floor(beat * step) / step));
        }
    }

    BeatPosition BeatFloor(float time, int factor, int sep) 
    {
        int fMin = (int)Math.Pow(sep, Math.Max(factor, 0));
        int fMax = (int)Math.Pow(sep, Math.Max(-factor, 0));
        return new(
            (int)(Mathf.Floor(time / fMin) * fMin),
            fMax == 1 ? 0 : (int)(Mathf.Floor(time % 1 * fMax)),
            fMax
        );
    }
    BeatPosition BeatInterval(int factor, int sep) 
    {
        int fMin = (int)Math.Pow(sep, Math.Max(factor, 0));
        int fMax = (int)Math.Pow(sep, Math.Max(-factor, 0));
        return new(0, fMin, fMax);
    }

    static int GetSepFactor(BeatPosition time, int sep) 
    {
        if (time.Denominator == 1) 
        {
            if (time.Number == 0) return int.MaxValue;
            int s = 0;
            while (time.Number % Mathf.Pow(sep, s + 1) == 0) s++;
            return s;
        }
        else 
        {
            return -Mathf.RoundToInt(Mathf.Log(time.Denominator, sep));
        }
    }

    Color GetBeatColor(BeatPosition time)
    {
        switch (time.Denominator)
        {
            case 1:      return Themer.main.Keys["TimelineTickMain"];
            case 2:      return Themer.main.Keys["TimelineTick2"];
            case 4:      return Themer.main.Keys["TimelineTick4"];
            case 8:      return Themer.main.Keys["TimelineTick8"];
            case 3:      return Themer.main.Keys["TimelineTick3"];
            case 6:      return Themer.main.Keys["TimelineTick6"];
            default:     return Themer.main.Keys["TimelineTickOther"];
        }
    }

    float InverseLerpUnclamped(float start, float end, float value)
    {
        return (value - start) / (end - start);
    }

    Vector2 lastLimit;
    
    TimelineDragMode dragMode;
    Vector2 dragStart, dragEnd;
    float timeStart, timeEnd, beatStart, beatEnd;
    public bool isDragged { get; private set; }
    PointerEventData lastDrag;

    public void OnPointerDown(PointerEventData eventData)
    {
        bool contains(RectTransform rt) => RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.pressPosition, eventData.pressEventCamera);
        bool localPos(RectTransform rt, out Vector2 pos) => RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.pressPosition, eventData.pressEventCamera, out pos);
        isDragged = false;
        lastDrag = eventData;
        DraggingItem = null;

        if (contains(ItemsHolder))
        {
            if (eventData.button == PointerEventData.InputButton.Middle)
                dragMode = TimelineDragMode.TimelineDrag;
            else if (eventData.button == PointerEventData.InputButton.Right || PickerPanel.main.CurrentTimelinePickerMode == TimelinePickerMode.Select)
                dragMode = TimelineDragMode.Select;
            else
                dragMode = TimelineDragMode.Timeline;

            localPos(ItemsHolder, out dragStart);

            dragEnd = dragStart;
            timeStart = timeEnd = Mathf.Lerp(PeekRange.x, PeekRange.y, dragStart.x / ItemsHolder.rect.width);

            Metronome metronome = Chartmaker.main.CurrentSong.Timing;
            beatStart = RoundBeat(timeStart);

            if (eventData.button == PointerEventData.InputButton.Left)
                Chartmaker.main.SongSource.time = Mathf.Clamp(metronome.ToSeconds(beatStart), 0, Chartmaker.main.SongSource.clip.length);
        }
        else if (contains(PeekStartSlider))
        {
            dragMode = TimelineDragMode.PeekStart;
            localPos(PeekStartSlider, out dragStart);
        }
        else if (contains(PeekEndSlider))
        {
            dragMode = TimelineDragMode.PeekEnd;
            localPos(PeekEndSlider, out dragStart);
        }
        else if (contains(CurrentTimeSlider))
        {
            dragMode = TimelineDragMode.CurrentTime;
            localPos(CurrentTimeSlider, out dragStart);
        }
        else if (contains(PeekRangeSlider))
        {
            dragMode = TimelineDragMode.PeekRange;
            localPos(PeekRangeSlider, out dragStart);
        }
        else 
        {
            dragMode = TimelineDragMode.None;
        }
    }

    public void BeginDragItem(IList items, PointerEventData eventData) 
    {
        DraggingItem = items;
        dragMode = TimelineDragMode.ItemDrag;
        
        bool localPos(RectTransform rt, out Vector2 pos) => RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.pressPosition, eventData.pressEventCamera, out pos);
        localPos(ItemsHolder, out dragStart);

        dragEnd = dragStart;
        timeStart = timeEnd = Mathf.Lerp(PeekRange.x, PeekRange.y, dragStart.x / ItemsHolder.rect.width);
        
        Metronome metronome = Chartmaker.main.CurrentSong.Timing;
        beatStart = RoundBeat(timeStart);
        
        ChartmakerHistory history = Chartmaker.main.History;
        var last = history.ActionsBehind.Count == 0 ? null : history.ActionsBehind.Peek();
        if (last is ChartmakerTimelineDragFloatAction lastMove && lastMove.Targets == DraggingItem)
        {
            DraggingItemOffset = lastMove.Value;
        } 
        else 
        {
            DraggingItemOffset = 0;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragged = true;
        if (dragMode == TimelineDragMode.None) return;

        Chartmaker cm = Chartmaker.main;
        Vector2 limit = new(
            Mathf.Min(PeekRange.x, PeekLimit.x),
            Mathf.Max(PeekRange.y, PeekLimit.y)
        );
        float width = limit.y - limit.x;
        float time;
        
        bool localPos(RectTransform rt, out Vector2 pos) => RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out pos);
        
        // Timeline dragging

        if ((int)dragMode % 2 == 1)
        {

            localPos(ItemsHolder, out dragEnd);
            timeEnd = Mathf.Lerp(PeekRange.x, PeekRange.y, dragEnd.x / ItemsHolder.rect.width);

            Metronome metronome = Chartmaker.main.CurrentSong.Timing;
            beatEnd = RoundBeat(timeEnd);

            if (DraggingItem != null) 
            {
                if (DraggingItem.Count > 0 && DraggingItem[0] is not Lane) 
                {
                    ChartmakerHistory history = Chartmaker.main.History;
                    if (DraggingItem[0] is BPMStop) 
                    {
                        ChartmakerTimelineDragFloatAction action;
                        var last = history.ActionsBehind.Count == 0 ? null : history.ActionsBehind.Peek();
                        if (last is ChartmakerTimelineDragFloatAction lastMove && lastMove.Targets == DraggingItem)
                        {
                            action = lastMove;
                            action.Undo();
                        } 
                        else 
                        {
                            action = new ChartmakerTimelineDragFloatAction {
                                Targets = DraggingItem,
                            };
                            history.ActionsBehind.Push(action);
                        }
                        action.Value = (DraggingItem[0] is BPMStop ? timeEnd - timeStart : beatEnd - beatStart) + DraggingItemOffset;
                        action.Redo();
                    }
                    else
                    {
                        ChartmakerTimelineDragBeatPositionAction action;
                        var last = history.ActionsBehind.Count == 0 ? null : history.ActionsBehind.Peek();
                        if (last is ChartmakerTimelineDragBeatPositionAction lastMove && lastMove.Targets == DraggingItem)
                        {
                            action = lastMove;
                            action.Undo();
                        } 
                        else 
                        {
                            action = new ChartmakerTimelineDragBeatPositionAction {
                                Targets = DraggingItem,
                            };
                            history.ActionsBehind.Push(action);
                        }
                        action.Value = ToRoundedBeat((DraggingItem[0] is BPMStop ? timeEnd - timeStart : beatEnd - beatStart) + DraggingItemOffset);
                        action.Redo();
                    }
                    history.ActionsAhead.Clear();
                    Chartmaker.main.OnHistoryDo();
                    Chartmaker.main.OnHistoryUpdate();
                }
            } 
            else if (dragMode == TimelineDragMode.TimelineDrag)
            {
                float offset = Mathf.Clamp(-eventData.delta.x * (PeekRange.y - PeekRange.x) / TicksHolder.rect.width, limit.x - PeekRange.x, limit.y - PeekRange.y);
                PeekRange.x += offset;
                PeekRange.y += offset;
            }
            else if (dragMode == TimelineDragMode.Select)
            {
                SelectionRect.gameObject.SetActive(true);
                
                if (CurrentMode is TimelineMode.Storyboard or TimelineMode.HitObjects)
                {
                    SelectionRect.anchorMin = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, Mathf.Min(timeStart, timeEnd)), 0);
                    SelectionRect.anchorMax = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, Mathf.Max(timeStart, timeEnd)), 0);
                    SelectionRect.anchoredPosition = new (0, Mathf.Round(Mathf.Min(dragStart.y, dragEnd.y)));
                    SelectionRect.sizeDelta = new (0, Mathf.Round(Mathf.Abs(dragStart.y - dragEnd.y)));
                }
                else
                {
                    SelectionRect.anchorMin = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, Mathf.Min(timeStart, timeEnd)), 0);
                    SelectionRect.anchorMax = new (InverseLerpUnclamped(PeekRange.x, PeekRange.y, Mathf.Max(timeStart, timeEnd)), 1);
                    SelectionRect.anchoredPosition = SelectionRect.sizeDelta = new (0, 0);
                }
            }
            else if (dragMode == TimelineDragMode.Timeline)
            {
                Chartmaker.main.SongSource.time = Mathf.Clamp(metronome.ToSeconds(beatEnd), 0, Chartmaker.main.SongSource.clip.length);
            }

            return;
        }

        // Slider dragging

        if (localPos(TimeSliderHolder, out Vector2 localMousePos))
        {
            float sliderWidth = TimeSliderHolder.rect.width;
            time = ((localMousePos - dragStart).x / sliderWidth + TimeSliderHolder.pivot.x) * width + limit.x;
        }
        else
        {
            return;
        }
        
        if (dragMode == TimelineDragMode.CurrentTime)
        {
            if (cm.SongSource.time == 0 && !cm.SongSource.isPlaying)
            {
                cm.SongSource.Play();
                cm.SongSource.Pause();
            }
            cm.SongSource.timeSamples = (int)Mathf.Clamp(time * cm.SongSource.clip.frequency, 0, cm.SongSource.clip.samples - 1);
        }
        else if (dragMode == TimelineDragMode.PeekRange)
        {
            float mid = (PeekRange.x + PeekRange.y) / 2;
            float offset = Mathf.Clamp(time - mid, limit.x - PeekRange.x, limit.y - PeekRange.y);
            PeekRange.x += offset;
            PeekRange.y += offset;
        }
        else if (dragMode == TimelineDragMode.PeekStart)
        {
            PeekRange.x = Mathf.Clamp(time, limit.x, PeekRange.y);
        }
        else if (dragMode == TimelineDragMode.PeekEnd)
        {
            PeekRange.y = Mathf.Clamp(time, PeekRange.x, limit.y);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragged)
        {
            Metronome metronome = Chartmaker.main.CurrentSong.Timing;
            if (eventData.button == PointerEventData.InputButton.Right)
                Chartmaker.main.SongSource.time = Mathf.Clamp(metronome.ToSeconds(beatStart), 0, Chartmaker.main.SongSource.clip.length);
            
            OnEndDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DraggingItem != null) 
        {
            DraggingItem = null;
        }
        else if (dragMode == TimelineDragMode.Select)
        {
            Metronome metronome = Chartmaker.main.CurrentSong.Timing;
            float beatStart = metronome.ToBeat(Mathf.Min(timeStart, timeEnd));
            float beatEnd = metronome.ToBeat(Mathf.Max(timeEnd, timeStart));

            IList list = null;

            switch (CurrentMode)
            {
                case TimelineMode.Storyboard:
                {
                    if (InspectorPanel.main.CurrentObject is not IStoryboardable thing) break;

                    TimestampType[] types = (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null);
                    Storyboard sb = thing.Storyboard;

                    int yStart = Mathf.FloorToInt(Mathf.Clamp((ItemsHolder.rect.height - Mathf.Max(dragStart.y, dragEnd.y) - 3) / 24, 0, TimelineHeight - 1)) + ScrollOffset;
                    int yEnd = Mathf.FloorToInt(Mathf.Clamp((ItemsHolder.rect.height - Mathf.Min(dragStart.y, dragEnd.y) - 3) / 24, 0, TimelineHeight - 1)) + ScrollOffset;

                    list = sb.Timestamps.FindAll(x =>
                    {
                        int index = Array.FindIndex(types, y => x.ID == y.ID);
                        return x.Offset >= beatStart && x.Offset <= beatEnd && index >= yStart && index <= yEnd;
                    });
                }
                break;

                case TimelineMode.Timing:
                {
                    list = Chartmaker.main.CurrentSong.Timing.Stops.FindAll(x =>
                    {
                        return x.Offset >= timeStart && x.Offset <= timeEnd;
                    });
                }
                break;

                case TimelineMode.Lanes:
                {
                    if (Chartmaker.main.CurrentChart == null) break;

                    list = Chartmaker.main.CurrentChart.Lanes.FindAll(x =>
                    {
                        return x.LaneSteps[0].Offset >= beatStart && x.LaneSteps[0].Offset <= beatEnd;
                    });
                }
                break;

                case TimelineMode.LaneSteps:
                {
                    if (InspectorPanel.main.CurrentHierarchyObject is not Lane lane) break;

                    list = lane.LaneSteps.FindAll(x =>
                    {
                        return x.Offset >= beatStart && x.Offset <= beatEnd;
                    });
                }
                break;

                case TimelineMode.HitObjects:
                {
                    if (InspectorPanel.main.CurrentHierarchyObject is not Lane lane) break;

                    float vpStart = .5f - VerticalScale * .5f + VerticalOffset;
                    float vpEnd = .5f + VerticalScale * .5f + VerticalOffset;

                    float yStart = Mathf.Lerp(vpStart, vpEnd, Mathf.Clamp01(1 - (Mathf.Max(dragStart.y, dragEnd.y) - 4) / (ItemsHolder.rect.height - 8)));
                    float yEnd = Mathf.Lerp(vpStart, vpEnd, Mathf.Clamp01(1 - (Mathf.Min(dragStart.y, dragEnd.y) - 4) / (ItemsHolder.rect.height - 8)));

                    list = lane.Objects.FindAll(x =>
                    {
                        return x.Offset >= beatStart && x.Offset <= beatEnd && x.Position <= yEnd && x.Position + x.Length >= yStart;
                    });
                }
                break;
            }

            if (list?.Count >= 2) InspectorPanel.main.SetObject(list);
            else if (list?.Count == 1) InspectorPanel.main.SetObject(list[0]);
        }
        else if (dragMode == TimelineDragMode.Timeline)
        {
            if (!Chartmaker.main.SongSource.isPlaying)
            {
                TimelinePickerMode pickMode = PickerPanel.main.CurrentTimelinePickerMode;
                
                Metronome metronome = Chartmaker.main.CurrentSong.Timing;

                float density = (PeekRange.y - PeekRange.x) * metronome.GetStop(timeEnd, out _).BPM / TicksHolder.rect.width / 8;
                float factor = Mathf.Floor(Mathf.Log(density, SeparationFactor));
                float step = Mathf.Pow(SeparationFactor, factor + 1);
                float beat = Mathf.Round(metronome.ToBeat(timeEnd) / step) * step;

                switch (PickerPanel.main.CurrentTimelinePickerMode) 
                {
                    case TimelinePickerMode.Timestamp:
                    {
                        if (InspectorPanel.main.CurrentObject is not IStoryboardable thing) break;

                        TimestampType[] types = (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null);
                        Storyboard sb = thing.Storyboard;
                        TimestampType type = types[Math.Clamp(Mathf.FloorToInt((ItemsHolder.rect.height - dragEnd.y - 3) / 24) + ScrollOffset, 0, types.Length - 1)];
                        Chartmaker.main.AddItem(new Timestamp {
                            ID = type.ID,
                            Offset = (BeatPosition)(isDragged ? Mathf.Min(beatStart, beatEnd) : beatStart),
                            Duration = isDragged ? Mathf.Abs(beatStart - beatEnd) : 0,
                            Target = type.Get(thing.Get(isDragged ? Mathf.Min(beatStart, beatEnd) : beatStart)),
                        });
                    }
                    break;
                    case TimelinePickerMode.BPMStop:
                    {
                        if (isDragged) break;
                        BPMStop baseStop = Chartmaker.main.CurrentSong.Timing.GetStop(timeStart, out _);
                        
                        Chartmaker.main.AddItem(new BPMStop(baseStop.BPM, timeStart) { Signature = baseStop.Signature });
                    }
                    break;
                    case TimelinePickerMode.Lane:
                    {
                        Lane lane = new Lane {
                            Position = new(0, -4, 0)
                        };
                        lane.LaneSteps.Add(new LaneStep{ 
                            StartPos = new(-8, 0),
                            EndPos = new(8, 0),
                            Offset = (BeatPosition)(isDragged ? Math.Min(beatStart, beatEnd) : beatStart)
                        });
                        lane.LaneSteps.Add(new LaneStep{ 
                            StartPos = new(-8, 0),
                            EndPos = new(8, 0),
                            Offset = (BeatPosition)(isDragged ? Math.Max(beatStart, beatEnd) : beatStart + 1),
                        });
                        Chartmaker.main.AddItem(lane);
                    }
                    break;
                    case TimelinePickerMode.LaneStep:
                    {
                        if (isDragged) break;

                        if (InspectorPanel.main.CurrentHierarchyObject is not Lane lane) break;

                        LanePosition basePos = ((Lane)lane.Get(timeEnd)).GetLanePosition(timeStart, timeStart, metronome);
                        LaneStep baseStep = new() {
                            StartPos = basePos.StartPos,
                            EndPos = basePos.EndPos,
                            Offset = (BeatPosition)(isDragged ? beatEnd : beatStart),
                        };

                        Chartmaker.main.AddItem(baseStep);
                    }
                    break;
                    case TimelinePickerMode.NormalHit or TimelinePickerMode.CatchHit:
                    {
                        HitObject hit = null;

                        if (InspectorPanel.main.CurrentObject is HitObject baseHit) hit = baseHit.DeepClone();
                        else hit = new() { Length = 1 };

                        if (isDragged) 
                        {
                            hit.Offset = (BeatPosition)(Math.Min(beatStart, beatEnd));
                            hit.HoldLength = Math.Abs(beatStart - beatEnd);
                            float vpStart = .5f - VerticalScale * .5f + VerticalOffset;
                            float vpEnd = .5f + VerticalScale * .5f + VerticalOffset;
                            float yStart = Mathf.Lerp(vpStart, vpEnd, Mathf.Round(Mathf.Clamp01(1 - (Mathf.Max(dragStart.y, dragEnd.y) - 4) / (ItemsHolder.rect.height - 8)) / .05f) * .05f);
                            float yEnd = Mathf.Lerp(vpStart, vpEnd, Mathf.Round(Mathf.Clamp01(1 - (Mathf.Min(dragStart.y, dragEnd.y) - 4) / (ItemsHolder.rect.height - 8)) / .05f) * .05f);
                            if (yStart != yEnd) 
                            {
                                hit.Position = yStart;
                                hit.Length = yEnd - yStart;
                            }
                        } 
                        else 
                        {
                            hit.Offset = (BeatPosition)beatStart;
                            hit.HoldLength = 0;
                        }

                        hit.Type = PickerPanel.main.CurrentTimelinePickerMode == TimelinePickerMode.CatchHit ? HitObject.HitType.Catch : HitObject.HitType.Normal;

                        Chartmaker.main.AddItem(hit);
                    }
                    break;
                }
            }
        }

        isDragged = false;
        dragMode = TimelineDragMode.None;
        SelectionRect.gameObject.SetActive(false);
    }

    public void ShowEditHistory()
    {
        ChartmakerHistory history = Chartmaker.main.History;
        ContextMenuList list = new();
        var ahead = history.ActionsAhead.ToArray();
        var behind = history.ActionsBehind.ToArray();

        if (ahead.Length != 0) for (int a = Mathf.Min(ahead.Length, 10) - 1; a >= 0; a--)
        {
            int A = a;
            list.Items.Add(new ContextMenuListAction(ahead[a].GetName(), () => Chartmaker.main.Redo(A + 1), icon: "Redo"));
        }

        list.Items.Add(new ContextMenuListAction(
            ahead.Length == 0 && behind.Length == 0 ? "No edit history" : "↑ Redo || Undo ↓",
            () => {}, _enabled: false
        ));

        if (behind.Length != 0) for (int a = 0; a < Mathf.Min(behind.Length, 10); a++)
        {
            int A = a;
            list.Items.Add(new ContextMenuListAction(behind[a].GetName(), () => Chartmaker.main.Undo(A + 1), icon: "Undo"));
        }

        ContextMenuHolder.main.OpenRoot(list, EditHistoryHolder, ContextMenuDirection.Up);
    }

    public void OnResizerDrag()
    {
        ResizeTimeline(Input.mousePosition.y, false);
    }
    public void OnResizerEndDrag()
    {
        ResizeTimeline(Input.mousePosition.y);
    }

    public float SnapTimeline(float height)
    {
        return height < 72 ? 40 : Mathf.Max(Mathf.Round((height - 80) / 24) * 24 + 80, 104);
    }
    
    public void ResizeTimeline(float height, bool snap = true)
    {
        float maxHeight = SnapTimeline(Screen.height * 0.5f);
        height = Mathf.Round(Mathf.Clamp(height, 40, maxHeight));
        if (snap) height = SnapTimeline(height);
        Chartmaker.main.TimelineHolder.anchoredPosition = new (
            Chartmaker.main.TimelineHolder.sizeDelta.x, 
            -Mathf.Pow(Mathf.Max(104 - height, 0) / 64, 2) * 32
        );
        Chartmaker.main.TimelineHolder.sizeDelta = new (
            Chartmaker.main.TimelineHolder.sizeDelta.x, 
            height - Chartmaker.main.TimelineHolder.anchoredPosition.y
        );
        Chartmaker.main.MainViewHolder.sizeDelta = new (Chartmaker.main.MainViewHolder.sizeDelta.x, - 33 - height);
        Chartmaker.main.PickerHolder.sizeDelta = new (
            Chartmaker.main.PickerHolder.sizeDelta.x, 
            -32 - Chartmaker.main.TimelineHolder.anchoredPosition.y + Chartmaker.main.PickerHolder.anchoredPosition.y
        );
        CurrentTimeCoonectorGroup.alpha = PeekSliderGroup.alpha = BlockerTextGroup.alpha =
            1 + Chartmaker.main.TimelineHolder.anchoredPosition.y / 32;
        TimelineHeight = height <= 40 ? 0 : Mathf.Max(Mathf.RoundToInt((height - 80) / 24), 1);
        if (snap) 
        {
            if (TimelineHeight > 0) TimelineExpandHeight = TimelineHeight;
            UpdateTabs();
        }
        UpdateTimeline(true);
        UpdateScrollbar();
        PlayerView.main.Update();
        PlayerView.main.UpdateObjects();
    }
    
    public void Collapse()
    {
        TimelineRestoreHeight = TimelineHeight;
        ResizeTimeline(40);
    }
    
    public void Restore()
    {
        if (TimelineHeight <= 0) ResizeTimeline(TimelineRestoreHeight * 24 + 80);
    }

    public void OnScroll(PointerEventData eventData)
    {
        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        Chartmaker cm = Chartmaker.main;

        // Ctrl+Shift modifier = Vertical zoom
        if (isCtrl && isShift)
        {
            if (CurrentMode == TimelineMode.HitObjects)
            {
                float zoom = Mathf.Pow(2, ResizeVelocity * -eventData.scrollDelta.y / 10f);
                Options.VerticalScale = VerticalScale *= zoom;
                Options.UpdateFields();
            }
            UpdateTimeline(true);
        }
        // Shift modifier = Vertical scroll
        else if (isShift)
        {
            if (CurrentMode == TimelineMode.HitObjects)
            {
                Options.VerticalOffset = VerticalOffset += -eventData.scrollDelta.y * VerticalScale / 10;
                Options.UpdateFields();
            }
            else 
            {
                ScrollOffset = Mathf.Max(Mathf.Min(ScrollOffset + (int)Mathf.Sign(-eventData.scrollDelta.y), ItemHeight - TimelineHeight), 0);
            }
            UpdateTimeline(true);
            UpdateScrollbar();
        }
        // Ctrl modifier = Horizontal zoom
        else if (isCtrl)
        {
            float zoom = Mathf.Pow(2, ResizeVelocity * -eventData.scrollDelta.y / 10f);
            float center = GetPointerTimeAtTimeline(eventData);
            float currentXRange = PeekRange.x - (center - PeekRange.x) * (zoom - 1);
            float currentYRange = PeekRange.y - (center - PeekRange.y) * (zoom - 1);

            Debug.Log($"{PeekRange.x} -> {currentXRange}, {PeekRange.y} -> {currentYRange}");

            PeekRange.x = Mathf.Clamp(currentXRange, PeekLimit.x, PeekRange.y);
            PeekRange.y = Mathf.Clamp(currentYRange, PeekRange.x, PeekLimit.y);
        }
        // Alt modifier = Seek current time
        else if (isAlt || (Options.FollowSeekLine && cm.SongSource.isPlaying))
        {

            Metronome metronome = cm.CurrentSong.Timing;
            float bpm = metronome.GetStop(cm.SongSource.time, out _).BPM;
            float density = (PeekRange.y - PeekRange.x) * bpm / TicksHolder.rect.width / 8;
            float factor = Mathf.Floor(Mathf.Log(density, SeparationFactor));
            float step = Mathf.Pow(SeparationFactor, factor + 1);

            float time = cm.SongSource.time + (-eventData.scrollDelta.y * step / bpm * 240);
            if (cm.SongSource.time == 0 && !cm.SongSource.isPlaying)
            {
                cm.SongSource.Play();
                cm.SongSource.Pause();
            }
            cm.SongSource.time = Mathf.Clamp(time, 0, cm.SongSource.clip.length);
        }
        // No modifier = Horizontal scroll
        else
        {
            float offset = Mathf.Clamp(
                (PeekRange.y - PeekRange.x) / TicksHolder.rect.width * 50 * -eventData.scrollDelta.y,
                PeekLimit.x - PeekRange.x,
                PeekLimit.y - PeekRange.y
            );

            PeekRange.x += offset;
            PeekRange.y += offset;
        }
    }

    public float GetPointerTimeAtTimeline(PointerEventData eventData)
    {
        Chartmaker cm = Chartmaker.main;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(TimeSliderHolder, eventData.position, eventData.pressEventCamera, out Vector2 mousePosition))
        {
            return Mathf.Lerp(PeekRange.x, PeekRange.y, mousePosition.x / ItemsHolder.rect.width + .5f);
        }
        else
        {
            return cm.SongSource.time;
        }
    }

    public void RightClickItem(TimelineItem item)
    {
        static string KeyOf(string id) => KeyboardHandler.main.Keybindings[id].Keybind.ToString();

        if (item.Lane != null) InspectorPanel.main.SetObject(item.Lane);
        InspectorPanel.main.SetObject(item.Item);
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            new ContextMenuListAction("Cut", Chartmaker.main.Cut, KeyOf("ED:Cut"), 
                icon: "Cut", _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Copy", Chartmaker.main.Copy, KeyOf("ED:Copy"), 
                icon: "Copy", _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Paste <i>" + (Chartmaker.main.CanPaste() ? Chartmaker.GetItemName(Chartmaker.main.ClipboardItem) : ""), Chartmaker.main.Paste, KeyOf("ED:Paste"), 
                icon: "Paste", _enabled: Chartmaker.main.CanPaste()),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Delete", () => KeyboardHandler.main.Keybindings["ED:Delete"].Invoke(), KeyOf("ED:Delete"), 
                _enabled: Chartmaker.main.CanCopy())
        ), (RectTransform)item.transform, ContextMenuDirection.Cursor);
    }
}

public enum TimelineMode
{
    Storyboard,
    Timing,
    Lanes,
    LaneSteps,
    HitObjects,
}

public enum TimelineDragMode
{
    None = 0,

    CurrentTime = 2,
    PeekRange = 4,
    PeekStart = 6,
    PeekEnd = 8,

    TimelineDrag = 1,
    Timeline = 3,
    Select = 5,
    ItemDrag = 7,
}

public enum FrequencyScale
{
    Linear,
    Logarithmic,
    Mel,
    Bark,
}