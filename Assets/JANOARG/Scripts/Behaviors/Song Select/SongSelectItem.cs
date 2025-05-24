using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;

public class SongSelectItem : MonoBehaviour
{
    public TMP_Text SongNameLabel;
    public TMP_Text SongArtistLabel;
    public TMP_Text ChartDifficultyLabel;
    public CanvasGroup MainGroup;

    public RawImage CoverImage;
    public Image CoverBorder;
    public Image CoverStatusBorder;

    [NonSerialized] public int Index;
    [NonSerialized] public float Position;
    [NonSerialized] public float PositionOffset;
    [NonSerialized] public PlaylistSong MapInfo;
    [NonSerialized] public PlayableSong Song;

    public void SetItem(PlaylistSong mapInfo, PlayableSong song, int index, float pos) 
    {
        SongNameLabel.text = song.SongName;
        SongArtistLabel.text = song.SongArtist;
        CoverBorder.color = Color.white;
        CoverStatusBorder.gameObject.SetActive(false);
        Index = index;
        Position = pos;
        Song = song;
        MapInfo = mapInfo;

        SetDifficulty(SongSelectScreen.main.GetNearestDifficulty(song.Charts));
        LoadCoverImage();
    }

    public void SetDifficulty(ExternalChartMeta chart) 
    {
        ChartDifficultyLabel.text = chart.DifficultyLevel;
        ChartDifficultyLabel.color = Common.main.Constants.GetDifficultyColor(chart.DifficultyIndex);
    }

    public Coroutine CoverLoadRoutine = null;
    public void LoadCoverImage() 
    {
        Resources.UnloadAsset(CoverImage.texture);
        CoverImage.texture = null;
        CoverImage.color = Song.Cover.BackgroundColor;
        if (CoverLoadRoutine != null) StopCoroutine(CoverLoadRoutine);
        CoverLoadRoutine = StartCoroutine(LoadCoverImageRoutine());
    }

    private IEnumerator LoadCoverImageRoutine()
    {
        yield return SongSelectCoverManager.main.RegisterUse(CoverImage, MapInfo.ID, Song);
    }

    public void SetVisibilty(float a)
    {
        MainGroup.blocksRaycasts = a == 1;
        MainGroup.alpha = a;
    }
}