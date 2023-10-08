using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineOptionsPanel : MonoBehaviour
{
    [Header("Fields")]
    public float Speed = 1;
    public int SeparationFactor = 2;
    public bool FollowSeekLine = true;

    [Header("Objects")]
    public TMP_InputField SpeedField;
    public TMP_InputField SeparatorField;
    public Toggle FollowSeekLineToggle;

    bool recursionBuster;
    bool isDirty;

    public void Awake()
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
            Storage str = Chartmaker.main.PreferencesStorage;
            str.Set("PBK_Speed", Speed);
            str.Set("TML_SeparationFactor", SeparationFactor);
            str.Set("TML_FollowSeekLine", FollowSeekLine);
            Chartmaker.main.StartSavePrefsRoutine();
        }
    }

    public void GetValues()
    {
        Storage str = Chartmaker.main.PreferencesStorage;
        Speed = str.Get("PBK_Speed", 1f);
        SeparationFactor = str.Get("TML_SeparationFactor", 2);
        FollowSeekLine = str.Get("TML_FollowSeekLine", true);
    }

    public void SetValues()
    {
        Chartmaker.main.SongSource.pitch = Speed;
        if (TimelinePanel.main.SeparationFactor != SeparationFactor)
        {
            TimelinePanel.main.SeparationFactor = SeparationFactor;
            TimelinePanel.main.UpdateTimeline(true);
        }
    }

    public void UpdateFields()
    {
        SpeedField.text = Speed.ToString();
        SeparatorField.text = SeparationFactor.ToString();
        FollowSeekLineToggle.isOn = FollowSeekLine;
    }

    public void OnFieldSet()
    {
        if (recursionBuster) return;
        recursionBuster = true;
        float.TryParse(SpeedField.text, out Speed);
        int.TryParse(SeparatorField.text, out SeparationFactor);
        SeparationFactor = Mathf.Max(SeparationFactor, 2);
        FollowSeekLine = FollowSeekLineToggle.isOn;
        SetValues();
        recursionBuster = false;
        isDirty = true;
    }
}