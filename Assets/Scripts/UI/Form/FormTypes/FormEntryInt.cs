using TMPro;

public class FormEntryInt : FormEntry<int>
{
    public TMP_InputField Field;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset() => Field.SetTextWithoutNotify(CurrentValue.ToString());
    public void SetValue(string value)
    {
        if (int.TryParse(value, out int v)) SetValue(v);
    }
}
