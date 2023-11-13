using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ThemeKey))]
public class ThemeKeyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var keyRect = new Rect(position.x, position.y, position.width / 2, position.height);
        var valueRect = new Rect(position.x + position.width / 2 + 2, position.y, position.width / 2 - 2, position.height);

        EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("Key"), GUIContent.none);
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Value"), GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}