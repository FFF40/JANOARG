using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.UI
{
    public class ScrollingCounterDigit : MonoBehaviour
    {
        public TMP_Text TopLabel;
        public TMP_Text BottomLabel;

        public int MaxLength = 20;

        public string CurrentDigit;
        public float  Progress;
        public float  Speed = 9;
        public List<string> list { get; } = new();

        public void Start()
        {
            SetDigit("0");
        }

        public void Update()
        {
            if (Progress > 0.001f || list.Count > 0)
            {
                Progress -= (list.Count + Progress) * (1 - Mathf.Pow(0.1f, Time.deltaTime * Speed));

                while (Progress < 0)
                {
                    TopLabel.text = BottomLabel.text;
                    BottomLabel.text = list[0];
                    list.RemoveAt(0);
                    Progress++;
                }

                var rt = transform as RectTransform;
                Vector2 skew = new(.26795f, 1);
                BottomLabel.rectTransform.anchoredPosition = skew * (Progress * rt!.rect.height);
                TopLabel.rectTransform.anchoredPosition = skew * ((Progress - 1) * rt.rect.height);
            }
        }

        public void SetDigit(string str)
        {
            CurrentDigit = str;

            if (list.Count >= MaxLength) list[^1] = str;
            else list.Add(str);
        }
    }
}