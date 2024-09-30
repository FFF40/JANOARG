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
    public RectTransform GlobalsButton;
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

    public void OpenGlobalsMenu()
    {
        ContextMenuHolder.main.OpenRoot(GetGlobalsMenu(), GlobalsButton);
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
                new ContextMenuListSublist("Globals", GetGlobalsMenu().Items.ToArray()),
                new ContextMenuListSublist("Help", GetHelpMenu().Items.ToArray())
            ), MenuButton);
        }
        else 
        {
            ContextMenuHolder.main.OpenRoot(new ContextMenuList(
                new ContextMenuListSublist("File", GetFileMenu().Items.ToArray()),
                new ContextMenuListSublist("Globals", GetGlobalsMenu().Items.ToArray()),
                new ContextMenuListSublist("Help", GetHelpMenu().Items.ToArray())
            ), MenuButton);
        }
    }

    public ContextMenuList GetFileMenu()
    {
        ContextMenuListItem openChartItem = null;
        if (Chartmaker.main.CurrentSong == null || Chartmaker.main.CurrentSong.Charts.Count <= 0)
        {
            openChartItem = new ContextMenuListAction("Open Chart", () => {}, _enabled: false);
        }
        else 
        {
            ContextMenuListSublist list = new ("Open Chart");
            openChartItem = list;
            foreach (ExternalChartMeta chart in Chartmaker.main.CurrentSong.Charts)
            {
                ExternalChartMeta _chart = chart;
                list.Items.Items.Add(new ContextMenuListAction(chart.DifficultyName + " " + chart.DifficultyLevel, () => {
                    StartCoroutine(Chartmaker.main.OpenChartRoutine(_chart));
                }));
            }
        }

        return new ContextMenuList(
            new ContextMenuListAction("New Song...", () => ModalHolder.main.Spawn<NewSongModal>(), KeyOf("FL:New")),
            new ContextMenuListAction("Open Song...", Chartmaker.main.OpenSongModal, KeyOf("FL:Open")),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Create Chart...", () => ModalHolder.main.Spawn<NewChartModal>(), _enabled: Chartmaker.main.CurrentSong != null),
            openChartItem,
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Save", Chartmaker.main.StartSaveRoutine, KeyOf("FL:Save"), _enabled: Chartmaker.main.CurrentSong != null),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Render...", () => ModalHolder.main.Spawn<RenderModal>(), _enabled: Chartmaker.main.CurrentChart != null),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Reveal Song Folder", () => Application.OpenURL("file://" + System.IO.Path.GetDirectoryName(Chartmaker.main.CurrentSongPath)), _enabled: Chartmaker.main.CurrentSong != null),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Close Song", Chartmaker.main.TryCloseSong, _enabled: Chartmaker.main.CurrentSong != null),
            new ContextMenuListAction("Exit Chartmaker", Application.Quit)
        );
    }

    public ContextMenuList GetEditMenu()
    {
        bool canUndo = Chartmaker.main.History.ActionsBehind.TryPeek(out IChartmakerAction actionBehind);
        bool canRedo = Chartmaker.main.History.ActionsAhead.TryPeek(out IChartmakerAction actionAhead);

        return new ContextMenuList(
            new ContextMenuListAction("Undo <i>" + (actionBehind?.GetName() ?? ""), () => Chartmaker.main.Undo(), KeyOf("ED:Undo"), icon: "Undo", _enabled: canUndo),
            new ContextMenuListAction("Redo <i>" + (actionAhead?.GetName() ?? ""), () => Chartmaker.main.Redo(), KeyOf("ED:Redo"), icon: "Redo", _enabled: canRedo),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Cut", Chartmaker.main.Cut, KeyOf("ED:Cut"), icon: "Cut", _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Copy", Chartmaker.main.Copy, KeyOf("ED:Copy"), icon: "Copy", _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Paste <i>" + (Chartmaker.main.CanPaste() ? Chartmaker.GetItemName(Chartmaker.main.ClipboardItem) : ""), Chartmaker.main.Paste, KeyOf("ED:Paste"), icon: "Paste", _enabled: Chartmaker.main.CanPaste()),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Rename", () => HierarchyPanel.main.RenameCurrent(), KeyOf("ED:Rename"), _enabled: Chartmaker.main.CanRename()),
            new ContextMenuListAction("Delete", () => KeyboardHandler.main.Keybindings["ED:Delete"].Invoke(), KeyOf("ED:Delete"), _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListSeparator(),
            new ContextMenuListSublist("Timeline", 
                new ContextMenuListAction("Select All", () => KeyboardHandler.main.Keybindings["ED:SelectAll"].Invoke(), KeyOf("ED:SelectAll")),
                new ContextMenuListAction("Invert Selection", InvertSelection)
            )
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

    public ContextMenuList GetGlobalsMenu()
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
            new ContextMenuListAction("Source Code on GitHub", () => Application.OpenURL("https://github.com/FFF40/JANOARG"), icon: "Github Icon"),
            new ContextMenuListAction("Report an Issue / Suggestion", () => Application.OpenURL("https://github.com/FFF40/JANOARG/issues")),
            new ContextMenuListAction("FFF40 Studios Discord Server", () => Application.OpenURL("https://discord.gg/vXJTPFQBHm"), icon: "Discord Icon"),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Check for Updates", () => VersionCheckerModal.InitFetch()),
            new ContextMenuListAction("Show All Releases", () => Application.OpenURL("https://github.com/FFF40/JANOARG/releases")),
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
            if (InspectorPanel.main.CurrentHierarchyObject is Lane lane) InspectorPanel.main.SetObject(lane.LaneSteps.FindAll(x => !list.Contains(x)));
        } else if (TimelinePanel.main.CurrentMode == TimelineMode.HitObjects) {
            if (InspectorPanel.main.CurrentHierarchyObject is Lane lane) InspectorPanel.main.SetObject(lane.Objects.FindAll(x => !list.Contains(x)));
        } else if (TimelinePanel.main.CurrentMode == TimelineMode.Timing) {
            if (Chartmaker.main.CurrentSong != null) InspectorPanel.main.SetObject(Chartmaker.main.CurrentSong.Timing.Stops.FindAll(x => !list.Contains(x)));
        }
    }
}
