using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HomeModal : Modal
{
    public static HomeModal main;
    
    public TMP_Text VersionLabel;

    public List<GameObject> Tabs;
    public List<Button> TabButtons;
    
    public RectTransform RecentSongsHolder;
    public RecentSongItem RecentSongSample;

    readonly List<RecentSongItem> SongItems = new ();

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    public new void Start() 
    {
        base.Start();
        VersionLabel.text = "Version " + Application.version;
        List<RecentSong> list = new(Chartmaker.main.RecentSongsStorage.Get("List", new RecentSong[] {}));
        foreach (RecentSong recent in list) 
        {
            RecentSongItem item = Instantiate(RecentSongSample, RecentSongsHolder);
            item.Background.color = recent.BackgroundColor;
            item.SongArtistLabel.text = recent.SongArtist; 
            item.SongNameLabel.text = recent.SongName;
            item.Tooltip.Text = recent.Path;
            item.Button.onRightClick.AddListener(() => {
                ShowRecentSongRightClickMenu(item, recent);
            });
            item.Button.onClick.AddListener(() => {
                Chartmaker.main.LoaderPanel.SetSong(recent.SongName, recent.SongArtist, recent.BackgroundColor, recent.InterfaceColor);
                StartCoroutine(Chartmaker.main.OpenSongRoutine(recent.Path));
            });
            if (!string.IsNullOrWhiteSpace(recent.IconPath)) 
            {
                StartCoroutine(LoadIconImageRoutine(item, recent.IconPath));
            }
            SongItems.Add(item);
        }

        SetTab(list.Count > 0 ? 1 : 0);
    }

    public void SetTab(int tab) 
    {
        for (int i = 0; i < Tabs.Count; i++) 
        {
            bool isActive = i == tab;
            Tabs[i].SetActive(isActive);
            TabButtons[i].interactable = !isActive;
        }
    }

    private void ShowRecentSongRightClickMenu(RecentSongItem item, RecentSong recent)
    {
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            new ContextMenuListAction("Open Song Path", () => {
                Application.OpenURL("file://" + Path.GetDirectoryName(recent.Path));
            }),
            new ContextMenuListSeparator(),
            new ContextMenuListAction("Remove from List", () => {
                List<RecentSong> list = new(Chartmaker.main.RecentSongsStorage.Get("List", new RecentSong[] {}));
                int index = list.IndexOf(recent);
                if (index >= 0) 
                {
                    Chartmaker.main.RemoveFromRecent(index);
                    Destroy(item.Icon.texture);
                    Destroy(item.gameObject);
                    SongItems.Remove(item);
                }
            })
        ), (RectTransform)item.transform, ContextMenuDirection.Cursor);
    }

    private IEnumerator LoadIconImageRoutine(RecentSongItem item, string path)
    {
        Task<byte[]> activeTask = File.ReadAllBytesAsync(path);
        yield return new WaitUntil(() => activeTask.IsCompleted);
        if (activeTask.IsFaulted) 
        {
            Debug.LogWarning($"Icon for {item.SongArtistLabel.text} - {item.SongNameLabel.text} failed to load: ${activeTask.Exception}");
            yield break;
        }

        Texture2D texture = new (1, 1);
        ImageConversion.LoadImage(texture, activeTask.Result);
        item.Icon.texture = texture;

    }

    public void OnDestroy() 
    {
        foreach (RecentSongItem item in SongItems) Destroy(item.Icon.texture);
    }

    public void OpenSong()
    {
        Chartmaker.main.OpenSongModal();
    }

    public void NewSong()
    {
        ModalHolder.main.Spawn<NewSongModal>();
    }

    public void ExitChartmaker()
    {
        Application.Quit();
    }

    public void OpenInteractiveTutorial()
    {
        ModalHolder.main.Spawn<TutorialModal>();
    }

    public void OpenHelp()
    {
        ModalHolder.main.Spawn<HelpModal>();
    }

    public void OpenGitHubIssueTracker()
    {
        Application.OpenURL("https://github.com/FFF40/JANOARG/issues");
    }

    public void OpenDiscordServer()
    {
        Application.OpenURL("https://discord.gg/vXJTPFQBHm");
    }


}
