using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    public static LoadingBar main;

    public Image Background;

    public TMP_Text StatusText;
    public TMP_Text FlavorText;

    public Slider ProgressBar;
    public RectTransform ProgressBarHolder;
    public Image ProgressBarFill;

    RectTransform self;

    public static readonly FlavorTextEntry[] FlavorTextEntries = new[] {

        /* ----- TIPS ----- */

        // Gameplay tips
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>Use wired headphones for the developer-intended ✨flawless✨rhythm✨game✨experience✨."),
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>You can apply a few drops of water between the back of your device and the table to add friction and reduce device drifting. Make sure your device has a case!"),
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>Don't think about it, just feel it."),
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>Increase your Ability Rating to improve your place in social and global rankings."),
        
        /* ----- FACTS ----- */
        
        // General game facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>“JANOARG” is an acronym for “Just Another Normal, Ordinary, Acceptable Rhythm Game”."),
        
        // Gameplay facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>The maximum score that you can obtain on any song is 1000000ppm regardless of difficulty, and 1000000ppm = the rank that you get by reaching that score."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Catch Hit Objects and Flickable Hit Objects always give Flawless or Broken judgment, or in other words, it's either hit or miss."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Multiple overlapping Hold Tails can be hold with just one finger. Just remember to tap the Hit Objects beforehand."),
        
        // Meta facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Most of the facts shown here are nonsense."),
        
        /* ----- OTHER ----- */
        new FlavorTextEntry("<b>🐌"),
    };

    public static readonly FlavorTextEntry[] CompletedStatuses = new[] {

        // Always shown
        new FlavorTextEntry("LOADING COMPLETED"),
        new FlavorTextEntry("LOADING SUCCESS"),
        new FlavorTextEntry("APPROACHING DESTINATION"),
        new FlavorTextEntry("CONNECTION ESTABLISHED"),
        
    };

    public void Awake()
    {
        main = this;
        self = GetComponent<RectTransform>();
        FlavorText.alpha = StatusText.alpha = 0;
        Background.color = Color.clear;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        SetFlavorText(FlavorTextEntry.GetRandom(FlavorTextEntries).Message);

        StopCoroutine(HideAnim());
        StartCoroutine(ShowAnim());
    }

    public IEnumerator ShowAnim()
    {
        yield return null;

        StatusText.text = "NOW LOADING...";

        Color background = Color.black;

        yield return Ease.Animate(2f, (a) => {
            float lerp = Mathf.Pow(Ease.Get(a * 3f, EaseFunction.Circle, EaseMode.Out), 2);
            FlavorText.alpha = lerp;
            float lerp2 = Mathf.Pow(Ease.Get(a * 3f - 0.5f, EaseFunction.Circle, EaseMode.Out), 2);
            StatusText.alpha = lerp2;
            float lerp3 = Mathf.Pow(Ease.Get(a, EaseFunction.Quadratic, EaseMode.Out), 2);
            Background.color = background * new Color(1, 1, 1, .5f * lerp3);
        });
    }

    public void Hide()
    {
        StopCoroutine(ShowAnim());
        StartCoroutine(HideAnim());
    }

    public IEnumerator HideAnim()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    public void SetFlavorText(string text) 
    {
        FlavorText.text = text;
        FlavorText.ForceMeshUpdate();
        FlavorText.rectTransform.localPosition = -FlavorText.textBounds.center;
    }
}

public class FlavorTextEntry
{
    public string Message;
    public Func<bool> Condition = () => true;

    public FlavorTextEntry(string message)
    {
        Message = message;
    }

    public FlavorTextEntry(string message, Func<bool> condition)
    {
        Message = message;
        Condition = condition;
    }

    public static FlavorTextEntry GetRandom(FlavorTextEntry[] entries)
    {
        FlavorTextEntry entry = entries[UnityEngine.Random.Range(0, entries.Length)];

        for (int a = 0; a < 1000; a++) 
        {
            if (entry.Condition()) break;
            entry = entries[UnityEngine.Random.Range(0, entries.Length)];
        }
        return entry;
    }
}
