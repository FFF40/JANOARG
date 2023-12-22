using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Chartmaker : MonoBehaviour
{
    public static Chartmaker main;

    public string CurrentSongPath;
    public string CurrentChartPath;
    [Space]
    public AudioSource SongSource;
    [Space]
    public RectTransform NavBarItemHolder;
    public RectTransform InfoBarHolder;
    public RectTransform TimelineHolder;
    public RectTransform InspectorHolder;
    public RectTransform PickerHolder;
    public RectTransform PlayerViewHolder;
    [Space]
    public TMP_Text NotificationLabel;
    public CanvasGroup NotificationText;
    public CanvasGroup NotificationBox;
    public float NotificationTime;
    public float NotificationFlashTime;
    [Space]
    public GameObject Loader;
    public TMP_Text LoaderLabel;
    public bool IsDirty;

    public PlayableSong CurrentSong { get; private set; } = null;
    public Chart CurrentChart { get; private set; } = null;
    public ExternalChartMeta CurrentChartMeta { get; private set; } = null;

    public object ClipboardItem;
    public ChartmakerHistory History = new();

    public static Storage PreferencesStorage;
    public Storage KeybindingsStorage;
    public Storage RecentSongsStorage;
    public static ChartmakerPrefs Preferences = new();
    [Space]
    public Themer Themer;

    public Task ActiveTask;

    bool lastPlayed;

    public void Awake()
    {
        main = this;
        KeybindingsStorage = new("cm_keys");
        RecentSongsStorage = new("cm_recent");
        Preferences.Load(PreferencesStorage);
        Themer.InitTheme();
    }

    public void Start()
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        ModalHolder.main.Spawn<HomeModal>();
        Application.wantsToQuit += QuitCheck;
        InformationBar.main.PlayOptions.Init();
        TimelinePanel.main.Options.Init();
        SetEditorActive(false);
    }

    public void Update()
    {
        NotificationText.alpha = NotificationTime;
        NotificationBox.alpha = NotificationFlashTime / .5f;
        NotificationTime -= Time.deltaTime;
        NotificationFlashTime -= Time.deltaTime;

        if (Preferences.SaveOnPlay && SongSource.isPlaying && !lastPlayed && IsDirty && ActiveTask?.IsCompleted != false)
        {
            StartSaveRoutine();
        }
        lastPlayed = SongSource.isPlaying;
    }

    public void SetEditorActive(bool value)
    {
        NavBarItemHolder.gameObject.SetActive(value);
        InfoBarHolder.gameObject.SetActive(value);
        TimelineHolder.gameObject.SetActive(value);
        InspectorHolder.gameObject.SetActive(value);
        PickerHolder.gameObject.SetActive(value);
        PlayerViewHolder.gameObject.SetActive(value);
    }

    public void OpenSongModal() 
    {
        FileModal modal = ModalHolder.main.Spawn<FileModal>();
        modal.AcceptedTypes = new List<FileModalFileType> {
            new("JANOARG Playable Song file", "japs"),
            new("All files"),
        };
        modal.HeaderLabel.text = "Select a Playable Song...";
        modal.SelectLabel.text = "Open";
        modal.OnSelect.AddListener(() => {
            StartCoroutine(OpenSongRoutine(modal.SelectedEntry.Path));
        });
    }

    public void OpenSong(string path)
    {
        CurrentChart = null;
        CurrentChartMeta = null;
        CurrentChartPath = "";
        CurrentSong = JAPSDecoder.Decode(File.ReadAllText(path));
        CurrentSongPath = path;
    }

    public Task OpenSongAsync(string path)
    {
        return Task.Run(() => OpenSong(path));
    }

    public void AddToRecent()
    {
        List<RecentSong> list = new(RecentSongsStorage.Get("List", new RecentSong[] {}));
        int index = list.FindIndex(x => x.Path == CurrentSongPath);
        if (index == 0 && list[0].SongName == CurrentSong.SongName && list[0].SongArtist == CurrentSong.SongArtist) return;
        else if (index >= 0) list.RemoveAt(index);
        list.Insert(0, new RecentSong {
            Path = CurrentSongPath,
            SongName = CurrentSong.SongName,
            SongArtist = CurrentSong.SongArtist,
        });
        RecentSongsStorage.Set("List", list.ToArray());
        RecentSongsStorage.Save();
    }

    public Task AddToRecentAsync()
    {
        return Task.Run(AddToRecent);
    }
    
    public IEnumerator OpenSongRoutine(string path) {
        if (ActiveTask?.IsCompleted == false) yield break;

        Loader.SetActive(true);
        LoaderLabel.text = "Parsing Playable Song data...";

        ActiveTask = OpenSongAsync(path);
        yield return new WaitUntil(() => ActiveTask.IsCompleted);
        if (!ActiveTask.IsCompletedSuccessfully) 
        {
            Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Parsing Error", ActiveTask.Exception.Message, new string[] {"Ok"}, _ => {});
            yield break;
        }

        ActiveTask = AddToRecentAsync();
        yield return new WaitUntil(() => ActiveTask.IsCompleted);

        LoaderLabel.text = "Fetching audio file...";

        UnityWebRequest stream = UnityWebRequestMultimedia.GetAudioClip("file://" + Path.Combine(Path.GetDirectoryName(path), CurrentSong.ClipPath), AudioType.UNKNOWN);
        Debug.Log(stream.url);
        yield return stream.SendWebRequest();

        if (stream.result != UnityWebRequest.Result.Success)
        {
            Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Fetch Error", "Couldn't fetch the audio file!\n" + stream.error, new string[] {"Ok"}, _ => {});
            yield break;
        }
        else
        {
            try
            {
                SongSource.clip = CurrentSong.Clip = DownloadHandlerAudioClip.GetContent(stream);
            }
            catch (Exception e)
            {
                Loader.SetActive(false);
                DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
                modal.SetDialog("Fetch Error", "Couldn't fetch the audio file!\n" + e.Message, new string[] {"Ok"}, _ => {});
                yield break;
            }
        }

        Loader.SetActive(false);
        if (HomeModal.main) HomeModal.main.Close();

        SongSource.time = 0;
        InformationBar.main.UpdateSongButton();
        InformationBar.main.UpdateChartButton();
        InspectorPanel.main.UpdateButtons();
        InspectorPanel.main.SetObject(null);
        InspectorPanel.main.CurrentLane = null;
        TimelinePanel.main.UpdatePeekLimit();
        TimelinePanel.main.UpdateItems();
        PlayerView.main.UpdateObjects();
        History = new();
        OnHistoryUpdate();
        ClipboardItem = null;
        OnClipboardUpdate();

        BorderlessWindow.RenameWindow(CurrentSong.SongArtist + " - " + CurrentSong.SongName + " // JANOARG Chartmaker");
        
        SetEditorActive(true);
    }
    

    public void OpenChart(ExternalChartMeta chart)
    {
        string path = Path.Combine(Path.GetDirectoryName(CurrentSongPath), chart.Target + ".jac");
        CurrentChart = JACDecoder.Decode(File.ReadAllText(path));
        CurrentChartPath = path;
        CurrentChartMeta = chart;
    }

    public Task OpenChartAsync(ExternalChartMeta chart)
    {
        return Task.Run(() => OpenChart(chart));
    }
    
    public IEnumerator OpenChartRoutine(ExternalChartMeta chart) {
        if (ActiveTask?.IsCompleted == false) yield break;
        
        Loader.SetActive(true);
        LoaderLabel.text = "Parsing Playable Song data...";

        ActiveTask = OpenChartAsync(chart);
        yield return new WaitUntil(() => ActiveTask.IsCompleted);
        if (!ActiveTask.IsCompletedSuccessfully) 
        {
            Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Parsing Error", ActiveTask.Exception.Message, new string[] {"Ok"}, _ => {});
            yield break;
        }

        Loader.SetActive(false);
        InformationBar.main.UpdateChartButton();
        InspectorPanel.main.UpdateButtons();
        InspectorPanel.main.UpdateForm();
        InspectorPanel.main.CurrentLane = null;
        TimelinePanel.main.UpdateItems();
        PlayerView.main.UpdateObjects();
        History = new();
        OnHistoryUpdate();
        ClipboardItem = null;
        OnClipboardUpdate();
    }

    public void Save()
    {
        if (CurrentSong != null) File.WriteAllText(CurrentSongPath, JAPSEncoder.Encode(CurrentSong, CurrentSong.ClipPath));
        if (CurrentChart != null) File.WriteAllText(CurrentChartPath, JACEncoder.Encode(CurrentChart));
        IsDirty = false;
    }

    public Task SaveAsync()
    {
        return Task.Run(Save);
    }
    
    public IEnumerator SaveRoutine() {
        if (ActiveTask?.IsCompleted == false) yield break;
        if (CurrentSong == null) yield break;
        
        NotificationLabel.text = "Saving song data...";
        NotificationTime = float.PositiveInfinity;

        ActiveTask = SaveAsync();
        yield return new WaitUntil(() => ActiveTask.IsCompleted);
        if (!ActiveTask.IsCompletedSuccessfully) 
        {
        Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", ActiveTask.Exception.Message, new string[] {"Ok"}, _ => {});
            yield break;
        }

        NotificationLabel.text = "Song data saved!";
        NotificationFlashTime = 0.5f;
        NotificationTime = 3;
    }
    
    public IEnumerator SaveThenQuit() {
        if (ActiveTask?.IsCompleted == false) yield break;
        if (CurrentSong == null) yield break;

        Loader.SetActive(true);
        LoaderLabel.text = "Saving song before quitting...";

        yield return SaveRoutine();

        if (!IsDirty) Application.Quit();
    }
    
    public void StartSaveRoutine() {
        StartCoroutine(SaveRoutine());
    }
    
    public IEnumerator SavePrefsRoutine() {
        if (ActiveTask?.IsCompleted == false) yield break;
        
        NotificationLabel.text = "Saving preferences...";
        NotificationTime = float.PositiveInfinity;

        ActiveTask = Task.Run(PreferencesStorage.Save);
        yield return new WaitUntil(() => ActiveTask.IsCompleted);
        if (!ActiveTask.IsCompletedSuccessfully) 
        {
            Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", ActiveTask.Exception.Message, new string[] {"Ok"}, _ => {});
            yield break;
        }

        NotificationLabel.text = "Preferences saved!";
        NotificationFlashTime = 0.5f;
        NotificationTime = 3;
    }
    
    public void StartSavePrefsRoutine() {
        StartCoroutine(SavePrefsRoutine());
    }
    
    
    public void CloseSong() {
        CurrentSongPath = CurrentChartPath = "";
        CurrentSong = null;
        CurrentChart = null;

        if (!HomeModal.main) ModalHolder.main.Spawn<HomeModal>();

        SongSource.time = 0;
        SetEditorActive(false);
        PlayerView.main.MainCamera.rect = new (0, 0, 1, 1);
        
        BorderlessWindow.RenameWindow("JANOARG Chartmaker");

        IsDirty = false;
    }
    
    public void TryCloseSong() {
        DirtyModal(CloseSong);
    }
    
    public bool QuitCheck() 
    {
        if (!IsDirty) WindowHandler.main.Quit();
        else 
        {
            if (Preferences.SaveOnQuit) StartCoroutine(SaveThenQuit());
            else DirtyModal(() => {
                IsDirty = false;
                Application.Quit();
            });
        }
        return !IsDirty;
    }
    
    DialogModal dirtyDialog;
    public void DirtyModal(Action action) 
    {
        if (dirtyDialog)
        {
            return;
        }
        else if (IsDirty)
        {
            dirtyDialog = ModalHolder.main.Spawn<DialogModal>();
            dirtyDialog.SetDialog("Close Song", "Would you like to save changes made to " + CurrentSong.SongName + "?", new [] {"Save", "Don't Save", "Cancel"}, a => {
                if (a == 0) { Save(); action(); }
                else if (a == 1) { action(); }
            });
        }
        else
        {
            action();
        }
    }
    

    public static string GetItemName(object item) => item switch
    {
        IList list =>   list.Count > 0 ? (list.Count > 1 ? list.Count + " " + GetItemName(list[0]) + "s" : GetItemName(list[0])) : "Empty List",
        Chart =>        "Chart",
        BPMStop =>      "BPM Stop",
        HitStyle =>     "Hit Style",
        LaneStyle =>    "Lane Style",
        LaneGroup =>    "Lane Group",
        Lane =>         "Lane",
        LaneStep =>     "Lane Step",
        HitObject =>    "Hit Object",
        _ =>            item.ToString()
    };

    public void OnHistoryDo()
    {
        InspectorPanel.main?.UpdateForm();
        TimelinePanel.main?.UpdateItems();
        PlayerView.main?.UpdateObjects();
        IsDirty = true;
    }

    bool recursionBuster;

    public void OnHistoryUpdate()
    {
        TimelinePanel tl = TimelinePanel.main;

        tl.UndoButton.interactable = History.ActionsBehind.Count > 0;
        tl.UndoButtonGroup.alpha = tl.UndoButton.interactable ? 1 : .5f;

        tl.RedoButton.interactable = History.ActionsAhead.Count > 0;
        tl.RedoButtonGroup.alpha = tl.RedoButton.interactable ? 1 : .5f;

        tl.ActionsBehindCounter.text = Mathf.Min(History.ActionsBehind.Count, 999).ToString();
        tl.ActionsAheadCounter.text = Mathf.Min(History.ActionsAhead.Count, 999).ToString();
    }

    public void SetItem(object target, string field, object value)
    {
        Debug.Log(History.ActionsBehind.Count + " " + History.ActionsAhead.Count + " " + recursionBuster);
        if (recursionBuster) return;
        History.SetItem(target, field, value);
        if (field == "Offset") SortList(GetListTarget(target));
        TimelinePanel.main?.UpdateItems();
        PlayerView.main?.UpdateObjects();
        IsDirty = true;
        OnHistoryUpdate();
    }

    public IList GetListTarget(object obj) => obj switch {
        IList list  => list.Count > 0 ? GetListTarget(list[0]) : throw new ArgumentException("Can't determine list target of an empty list"),
        Timestamp   => ((IStoryboardable)InspectorPanel.main.CurrentObject).Storyboard.Timestamps,
        BPMStop     => CurrentSong.Timing.Stops,
        LaneStyle   => CurrentChart.Pallete.LaneStyles,
        HitStyle    => CurrentChart.Pallete.HitStyles,
        LaneGroup   => CurrentChart.Groups,
        Lane        => CurrentChart.Lanes,
        LaneStep    => InspectorPanel.main.CurrentLane.LaneSteps,
        HitObject   => InspectorPanel.main.CurrentLane.Objects,
        null        => throw new ArgumentException("Object can't be null"),
        _           => throw new ArgumentException("No list target found for " + obj.GetType()),
    };

    public static void SortList(IList list)
    {
        if (list is List<BPMStop> bsl)          bsl.Sort((x, y) => x.Offset.CompareTo(y.Offset));
        else if (list is List<Timestamp> tsl)   tsl.Sort((x, y) => x.Offset.CompareTo(y.Offset));
        else if (list is List<Lane> lal)        lal.Sort((x, y) => x.LaneSteps[0].Offset.CompareTo(y.LaneSteps[0].Offset));
        else if (list is List<LaneStep> lsl)    lsl.Sort((x, y) => x.Offset.CompareTo(y.Offset));
        else if (list is List<HitObject> hol)   hol.Sort((x, y) => x.Offset.CompareTo(y.Offset));
    }

    public void DeleteItem(object obj, bool setNull = true)
    {
        ChartmakerDeleteAction action = new ChartmakerDeleteAction {
            Target = GetListTarget(obj),
            Item = obj,
        };
        action.Redo();
        History.AddAction(action);
        OnHistoryDo();
        OnHistoryUpdate();
        if (setNull) InspectorPanel.main.UnsetObject();
    }

    public void AddItem(object obj)
    {
        ChartmakerAddAction action = new ChartmakerAddAction {
            Target = GetListTarget(obj),
            Item = obj,
        };
        action.Redo();
        History.AddAction(action);
        OnHistoryDo();
        OnHistoryUpdate();
        InspectorPanel.main.SetObject(obj);
    }
    public void AddItem(object obj, float startingOffset)
    {
        IList list = obj is IList l ? l : new [] { obj };
        FieldInfo field = list[0].GetType().GetField("Offset");
        if (list[0] is BPMStop fbs)
        {
            float offset = startingOffset - fbs.Offset;
            foreach (object item in list)
            {
                BPMStop stop = (BPMStop)item;
                stop.Offset += offset;
            }
        }
        AddItem(list);
    }

    public void AddItem(object obj, BeatPosition startingOffset)
    {
        IList list = obj is IList l ? l : new [] { obj };
        FieldInfo field = list[0].GetType().GetField("Offset");
        if (list[0] is Lane fl)
        {
            BeatPosition offset = startingOffset - fl.LaneSteps[0].Offset;
            foreach (object item in list)
            {
                Lane lane = (Lane)item;
                foreach (LaneStep step in lane.LaneSteps) 
                {
                    step.Offset += offset;
                    foreach (Timestamp ts in step.Storyboard.Timestamps) ts.Offset += offset;
                }
                foreach (HitObject hit in lane.Objects)
                {
                    hit.Offset += offset;
                    foreach (Timestamp ts in hit.Storyboard.Timestamps) ts.Offset += offset;
                }
            }
        }
        else if (field != null)
        {
            BeatPosition offset = startingOffset - (BeatPosition)field.GetValue(list[0]);
            foreach (object item in list)
            {
                field.SetValue(item, (BeatPosition)field.GetValue(item) + offset);
                if (item is IStoryboardable isb)
                {
                    foreach (Timestamp ts in isb.Storyboard.Timestamps) ts.Offset += offset;
                }
            }
        }
        AddItem(list);
    }
    public List<T> DeepClone<T>(List<T> obj) where T : IDeepClonable<T>
    {
        List<T> newList = new (); 
        foreach (object item in obj) newList.Add(DeepClone((T)item)); 
        return newList;
    }

    public T DeepClone<T>(T obj) where T : IDeepClonable<T>
    {
        return obj.DeepClone();
    }

    public object SmartClone(object obj) => obj switch {
        List<Timestamp> lts  => DeepClone(lts),
        List<BPMStop> lbs    => DeepClone(lbs),
        List<LaneStyle> lls  => DeepClone(lls),
        List<HitStyle> lhs   => DeepClone(lhs),
        List<LaneGroup> llg  => DeepClone(llg),
        List<Lane> lla       => DeepClone(lla),
        List<LaneStep> lst   => DeepClone(lst),
        List<HitObject> lho  => DeepClone(lho),
        Timestamp ts  => ts.DeepClone(),
        BPMStop bs    => bs.DeepClone(),
        LaneStyle ls  => ls.DeepClone(),
        HitStyle hs   => hs.DeepClone(),
        LaneGroup lg  => lg.DeepClone(),
        Lane la       => la.DeepClone(),
        LaneStep st   => st.DeepClone(),
        HitObject ho  => ho.DeepClone(),
        null          => throw new ArgumentException("Object can't be null"),
        _             => obj,
    };

    public void Undo(int times = 1)
    {
        recursionBuster = true;
        History.Undo(times);
        OnHistoryDo();
        OnHistoryUpdate();
        recursionBuster = false;
    }

    public void Redo(int times = 1)
    {
        recursionBuster = true;
        History.Redo(times);
        OnHistoryDo();
        OnHistoryUpdate();
        recursionBuster = false;
    }

    public bool CanCopy()
    {
        if (InspectorPanel.main.CurrentTimestamp?.Count > 0) return true;
        object currentItem = InspectorPanel.main.CurrentObject;
        return currentItem is not (null or PlayableSong or Chart or Pallete or CameraController) && currentItem != CurrentChart?.Groups;
    }

    public bool CanPaste()
    {
        return ClipboardItem != null;
    }

    public void OnClipboardUpdate()
    {
        TimelinePanel tl = TimelinePanel.main;
        object currentItem = InspectorPanel.main.CurrentObject;
        
        tl.CutButton.interactable = tl.CopyButton.interactable = CanCopy();
        tl.CutButtonGroup.alpha = tl.CopyButtonGroup.alpha = tl.CutButton.interactable ? 1 : .5f;

        tl.PasteButton.interactable = CanPaste();
        tl.PasteButtonGroup.alpha = tl.PasteButton.interactable ? 1 : .5f;
    }
    
    public void Cut()
    {
        if (!CanCopy()) return;
        if ((InspectorPanel.main.CurrentTimestamp?.Count ?? 0) > 0) ClipboardItem = InspectorPanel.main.CurrentTimestamp;
        else ClipboardItem = InspectorPanel.main.CurrentObject;
        DeleteItem(ClipboardItem);
        InspectorPanel.main.UnsetObject();
    }

    public void Copy()
    {
        if (!CanCopy()) return;
        if ((InspectorPanel.main.CurrentTimestamp?.Count ?? 0) > 0) ClipboardItem = InspectorPanel.main.CurrentTimestamp;
        else ClipboardItem = InspectorPanel.main.CurrentObject;
        OnClipboardUpdate();
    }

    public void Paste()
    {
        if (!CanPaste()) return;
        object obj = SmartClone(ClipboardItem);
        if (obj is BPMStop or List<BPMStop>)
        {
            AddItem(obj, SongSource.time);
        }
        else 
        {
            AddItem(obj, TimelinePanel.main.ToRoundedBeat(CurrentSong.Timing.ToBeat(SongSource.time)));
        }
        InspectorPanel.main.SetObject(obj);
    }
}

public class ChartmakerPrefs {
    public bool SaveOnQuit;
    public bool SaveOnPlay;

    public string Theme = "Prototype";
    public bool UseDefaultWindow;

    public void Load(Storage storage)
    {
        SaveOnPlay = storage.Get("AS:SaveOnPlay", SaveOnPlay);
        SaveOnQuit = storage.Get("AS:SaveOnQuit", SaveOnQuit);

        Theme = storage.Get("AP:Theme", Theme);
        UseDefaultWindow = storage.Get("LA:UseDefaultWindow", UseDefaultWindow);
    }
}
