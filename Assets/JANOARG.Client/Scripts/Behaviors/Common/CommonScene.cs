using UnityEngine.SceneManagement;

namespace JANOARG.Client.Behaviors.Common
{
    /// <summary>
    /// Utility class for handling the Common scene.
    /// </summary>
    public static class CommonScene
    {
        public static bool isLoaded { get; private set; }

        /// <summary>
        /// Load the Common scene if it hasn't been loaded.
        /// </summary>
        public static void Load()
        {
            if (!isLoaded)
            {
                SceneManager.LoadScene("Common", LoadSceneMode.Additive);
                isLoaded = true;
            }
        }

        /// <summary>
        /// Called by the Common scene to load a scene when it's the first scene that is loaded.
        /// </summary>
        public static void LoadAlt(string targetScene)
        {
            if (!isLoaded)
            {
                SceneManager.LoadScene(targetScene, LoadSceneMode.Additive);
                isLoaded = true;
            }
        }
    }
}