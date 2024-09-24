using TMPro;
using UnityEngine.UI;

public class FormEntryToggleFloat : FormEntry<float>
{
    public Toggle Toggle;
    public TMP_InputField Field;

    public new void Start() 
    {
        base.Start();
        Reset();
    }

    public void Reset()
    {
        Toggle.SetIsOnWithoutNotify(!float.IsNaN(CurrentValue));
        Field.gameObject.SetActive(Toggle.isOn);
        Field.SetTextWithoutNotify(Toggle.isOn ? CurrentValue.ToString() : "0");
    }

    public void SetValue(bool value)
    {
        Field.gameObject.SetActive(value);
        SetValue(value ? Field.text : "NaN");
    }
    
    public void SetValue(string value)
    {
        if (float.TryParse(value, out float v)) SetValue(v);
    }
}
