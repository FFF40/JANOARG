using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HomeModal : Modal
{
    public static HomeModal main;
    
    public TMP_Text VersionLabel;
    
    public RectTransform RecentSongsHolder;
    public RecentSongItem RecentSongSample;

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
            item.Button.onClick.AddListener(() => {
                Chartmaker.main.LoaderPanel.SetSong(recent.SongName, recent.SongArtist, recent.BackgroundColor, recent.InterfaceColor);
                StartCoroutine(Chartmaker.main.OpenSongRoutine(recent.Path));
            });
        }
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
}
