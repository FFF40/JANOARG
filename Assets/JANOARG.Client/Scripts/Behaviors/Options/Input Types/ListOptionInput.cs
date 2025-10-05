using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Options.Input_Types
{
    public class ListOptionInput<T> : OptionInput<T>
    {
        [Space] public TMP_Text ValueHolder;

        [Space] public Dictionary<T, string> ValidValues = new();

        public new void Start()
        {
            UpdateValue();
        }

        public new void UpdateValue()
        {
            base.UpdateValue();
            if (!ValidValues.TryGetValue(CurrentValue, out string text)) text = "<i>Select an option...</i>";
            ValueHolder.text = text;
        }

        public void Edit()
        {
            OptionInputHandler.sMain.Edit(this);
        }
    }

    public class ListOptionInput : ListOptionInput<string>
    {
    }
}
