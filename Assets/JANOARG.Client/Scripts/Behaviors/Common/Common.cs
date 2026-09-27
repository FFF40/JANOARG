using System;
using System.Collections;
using System.Globalization;
using JANOARG.Client.Data.Constant;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.Rendering;
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

        [Header("Rendering")]
        [Tooltip("Dynamic-resolution scale for the 3D scene (1 = native). Screen-space overlay UI is " +
                 "unaffected. Drop it on fillrate-bound GPUs (e.g. 0.7 on a low-end Adreno) to trade " +
                 "a little softness for GPU headroom.")]
        [Range(0.5f, 1f)]
        public float RenderScale = 1f;

        // Frame-rate ceiling for the build. The effective target is further clamped to the
        // display's reported refresh below — asking for 120 on a 60 Hz phone only burns GPU
        // headroom and makes the Android frame pacer more likely to miss a slot (which drops it
        // to half rate). Set _MAX_FPS = 60 to force 60 on high-refresh devices.
        private const bool _UNLIMITED_FPS = false; // Change before build
        private const int  _MAX_FPS       = 120;

        // Runs before the first scene loads and before Unity's Android frame pacing (Swappy /
        // Choreographer) is configured, so the target it reads is not the Android default of 30.
        // Awake runs too late — by then the swap interval is already set for 30 fps, and re-setting
        // Application.targetFrameRate afterwards doesn't change it.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void EarlyFrameRateInit()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _MAX_FPS;
        }

        public void Awake()
        {
            sMain = this;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            Storage = new Storage("save");
            int count = Storage.Get("STAT:Count", 0) + 1;
            Storage.Set("STAT:Count", count);
            Storage.Save();

            Preferences = new Storage("prefs");

            // vSyncCount must be 0 for targetFrameRate to take effect;
            // if vsync is on Unity ignores targetFrameRate entirely.
            // -1 = truly unlimited (vs the previous 1000 cap).
            QualitySettings.vSyncCount = 0;

            // Match the target to the panel's actual rate. On Android the vsync scheduler decides the
            // presented rate, so asking for more than the panel reports (or letting Unity's target
            // drift above it) can push the pacer onto a higher grid it then half-rates.
            int displayHz = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

            _TargetHz = displayHz > 0 ? Mathf.Min(_MAX_FPS, displayHz) : _MAX_FPS;

            // Exact refresh rate. The +1 "off the vsync" trick is a workaround for the non-Swappy
            // limiter; with Optimized Frame Pacing on, Swappy wants an exact divisor of the refresh.
            Application.targetFrameRate = _UNLIMITED_FPS ? -1 : _TargetHz;

            // Unity resets Application.targetFrameRate when a scene loads (a known Android behaviour):
            // the app runs fine until the first scene transition, then locks to the OS default 30. So
            // re-assert it on every scene load.
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Dynamic resolution: opt the render camera in and scale the 3D render target. No-op at
            // 1.0; lower RenderScale on fillrate-bound devices. It scales only the 3D scene — the
            // screen-space-overlay HUD stays full resolution, so only lanes/effects soften.
            if (MainCamera != null)
                MainCamera.allowDynamicResolution = true;

            ScalableBufferManager.ResizeBuffers(RenderScale, RenderScale);

#if UNITY_ANDROID && !UNITY_EDITOR
            RequestDisplayRefresh(Application.targetFrameRate);
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"[CommonSys] FPS target={Application.targetFrameRate} (max={_MAX_FPS}, display={displayHz}Hz) " +
                $"vsync={QualitySettings.vSyncCount} res={Screen.currentResolution.width}x{Screen.currentResolution.height} " +
                $"renderScale={RenderScale:0.00} api={SystemInfo.graphicsDeviceType} gpu={SystemInfo.graphicsDeviceName}");
#endif

            CommonScene.LoadAlt("Intro");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (sMain == this)
                sMain = null;
        }

        // The effective target computed once in Awake; re-asserted on every scene load.
        private int _TargetHz = 60;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Application.targetFrameRate = _UNLIMITED_FPS ? -1 : _TargetHz;

#if UNITY_ANDROID && !UNITY_EDITOR
            RequestDisplayRefresh(_TargetHz);
#endif
        }

        // Application.targetFrameRate sets the render-loop target but does not switch the panel's
        // display mode, and on Android it's the vsync scheduler that decides the presented rate.
        // Match both the window's display mode and the game surface's requested frame rate to the
        // panel's actual rate. Everything is best-effort: API < 23 / OEM restrictions are caught.
#if UNITY_ANDROID && !UNITY_EDITOR
        private void RequestDisplayRefresh(int targetHz)
        {
            try
            {
                var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = player.GetStatic<AndroidJavaObject>("currentActivity");

                int bestModeId = -1;
                float bestHz = 0f;

                using (var windowManager = activity.Call<AndroidJavaObject>("getWindowManager"))
                using (var display = windowManager.Call<AndroidJavaObject>("getDefaultDisplay"))
                {
                    IntPtr modes = AndroidJNI.CallObjectMethod(
                        display.GetRawObject(),
                        AndroidJNIHelper.GetMethodID(display.GetRawClass(), "getSupportedModes", "()[Landroid/view/Display$Mode;"),
                        new jvalue[0]);

                    if (modes != IntPtr.Zero)
                    {
                        int count = AndroidJNI.GetArrayLength(modes);

                        for (int i = 0; i < count; i++)
                        {
                            IntPtr mode = AndroidJNI.GetObjectArrayElement(modes, i);
                            IntPtr modeClass = AndroidJNI.GetObjectClass(mode);

                            float hz = AndroidJNI.CallFloatMethod(
                                mode, AndroidJNIHelper.GetMethodID(modeClass, "getRefreshRate", "()F"), new jvalue[0]);

                            // Pick the supported mode closest to the target rate.
                            if (bestModeId < 0 || Mathf.Abs(hz - targetHz) < Mathf.Abs(bestHz - targetHz))
                            {
                                bestHz = hz;
                                bestModeId = AndroidJNI.CallIntMethod(
                                    mode, AndroidJNIHelper.GetMethodID(modeClass, "getModeId", "()I"), new jvalue[0]);
                            }

                            AndroidJNI.DeleteLocalRef(mode);
                            AndroidJNI.DeleteLocalRef(modeClass);
                        }

                        AndroidJNI.DeleteLocalRef(modes);
                    }
                }

                // Resolve window/attrs here, then apply on the Android UI thread: setAttributes
                // touches the view hierarchy, so calling it from Unity's main thread throws
                // CalledFromWrongThread. The runnable runs asynchronously, so window/attrs must
                // outlive this scope and are disposed inside the runnable.
                AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow");
                AndroidJavaObject attrs = window.Call<AndroidJavaObject>("getAttributes");

                if (bestModeId >= 0)
                    attrs.Set("preferredDisplayModeId", bestModeId);
                else
                    attrs.Set("preferredRefreshRate", (float)targetHz);

                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        window.Call("setAttributes", attrs);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[CommonSys] Display-mode apply failed: {e.Message}");
                    }
                    finally
                    {
                        attrs.Dispose();
                        window.Dispose();
                    }
                }));

                // And assert the frame rate on the game surface — the path Android 11+ actually
                // honours (preferredDisplayModeId is the old window-level one and is often ignored).
                StartCoroutine(ApplySurfaceFrameRateRoutine(activity, targetHz));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CommonSys] Display refresh request failed: {e.Message}");
            }
        }

        // The SurfaceView's Surface may not exist at Awake / right after a scene load, so retry
        // briefly until it's valid. Re-invoked on each scene load (see OnSceneLoaded).
        private IEnumerator ApplySurfaceFrameRateRoutine(AndroidJavaObject activity, int hz)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                bool applied = false;

                try
                {
                    applied = ApplySurfaceFrameRate(activity, hz);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CommonSys] Surface.setFrameRate failed: {e.Message}");
                }

                if (applied)
                    yield break;

                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        private static bool ApplySurfaceFrameRate(AndroidJavaObject activity, int hz)
        {
            using (var window = activity.Call<AndroidJavaObject>("getWindow"))
            using (var decor = window.Call<AndroidJavaObject>("getDecorView"))
            {
                AndroidJavaObject surfaceView = FindSurfaceView(decor);

                if (surfaceView == null) return false;

                using (var holder = surfaceView.Call<AndroidJavaObject>("getHolder"))
                using (var surface = holder.Call<AndroidJavaObject>("getSurface"))
                {
                    if (surface == null || !surface.Call<bool>("isValid")) return false;

                    // FRAME_RATE_COMPATIBILITY_FIXED_SOURCE = 1
                    surface.Call("setFrameRate", (float)hz, 1);
                    return true;
                }
            }
        }

        private static AndroidJavaObject FindSurfaceView(AndroidJavaObject view)
        {
            if (view == null) return null;

            // Duck-type on getHolder(): only SurfaceView (and its subclasses) expose it. Matched this
            // way rather than by class name because Unity's rendering surface is an obfuscated
            // com.unity3d.player.* subclass, and Class.isInstance can't be called through
            // AndroidJavaObject.Call (its JNI signature uses the argument's concrete class).
            try
            {
                using (var holder = view.Call<AndroidJavaObject>("getHolder"))
                    if (holder != null)
                        return view;
            }
            catch
            {
                // not a SurfaceView
            }

            int childCount;
            try
            {
                childCount = view.Call<int>("getChildCount");
            }
            catch
            {
                return null; // not a ViewGroup
            }

            for (int i = 0; i < childCount; i++)
            {
                AndroidJavaObject found = FindSurfaceView(view.Call<AndroidJavaObject>("getChildAt", i));
                if (found != null) return found;
            }

            return null;
        }

#endif

        public static void Load(string target, Func<bool> completed, Action onComplete, bool showBar = true)
        {
            sMain.StartCoroutine(sMain.LoadAnim(target, completed, onComplete, showBar));
        }

        public IEnumerator LoadAnim(string target, Func<bool> completed, Action onComplete, bool showBar = true)
        {
            yield return SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive);
            yield return Resources.UnloadUnusedAssets();
            if (completed != null)
                yield return new WaitUntil(completed);

            if (onComplete != null) onComplete();
        }
    }
}
