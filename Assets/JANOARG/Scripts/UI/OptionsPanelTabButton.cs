using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelTabButton : MonoBehaviour
{
    public Graphic Fill;
    public Graphic Icon;
    public RectTransform Holder;

    public void SetFill(float amount) {
        Fill.rectTransform.anchorMin = new (1 - amount, 0);
        Icon.color = Color.Lerp(Color.white, Color.black, amount);
        Holder.sizeDelta = new (54 + 10 * amount, Holder.sizeDelta.y);
    }
}
