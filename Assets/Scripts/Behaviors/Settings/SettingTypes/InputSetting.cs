using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputSetting : Setting
{
    public TMP_InputField Field;
    public string DefaultValue;

    public TMP_Text Limit;

    public void Start()
    {
        Field.text = Common.main.Storage.Get(ID, DefaultValue);
        Limit.alpha = 0;
    }

    public void OnType()
    {
        if (Field.characterLimit > 0) Limit.text = Field.text.Length + " / " + Field.characterLimit;
    }

    public void OnChange()
    {
        Common.main.Storage.Set(ID, Field.text);
    }

    public void OnFieldFocus()
    {
        SettingsPanel.main.FocusSetting(this);
        if (Field.characterLimit > 0) Limit.text = Field.text.Length + " / " + Field.characterLimit;
    }

    public override void FocusLerp(float value)
    {
        if (Field.characterLimit > 0)
        {
            RectTransform rt = Field.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(-208 - 100 * value, rt.sizeDelta.y);
            Limit.rectTransform.anchoredPosition = new Vector2(100 * (1 - value), 0);
            Limit.alpha = value;
        }
    }
}
