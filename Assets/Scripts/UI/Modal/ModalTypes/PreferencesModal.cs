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
    public int CurrentTab {get; private set;} = -1;

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
        if (CurrentTab < 0) SetTab(0);
    }

    public void SetTab(int tab)
    {
        CurrentTab = tab;
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

            SpawnForm<FormEntryHeader>("Updates");
            SpawnForm<FormEntryBool, bool>("Auto-Check on Start", () => prefs.AutoUpdateCheck, x => {
                storage.Set("UP:AutoUpdateCheck", prefs.AutoUpdateCheck = x); IsDirty = true;
            });
            
        }
        else if (tab == 1)
        {
            SpawnForm<FormEntrySpace>("");
            SpawnForm<FormEntryLabel>("Click on the button associated with a keybind, then press a key or key combination on your keyboard to change that keybind.");

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

            SpawnForm<FormEntryHeader>("Appearance");
            var themeDropdown = SpawnForm<FormEntryDropdown, object>("Theme", () => prefs.Theme, x => {
                if (prefs.Theme != x.ToString()) 
                {
                    storage.Set("AP:Theme", prefs.Theme = x.ToString()); IsDirty = true;
                    Themer.main.InitTheme(); if (Chartmaker.main.CurrentSong != null) TimelinePanel.main.UpdateTimeline(true);
                }
            });
            themeDropdown.ValidValues.Add("Prototype", "Prototype (default)");
            themeDropdown.ValidValues.Add("PastelDay", "Pastelland - Day");
            themeDropdown.ValidValues.Add("PastelNight", "Pastelland - Night");
            themeDropdown.ValidValues.Add("SpaceChrome", "Spaceware - Chrome");
            themeDropdown.ValidValues.Add("Hyperpop", "Hyperpop");

            SpawnForm<FormEntrySpace>("");
            
            #if UNITY_STANDALONE_WIN
                var cursorDropdown = SpawnForm<FormEntryDropdown, object>("Cursor Mode", () => prefs.CustomCursors, x => {
                    storage.Set("AP:CustomCursors", prefs.CustomCursors = (bool)x); IsDirty = true;
                    if (CursorChanger.Cursors.Count > 0) CursorChanger.PopCursor(); 
                    CursorChanger.PushCursor(CursorType.Arrow); BorderlessWindow.UpdateCursor();
                });
                cursorDropdown.ValidValues.Add(false, "Native");
                cursorDropdown.ValidValues.Add(true, "Custom");
            #endif
            
            SpawnForm<FormEntryHeader>("Layout");

            #if UNITY_STANDALONE_WIN
                var windowDropdown = SpawnForm<FormEntryDropdown, object>("Window Frame Mode", () => prefs.UseDefaultWindow, x => {
                    bool y = prefs.UseDefaultWindow;
                    storage.Set("LA:UseDefaultWindow", prefs.UseDefaultWindow = (bool)x); IsDirty = true;
                    #if !UNITY_EDITOR && UNITY_STANDALONE_WIN 
                        if ((bool)x != y)
                        {
                            if ((bool)x) 
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
                        }
                    #endif
                });
                windowDropdown.ValidValues.Add(true, "Native");
                windowDropdown.ValidValues.Add(false, "Custom");
            #endif
        }
        else if (tab == 3)
        {
            var prefs = Chartmaker.Preferences;
            var storage = Chartmaker.PreferencesStorage;
            
            SpawnForm<FormEntryHeader>("Fun<i>!</i> :D");
            SpawnForm<FormEntryBool, bool>("More Perfect Hitsounds", () => prefs.PerfectHitsounds, x => {
                storage.Set("BO:PerfectHitsounds", prefs.PerfectHitsounds = x); IsDirty = true;
            });
        }
        else if (tab == 4)
        {
            var prefs = Chartmaker.Preferences;
            var storage = Chartmaker.PreferencesStorage;

            SpawnForm<FormEntryHeader>("Display");
            SpawnForm<FormEntryBool, bool>("Vertical Sync", () => QualitySettings.vSyncCount > 0, x => {
                QualitySettings.vSyncCount = x ? 1 : 0; IsDirty = true;
            });

            SpawnForm<FormEntryHeader>("Quality");
            var cursorDropdown = SpawnForm<FormEntryDropdown, object>("Anti-Aliasing", () => QualitySettings.antiAliasing, x => {
                QualitySettings.antiAliasing = (int)x; IsDirty = true;
            });
            cursorDropdown.ValidValues.Add(0, "Disabled");
            cursorDropdown.ValidValues.Add(2, "2x MSAA");
            cursorDropdown.ValidValues.Add(4, "4x MSAA");
            cursorDropdown.ValidValues.Add(8, "8x MSAA");
        }
        else if (tab == 5)
        {
            var prefs = Chartmaker.Preferences;
            var storage = Chartmaker.PreferencesStorage;

            SpawnForm<FormEntryHeader>("FFT");
            var windowDropdown = SpawnForm<FormEntryDropdown, object>("Window Function", () => prefs.FFTWindow, x => {
                storage.Set("AL:FFTWindow", prefs.FFTWindow = (FFTWindow)x); IsDirty = true;
                if (TimelinePanel.main.Options.WaveformMode >= 2) TimelinePanel.main.DiscardWaveform();
            });
            windowDropdown.ValidValues.Add(FFTWindow.Rectangular, "Rectangular");
            windowDropdown.ValidValues.Add(FFTWindow.Triangular, "Triangular");
            windowDropdown.ValidValues.Add(FFTWindow.Hamming, "Hamming");
            windowDropdown.ValidValues.Add(FFTWindow.Hann, "Hann");
            windowDropdown.ValidValues.Add(FFTWindow.Blackman, "Blackman");
            windowDropdown.ValidValues.Add(FFTWindow.BlackmanNuttal, "Blackman-Nuttal");
            windowDropdown.ValidValues.Add(FFTWindow.BlackmanHarris, "Blackman-Harris");
            windowDropdown.ValidValues.Add(FFTWindow.FlatTop, "Flat top");

            SpawnForm<FormEntryHeader>("Spectrogram");
            SpawnForm<FormEntryDropdown, object>("Frequency Scale", () => prefs.FrequencyScale, x => {
                storage.Set("AL:FrequencyScale", prefs.FrequencyScale = (FrequencyScale)x); IsDirty = true;
                if (TimelinePanel.main.Options.WaveformMode == 2) TimelinePanel.main.DiscardWaveform();
            }).TargetEnum(typeof(FrequencyScale));

            SpawnForm<FormEntryVector2, Vector2>("Frequency Range", () => new (prefs.FrequencyMin, prefs.FrequencyMax), x => {
                if (prefs.FrequencyMin != x[0]) prefs.FrequencyMin = Math.Clamp(x[0], 0, prefs.FrequencyMax - 1);
                else prefs.FrequencyMax = Math.Clamp(x[1], prefs.FrequencyMin + 1, 24000);
                IsDirty = true;
                if (TimelinePanel.main.Options.WaveformMode == 2) TimelinePanel.main.DiscardWaveform();
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
