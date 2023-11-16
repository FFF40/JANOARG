using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayOptionsPanel : MonoBehaviour
{

    [Header("Data")]
    public float MainVolume = 1;
    public float MetronomeVolume = 1;
    public float HitsoundsVolume = 1;
    
    [Header("Objects")]
    public Slider MainVolumeSlider;
    public Slider MetronomeVolumeSlider;
    public Slider HitsoundsVolumeSlider;
    [Space]
    public TMP_InputField MainVolumeField;
    public TMP_InputField MetronomeVolumeField;
    public TMP_InputField HitsoundsVolumeField;

    bool recursionBuster;
    bool isDirty;

    public void Init()
    {
        GetValues();
        SetValues();
    }

    public void OnEnable()
    {
        recursionBuster = true;
        UpdateSliders();
        UpdateFields();
        recursionBuster = false;
    }

    public void OnDisable()
    {
        if (isDirty)
        {
            isDirty = false;
            Storage str = Chartmaker.PreferencesStorage;
            str.Set("PB:Volume:Main", MainVolume);
            str.Set("PB:Volume:Metronome", MetronomeVolume);
            str.Set("PB:Volume:Hitsounds", HitsoundsVolume);
            Chartmaker.main.StartSavePrefsRoutine();
        }
    }

    public void GetValues()
    {
        Storage str = Chartmaker.PreferencesStorage;
        MainVolume = str.Get("PB:Volume:Main", 1f);
        MetronomeVolume = str.Get("PB:Volume:Metronome", 0f);
        HitsoundsVolume = str.Get("PB:Volume:Hitsounds", 1f);
    }

    public void SetValues()
    {
        Chartmaker.main.SongSource.volume = MainVolume;
    }

    public void UpdateSliders()
    {
        MainVolumeSlider.value = MainVolume;
        MetronomeVolumeSlider.value = MetronomeVolume;
        HitsoundsVolumeSlider.value = HitsoundsVolume;
    }

    public void UpdateFields()
    {
        MainVolumeField.text = Mathf.Round(MainVolume * 100).ToString();
        MetronomeVolumeField.text = Mathf.Round(MetronomeVolume * 100).ToString();
        HitsoundsVolumeField.text = Mathf.Round(HitsoundsVolume * 100).ToString();
    }

    public void OnSliderSet()
    {
        if (recursionBuster) return;
        recursionBuster = true;
        MainVolume = MainVolumeSlider.value;
        MetronomeVolume = MetronomeVolumeSlider.value;
        HitsoundsVolume = HitsoundsVolumeSlider.value;
        SetValues();
        UpdateFields();
        recursionBuster = false;
        isDirty = true;
    }

    public void OnFieldSet()
    {
        if (recursionBuster) return;
        recursionBuster = true;
        float.TryParse(MainVolumeField.text, out MainVolume);
        MainVolume = Mathf.Clamp01(MainVolume / 100);
        float.TryParse(MetronomeVolumeField.text, out MetronomeVolume);
        MetronomeVolume = Mathf.Clamp01(MetronomeVolume / 100);
        float.TryParse(HitsoundsVolumeField.text, out HitsoundsVolume);
        HitsoundsVolume = Mathf.Clamp01(HitsoundsVolume / 100);
        SetValues();
        UpdateSliders();
        recursionBuster = false;
        isDirty = true;
    }
}