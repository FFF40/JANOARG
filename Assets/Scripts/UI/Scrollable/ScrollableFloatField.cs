using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollableFloatField : MonoBehaviour, IScrollHandler
{
    public TMP_InputField field;

    public void OnScroll(PointerEventData eventData)
    {
        if (float.TryParse(field.text, out float f))
        {
            // If scrolls up, return 1, if scrolls down, return -1, if not return 0
            float mult = eventData.scrollDelta.y > 0 ? 1 : eventData.scrollDelta.y < 0 ? -1 : 0;

            float addition;
            if (IsCtrlPressed() && IsShiftPressed())
                addition = 0.01f;
            else if (IsCtrlPressed())
                addition = 0.1f;
            else if (IsShiftPressed())
                addition = 10f;
            else
                addition = 1f;
                
            field.text = (f + (addition * mult)).ToString();
        }
    }

    public bool IsCtrlPressed() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    public bool IsShiftPressed() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
}