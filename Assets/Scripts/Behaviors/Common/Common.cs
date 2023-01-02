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
    }

    void OnDestroy()
    {
        main = main == this ? null : main;
    }

}
