using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AboutModal : Modal
{
    public TMP_Text FlavorTextLabel;

    public static readonly string[] FlavorTexts = new[] {
        "<i>Did you know:</i>\n<b>JANOARG stands for \"Just Another Normal, Ordinary, Acceptable Rhythm Game\".",
        "<i>Did you know:</i>\n<b>Our team consists of a nursery of snails which are current developing this game at record snail speed.",
        "<i>Did you know:</i>\n<b>We chose the \"developing at snail speed\" subtitle because we believe that by the time this game officially releases it will become true to its name.",
        "<i>Did you know:</i>\n<b>A nursery of snails is called an \"escargatoire\".",
        "<i>Did you know:</i>\n<b>This randomly displayed text box may not always give 100% factual statements.",
        "<i>Did you know:</i>\n<b>You can click on the visualizer (metronome) on the top right corner to switch between different visualizers.",
        
        "<i>Charting tips:</i>\n<b>Missing something? Want more features? Have message ideas to put here? Suggest them in our Discord server and we may make them real!",

        "<i>Missing snail:</i>\n<b>We're trying to find a lost snail which is currently hiding somewhere inside the Picker. Can you find it?",
    };

    public void Awake ()
    {
        FlavorTextLabel.text = FlavorTexts[Random.Range(0, FlavorTexts.Length)];
    }
}
