using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CursorFrame))]
public class CursorFrameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var keyRect = new Rect(position.x, position.y, position.width / 3 * 2, position.height);
        var valueRect = new Rect(position.x + position.width / 3 * 2 + 2, position.y, position.width / 3 - 2, position.height);

        EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("Texture"), GUIContent.none);
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Duration"), GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}