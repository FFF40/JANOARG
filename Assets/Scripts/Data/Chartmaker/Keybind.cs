
using System.Collections.Generic;
using UnityEngine;

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
        EventModifiers cas = EventModifiers.Shift | EventModifiers.Alt | EventModifiers.Control | EventModifiers.Command;
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

        if (Application.platform is RuntimePlatform.OSXPlayer or RuntimePlatform.OSXEditor)
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
        
        if (str.StartsWith("%")) { mod |= EventModifiers.Command; str = str[1..]; }
        if (str.StartsWith("^")) { mod |= EventModifiers.Control; str = str[1..]; }
        if (str.StartsWith("&")) { mod |= EventModifiers.Alt;     str = str[1..]; }
        if (str.StartsWith("#")) { mod |= EventModifiers.Shift;   str = str[1..]; }

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

    public void LoadKeys()
    {
        foreach (KeyValuePair<string, KeybindAction> action in this) 
        {
            string str = Chartmaker.main.KeybindingsStorage.Get(action.Key, (string)null);
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
