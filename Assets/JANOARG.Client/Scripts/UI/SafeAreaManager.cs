using UnityEngine;

namespace JANOARG.Client.UI
{
    public class SafeAreaManager : MonoBehaviour
    {
        private void Awake()
        {
            var rt = GetComponent<RectTransform>();
            var screen = new Rect(0, 0, Screen.width, Screen.height);
            Rect safeArea = Screen.safeArea;
            float scale = Screen.width / 800f;

            rt.anchoredPosition = new Vector2(
                0,
                0 /* ((sc.yMax - sa.yMax) - (sc.yMin - sa.yMin)) / 2 / scale */);

            rt.sizeDelta = new Vector2(
                -Mathf.Max(screen.xMax - safeArea.xMax, safeArea.xMin - screen.xMin) * 2 / scale,
                0 /* (sa.height - sc.height) / scale */);
        }
    }
}
