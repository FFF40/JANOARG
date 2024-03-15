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

    public LoadingBar LoadingBar;
    public Storage Storage;

    public void Awake()
    {
        main = this;

        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Storage = new Storage("save");
        Storage.Set("Count", Storage.Get("Count", 0) + 1);
        Debug.Log(Storage.Get("Count", 0));
        Storage.Save();

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
