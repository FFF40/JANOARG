using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class JAEditorSettings : EditorWindow
{
    public static void InitSettings ()
    {
        if (Storage == null) Storage = new Storage("editor");
        if (Keybindings == null) Keybindings = new Storage("editor_keys");
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

    public static Storage Storage;
    public static Storage Keybindings;

    string ChangingAction;

    int currentTab = 0;

    void OnGUI () 
    {
        InitSettings();

        if (ChangingAction != null)
        {
            if (Event.current.isMouse) 
            {
                ChangingAction = null;
                Repaint();
            }
            else if (Event.current.type == EventType.KeyUp)
            {
                Chartmaker.KeybindActions.SetKey(ChangingAction, new(Event.current));
                ChangingAction = null;
                Repaint();
            }
        }

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

            GUIStyle miniTextField = new GUIStyle("miniTextField");
            miniTextField.fontSize = EditorStyles.miniLabel.fontSize;

            GUIStyle miniButton = new GUIStyle("button");
            miniButton.fontSize = EditorStyles.miniLabel.fontSize;
            
            int rows = Mathf.Max((int)(Screen.width - 200) / 200, 1);
            int crow = 0;
            float w = (Screen.width - 200) / rows;
            float[] hs = new float[rows];

            KeybindActionList list = Chartmaker.KeybindActions;
            Dictionary<string, Dictionary<string, KeybindAction>> cats = list.MakeCategoryGroups();
            
            GUILayout.Label("Keybindings", title, GUILayout.MinWidth(Screen.width - 210));

            foreach (KeyValuePair<string, Dictionary<string, KeybindAction>> cat in cats)
            {
                crow = 0;
                for (int a = 1; a < rows; a++) if (hs[a] < hs[crow]) crow = a;
                GUI.Label(new Rect(w * crow + 5, hs[crow] + 70, w - 2, 27 + 18 * cat.Value.Count), "", "HelpBox");
                GUI.Label(new Rect(w * crow + 5, hs[crow] + 70, w - 2, 22), cat.Key, "button");
                hs[crow] += 30;

                foreach (KeyValuePair<string, KeybindAction> action in cat.Value)
                {

                    GUI.Label(new Rect(w * crow + 8, hs[crow] + 65, 108, 16), action.Value.Name, "miniLabel");
                    if (ChangingAction == action.Key)
                    {
                        GUI.Button(new Rect(w * crow + 123, hs[crow] + 65, w - 124, 16), "Press a key combination...", miniButton);
                    }
                    else if (GUI.Button(new Rect(w * crow + 123, hs[crow] + 65, w - 124, 16), action.Value.Keybind.ToString(), miniTextField))
                    {
                        ChangingAction = action.Key;
                    }
                    hs[crow] += 18;
                }
            }
            
            crow = 0;
            for (int a = 1; a < rows; a++) if (hs[a] > hs[crow]) crow = a;
            GUILayout.Space(hs[crow] + 42);

            GUI.Label(new Rect(0, Mathf.Max(28, scrollPos.y + 1), Screen.width, 40), "", "CN Box");
            EditorGUI.DrawRect(new Rect(0, Mathf.Max(28, scrollPos.y), Screen.width, 40), 
                EditorGUIUtility.isProSkin
                    ? new Color32(56, 56, 56, 255)
                    : new Color32(194, 194, 194, 255)
            );
            
            GUI.Label(new Rect(4, Mathf.Max(32, scrollPos.y + 4), Screen.width, 37), string.IsNullOrWhiteSpace(ChangingAction)
                ? "Click on a keybind, then press a key or key combination to change it." 
                : "Editing " + list[ChangingAction].Category + "/" + list[ChangingAction].Name + "..." +
                    "\nPress a key or key combination to change the keybind, or mouse click to cancel."
            );
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

    public Keybind () {}

    public Keybind (KeyCode keyCode, EventModifiers modifiers = EventModifiers.None)
    {
        KeyCode = keyCode;
        Modifiers = modifiers;
    }

    public Keybind (Event ev)
    {
        KeyCode = ev.keyCode;
        Modifiers = ev.modifiers & (
              EventModifiers.Shift 
            | EventModifiers.Alt 
            | EventModifiers.Control 
            | EventModifiers.Command
        );
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
            case >= KeyCode.A and <= KeyCode.Z: break;
            case >= KeyCode.Exclaim and <= KeyCode.Tilde: str = (char)KeyCode + ""; break;

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
        switch (KeyCode)
        {
            case KeyCode.UpArrow: str = "UP"; break;
            case KeyCode.DownArrow: str = "DOWN"; break;
            case KeyCode.LeftArrow: str = "LEFT"; break;
            case KeyCode.RightArrow: str = "RIGHT"; break;
            case KeyCode.Home: str = "HOME"; break;
            case KeyCode.End: str = "END"; break;
            case KeyCode.PageUp: str = "PGUP"; break;
            case KeyCode.PageDown: str = "PGDN"; break;
            case KeyCode.Insert: str = "INS"; break;
            case KeyCode.Delete: str = "DEL"; break;
            case KeyCode.Tab: str = "TAB"; break;
            case KeyCode.Space: str = "SPACE"; break;

            case >= KeyCode.A and <= KeyCode.Z: break;
            case >= KeyCode.Exclaim and <= KeyCode.Tilde: str = (char)KeyCode + ""; break;
        }

        if ((Modifiers & EventModifiers.Shift) > 0) str = "#" + str;
        if ((Modifiers & EventModifiers.Alt) > 0) str = "&" + str;
        if ((Modifiers & EventModifiers.Control) > 0) str = "^" + str;
        if ((Modifiers & EventModifiers.Command) > 0) str = "%" + str;
        if (Modifiers == 0) str = "_" + str;
        return str;
    }

    public string ToSaveString()
    {
        string str = ((int)KeyCode).ToString();

        if ((Modifiers & EventModifiers.Shift) > 0) str = "#" + str;
        if ((Modifiers & EventModifiers.Alt) > 0) str = "&" + str;
        if ((Modifiers & EventModifiers.Control) > 0) str = "^" + str;
        if ((Modifiers & EventModifiers.Command) > 0) str = "%" + str;

        return str;
    }

    public static Keybind FromSaveString(string str)
    {
        EventModifiers mod = EventModifiers.None;
        
        if (str.StartsWith("%")) { mod |= EventModifiers.Command; str = str.Substring(1); }
        if (str.StartsWith("^")) { mod |= EventModifiers.Control; str = str.Substring(1); }
        if (str.StartsWith("&")) { mod |= EventModifiers.Alt;     str = str.Substring(1); }
        if (str.StartsWith("#")) { mod |= EventModifiers.Shift;   str = str.Substring(1); }

        return new((KeyCode)int.Parse(str), mod);
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
    public string SaveID;

    public KeybindActionList (string key) 
    {
        SaveID = key;
    }

    public void LoadKeys()
    {
        JAEditorSettings.InitSettings();
        foreach (KeyValuePair<string, KeybindAction> action in this) 
        {
            string str = JAEditorSettings.Keybindings.Get(SaveID + ":" + action.Key, (string)null);
            if (!string.IsNullOrWhiteSpace(str)) action.Value.Keybind = Keybind.FromSaveString(str);
        }
    }

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

    public void SetKey(string id, Keybind key)
    {
        this[id].Keybind = key;
        JAEditorSettings.Keybindings.Set(SaveID + ":" + id, key.ToSaveString());
        JAEditorSettings.Keybindings.Save();
    }

    public Dictionary<string, Dictionary<string, KeybindAction>> MakeCategoryGroups()
    {
        Dictionary<string, Dictionary<string, KeybindAction>> dict = new();
        foreach (KeyValuePair<string, KeybindAction> action in this) 
        {
            if (!dict.ContainsKey(action.Value.Category))
            {
                dict.Add(action.Value.Category, new Dictionary<string, KeybindAction>());
            }
            dict[action.Value.Category].Add(action.Key, action.Value);
        }
        return dict;
    }
}
