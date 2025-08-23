using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Behaviors.Options.Input_Types;
using JANOARG.Client.UI;
using JANOARG.Shared.Data.ChartInfo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Options
{
    public class OptionInputHandler : MonoBehaviour
    {
        public static OptionInputHandler sMain;

        [Space]
        public Graphic Background;

        public Graphic InputBackground;

        [Space]
        public GameObject MainHolder;

        public TMP_Text      TitleText;
        public CanvasGroup   RightHolder;
        public RectTransform RightTransform;

        [Space]
        public TMP_Text BeforeText;

        public TMP_Text AfterText;

        [Space]
        public CanvasGroup AdvancedInputGroup;

        public RectTransform  AdvancedInputTransform;
        public AnimatedToggle AdvancedInputToggle;

        [Space]
        public CanvasGroup ListGroup;

        public RectTransform          ListTransform;
        public OptionInputListHandler ListHandler;

        [Space]
        public OptionInputField InputSample;

        public RectTransform          InputHolder;
        public List<OptionInputField> CurrentInputs;
        public LayoutGroup            InputLayout;
        public CanvasGroup            InputGroup;

        [Space]
        public OptionCalibrationWizard CalibrationWizard;

        [Space]
        public TMP_Text TextAnimSample;

        public RectTransform  TextAnimHolder;
        public List<TMP_Text> TextAnimLabels;

        [HideInInspector]
        public bool IsAnimating;

        [HideInInspector]
        public bool IsAdvanced;

        [HideInInspector]
        public OptionItem CurrentItem;

        bool _RecursionBuster = false;

        public void Awake()
        {
            sMain = this;
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
                    StringOptionInput stringItem = (StringOptionInput)item;

                    OptionInputField field = Instantiate(InputSample, InputHolder);
                    field.Slider.gameObject.SetActive(false);
                    field.InputField.text = stringItem.CurrentValue;
                    field.InputField.characterLimit = stringItem.Limit;
                    CurrentInputs.Add(field);

                    TMP_Text textAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    Vector3[] corners = new Vector3[4];
                    stringItem.ValueHolder.rectTransform.GetWorldCorners(corners);
                    textAnim.rectTransform.position = corners[3];
                    textAnim.text = stringItem.CurrentValue;
                    TextAnimLabels.Add(textAnim);

                    BeforeText.gameObject.SetActive(false);
                    AfterText.gameObject.SetActive(true);
                    AfterText.text = stringItem.CurrentValue.Length.ToString() + " / " + stringItem.Limit.ToString();
                    AdvancedInputGroup.gameObject.SetActive(false);
                    field.InputField.onValueChanged.AddListener(value =>
                    {
                        AfterText.text = value.Length.ToString() + " / " + stringItem.Limit.ToString();
                    });
                    field.InputField.onEndEdit.AddListener(value =>
                    {
                        textAnim.text = value;
                        stringItem.Set(value);
                        stringItem.UpdateValue();
                    });

                    break;
                }

                case JudgmentOffsetOptionInput:
                case VisualOffsetOptionInput:
                {
                    FloatOptionInput visualOffsetItem = (FloatOptionInput)item;

                    AdvancedInputGroup.gameObject.SetActive(false);
                    BeforeText.gameObject.SetActive(false);
                    AfterText.gameObject.SetActive(false);
                    CalibrationWizard.IntializeWizard((FloatOptionInput)item);

                    TMP_InputField field = CalibrationWizard.InputField;
                    field.text = visualOffsetItem.CurrentValue.ToString();

                    TMP_Text unit = CalibrationWizard.InputFieldUnit;
                    unit.text = "<alpha=#77>" + visualOffsetItem.Unit;

                    TMP_Text textAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    Vector3[] corners = new Vector3[4];
                    visualOffsetItem.ValueHolder.rectTransform.GetWorldCorners(corners);
                    textAnim.rectTransform.position = corners[3];
                    textAnim.text = visualOffsetItem.ValueHolder.text;
                    TextAnimLabels.Add(textAnim);

                    TMP_Text unitAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    visualOffsetItem.UnitLabel.rectTransform.GetWorldCorners(corners);
                    unitAnim.rectTransform.position = corners[3];
                    unitAnim.font = visualOffsetItem.UnitLabel.font;
                    unitAnim.text = visualOffsetItem.UnitLabel.text;
                    TextAnimLabels.Add(unitAnim);
                    field.onEndEdit.AddListener(value =>
                    {
                        if (_RecursionBuster) return;

                        _RecursionBuster = true;
                        bool valid = float.TryParse(value, out float val);

                        if (valid)
                        {
                            val = Mathf.Clamp(val, visualOffsetItem.Min, visualOffsetItem.Max);
                            if (visualOffsetItem.Step != 0) val = Mathf.Round(val / visualOffsetItem.Step) * visualOffsetItem.Step;
                            visualOffsetItem.Set(val);
                            visualOffsetItem.UpdateValue();
                        }

                        textAnim.text = field.text = visualOffsetItem.CurrentValue.ToString();
                        _RecursionBuster = false;
                    });

                    break;
                }

                case FloatOptionInput:
                {
                    FloatOptionInput floatItem = (FloatOptionInput)item;

                    OptionInputField field = Instantiate(InputSample, InputHolder);
                    field.InputField.text = floatItem.CurrentValue.ToString();
                    field.InputField.contentType = floatItem.Step != 0 && floatItem.Step % 1 == 0
                        ? TMP_InputField.ContentType.IntegerNumber
                        : TMP_InputField.ContentType.DecimalNumber;
                    field.Slider.minValue = floatItem.Min;
                    field.Slider.maxValue = floatItem.Max;
                    field.Slider.value = floatItem.CurrentValue;
                    CurrentInputs.Add(field);

                    TMP_Text textAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    Vector3[] corners = new Vector3[4];
                    floatItem.ValueHolder.rectTransform.GetWorldCorners(corners);
                    textAnim.rectTransform.position = corners[3];
                    textAnim.text = field.InputField.text;
                    TextAnimLabels.Add(textAnim);

                    TMP_Text unitAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    floatItem.UnitLabel.rectTransform.GetWorldCorners(corners);
                    unitAnim.rectTransform.position = corners[3];
                    unitAnim.font = floatItem.UnitLabel.font;
                    field.UnitLabel.text = unitAnim.text = floatItem.UnitLabel.text;
                    TextAnimLabels.Add(unitAnim);

                    BeforeText.gameObject.SetActive(true);
                    BeforeText.text = floatItem.Min + "<alpha=#77>" + floatItem.Unit + "<alpha=#ff> ≤";
                    AfterText.gameObject.SetActive(true);
                    AfterText.text = "≤ " + floatItem.Max + "<alpha=#77>" + floatItem.Unit;
                    AdvancedInputGroup.gameObject.SetActive(false);

                    field.InputField.onEndEdit.AddListener(value =>
                    {
                        if (_RecursionBuster) return;

                        _RecursionBuster = true;
                        bool valid = float.TryParse(value, out float val);

                        if (valid)
                        {
                            val = Mathf.Clamp(val, floatItem.Min, floatItem.Max);
                            if (floatItem.Step != 0) val = Mathf.Round(val / floatItem.Step) * floatItem.Step;
                            field.Slider.value = val;
                            floatItem.Set(val);
                            floatItem.UpdateValue();
                        }

                        textAnim.text = field.InputField.text = floatItem.CurrentValue.ToString();
                        _RecursionBuster = false;
                    });
                    field.Slider.onValueChanged.AddListener(val =>
                    {
                        if (_RecursionBuster) return;

                        val = Mathf.Clamp(val, floatItem.Min, floatItem.Max);
                        if (floatItem.Step != 0) val = Mathf.Round(val / floatItem.Step) * floatItem.Step;
                        textAnim.text = field.InputField.text = val.ToString();
                        floatItem.Set(val);
                        floatItem.UpdateValue();
                        _RecursionBuster = false;
                    });

                    break;
                }

                case MultiFloatOptionInput:
                {
                    MultiFloatOptionInput multiFloatInput = (MultiFloatOptionInput)item;
                    var fieldInfos = MultiValueFieldData.sInfo[multiFloatInput.ValueType];
                    bool recursionBuster = false;

                    IsAdvanced = multiFloatInput.CurrentValue.Length > 1;

                    for (int a = 0; a < fieldInfos.Count; a++)
                    {
                        int aa = a;
                        var fieldInfo = fieldInfos[a];

                        OptionInputField field = Instantiate(InputSample, InputHolder);
                        field.InputField.text = (multiFloatInput.CurrentValue.Length > 1 ? multiFloatInput.CurrentValue[a] : multiFloatInput.CurrentValue[0]).ToString();
                        field.InputField.contentType = multiFloatInput.Step != 0 && multiFloatInput.Step % 1 == 0
                            ? TMP_InputField.ContentType.IntegerNumber
                            : TMP_InputField.ContentType.DecimalNumber;
                        field.UnitLabel.text = "<alpha=#77>" + multiFloatInput.Unit;
                        field.Slider.minValue = multiFloatInput.Min;
                        field.Slider.maxValue = multiFloatInput.Max;
                        field.Slider.value = multiFloatInput.CurrentValue.Length > 1 ? multiFloatInput.CurrentValue[a] : multiFloatInput.CurrentValue[0];
                        field.SetColor(fieldInfo.Color);
                        CurrentInputs.Add(field);

                        TMP_Text textAnim = Instantiate(TextAnimSample, TextAnimHolder);
                        Vector3[] corners = new Vector3[4];
                        multiFloatInput.ValueHolders[a]
                            .rectTransform.GetWorldCorners(corners);
                        textAnim.rectTransform.position = corners[3];
                        textAnim.text = multiFloatInput.ValueHolders[a].text;
                        TextAnimLabels.Add(textAnim);

                        TMP_Text unitAnim = Instantiate(TextAnimSample, TextAnimHolder);
                        multiFloatInput.UnitLabels[a]
                            .rectTransform.GetWorldCorners(corners);
                        unitAnim.rectTransform.position = corners[3];
                        unitAnim.font = multiFloatInput.UnitLabels[a].font;
                        unitAnim.text = multiFloatInput.UnitLabels[a].text;
                        TextAnimLabels.Add(unitAnim);

                        textAnim.color = unitAnim.color = fieldInfo.Color;

                        field.InputField.onEndEdit.AddListener(value =>
                        {
                            if (recursionBuster) return;

                            recursionBuster = true;
                            bool valid = float.TryParse(value, out float val);

                            if (valid)
                            {
                                val = Mathf.Clamp(val, multiFloatInput.Min, multiFloatInput.Max);
                                if (multiFloatInput.Step != 0) val = Mathf.Round(val / multiFloatInput.Step) * multiFloatInput.Step;
                                field.Slider.value = val;
                                multiFloatInput.CurrentValue[aa] = val;
                                multiFloatInput.Set(multiFloatInput.CurrentValue);
                                multiFloatInput.UpdateValue();
                            }

                            textAnim.text = field.InputField.text = multiFloatInput.CurrentValue[aa]
                                .ToString();
                            recursionBuster = false;
                        });
                        field.Slider.onValueChanged.AddListener(val =>
                        {
                            if (recursionBuster) return;

                            val = Mathf.Clamp(val, multiFloatInput.Min, multiFloatInput.Max);
                            if (multiFloatInput.Step != 0) val = Mathf.Round(val / multiFloatInput.Step) * multiFloatInput.Step;
                            textAnim.text = field.InputField.text = val.ToString();
                            multiFloatInput.CurrentValue[aa] = val;
                            multiFloatInput.Set(multiFloatInput.CurrentValue);
                            multiFloatInput.UpdateValue();
                            recursionBuster = false;
                        });
                    }

                    BeforeText.gameObject.SetActive(true);
                    BeforeText.text = multiFloatInput.Min + "<alpha=#77>" + multiFloatInput.Unit + "<alpha=#ff> ≤";
                    AfterText.gameObject.SetActive(true);
                    AfterText.text = "≤ " + multiFloatInput.Max + "<alpha=#77>" + multiFloatInput.Unit;
                    AdvancedInputGroup.gameObject.SetActive(true);
                    recursionBuster = true;
                    AdvancedInputToggle.value = IsAdvanced;
                    recursionBuster = false;
                    SetAdvancedInput(multiFloatInput.ValueType, IsAdvanced);

                    break;
                }

                case ListOptionInput:
                {
                    Debug.Log("ListOptionInput");
                    ListOptionInput listInput = (ListOptionInput)item;

                    TMP_Text textAnim = Instantiate(TextAnimSample, TextAnimHolder);
                    Vector3[] corners = new Vector3[4];
                    listInput.ValueHolder.rectTransform.GetWorldCorners(corners);
                    textAnim.rectTransform.position = corners[3];
                    textAnim.text = listInput.ValueHolder.text;
                    TextAnimLabels.Add(textAnim);

                    ListHandler.gameObject.SetActive(true);
                    ListHandler.SetList(item as ListOptionInput<string>);
                    AdvancedInputGroup.gameObject.SetActive(false);
                    BeforeText.text = AfterText.text = "";

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
                    StringOptionInput stringInput = (StringOptionInput)item;
                    inputLerp = x =>
                    {
                        LerpText(TextAnimLabels[0], stringInput.ValueHolder, CurrentInputs[0].InputField.textComponent, x);
                    };
                    endLerp = () =>
                    {
                    };

                    break;
                }
                case JudgmentOffsetOptionInput:
                case VisualOffsetOptionInput:
                {
                    FloatOptionInput input = (FloatOptionInput)item;
                    inputLerp = x =>
                    {
                        LerpText(TextAnimLabels[0], input.ValueHolder, CalibrationWizard.InputField.textComponent, x);
                        LerpText(TextAnimLabels[1], input.UnitLabel, CalibrationWizard.InputFieldUnit, x);
                    };
                    endLerp = () =>
                    {
                    };

                    break;
                }
                case FloatOptionInput:
                {
                    FloatOptionInput floatInput = (FloatOptionInput)item;
                    inputLerp = x =>
                    {
                        LerpText(TextAnimLabels[0], floatInput.ValueHolder, CurrentInputs[0].InputField.textComponent, x);
                        LerpText(TextAnimLabels[1], floatInput.UnitLabel, CurrentInputs[0].UnitLabel, x);
                    };
                    endLerp = () =>
                    {
                    };

                    break;
                }
                case MultiFloatOptionInput:
                {
                    MultiFloatOptionInput multiFloatInput = (MultiFloatOptionInput)item;
                    inputLerp = x =>
                    {
                        for (int a = 0; a < CurrentInputs.Count; a++)
                        {
                            LerpText(TextAnimLabels[a * 2], multiFloatInput.ValueHolders[a], CurrentInputs[a].InputField.textComponent, x);
                            LerpText(TextAnimLabels[(a * 2) + 1], multiFloatInput.UnitLabels[a], CurrentInputs[a].UnitLabel, x);
                        }
                    };
                    endLerp = () =>
                    {
                    };

                    break;
                }
                case ListOptionInput:
                {
                    ListOptionInput listInput = (ListOptionInput)item;
                    inputLerp = x =>
                    {
                        LerpText(TextAnimLabels[0], listInput.ValueHolder, ListHandler.Items[ListHandler.CurrentPosition].Text, x);
                    };
                    endLerp = () =>
                    {
                    };

                    break;
                }

                default:
                {
                    inputLerp = _ => { };
                    endLerp = () => { };

                    break;
                }
            }
        }

        public void GetItemFinishFunctions(OptionItem item, out Action onFinish)
        {
            switch (item)
            {
                case JudgmentOffsetOptionInput:
                case VisualOffsetOptionInput:
                {
                    onFinish = () =>
                    {
                        CalibrationWizard.HideWizard();
                    };

                    break;
                }

                case ListOptionInput:
                {
                    onFinish = () =>
                    {
                        ListHandler.Finish();
                        TextAnimLabels[0].text = ListHandler.Items[ListHandler.CurrentPosition].Text.text;
                    };

                    break;
                }

                default:
                {
                    onFinish = () => { };

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
            Vector2 listPos = ListTransform.anchoredPosition;

            yield return Ease.Animate(.45f, x =>
            {
                float ease = Ease.Get(x * 1.5f, EaseFunction.Cubic, EaseMode.Out);
                Background.color = new Color(0, 0, 0, .5f * ease);
                InputBackground.rectTransform.sizeDelta = new(InputBackground.rectTransform.sizeDelta.x, ease * 40);

                float ease2 = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                float offset = 30 * (1 - ease2);
                TitleText.rectTransform.anchoredPosition = titlePos + (Vector2.left * offset);
                RightTransform.anchoredPosition = rightPos + (Vector2.right * offset);
                ListTransform.anchoredPosition = listPos + (Vector2.right * offset);

                float ease3 = Ease.Get((x * 1.5f) - .5f, EaseFunction.Cubic, EaseMode.Out);
                TitleText.alpha = RightHolder.alpha = ListGroup.alpha = ease3;
                AdvancedInputTransform.anchoredPosition = new(
                    (IsAdvanced ? -690 : -190) - (10 * ease3),
                    AdvancedInputTransform.anchoredPosition.y
                );
                AdvancedInputGroup.alpha = ease3;

                inputLerp(ease);
            });

            endLerp();

            IsAnimating = false;
        }

        public void EndEdit()
        {
            if (IsAnimating)
                return;

            StartCoroutine(EditOutro());
        }

        public IEnumerator EditOutro()
        {
            IsAnimating = true;
            GetItemLerpFunctions(CurrentItem, out Action<float> inputLerp, out Action endLerp);
            GetItemFinishFunctions(CurrentItem, out Action onFinish);
            onFinish();

            Vector2 titlePos = TitleText.rectTransform.anchoredPosition;
            Vector2 rightPos = RightTransform.anchoredPosition;
            Vector2 listPos  = ListTransform.anchoredPosition;

            yield return Ease.Animate(.3f, x =>
            {
                float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                InputBackground.rectTransform.sizeDelta = new(InputBackground.rectTransform.sizeDelta.x, (1 - ease) * 40);
                Background.color = new Color(0, 0, 0, .5f * (1 - ease));

                float offset = 10 * ease;
                TitleText.rectTransform.anchoredPosition = titlePos + (Vector2.left * offset);
                RightTransform.anchoredPosition = rightPos + (Vector2.right * offset);
                ListTransform.anchoredPosition = listPos + (Vector2.right * offset);
                TitleText.alpha = RightHolder.alpha = ListGroup.alpha = 1 - ease;

                AdvancedInputTransform.anchoredPosition = new(
                    (IsAdvanced ? -700 : -200) + (10 * ease),
                    AdvancedInputTransform.anchoredPosition.y
                );
                AdvancedInputGroup.alpha = 1 - ease;

                inputLerp(1 - ease);
            });

            foreach (var field in CurrentInputs)
                Destroy(field.gameObject);
            CurrentInputs.Clear();
            
            foreach (var textAnim in TextAnimLabels) 
                Destroy(textAnim.gameObject);
            TextAnimLabels.Clear();

            Background.gameObject.SetActive(false);
            MainHolder.SetActive(false);
            ListHandler.gameObject.SetActive(false);
            TitleText.rectTransform.anchoredPosition = titlePos;
            RightTransform.anchoredPosition = rightPos;
            ListTransform.anchoredPosition = listPos;

            IsAnimating = false;
        }

        public static void LerpText(TMP_Text textAnim, TMP_Text from, TMP_Text to, float lerp)
        {
            from.alpha = lerp == 0 ? 1 : 0;
            to.alpha = Mathf.Approximately(lerp, 1) ? 1 : 0;
            textAnim.gameObject.SetActive(lerp != 0 && !Mathf.Approximately(lerp, 1));

            Vector3[] corners = new Vector3[4];
            from.rectTransform.GetWorldCorners(corners);
            Vector3 cornerFrom = corners[0];
            to.rectTransform.GetWorldCorners(corners);
            Vector3 cornerTo = corners[0];
            textAnim.rectTransform.position = Vector3.Lerp(cornerFrom, cornerTo, lerp);
        }

        public void UpdateAdvancedInput()
        {
            if (IsAnimating || _RecursionBuster)
            {
                _RecursionBuster = true;
                AdvancedInputToggle.value = !AdvancedInputToggle.value;
                _RecursionBuster = false;

                return;
            }

            MultiValueType type;

            switch (CurrentItem)
            {
                case MultiFloatOptionInput:
                {
                    MultiFloatOptionInput multiFloatInput = (MultiFloatOptionInput)CurrentItem;
                    type = multiFloatInput.ValueType;
                    multiFloatInput.Set(AdvancedInputToggle.value ? CurrentInputs.Select(x => x.Slider.value)
                        .ToArray() : new[]
                    {
                        CurrentInputs[0].Slider.value,
                    });
                    multiFloatInput.UpdateValue();

                    break;
                }

                default: return;
            }

            StartCoroutine(AdvancedInputAnim(type, AdvancedInputToggle.value));
        }

        public void SetAdvancedInput(MultiValueType type, bool active)
        {
            IsAdvanced = active;
            var list = MultiValueFieldData.sInfo[type];

            Color firstColor = active ? list[0].Color : Color.white;
            TextAnimLabels[0].color = TextAnimLabels[1].color = firstColor;
            CurrentInputs[0]
                .SetColor(firstColor);

            for (int a = 0; a < CurrentInputs.Count; a++)
            {
                CurrentInputs[a].Title.text = active ? list[a].Name : "";
            }

            for (int a = 1; a < CurrentInputs.Count; a++)
            {
                CurrentInputs[a]
                    .gameObject.SetActive(active);
                TextAnimLabels[a * 2].text = active ? CurrentInputs[a].InputField.text : "";
                TextAnimLabels[(a * 2) + 1].text = active ? CurrentInputs[a].UnitLabel.text : "";
            }
        }

        public IEnumerator AdvancedInputAnim(MultiValueType type, bool active)
        {
            IsAnimating = true;

            if (active)
            {
                StartCoroutine(Ease.Animate(.4f, x =>
                {
                    float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                    AdvancedInputTransform.anchoredPosition = new(-200 - (500 * ease), AdvancedInputTransform.anchoredPosition.y);
                }));
            }
            else
            {
                StartCoroutine(Ease.Animate(.5f, x =>
                {
                    float ease2 = Ease.Get(Mathf.Clamp01((x * 1.2f) - .2f), EaseFunction.Cubic, EaseMode.Out);
                    AdvancedInputTransform.anchoredPosition = new(-700 + (500 * ease2), AdvancedInputTransform.anchoredPosition.y);
                }));
            }

            yield return Ease.Animate(.2f, x =>
            {
                float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                InputHolder.sizeDelta = new(InputHolder.sizeDelta.x, -30 * ease);
                InputGroup.alpha = (1 - ease) * (1 - ease);
            });

            SetAdvancedInput(type, active);

            yield return Ease.Animate(.4f, x =>
            {
                float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                InputHolder.sizeDelta = new(InputHolder.sizeDelta.x, -30 * (1 - ease));
                InputGroup.alpha = ease * ease;
            });

            IsAnimating = false;
        }
    }
}