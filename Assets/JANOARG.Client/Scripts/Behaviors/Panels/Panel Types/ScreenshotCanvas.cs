using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using JANOARG.Client.Behaviors.Common;

public class ScreenshotCanvas : MonoBehaviour
{
    public TMP_Text NameLabel;
    public TMP_Text TitleLabel;

    public TMP_Text LevelLabel;
    public TMP_Text LevelText;
  
    public Slider LevelProgressBar;
    public Graphic LevelBackgroundGraphic;
    public Graphic LevelFillGraphic;

    public TMP_Text AbilityRatingLabel;
    public TMP_Text AbilityRatingText;

    

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

}
