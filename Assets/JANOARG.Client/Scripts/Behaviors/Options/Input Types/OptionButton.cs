using System;
using UnityEngine.Events;

namespace JANOARG.Client.Behaviors.Options.Input_Types
{
    public class OptionButton : OptionItem
    {
        public UnityAction Action;

        public OptionButton(UnityAction action)
        {
            Action = action;
        }

        public void TriggerAction()
        {
            Action?.Invoke();
        }
    }
}