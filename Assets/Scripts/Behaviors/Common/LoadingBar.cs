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

        // Rhythm tips
        new FlavorTextEntry("Remember: Everything in JANOARG can move!"),
        new FlavorTextEntry("JANOARG is erratic, but sometimes it is more erratic than others."),

        new FlavorTextEntry("The highest performance score you can get will always be 1000000ppm per difficulty."),
        new FlavorTextEntry("Flicks and Catchs will always give you their maximum point value as long as you hit them!"),
        new FlavorTextEntry("You can hold overlapping Hold Tails with just one finger! Just make sure you hit them all first."),

        new FlavorTextEntry("Beware of an asterisk (*) on a difficulty rating, it means that that difficulty requires 3 or more fingers to Full Streak!"),
        new FlavorTextEntry("Track difficulties should be compared based on their rating numbers, not by their names."),
        
        // Resources tips
        /* TODO: Uncomment this when resources and objects are implemented
        new FlavorTextEntry("Gaining resources too slow? Maybe it's time to try out some of the harder tracks...", 
           () => PlaylistScroll.main?.SelectedDifficulty?.Chart?.ChartConstant < 20),
        new FlavorTextEntry("Essence multiplies your Orb and Experience gain! Make sure to stack a lot of them!"),
        new FlavorTextEntry("Get high scores on more complex sequences of a track to gain extra Essence!"),

        new FlavorTextEntry("Offline gains? Is this an idle game or something?"),
        new FlavorTextEntry("The Idlinator will still work when you close the game for more than 24 hours, but its effectiveness reduces the longer you idle!"),
        
        new FlavorTextEntry("Make sure to give your Objects some of your care - they give a lot of benefits!"),
        new FlavorTextEntry("All Objects work across worlds!"),
        */

        // Meta
        new FlavorTextEntry("JANOARG - Developing at snail speed"),
        new FlavorTextEntry("haha System.Func go brrrrrrrrrrr"),

        // References
        new FlavorTextEntry("Colors of the world are determined by what our world designers feel like that day."),
        new FlavorTextEntry("History is learned through the visual novel mechanic."),
        new FlavorTextEntry("Gyroscope gimmick? Maybe later..."),
        new FlavorTextEntry("Snail's House? Maybe later...", 
            () => ChartPlayer.main?.CurrentChart?.DifficultyName == "🐌" || PlaylistScroll.main?.SelectedDifficulty?.Chart?.DifficultyName == "🐌"),

        // Loading bar
        new FlavorTextEntry("Disclaimer: The loading bar is not guaranteed to always give you actually useful in-game tips"),
        new FlavorTextEntry("Loading bar is the new news ticker"),
        new FlavorTextEntry("Look ma, I'm in a rhythm game's loading screen!"),

        new FlavorTextEntry("This loading text will never appear in the loading bar, isn't that weird?",
            () => false),

    };

    public static readonly FlavorTextEntry[] CompletedStatuses = new[] {

        // Always shown
        new FlavorTextEntry("LOADING COMPLETED"),
        new FlavorTextEntry("LOADING SUCCESS"),
        new FlavorTextEntry("APPROACHING DESTINATION"),
        new FlavorTextEntry("CONNECTION ESTABLISHED"),

        // When starting a song
        new FlavorTextEntry("HERE WE GO", () => ChartPlayer.main?.CurrentTime < 0),
        new FlavorTextEntry("LET'S GO", () => ChartPlayer.main?.CurrentTime < 0),
        new FlavorTextEntry("GET READY", () => ChartPlayer.main?.CurrentTime < 0),
        new FlavorTextEntry("MUSIC START", () => ChartPlayer.main?.CurrentTime < 0),
        
    };

    public void Awake()
    {
        main = this;
        self = GetComponent<RectTransform>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        FlavorText.text = FlavorTextEntry.GetRandom(FlavorTextEntries).Message;
        StopCoroutine(HideAnim());
        StartCoroutine(ShowAnim());
    }

    public IEnumerator ShowAnim()
    {
        Color background = Themer.Colors.ContainsKey("Background") ? Themer.Colors["Background"] : Color.black;
        Color foreground = Themer.Colors.ContainsKey("Foreground") ? Themer.Colors["Foreground"] : Color.white;

        Background.color = background;
        FlavorText.color = StatusText.color = ProgressBarFill.color = foreground;
        StatusText.rectTransform.anchoredPosition = new Vector3(StatusText.rectTransform.anchoredPosition.x, -34);
        ProgressBar.value = 0;

        void LerpSelection(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Quintic, EaseMode.Out);
            self.anchoredPosition = new Vector2(0, 48 * ease);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .3f)
        {
            LerpSelection(a);
            yield return null;
        }
        LerpSelection(1);
    }

    public void Hide()
    {
        StopCoroutine(ShowAnim());
        StartCoroutine(HideAnim());
    }

    public IEnumerator HideAnim()
    {
        StatusText.text = FlavorTextEntry.GetRandom(CompletedStatuses).Message;
        ProgressBar.value = 0;
        
        Color background = Themer.Colors.ContainsKey("Background") ? Themer.Colors["Background"] : Color.black;
        Color foreground = Themer.Colors.ContainsKey("Foreground") ? Themer.Colors["Foreground"] : Color.white;

        void LerpSelection(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Exponential, EaseMode.Out);
            Background.color = Color.Lerp(foreground, background, ease);
            Background.color = Color.Lerp(foreground, background, ease);
            ProgressBarHolder.sizeDelta = new Vector2(ProgressBarHolder.sizeDelta.x, -38 * (1 - ease));
            FlavorText.alpha = 1 - ease;
            StatusText.rectTransform.anchoredPosition = new Vector3(StatusText.rectTransform.anchoredPosition.x, -34 + 8 * ease);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .5f)
        {
            LerpSelection(a);
            yield return null;
        }
        LerpSelection(1);

        void LerpSelection2(float value)
        {
            float ease = 1 - Ease.Get(value, EaseFunction.Exponential, EaseMode.In);
            self.anchoredPosition = new Vector2(0, 48 * ease);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .5f)
        {
            LerpSelection2(a);
            yield return null;
        }
        LerpSelection2(1);

        gameObject.SetActive(false);
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
