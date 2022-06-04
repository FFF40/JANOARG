using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharterSettings : EditorWindow
{
    public static CharterKeybinds Keybinds;

    public static void InitSettings ()
    {
        if (Keybinds == null)
        {
            Keybinds = new CharterKeybinds();
        }
    }

    [MenuItem("J.A.N.O.A.R.G./Charter Settings", false, 1)]
    public static void Open()
    {
        CharterSettings wnd = GetWindow<CharterSettings>();
        wnd.titleContent = new GUIContent("Charter Settings");
        wnd.minSize = new Vector2(720, 400);
    }
    public static void Open(int tab)
    {
        CharterSettings wnd = GetWindow<CharterSettings>();
        wnd.titleContent = new GUIContent("Charter Settings");
        wnd.minSize = new Vector2(720, 400);
        wnd.currentTab = tab;
    }

    Vector2 scrollPos = Vector2.zero;

    int currentTab = 0;

    void OnGUI () 
    {
        CharterSettings.InitSettings();

        GUIStyle title = new GUIStyle("label");
        title.fontSize = 20;
        title.fontStyle = FontStyle.Bold;

        GUIStyle title2 = new GUIStyle("label");
        title2.fontSize = 16;
        title2.fontStyle = FontStyle.Bold;

        GUIStyle tab = new GUIStyle("toolbarButton");
        tab.padding = new RectOffset(12, 12, 0, 2);
        tab.fixedHeight = 32;
        tab.fixedWidth = 150;
        tab.fontSize = 14;
        tab.alignment = TextAnchor.MiddleLeft;

        GUIStyle bg = new GUIStyle("toolbarButton");
        bg.fixedHeight = 0;
        
        EditorGUILayout.BeginHorizontal();

        GUI.Label(new Rect(0, 0, 149, Screen.height), "", bg);
        EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
        if (GUILayout.Toggle(currentTab == 0, "Preferences", tab)) currentTab = 0;
        if (GUILayout.Toggle(currentTab == 1, "Keybindings", tab)) currentTab = 1;
        EditorGUILayout.EndScrollView();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Space(4);
        if (currentTab == 0)
        {
            GUILayout.Label("Preferences", title);
            GUILayout.Space(8);
            GUILayout.Label("Coming soon...");
            GUILayout.Label("(I need to figure out how to store data in the editor first)");
        }
        else if (currentTab == 1)
        {
            GUILayout.Label("Keybindings", title, GUILayout.MinWidth(Screen.width - 180));
            string curCat = "";
            
            int rows = Mathf.Max((int)(Screen.width - 180) / 220, 1);
            int crow = 0;
            float w = (Screen.width - 180) / rows;
            float[] hs = new float[rows];

            foreach (KeyValuePair<string, Keybind> kb in Keybinds.Values)
            {
                string cat = kb.Key.Remove(kb.Key.IndexOf("/"));
                string name = kb.Key.Substring(kb.Key.IndexOf("/") + 1);

                if (curCat != cat) 
                {
                    crow = 0;
                    for (int a = 1; a < rows; a++) if (hs[a] < hs[crow]) crow = a;
                    GUI.Label(new Rect(w * crow + 4, hs[crow] + 32, w - 5, 24), cat, title2);
                    hs[crow] += 32;
                    curCat = cat;
                }

                GUI.Label(new Rect(w * crow + 4, hs[crow] + 25, 120, 20), name);
                GUI.Button(new Rect(w * crow + 130, hs[crow] + 24, w - 132, 20), kb.Value.ToString(), EditorStyles.textField);
                hs[crow] += 22;
            }
            
            crow = 0;
            for (int a = 1; a < rows; a++) if (hs[a] > hs[crow]) crow = a;
            GUILayout.Space(hs[crow]);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
    }
}

[System.Serializable]
public class Keybind
{
    public KeyCode KeyCode = KeyCode.Space;
    public EventModifiers Modifiers = EventModifiers.None;

    public Keybind (KeyCode keyCode, EventModifiers modifiers)
    {
        KeyCode = keyCode;
        Modifiers = modifiers;
    }

    public bool Matches(Event ev)
    {
        EventModifiers cas = (EventModifiers.Shift | EventModifiers.Alt | EventModifiers.Control);
        return ev.keyCode == KeyCode && (ev.modifiers & cas) == Modifiers;
    }

    public static bool operator == (Event ev, Keybind keybind)
    {
        return keybind.Matches(ev);
    }

    public static bool operator != (Event ev, Keybind keybind)
    {
        return !keybind.Matches(ev);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString() 
    {
        string str = KeyCode.ToString();
        switch (str)
        {
            case "Slash": str = "/"; break;
            case "Backslash": str = "\\"; break;
            case "UpArrow": str = "↑"; break;
            case "DownArrow": str = "↓"; break;
            case "LeftArrow": str = "←"; break;
            case "RightArrow": str = "→"; break;
        }

        if ((Modifiers & EventModifiers.Shift) > 0) str = "Shift+" + str;
        if ((Modifiers & EventModifiers.Alt) > 0) str = "Alt+" + str;
        if ((Modifiers & EventModifiers.Control) > 0) str = "Ctrl+" + str;
        return str;
    }
}

[System.Serializable]
public class CharterKeybinds
{
    public Dictionary<string, Keybind> Values;

    public Keybind this[string index]
    {
        get {
            return Values[index]; 
        }
    }

    public CharterKeybinds()
    {
        Values = new Dictionary<string, Keybind>();

        Values["General/Toggle Play/Pause"] = new Keybind(KeyCode.P, EventModifiers.None);

        Values["Picker/Cursor"] = new Keybind(KeyCode.A, EventModifiers.None);
        Values["Picker/Select"] = new Keybind(KeyCode.S, EventModifiers.None);
        Values["Picker/Delete"] = new Keybind(KeyCode.D, EventModifiers.None);

        Values["Selection/Previous Item"] = new Keybind(KeyCode.LeftArrow, EventModifiers.None);
        Values["Selection/Next Item"] = new Keybind(KeyCode.RightArrow, EventModifiers.None);
        Values["Selection/Previous Lane"] = new Keybind(KeyCode.LeftArrow, EventModifiers.Shift);
        Values["Selection/Next Lane"] = new Keybind(KeyCode.RightArrow, EventModifiers.Shift);

        Values["Misc./Show Keybindings"] = new Keybind(KeyCode.Slash, EventModifiers.Shift);
    }
}