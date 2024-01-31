using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SongSelectItem : MonoBehaviour
{
    public TMP_Text SongNameLabel;
    public TMP_Text SongArtistLabel;
    public TMP_Text ChartDifficultyLabel;
    public CanvasGroup MainGroup;

    public Image CoverImage;
    public Image CoverBorder;
    public Image CoverStatusBorder;

    public int Index;
    public float Position;
    public float PositionOffset;
    [NonSerialized]
    public PlayableSong Song;

    public void SetItem(PlayableSong song, int index, float pos) 
    {
        SongNameLabel.text = song.SongName;
        SongArtistLabel.text = song.SongArtist;
        ChartDifficultyLabel.text = song.Charts[0].DifficultyLevel;
        CoverBorder.color = Color.white;
        CoverStatusBorder.gameObject.SetActive(false);
        Index = index;
        Position = pos;
        Song = song;
    }

    public void SetVisibilty(float a)
    {
        MainGroup.blocksRaycasts = a == 1;
        MainGroup.alpha = a;
    }
}