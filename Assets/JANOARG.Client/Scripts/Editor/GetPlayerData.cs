using UnityEditor;
using UnityEngine;
using JANOARG.Client.Data.Player;

[CustomEditor(typeof(PlayerAvatars))]
public class PlayerAvatarsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerAvatars data = (PlayerAvatars)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Get All Song Avatars"))
        {
            data.GetAllSongAvatars();
            EditorUtility.SetDirty(data);
        }
    }
}

[CustomEditor(typeof(PlayerTitles))]
public class PlayerTitlesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerTitles data = (PlayerTitles)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Get All Song Avatars"))
        {
            data.GetAllSongTitles();
            EditorUtility.SetDirty(data);
        }
    }
}