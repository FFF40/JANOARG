using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
    
[RequireComponent(typeof(ScrollRect))]
[ExecuteAlways]
public class ScrollRectLayout : LayoutElement, ILayoutElement {

    public ScrollRect ScrollRect;

    protected override void OnEnable()
    {
        if (!Application.IsPlaying(gameObject))
        {
            if (!ScrollRect) ScrollRect = GetComponent<ScrollRect>();
        }
        base.OnEnable();
    }

    public override void CalculateLayoutInputHorizontal() 
    {
        preferredWidth = ScrollRect ? LayoutUtility.GetPreferredWidth(ScrollRect.content) : 1;
    }
    public override void CalculateLayoutInputVertical() 
    {
        preferredHeight = ScrollRect ? LayoutUtility.GetPreferredHeight(ScrollRect.content) : 1;
    }
}
    
#if UNITY_EDITOR
[CustomEditor(typeof(ScrollRectLayout))]
public class ScrollRectLayoutEditor : Editor {
    public override void OnInspectorGUI() {
        ScrollRectLayout target = (ScrollRectLayout)this.target;
        EditorGUILayout.ObjectField("Scroll Rect", target, typeof(ScrollRectLayout), true);
    }
}
#endif
