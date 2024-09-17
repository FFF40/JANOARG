using TMPro;

public class FormEntryKeybind : FormEntry<Keybind>
{
    public string Category;
    public TMP_Text Field;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset() => Field.text = CurrentValue.ToString();
    public void StartChange() 
    {
        KeyboardHandler.main.StartKeybindChange(this);
    }
}
