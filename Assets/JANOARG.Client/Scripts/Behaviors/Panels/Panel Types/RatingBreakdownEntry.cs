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
        public TMP_Text SongName;
        public TMP_Text SongArtist;
        public TMP_Text DifficultyRating;


        public void Awake()
        {
            
        }

        public void SetEntry(string rating, string difficulty, string songName, string songArtist, string difficultyRating)
        {
            Rating.text = rating;
            Difficulty.text = difficulty;
            SongName.text = songName;
            SongArtist.text = songArtist;
            DifficultyRating.text = difficultyRating;
        }

    }
}
