using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreferencesModal : Modal
{
    public static PreferencesModal main;

    public RectTransform FormHolder;
    public Button[] TabButtons;

    public bool IsDirty = false;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    public void OnDestroy()
    {
        if (IsDirty) Chartmaker.main.StartSavePrefsRoutine();
    }

    public new void Start()
    {
        base.Start();
        SetTab(0);
    }

    public void SetTab(int tab)
    {
        for (int a = 0; a < TabButtons.Length; a++) 
        {
            TabButtons[a].interactable = tab != a;
        }

        ClearForm();

        if (tab == 0)
        {
            var prefs = Chartmaker.Preferences;
            var storage = Chartmaker.PreferencesStorage;
            
            SpawnForm<FormEntryHeader>("Player");
            SpawnForm<FormEntryBool, bool>("Maximize on Play", () => prefs.MaximizeOnPlay, x => {
                storage.Set("PL:MaximizeOnPlay", prefs.MaximizeOnPlay = x); IsDirty = true;
            });

            SpawnForm<FormEntryHeader>("Auto-Save");
            SpawnForm<FormEntryBool, bool>("Save on Play", () => prefs.SaveOnPlay, x => {
                storage.Set("AS:SaveOnPlay", prefs.SaveOnPlay = x); IsDirty = true;
            });
            SpawnForm<FormEntryBool, bool>("Save on Quit", () => prefs.SaveOnQuit, x => {
                storage.Set("AS:SaveOnQuit", prefs.SaveOnQuit = x); IsDirty = true;
            });
        }
        else if (tab == 1)
        {
            var categories = KeyboardHandler.main.Keybindings.MakeCategoryGroups();
            foreach (var cat in categories)
            {
                SpawnForm<FormEntryHeader>(cat.Key);
                foreach (var entry in cat.Value)
                {
                    var field = SpawnForm<FormEntryKeybind, Keybind>(entry.Value.Name, () => entry.Value.Keybind, x => {
                        Chartmaker.main.KeybindingsStorage.Set(entry.Key, (entry.Value.Keybind = x).ToSaveString());
                        IsDirty = true;
                    });
                    field.Category = cat.Key;
                }
            }
        }
        else if (tab == 2)
        {
            var prefs = Chartmaker.Preferences;
            var storage = Chartmaker.PreferencesStorage;

            SpawnForm<FormEntryHeader>("Theme");
            var themeDropdown = SpawnForm<FormEntryDropdown, object>("", () => prefs.Theme, x => {
                if (prefs.Theme != x.ToString()) 
                {
                    storage.Set("AP:Theme", prefs.Theme = x.ToString()); IsDirty = true;
                    Themer.main.InitTheme(); TimelinePanel.main.UpdateTimeline(true);
                }
            });
            themeDropdown.ValidValues.Add("Prototype", "Prototype (default)");
            themeDropdown.ValidValues.Add("PastelDay", "Pastelland - Day");
            themeDropdown.ValidValues.Add("PastelNight", "Pastelland - Night");
            themeDropdown.ValidValues.Add("SpaceChrome", "Spaceware - Chrome");
            themeDropdown.ValidValues.Add("Hyperpop", "Hyperpop");
            themeDropdown.TitleLabel.gameObject.SetActive(false);
            themeDropdown.GetComponent<HorizontalLayoutGroup>().padding.left = 10;
            
            SpawnForm<FormEntryHeader>("Layout");
            SpawnForm<FormEntryBool, bool>("Default Window Frame", () => prefs.UseDefaultWindow, x => {
                storage.Set("LA:UseDefaultWindow", prefs.UseDefaultWindow = x); IsDirty = true;
                #if !UNITY_EDITOR && UNITY_STANDALONE_WIN 
                    if (x) 
                    {
                        BorderlessWindow.SetFramedWindow();
                        BorderlessWindow.ResizeWindowDelta(2, 1);
                        BorderlessWindow.MoveWindowDelta(new(-1, 0));
                    }
                    else 
                    {
                        BorderlessWindow.SetFramelessWindow();
                        BorderlessWindow.ResizeWindowDelta(-2, -1);
                        BorderlessWindow.MoveWindowDelta(new(1, 0));
                    }
                #endif
            });
        }
    }
    
    public void ClearForm()
    {
        foreach (RectTransform rt in FormHolder)
        {
            Destroy(rt.gameObject);
        }
    }

    T SpawnForm<T>(string title = "") where T : FormEntry
        => Formmaker.main.Spawn<T>(FormHolder, title);

    T SpawnForm<T, U>(string title, Func<U> get, Action<U> set) where T : FormEntry<U>
        => Formmaker.main.Spawn<T, U>(FormHolder, title, get, set);
}
