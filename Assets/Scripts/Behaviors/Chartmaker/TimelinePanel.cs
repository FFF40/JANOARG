using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimelinePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
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
    public Image WaveformImage;
    [Space]
    public Scrollbar VerticalScrollbar;
    public GameObject Blocker;
    public TMP_Text BlockerLabel;
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

    const int TimelineHeight = 5;
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
        LaneStepTab.gameObject.SetActive(InspectorPanel.main.CurrentLane != null);
        HitObjectTab.gameObject.SetActive(InspectorPanel.main.CurrentLane != null);

        StoryboardTab.interactable = CurrentMode != TimelineMode.Storyboard;
        TimingTab.interactable = CurrentMode != TimelineMode.Timing;
        LaneTab.interactable = CurrentMode != TimelineMode.Lanes;
        LaneStepTab.interactable = CurrentMode != TimelineMode.LaneSteps;
        HitObjectTab.interactable = CurrentMode != TimelineMode.HitObjects;

        PickerPanel.main.UpdateButtons();
    }

    public void SetTabMode(int mode) => SetTabMode((TimelineMode)mode);

    public void SetTabMode(TimelineMode mode)
    {
        CurrentMode = mode;

        UpdateTabs();
        UpdateItems();
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
            if (InspectorPanel.main.CurrentLane != null)
            {
                foreach (LaneStep step in InspectorPanel.main.CurrentLane.LaneSteps)
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
            if (InspectorPanel.main.CurrentLane != null)
            {
                float height = ItemsHolder.rect.height - 8;
                float dOffset = 4 * density;

                foreach (HitObject hit in InspectorPanel.main.CurrentLane.Objects)
                {
                    float time = metronome.ToSeconds(hit.Offset);
                    float timeEnd = metronome.ToSeconds(hit.Offset + hit.HoldLength);
                    if (timeEnd < PeekRange.x - dOffset || time > PeekRange.y + dOffset) continue;

                    float start = hit.Position;
                    float end = hit.Position + hit.Length;
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

    public void UpdateWaveform()
    {
        if (PeekRange.y == PeekRange.x) 
        {
            WaveformImage.enabled = false;
            return;
        }
        if (Options.Waveform < (Chartmaker.main.SongSource.isPlaying ? 2 : 1)) 
        {
            WaveformImage.enabled = false;
            return;
        }
        
        WaveformImage.enabled = true;
        Color color = Themer.main.Keys["TimelineTickMain"];

        Sprite wave = WaveformImage.sprite;
        RectTransform waveRT = WaveformImage.rectTransform;
        if (!wave || wave.texture.width != (int)waveRT.rect.width || wave.texture.height != (int)waveRT.rect.height)
        {
            WaveformImage.sprite = wave = Sprite.Create(
                new Texture2D((int)waveRT.rect.width, (int)waveRT.rect.height), 
                new Rect(0, 0, waveRT.rect.width, waveRT.rect.height), 
                new Vector2(.5f, .5f)
            );
        }

        Texture2D tex = wave.texture;
        AudioClip clip = Chartmaker.main.SongSource.clip;

        float density = clip.frequency * (PeekRange.y - PeekRange.x) / tex.width;
        if (density > 128) density = 28 * Mathf.Log(density, 128);

        float step = (PeekRange.y - PeekRange.x) / tex.width;
        float sec = Mathf.Floor(PeekRange.x / step) * step;

        switch (Options.WaveformMode) 
        {
            case 0: {
                float[] data = new float[Mathf.CeilToInt(Mathf.Max(density, 32))];
                float denY = 1f / tex.height;
                Color[] buffer = new Color[tex.height];
                
                for (int x = 0; x < tex.width; x++) 
                {
                    float sum = 0;
                    int pos = (int)(sec * clip.frequency);
                    if (pos >= 0 && pos < clip.samples - data.Length)
                    {
                        clip.GetData(data, pos);
                        for (int a = 0; a < data.Length; a++)
                        {
                            int chan = (a + pos) % clip.channels;
                            sum += data[a] * data[a];
                        }
                    }
                    sum = Mathf.Sqrt(sum / data.Length) * .8f;
                    float sPos = 0;
                    for (int y = 0; y < tex.height; y++) 
                    {
                        float window = 1 - sPos * 2f;
                        buffer[y] = window >= -sum - denY && window <= sum + denY ? color : Color.clear;
                        sPos += denY;
                    }
                    tex.SetPixels(x, 0, 1, tex.height, buffer);
                    sec += step;
                }
            } break;

            case 1: {
                float[] data = new float[Mathf.CeilToInt(density / clip.channels) * clip.channels];
                float denY = 1f / tex.height * clip.channels;
                float[] lastMin = new float[clip.channels], lastMax = new float[clip.channels];
                Color[] buffer = new Color[tex.height];

                for (int a = 0; a < clip.channels; a++)
                {
                    lastMin[a] = 1;
                    lastMax[a] = -1;
                }

                for (int x = 0; x < tex.width; x++) 
                {
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
                        buffer[y] = window >= min[channel] - denY && window <= max[channel] + denY ? color : Color.clear;
                        sPos += denY;
                    }
                    tex.SetPixels(x, 0, 1, tex.height, buffer);
                    sec += step;
                }
            } break;
        }

        tex.Apply();
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
                Mathf.FloorToInt(beat),
                Mathf.RoundToInt((beat % 1) * step),
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
            else if (eventData.button == PointerEventData.InputButton.Right || PickerPanel.main.CurrentMode == PickerMode.Select)
                dragMode = TimelineDragMode.Select;
            else
                dragMode = TimelineDragMode.Timeline;

            localPos(ItemsHolder, out dragStart);

            dragEnd = dragStart;
            timeStart = timeEnd = Mathf.Lerp(PeekRange.x, PeekRange.y, dragStart.x / ItemsHolder.rect.width);

            Metronome metronome = Chartmaker.main.CurrentSong.Timing;
            beatStart = RoundBeat(timeStart);

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
            cm.SongSource.time = Mathf.Clamp(time, 0, cm.SongSource.clip.length);
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
                    if (InspectorPanel.main.CurrentLane == null) break;

                    list = InspectorPanel.main.CurrentLane.LaneSteps.FindAll(x =>
                    {
                        return x.Offset >= beatStart && x.Offset <= beatEnd;
                    });
                }
                break;

                case TimelineMode.HitObjects:
                {
                    if (InspectorPanel.main.CurrentLane == null) break;

                    float yStart = Mathf.Lerp(0, 1, Mathf.Clamp01(1 - (Mathf.Max(dragStart.y, dragEnd.y) - 4) / (ItemsHolder.rect.height - 8)));
                    float yEnd = Mathf.Lerp(0, 1, Mathf.Clamp01(1 - (Mathf.Min(dragStart.y, dragEnd.y) - 4) / (ItemsHolder.rect.height - 8)));

                    list = InspectorPanel.main.CurrentLane.Objects.FindAll(x =>
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
                PickerMode pickMode = PickerPanel.main.CurrentMode;
                
                Metronome metronome = Chartmaker.main.CurrentSong.Timing;

                float density = (PeekRange.y - PeekRange.x) * metronome.GetStop(timeEnd, out _).BPM / TicksHolder.rect.width / 8;
                float factor = Mathf.Floor(Mathf.Log(density, SeparationFactor));
                float step = Mathf.Pow(SeparationFactor, factor + 1);
                float beat = Mathf.Round(metronome.ToBeat(timeEnd) / step) * step;

                switch (PickerPanel.main.CurrentMode) 
                {
                    case PickerMode.Timestamp:
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
                    case PickerMode.BPMStop:
                    {
                        if (isDragged) break;
                        BPMStop baseStop = Chartmaker.main.CurrentSong.Timing.GetStop(timeStart, out _);
                        
                        Chartmaker.main.AddItem(new BPMStop(baseStop.BPM, timeStart) { Signature = baseStop.Signature });
                    }
                    break;
                    case PickerMode.Lane:
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
                    case PickerMode.LaneStep:
                    {
                        if (isDragged) break;

                        Lane lane = InspectorPanel.main.CurrentLane;
                        if (lane == null) break;

                        LanePosition basePos = ((Lane)lane.Get(timeEnd)).GetLanePosition(timeStart, timeStart, metronome);
                        LaneStep baseStep = new() {
                            StartPos = basePos.StartPos,
                            EndPos = basePos.EndPos,
                            Offset = (BeatPosition)(isDragged ? beatEnd : beatStart),
                        };

                        Chartmaker.main.AddItem(baseStep);
                    }
                    break;
                    case PickerMode.NormalHit or PickerMode.CatchHit:
                    {
                        HitObject hit = null;

                        if (InspectorPanel.main.CurrentObject is HitObject baseHit) hit = baseHit.DeepClone();
                        else hit = new() { Length = 1 };

                        if (isDragged) 
                        {
                            hit.Offset = (BeatPosition)(Math.Min(beatStart, beatEnd));
                            hit.HoldLength = Math.Abs(beatStart - beatEnd);
                            float yStart = Mathf.Lerp(0, 1, Mathf.Round(Mathf.Clamp01(1 - (Mathf.Max(dragStart.y, dragEnd.y) - 4) / (ItemsHolder.rect.height - 8)) / .05f) * .05f);
                            float yEnd = Mathf.Lerp(0, 1, Mathf.Round(Mathf.Clamp01(1 - (Mathf.Min(dragStart.y, dragEnd.y) - 4) / (ItemsHolder.rect.height - 8)) / .05f) * .05f);
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

                        hit.Type = PickerPanel.main.CurrentMode == PickerMode.CatchHit ? HitObject.HitType.Catch : HitObject.HitType.Normal;

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