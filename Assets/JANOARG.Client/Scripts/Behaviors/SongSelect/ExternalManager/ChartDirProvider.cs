using System;
using System.IO;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public class ChartDirProvider
    {
        public const string CHARTS_FOLDER_NAME = "JANOARG Charts";

        public static string GetStorageRootDirectory()
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
            Path.Combine(GetStorageRootDirectory(), CHARTS_FOLDER_NAME);

        public static bool ChartsDirectoryExists() =>
            Directory.Exists(GetChartsDirectoryPath());

        public static void EnsureAccess(Action<bool> onComplete = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string path = GetChartsDirectoryPath();
            if (!Directory.Exists(path))
            {
                if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                {
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionGranted += _ => { TryCreateDirectory(path); onComplete?.Invoke(Directory.Exists(path)); };
                    callbacks.PermissionDenied += _ => onComplete?.Invoke(false);
                    Permission.RequestUserPermission(Permission.ExternalStorageWrite, callbacks);
                    return;
                }
                TryCreateDirectory(path);
                onComplete?.Invoke(Directory.Exists(path));
            }
            else
            {
                if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
                {
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionGranted += _ => onComplete?.Invoke(true);
                    callbacks.PermissionDenied += _ => onComplete?.Invoke(false);
                    Permission.RequestUserPermission(Permission.ExternalStorageRead, callbacks);
                    return;
                }
                onComplete?.Invoke(true);
            }
#elif UNITY_IOS
            // UNTESTABLE CODE, MIGHT NOT WORK
            TryCreateDirectory(GetChartsDirectoryPath());
            onComplete?.Invoke(true);
#else
            TryCreateDirectory(GetChartsDirectoryPath());
            onComplete?.Invoke(true);
#endif
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
