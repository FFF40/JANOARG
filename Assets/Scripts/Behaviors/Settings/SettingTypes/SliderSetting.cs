using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderSetting : Setting
{
    public Slider Slider;
    public TMP_InputField Field;
    public float DefaultValue;
    [Space]
    public float MinValue;
    public float MaxValue = 1;
    public float Step;
    [Space]
    public RectTransform SliderTransform;
    public TMP_Text MinLabel;
    public TMP_Text MaxLabel;

    float lastSliderValue;
    Vector2? SliderPosition;

    public void Start()
    {
        Slider.minValue = MinValue;
        Slider.maxValue = MaxValue;
        Field.contentType = Step > 0 && Step % 1 == 0
            ? TMP_InputField.ContentType.IntegerNumber 
            : TMP_InputField.ContentType.DecimalNumber;

        Slider.value = Common.main.Storage.Get(ID, DefaultValue);
        Field.text = Slider.value.ToString("0.###", CultureInfo.InvariantCulture);
        MinLabel.alpha = MaxLabel.alpha = 0;

        lastSliderValue = Slider.value;
        
        Slider.onValueChanged.AddListener(_ => OnSliderChange());
    }

    public void OnSliderChange()
    {
        if (!Field.isFocused)
        {
            if (Step > 0) Slider.value = Mathf.Round(Slider.value / Step) * Step;

            float difference = Mathf.Abs(lastSliderValue - Slider.value);
            difference = difference <= Step ? 0 : difference / (Slider.maxValue - Slider.minValue) / Time.deltaTime;
            lastSliderValue = Slider.value;
            
            if (difference > 1)
            {
                if (Slider.value <= Slider.minValue)
                {
                    if (SliderPosition != null) SliderTransform.anchoredPosition = (Vector2)SliderPosition;
                    StopAllCoroutines();
                    StartCoroutine(SliderNudge(Vector2.left * Mathf.Min(difference, 6)));
                }
                if (Slider.value >= Slider.maxValue)
                {
                    if (SliderPosition != null) SliderTransform.anchoredPosition = (Vector2)SliderPosition;
                    StopAllCoroutines();
                    StartCoroutine(SliderNudge(Vector2.right * Mathf.Min(difference, 6)));
                }
            }

            Field.text = Slider.value.ToString("0.###", CultureInfo.InvariantCulture);
            Common.main.Storage.Set(ID, Slider.value);
        }
    }

    public void OnFieldChange()
    {
        if (float.TryParse(Field.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            if (Step > 0) Slider.value = Mathf.Round(Slider.value / Step) * Step;
            lastSliderValue = Slider.value = value;
            Common.main.Storage.Set(ID, Slider.value);
        }
    }

    public void OnFieldFocus()
    {
        SettingsPanel.main.FocusSetting(this);
        MinLabel.text = Slider.minValue + " ≥";
        MaxLabel.text = "≥ " + Slider.maxValue;
    }

    public IEnumerator SliderNudge(Vector2 pos)
    {
        SliderPosition = SliderTransform.anchoredPosition;

        void LerpText(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Cubic, EaseMode.Out);

            SliderTransform.anchoredPosition = (Vector2)SliderPosition + pos * (1 - ease);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .2f)
        {
            LerpText(a);
            yield return null;
        }
        LerpText(1);
        
        SliderPosition = null;
    }
    
    public override void FocusLerp(float value)
    {
        RectTransform rt = Slider.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(-316 - 200 * value, rt.sizeDelta.y);
        
        rt = Field.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(-8 - 100 * value, 0);

        MinLabel.rectTransform.anchoredPosition = new Vector2(-116 - 100 * value, 0);
        MaxLabel.rectTransform.anchoredPosition = new Vector2(-100 * value, 0);
        MinLabel.alpha = MaxLabel.alpha = value;
    }
}
