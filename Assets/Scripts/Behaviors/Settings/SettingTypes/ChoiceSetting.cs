using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceSetting : Setting
{
    public TMP_Dropdown Field;
    public int DefaultValue;
    [Space]
    public TMP_Text TextHolder;

    public void Start()
    {
        Field.value = Common.main.Storage.Get(ID, DefaultValue);
    }

    public void OnChange()
    {
        Common.main.Storage.Set(ID, Field.value);
    }

    public void SetPrevious()
    {
        Field.value = (Field.value + Field.options.Count - 1) % Field.options.Count;
        StopAllCoroutines();
        StartCoroutine(TextNudge(Vector2.left * 6));
    }

    public void SetNext()
    {
        Field.value = (Field.value + 1) % Field.options.Count;
        StopAllCoroutines();
        StartCoroutine(TextNudge(Vector2.right * 6));
    }

    public IEnumerator TextNudge(Vector2 pos)
    {
        void LerpText(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Cubic, EaseMode.Out);

            TextHolder.rectTransform.anchoredPosition = pos * (1 - ease);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .2f)
        {
            LerpText(a);
            yield return null;
        }
        LerpText(1);
    }
}
