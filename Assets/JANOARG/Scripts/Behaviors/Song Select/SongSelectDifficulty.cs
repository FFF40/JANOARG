using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SongSelectDifficulty : MonoBehaviour
{
    public TMP_Text ChartDifficultyLabel;
    public Button Button;
    public RectTransform Holder;

    public Image CoverImage;
    public Image CoverBorder;

    public GameObject FCIndicator;
    public GameObject APIndicator;
    public Graphic[] IndicatorGraphics;

    public Image[] ScoreDials;
    public Graphic[] ScoreGraphics;
    
    [NonSerialized]
    public ExternalChartMeta Chart;
    [NonSerialized]
    public ScoreStoreEntry Record;
    [NonSerialized]
    public Color Color;

    public void SetItem(ExternalChartMeta chart, ScoreStoreEntry record, Color color) 
    {
        ChartDifficultyLabel.text = chart.DifficultyLevel;

        Chart = chart;
        Record = record;
        Color = color;

        FCIndicator.SetActive(false);
        APIndicator.SetActive(false);
        if (record == null || record.BadCount > 0) {}
        else if (record.GoodCount > 0) FCIndicator.SetActive(true);
        else APIndicator.SetActive(true);

        float score = record?.Score ?? 0;
        foreach (var image in ScoreDials)
        {
            image.fillAmount = score / 1e6f;
            score = (score * 10) - 9e6f;
        }

        foreach (var grph in IndicatorGraphics) grph.color = color;

        SetSelectability(0);
    }

    public void SetSelectability(float a)
    {
        Color col = Color.Lerp(Color.black, Color, a);
        Color colInv = Color.Lerp(Color.black, Color, 1 - a);

        CoverImage.color = col;
        ChartDifficultyLabel.color = colInv;
        foreach (var grph in ScoreGraphics) grph.color = colInv * new Color(1, 1, 1, grph.color.a);
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;
}