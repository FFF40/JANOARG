using UnityEditor;
using UnityEngine;
using JANOARG.Client.Data.Player;

[CustomEditor(typeof(PlayerAvatar))]
public class PlayerAvatarConstantsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerAvatar data = (PlayerAvatar)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Get All Song Avatars"))
        {
            data.GetAllSongAvatars();
            EditorUtility.SetDirty(data);
        }
    }
}