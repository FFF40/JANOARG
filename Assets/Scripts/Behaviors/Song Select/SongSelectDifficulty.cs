using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SongSelectDifficulty : MonoBehaviour
{
    public TMP_Text ChartDifficultyLabel;
    public TMP_Text ChartNameLabel;
    public Button Button;

    public Image CoverImage;
    public Image CoverBorder;
    public Image CoverStatusBorder;
    
    [NonSerialized]
    public ExternalChartMeta Chart;

    public void SetItem(ExternalChartMeta chart) 
    {
        ChartDifficultyLabel.text = chart.DifficultyLevel;
        ChartNameLabel.text = chart.DifficultyName;

        Chart = chart;
    }

    public void SetSelectability(float a)
    {
        rt(this).sizeDelta = new(Mathf.Lerp(42, 180, a), rt(this).sizeDelta.y);
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;
}