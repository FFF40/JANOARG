using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
    
public class RestrainedSizeFitter : ContentSizeFitter {
    
    // Define the min and max width and height
    public float minWidth = 0;
    public float maxWidth = float.PositiveInfinity;
    public float minHeight = 0;
    public float maxHeight = float.PositiveInfinity;
    
    
    public override void SetLayoutHorizontal()
    {
        base.SetLayoutHorizontal();
        RectTransform rt = (RectTransform)transform;
        Rect rect = rt.rect;
        Vector2 sizeDelta = rt.sizeDelta; 
        sizeDelta.x += Mathf.Clamp(rect.width, minWidth, maxWidth) - rect.width;
        rt.sizeDelta = sizeDelta;
    }
    
    
    public override void SetLayoutVertical()
        {
        base.SetLayoutVertical();
        RectTransform rt = (RectTransform)transform;
        Rect rect = rt.rect;
        Vector2 sizeDelta = rt.sizeDelta; 
        sizeDelta.y += Mathf.Clamp(rect.height, minHeight, maxHeight) - rect.height;
        rt.sizeDelta = sizeDelta;
    }
}
    
#if UNITY_EDITOR
[CustomEditor(typeof(RestrainedSizeFitter))]
public class RestrainedSizeFitterEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }
}
#endif
