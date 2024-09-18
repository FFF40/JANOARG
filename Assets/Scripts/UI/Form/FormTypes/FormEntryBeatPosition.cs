using TMPro;

public class FormEntryBeatPosition : FormEntry<BeatPosition>
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
        if (BeatPosition.TryParse(value, out BeatPosition v)) SetValue(v);
    }
}
