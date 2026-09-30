using System.IO;
using JANOARG.Shared.Data.ChartInfo;
using UnityEditor;
using UnityEngine;

namespace JANOARG.Client.Data.Playlist.Editor
{
    [CustomPropertyDrawer(typeof(JAPSPickerAttribute))]
    public class JAPSPickerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // TODO Would this operation too expensive to run every GUI update?
            ExternalPlayableSong current = FindSongAsset(property.stringValue);

            EditorGUI.BeginChangeCheck();
            var picked = (ExternalPlayableSong)EditorGUI.ObjectField(
                position, label, current, typeof(ExternalPlayableSong), false);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = picked
                    ? Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(picked))
                    : "";
            }

            EditorGUI.EndProperty();
        }

        static ExternalPlayableSong FindSongAsset(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            // Same path SongSelectScreen.InitPlaylist resolves at runtime - Resources'
            // own index, not a project-wide AssetDatabase search.
            return Resources.Load<ExternalPlayableSong>($"Songs/{id}/{id}");
        }
    }
}
