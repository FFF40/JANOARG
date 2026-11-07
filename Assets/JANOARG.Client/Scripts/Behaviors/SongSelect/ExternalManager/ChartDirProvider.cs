using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public static class ChartDirProvider
    {
        private const string _CHARTS_FOLDER_NAME = "JANOARG Charts";
        private const int _ALL_FILES_ACCESS_API = 30;

#if UNITY_ANDROID && !UNITY_EDITOR
        private static bool _AllFilesAccessPromptInFlight;
        private static readonly List<Action<bool>> _PendingAllFilesAccessCallbacks = new();
#endif

        private static string GetStorageRootDirectory()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var env = new AndroidJavaClass("android.os.Environment"))
                using (var file = env.CallStatic<AndroidJavaObject>("getExternalStorageDirectory"))
                {
                    return file.Call<string>("getAbsolutePath");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not resolve external storage directory: {e}");
            }
#elif UNITY_IOS
            // UNTESTABLE CODE, MIGHT NOT WORK
            try
            {
                // iOS apps are sandboxed; use the app's Documents directory.
                return Application.persistentDataPath;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not resolve iOS storage directory: {e}");
            }
#endif
            return Application.persistentDataPath;
        }

        public static string GetChartsDirectoryPath() =>
            Path.Combine(GetStorageRootDirectory(), _CHARTS_FOLDER_NAME);

        public static bool ChartsDirectoryExists() =>
            Directory.Exists(GetChartsDirectoryPath());

        public static void EnsureAccess(Action<bool> onComplete = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            int sdk = GetSdkInt();
            if (sdk >= _ALL_FILES_ACCESS_API)
            {
                EnsureAllFilesAccess(onComplete);
                return;
            }

            EnsureLegacyStorageAccess(onComplete);
#elif UNITY_IOS
            // UNTESTABLE CODE, MIGHT NOT WORK
            TryCreateDirectory(GetChartsDirectoryPath());
            onComplete?.Invoke(true);
#else
            TryCreateDirectory(GetChartsDirectoryPath());
            onComplete?.Invoke(true);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static int GetSdkInt()
        {
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    return version.GetStatic<int>("SDK_INT");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not determine Android API level: {e}");
                return 0;
            }
        }

        private static bool HasAllFilesAccess()
        {
            try
            {
                using (var env = new AndroidJavaClass("android.os.Environment"))
                    return env.CallStatic<bool>("isExternalStorageManager");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not check All files access: {e}");
                return false;
            }
        }

        private static void EnsureAllFilesAccess(Action<bool> onComplete)
        {
            if (HasAllFilesAccess())
            {
                onComplete?.Invoke(GrantAllFilesAccess());
                return;
            }

            if (onComplete != null)
                _PendingAllFilesAccessCallbacks.Add(onComplete);

            if (_AllFilesAccessPromptInFlight)
                return;

            if (!TryOpenAllFilesAccessSettings())
            {
                CompleteAllFilesAccessRequest(false);
                return;
            }

            _AllFilesAccessPromptInFlight = true;

            ChartDirPermissionPoller.Poll(() =>
            {
                if (!HasAllFilesAccess())
                    return false;

                return GrantAllFilesAccess();
            }, CompleteAllFilesAccessRequest);
        }

        private static bool GrantAllFilesAccess()
        {
            string path = GetChartsDirectoryPath();
            TryCreateDirectory(path);
            return CanReadDirectory(path);
        }

        private static void CompleteAllFilesAccessRequest(bool success)
        {
            _AllFilesAccessPromptInFlight = false;

            Action<bool>[] callbacks = _PendingAllFilesAccessCallbacks.ToArray();
            _PendingAllFilesAccessCallbacks.Clear();

            foreach (Action<bool> callback in callbacks)
            {
                try
                {
                    callback?.Invoke(success);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Storage access callback failed: {e}");
                }
            }
        }

        private static bool TryOpenAllFilesAccessSettings()
        {
            if (StartSettingsActivity("android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION", true))
                return true;

            return StartSettingsActivity("android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION", false);
        }

        private static bool StartSettingsActivity(string action, bool withPackageData)
        {
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var intent = new AndroidJavaObject("android.content.Intent", action))
                {
                    if (withPackageData)
                    {
                        using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                        using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", "package:" + activity.Call<string>("getPackageName")))
                        {
                            intent.Call<AndroidJavaObject>("setData", uri);
                        }
                    }

                    activity.Call("startActivity", intent);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not open storage settings '{action}': {e}");
                return false;
            }
        }

        private static void EnsureLegacyStorageAccess(Action<bool> onComplete)
        {
            string path = GetChartsDirectoryPath();
            bool exists = Directory.Exists(path);
            string permission = exists ? Permission.ExternalStorageRead : Permission.ExternalStorageWrite;

            if (Permission.HasUserAuthorizedPermission(permission))
            {
                CompleteLegacyAccess(path, onComplete);
                return;
            }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => CompleteLegacyAccess(path, onComplete);
            callbacks.PermissionDenied += _ => onComplete?.Invoke(false);
            Permission.RequestUserPermission(permission, callbacks);
        }

        private static void CompleteLegacyAccess(string path, Action<bool> onComplete)
        {
            if (!Directory.Exists(path))
                TryCreateDirectory(path);

            onComplete?.Invoke(CanReadDirectory(path));
        }
#endif

        private static bool CanReadDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return false;

                using (var entries = Directory.EnumerateFileSystemEntries(path).GetEnumerator())
                    entries.MoveNext();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read charts directory '{path}': {e}");
                return false;
            }
        }

        static void TryCreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not create charts directory '{path}': {e}");
            }
        }
    }
}
