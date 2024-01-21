using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Common : MonoBehaviour
{
    public static Common main;

    public Camera MainCamera;

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
}
