using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormEntryEasing : FormEntry<EasingPair>
{
    public TMP_Text EaseFunctionLabel;
    public Button EaseFunctionButton;
    public TMP_Text EaseModeLabel;
    public Button EaseModeButton;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset()
    {
        EaseFunctionLabel.text = Enum.GetName(typeof(EaseFunction), CurrentValue.Function) ?? "Select...";
        EaseModeLabel.text = 
            CurrentValue.Mode == EaseMode.In ? "I" :
            CurrentValue.Mode == EaseMode.Out ? "O" :
            CurrentValue.Mode == EaseMode.InOut ? "IO" : "Select...";
    }

    public void OpenFunctionList()
    {
        ContextMenuList list = new();
        foreach (var item in Enum.GetValues(typeof(EaseFunction)))
        {
            var Item = item;
            string name = Enum.GetName(typeof(EaseFunction), item);
            list.Items.Add(new ContextMenuListAction(name, () => {
                CurrentValue.Function = (EaseFunction)Item; SetValue(CurrentValue); Reset();
            }, _checked: CurrentValue.Function == (EaseFunction)Item));
        }
        ContextMenuHolder.main.OpenRoot(list, (RectTransform)EaseFunctionButton.transform);
    }

    public void OpenModeList()
    {
        ContextMenuList list = new();
        foreach (var item in Enum.GetValues(typeof(EaseMode)))
        {
            var Item = item;
            string name = Enum.GetName(typeof(EaseMode), item);
            list.Items.Add(new ContextMenuListAction(name, () => {
                CurrentValue.Mode = (EaseMode)Item; SetValue(CurrentValue); Reset();
            }, _checked: CurrentValue.Mode == (EaseMode)Item));
        }
        ContextMenuHolder.main.OpenRoot(list, (RectTransform)EaseModeButton.transform);
    }
}

public struct EasingPair {
    public EaseFunction Function;
    public EaseMode Mode;

    public EasingPair(EaseFunction function, EaseMode mode)
    {
        Function = function;
        Mode = mode;
    }
}
