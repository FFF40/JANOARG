using JANOARG.Client.UI;

namespace JANOARG.Client.Behaviors.Options.Input_Types
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
            Toggle.value = CurrentValue;
        }

        public void OnToggle()
        {
                CurrentValue = Toggle.value;
                Set(CurrentValue);
        }
    }
}
