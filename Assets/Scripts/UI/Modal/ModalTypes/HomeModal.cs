using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeModal : Modal
{
    public static HomeModal main;

    public void Awake()
    {
        if (main) Close();
        else main = this;
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
