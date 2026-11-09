using JANOARG.Client.Data.Constant;
using UnityEditor;

[CustomEditor(typeof(StoryActorConstants))]
public class StoryActorConstantsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "Ensure the sprite has no extra blank space at the top or bottom of the artwork.",
            MessageType.Info
        );

        DrawDefaultInspector();
    }
}