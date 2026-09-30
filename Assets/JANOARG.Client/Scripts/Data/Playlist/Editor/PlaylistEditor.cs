using UnityEditor;
using UnityEngine;

namespace JANOARG.Client.Data.Playlist.Editor
{
    [CustomEditor(typeof(Playlist))]
    public class PlaylistEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            int prevSongsCount = serializedObject.FindProperty("Songs").arraySize;
            int prevPlaylistsCount = serializedObject.FindProperty("Playlists").arraySize;

            DrawDefaultInspector();

            int newSongsCount = serializedObject.FindProperty("Songs").arraySize;
            int newPlaylistsCount = serializedObject.FindProperty("Playlists").arraySize;

            if (newSongsCount > prevSongsCount || newPlaylistsCount > prevPlaylistsCount)
            {
                // Don't mutate SerializeReference data inline here - OnInspectorGUI runs once
                // for the Layout event and once for Repaint in the same frame, and Repaint
                // would then see different data than what Layout measured, which throws
                // "Getting control X's position in a group with only Y controls". Defer the
                // fix-up to run entirely outside the current GUI pass instead.
                Object targetObject = target;
                int songsFrom = prevSongsCount;
                int playlistsFrom = prevPlaylistsCount;
                EditorApplication.delayCall += () => FixUpNewEntries(targetObject, songsFrom, playlistsFrom);
            }
        }

        // Unity's default "+" duplicates the last element's raw serialized data.
        // For [SerializeReference] fields that copies the rid pointer rather than
        // the referenced object, aliasing every duplicated entry's conditionals
        // to the same managed instance. Deep-clone them here so each new entry
        // owns its own conditional objects instead.
        static void FixUpNewEntries(Object targetObject, int songsFrom, int playlistsFrom)
        {
            if (!targetObject) return;

            var so = new SerializedObject(targetObject);
            SerializedProperty songs = so.FindProperty("Songs");
            SerializedProperty playlists = so.FindProperty("Playlists");

            bool changed = false;
            if (songs.arraySize > songsFrom)
            {
                CloneNewEntryConditionals(songs, songsFrom);
                changed = true;
            }
            if (playlists.arraySize > playlistsFrom)
            {
                CloneNewEntryConditionals(playlists, playlistsFrom);
                changed = true;
            }

            if (changed)
                so.ApplyModifiedProperties();
        }

        static void CloneNewEntryConditionals(SerializedProperty arrayProp, int previousCount)
        {
            for (int i = previousCount; i < arrayProp.arraySize; i++)
            {
                SerializedProperty element = arrayProp.GetArrayElementAtIndex(i);
                DeepCloneConditionals(element.FindPropertyRelative("RevealConditions"));
                DeepCloneConditionals(element.FindPropertyRelative("UnlockConditions"));
            }
        }

        static void DeepCloneConditionals(SerializedProperty conditionals)
        {
            if (conditionals == null) return;
            for (int i = 0; i < conditionals.arraySize; i++)
            {
                SerializedProperty element = conditionals.GetArrayElementAtIndex(i);
                if (element.boxedValue is GameConditional conditional)
                {
                    element.boxedValue = conditional.Clone();
                }
            }
        }
    }
}
