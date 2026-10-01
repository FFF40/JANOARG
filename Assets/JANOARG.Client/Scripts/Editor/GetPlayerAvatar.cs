using UnityEditor;
using UnityEngine;
using JANOARG.Client.Data.Constant;

[CustomEditor(typeof(PlayerAvatarConstants))]
public class PlayerAvatarConstantsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerAvatarConstants data = (PlayerAvatarConstants)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Get All Song Avatars"))
        {
            data.GetAllSongAvatars();
            EditorUtility.SetDirty(data);
        }
    }
}