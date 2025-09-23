using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using JANOARG.Client.Behaviors.Common;

namespace JANOARG.Client.Behaviors.Panels.Profile
{
    public class ScreenshotCanvas : MonoBehaviour
    {
        public static ScreenshotCanvas sMain;
        public TMP_Text NameLabel;
        public TMP_Text TitleLabel;

        public TMP_Text LevelLabel;
        public TMP_Text LevelText;

        public Slider LevelProgressBar;
        public Graphic LevelBackgroundGraphic;
        public Graphic LevelFillGraphic;

        public TMP_Text AbilityRatingLabel;
        public TMP_Text AbilityRatingText;

        public RawImage BestSongCover;
        public bool IsCoverSet = false;

        void Awake()
        {
            sMain = this;
        }

        private void Start()
        {
            NameLabel.text = ProfileBar.sMain.NameLabel.text;
            TitleLabel.text = ProfileBar.sMain.TitleLabel.text;

            LevelLabel.text = ProfileBar.sMain.LevelLabel.text;
            LevelText.text = ProfileBar.sMain.LevelText.text;

            LevelProgressBar.value = ProfileBar.sMain.LevelProgressBar.value;
            LevelBackgroundGraphic.color = ProfileBar.sMain.LevelBackgroundGraphic.color;
            LevelFillGraphic.color = ProfileBar.sMain.LevelFillGraphic.color;

            AbilityRatingLabel.text = ProfileBar.sMain.AbilityRatingLabel.text;
            AbilityRatingText.text = ProfileBar.sMain.AbilityRatingText.text;
        }

        public void SetBestSongCover(Texture2D texture)
        {
            if (IsCoverSet == false)
            {
                BestSongCover.texture = texture;
                BestSongCover.color = Color.gray;

                IsCoverSet = true;
            }
        }

        public void SetBestSongCover(Color color)
        {
            if (IsCoverSet == false)
            {
                BestSongCover.color = new Color(color.r, color.g, color.b, 0.75f);

                IsCoverSet = true;
            }
        }
    }
}
