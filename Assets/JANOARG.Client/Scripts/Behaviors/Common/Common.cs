using System;
using System.Collections;
using System.Globalization;
using JANOARG.Client.Data.Constant;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Common
{
    public class CommonSys : MonoBehaviour
    {
        public static CommonSys main;

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

        /** 
            <summary>
                Load a scene named <c>target</c>, wait until <c>completed</c> return true,
                and call <c>onComplete</c> when done.
            </summary>
        */
        public static Coroutine LoadScene(string target, Func<bool> completed, Action onComplete, bool showBar = true)
        {
            return main.StartCoroutine(main.LoadSceneAnim(target, completed, onComplete, showBar));
        }

        /** 
            <summary>
                Animation to be played when <c>LoadScene</c> is called
            </summary>
        */
        IEnumerator LoadSceneAnim(string target, Func<bool> completed, Action onComplete, bool showBar = true) 
        {
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(target, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            yield return Resources.UnloadUnusedAssets();
            yield return new WaitUntil(completed);
            if (onComplete != null) onComplete();
        }
    }
}
