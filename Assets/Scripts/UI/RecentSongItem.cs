using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecentSongItem : MonoBehaviour
{
    public Button Button;
    public TMP_Text DataLabel;
    public TMP_Text PathLabel;
}

[Serializable]
public struct RecentSong
{
    public string Path;
    public string SongName;
    public string SongArtist;
}
