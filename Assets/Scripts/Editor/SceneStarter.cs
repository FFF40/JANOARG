using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SceneStarter
{

    [MenuItem("JANOARG/Scene Starter/Set Target Scene")]
	static void SetTargetScene() 
	{
		string targetScene = EditorUtility.OpenFilePanel("Set target scene", "Assets", "unity");
		PlayerPrefs.SetString("TargetScene", targetScene);
		Debug.Log("Set target scene to " + targetScene);
	}

    static SceneStarter()
	{
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		string targetScene = PlayerPrefs.GetString("TargetScene");

		if (!string.IsNullOrEmpty(targetScene)) 
		{
			if (state == PlayModeStateChange.EnteredPlayMode) 
			{
				Debug.ClearDeveloperConsole();
				Debug.Log("Loading scene " + targetScene);
				EditorSceneManager.LoadSceneInPlayMode(targetScene, new (LoadSceneMode.Single));
			}
		}
    }
}