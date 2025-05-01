using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabbedSidebar : Sidebar
{

    public ScrollRect ScrollView;
    public RectTransform ScrollHolder;

    public List<Tab> Tabs;
    public CanvasGroup TabGroup;

    public TabButton TabButtonSample;
    public RectTransform TabButtonHolder;
    public VerticalLayoutGroup TabButtonGroup;
    public RectTransform TabButtonIndicator;

    public TMP_Text TabLabel;

    public int CurrentTab;
    void Awake()
    {
        for (int x = 0; x < Tabs.Count; x++)
        {
            Tab tab = Tabs[x];
            tab.Target.gameObject.SetActive(false);
            TabButton tb = Instantiate(TabButtonSample, TabButtonHolder);
            tb.Text.text = tab.Name;
            tb.Icon.sprite = tab.Icon;
            int a = x;
            tb.Button.onClick.AddListener(() => SetTabAnimate(a));
            Tabs[x].Button = tb;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(TabButtonHolder);
    }

    void Start()
    {
        SetTab(CurrentTab);
    }

    void Update()
    {
        
    }

    public void SetTab(int index)
    {
        Tabs[CurrentTab].Target.gameObject.SetActive(false);
        CurrentTab = index;
        Tabs[CurrentTab].Target.gameObject.SetActive(true);
        ScrollView.content = Tabs[CurrentTab].Target;
        TabButtonIndicator.anchoredPosition = Tabs[CurrentTab].Button.GetComponent<RectTransform>().anchoredPosition;
        TabLabel.text = " > " + Tabs[CurrentTab].Name;
    }

    public void SetTabAnimate(int index)
    {
        if (!isAnimating && index != CurrentTab) StartCoroutine(TabAnimation(index));
    }

    public IEnumerator TabAnimation(int index)
    {
        isAnimating = true;

        int oldIndex = CurrentTab;
        RectTransform oldRT = Tabs[oldIndex].Button.GetComponent<RectTransform>();
        RectTransform newRT = Tabs[index].Button.GetComponent<RectTransform>();
        Vector2 sizeDelta = TabButtonIndicator.sizeDelta;

        void LerpIndicator(float value)
        {
            float ease = Ease.Get(value / .75f, EaseFunction.Cubic, EaseMode.Out);
            float ease2 = Ease.Get((value - .25f) / .75f, EaseFunction.Cubic, EaseMode.Out);
            
            float min = Mathf.Lerp(oldRT.anchoredPosition.y, newRT.anchoredPosition.y, oldIndex > index ? ease2 : ease);
            float max = Mathf.Lerp(oldRT.anchoredPosition.y, newRT.anchoredPosition.y, oldIndex < index ? ease2 : ease);

            TabButtonIndicator.anchoredPosition = Vector2.up * min;
            TabButtonIndicator.sizeDelta = sizeDelta + Vector2.up * (max - min);
        }
        void LerpContent(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Cubic, EaseMode.Out);

            TabGroup.alpha = value;
            TabLabel.alpha = value / 2;
            ScrollHolder.anchoredPosition = Vector3.left * 50 * (1 - ease);
        }

        bool isSet = false;
        
        for (float a = 0; a < 1; a += Time.deltaTime / .4f) 
        {
            if (a >= .25f && !isSet)
            {
                SetTab(index);
                isSet = true;
            }

            LerpIndicator(a);
            LerpContent(a < .25f ? 1 - a * 4 : (a - .25f) / .75f);
            yield return null;
        }
        LerpIndicator(1);
        LerpContent(1);


        isAnimating = false;
    }
}

[System.Serializable]
public class Tab
{
    public string Name;
    public Sprite Icon;
    public RectTransform Target;
    [HideInInspector]
    public TabButton Button;
}
