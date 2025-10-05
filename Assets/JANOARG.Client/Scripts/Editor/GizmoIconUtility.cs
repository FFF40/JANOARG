using JANOARG.Shared.Data.ChartInfo;
using UnityEditor;
using UnityEngine;

namespace JANOARG.Client.Editor
{
    public class GizmoIconUtility
    {
        static GizmoIconUtility()
        {
            EditorApplication.projectWindowItemOnGUI = ItemOnGUI;
        }

        private static void ItemOnGUI(string guid, Rect rect)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            if (rect.height > rect.width) rect.height = rect.width;
            else rect.width = rect.height;

            if (Mathf.Approximately(rect.height, 16)) rect.x += 3;

            if (obj is ExternalPlayableSong)
            {
                EditorGUI.DrawRect(rect, Color.black);
            }
            else if (obj is ExternalChart)
            {
                var item = (ExternalChart)obj;
                EditorGUI.DrawRect(rect, Color.white);

                GUIStyle diffStyle = new("label");
                diffStyle.alignment = TextAnchor.MiddleCenter;
                diffStyle.normal.textColor = Color.black;
                diffStyle.fontSize = Mathf.RoundToInt(rect.height / 2);

                GUI.Label(rect, item.Data.DifficultyLevel, diffStyle);
            }
        }
    }
}