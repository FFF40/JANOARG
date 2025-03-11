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
    public Image CoverStatusBorder;

    public GameObject FCIndicator;
    public GameObject APIndicator;
    
    [NonSerialized]
    public ExternalChartMeta Chart;
    [NonSerialized]
    public ScoreStoreEntry Record;

    public void SetItem(ExternalChartMeta chart, ScoreStoreEntry record) 
    {
        ChartDifficultyLabel.text = chart.DifficultyLevel;

        Chart = chart;
        Record = record;

        FCIndicator.SetActive(false);
        APIndicator.SetActive(false);
        if (record == null || record.BadCount > 0) {}
        else if (record.GoodCount > 0) FCIndicator.SetActive(true);
        else APIndicator.SetActive(true);
    }

    public void SetSelectability(float a)
    {
        Holder.localPosition = new(Holder.localPosition.x, 5 * a);
        Holder.localEulerAngles = 15 * a * Vector3.back;
    }

    RectTransform rt (Component obj) => obj.transform as RectTransform;
}