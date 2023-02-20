using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class JAEditorSettings : EditorWindow
{
    public static void InitSettings ()
    {
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

            GUIStyle miniTextField = "miniTextField";
            miniTextField.fontSize = EditorStyles.miniLabel.fontSize;
            
            int rows = Mathf.Max((int)(Screen.width - 200) / 200, 1);
            int crow = 0;
            float w = (Screen.width - 200) / rows;
            float[] hs = new float[rows];

            Dictionary<string, List<KeybindAction>> cats = Chartmaker.KeybindActions.MakeCategoryGroups();

            foreach (KeyValuePair<string, List<KeybindAction>> cat in cats)
            {
                crow = 0;
                for (int a = 1; a < rows; a++) if (hs[a] < hs[crow]) crow = a;
                GUI.Label(new Rect(w * crow + 5, hs[crow] + 68, w - 2, 27 + 18 * cat.Value.Count), "", "HelpBox");
                GUI.Label(new Rect(w * crow + 5, hs[crow] + 68, w - 2, 22), cat.Key, "button");
                hs[crow] += 30;

                foreach (KeybindAction action in cat.Value)
                {

                    GUI.Label(new Rect(w * crow + 8, hs[crow] + 63, 108, 16), action.Name, "miniLabel");
                    GUI.Button(new Rect(w * crow + 123, hs[crow] + 63, w - 124, 16), action.Keybind.ToString(), miniTextField);
                    hs[crow] += 18;
                }
            }
            
            crow = 0;
            for (int a = 1; a < rows; a++) if (hs[a] > hs[crow]) crow = a;
            GUILayout.Space(hs[crow] + 12);
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
        switch (KeyCode)
        {
            case >= KeyCode.Alpha0 and <= KeyCode.Alpha9: str = ((int)KeyCode - 48).ToString(); break;

            case KeyCode.Slash: str = "/"; break;
            case KeyCode.Backslash: str = "\\"; break;

            case KeyCode.UpArrow: str = "↑"; break;
            case KeyCode.DownArrow: str = "↓"; break;
            case KeyCode.LeftArrow: str = "←"; break;
            case KeyCode.RightArrow: str = "→"; break;
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

public class KeybindAction
{
    public string Name;
    public string Category;
    public Keybind Keybind;
    public System.Action Invoke;
}

public class KeybindActionList: Dictionary<string, KeybindAction>
{
    public void HandleEvent(Event ev)
    {
        foreach (KeybindAction action in this.Values) 
        {
            if (action.Keybind.Matches(ev)) 
            {
                action.Invoke();
                ev.Use();
                break;
            }
        }
    }

    public Dictionary<string, List<KeybindAction>> MakeCategoryGroups()
    {
        Dictionary<string, List<KeybindAction>> dict = new Dictionary<string, List<KeybindAction>>();
        foreach (KeybindAction action in this.Values) 
        {
            if (!dict.ContainsKey(action.Category))
            {
                dict.Add(action.Category, new List<KeybindAction>());
            }
            dict[action.Category].Add(action);
        }
        return dict;
    }
}
