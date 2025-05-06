using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;
using System;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;



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
    [Space]
    public int bonusCount;
    public long bonusReset;

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
    public GameObject LevelUpHolder;
    public Graphic LevelUpLevelGraphic;
    public TMP_Text LevelUpLevelText;
    public Graphic LevelUpLabelGraphic;
    public TMP_Text LevelUpLabelText;
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
    public TMP_Text BonusLabel;
    public Graphic[] BonusBlocks;

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
    public float LastBonusUpdate { get; private set; } = 0;


    Coroutine songGainAnim;
    Coroutine levelUpAnim;
    bool SongGainSkipLock;
    bool SongGainSkipQueued;

    public const int BonusCap = 5;

    public void Awake()
    {
        main = this;
        self = GetComponent<RectTransform>();
    }

    public void CompleteSong(long baseExp, long baseCoins) 
    {
        if (songGainAnim != null) StopCoroutine(songGainAnim);
        songGainAnim = StartCoroutine(SongGainRoutine(baseExp, baseCoins));
    }

    public void AddEXP(long exp)
    {
        if (songGainAnim != null) StopCoroutine(songGainAnim);
        songGainAnim = StartCoroutine(SongGainRoutine(exp, 0));
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

        UpdateDailyCoinBonus();
        UpdateBonusLabels();
    }

    public void UpdateBonusLabels() 
    {
        if (bonusCount <= 0) BonusLabel.text = "Full!";
        else BonusLabel.text = "<alpha=#aa><i>"
            + (DateTimeOffset.FromUnixTimeSeconds(bonusReset) - DateTimeOffset.UtcNow).ToString("%h\\:mm");

        for (int i = 0; i < BonusBlocks.Length; i++) SetBonusBlock(BonusBlocks[i], i >= bonusCount ? 1 : 0);
        LastBonusUpdate = 0;
    }

    public void UpdateRatingInfo()
    {
        // Get all records in save
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
        foreach (float rating in ratingEntries) SongEssence += Mathf.Floor(rating);
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

    void Update()
    {
        if (bonusCount > 0)
        {
            LastBonusUpdate += Time.deltaTime;
            if (LastBonusUpdate >= 20 && RightPane.alpha > 0)
            {
                UpdateDailyCoinBonus();
                UpdateBonusLabels();
            }
        }
        if (songGainAnim != null)
        {
            if (Touchscreen.current?.primaryTouch?.phase.value == UnityEngine.InputSystem.TouchPhase.Began) SongGainSkipQueued = true;
            if (SongGainSkipQueued && SongGainSkipLock) SkipSongGain();
        }
    }

    IEnumerator SongGainRoutine(long baseOrbs, long baseCoins)
    {
        SongGainSkipQueued = SongGainSkipLock = false;
        
        // Calculate AR and essence
        float arOld = AbilityRating;
        float essenceOld = TotalEssence;
        UpdateRatingInfo();
        float arChange = AbilityRating - arOld;
        float essenceChange = TotalEssence - essenceOld;

        // TODO daily bonus
        int bonusMult = GetDailyCoinBonus();
        long finalCoins = baseCoins * bonusMult;
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

        // For testing
        // arChange += 1; AbilityRating += 1; 
        // essenceChange += 10; SongEssence += 10; 

        // Animation setup
        yield return new WaitForSeconds(.4f);

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
        List<CollectingParticle> coinParticles = new();
        while (baseCoins > 0)
        {
            long amount = baseCoins / pCount;
            baseCoins -= amount;
            pCount--;
            coinParticles.Add(SpawnParticle(CoinParticleSample, rt(ChangeCoinIcon.transform), ParticleCoinTarget, () => {
                coinsOld += amount;
                CoinLabel.text = Helper.FormatCurrency(coinsOld);
                SetRewardLerp(1);
                if (coinAnim != null) StopCoroutine(coinAnim);
                coinAnim = StartCoroutine(Ease.Animate(.2f, (x) => {
                    CoinLabel.margin *= new Vector4Frag(y: 3 - 3 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));
                    ParticleCoinFlash.color *= new ColorFrag(a: 1 - x);
                }));
            }));
        } 
        // Orb particles
        Coroutine orbAnim = null;
        pCount = (int)Mathf.Clamp(Mathf.Sqrt(baseOrbs), 1, 30);
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
                    if (levelUpAnim != null) StopCoroutine(levelUpAnim);
                    levelUpAnim = StartCoroutine(LevelUpAnim(levelOld));
                }
                LevelProgressText.text = Helper.FormatCurrency(progOld) + " / " + Helper.FormatCurrency(levelGoal);
                LevelProgressBar.maxValue = levelGoal;
                LevelProgressBar.value = progOld;
                SetRewardLerp(1);

                if (orbAnim != null) StopCoroutine(orbAnim);
                orbAnim = StartCoroutine(Ease.Animate(.2f, (x) => {
                    OrbLabel.margin *= new Vector4Frag(y: 3 - 3 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));
                    ParticleOrbFlash.color *= new ColorFrag(a: 1 - x);
                }));
            });
        } 
        // Essence particles
        Coroutine essenceAnim = null;
        pCount = essenceOld == TotalEssence ? 0 : (int)Mathf.Clamp(essenceChange * 10, 1, 50);
        for (int i = 0; i < pCount; i++)
        {
            SpawnParticle(EssenceParticleSample, rt(ChangeEssenceIcon.transform), ParticleEssenceTarget, () => {
                essenceOld += essenceChange / pCount;
                arOld += arChange / pCount;

                AbilityRatingText.text = arOld.ToString("F2", CultureInfo.InvariantCulture);
                EssenceLabel.text = "+" + essenceOld.ToString("F1", CultureInfo.InvariantCulture) + "%";

                if (essenceAnim != null) StopCoroutine(essenceAnim);
                essenceAnim = StartCoroutine(Ease.Animate(.2f, (x) => {
                    ParticleEssenceFlash.color *= new ColorFrag(a: 1 - x);
                }));
            });
        } 

        SongGainSkipLock = true;

        // Bonus
        if (bonusMult > 1)
        {
            yield return new WaitForSeconds(0.3f);
            foreach (var sourceParticle in coinParticles)
            {
                sourceParticle.Velocity = 600 * Random.insideUnitCircle.normalized;
                for (int i = 1; i < bonusMult; i++) 
                {
                    var particle = Instantiate(sourceParticle, CollectingParticleHolder);
                    particle.Velocity = 600 * Random.insideUnitCircle.normalized;
                    particle.Lifetime *= Mathf.Pow(Random.Range(1f, 1.2f), 2);
                    particle.SpinVelocity = Random.Range(-100, 100f);
                    particle.OnComplete = sourceParticle.OnComplete;
                    particle.Reset();
                }
            }

            int index = Common.main.Storage.Get("INFO:BonusCount", 0) - 1;
            BonusLabel.text = bonusMult + "× BONUS!";
            ChangeCoinLabel.text = "+" + Helper.FormatCurrency(finalCoins) + " (×" +bonusMult + ")";
            yield return Ease.Animate(1f, (x) => {
                float lerp = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
                SetBonusBlock(BonusBlocks[index], 1 - lerp);
                SetChangeLerp(1);
                rt(LeftLayout).anchoredPosition = (1 - lerp) * (10) * Vector2.right;
                rt(RightLayout).anchoredPosition = (1 - lerp) * (10) * Vector2.left;
                SetRewardLerp(1);

                float lerp2 = Ease.Get(x * 2 - 1, EaseFunction.Exponential, EaseMode.In);
                BonusLabel.alpha = 1 - lerp2;
            });
            UpdateBonusLabels();
            yield return Ease.Animate(0.5f, (x) => {
                float lerp2 = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
                BonusLabel.alpha = lerp2;
            });
        }

        yield return null;
        yield return new WaitUntil(() => CollectingParticleHolder.childCount == 0);
        yield return new WaitUntil(() => levelUpAnim == null);
        yield return new WaitForSeconds(1);

        yield return Ease.Animate(0.5f, (x) => {
            float lerp = Ease.Get(Mathf.Pow(x, 0.5f), EaseFunction.Exponential, EaseMode.Out);
            SetRewardLerp(1 - lerp);

            float lerp2 = Ease.Get(x / .6f - .2f, EaseFunction.Quintic, EaseMode.Out);
            SetChangeLerp(1 - lerp2);
        });

        ChangeHeader.gameObject.SetActive(false);
        songGainAnim = null;
        yield return null;
    }

    public void SkipSongGain() 
    {
        StopCoroutine(songGainAnim);
        StartCoroutine(SkipSongGainAnim());
    }

    IEnumerator SkipSongGainAnim()
    {
        foreach (var particle in CollectingParticleHolder.GetComponentsInChildren<CollectingParticle>())
        {
            particle.Lifetime = 0;
        }

        yield return null;
        UpdateLabels();
        BonusLabel.alpha = 1;

        yield return Ease.Animate(0.8f, (x) => {
            float lerp = Ease.Get(Mathf.Pow(x, 0.5f), EaseFunction.Exponential, EaseMode.Out);
            SetRewardLerp(1 - lerp);

            float lerp2 = Ease.Get(x * 3 - 2, EaseFunction.Quintic, EaseMode.Out);
            SetChangeLerp(1 - lerp2);
        });
    }

    IEnumerator LevelUpAnim(int level)
    {
        LevelUpHolder.SetActive(true);
        LevelUpLevelGraphic.rectTransform.anchorMin = new (0, 0);
        LevelUpLabelGraphic.rectTransform.anchorMin = new (0, 0);
        StartCoroutine(Ease.Animate(1.2f, (x) => {
            LevelUpLevelText.text = $"<alpha=#{(int)Math.Clamp((x * 2 + Random.value) * 256, 0, 255):x2}>" + (level - 1) 
                + $"<alpha=#{(int)Math.Clamp((x * 2 - .5 + Random.value) * 256, 0, 255):x2}>" + " → " 
                + $"<alpha=#{(int)Math.Clamp((x * 2 - 1 + Random.value) * 256, 0, 255):x2}>" + level;
        }));
        yield return Ease.Animate(.7f, (x) => {
            float lerp = Ease.Get(Mathf.Pow(x, .7f), EaseFunction.Exponential, EaseMode.Out);
            LevelUpLevelGraphic.rectTransform.anchorMax = new (lerp, 1);
            float lerp2 = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
            LevelUpLabelText.rectTransform.anchoredPosition = new (0, -50 * (1 - lerp2));
        });
         LevelText.text = level.ToString();
        yield return Ease.Animate(0.7f, (x) => {
            float lerp = Ease.Get(Mathf.Pow(x, 1.5f), EaseFunction.Exponential, EaseMode.In);
            LevelUpLevelGraphic.rectTransform.anchorMin = LevelUpLabelGraphic.rectTransform.anchorMin = new (lerp, 0);
        });
        LevelUpHolder.SetActive(false);
        levelUpAnim = null;
    }

    void UpdateDailyCoinBonus() 
    {
        SongGainSkipQueued = SongGainSkipLock = false;

        bonusCount = Common.main.Storage.Get("INFO:BonusCount", 0);
        bonusReset = Common.main.Storage.Get("INFO:BonusReset", 0L);
        long now = DateTimeOffset.Now.ToUnixTimeSeconds();

        if (now >= bonusReset) 
        {
            bonusCount = 0;
            var resetTime = new DateTimeOffset(DateTime.Today.AddDays(1));
            bonusReset = resetTime.ToUnixTimeSeconds();
        }
    }

    int GetDailyCoinBonus() 
    {
        UpdateDailyCoinBonus();

        int multi = 1;
        if (bonusCount < BonusCap)
        {
            multi = Random.value switch {
                < .5f => 3,
                < .9f => 5,
                _ => 7,
            };
            bonusCount++;
        }

        Common.main.Storage.Set("INFO:BonusCount", bonusCount);
        Common.main.Storage.Set("INFO:BonusReset", bonusReset);
        return multi;
    }

    CollectingParticle SpawnParticle(CollectingParticle sample, RectTransform source, RectTransform target, Action onComplete) 
    {
        var particle = Instantiate(sample, CollectingParticleHolder);
        particle.transform.position = source.position;
        particle.Target = target;
        particle.Velocity = 600 * Random.value * Random.insideUnitCircle;
        particle.Lifetime = Mathf.Pow(Random.Range(0.8f, 1.5f), 2);
        particle.SpinVelocity = Random.Range(-100, 100f);
        particle.OnComplete.AddListener(() => onComplete());
        return particle;
    }

    public void SetVisibilty(float a)
    {
        LeftPane.alpha = RightPane.alpha = a * a;
        LeftPane.blocksRaycasts = RightPane.blocksRaycasts = a == 1;
        rt(LeftPane).anchoredPosition *= new Vector2Frag(x: -10 * (1 - a));
        rt(RightPane).anchoredPosition *= new Vector2Frag(x: 10 * (1 - a));
    }

    public void SetRewardLerp(float lerp) 
    {

        AbilityRatingHolder.sizeDelta *= new Vector2Frag(x: 60 + 20 * lerp);
        LevelHolder.anchoredPosition *= new Vector2Frag(x: AbilityRatingHolder.rect.xMin - 1);
        LevelHolder.sizeDelta *= new Vector2Frag(x: 60 + 60 * lerp);
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
        rt(ChangeAREssencePane).sizeDelta *= new Vector2Frag(x: ChangeAREssencePane.preferredWidth * lerp);
        rt(ChangeCurrencyPane).sizeDelta *= new Vector2Frag(x: ChangeCurrencyPane.preferredWidth * lerp);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt(ChangeHeader));
    }

    public void SetBonusBlock(Graphic target, float lerp) 
    {
        target.rectTransform.sizeDelta *= new Vector2Frag(y: 1 + 6 * lerp);
        target.color *= new ColorFrag(a: .6f + .4f * lerp);
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
