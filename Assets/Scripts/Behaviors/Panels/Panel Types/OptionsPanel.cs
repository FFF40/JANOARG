using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class OptionsPanel : MonoBehaviour
{

    public int CurrentTab;
    [Space]
    public TMP_Text SubtitleLabel;
    [Space]
    public ScrollRect ContentScroller;
    public RectTransform ContentViewport;
    public CanvasGroup ContentGroup;
    [Space]
    public List<OptionsPanelTabButton> TabButtons;
    [Space]
    public RectTransform OptionItemHolder;
    public List<OptionItem> OptionItems;
    [Space]
    public GameObject AboutPane;
    [HideInInspector]
    public List<OptionItem> CurrentItems;

    public Panel CurrentPanel;

    [HideInInspector]
    public bool IsAnimating;

    void Start()
    {
        TabButtons[CurrentTab].SetFill(1);
        MakeTab(CurrentTab);
    }

    public void Close()
    {
        Common.main.Storage.Save();
        Common.main.Preferences.Save();
        CurrentPanel.Close();
    }

    public void SetTab(int tab)
    {
        if (!IsAnimating && tab != CurrentTab) StartCoroutine(SetTabAnim(tab));
    }

    public IEnumerator SetTabAnim(int tab)
    {
        IsAnimating = true;

        Vector2 basePos = ContentViewport.anchoredPosition;

        yield return Ease.Animate(.2f, x => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            TabButtons[CurrentTab].SetFill(1 - ease);
            SubtitleLabel.alpha = 1 - ease;
            SubtitleLabel.rectTransform.anchoredPosition = Vector2.left * 10 * ease;
            ContentViewport.anchoredPosition = basePos + Vector2.left * 10 * ease;
            ContentGroup.alpha = 1 - ease;
        });

        MakeTab(tab);
        CurrentTab = tab;

        yield return Ease.Animate(.2f, x => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            TabButtons[CurrentTab].SetFill(ease);
            SubtitleLabel.alpha = ease;
            SubtitleLabel.rectTransform.anchoredPosition = Vector2.left * 10 * (1 - ease);
            ContentViewport.anchoredPosition = basePos + Vector2.left * 10 * (1 - ease);
            ContentGroup.alpha = ease;
        });

        IsAnimating = false;
    }

    public void MakeTab(int tab) 
    {
        ClearAll();
        ContentScroller.content.anchoredPosition = Vector2.zero;
        ContentScroller.velocity = Vector2.zero;
        AboutPane.SetActive(false);

        Storage Storage = Common.main.Storage;
        Storage Preferences = Common.main.Preferences;

        switch (tab)
        {
            case 0:
            {
                SubtitleLabel.text = " > General";

                Spawn<OptionCategoryTitle>("Profile");

                GetOptionItemSample<StringOptionInput>().Limit = 16;
                Spawn<StringOptionInput, string>("Player Name", 
                    () => Storage.Get("INFO_Name", "JANOARG"),
                    x => Storage.Set("INFO_Name", x)
                );
            }
            break;

            case 1:
            {
                SubtitleLabel.text = " > Player";

                Spawn<OptionCategoryTitle>("Syncronization");

                FloatOptionInput sample = GetOptionItemSample<FloatOptionInput>();
                sample.Min = -500;
                sample.Max = 500;
                sample.Step = 1;
                sample.Unit = "ms";
                var judgOffset = Spawn<FloatOptionInput, float>("Judgment Offset", 
                    () => Preferences.Get("PLYR_JudgmentOffset", 0f),
                    x => Preferences.Set("PLYR_JudgmentOffset", x)
                );
                var visOffset = Spawn<FloatOptionInput, float>("Visual Offset", 
                    () => Preferences.Get("PLYR_VisualOffset", 0f),
                    x => Preferences.Set("PLYR_VisualOffset", x)
                );
            }
            break;

            case 2:
            {
                SubtitleLabel.text = " > About";

                AboutPane.SetActive(true);
                Spawn<OptionCategoryTitle>("LEAD PROGRAMMER / GAME DESIGNER");
                Spawn<OptionText>("duducat / ducdat0507");

                Spawn<OptionText>("");
                Spawn<OptionCategoryTitle>("SOUNDTRACK COMPOSERS (ORIGINAL TRACKS)");
                Spawn<OptionText>("Insert name of a famous artist here");

                Spawn<OptionText>("");
                Spawn<OptionCategoryTitle>("SOUNDTRACK COMPOSERS (LICENSED / FREE USE TRACKS)");
                Spawn<OptionText>("mrcool909090");
                Spawn<OptionText>("Sound Souler");
                
                Spawn<OptionText>("");
                Spawn<OptionCategoryTitle>("SOUNDTRACK COMPOSERS (UI BACKGROUND MUSIC)");
                Spawn<OptionText>("duducat");

                Spawn<OptionText>("");
                Spawn<OptionCategoryTitle>("COVER ARTISTS");
                Spawn<OptionText>(":blobcat:");

                Spawn<OptionText>("");
                Spawn<OptionCategoryTitle>("CHART DESIGNERS");
                Spawn<OptionText>("duducat");
            }
            break;
        }
    }

    public TType GetOptionItemSample<TType>() where TType : OptionItem 
    {
        foreach (OptionItem item in OptionItems) 
        {
            if (item is TType type) return type;
        }
        return null;
    }

    public TType Spawn<TType>(string title) where TType : OptionItem 
    {
        TType item = Instantiate(GetOptionItemSample<TType>(), OptionItemHolder);
        item.TitleLabel.text = title;
        CurrentItems.Add(item);
        return item;
    }

    public TType Spawn<TType, TObject>(string title, Func<TObject> get, Action<TObject> set) where TType : OptionInput<TObject>
    {
        TType item = Instantiate(GetOptionItemSample<TType>(), OptionItemHolder);
        item.TitleLabel.text = title;
        item.OnGet = get;
        item.OnSet = set;
        CurrentItems.Add(item);
        return item;
    }

    public void ClearAll() 
    {
        foreach (OptionItem item in CurrentItems) 
        {
            Destroy(item.gameObject);
        }
        CurrentItems.Clear();
    }
}
