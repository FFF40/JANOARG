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
        
        private const bool _UNLIMITED_FPS = false; // Change before build
        private const int  _MAX_FPS       = 120;

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

            Application.targetFrameRate = _UNLIMITED_FPS 
                ? 1000 : _MAX_FPS;

            CommonScene.LoadAlt("Intro");
        }

        private void OnDestroy()
        {
            sMain = sMain == this ? null : sMain;
        }

        public static void Load(string target, Func<bool> completed, Action onComplete, bool showBar = true)
        {
            sMain.StartCoroutine(sMain.LoadAnim(target, completed, onComplete, showBar));
        }

        public IEnumerator LoadAnim(string target, Func<bool> completed, Action onComplete, bool showBar = true)
        {
            yield return SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive);
            yield return Resources.UnloadUnusedAssets();
            yield return new WaitUntil(completed);

            if (onComplete != null) onComplete();
        }
    }
}
