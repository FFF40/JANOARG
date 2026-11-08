using System.Text.RegularExpressions;
using JANOARG.Client.Data.Story.Instructions;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StoryInstruction), true)] // true = also applies to derived classes
public class StoryInstructionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float height = EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded) return height;

        var iter = property.Copy();
        var end = iter.GetEndProperty();
        bool enter = true;
        while (iter.NextVisible(enter) && !SerializedProperty.EqualContents(iter, end))
        {
            enter = false;
            height += spacing + EditorGUI.GetPropertyHeight(iter, true);
        }
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Our own title instead of "Element N" / first string field
        var title = new GUIContent(GetTypeName(property));
        var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, title, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float y = line.yMax + EditorGUIUtility.standardVerticalSpacing;

            var iter = property.Copy();
            var end = iter.GetEndProperty();
            bool enter = true;
            while (iter.NextVisible(enter) && !SerializedProperty.EqualContents(iter, end))
            {
                enter = false;
                float h = EditorGUI.GetPropertyHeight(iter, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), iter, true);
                y += h + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private static string GetTypeName(SerializedProperty property)
    {
        // Works for [SerializeReference]: "Assembly Namespace.TypeName"
        string full = property.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(full)) return "StoryInstruction";
        
        int dot = full.LastIndexOf('.');
        int space = full.LastIndexOf(' ');
        string typeName = full.Substring(Mathf.Max(dot, space) + 1);

        return Prettify(typeName);
    }

    private static string Prettify(string typeName)
    {
        string cleaned = typeName.Replace("StoryInstruction", "");
        if (string.IsNullOrEmpty(cleaned)) cleaned = typeName;
        cleaned = Regex.Replace(cleaned, @"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");

        return cleaned.Trim();
    }
}