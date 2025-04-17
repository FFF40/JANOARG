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
        ScoreStore scores = new ScoreStore();
        float rating = 0.00f;

        scores.Load();

        foreach (var entry in scores.Entries)
        {
            var key = entry.Key;
            var value = entry.Value;

            var record = scores.Get(key + "/" + value);

            if (record == null)
            {
                continue;
            }
            if (record.Rating == null)
            {

            }

            rating = record.Rating + rating;
        }

        // Get all records in save
        // Get best 30
        // Compute

        // If has difference animate to show diff then the new rating
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

    void AnimateRating()
    {
        // diff = 
        // rating = 
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;

}
