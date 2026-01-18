using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JANOARG.Client.Editor
{
    [InitializeOnLoad]
    public static class SceneStarter
    {
        [MenuItem("JANOARG/Scene Starter/Set Target Scene", priority = 10000)]
        private static void SetTargetScene()
        {
            string targetScene = EditorUtility.OpenFilePanel("Set target scene", "Assets", "unity");
            PlayerPrefs.SetString("TargetScene", targetScene);
            UnityEngine.Debug.Log("Set target scene to " + targetScene);
        }

        static SceneStarter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            string targetScene = PlayerPrefs.GetString("TargetScene");

            if (!string.IsNullOrEmpty(targetScene))
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    UnityEngine.Debug.ClearDeveloperConsole();
                    UnityEngine.Debug.Log("Loading scene " + targetScene);
                    EditorSceneManager.LoadSceneInPlayMode(targetScene, new LoadSceneParameters(LoadSceneMode.Single));
                }
        }
    }
}