using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class JAEditorSettings : EditorWindow
{
    public static Keybinds ChartmakerKeybinds;

    public static void InitSettings ()
    {
        if (ChartmakerKeybinds == null)
        {
            ChartmakerKeybinds = new Keybinds(new Dictionary<string, Keybind> {
                { "General/Toggle Play/Pause", new Keybind(KeyCode.P) },
                { "General/Play Chart in Player", new Keybind(KeyCode.P, EventModifiers.Shift) },

                { "File/Save", new Keybind(KeyCode.S, EventModifiers.Command) },

                { "Edit/Undo", new Keybind(KeyCode.Z, EventModifiers.Command) },
                { "Edit/Redo", new Keybind(KeyCode.Y, EventModifiers.Command) },
                { "Edit/Cut", new Keybind(KeyCode.X, EventModifiers.Command) },
                { "Edit/Copy", new Keybind(KeyCode.C, EventModifiers.Command) },
                { "Edit/Paste", new Keybind(KeyCode.V, EventModifiers.Command) },
                { "Edit/Delete", new Keybind(KeyCode.Backspace, EventModifiers.None) },

                { "Picker/Cursor", new Keybind(KeyCode.A) },
                { "Picker/Select", new Keybind(KeyCode.S) },
                { "Picker/Delete", new Keybind(KeyCode.D) },
                { "Picker/1st Item", new Keybind(KeyCode.Q) },
                { "Picker/2nd Item", new Keybind(KeyCode.W) },

                { "Selection/Previous Item", new Keybind(KeyCode.LeftArrow) },
                { "Selection/Next Item", new Keybind(KeyCode.RightArrow) },
                { "Selection/Previous Lane", new Keybind(KeyCode.LeftArrow, EventModifiers.Shift) },
                { "Selection/Next Lane", new Keybind(KeyCode.RightArrow, EventModifiers.Shift) },

                { "Misc./Show Keybindings", new Keybind(KeyCode.Slash, EventModifiers.Shift) },
            });
        }
    }

    [MenuItem("JANOARG/Editor Settings", false, 100)]
    public static void Open()
    {
        JAEditorSettings wnd = GetWindow<JAEditorSettings>();
        wnd.titleContent = new GUIContent("Editor Settings");
        wnd.minSize = new Vector2(720, 400);
    }
    public static void Open(int tab)
    {
        JAEditorSettings wnd = GetWindow<JAEditorSettings>();
        wnd.titleContent = new GUIContent("Editor Settings");
        wnd.minSize = new Vector2(720, 400);
        wnd.currentTab = tab;
    }

    Vector2 tabScrollPos = Vector2.zero;
    Vector2 scrollPos = Vector2.zero;

    int currentTab = 0;

    void OnGUI () 
    {
        JAEditorSettings.InitSettings();

        GUIStyle title = new GUIStyle("label");
        title.fontSize = 20;
        title.fontStyle = FontStyle.Bold;

        GUIStyle title2 = new GUIStyle("label");
        title2.fontSize = 16;
        title2.fontStyle = FontStyle.Bold;

        GUIStyle tab = new GUIStyle("toolbarButton");
        tab.padding = new RectOffset(12, 12, 0, 2);
        tab.fixedHeight = 32;
        tab.fixedWidth = 180;
        tab.fontSize = 14;
        tab.alignment = TextAnchor.MiddleLeft;

        GUIStyle bg = new GUIStyle("toolbarButton");
        bg.fixedHeight = 0;
        
        EditorGUILayout.BeginHorizontal();

        GUI.Label(new Rect(0, 0, 179, Screen.height), "", bg);
        tabScrollPos = EditorGUILayout.BeginScrollView(tabScrollPos, GUILayout.MinWidth(180), GUILayout.MaxWidth(180));
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
            GUILayout.Label("Keybindings", title, GUILayout.MinWidth(Screen.width - 210));
            GUILayout.Space(8);
            GUILayout.Label("(These can't be modified yet, for now use this as a reference)");
            string curCat = "";
            
            int rows = Mathf.Max((int)(Screen.width - 210) / 220, 1);
            int crow = 0;
            float w = (Screen.width - 210) / rows;
            float[] hs = new float[rows];

            Keybinds list = ChartmakerKeybinds;

            foreach (KeyValuePair<string, Keybind> kb in list.Values)
            {
                string cat = kb.Key.Remove(kb.Key.IndexOf("/"));
                string name = kb.Key.Substring(kb.Key.IndexOf("/") + 1);

                if (curCat != cat) 
                {
                    crow = 0;
                    for (int a = 1; a < rows; a++) if (hs[a] < hs[crow]) crow = a;
                    GUI.Label(new Rect(w * crow + 4, hs[crow] + 68, w - 5, 24), cat, title2);
                    hs[crow] += 32;
                    curCat = cat;
                }

                GUI.Label(new Rect(w * crow + 4, hs[crow] + 59, 120, 20), name);
                GUI.Button(new Rect(w * crow + 130, hs[crow] + 60, w - 132, 20), kb.Value.ToString(), EditorStyles.textField);
                hs[crow] += 22;
            }
            
            crow = 0;
            for (int a = 1; a < rows; a++) if (hs[a] > hs[crow]) crow = a;
            GUILayout.Space(hs[crow]);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndHorizontal();
    }
}

[System.Serializable]
public class Keybind
{
    public KeyCode KeyCode = KeyCode.Space;
    public EventModifiers Modifiers = EventModifiers.None;

    public Keybind (KeyCode keyCode, EventModifiers modifiers = EventModifiers.None)
    {
        KeyCode = keyCode;
        Modifiers = modifiers;
    }

    public bool Matches(Event ev)
    {
        EventModifiers cas = (EventModifiers.Shift | EventModifiers.Alt | EventModifiers.Control | EventModifiers.Command);
        if (Application.platform != RuntimePlatform.OSXEditor && ev.control) 
            ev.modifiers = ev.modifiers ^ EventModifiers.Control | EventModifiers.Command;
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
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            if ((Modifiers & EventModifiers.Shift) > 0) str = "⇧" + str;
            if ((Modifiers & EventModifiers.Alt) > 0) str = "⌥" + str;
            if ((Modifiers & EventModifiers.Command) > 0) str = "⌘" + str;
            if ((Modifiers & EventModifiers.Control) > 0) str = "⌃" + str;
        }
        else 
        {
            if ((Modifiers & EventModifiers.Shift) > 0) str = "Shift+" + str;
            if ((Modifiers & EventModifiers.Alt) > 0) str = "Alt+" + str;
            if ((Modifiers & (EventModifiers.Command | EventModifiers.Control)) > 0) str = "Ctrl+" + str;
        }
        return str;
    }

    public string ToUnityHotkeyString() 
    {
        string str = KeyCode.ToString();
        switch (str)
        {
            case "Slash": str = "/"; break;
            case "Backslash": str = "\\"; break;
            case "UpArrow": str = "UP"; break;
            case "DownArrow": str = "DOWN"; break;
            case "LeftArrow": str = "LEFT"; break;
            case "RightArrow": str = "RIGHT"; break;
        }

        if ((Modifiers & EventModifiers.Shift) > 0) str = "#" + str;
        if ((Modifiers & EventModifiers.Alt) > 0) str = "&" + str;
        if ((Modifiers & EventModifiers.Control) > 0) str = "^" + str;
        if ((Modifiers & EventModifiers.Command) > 0) str = "%" + str;
        return str;
    }
}

[System.Serializable]
public class Keybinds
{
    public Dictionary<string, Keybind> Values;

    public Keybind this[string index]
    {
        get {
            return Values[index]; 
        }
    }

    public Keybinds(Dictionary<string, Keybind> values)
    {
        Values = values;
    }

    public Keybinds()
    {
        Values = new Dictionary<string, Keybind>();

        Values["General/Toggle Play/Pause"] = new Keybind(KeyCode.P);
        Values["General/Play Chart in Player"] = new Keybind(KeyCode.P, EventModifiers.Shift);

        Values["File/Save"] = new Keybind(KeyCode.S, EventModifiers.Command);

        Values["Edit/Undo"] = new Keybind(KeyCode.Z, EventModifiers.Command);
        Values["Edit/Redo"] = new Keybind(KeyCode.Y, EventModifiers.Command);
        Values["Edit/Cut"] = new Keybind(KeyCode.X, EventModifiers.Command);
        Values["Edit/Copy"] = new Keybind(KeyCode.C, EventModifiers.Command);
        Values["Edit/Paste"] = new Keybind(KeyCode.V, EventModifiers.Command);
        Values["Edit/Delete"] = new Keybind(KeyCode.Backspace, EventModifiers.None);

        Values["Picker/Cursor"] = new Keybind(KeyCode.A);
        Values["Picker/Select"] = new Keybind(KeyCode.S);
        Values["Picker/Delete"] = new Keybind(KeyCode.D);
        Values["Picker/1st Item"] = new Keybind(KeyCode.Q);
        Values["Picker/2nd Item"] = new Keybind(KeyCode.W);

        Values["Selection/Previous Item"] = new Keybind(KeyCode.LeftArrow);
        Values["Selection/Next Item"] = new Keybind(KeyCode.RightArrow);
        Values["Selection/Previous Lane"] = new Keybind(KeyCode.LeftArrow, EventModifiers.Shift);
        Values["Selection/Next Lane"] = new Keybind(KeyCode.RightArrow, EventModifiers.Shift);

        Values["Misc./Show Keybindings"] = new Keybind(KeyCode.Slash, EventModifiers.Shift);
    }
}