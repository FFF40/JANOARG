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

    public void OpenFileMenu()
    {
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            new ContextMenuListAction("New Song...", () => ModalHolder.main.Spawn<NewSongModal>(), "Ctrl+N"),
            new ContextMenuListAction("Open Song...", Chartmaker.main.OpenSongModal, "Ctrl+O"),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Save Song", Chartmaker.main.StartSaveRoutine, "Ctrl+S"),
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
            new ContextMenuListAction("Undo", () => Chartmaker.main.Undo(), "Ctrl+Z", icon: "Undo"),
            new ContextMenuListAction("Redo", () => Chartmaker.main.Redo(), "Ctrl+Y", icon: "Redo"),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Cut", Chartmaker.main.Cut, "Ctrl+X", icon: "Cut"),
            new ContextMenuListAction("Copy", Chartmaker.main.Copy, "Ctrl+C", icon: "Copy"),
            new ContextMenuListAction("Paste", Chartmaker.main.Paste, "Ctrl+V", icon: "Paste")
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
