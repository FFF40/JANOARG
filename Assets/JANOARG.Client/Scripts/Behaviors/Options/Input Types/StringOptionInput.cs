using TMPro;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Options.Input_Types
{
    public class StringOptionInput : OptionInput<string>
    {
        [Space] public int Limit = 16;

        public TMP_Text ValueHolder;

        public new void Start()
        {
            UpdateValue();
        }

        public new void UpdateValue()
        {
            base.UpdateValue();
            ValueHolder.text = CurrentValue;
        }

        public void Edit()
        {
            OptionInputHandler.sMain.Edit(this);
        }
    }
}
