using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    /// <summary>
    /// Off-thread polling fallback for <see cref="ChartScanner"/>. The OS file watcher can drop
    /// events (buffer overflow / platform quirks), so this periodically recomputes a signature of
    /// the charts directory on a background thread and marks the scanner dirty when it changes.
    /// </summary>
    public class ParallelPollWatch : MonoBehaviour
    {
        public float PollIntervalSeconds = 2f;

        private Thread _Thread;
        private volatile bool _Running;
        private ulong _PreviousSignature;
        private bool _HasPrevious;

        void OnEnable()
        {
            _Running = true;
            _Thread = new Thread(Loop) { IsBackground = true, Name = "ChartPoll" };
            _Thread.Start();
        }

        void OnDisable() => Stop();
        void OnDestroy() => Stop();

        void Stop()
        {
            _Running = false;
            if (_Thread != null && _Thread.IsAlive)
            {
                _Thread.Join(2500);
                _Thread = null;
            }
        }

        void Loop()
        {
            while (_Running)
            {
                try
                {
                    string dir = ChartDirProvider.GetChartsDirectoryPath();
                    ulong signature = ComputeSignature(dir);
                    if (_HasPrevious && signature != _PreviousSignature)
                    {
                        ChartScanner.sMain?.MarkDirty();
                    }
                    _PreviousSignature = signature;
                    _HasPrevious = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ParallelPollWatch] Poll failed: {e.Message}");
                }

                int waitMs = (int)(PollIntervalSeconds * 1000f);
                if (waitMs < 250) waitMs = 250;
                Thread.Sleep(waitMs);
            }
        }

        static ulong ComputeSignature(string root)
        {
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return 0UL;
            ulong hash = 5381UL;
            try
            {
                foreach (string path in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var info = new FileInfo(path);
                        hash = hash * 31 + (ulong)path.GetHashCode();
                        hash = hash * 31 + (ulong)info.Length;
                        hash = hash * 31 + (ulong)info.LastWriteTimeUtc.Ticks;
                    }
                    catch { /* skip unreadable */ }
                }
                foreach (string path in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var info = new DirectoryInfo(path);
                        hash = hash * 31 + (ulong)path.GetHashCode();
                        hash = hash * 31 + (ulong)info.LastWriteTimeUtc.Ticks;
                    }
                    catch { /* skip unreadable */ }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ParallelPollWatch] Enumerate failed: {e.Message}");
            }
            return hash;
        }
    }
}
