using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class ProfileBar : MonoBehaviour
{
    [Header("Data")]
    public long CoinCount;
    public long OrbCount;
    public long EssenceCount;

    public TMP_Text CoinLabel;
    public TMP_Text OrbLabel;
    public TMP_Text EssenceLabel;

    // Start is called before the first frame update
    void Start()
    {
        CoinLabel.text = CoinCount.ToString(CultureInfo.InvariantCulture);
        OrbLabel.text = OrbCount.ToString(CultureInfo.InvariantCulture);
        EssenceLabel.text = EssenceCount.ToString(CultureInfo.InvariantCulture);
    }

    // Update is called once per frame
    void Update()
    {
    }

}
