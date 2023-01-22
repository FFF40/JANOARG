using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Common : MonoBehaviour
{
    public static Common main;

    public LoadingBar LoadingBar;

    public void Awake()
    {
        main = this;
        CommonScene.LoadAlt("Song Select");
    }

    void OnDestroy()
    {
        main = main == this ? null : main;
    }
}
