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

    public TMP_Text NameLabel;

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

    public void UpdateCurrencies()
    {
        CoinLabel.text = Common.main.Storage.Get("CURR:Coins", 0L).ToString(CultureInfo.InvariantCulture);
        OrbLabel.text = Common.main.Storage.Get("CURR:Orbs", 0L).ToString(CultureInfo.InvariantCulture);
        EssenceLabel.text = Common.main.Storage.Get("CURR:Essence", 0L).ToString(CultureInfo.InvariantCulture);
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateName();
        UpdateCurrencies();
        Common.main.Storage.OnSave.AddListener(OnSave);
    }

    void OnDestroy()
    {
        Common.main?.Storage?.OnSave.RemoveListener(OnSave);
    }

    void OnSave()
    {
        UpdateName();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
