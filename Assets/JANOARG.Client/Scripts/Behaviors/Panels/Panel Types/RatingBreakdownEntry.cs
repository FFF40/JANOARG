using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JANOARG.Client.Behaviors.Common;
using Unity.VisualScripting;
using System;

namespace JANOARG.Client.Behaviors.Panels.Panel_Types
{
    public class RatingBreakdownEntry : MonoBehaviour
    {
        public TMP_Text Rating;
        public TMP_Text BestScore;
        public TMP_Text SongName;
        public TMP_Text SongArtist;
        public TMP_Text ChartConstant;

        public void SetEntry(string rating, string difficulty,
                string bestScore, string songName, string songArtist, string chartConstant)
        {
            int diff_Index;
            //temporary code for difficulty color 
            switch (difficulty)
            {
                case "simple":
                    diff_Index = 0;
                    break;
                case "normal":
                    diff_Index = 1;
                    break;
                case "complex":
                    diff_Index = 2;
                    break;
                case "overdrive":
                    diff_Index = 3;
                    break;
                default:
                    diff_Index = -1;
                    break;
            }


            Rating.text = rating + "<size=50%><b>.0";
            BestScore.text = bestScore + "<size=50%><b>ppm";
            SongName.text = songName;
            SongArtist.text = songArtist;
            ChartConstant.text = chartConstant;
            ChartConstant.color = CommonSys.sMain.Constants.
                GetDifficultyColor(diff_Index);
        }

    }
}
