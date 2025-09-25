using System;
using JANOARG.Client.Data.Storage;
using JANOARG.Shared.Data.ChartInfo;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.SongSelect
{
    public class SongSelectDifficulty : MonoBehaviour
    {
        public TMP_Text      ChartDifficultyLabel;
        public Button        Button;
        public RectTransform Holder;

        public Image CoverImage;
        public Image CoverBorder;

        [FormerlySerializedAs("FCIndicator")]public GameObject FullStreakIndicator;
        
        [FormerlySerializedAs("APIndicator")]public GameObject AllFlawlessIndicator;
        public Graphic[]  IndicatorGraphics;

        public Image[]   ScoreDials;
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

            FullStreakIndicator.SetActive(false);
            AllFlawlessIndicator.SetActive(false);

            if (record == null || record.BadCount > 0)
            {
            }
            else if (record.GoodCount > 0)
            {
                FullStreakIndicator.SetActive(true);
            }
            else
            {
                AllFlawlessIndicator.SetActive(true);
            }

            float score = record?.Score ?? 0;

            foreach (Image image in ScoreDials)
            {
                image.fillAmount = score / 1e6f;
                score = score * 10 - 9e6f;
            }

            foreach (Graphic graphic in IndicatorGraphics) graphic.color = color;

            SetSelectability(0);
        }

        public void SetSelectability(float a)
        {
            Color col = Color.Lerp(Color.black, Color, a);
            Color colInv = Color.Lerp(Color.black, Color, 1 - a);

            CoverImage.color = col;
            ChartDifficultyLabel.color = colInv;
            foreach (Graphic grph in ScoreGraphics) grph.color = colInv * new Color(1, 1, 1, grph.color.a);
        }

        private RectTransform RT(Component obj)
        {
            return obj.transform as RectTransform;
        }
    }
}