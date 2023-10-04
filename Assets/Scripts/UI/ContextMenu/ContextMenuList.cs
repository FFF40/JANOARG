
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class ContextMenuList
{
    public List<ContextMenuListItem> Items;

    public ContextMenuList (params ContextMenuListItem[] items) {
        Items = new List<ContextMenuListItem>(items);
    }
}

public class ContextMenuListItem
{
}

public class ContextMenuListSeparator : ContextMenuListItem
{
}
public class ContextMenuListAction : ContextMenuListItem
{
    public string Content;
    public UnityAction Action;
    public string Shortcut;
    public bool Checked;
    public bool Enabled;
    public string Icon;

    public ContextMenuListAction (string content, UnityAction action, string shortcut = "", bool _checked = false, bool _enabled = true, string icon = "")
    {
        Content = content;
        Action = action;
        Shortcut = shortcut;
        Checked = _checked;
        Enabled = _enabled;
        Icon = icon;
    }
}

public class ContextMenuListSublist : ContextMenuListItem
{
    public string Title;
    public ContextMenuList Items;
    public string Icon;

    public ContextMenuListSublist (string title, params ContextMenuListItem[] items) {
        Title = title;
        Items = new ContextMenuList(items);
        Icon = "";
    }
    public ContextMenuListSublist (string title, string icon, params ContextMenuListItem[] items) {
        Title = title;
        Items = new ContextMenuList(items);
        Icon = icon;
    }
}