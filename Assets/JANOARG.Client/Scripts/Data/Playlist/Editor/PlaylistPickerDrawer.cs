using System.IO;
using JANOARG.Client.Data.Playlist;
using UnityEditor;
using UnityEngine;

namespace JANOARG.Client.Data.Playlist.Editor
{
    [CustomPropertyDrawer(typeof(PlaylistPickerAttribute))]
    public class PlaylistPickerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Playlist current = FindPlaylist(property.stringValue);

            EditorGUI.BeginChangeCheck();
            var picked = (Playlist)EditorGUI.ObjectField(
                position, label, current, typeof(Playlist), false);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = picked != null
                    ? (!string.IsNullOrEmpty(picked.ID) ? picked.ID : (!string.IsNullOrEmpty(picked.MapName) ? picked.MapName : picked.name))
                    : "";
            }

            EditorGUI.EndProperty();
        }

        static Playlist FindPlaylist(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (Playlist p in Resources.LoadAll<Playlist>("Playlists"))
            {
                if (p.ID == id || p.MapName == id || p.name == id) return p;
            }
            return null;
        }
    }
}
