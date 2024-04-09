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
    public Image StatusBackground;
    public TMP_Text FlavorText;
    public Image FlavorBackground;

    public Slider ProgressBar;
    public RectTransform ProgressBarHolder;
    public Image ProgressBarFill;

    RectTransform self;
    [HideInInspector]
    public bool IsAnimating;

    public static readonly FlavorTextEntry[] FlavorTextEntries = new[] {

        /* ----- TIPS ----- */

        // Gameplay tips
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>Use wired headphones for the developer-intended ✨flawless✨rhythm✨game✨experience✨."),
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>You can apply a few drops of water between the back of your device and the table to add friction and reduce device drifting. Make sure your device has a case first!"),
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>Don't think about it, just feel it. And if you can't feel it, memorize it."),
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>Increase your Ability Rating to improve your place in social rankings and bragging rights."),
        
        /* ----- FACTS ----- */
        
        // General game facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>JANOARG's backronym is “Just Another Normal, Ordinary, Acceptable Rhythm Game”."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Some people consider JANOARG's mechanic so powerful that they call it every rhythm game ever made merged into one. Everythm, if you will."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Despite having “ARG” in the name, we JANOARG developers actually don't host any ARGs... yet, for now."),
        
        // Gameplay facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>The maximum score that you can obtain on any song is 1000000ppm regardless of difficulty."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Catch and Flickable Hit Objects can only give Flawless or Broken judgment. They are either hit or miss."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Multiple overlapping Hold Tails can be hold with just one finger. Just remember to tap the Hit Objects beforehand."),
        
        // Meta facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Most of the facts shown here are nonsense... or are they?"),
        
        /* ----- OTHER ----- */
        
        // 🐌 is love, 🐌 is life.
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
        IsAnimating = true;
        StatusText.text = "NOW LOADING...";

        Color background = Color.black;

        yield return Ease.Animate(3f, (a) => {
            float lerp = Mathf.Pow(Ease.Get(a * 3f, EaseFunction.Circle, EaseMode.Out), 2);
            FlavorText.alpha = lerp;
            float lerp2 = Mathf.Pow(Ease.Get(a * 3f - 0.5f, EaseFunction.Circle, EaseMode.Out), 2);
            StatusText.alpha = lerp2;
            float lerp3 = Ease.Get(a * 1.5f, EaseFunction.Quartic, EaseMode.Out);
            FlavorBackground.color = background * new Color(1, 1, 1, .5f * lerp3);
            float lerp4 = Ease.Get(a, EaseFunction.Quartic, EaseMode.Out);
            Background.color = background * new Color(1, 1, 1, .5f * lerp4);
        });
        IsAnimating = false;
    }

    public void Hide()
    {
        StopCoroutine(ShowAnim());
        StartCoroutine(HideAnim());
    }

    public IEnumerator HideAnim()
    {
        IsAnimating = true;
        StatusText.text = FlavorTextEntry.GetRandom(CompletedStatuses).Message;

        Color background = Color.black;

        yield return Ease.Animate(1, (a) => {
            float lerp = Mathf.Pow(Ease.Get(a * 4f, EaseFunction.Circle, EaseMode.Out), 2);
            StatusBackground.color = StatusText.color * new Color(1, 1, 1, 1 - lerp);
            float lerp2 = Mathf.Pow(Ease.Get(a * 4f - 3, EaseFunction.Circle, EaseMode.Out), 2);
            StatusText.alpha = 1 - lerp2;
            float lerp3 = Mathf.Clamp01(a * 4);
            FlavorBackground.color = FlavorText.color * new Color(1, 1, 1, .5f * (1 - lerp3));
            Background.color = background * new Color(1, 1, 1, .5f * (1 - lerp3));
            FlavorText.alpha = Background.color.a * 2;
        });

        gameObject.SetActive(false);
        IsAnimating = false;
    }

    public void Load(Func<bool> isLoaded, Action onLoad) 
    {
        StartCoroutine(LoadAnim(isLoaded, onLoad));
    }

    public IEnumerator LoadAnim(Func<bool> isLoaded, Action onLoad) 
    {
        yield return ShowAnim();
        yield return new WaitWhile(isLoaded);
        yield return HideAnim();
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
