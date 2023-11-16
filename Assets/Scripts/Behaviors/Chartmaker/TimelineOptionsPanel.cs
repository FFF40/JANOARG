using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineOptionsPanel : MonoBehaviour
{
    [Header("Fields")]
    public float Speed = 1;
    public int SeparationFactor = 2;
    public bool FollowSeekLine = true;
    public int Waveform = 1;
    public int WaveformMode = 1;

    [Header("Objects")]
    public TMP_InputField SpeedField;
    public TMP_InputField SeparatorField;
    public Toggle FollowSeekLineToggle;
    public ToggleGroup WaveformToggleGroup;
    public List<Toggle> WaveformToggles;
    public ToggleGroup WaveformModeToggleGroup;
    public List<Toggle> WaveformModeToggles;

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
        UpdateFields();
        recursionBuster = false;
    }

    public void OnDisable()
    {
        if (isDirty)
        {
            isDirty = false;
            Storage str = Chartmaker.PreferencesStorage;
            str.Set("PB:Speed", Speed);
            str.Set("TL:SeparationFactor", SeparationFactor);
            str.Set("TL:FollowSeekLine", FollowSeekLine);
            str.Set("TL:Waveform", Waveform);
            Chartmaker.main.StartSavePrefsRoutine();
        }
    }

    public void GetValues()
    {
        Storage str = Chartmaker.PreferencesStorage;
        Speed = str.Get("PB:Speed", 1f);
        SeparationFactor = str.Get("TL:SeparationFactor", 2);
        FollowSeekLine = str.Get("TL:FollowSeekLine", true);
        Waveform = str.Get("TL:Waveform", 1);
        WaveformMode = str.Get("TL:WaveformMode", 1);
    }

    public void SetValues()
    {
        Chartmaker.main.SongSource.pitch = Speed;
        if (TimelinePanel.main.SeparationFactor != SeparationFactor)
        {
            TimelinePanel.main.SeparationFactor = SeparationFactor;
            if (Chartmaker.main.CurrentSong != null) TimelinePanel.main.UpdateTimeline(true);
        }
    }

    public void UpdateFields()
    {
        SpeedField.text = Speed.ToString();
        SeparatorField.text = SeparationFactor.ToString();
        FollowSeekLineToggle.isOn = FollowSeekLine;
        for (int a = 0; a < WaveformToggles.Count; a++) WaveformToggles[a].isOn = a == Waveform;
        for (int a = 0; a < WaveformModeToggles.Count; a++) WaveformModeToggles[a].isOn = a == WaveformMode;
    }

    public void OnFieldSet()
    {
        if (recursionBuster) return;
        recursionBuster = true;
        float.TryParse(SpeedField.text, out Speed);
        int.TryParse(SeparatorField.text, out SeparationFactor);
        SeparationFactor = Mathf.Max(SeparationFactor, 2);
        FollowSeekLine = FollowSeekLineToggle.isOn;
        bool thing = false;
        if (!WaveformToggles[Waveform].isOn) thing = true;
        for (int a = 0; a < WaveformToggles.Count; a++) if (WaveformToggles[a].isOn) Waveform = a;
        if (!WaveformModeToggles[WaveformMode].isOn) thing = true;
        for (int a = 0; a < WaveformModeToggles.Count; a++) if (WaveformModeToggles[a].isOn) WaveformMode = a;
        if (thing && !Chartmaker.main.SongSource.isPlaying) TimelinePanel.main.UpdateTimeline(true);
        SetValues();
        recursionBuster = false;
        isDirty = true;
    }
}