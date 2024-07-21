using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class KeyboardHandler : MonoBehaviour
{
    public static KeyboardHandler main;
    public FormEntryKeybind ChangingKeybind;
    public DialogModal ChangingModal;

    public KeybindActionList Keybindings = new KeybindActionList {
        // -------------------------------------------------- General
        { "GE:PlayPause", new KeybindAction {
            Category = "General",
            Name = "Play / Pause",
            Keybind = new Keybind(KeyCode.Space),
            Invoke = () => InformationBar.main.ToggleSong(),
        }},

        // -------------------------------------------------- File
        { "FL:New", new KeybindAction {
            Category = "File",
            Name = "New Song",
            Keybind = new Keybind(KeyCode.N, EventModifiers.Command),
            Invoke = () => ModalHolder.main.Spawn<NewSongModal>(),
        }},
        { "FL:Open", new KeybindAction {
            Category = "File",
            Name = "Open Song",
            Keybind = new Keybind(KeyCode.O, EventModifiers.Command),
            Invoke = () => Chartmaker.main.OpenSongModal(),
        }},
        { "FL:Save", new KeybindAction {
            Category = "File",
            Name = "Save Song",
            Keybind = new Keybind(KeyCode.S, EventModifiers.Command),
            Invoke = () => Chartmaker.main.StartSaveRoutine(),
        }},

        // -------------------------------------------------- Edit
        { "ED:Cut", new KeybindAction {
            Category = "Edit",
            Name = "Cut",
            Keybind = new Keybind(KeyCode.X, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Cut(),
        }},
        { "ED:Copy", new KeybindAction {
            Category = "Edit",
            Name = "Copy",
            Keybind = new Keybind(KeyCode.C, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Copy(),
        }},
        { "ED:Paste", new KeybindAction {
            Category = "Edit",
            Name = "Paste",
            Keybind = new Keybind(KeyCode.V, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Paste(),
        }},
        { "ED:SelectAll", new KeybindAction {
            Category = "Edit",
            Name = "Select All",
            Keybind = new Keybind(KeyCode.A, EventModifiers.Command),
            Invoke = () => { 
                if (TimelinePanel.main.CurrentMode == TimelineMode.Storyboard) {
                    if (InspectorPanel.main.CurrentObject is IStoryboardable) InspectorPanel.main.SetObject(((IStoryboardable)InspectorPanel.main.CurrentObject).Storyboard.Timestamps.FindAll(x => true));
                } else if (TimelinePanel.main.CurrentMode == TimelineMode.Lanes) {
                    if (Chartmaker.main.CurrentChart != null) InspectorPanel.main.SetObject(Chartmaker.main.CurrentChart.Lanes.FindAll(x => true));
                } else if (TimelinePanel.main.CurrentMode == TimelineMode.LaneSteps) {
                    if (InspectorPanel.main.CurrentHierarchyObject is Lane lane) InspectorPanel.main.SetObject(lane.LaneSteps.FindAll(x => true));
                } else if (TimelinePanel.main.CurrentMode == TimelineMode.HitObjects) {
                    if (InspectorPanel.main.CurrentHierarchyObject is Lane lane) InspectorPanel.main.SetObject(lane.Objects.FindAll(x => true));
                } else if (TimelinePanel.main.CurrentMode == TimelineMode.Timing) {
                    if (Chartmaker.main.CurrentSong != null) InspectorPanel.main.SetObject(Chartmaker.main.CurrentSong.Timing.Stops.FindAll(x => true));
                }
            },
        }},
        { "ED:Delete", new KeybindAction {
            Category = "Edit",
            Name = "Delete",
            Keybind = new Keybind(KeyCode.Delete),
            Invoke = () => { 
                if (Chartmaker.main.CanCopy()) 
                    Chartmaker.main.DeleteItem(InspectorPanel.main.CurrentTimestamp.Count > 0 ? InspectorPanel.main.CurrentTimestamp : InspectorPanel.main.CurrentObject);
            },
        }},
        { "ED:Undo", new KeybindAction {
            Category = "Edit",
            Name = "Undo",
            Keybind = new Keybind(KeyCode.Z, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Undo(),
        }},
        { "ED:Redo", new KeybindAction {
            Category = "Edit",
            Name = "Redo",
            Keybind = new Keybind(KeyCode.Y, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Redo(),
        }},

        // -------------------------------------------------- Timeline
        { "TL:Timing", new KeybindAction {
            Category = "Timeline",
            Name = "Timing",
            Keybind = new Keybind(KeyCode.BackQuote),
            Invoke = () => TimelinePanel.main.SetTabMode(TimelineMode.Timing),
        }},
        { "TL:Storyboard", new KeybindAction {
            Category = "Timeline",
            Name = "Storyboard",
            Keybind = new Keybind(KeyCode.Alpha1),
            Invoke = () => TimelinePanel.main.SetTabMode(TimelineMode.Storyboard),
        }},
        { "TL:Lanes", new KeybindAction {
            Category = "Timeline",
            Name = "Lanes",
            Keybind = new Keybind(KeyCode.Alpha2),
            Invoke = () => TimelinePanel.main.SetTabMode(TimelineMode.Lanes),
        }},
        { "TL:LaneStep", new KeybindAction {
            Category = "Timeline",
            Name = "Lane Steps",
            Keybind = new Keybind(KeyCode.Alpha3),
            Invoke = () => TimelinePanel.main.SetTabMode(TimelineMode.LaneSteps),
        }},
        { "TL:HitObjects", new KeybindAction {
            Category = "Timeline",
            Name = "Hit Objects",
            Keybind = new Keybind(KeyCode.Alpha4),
            Invoke = () => TimelinePanel.main.SetTabMode(TimelineMode.HitObjects),
        }},

        // -------------------------------------------------- Picker
        { "PK:Cursor", new KeybindAction {
            Category = "Picker",
            Name = "Cursor",
            Keybind = new Keybind(KeyCode.E),
            Invoke = () => PickerPanel.main.SetTabMode(PickerMode.Cursor),
        }},
        { "PK:Select", new KeybindAction {
            Category = "Picker",
            Name = "Select",
            Keybind = new Keybind(KeyCode.R),
            Invoke = () => PickerPanel.main.SetTabMode(PickerMode.Select),
        }},
        { "PK:Delete", new KeybindAction {
            Category = "Picker",
            Name = "Delete",
            Keybind = new Keybind(KeyCode.T),
            Invoke = () => PickerPanel.main.SetTabMode(PickerMode.Delete),
        }},
        { "PK:Item1", new KeybindAction {
            Category = "Picker",
            Name = "1st Item",
            Keybind = new Keybind(KeyCode.F),
            Invoke = () => {
                var mode = TimelinePanel.main.CurrentMode;
                     if (mode == TimelineMode.Storyboard) PickerPanel.main.SetTabMode(PickerMode.Timestamp);
                else if (mode == TimelineMode.Timing) PickerPanel.main.SetTabMode(PickerMode.BPMStop);
                else if (mode == TimelineMode.Lanes) PickerPanel.main.SetTabMode(PickerMode.Lane);
                else if (mode == TimelineMode.LaneSteps) PickerPanel.main.SetTabMode(PickerMode.LaneStep);
                else if (mode == TimelineMode.HitObjects) PickerPanel.main.SetTabMode(PickerMode.NormalHit);
            }
        }},
        { "PK:Item2", new KeybindAction {
            Category = "Picker",
            Name = "2nd Item",
            Keybind = new Keybind(KeyCode.G),
            Invoke = () => {
                var mode = TimelinePanel.main.CurrentMode;
                if (mode == TimelineMode.HitObjects) PickerPanel.main.SetTabMode(PickerMode.CatchHit);
            }
        }},

        // -------------------------------------------------- Selection
        { "SE:PreviousLane", new KeybindAction {
            Category = "Selection",
            Name = "Previous Lane",
            Keybind = new Keybind(KeyCode.UpArrow),
            Invoke = () => {
                if (InspectorPanel.main.CurrentHierarchyObject is Lane lane)
                {
                    var lanes = Chartmaker.main.CurrentChart.Lanes;
                    int index = lanes.IndexOf(lane);
                    InspectorPanel.main.SetObject(lanes[Mathf.Max(index - 1, 0)]);
                }
            },
        }},
        { "SE:NextLane", new KeybindAction {
            Category = "Selection",
            Name = "Next Lane",
            Keybind = new Keybind(KeyCode.DownArrow),
            Invoke = () => {
                if (InspectorPanel.main.CurrentHierarchyObject is Lane lane)
                {
                    var lanes = Chartmaker.main.CurrentChart.Lanes;
                    int index = lanes.IndexOf(lane);
                    InspectorPanel.main.SetObject(lanes[Mathf.Min(index + 1, lanes.Count - 1)]);
                }
            },
        }},
    };
    
    public void Awake() 
    {
        main = this;
    }

    public void Start() 
    {
        Keybindings.LoadKeys();
    }

    public void OnGUI()
    {
        if (EventSystem.current?.currentSelectedGameObject?.GetComponent<TMP_InputField>())
        {
            return;
        }
        if (Event.current.type == EventType.KeyDown)
        {
            if (!ChangingKeybind) Keybindings.HandleEvent(Event.current);
            Event.current.Use();
        }
        else if (Event.current.type == EventType.KeyUp)
        {
            if (ChangingKeybind) EndKeybindChange(new (Event.current));
            Event.current.Use();
        }
    }

    public void StartKeybindChange(FormEntryKeybind target)
    {
        if (ChangingKeybind) return;
        ChangingKeybind = target; 
        ChangingModal = ModalHolder.main.Spawn<DialogModal>();
        ChangingModal.SetDialog("Change Keybind", 
                "Press a key or key combination to change keybind for: \n"
              + "<indent=2em><b>" + target.Category + " > " + target.Title, 
            new [] {"Cancel"}, x => {
                CancelKeybindChange();
            }, false
        );
    }

    public void EndKeybindChange(Keybind keybind)
    {
        if (ChangingModal) ChangingModal.Close();
        ChangingKeybind.SetValue(keybind);
        ChangingKeybind.Reset();
        ChangingKeybind = null; 
    }

    public void CancelKeybindChange()
    {
        ChangingKeybind = null; 
    }
}
