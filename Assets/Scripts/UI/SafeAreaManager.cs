using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaManager : MonoBehaviour
{
    void Awake()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Rect sc = new Rect(0, 0, Screen.width, Screen.height);
        Rect sa = Screen.safeArea;
        float scale = Screen.width / 800;
        rt.anchoredPosition = new Vector2(
            0, 
            0 /* ((sc.yMax - sa.yMax) - (sc.yMin - sa.yMin)) / 2 / scale */ );
        rt.sizeDelta = new Vector2(
            -Mathf.Max(sc.xMax - sa.xMax, sa.xMin - sc.xMin) * 2 / scale, 
            0 /* (sa.height - sc.height) / scale */ );
    }
}
