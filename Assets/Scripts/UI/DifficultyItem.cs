using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Globalization;

public class DifficultyItem : MonoBehaviour
{
    public ExternalChartMeta Chart;
    public Color Accent;
    [Space]
    public Image Background;
    public Image Border;
    public Button SelectButton;
    [Space]
    public RectTransform DifficultyNameGroup;
    public TMP_Text DifficultyNameText;
    public TMP_Text DifficultyRecordText;
    public TMP_Text DifficultyLevelText;
    [Space]
    public GameObject ScoreBarHolder;
    public List<Slider> ScoreBars;
    public TMP_Text RankText;

    RectTransform self;

    public void Awake()
    {
        self = GetComponent<RectTransform>();
    }

    public void SetChart(ExternalChartMeta meta)
    {
        DifficultyNameText.text = meta.DifficultyName;
        if (false)
        {
            /* TODO: Implement high score saving
            float score = Random.Range(800000, 1000000);
            DifficultyRecordText.text = "<b><i>FS</i></b> " + score.ToString("0000000", CultureInfo.InvariantCulture) + "<size=9>ppm";
            RankText.text = Helper.GetRank(score);
            score /= 1e6f;
            foreach (Slider bar in ScoreBars)
            {
                bar.value = score;
                score = score * 10 - 9;
            }
            */
        }
        else
        {
            ScoreBarHolder.SetActive(false);
        }

        DifficultyLevelText.text = meta.DifficultyLevel;
        if (DifficultyLevelText.text.EndsWith("*")) DifficultyLevelText.text = 
            DifficultyLevelText.text.Remove(DifficultyLevelText.text.Length - 1) + "<sup>*";

        Accent = PlaylistScroll.main.DifficultyColors[meta.DifficultyIndex + 1];
        Background.color = Accent;
        Chart = meta;
    }

    public void Select()
    {
        StopAllCoroutines();
        StartCoroutine(SelectAnim());
    }

    public IEnumerator SelectAnim()
    {
        SelectButton.interactable = false;

        void LerpSelection(float value)
        {
            float ease = Ease.Get(value, "Quartic", EaseMode.Out);
            self.sizeDelta = new Vector2(40 + 160 * ease, self.sizeDelta.y);
            DifficultyNameText.color = DifficultyRecordText.color = DifficultyLevelText.color = Color.Lerp(Color.black, Accent, ease);
            RankText.color = Accent * new Color(1, 1, 1, ease * .25f);
            DifficultyLevelText.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(40, DifficultyLevelText.preferredWidth, ease), 40);
            DifficultyNameGroup.sizeDelta = new Vector2(Mathf.Max(DifficultyNameText.preferredWidth, DifficultyRecordText.preferredWidth) * ease, 20);

            Border.pixelsPerUnitMultiplier = .5f * (1 - ease);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .3f)
        {
            LerpSelection(a);
            yield return null;
        }
        LerpSelection(1);

    }

    public void Deselect()
    {
        StopAllCoroutines();
        StartCoroutine(DeselectAnim());
    }

    public IEnumerator DeselectAnim()
    {
        SelectButton.interactable = true;

        void LerpSelection(float value)
        {
            float ease = Ease.Get(value, "Quartic", EaseMode.Out);
            self.sizeDelta = new Vector2(200 - 160 * ease, self.sizeDelta.y);
            DifficultyNameText.color = DifficultyRecordText.color = DifficultyLevelText.color = Color.Lerp(Accent, Color.black, ease);
            RankText.color = Accent * new Color(1, 1, 1, (1 - ease) * .25f);
            DifficultyLevelText.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(DifficultyLevelText.preferredWidth, 40, ease), 40);
            DifficultyNameGroup.sizeDelta = new Vector2(Mathf.Max(DifficultyNameText.preferredWidth, DifficultyRecordText.preferredWidth) * (1 - ease), 20);

            Border.pixelsPerUnitMultiplier = .5f * (value);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .3f)
        {
            LerpSelection(a);
            yield return null;
        }
        LerpSelection(1);

    }

    public void OnItemClick()
    {
        PlaylistScroll.main.SelectedDifficulty.Deselect();
        PlaylistScroll.main.SelectedDifficultyIndex = Chart.DifficultyIndex;
        PlaylistScroll.main.SelectedDifficulty = this;
        Select();
    }
}
