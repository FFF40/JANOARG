using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JANOARG.Client.Behaviors.Panels.Panel_Types
{
    public class RatingBreakdownEntry : MonoBehaviour
    {
        public TMP_Text Rating;
        public TMP_Text Difficulty;
        public TMP_Text BestScore;
        public TMP_Text SongName;
        public TMP_Text SongArtist;
        public TMP_Text DifficultyRating;

        public void SetEntry(string rating, string difficulty, string bestScore, string songName, string songArtist, string difficultyRating)
        {
            Rating.text = rating + "<size=50%><b>.0";
            Difficulty.text = difficulty;
            BestScore.text = bestScore + "<size=50%><b>ppm";
            SongName.text = songName;
            SongArtist.text = songArtist;
            DifficultyRating.text = difficultyRating;
        }

    }
}
