using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TutorialModal : Modal
{
    public static TutorialModal main;

    public Tutorial CurrentTutorial { get; private set; }
    public int CurrentStep { get; private set; }

    public TMP_Text TitleLabel;

    [Space]
    public GameObject ListSectionHolder;
    public TutorialEntry EntrySample;
    public RectTransform EntryHolder;

    [Space] 
    public GameObject TutorialSectionHolder;
    public Slider TutorialProgress;
    public TMP_Text ContentLabel;
    public GameObject NextButton;
    public TMP_Text NextConditionLabel;
    public GameObject LastStepActionsHolder;
    public GameObject NextTutorialButton;
    public TMP_Text NextTutorialLabel;

    [Space] 
    public RectTransform CurrentFocusItem;
    public RectTransform FocusIndicator;
    public Graphic FocusIndicator2;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }
    
    public new void Start()
    {
        base.Start();
        transform.SetParent(ModalHolder.main.PriorityModalHolder);
        FocusIndicator.SetParent(transform.parent);
        foreach (Tutorial tut in Tutorials.tutorials) 
        {
            TutorialEntry entry = Instantiate(EntrySample, EntryHolder);
            entry.Label.text = tut.Name;
            entry.Button.onClick.AddListener(() => StartTutorial(tut));
        }
        UpdateUI();
    }

    public void OnDestroy() 
    {
        Destroy(FocusIndicator.gameObject);
    }

    public void Update() {
        var condition = CurrentTutorial?.Steps[CurrentStep].NextCondition;
        if (condition != null && condition()) NextStep();

        float lerp = 1 - Mathf.Pow(0.01f, Time.deltaTime);
        TutorialProgress.value = Mathf.Lerp(TutorialProgress.value, CurrentStep, lerp);

        FocusIndicator.gameObject.SetActive(CurrentFocusItem);
        if (CurrentFocusItem) 
        {
            Vector3[] corners = new Vector3[4];
            CurrentFocusItem.GetWorldCorners(corners);
            Vector2 center = (corners[0] + corners[2]) / 2;
            Vector2 size = corners[2] - corners[0];

            FocusIndicator.position = Vector2.Lerp(FocusIndicator.position, center, lerp);
            FocusIndicator.sizeDelta = Vector2.Lerp(FocusIndicator.sizeDelta, size + new Vector2(4, 4), lerp);
            float lerp2 = 1 - Mathf.Pow(0.02f, Time.deltaTime);
            FocusIndicator2.rectTransform.position = Vector2.Lerp(FocusIndicator2.rectTransform.position, center, lerp2);
            FocusIndicator2.rectTransform.sizeDelta = Vector2.Lerp(FocusIndicator2.rectTransform.sizeDelta, size + new Vector2(8, 8), lerp2);
            FocusIndicator2.color = Color.HSVToRGB(Time.time % 1, 1, 1);
        }
    }

    public void UpdateUI() 
    {
        bool isActive = CurrentTutorial != null;
        ListSectionHolder.SetActive(!isActive);
        TutorialSectionHolder.SetActive(isActive);
        TitleLabel.text = isActive 
            ? CurrentTutorial.Name 
            : "Tutorials";
        CurrentFocusItem = null;
        if (isActive)
        {
            TutorialStep step = CurrentTutorial.Steps[CurrentStep];
            bool isLastStep = CurrentStep >= CurrentTutorial.Steps.Length - 1;
            TutorialProgress.maxValue = CurrentTutorial.Steps.Length - 1;
            ContentLabel.text = step.Content;
            NextButton.gameObject.SetActive(step.NextCondition == null && !isLastStep);
            NextConditionLabel.gameObject.SetActive(step.NextCondition != null);
            NextConditionLabel.text = step.NextConditionLabel;
            LastStepActionsHolder.SetActive(isLastStep);

            if (!string.IsNullOrWhiteSpace(step.FocusItemPath)) 
            {
                string[] paths = step.FocusItemPath.Split("->");
                GameObject obj = GameObject.Find(paths[0]);
                CurrentFocusItem = obj ? (RectTransform)obj.transform : null;
                for (int i = 1; i < paths.Length; i++) 
                {
                    CurrentFocusItem = (RectTransform)FindRecursive(CurrentFocusItem, paths[i]);
                    if (!CurrentFocusItem) break;
                }
                FocusIndicator2.rectTransform.position = FocusIndicator.position = new Vector2(Screen.width, Screen.height) / 2;
                FocusIndicator2.rectTransform.sizeDelta = FocusIndicator.sizeDelta = new Vector2(Screen.width, Screen.height);
            }
            Debug.Log(CurrentFocusItem);

            if (isLastStep) 
            {
                int index = Array.IndexOf(Tutorials.tutorials, CurrentTutorial);
                bool isLastTutorial = index + 1 >= Tutorials.tutorials.Length;

                NextTutorialButton.SetActive(true);
                if (isLastTutorial) NextTutorialLabel.text = "Close Tutorial";
                else NextTutorialLabel.text = "Continue to " + Tutorials.tutorials[index + 1].Name;
            }
        }
    }

    public void StartTutorial(Tutorial tutorial)
    {
        if (tutorial.Checker(() => StartTutorial(tutorial)))
        {
            CurrentTutorial = tutorial;
            TutorialProgress.value = CurrentStep = 0;
            UpdateUI();
        }
    }

    public void StartNextTutorial()
    {
        int index = Array.IndexOf(Tutorials.tutorials, CurrentTutorial);
        if (index + 1 >= Tutorials.tutorials.Length) { Close(); return; }
        StartTutorial(Tutorials.tutorials[index + 1]);
    }

    public void NextStep() 
    {
        CurrentStep++;
        base.Start();
        UpdateUI();
    }

    Transform FindRecursive(Transform parent, string name)
    {
        if (parent == null) return null;

        Transform res = parent.Find(name);
        if (res) return res;

        foreach (Transform child in parent) 
        {
            res = FindRecursive(child, name);
            if (res) return res;
        }

        return null;
    }
}

public class Tutorial
{
    public string Name;
    public Func<Action, bool> Checker;
    public TutorialStep[] Steps;

    public Tutorial (string name, Func<Action, bool> checker, TutorialStep[] steps)
    {
        Name = name;
        Checker = checker;
        Steps = steps;
    }
}

public class TutorialStep
{
    public string Content;
    public string FocusItemPath;
    public Func<bool> NextCondition;
    public string NextConditionLabel;

    public TutorialStep (string content)
    {
        Content = content;
    }

    public TutorialStep (string content, Func<bool> nextCon, string nextLabel) 
    {
        Content = content;
        NextCondition = nextCon;
        NextConditionLabel = nextLabel;
    }

    public TutorialStep (string content, string focusPath)
    {
        Content = content;
        FocusItemPath = focusPath;
    }

    public TutorialStep (string content, string focusPath, Func<bool> nextCon, string nextLabel) 
    {
        Content = content;
        FocusItemPath = focusPath;
        NextCondition = nextCon;
        NextConditionLabel = nextLabel;
    }
}