using System;
using System.Collections;
using System.Globalization;
using JANOARG.Client.Data.Constant;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JANOARG.Client.Behaviors.Common
{
    public class CommonSys : MonoBehaviour
    {
        public static CommonSys sMain;

        public Camera          MainCamera;
        public RectTransform   CommonCanvas;
        public CommonConstants Constants;

        public LoadingBar LoadingBar;
        public Storage    Preferences;
        public Storage    Storage;

        public void Awake()
        {
            sMain = this;

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

        private void OnDestroy()
        {
            sMain = sMain == this ? null : sMain;
        }

        /// <summary>
        /// Load a scene named <see cref="target"/>, wait until <see cref="completed"/> return true,
        /// and call <see cref="onComplete"/> when done.
        /// </summary>
        /// <param name="target">Target scene to load (in string)</param>
        /// <param name="completed">Function that will return true to signal completion</param>
        /// <param name="onComplete">Action to do when <see cref="completed"/> is true</param>
        /// <param name="showBar">Show loading bar (unused)</param>
        public static void LoadScene(string target, Func<bool> completed, Action onComplete, bool showBar = true)
        {
            sMain.StartCoroutine(sMain.LoadSceneAnimation(target, completed, onComplete, showBar));
        }

        /// <summary>
        /// Animation to be played when <see cref="LoadScene"/> is called
        /// </summary>
        /// <param name="target">Target scene to load (in string)</param>
        /// <param name="completed">Function that will return true to signal completion</param>
        /// <param name="onComplete">Action to do when <see cref="completed"/> is true</param>
        /// <param name="showBar">Show loading bar (unused)</param>
        /// <returns>Serves as a coroutine.</returns>
        public IEnumerator LoadSceneAnimation(string target, Func<bool> completed, Action onComplete, bool showBar = true)
        {
            yield return SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive);
            yield return Resources.UnloadUnusedAssets();
            yield return new WaitUntil(completed);

            if (onComplete != null) 
                onComplete();
        }
    }
}