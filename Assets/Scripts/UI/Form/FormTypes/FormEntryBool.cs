using UnityEngine.UI;

public class FormEntryBool : FormEntry<bool>
{
    public Toggle Toggle;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset() => Toggle.SetIsOnWithoutNotify(CurrentValue);
}
