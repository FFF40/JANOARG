using System.Collections.Generic;
using System.IO;
using System.Linq;
using JANOARG.Client.Behaviors.Player;
using JANOARG.Shared.Data.ChartInfo;
using UnityEditor;
using UnityEngine;

namespace JANOARG.Client.Editor
{
    [CustomEditor(typeof(PlayerScreen))]
    internal class PlayerScreenEditor : UnityEditor.Editor
    {
        private SerializedProperty _RunChart;
        private SerializedProperty _Chart;

        private void OnEnable()
        {
            _RunChart = serializedObject.FindProperty("RunChart");
            _Chart = serializedObject.FindProperty("Chart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((PlayerScreen)target), typeof(PlayerScreen), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Headless Initialisation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_RunChart);

            DrawChartSelector();

            EditorGUILayout.Space();
            DrawPropertiesExcluding(serializedObject, "m_Script", "RunChart", "Chart");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawChartSelector()
        {
            ExternalPlayableSong runChart = _RunChart.objectReferenceValue as ExternalPlayableSong;
            if (runChart == null)
            {
                if (_Chart.objectReferenceValue != null)
                    _Chart.objectReferenceValue = null;

                EditorGUILayout.HelpBox("Assign Run Chart to choose a chart from the same source folder.", MessageType.Info);
                return;
            }

            string runChartPath = AssetDatabase.GetAssetPath(runChart);
            string folderPath = Path.GetDirectoryName(runChartPath);

            if (string.IsNullOrEmpty(folderPath))
            {
                EditorGUILayout.HelpBox("Couldn't resolve the Run Chart asset folder.", MessageType.Warning);
                return;
            }

            List<ExternalChart> charts = AssetDatabase.FindAssets("t:ExternalChart", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetDirectoryName(path) == folderPath)
                .Select(AssetDatabase.LoadAssetAtPath<ExternalChart>)
                .Where(chart => chart != null)
                .OrderBy(chart => chart.name)
                .ToList();

            if (charts.Count == 0)
            {
                if (_Chart.objectReferenceValue != null)
                    _Chart.objectReferenceValue = null;

                EditorGUILayout.HelpBox($"No ExternalChart assets were found beside '{runChart.name}'.", MessageType.Warning);
                return;
            }

            ExternalChart selectedChart = _Chart.objectReferenceValue as ExternalChart;
            if (selectedChart != null && !charts.Contains(selectedChart))
            {
                selectedChart = null;
                _Chart.objectReferenceValue = null;
            }

            string[] options = charts
                .Select(chart => $"{chart.name} ({Path.GetFileName(AssetDatabase.GetAssetPath(chart))})")
                .ToArray();

            int currentIndex = Mathf.Max(0, selectedChart == null ? 0 : charts.IndexOf(selectedChart));
            int nextIndex = EditorGUILayout.Popup("Chart", currentIndex, options);

            _Chart.objectReferenceValue = charts[nextIndex];

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Selected Chart", _Chart.objectReferenceValue, typeof(ExternalChart), false);
        }
    }
}
