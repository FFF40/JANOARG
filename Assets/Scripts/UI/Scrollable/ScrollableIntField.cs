using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollableIntField : MonoBehaviour, IScrollHandler
{
    public TMP_InputField field;

    public void OnScroll(PointerEventData eventData)
    {
        if (int.TryParse(field.text, out int i))
        {
            // If scrolls up, return 1, if scrolls down, return -1, if not return 0
            float mult = eventData.scrollDelta.y > 0 ? 1 : eventData.scrollDelta.y < 0 ? -1 : 0;
            
            float addition;
            if (IsCtrlPressed() && IsShiftPressed())
                addition = 1000;
            else if (IsCtrlPressed())
                addition = 100;
            else if (IsShiftPressed())
                addition = 10;
            else
                addition = 1;
                
            field.text = (i + (mult * addition)).ToString();
        }
    }

    public bool IsCtrlPressed() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    public bool IsShiftPressed() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
}