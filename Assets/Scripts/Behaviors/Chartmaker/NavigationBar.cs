using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavigationBar : MonoBehaviour
{
    [Header("Objects")]
    public RectTransform FileButton;
    public RectTransform EditButton;
    public RectTransform ToolsButton;
    public RectTransform OptionsButton;
    public RectTransform HelpButton;
    public RectTransform MenuButton;

    string KeyOf(string id) => KeyboardHandler.main.Keybindings[id].Keybind.ToString();

    public void OpenFileMenu()
    {
        ContextMenuHolder.main.OpenRoot(GetFileMenu(), FileButton);
    }

    public void OpenEditMenu()
    {
        ContextMenuHolder.main.OpenRoot(GetEditMenu(), EditButton);
    }

    public void OpenToolsMenu()
    {
        ContextMenuHolder.main.OpenRoot(GetToolsMenu(), ToolsButton);
    }

    public void OpenOptionsMenu()
    {
        ContextMenuHolder.main.OpenRoot(GetOptionsMenu(), OptionsButton);
    }

    public void OpenHelpMenu()
    {
        ContextMenuHolder.main.OpenRoot(GetHelpMenu(), HelpButton);
    }

    public void OpenMenu()
    {
        if (Chartmaker.main.CurrentSong != null)
        {
            ContextMenuHolder.main.OpenRoot(new ContextMenuList(
                new ContextMenuListSublist("File", GetFileMenu().Items.ToArray()),
                new ContextMenuListSublist("Edit", GetEditMenu().Items.ToArray()),
                new ContextMenuListSublist("Options", GetOptionsMenu().Items.ToArray()),
                new ContextMenuListSublist("Help", GetHelpMenu().Items.ToArray())
            ), MenuButton);
        }
        else 
        {
            ContextMenuHolder.main.OpenRoot(new ContextMenuList(
                new ContextMenuListSublist("File", GetFileMenu().Items.ToArray()),
                new ContextMenuListSublist("Options", GetOptionsMenu().Items.ToArray()),
                new ContextMenuListSublist("Help", GetHelpMenu().Items.ToArray())
            ), MenuButton);
        }
    }

    public ContextMenuList GetFileMenu()
    {
        return new ContextMenuList(
            new ContextMenuListAction("New Song...", () => ModalHolder.main.Spawn<NewSongModal>(), KeyOf("FL:New")),
            new ContextMenuListAction("Open Song...", Chartmaker.main.OpenSongModal, KeyOf("FL:Open")),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Create Chart...", () => ModalHolder.main.Spawn<NewChartModal>(), _enabled: Chartmaker.main.CurrentSong != null),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Save", Chartmaker.main.StartSaveRoutine, KeyOf("FL:Save"), _enabled: Chartmaker.main.CurrentSong != null),
            new ContextMenuListSeparator(),
            // new ContextMenuListSublist("Export", 
            //     new ContextMenuListAction("Record Video...", () => {}),
            //     new ContextMenuListAction("Bundle...", () => {})
            // ),
            // new ContextMenuListSeparator(),
            new ContextMenuListAction("Close Song", Chartmaker.main.TryCloseSong, _enabled: Chartmaker.main.CurrentSong != null),
            new ContextMenuListAction("Exit Chartmaker", Application.Quit)
        );
    }

    public ContextMenuList GetEditMenu()
    {
        return new ContextMenuList(
            new ContextMenuListAction("Undo", () => Chartmaker.main.Undo(), KeyOf("ED:Undo"), icon: "Undo", _enabled: Chartmaker.main.History.ActionsBehind.Count > 0),
            new ContextMenuListAction("Redo", () => Chartmaker.main.Redo(), KeyOf("ED:Redo"), icon: "Redo", _enabled: Chartmaker.main.History.ActionsAhead.Count > 0),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Cut", Chartmaker.main.Cut, KeyOf("ED:Cut"), icon: "Cut", _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Copy", Chartmaker.main.Copy, KeyOf("ED:Copy"), icon: "Copy", _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Paste", Chartmaker.main.Paste, KeyOf("ED:Paste"), icon: "Paste", _enabled: Chartmaker.main.CanPaste()),
            new ContextMenuListAction("Delete", () => KeyboardHandler.main.Keybindings["ED:Delete"].Invoke(), KeyOf("ED:Delete"), _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Select All", () => KeyboardHandler.main.Keybindings["ED:SelectAll"].Invoke(), KeyOf("ED:SelectAll")),
            new ContextMenuListAction("Invert Selection", InvertSelection)
        );
    }

    public ContextMenuList GetToolsMenu()
    {
        return new ContextMenuList(
            new ContextMenuListSublist("Modules", 
                new ContextMenuListAction("Import Module...", () => {})
            )
        );
    }

    public ContextMenuList GetOptionsMenu()
    {
        return new ContextMenuList(
            new ContextMenuListAction("Preferences...", () => ModalHolder.main.Spawn<PreferencesModal>()),
            new ContextMenuListAction("Show Keybindings...", () => ModalHolder.main.Spawn<PreferencesModal>().SetTab(1))
        );
    }

    public ContextMenuList GetHelpMenu()
    {
        return new ContextMenuList(
            new ContextMenuListAction("Chartmaker Manual...", () => ModalHolder.main.Spawn<HelpModal>()),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Source Code on GitHub", () => Application.OpenURL("https://github.com/ducdat0507/JANOARG"), icon: "Github Icon"),
            new ContextMenuListAction("Report an Issue / Suggestion", () => Application.OpenURL("https://github.com/ducdat0507/JANOARG/issues")),
            new ContextMenuListAction("FFF40 Studios Discord Server", () => Application.OpenURL("https://discord.gg/vXJTPFQBHm"), icon: "Discord Icon"),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("About Chartmaker...", () => ModalHolder.main.Spawn<AboutModal>(), icon: "Credits")
        );
    }





    public void InvertSelection() 
    {
        IList list = InspectorPanel.main.CurrentObject is IList li ? li : new List<object> { InspectorPanel.main.CurrentObject };
        if (TimelinePanel.main.CurrentMode == TimelineMode.Storyboard) {
            if (InspectorPanel.main.CurrentObject is IStoryboardable) InspectorPanel.main.SetObject(((IStoryboardable)InspectorPanel.main.CurrentObject).Storyboard.Timestamps.FindAll(x => InspectorPanel.main.CurrentTimestamp?.Contains(x) == false));
        } else if (TimelinePanel.main.CurrentMode == TimelineMode.Lanes) {
            if (Chartmaker.main.CurrentChart != null) InspectorPanel.main.SetObject(Chartmaker.main.CurrentChart.Lanes.FindAll(x => !list.Contains(x)));
        } else if (TimelinePanel.main.CurrentMode == TimelineMode.LaneSteps) {
            if (InspectorPanel.main.CurrentLane != null) InspectorPanel.main.SetObject(InspectorPanel.main.CurrentLane.LaneSteps.FindAll(x => !list.Contains(x)));
        } else if (TimelinePanel.main.CurrentMode == TimelineMode.HitObjects) {
            if (InspectorPanel.main.CurrentLane != null) InspectorPanel.main.SetObject(InspectorPanel.main.CurrentLane.Objects.FindAll(x => !list.Contains(x)));
        } else if (TimelinePanel.main.CurrentMode == TimelineMode.Timing) {
            if (Chartmaker.main.CurrentSong != null) InspectorPanel.main.SetObject(Chartmaker.main.CurrentSong.Timing.Stops.FindAll(x => !list.Contains(x)));
        }
    }
}
