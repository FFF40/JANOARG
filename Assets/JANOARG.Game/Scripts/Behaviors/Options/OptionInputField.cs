using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OptionInputField : MonoBehaviour
{
    public TMP_InputField InputField;
    public Slider Slider;
    public TMP_Text Title;
    public TMP_Text UnitLabel;
    [Space]
    public Graphic InputUnderline;
    public List<Graphic> SliderTints;

    public void SetColor(Color color) 
    {
        InputUnderline.color = InputField.textComponent.color = 
            Title.color = UnitLabel.color = color;

        foreach (Graphic graphic in SliderTints) graphic.color = color;
        
        InputField.selectionColor = color * new Color(1, 1, 1, .4f);
    }
}