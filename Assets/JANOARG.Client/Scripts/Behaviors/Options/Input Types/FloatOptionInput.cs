using TMPro;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Options.Input_Types
{
    public class FloatOptionInput : OptionInput<float>
    {
        [Space] public float Min;

        public float  Max  = 100;
        public float  Step = 1;
        public string Unit;

        [Space] public TMP_Text ValueHolder;

        public TMP_Text UnitLabel;

        public new void Start()
        {
            UpdateValue();
        }

        public new void UpdateValue()
        {
            base.UpdateValue();
            ValueHolder.text = CurrentValue.ToString();
            UnitLabel.text = "<alpha=#77>" + Unit;
        }

        public void Edit()
        {
            OptionInputHandler.sMain.Edit(this);
        }
    }
}
