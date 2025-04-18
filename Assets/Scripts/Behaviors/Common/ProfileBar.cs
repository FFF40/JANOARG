using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

public class ProfileBar : MonoBehaviour
{
    public static ProfileBar main;

    [Header("Data")]
    public long CoinCount;
    public long OrbCount;
    public long EssenceCount;

    public CanvasGroup LeftPane;
    public TMP_Text NameLabel;
    public TMP_Text LevelLabel;
    public TMP_Text LevelValueLabel;
    public Slider slider;
    public GameObject sliderFill;
    public TMP_Text AbilityRatingLabel;

    public CanvasGroup RightPane;
    public TMP_Text CoinLabel;
    public TMP_Text OrbLabel;
    public TMP_Text EssenceLabel;

    public RectTransform self { get; private set; }


    public void Awake()
    {
        main = this;
        self = GetComponent<RectTransform>();
    }

    public void UpdateName()
    {
        NameLabel.text = Common.main.Storage.Get("INFO:Name", "JANOARG");
    }

    public void UpdateLevel(int experiencePoints = 0)
    {
        LevelValueLabel.text = Common.main.Storage.Get("INFO:Level", 1).ToString();

        // Adjust slider 
        int levelProgressGained = Common.main.Storage.Get("INFO:LevelProgressNumerator", 1);
        int levelProgressLimit = Common.main.Storage.Get("INFO:LevelProgressDenominator", 100);

        float levelProgress = (float)levelProgressGained / levelProgressLimit;
        

        slider.value = levelProgress ;
        Debug.Log(levelProgressGained + "/" + levelProgressLimit + " =" + levelProgress);
        Debug.Log(slider.value);
        if (experiencePoints > 0)
        {
             StartCoroutine(AnimateLevel(experiencePoints));
        }

    }

    public void UpdateAbilityRating()
    {
        // Get AR from Save
        float currentAbilityRating = Common.main.Storage.Get("INFO:AbilityRating", 0.00f);
        AbilityRatingLabel.text = currentAbilityRating.ToString("f2");
        float newAbilityRating = 0.00f;

        // Get all records in save
        ScoreStore scores = new ScoreStore();
        List<float> ratingEntries = new List<float>();
        scores.Load();

        foreach (var entry in scores.Entries)
        {
            string key = entry.Key;
            int slashIndex = key.LastIndexOf('/');
            string SongID = key.Substring(0, slashIndex); 
            string ChartID = key.Substring(slashIndex + 1);

            var record = scores.Get(SongID, ChartID);

            if (record == null)
            {
                Debug.LogWarning("Record of " + key + " is missing!");
                continue;
            }

            if (record.Rating > 0.00f)
            {
                ratingEntries.Add(record.Rating);
            }
        }

        // Get best 30
        if (ratingEntries.Count > 30)
        {
            ratingEntries.Sort();
            ratingEntries.Reverse();
            ratingEntries.RemoveRange(30, ratingEntries.Count - 30);
        }

        foreach (float ratingEntry in ratingEntries)
        {
            newAbilityRating += ratingEntry;
        }

        float ratingDiff = newAbilityRating - currentAbilityRating;
        
        if (ratingDiff > 0)
        {
            Common.main.Storage.Set("INFO:AbilityRating", newAbilityRating);
            Common.main.Storage.Save();
            AbilityRatingLabel.text = Common.main.Storage.Get("INFO:AbilityRating", 0.00f).ToString("f2");

            StartCoroutine(AnimateRating(ratingDiff));
        }
    }

    public void UpdateCurrencies()
    {
        CoinLabel.text = Common.main.Storage.Get("CURR:Coins", 0L).ToString(CultureInfo.InvariantCulture);
        OrbLabel.text = Common.main.Storage.Get("CURR:Orbs", 0L).ToString(CultureInfo.InvariantCulture);
        EssenceLabel.text = "+" + (Common.main.Storage.Get("CURR:Essence", 0L) / 10f).ToString("F1", CultureInfo.InvariantCulture) + "%";
    }

    void Start()
    {
        UpdateName();
        UpdateLevel();
        UpdateAbilityRating();
        UpdateCurrencies();
        Common.main.Storage.OnSave.AddListener(OnSave);
        SetVisibilty(0);
    }

    void OnDestroy()
    {
        Common.main?.Storage?.OnSave.RemoveListener(OnSave);
    }

    void OnSave()
    {
        UpdateName();
    }

    void Update()
    {
        
    }

    public void SetVisibilty(float a)
    {
        LeftPane.alpha = RightPane.alpha = a * a;
        LeftPane.blocksRaycasts = RightPane.blocksRaycasts = a == 1;
        rt(LeftPane).anchoredPosition = new (-10 * (1 - a), rt(LeftPane).anchoredPosition.y);
        rt(RightPane).anchoredPosition = new (10 * (1 - a), rt(RightPane).anchoredPosition.y);
    }

    IEnumerator AnimateRating(float difference)
    {
        float diff = difference;
        float rating = Common.main.Storage.Get("INFO:AbilityRating", 0.00f);

        AbilityRatingLabel.text = (diff > 0 ? "+" : "-") + diff.ToString("f2");

        yield return Ease.Animate(2.5f, (x) =>
        {
            float lerp = Ease.Get(x * 1, EaseFunction.Cubic, EaseMode.In);
            AbilityRatingLabel.color = new Color(1, 1, 1, 1 - lerp);
        });


        AbilityRatingLabel.text = rating.ToString("f2");
        yield return Ease.Animate(1f, (x) =>
        {
            AbilityRatingLabel.color = new Color(1, 1, 1, 0 + x);
        });

    }

    // Change it if you want
    IEnumerator AnimateLevel(int difference)
    {
        int buffer = difference;


        while (buffer > 0)
        {
            int level = Common.main.Storage.Get("INFO:Level", 1);
            int levelProgressGained = Common.main.Storage.Get("INFO:LevelProgressNumerator", 1);
            int levelProgressLimit = Common.main.Storage.Get("INFO:LevelProgressDenominator", 100);
            float levelProgress = (float)levelProgressGained / levelProgressLimit;

            // get remaining exp needed
            int remainingEXPneed = levelProgressLimit - levelProgressGained;

            // Switch Lv. 1 to + (exp) for 1 second
            LevelLabel.text = "+";
            LevelValueLabel.text = buffer.ToString();

            // Fade Out
            yield return Ease.Animate(2.5f, (x) =>
            {
                float lerp = Ease.Get(x * 1, EaseFunction.Cubic, EaseMode.In);
                LevelLabel.color = new Color(0, 0, 0, 1 - lerp);
                LevelValueLabel.color = new Color(0, 0, 0, 1 - lerp);
            });

            // Fade In
            LevelLabel.text = "Lv.";
            LevelValueLabel.text = level.ToString();
            yield return Ease.Animate(1f, (x) =>
            {
                LevelLabel.color = new Color(0, 0, 0, 0 + x);
                LevelValueLabel.color = new Color(0, 0, 0, 0 + x);
            });

            // if level up 
            if (buffer > remainingEXPneed)
            {
                // animate slider
                yield return Ease.Animate(2.5f, (x) =>
                {
                    float lerp = Ease.Get(x * 1, EaseFunction.Cubic, EaseMode.Out);
                    slider.value = Mathf.Lerp(levelProgress,1f,lerp);
                });

                // show level up text
                LevelValueLabel.color = new Color(1, 1, 1, 0);
                LevelLabel.text = "LEVEL UP";
                LevelLabel.alignment = TextAlignmentOptions.Center;
                level += 1;

                // rainbow effect
                var graphic = sliderFill.GetComponent<GraphicParallelogram>();
                Color fillObjectColor = graphic.color;

                yield return Ease.Animate(2f, (x) =>
                {
                    fillObjectColor = Color.HSVToRGB(x, 0.5f, 1f);
                    graphic.color = fillObjectColor;

                });

                yield return new WaitForSeconds(1f);

                // Back to White
                yield return Ease.Animate(1f, (x) =>
                {
                    float lerp = Ease.Get(x * 1, EaseFunction.Cubic, EaseMode.In);
                    slider.value = 1 - lerp;
                    graphic.color = Color.Lerp(graphic.color,Color.white, lerp);
                });

                //Back to Default Values
                LevelValueLabel.color = new Color(1, 1, 1, 1);
                LevelLabel.text = "Lv.";
                LevelLabel.alignment = TextAlignmentOptions.Left;
                LevelValueLabel.text = level.ToString();
                yield return Ease.Animate(1f, (x) =>
                {
                    LevelLabel.color = new Color(0, 0, 0, 0 + x);
                    LevelValueLabel.color = new Color(0, 0, 0, 0 + x);
                });

                buffer = buffer - remainingEXPneed;
                levelProgressGained = 0;
                levelProgressLimit = Helper.GetEXPLimit(level);
            }
            else
            {
                float initLevelProgress = (float)levelProgressGained / levelProgressLimit;
                levelProgressGained += buffer;
                levelProgress = (float)levelProgressGained / levelProgressLimit;
                Debug.Log(levelProgressGained + "/" + levelProgressLimit + " =" + levelProgress + "Animating");

                yield return Ease.Animate(2.5f, (x) =>
                {
                    float lerp = Ease.Get(x * 1, EaseFunction.Cubic, EaseMode.Out);
                    slider.value = Mathf.Lerp(initLevelProgress, levelProgress,lerp);
                });

                buffer = 0;
            }


            // save
            Common.main.Storage.Set("INFO:Level", level);
            Common.main.Storage.Set("INFO:LevelProgressNumerator", levelProgressGained);
            Common.main.Storage.Set("INFO:LevelProgressDenominator", levelProgressLimit);
            Common.main.Storage.Save();
        }

        
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;

}
