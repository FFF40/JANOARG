using System;
using System.Collections;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    internal sealed class ChartDirPermissionPoller : MonoBehaviour
    {
        private const float _POLL_INTERVAL = 0.25f;
        private const float _TIMEOUT = 120f;

        private static ChartDirPermissionPoller _Instance;

        public static void Poll(Func<bool> check, Action<bool> onComplete)
        {
            if (_Instance == null)
            {
                var host = new GameObject("[ChartDirPermissionPoller]")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                DontDestroyOnLoad(host);
                _Instance = host.AddComponent<ChartDirPermissionPoller>();
            }

            _Instance.StartCoroutine(_Instance.PollRoutine(check, onComplete));
        }

        private IEnumerator PollRoutine(Func<bool> check, Action<bool> onComplete)
        {
            float elapsed = 0f;

            while (elapsed < _TIMEOUT)
            {
                yield return new WaitForSecondsRealtime(_POLL_INTERVAL);
                elapsed += _POLL_INTERVAL;

                bool granted;
                try
                {
                    granted = check();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not poll storage access: {e}");
                    granted = false;
                }

                if (granted)
                {
                    onComplete?.Invoke(true);
                    yield break;
                }
            }

            onComplete?.Invoke(false);
        }
    }
}
