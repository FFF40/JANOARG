using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.Callbacks;

public class GizmoIconUtility 
{
	[DidReloadScripts]
	static GizmoIconUtility()
	{
		EditorApplication.projectWindowItemOnGUI = ItemOnGUI;
	}

	static void ItemOnGUI(string guid, Rect rect)
	{
		string assetPath = AssetDatabase.GUIDToAssetPath(guid);

		Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

        if (rect.height > rect.width) rect.height = rect.width;
        else rect.width = rect.height;
        if (rect.height == 16) rect.x += 3;

		if (obj is ExternalPlayableSong)
		{
			EditorGUI.DrawRect(rect, Color.black);
		}
		else if (obj is ExternalChart)
		{
            ExternalChart item = (ExternalChart)obj;
			EditorGUI.DrawRect(rect, Color.white);

            GUIStyle diffStyle = new GUIStyle("label");
            diffStyle.alignment = TextAnchor.MiddleCenter;
            diffStyle.normal.textColor = Color.black;
            diffStyle.fontSize = Mathf.RoundToInt(rect.height / 2);

			GUI.Label(rect, item.Data.DifficultyLevel, diffStyle);
		}
	}
}