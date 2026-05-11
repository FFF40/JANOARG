using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Data.Storage;
using JANOARG.Client.Behaviors.SongSelect;
using JANOARG.Shared.Data.ChartInfo;
using System.IO;

using JANOARG.Client.Utils;
using JANOARG.Client.Behaviors.SongSelect.List;

namespace JANOARG.Client.Behaviors.Panels.Profile
{
    public class RatingBreakdownEntry : MonoBehaviour
    {
        public TMP_Text Rating;
        public TMP_Text BestScore;
        public TMP_Text SongName;
        public TMP_Text SongArtist;
        public TMP_Text ChartConstant;

        public RawImage Icon;
        public RawImage BackgroundCover;

        public GameObject FullStreakIndicator;
        public GameObject AllFlawlessIndicator;

        public TMP_Text FlawlessCount;
        public TMP_Text MisalignedCount;
        public TMP_Text BrokenCount;

        // private List<SongSelectItemUI> _SongSelectItems;
        // private Color _Color;

        public void SetData(ScoreStoreEntry entry)
        {
            string ratingText = entry.Rating.ToString("F2");
            string[] parts = ratingText.Split('.');
            Rating.text = $"<b>{parts[0]}.</b><size=50%>{parts[1]}</size>";;
            BestScore.text = Helper.PadScore(entry.Score.ToString()) + "<size=50%><b>ppm";

            

            if (entry.PerfectCount == entry.MaxCombo)
            {
                AllFlawlessIndicator.SetActive(true);
            } else if (entry.BadCount == 0)
            {
                FullStreakIndicator.SetActive(true);
            }
            
            if (BrokenCount != null)
            {
                BrokenCount.text = entry.BadCount.ToString();
            }
            if (MisalignedCount != null)
            {
                MisalignedCount.text = entry.GoodCount.ToString();
            }
            if (FlawlessCount != null)
            {
                FlawlessCount.text = entry.PerfectCount.ToString();
            }


        }

        
    }
}