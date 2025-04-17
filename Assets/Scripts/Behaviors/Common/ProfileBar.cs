using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
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

    public void UpdateLevel()
    {
        // Adjust Fill for level
        // briefly show level progression
        // back to show the level
    }

    public void UpdateAbilityRating()
    {
        // Get AR from Save
        float currentAbilityRating = Common.main.Storage.Get("INFO:AbilityRating", 0.00f);
        //Debug.Log(currentAbilityRating);
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

            // If has difference animate to show diff then the new rating
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
        UpdateCurrencies();
        UpdateAbilityRating();
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
        while (true)
        {
            float diff = difference;
            float rating = Common.main.Storage.Get("INFO:AbilityRating", 0.00f);

            yield return Ease.Animate(1f, (x) =>
            {
                AbilityRatingLabel.color = new Color(1, 1, 1, 1 - x);
            });

            AbilityRatingLabel.text = "+" + difference.ToString("f2");

            yield return Ease.Animate(1f, (x) =>
            {
                AbilityRatingLabel.color = new Color(1, 1, 1, 1 - x);
            });

            AbilityRatingLabel.text = rating.ToString("f2");

        }
    }
    RectTransform rt (Component obj) => obj.transform as RectTransform;

}
