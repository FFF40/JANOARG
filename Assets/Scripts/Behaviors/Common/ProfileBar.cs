using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;
using System;
using Random = UnityEngine.Random;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProfileBar : MonoBehaviour
{
    public static ProfileBar main;
    [Header("Data")]
    public float AbilityRating;
    public float SongEssence;
    public float TotalEssence => SongEssence;

    [Header("Left Pane")]
    public CanvasGroup LeftPane;
    public LayoutGroup LeftLayout;
    public TMP_Text NameLabel;
    public TMP_Text TitleLabel;
    public CanvasGroup MenuButtonGroup;
    public CanvasGroup AvatarGroup;
    [Space]
    public RectTransform LevelHolder;
    public TMP_Text LevelLabel;
    public TMP_Text LevelText;
    public TMP_Text LevelProgressText;
    public Slider LevelProgressBar;
    public Graphic LevelBackgroundGraphic;
    public Graphic LevelFillGraphic;
    [Space]
    public RectTransform AbilityRatingHolder;
    public TMP_Text AbilityRatingLabel;
    public TMP_Text AbilityRatingText;

    [Header("Right Pane")]
    public CanvasGroup RightPane;
    public LayoutGroup RightLayout;
    public TMP_Text CoinLabel;
    public TMP_Text OrbLabel;
    public TMP_Text EssenceLabel;

    [Header("Change Header")]
    public CanvasGroup ChangeHeader;
    public LayoutGroup ChangeLayout;
    public LayoutGroup ChangeCurrencyPane;
    public LayoutGroup ChangeAREssencePane;
    public GameObject ChangeCoinIcon;
    public TMP_Text ChangeCoinLabel;
    public GameObject ChangeOrbIcon;
    public TMP_Text ChangeOrbLabel;
    public GameObject ChangeARIcon;
    public TMP_Text ChangeARLabel;
    public GameObject ChangeEssenceIcon;
    public TMP_Text ChangeEssenceLabel;

    [Header("Collecting Particle")]
    public RectTransform CollectingParticleHolder;
    public CollectingParticle CoinParticleSample;
    public CollectingParticle OrbParticleSample;
    public CollectingParticle EssenceParticleSample;
    public RectTransform ParticleCoinTarget;
    public RectTransform ParticleOrbTarget;
    public RectTransform ParticleEssenceTarget;
    public Graphic ParticleCoinFlash;
    public Graphic ParticleOrbFlash;
    public Graphic ParticleEssenceFlash;

    public RectTransform self { get; private set; }


    public void Awake()
    {
        main = this;
        self = GetComponent<RectTransform>();
    }

    Coroutine levelAnim;

    public void CompleteSong(long baseExp, long baseCoins) 
    {
        if (levelAnim != null) StopCoroutine(levelAnim);
        levelAnim = StartCoroutine(SongGainRoutine(baseExp, baseCoins));
    }

    public void AddEXP(long exp)
    {
        if (levelAnim != null) StopCoroutine(levelAnim);
        levelAnim = StartCoroutine(SongGainRoutine(exp, 0));
    }

    public void UpdateLabels()
    {
        // Name
        NameLabel.text = Common.main.Storage.Get("INFO:Name", "JANOARG");
        
        // Levels
        int level = Common.main.Storage.Get("INFO:Level", 1);
        LevelText.text = level.ToString();
        LevelProgressBar.maxValue = Helper.GetLevelGoal(level);
        LevelProgressBar.value = Common.main.Storage.Get("INFO:LevelProgress", 0L);

        // Currencies
        CoinLabel.text = Helper.FormatCurrency(Common.main.Storage.Get("CURR:Coins", 0L));
        OrbLabel.text = Helper.FormatCurrency(Common.main.Storage.Get("CURR:Orbs", 0L));

        // AR & Essence
        AbilityRatingText.text = AbilityRating.ToString("F2", CultureInfo.InvariantCulture);
        EssenceLabel.text = "+" + TotalEssence.ToString("F1", CultureInfo.InvariantCulture) + "%";
    }

    public void UpdateRatingInfo()
    {

        // Get all records in save
        StorageManager.main.Scores.Load();
        Dictionary<string, ScoreStoreEntry> Entries = StorageManager.main.Scores.Entries;
        List<float> ratingEntries = new List<float>();
        
        foreach (var entry in Entries)
        {
            string key = entry.Key;
            int slashIndex = key.LastIndexOf('/');
            string SongID = key.Substring(0, slashIndex); 
            string ChartID = key.Substring(slashIndex + 1);

            var record = StorageManager.main.Scores.Get(SongID, ChartID);

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

        SongEssence = 0;
        foreach (float rating in ratingEntries) SongEssence += rating;
        SongEssence /= 10;

        // Get best 30
        if (ratingEntries.Count > 30)
        {
            ratingEntries.Sort();
            ratingEntries.RemoveRange(0, ratingEntries.Count - 30);
        }

        AbilityRating = 0;
        foreach (float rating in ratingEntries) AbilityRating += rating;
        AbilityRating /= 30;
    }

    void Start()
    {
        UpdateRatingInfo();
        UpdateLabels();
        SetVisibilty(0);
        SetRewardLerp(0);

        ChangeHeader.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
    }

    void Update()
    {
        
    }

    // Change it if you want
    IEnumerator SongGainRoutine(long baseOrbs, long baseCoins)
    {
        // Calculate AR and essence
        float arOld = AbilityRating;
        float essenceOld = TotalEssence;
        UpdateRatingInfo();
        float arChange = AbilityRating - arOld;
        float essenceChange = TotalEssence - essenceOld;

        // TODO daily bonus
        long finalCoins = baseCoins;
        long finalOrbs = (long)(baseOrbs * (1 + TotalEssence / 100));

        // Increase coins and orbs
        long orbsOld = Common.main.Storage.Get("CURR:Orbs", 0L),
            orbsNew = orbsOld + finalOrbs;
        long coinsOld = Common.main.Storage.Get("CURR:Coins", 0L),
            coinsNew = coinsOld + finalCoins;

        // Calculate final level and progress
        int levelOld = Common.main.Storage.Get("INFO:Level", 1),
            levelNew = levelOld;
        long progOld = Common.main.Storage.Get("INFO:LevelProgress", 0L),
            progNew = progOld + finalOrbs;
        long levelGoal = Helper.GetLevelGoal(levelNew);
        while (progNew >= levelGoal) 
        {
            levelNew++;
            progNew -= levelGoal;
            levelGoal = Helper.GetLevelGoal(levelNew);
        }

        // Save
        Common.main.Storage.Set("CURR:Coins", coinsNew);
        Common.main.Storage.Set("CURR:Orbs", orbsNew);
        Common.main.Storage.Set("INFO:Level", levelNew);
        Common.main.Storage.Set("INFO:LevelProgress", progNew);
        Common.main.Storage.Save();

        // Animation setup
        yield return new WaitForSeconds(.8f);

        ChangeARLabel.gameObject.SetActive(arOld != AbilityRating);
        ChangeARIcon.SetActive(arOld != AbilityRating);
        ChangeEssenceLabel.gameObject.SetActive(essenceOld != TotalEssence);
        ChangeEssenceIcon.SetActive(essenceOld != TotalEssence);
        ChangeAREssencePane.gameObject.SetActive(ChangeARIcon.activeSelf || ChangeEssenceIcon.activeSelf);

        ChangeHeader.gameObject.SetActive(true);
        ChangeCoinLabel.text = "+" + Helper.FormatCurrency(baseCoins);
        ChangeOrbLabel.text = "+" + Helper.FormatCurrency(finalOrbs);
        ChangeARLabel.text = (arChange < 0 ? "−" : "+") + Mathf.Abs(arChange).ToString("0.00");
        ChangeEssenceLabel.text = (essenceChange < 0 ? "−" : "+") + Mathf.Abs(essenceChange).ToString("0.0") + "%";

        levelGoal = Helper.GetLevelGoal(levelOld);
        LevelProgressText.text = Helper.FormatCurrency(progOld) + " / " + Helper.FormatCurrency(levelGoal);

        // Actual animation
        yield return Ease.Animate(0.5f, (x) => {
            float lerp = Ease.Get(Mathf.Pow(x, 0.5f), EaseFunction.Exponential, EaseMode.Out);
            SetRewardLerp(lerp);

            float lerp2 = Ease.Get(x / .6f, EaseFunction.Quintic, EaseMode.Out);
            SetChangeLerp(lerp2);
        });

        int pCount;
        // Coin particles
        Coroutine coinAnim = null;
        pCount = (int)Mathf.Clamp(Mathf.Sqrt(baseCoins), 1, 15);
        while (baseCoins > 0)
        {
            long amount = baseCoins / pCount;
            baseCoins -= amount;
            pCount--;
            SpawnParticle(CoinParticleSample, rt(ChangeCoinIcon.transform), ParticleCoinTarget, () => {
                coinsOld += amount;
                CoinLabel.text = Helper.FormatCurrency(coinsOld);
                SetRewardLerp(1);
                if (coinAnim != null) StopCoroutine(coinAnim);
                coinAnim = StartCoroutine(Ease.Animate(.2f, (x) => {
                    CoinLabel.margin *= new Vector4Frag(null, 3 - 3 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out), null, null);
                    ParticleCoinFlash.color *= new ColorFrag(null, null, null, 1 - x);
                }));
            });
        } 
        // Orb particles
        Coroutine orbAnim = null;
        pCount = (int)Mathf.Clamp(Mathf.Sqrt(baseOrbs), 1, 25);
        while (finalOrbs > 0)
        {
            long amount = finalOrbs / pCount;
            finalOrbs -= amount;
            pCount--;
            SpawnParticle(OrbParticleSample, rt(ChangeOrbIcon.transform), ParticleOrbTarget, () => {
                orbsOld += amount;
                OrbLabel.text = Helper.FormatCurrency(orbsOld);
                progOld += amount;
                if (progOld >= levelGoal) 
                {
                    levelOld++;
                    progOld -= levelGoal;
                    levelGoal = Helper.GetLevelGoal(levelNew);
                    LevelText.text = levelOld.ToString();
                }
                LevelProgressText.text = Helper.FormatCurrency(progOld) + " / " + Helper.FormatCurrency(levelGoal);
                LevelProgressBar.maxValue = levelGoal;
                LevelProgressBar.value = progOld;
                SetRewardLerp(1);

                if (orbAnim != null) StopCoroutine(orbAnim);
                orbAnim = StartCoroutine(Ease.Animate(.2f, (x) => {
                    OrbLabel.margin *= new Vector4Frag(null, 3 - 3 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out), null, null);
                    ParticleOrbFlash.color *= new ColorFrag(null, null, null, 1 - x);
                }));
            });
        } 
        // Essence particles
        Coroutine essenceAnim = null;
        pCount = essenceChange == 0 ? 0 : (int)Mathf.Clamp(essenceChange * 10, 1, 15);
        for (int i = 0; i < pCount; i++)
        {
            SpawnParticle(EssenceParticleSample, rt(ChangeEssenceIcon.transform), ParticleEssenceTarget, () => {
                essenceOld += essenceChange / pCount;
                arOld += arChange / pCount;

                AbilityRatingText.text = arOld.ToString("F2", CultureInfo.InvariantCulture);
                EssenceLabel.text = "+" + essenceOld.ToString("F1", CultureInfo.InvariantCulture) + "%";

                if (essenceAnim != null) StopCoroutine(essenceAnim);
                essenceAnim = StartCoroutine(Ease.Animate(.2f, (x) => {
                    ParticleEssenceFlash.color *= new ColorFrag(null, null, null, 1 - x);
                }));
            });
        } 

        yield return null;
        yield return new WaitUntil(() => CollectingParticleHolder.childCount == 0);
        yield return new WaitForSeconds(1);

        yield return Ease.Animate(0.5f, (x) => {
            float lerp = Ease.Get(Mathf.Pow(x, 0.5f), EaseFunction.Exponential, EaseMode.Out);
            SetRewardLerp(1 - lerp);

            float lerp2 = Ease.Get(x / .6f - .2f, EaseFunction.Quintic, EaseMode.Out);
            SetChangeLerp(1 - lerp2);
        });

        ChangeHeader.gameObject.SetActive(false);
        yield return null;
    }

    void SpawnParticle(CollectingParticle sample, RectTransform source, RectTransform target, Action onComplete) 
    {
        var particle = Instantiate(sample, CollectingParticleHolder);
        particle.transform.position = source.position;
        particle.Target = target;
        particle.Velocity = Random.insideUnitCircle * Random.value * 600;
        particle.Lifetime = Mathf.Pow(Random.Range(0.8f, 1.5f), 2);
        particle.SpinVelocity = Random.Range(-100, 100f);
        particle.OnComplete.AddListener(() => onComplete());
    }

    public void SetVisibilty(float a)
    {
        LeftPane.alpha = RightPane.alpha = a * a;
        LeftPane.blocksRaycasts = RightPane.blocksRaycasts = a == 1;
        rt(LeftPane).anchoredPosition *= new Vector2Frag(-10 * (1 - a), null);
        rt(RightPane).anchoredPosition *= new Vector2Frag(10 * (1 - a), null);
    }

    public void SetRewardLerp(float lerp) 
    {

        AbilityRatingHolder.sizeDelta *= new Vector2Frag(60 + 20 * lerp, null);
        LevelHolder.anchoredPosition *= new Vector2Frag(AbilityRatingHolder.rect.xMin - 1, null);
        LevelHolder.sizeDelta *= new Vector2Frag(60 + 60 * lerp, null);
        LevelLabel.rectTransform.sizeDelta = new (-12 - 80 * lerp, 0);
        AbilityRatingLabel.rectTransform.sizeDelta = new (-12 - 6 * lerp, 0);
        LevelText.rectTransform.sizeDelta = new (-12 - 6 * lerp, -2 * lerp);
        AbilityRatingText.rectTransform.sizeDelta = new (-12 - 16 * lerp, -28 * lerp);

        AbilityRatingText.fontSize = LevelText.fontSize = 8 + 3 * lerp;

        NameLabel.alpha = TitleLabel.alpha = MenuButtonGroup.alpha = AvatarGroup.alpha = 1 - lerp;
        LevelProgressText.alpha = lerp;
        LevelText.color = LevelLabel.color = Color.Lerp(Color.black, Color.white, lerp);
        LevelBackgroundGraphic.color = new (1, 1, 1, .75f - .6f * lerp);
        LevelFillGraphic.color = new (1, 1, 1, 1 - .6f * lerp);

        float width = 300 + RightLayout.preferredWidth - RightLayout.minWidth;
        float safeOffset = -rt(this).sizeDelta.x / 2;
        rt(LeftLayout).anchoredPosition = new (Mathf.Lerp(-1000, -500 - safeOffset - width / 2 - (LeftLayout.preferredWidth - LeftLayout.minWidth), lerp), 0);
        rt(RightLayout).anchoredPosition = new (Mathf.Lerp(1000, 600 + safeOffset + width / 2, lerp), 0);
    }

    public void SetChangeLerp(float lerp)
    {
        ChangeHeader.alpha = lerp;
        ChangeAREssencePane.padding.left = ChangeAREssencePane.padding.right
            = ChangeCurrencyPane.padding.left = ChangeCurrencyPane.padding.right
            = 10;
        rt(ChangeAREssencePane).sizeDelta *= new Vector2Frag(ChangeAREssencePane.preferredWidth * lerp, null);
        rt(ChangeCurrencyPane).sizeDelta *= new Vector2Frag(ChangeCurrencyPane.preferredWidth * lerp, null);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt(ChangeHeader));
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;

}

#if UNITY_EDITOR
[CustomEditor(typeof(ProfileBar))]
class ProfileBarHelperEditor : Editor 
{
    public override void OnInspectorGUI() 
    {
        GUILayout.Label("Testing", EditorStyles.boldLabel);
        if (GUILayout.Button("Do EXP Animation"))
        {
            if (EditorApplication.isPlaying)
                ((ProfileBar)serializedObject.targetObject).CompleteSong(408, 128);
        }

        DrawDefaultInspector();
    }
}
#endif
