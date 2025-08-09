using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;

public class SongSelectListSongUI : SongSelectItemUI<SongSelectListSong>
{
    public TMP_Text SongNameLabel;
    public TMP_Text SongArtistLabel;
    public TMP_Text ChartDifficultyLabel;
    public CanvasGroup MainGroup;

    public RawImage CoverImage;
    public Image CoverBorder;
    public Image CoverStatusBorder;

    [NonSerialized] public PlayableSong TargetSong;

    public void SetItem(SongSelectListSong target) 
    {
        Target = target;
        if (target == null)
        {
            TargetSong = null;
            return;
        }
        if (TargetSong == SongSelectScreen.main.PlayableSongByID[target.SongID])
        {
            return;
        }

        TargetSong = SongSelectScreen.main.PlayableSongByID[target.SongID];
        SongNameLabel.text = TargetSong.SongName;
        SongArtistLabel.text = TargetSong.SongArtist;
        CoverBorder.color = Color.white;
        CoverStatusBorder.gameObject.SetActive(false);

        SetDifficulty(SongSelectScreen.main.GetNearestDifficulty(TargetSong.Charts));
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
        UnloadCoverImage();
        CoverImage.color = TargetSong.Cover.BackgroundColor;
        if (CoverLoadRoutine != null) StopCoroutine(CoverLoadRoutine);
        CoverLoadRoutine = StartCoroutine(LoadCoverImageRoutine());
    }
    
    public void UnloadCoverImage()
    {
        Resources.UnloadAsset(CoverImage.texture);
        CoverImage.texture = null;
    }

    private IEnumerator LoadCoverImageRoutine()
    {
        yield return SongSelectCoverManager.main.RegisterUse(CoverImage, Target.SongID, TargetSong);
    }

    public void SetVisibility(float a)
    {
        MainGroup.blocksRaycasts = a == 1;
        MainGroup.alpha = a;
    }
}