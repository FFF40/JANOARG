using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public class ChartScanner : MonoBehaviour
    {
        public static ChartScanner sMain;

        // ulong is used for hashes
        internal Dictionary<ulong, string> JAPSDirs = new();

        private FileSystemWatcher _Watcher;
        private volatile bool _Dirty;
        private string _ChartsDir;
        private DateTime _LastScan;

        public void Awake()
        {
            Debug.Log("[ChartScanner] Awake");
            sMain = this;
            ChartDirProvider.EnsureAccess(success =>
            {
                if (!success) { Debug.LogError("Failed to access chart directory"); return; }
                _ChartsDir = ChartDirProvider.GetChartsDirectoryPath();
                try
                {
                    if (!Directory.Exists(_ChartsDir)) Directory.CreateDirectory(_ChartsDir);
                }
                catch (Exception e) { Debug.LogWarning(e); }
                StartWatching();
                Rescan();
                LogFinding();
                Debug.Log($"[ChartScanner] Scanned {_ChartsDir}: {JAPSDirs.Count} chart folders");
            });
        }

        void OnDestroy()
        {
            if (_Watcher != null)
            {
                _Watcher.EnableRaisingEvents = false;
                _Watcher.Dispose();
                _Watcher = null;
            }
        }

        // Bind to CHARTS_FOLDER_NAME directory for any file changes, then trigger a rescan
        // Dig to only depth 1 of dir, push to JAPSDir if found a JAPS file within, otherwise bail and ignore
        // Kind of like osu!stable I guess??
        public void Update()
        {
            if (_Dirty)
            {
                _Dirty = false;
                Rescan();
                LogFinding();
            }
        }
        
        void LogFinding()
        {
            var lines = new List<string> { "[ChartScanner] Found the following:" };
            foreach (var kv in JAPSDirs)
                lines.Add($"  {kv.Key}: {kv.Value}");
            Debug.Log(string.Join("\n", lines));
        }

        void StartWatching()
        {
            if (string.IsNullOrEmpty(_ChartsDir) || !Directory.Exists(_ChartsDir)) return;
            try
            {
                _Watcher = new FileSystemWatcher(_ChartsDir)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                                 | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    Filter = "*.*",
                    EnableRaisingEvents = true
                };
                _Watcher.Created += (_, _) => _Dirty = true;
                _Watcher.Changed += (_, _) => _Dirty = true;
                _Watcher.Deleted += (_, _) => _Dirty = true;
                _Watcher.Renamed += (_, _) => _Dirty = true;
            }
            catch (Exception e) { Debug.LogWarning($"Could not watch charts directory: {e}"); }
        }

        void Rescan()
        {
            if (string.IsNullOrEmpty(_ChartsDir) || !Directory.Exists(_ChartsDir)) return;
            // Throttle to avoid spamming
            if ((DateTime.UtcNow - _LastScan).TotalMilliseconds < 250) { _Dirty = true; return; }
            _LastScan = DateTime.UtcNow;

            var found = new Dictionary<ulong, string>();
            try
            {
                foreach (string subDir in Directory.GetDirectories(_ChartsDir))
                {
                    // depth 1 only: look for a JAPS file directly inside subDir
                    bool hasJaps = false;
                    foreach (string _ in Directory.GetFiles(subDir, "*.japs", SearchOption.TopDirectoryOnly))
                    {
                        hasJaps = true;
                        break;
                    }

                    if (!hasJaps)
                        continue;

                    ulong hash = 0;
                    // Hash all JAC and JAPS files: name + size + last-modified ticks
                    foreach (string pattern in new[] { "*.jac", "*.japs" })
                    {
                        foreach (string file in Directory.GetFiles(subDir, pattern, SearchOption.TopDirectoryOnly))
                        {
                            var info = new FileInfo(file);
                            // polynomial rolling hash
                            hash = hash * 131 + (ulong)info.Name.GetHashCode();
                            hash = hash * 131 + (ulong)info.Length;
                            hash = hash * 131 + (ulong)info.LastWriteTimeUtc.Ticks;
                        }
                    }
                    found[hash] = subDir;
                }
            }
            catch (Exception e) { Debug.LogWarning($"Rescan failed: {e}"); }

            JAPSDirs = found;
        }
    }
}
