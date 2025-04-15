using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    public static LoadingBar main;

    public RectTransform StatusHolder;
    public TMP_Text StatusText;
    public RectTransform StatusCompletedHolder;
    public TMP_Text StatusCompletedText;
    [Space]
    public TMP_Text FlavorText;
    public Image FlavorBackground;
    public Image FlavorBackground2;
    [Space]
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
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>You can apply a few drops of water between the back of your device and the table to reduce device drifting. Make sure your device has a case first!"),
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>Don't think about it, just feel it. And if you can't feel it, memorize it."),
        new FlavorTextEntry("<i>Gameplay tip:</i>\n<b>Increase your Ability Rating to improve your place in leaderboard rankings and bragging rights."),
        
        /* ----- FACTS ----- */
        
        // General game facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>JANOARG's backronym is “Just Another Normal, Ordinary, Acceptable Rhythm Game”."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Some people consider JANOARG's mechanic so powerful that they call it every rhythm game ever made merged into one. Everythm, if you will."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Despite having “ARG” in the name, we JANOARG developers actually don't host any ARGs... yet, for now."),
        
        // Gameplay facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>The maximum score that you can obtain on any song is 1000000ppm regardless of difficulty."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Catch and Flickable Hit Objects can only give Flawless or Broken judgment. They are either hit or miss."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Multiple overlapping Hold Tails can be hold with just one finger. Just remember to tap the Hit Objects beforehand."),
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Directional Flick Hit Objects have their hit detection zone extend infinitely along the back and forward directions of the flick!"),
        
        // Meta facts
        new FlavorTextEntry("<i>Did you know:</i>\n<b>Most of the facts shown here are nonsense... or are they?"),
        
        /* ----- OTHER ----- */
        
        // 🐌 is love, 🐌 is life.
        new FlavorTextEntry("<b>🐌"),
    };

    public static readonly FlavorTextEntry[] CompletedStatuses = new[] {

        // Always shown
        new FlavorTextEntry("LOADING COMPLETE"),
        new FlavorTextEntry("LOADING SUCCESS"),
        new FlavorTextEntry("APPROACHING DESTINATION"),
        new FlavorTextEntry("CONNECTION ESTABLISHED"),
        
    };

    public void Awake()
    {
        main = this;
        self = GetComponent<RectTransform>();
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
        StatusCompletedHolder.sizeDelta = new (0, 0);

        yield return Ease.Animate(3f, (a) => {
            float lerp = Ease.Get(a * 3f, EaseFunction.Exponential, EaseMode.Out);
            FlavorBackground.rectTransform.sizeDelta = new (FlavorBackground.rectTransform.sizeDelta.x, lerp * 100);
            float lerp2 = Ease.Get(a * 3f - 0.15f, EaseFunction.Exponential, EaseMode.Out);
            FlavorBackground2.rectTransform.sizeDelta = new (FlavorBackground2.rectTransform.sizeDelta.x, lerp2 * 100);
            float lerp3 = Ease.Get(a * 3f, EaseFunction.Exponential, EaseMode.Out);
            StatusHolder.anchoredPosition = new (1000 + (StatusHolder.rect.width - 1000 + self.sizeDelta.x / -2) * (1 - lerp3), 0);
            float lerp4 = Ease.Get(a, EaseFunction.Exponential, EaseMode.Out);
            FlavorText.rectTransform.anchoredPosition = new (1200 - 100 * lerp4, 0);
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
        StatusCompletedText.text = FlavorTextEntry.GetRandom(CompletedStatuses).Message;
        StatusCompletedText.ForceMeshUpdate();
        float width = StatusCompletedText.preferredWidth + 70;
        float padding = -self.sizeDelta.x / 2;
        StatusCompletedText.rectTransform.anchoredPosition = new (-padding / 2, 0);

        yield return Ease.Animate(1, (a) => {
            float lerp = Ease.Get(a * 1.2f - 0.1f, EaseFunction.Exponential, EaseMode.In);
            float lerp2 = Ease.Get(a * 1.2f, EaseFunction.Exponential, EaseMode.In);
            float lerp3 = Ease.Get(a * 2f, EaseFunction.Exponential, EaseMode.Out);
            float lerp4 = Ease.Get(a, EaseFunction.Exponential, EaseMode.In);
            float lerp5 = Ease.Get(a * 1.5f - 0.5f, EaseFunction.Exponential, EaseMode.In);

            FlavorBackground.rectTransform.sizeDelta = new (FlavorBackground.rectTransform.sizeDelta.x, (1 - lerp) * 100 * (1 - .3f * lerp3));
            FlavorBackground2.rectTransform.sizeDelta = new (FlavorBackground2.rectTransform.sizeDelta.x, (1 - lerp2) * 100 * (1 - .35f * lerp3));
            StatusCompletedHolder.sizeDelta = new ((width + padding) * lerp3, 0);
            FlavorText.rectTransform.anchoredPosition = new (1100 - 300 * lerp4, 0);
            StatusHolder.anchoredPosition = new (1000 - (width - StatusHolder.rect.width + 1000) * lerp3, 0);
            StatusHolder.anchoredPosition += new Vector2((width + padding) * lerp5, 0);
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
