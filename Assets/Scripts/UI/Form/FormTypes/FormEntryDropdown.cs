using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormEntryDropdown<T> : FormEntry<T>
{
    public TMP_Text ValueLabel;
    public Button DrowpdownButton;
    public Dictionary<T, string> ValidValues = new ();

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset()
    {
        ValueLabel.text = CurrentValue != null && ValidValues.ContainsKey(CurrentValue) ? ValidValues[CurrentValue] : "Select...";
    }

    public void OpenList()
    {
        ContextMenuList list = new ContextMenuList();
        foreach (KeyValuePair<T, string> item in ValidValues)
        {
            list.Items.Add(new ContextMenuListAction(item.Value, () => {
                SetValue(item.Key); Reset();
            }, _checked: item.Key.Equals(CurrentValue)));
        }
        ContextMenuHolder.main.OpenRoot(list, (RectTransform)DrowpdownButton.transform);
    }
}

public class FormEntryDropdown : FormEntryDropdown<object> {
    public void TargetEnum(Type type)
    {
        if (type.IsEnum)
        {
            foreach (var item in type.GetEnumValues())
            {
                string name = type.GetEnumName(item);
                ValidValues.Add(item, name);
            }
        }
        else
        {
            throw new ArgumentException("Type is not an enum");
        }
    }
    public void TargetList(params string[] list)
    {
        for (int a = 0; a < list.Length; a++)
        {
            ValidValues.Add(a, list[a]);
        }
    }
}
