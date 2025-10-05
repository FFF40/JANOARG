using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.UI
{
    public class OptionsPanelTabButton : MonoBehaviour
    {
        public Graphic       Fill;
        public Graphic       Icon;
        public RectTransform Holder;

        public void SetFill(float amount)
        {
            Fill.rectTransform.anchorMin = new Vector2(1 - amount, 0);
            Icon.color = Color.Lerp(Color.white, Color.black, amount);
            Holder.sizeDelta = new Vector2(54 + 10 * amount, Holder.sizeDelta.y);
        }
    }
}
