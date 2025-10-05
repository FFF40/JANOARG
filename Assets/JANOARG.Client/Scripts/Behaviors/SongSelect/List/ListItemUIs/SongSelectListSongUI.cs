using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Client.Behaviors.SongSelect.List.ListItems;
using JANOARG.Client.Behaviors.Common;

namespace JANOARG.Client.Behaviors.SongSelect.List.ListItemUIs
{
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
            if (TargetSong == SongSelectScreen.sMain.PlayableSongByID[target.SongID])
            {
                return;
            }

            TargetSong = SongSelectScreen.sMain.PlayableSongByID[target.SongID];
            SongNameLabel.text = TargetSong.SongName;
            SongArtistLabel.text = TargetSong.SongArtist;
            CoverBorder.color = Color.white;
            CoverStatusBorder.gameObject.SetActive(false);

            SetDifficulty(SongSelectScreen.sMain.GetNearestDifficulty(TargetSong.Charts));
            LoadCoverImage();
        }

        public void SetDifficulty(ExternalChartMeta chart) 
        {
            ChartDifficultyLabel.text = chart.DifficultyLevel;
            ChartDifficultyLabel.color = CommonSys.sMain.Constants.GetDifficultyColor(chart.DifficultyIndex);
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
            yield return SongSelectCoverManager.sMain.RegisterUse(CoverImage, Target.SongID, TargetSong);
        }

        public void SetVisibility(float a)
        {
            MainGroup.blocksRaycasts = a == 1;
            MainGroup.alpha = a;
        }
    }
}