using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecentSongItem : MonoBehaviour
{
    public Button Button;
    public TooltipTarget Tooltip;
    public TMP_Text SongArtistLabel;
    public TMP_Text SongNameLabel;
    public Graphic Background;
}

[Serializable]
public struct RecentSong
{
    public string Path;
    public string SongName;
    public string SongArtist;
    public Color BackgroundColor;
    public Color InterfaceColor;
}
