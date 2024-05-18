using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Windows;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class OptionInputHandler : MonoBehaviour
{
    public static OptionInputHandler main;
    [Space]
    public Graphic Background;
    public Graphic InputBackground;
    [Space]
    public GameObject MainHolder;
    public TMP_Text TitleText;
    public CanvasGroup RightHolder;
    public RectTransform RightTransform;
    [Space]
    public TMP_Text BeforeText;
    public TMP_Text AfterText;

    [Space]
    public OptionInputField InputSample;
    public RectTransform InputHolder;
    public List<OptionInputField> CurrentInputs;
    [Space]
    public TMP_Text TextAnimSample;
    public RectTransform TextAnimHolder;
    public List<TMP_Text> TextAnimLabels;

    [HideInInspector]
    public bool IsAnimating;
    [HideInInspector]
    public OptionItem CurrentItem;

    public void Awake() 
    {
        main = this;
    }

    public void Start() 
    {
        Background.gameObject.SetActive(false);
        MainHolder.SetActive(false);
    }

    public void Edit(OptionItem item)
    {
        if (IsAnimating) return;

        switch (item)
        {
            case StringOptionInput:
                {
                    StringOptionInput i = (StringOptionInput)item;

                    OptionInputField field = Instantiate(InputSample, InputHolder);
                    field.Slider.gameObject.SetActive(false);
                    field.InputField.text = i.CurrentValue;
                    field.InputField.characterLimit = i.Limit;
                    CurrentInputs.Add(field);

                    TMP_Text textAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    Vector3[] corners = new Vector3[4];
                    i.ValueHolder.rectTransform.GetWorldCorners(corners);
                    textAnim.rectTransform.position = corners[3];
                    textAnim.text = i.CurrentValue;
                    TextAnimLabels.Add(textAnim);

                    BeforeText.gameObject.SetActive(false);
                    AfterText.gameObject.SetActive(true);
                    AfterText.text = i.CurrentValue.Length.ToString() + " / " + i.Limit.ToString();
                    field.InputField.onValueChanged.AddListener(value =>
                    {
                        AfterText.text = value.Length.ToString() + " / " + i.Limit.ToString();
                    });
                    field.InputField.onEndEdit.AddListener(value =>
                    {
                        textAnim.text = value;
                        i.Set(value);
                        i.UpdateValue();
                    });
                    break;
                }

            case FloatOptionInput:
                {
                    FloatOptionInput i = (FloatOptionInput)item;

                    OptionInputField field = Instantiate(InputSample, InputHolder);
                    field.InputField.text = i.CurrentValue.ToString();
                    field.InputField.contentType = i.Step != 0 && i.Step % 1 == 0
                        ? TMP_InputField.ContentType.IntegerNumber
                        : TMP_InputField.ContentType.DecimalNumber;
                    field.Slider.minValue = i.Min;
                    field.Slider.maxValue = i.Max;
                    field.Slider.value = i.CurrentValue;
                    CurrentInputs.Add(field);

                    TMP_Text textAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    Vector3[] corners = new Vector3[4];
                    i.ValueHolder.rectTransform.GetWorldCorners(corners);
                    textAnim.rectTransform.position = corners[3];
                    textAnim.text = field.InputField.text;
                    TextAnimLabels.Add(textAnim);

                    TMP_Text unitAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    i.UnitLabel.rectTransform.GetWorldCorners(corners);
                    unitAnim.rectTransform.position = corners[3];
                    unitAnim.font = i.UnitLabel.font;
                    field.UnitLabel.text = unitAnim.text = i.UnitLabel.text;
                    TextAnimLabels.Add(unitAnim);

                    BeforeText.gameObject.SetActive(true);
                    BeforeText.text = i.Min + "<alpha=#77>" + i.Unit + "<alpha=#ff> <";
                    AfterText.gameObject.SetActive(true);
                    AfterText.text = "< " + i.Max + "<alpha=#77>" + i.Unit;

                    bool recursionBuster = false;
                    field.InputField.onEndEdit.AddListener(value =>
                    {
                        if (recursionBuster) return;
                        recursionBuster = true;
                        bool valid = float.TryParse(value, out float val);
                        if (valid)
                        {
                            val = Mathf.Clamp(val, i.Min, i.Max);
                            if (i.Step != 0) val = Mathf.Round(val / i.Step) * i.Step;
                            field.Slider.value = val;
                            i.Set(val);
                            i.UpdateValue();
                        }
                        textAnim.text = field.InputField.text = i.CurrentValue.ToString();
                        recursionBuster = false;
                    });
                    field.Slider.onValueChanged.AddListener(val =>
                    {
                        if (recursionBuster) return;
                        val = Mathf.Clamp(val, i.Min, i.Max);
                        if (i.Step != 0) val = Mathf.Round(val / i.Step) * i.Step;
                        textAnim.text = field.InputField.text = val.ToString();
                        i.Set(val);
                        i.UpdateValue();
                        recursionBuster = false;
                    });
                    break;
                }

            default:
                return;
        }

        if (CurrentInputs.Count > 0) 
        {
            SelectInputField(CurrentInputs[0].InputField);
        }

        CurrentItem = item;
        StartCoroutine(EditIntro(item));
    }

    public void SelectInputField(TMP_InputField field)
    {
        StartCoroutine(SelectInputFieldAnim(field));
    }

    public IEnumerator SelectInputFieldAnim(TMP_InputField field)
    {
        field.Select();
        yield return null;
        field.ActivateInputField();
    }

    public void GetItemLerpFunctions(OptionItem item, out Action<float> inputLerp, out Action endLerp) 
    {
        switch (item) 
        {
            case StringOptionInput: 
                {
                    StringOptionInput input = (StringOptionInput)item;
                    inputLerp = x => {
                        LerpText(TextAnimLabels[0], input.ValueHolder, CurrentInputs[0].InputField.textComponent, x);
                    };
                    endLerp = () => {
                    };
                    break;
                }
            case FloatOptionInput: 
                {
                    FloatOptionInput input = (FloatOptionInput)item;
                    inputLerp = x => {
                        LerpText(TextAnimLabels[0], input.ValueHolder, CurrentInputs[0].InputField.textComponent, x);
                        LerpText(TextAnimLabels[1], input.UnitLabel, CurrentInputs[0].UnitLabel, x);
                    };
                    endLerp = () => {
                    };
                    break;
                }

            default:
                {
                    inputLerp = _ => {};
                    endLerp = () => {};
                    break;
                }
        }
    }

    public IEnumerator EditIntro(OptionItem item)
    {
        IsAnimating = true;
        GetItemLerpFunctions(item, out Action<float> inputLerp, out Action endLerp);
        
        Background.gameObject.SetActive(true);
        MainHolder.SetActive(true);

        TitleText.text = item.TitleLabel.text;
        Vector2 titlePos = TitleText.rectTransform.anchoredPosition;
        TitleText.alpha = RightHolder.alpha = 0;
        Vector2 rightPos = RightTransform.anchoredPosition;

        yield return Ease.Animate(.45f, x => {

            float ease = Ease.Get(x * 1.5f, EaseFunction.Cubic, EaseMode.Out);
            Background.color = new Color(0, 0, 0, .5f * ease);
            InputBackground.rectTransform.sizeDelta = new (InputBackground.rectTransform.sizeDelta.x, ease * 40);

            float ease2 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            TitleText.rectTransform.anchoredPosition = titlePos + Vector2.left * 30 * (1 - ease2);
            RightTransform.anchoredPosition = rightPos + Vector2.right * 30 * (1 - ease2);

            float ease3 = Ease.Get(x * 1.5f - .5f, EaseFunction.Cubic, EaseMode.Out);
            TitleText.alpha = RightHolder.alpha = ease3;

            inputLerp(ease); 
        });

        endLerp();
    
        IsAnimating = false;
    }

    public void EndEdit() 
    {
        if (IsAnimating) return;
        StartCoroutine(EditOutro());
    }

    public IEnumerator EditOutro()
    {
        IsAnimating = true;
        GetItemLerpFunctions(CurrentItem, out Action<float> inputLerp, out Action endLerp);

        Vector2 titlePos = TitleText.rectTransform.anchoredPosition;
        Vector2 rightPos = RightTransform.anchoredPosition;

        yield return Ease.Animate(.3f, x => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            InputBackground.rectTransform.sizeDelta = new (InputBackground.rectTransform.sizeDelta.x, (1 - ease) * 40);
            Background.color = new Color(0, 0, 0, .5f * (1 - ease));

            TitleText.rectTransform.anchoredPosition = titlePos + Vector2.left * 10 * ease;
            RightTransform.anchoredPosition = rightPos + Vector2.right * 10 * ease;
            TitleText.alpha = RightHolder.alpha = 1 - ease;

            inputLerp(1 - ease);
        });

        foreach (var field in CurrentInputs) Destroy(field.gameObject);
        CurrentInputs.Clear();
        foreach (var textAnim in TextAnimLabels) Destroy(textAnim.gameObject);
        TextAnimLabels.Clear();
        
        Background.gameObject.SetActive(false);
        MainHolder.SetActive(false);
        TitleText.rectTransform.anchoredPosition = titlePos;
        RightTransform.anchoredPosition = rightPos;

        IsAnimating = false;
    }

    public static void LerpText(TMP_Text textAnim, TMP_Text from, TMP_Text to, float lerp)
    {
        from.alpha = lerp == 0 ? 1 : 0;
        to.alpha = lerp == 1 ? 1 : 0;
        textAnim.gameObject.SetActive(lerp != 0 && lerp != 1);

        Vector3[] corners = new Vector3[4];
        from.rectTransform.GetWorldCorners(corners);
        Vector3 cornerFrom = corners[0];
        to.rectTransform.GetWorldCorners(corners);
        Vector3 cornerTo = corners[0];
        textAnim.rectTransform.position = Vector3.Lerp(cornerFrom, cornerTo, lerp);
    }
}