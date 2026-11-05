using JANOARG.Client.Behaviors.SongSelect.Map;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightHalo))]
public class LightHaloEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Tint"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Size"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Intensity"));

        SerializedProperty bright = serializedObject.FindProperty("BrightCore");
        EditorGUILayout.PropertyField(bright);

        using (new EditorGUI.DisabledScope(!bright.boolValue))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CoreSize"));

        serializedObject.ApplyModifiedProperties();
    }
}
