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

    string KeyOf(string id) => KeyboardHandler.main.Keybindings[id].Keybind.ToString();

    public void OpenFileMenu()
    {
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            new ContextMenuListAction("New Song...", () => ModalHolder.main.Spawn<NewSongModal>(), KeyOf("FL:New")),
            new ContextMenuListAction("Open Song...", Chartmaker.main.OpenSongModal, KeyOf("FL:Open")),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Save Song", Chartmaker.main.StartSaveRoutine, KeyOf("FL:Save")),
            new ContextMenuListSeparator(),
            // new ContextMenuListSublist("Export", 
            //     new ContextMenuListAction("Record Video...", () => {}),
            //     new ContextMenuListAction("Bundle...", () => {})
            // ),
            // new ContextMenuListSeparator(),
            new ContextMenuListAction("Close Song", Chartmaker.main.TryCloseSong),
            new ContextMenuListAction("Exit Chartmaker", Application.Quit)
        ), FileButton);
    }

    public void OpenEditMenu()
    {
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            new ContextMenuListAction("Undo", () => Chartmaker.main.Undo(), KeyOf("ED:Undo"), icon: "Undo", _enabled: Chartmaker.main.History.ActionsBehind.Count > 0),
            new ContextMenuListAction("Redo", () => Chartmaker.main.Redo(), KeyOf("ED:Redo"), icon: "Redo", _enabled: Chartmaker.main.History.ActionsAhead.Count > 0),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Cut", Chartmaker.main.Cut, KeyOf("ED:Cut"), icon: "Cut", _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Copy", Chartmaker.main.Copy, KeyOf("ED:Copy"), icon: "Copy", _enabled: Chartmaker.main.CanCopy()),
            new ContextMenuListAction("Paste", Chartmaker.main.Paste, KeyOf("ED:Paste"), icon: "Paste", _enabled: Chartmaker.main.CanPaste())
        ), EditButton);
    }

    public void OpenToolsMenu()
    {
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            new ContextMenuListSublist("Modules", 
                new ContextMenuListAction("Import Module...", () => {})
            )
        ), ToolsButton);
    }

    public void OpenOptionsMenu()
    {
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            new ContextMenuListAction("Preferences...", () => ModalHolder.main.Spawn<PreferencesModal>())
        ), OptionsButton);
    }

    public void OpenHelpMenu()
    {
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            new ContextMenuListAction("About Chartmaker...", () => ModalHolder.main.Spawn<AboutModal>()),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Source Code on GitHub", () => Application.OpenURL("https://github.com/ducdat0507/janoarg")),
            new ContextMenuListAction("FFF40 Studios Discord Server", () => Application.OpenURL("https://discord.gg/vXJTPFQBHm"))
        ), HelpButton);
    }
}
