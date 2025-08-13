using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollingCounterDigit : MonoBehaviour
{
    public TMP_Text TopLabel;
    public TMP_Text BottomLabel;

    public int MaxLength = 20;
    public List<string> List { get; private set; } = new();

    public string CurrentDigit;
    public float Progress;
    public float Speed = 9;

    public void Start() 
    {
        SetDigit("0");
    }

    public void Update()
    {
        if (Progress > 0.001f || List.Count > 0) 
        {
            Progress -= (List.Count + Progress) * (1 - Mathf.Pow(0.1f, Time.deltaTime * Speed));
            while (Progress < 0) 
            {
                TopLabel.text = BottomLabel.text;
                BottomLabel.text = List[0];
                List.RemoveAt(0);
                Progress++;
            }
            RectTransform rt = this.transform as RectTransform;
            Vector2 skew = new (.26795f, 1);
            BottomLabel.rectTransform.anchoredPosition = skew * Progress * rt.rect.height;
            TopLabel.rectTransform.anchoredPosition = skew * (Progress - 1) * rt.rect.height;
        }
    }

    public void SetDigit(string str) 
    {
        CurrentDigit = str;
        if (List.Count >= MaxLength) List[^1] = str;
        else List.Add(str);
    }
}