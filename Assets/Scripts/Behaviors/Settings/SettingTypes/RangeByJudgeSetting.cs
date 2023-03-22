using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RangeByJudgeSetting : Setting
{
    public Slider Slider;
    public TMP_InputField Field;
    public TMP_InputField[] Fields;
    public float[] DefaultValue = new float[1];
    [Space]
    public float MinValue;
    public float MaxValue = 1;
    public float Step;
    [Space]
    public RectTransform FieldHolder;
    public CanvasGroup FieldGroup;
    public RectTransform FieldMask;
    public RectTransform SliderTransform;
    public CanvasGroup SliderGroup;
    public CanvasGroup MasterFieldGroup;
    public TMP_Text MinLabel;
    public TMP_Text MaxLabel;
    public RectTransform ChangeArrow;
    public RectTransform ChangeModeView;

    float lastSliderValue;
    Vector2? SliderPosition;

    [HideInInspector]
    public float[] Value;

    bool isAnimating;

    public void Start()
    {
        Slider.minValue = MinValue;
        Slider.maxValue = MaxValue;

        Value = Common.main.Storage.Get(ID, DefaultValue);
        Field.text = Value[0].ToString("0.###", CultureInfo.InvariantCulture);
        for (int a = 0; a < Fields.Length; a++)
        {
            Fields[a].contentType = Step > 0 && Step % 1 == 0
                ? TMP_InputField.ContentType.IntegerNumber 
                : TMP_InputField.ContentType.DecimalNumber;
            Fields[a].text = a < Value.Length ? Value[a].ToString("0.###", CultureInfo.InvariantCulture) : "";
        }
        
        Field.gameObject.SetActive(Value.Length < Fields.Length);
        FieldHolder.gameObject.SetActive(Value.Length >= Fields.Length);

        MinLabel.alpha = MaxLabel.alpha = 0;

        lastSliderValue = Slider.value;

        Slider.gameObject.SetActive(Value.Length < Fields.Length);

        Slider.onValueChanged.AddListener(_ => OnSliderChange());
        
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        OptionLerp(Value.Length < Fields.Length ? 0 : 1);
        FocusLerp(lastFocus);
    }

    public void OnFieldChange(int index)
    {
        if (float.TryParse((Value.Length < Fields.Length ? Field : Fields[index]).text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            if (Step > 0) value = Mathf.Round(value / Step) * Step;
            Value[Mathf.Min(index, Value.Length - 1)] = Mathf.Clamp(value, MinValue, MaxValue);
            Common.main.Storage.Set(ID, Value);
            if (index == 0) Slider.value = value;
        }
    }

    public void OnFieldFinish(int index)
    {
        Fields[index].text = Value[index].ToString("0.###", CultureInfo.InvariantCulture);
        if (index == 0) Field.text = Fields[index].text;
    }
    
    Coroutine SliderCoroutine;

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
                    if (SliderCoroutine != null) StopCoroutine(SliderCoroutine);
                    SliderCoroutine = StartCoroutine(SliderNudge(Vector2.left * Mathf.Min(difference, 6)));
                }
                if (Slider.value >= Slider.maxValue)
                {
                    if (SliderPosition != null) SliderTransform.anchoredPosition = (Vector2)SliderPosition;
                    if (SliderCoroutine != null) StopCoroutine(SliderCoroutine);
                    SliderCoroutine = StartCoroutine(SliderNudge(Vector2.right * Mathf.Min(difference, 6)));
                }
            }

            Fields[0].text = Field.text = Slider.value.ToString("0.###", CultureInfo.InvariantCulture);
            Value[0] = Slider.value;
            Common.main.Storage.Set(ID, Value);
        }
    }

    public void OnFieldFocus()
    {
        SettingsPanel.main.FocusSetting(this);
        MinLabel.text = Slider.minValue + " ≥";
        MaxLabel.text = "≥ " + Slider.maxValue;
    }

    public void ChangeMode()
    {
        if (isAnimating) return;

        float lastValue = Value[0];
        if (Value.Length < Fields.Length) System.Array.Fill(Value = new float[Fields.Length], lastValue);
        else Value = new float[]{ lastValue };

        Field.text = Value[0].ToString("0.###", CultureInfo.InvariantCulture);
        Field.gameObject.SetActive(Value.Length < Fields.Length);

        for (int a = 0; a < Value.Length; a++)
        {
            if (!float.TryParse(Fields[a].text, NumberStyles.Float, CultureInfo.InvariantCulture, out Value[a]))
                Fields[a].text = Value[0].ToString("0.###", CultureInfo.InvariantCulture);
        }

        if (Value.Length < Fields.Length) StartCoroutine(OptionOutAnimation());
        else StartCoroutine(OptionInAnimation());
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

    float lastOption = 0;

    public IEnumerator OptionInAnimation()
    {
        isAnimating = true;
        for (float a = 0; a < 1; a += Time.deltaTime / .3f)
        {
            OptionLerp(Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
            yield return null;
        }
        OptionLerp(1);
        isAnimating = false;
    }

    public IEnumerator OptionOutAnimation()
    {
        isAnimating = true;
        for (float a = 0; a < 1; a += Time.deltaTime / .3f)
        {
            OptionLerp(1 - Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
            yield return null;
        }
        OptionLerp(0);
        isAnimating = false;
    }

    public void OptionLerp (float value)
    {
        FieldHolder.sizeDelta = new Vector2(-192 - 200 * lastFocus, FieldHolder.sizeDelta.y);
        float width = Mathf.Lerp(-100, -FieldHolder.rect.width, value);

        FieldHolder.anchorMin = new Vector2(1 - value, 0);
        FieldHolder.anchorMax = new Vector2(2 - value, 1);
        FieldHolder.pivot = new Vector2(value, .5f);
        FieldHolder.anchoredPosition = new Vector2(-100 + 100 * value, 0);
        
        SliderTransform.sizeDelta = new Vector2(width - 216 - 200 * lastFocus, SliderTransform.sizeDelta.y);
        
        RectTransform rt = Field.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(width + 92 - 100 * lastFocus, 0);
        
        MasterFieldGroup.alpha = 1 - value;
        Field.gameObject.SetActive(MasterFieldGroup.alpha > 0);
        SliderGroup.alpha = 1 - value * 2;
        Slider.gameObject.SetActive(SliderGroup.alpha > 0);
        FieldGroup.alpha = value;
        FieldGroup.gameObject.SetActive(FieldGroup.alpha > 0);
        
        MinLabel.rectTransform.anchoredPosition = new Vector2(width - 16 - 100 * lastFocus, 0);

        ChangeArrow.localEulerAngles = Vector3.forward * (90 - 180 * value);
        ChangeArrow.anchoredPosition = Vector2.right * (8 - 16 * value);
        ChangeModeView.anchoredPosition = Vector2.right * (20 - 40 * value);

        lastOption = value;
    }

    float lastFocus = 0;
    
    public override void FocusLerp(float value)
    {
        FieldHolder.sizeDelta = new Vector2(-192 - 200 * value, FieldHolder.sizeDelta.y);
        float width = Mathf.Lerp(-100, -FieldHolder.rect.width, lastOption);
        FieldHolder.anchoredPosition = new Vector2(-100 + 100 * lastOption, 0);
        FieldMask.anchoredPosition = new Vector2(-8 - 100 * value, FieldMask.anchoredPosition.y);

        SliderTransform.sizeDelta = new Vector2(width - 216 - 200 * value, SliderTransform.sizeDelta.y);
        
        RectTransform rt = Field.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(width + 92 - 100 * value, 0);

        MinLabel.rectTransform.anchoredPosition = new Vector2(width - 16 - 100 * value, 0);
        MaxLabel.rectTransform.anchoredPosition = new Vector2(-100 * value, 0);
        lastFocus = MinLabel.alpha = MaxLabel.alpha = value;
    }
}
