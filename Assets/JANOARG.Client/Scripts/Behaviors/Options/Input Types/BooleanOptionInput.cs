using JANOARG.Client.Scripts.UI;

namespace JANOARG.Client.Scripts.Behaviors.Options.Input_Types
{
    public class BooleanOptionInput : OptionInput<bool>
    {
        public AnimatedToggle Toggle;

        public new void Start() 
        {
            UpdateValue();
        }

        public new void UpdateValue() 
        {
            base.UpdateValue();
            Toggle.Value = CurrentValue;
        }

        public void OnToggle()
        {
            if (Toggle.Value != CurrentValue)
            {
                CurrentValue = Toggle.Value;
                Set(CurrentValue);
            }
        }
    }
}
