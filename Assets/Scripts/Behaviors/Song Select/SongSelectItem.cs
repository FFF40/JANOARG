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
    [NonSerialized] public string SongPath;
    [NonSerialized] public PlayableSong Song;

    public void SetItem(string path, PlayableSong song, int index, float pos) 
    {
        SongNameLabel.text = song.SongName;
        SongArtistLabel.text = song.SongArtist;
        CoverBorder.color = Color.white;
        CoverStatusBorder.gameObject.SetActive(false);
        Index = index;
        Position = pos;
        Song = song;
        SongPath = path;

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
        string path = Path.Combine(Path.GetDirectoryName(SongPath), Song.Cover.IconTarget);
        if (Path.HasExtension(path)) path = Path.ChangeExtension(path, "")[0..^1];
        ResourceRequest req = Resources.LoadAsync<Texture2D>(path);
        yield return new WaitUntil(() => req.isDone);
        if (req.asset) 
        {
            Texture2D tex = (Texture2D)req.asset;
            CoverImage.texture = tex;
            CoverImage.color = Color.white;
        }
    }

    public void SetVisibilty(float a)
    {
        MainGroup.blocksRaycasts = a == 1;
        MainGroup.alpha = a;
    }
}