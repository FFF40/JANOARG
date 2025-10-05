using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CommonScene
{
    public static bool isLoaded {get; private set;}

    public static void Load ()
    {
        if (!isLoaded)
        {
            SceneManager.LoadScene("Common", LoadSceneMode.Additive);
            isLoaded = true;
        }
    }

    public static void LoadAlt (string targetScene)
    {
        if (!isLoaded)
        {
            SceneManager.LoadScene(targetScene, LoadSceneMode.Additive);
            isLoaded = true;
        }
    }
    
}
