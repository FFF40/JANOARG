using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using System.Reflection;
#endif

public class RightClickButton : Button
{
    public UnityEvent onRightClick;

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (eventData.button == PointerEventData.InputButton.Right) 
        {
            onRightClick.Invoke();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RightClickButton), true)]
[CanEditMultipleObjects]
public class RightClickButtonInspector : ButtonEditor {
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onRightClick"));
        serializedObject.ApplyModifiedProperties();
    }
}
#endif