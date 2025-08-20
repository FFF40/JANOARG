using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Scripts.Behaviors.Options.Input_Types
{
    public class ListOptionInput<T> : OptionInput<T>
    {
        [Space]
        public Dictionary<T, string> ValidValues = new ();
        [Space]
        public TMP_Text ValueHolder;

        public new void Start() 
        {
            UpdateValue();
        }

        public new void UpdateValue() 
        {
            base.UpdateValue();
            if (!ValidValues.TryGetValue(CurrentValue, out string text)) text = "<i>Select an option...</i>";
            ValueHolder.text = text.ToString();
        }

        public void Edit()
        {
            OptionInputHandler.main.Edit(this);
        }
    }

    public class ListOptionInput : ListOptionInput<string>
    {
    }
}