using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Common : MonoBehaviour
{
    public static Common main;

    public Camera MainCamera;
    public RectTransform CommonCanvas;
    public CommonConstants Constants;

    public LoadingBar LoadingBar;
    public Storage Storage;
    public Storage Preferences;

    public void Awake()
    {
        main = this;

        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Storage = new Storage("save");
        int count = Storage.Get("STAT:Count", 0) + 1;
        Storage.Set("STAT:Count", count);
        Debug.Log(count);
        Storage.Save();

        Preferences = new Storage("prefs");

        Application.targetFrameRate = 60;

        CommonScene.LoadAlt("Intro");
    }

    void OnDestroy()
    {
        main = main == this ? null : main;
    }

    public static void Load(string target, Func<bool> completed, Action onComplete, bool showBar = true) 
    {
        main.StartCoroutine(main.LoadAnim(target, completed, onComplete, showBar) );
    }

    public IEnumerator LoadAnim(string target, Func<bool> completed, Action onComplete, bool showBar = true) 
    {
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(target, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        yield return Resources.UnloadUnusedAssets();
        yield return new WaitUntil(completed);
        if (onComplete != null) onComplete();
    }
}
