using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JANOARG.Shared.Data.Files;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    /// <summary>A chart folder and the .japs file found directly inside it.</summary>
    internal readonly struct ChartEntry
    {
        public readonly string Dir;
        public readonly string JapsPath;

        public ChartEntry(string dir, string japsPath)
        {
            Dir = dir;
            JapsPath = japsPath;
        }
    }

    public class ChartScanner : MonoBehaviour
    {
        public static ChartScanner sMain;

        // ulong is used for hashes
        internal Dictionary<ulong, ChartEntry> JAPSDirs = new();

        private FileSystemWatcher _Watcher;
        private volatile bool _Dirty;
        private volatile bool _WatcherNeedsRestart;
        private string _ChartsDir;
        private DateTime _LastScan;
        private DateTime _LastPoll;

        // Chart targets decoded from each .japs, keyed by path and invalidated when its size/mtime change.
        // Lets a rescan re-parse only the .japs files that actually changed.
        private Dictionary<string, (long Length, long Ticks, string[] Targets)> _TargetCache = new();

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
                if (GetComponent<ParallelPollWatch>() == null)
                    gameObject.AddComponent<ParallelPollWatch>();
                Rescan();
                _LastPoll = DateTime.UtcNow;
                // LogFinding();
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
        public void MarkDirty() => _Dirty = true;

        public void Update()
        {
            if (_WatcherNeedsRestart)
            {
                _WatcherNeedsRestart = false;
                if (_Watcher != null)
                {
                    _Watcher.EnableRaisingEvents = false;
                    _Watcher.Dispose();
                    _Watcher = null;
                }
                StartWatching();
            }

            if (_Dirty)
            {
                _Dirty = false;
                var before = new HashSet<ulong>(JAPSDirs.Keys);
                Rescan();
                // LogFinding();
                var after = new HashSet<ulong>(JAPSDirs.Keys);
                if (!before.SetEquals(after))
                    RuntimeExternalManager.BackendError("Changes detected, reload to reimport");
            }
        }
        
        void LogFinding()
        {
            var lines = new List<string> { "[ChartScanner] Found the following:" };
            foreach (var kv in JAPSDirs)
                lines.Add($"  {kv.Key}: {kv.Value.Dir}");
            Debug.Log(string.Join("\n", lines));
        }

        void StartWatching()
        {
            if (string.IsNullOrEmpty(_ChartsDir) || !Directory.Exists(_ChartsDir)) return;
            try
            {
                _Watcher = new FileSystemWatcher(_ChartsDir)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                                 | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size,
                    Filter = "*.*",
                    InternalBufferSize = 65536,
                    EnableRaisingEvents = true
                };
                _Watcher.Created += (_, _) => _Dirty = true;
                _Watcher.Changed += (_, _) => _Dirty = true;
                _Watcher.Deleted += (_, _) => _Dirty = true;
                _Watcher.Renamed += (_, _) => _Dirty = true;
                _Watcher.Error += (_, e) => { _Dirty = true; _WatcherNeedsRestart = true; Debug.LogWarning($"[ChartScanner] Watcher error: {e.GetException()}"); };
            }
            catch (Exception e) { Debug.LogWarning($"Could not watch charts directory: {e}"); }
        }

        /// <summary>
        /// Rescans immediately (bypassing the throttle) and returns the current chart folders.
        /// Used by the importer so it shares this scanner's result instead of sweeping the disk itself.
        /// </summary>
        internal List<ChartEntry> ScanNow()
        {
            // The access callback in Awake may not have run (e.g. permission prompt); the importer has
            // already verified access before calling this, so resolve the directory ourselves.
            if (string.IsNullOrEmpty(_ChartsDir)) _ChartsDir = ChartDirProvider.GetChartsDirectoryPath();
            if (_Watcher == null) StartWatching();

            Rescan(force: true);
            // This scan is current; pending watcher events don't need another one.
            _Dirty = false;
            _LastPoll = DateTime.UtcNow;
            return JAPSDirs.Values.ToList();
        }

        void Rescan(bool force = false)
        {
            if (string.IsNullOrEmpty(_ChartsDir) || !Directory.Exists(_ChartsDir)) return;
            // Throttle to avoid spamming
            if (!force && (DateTime.UtcNow - _LastScan).TotalMilliseconds < 250) { _Dirty = true; return; }
            _LastScan = DateTime.UtcNow;
            _LastPoll = DateTime.UtcNow;

            var found = new Dictionary<ulong, ChartEntry>();
            var newCache = new Dictionary<string, (long Length, long Ticks, string[] Targets)>();
            try
            {
                foreach (string subDir in Directory.EnumerateDirectories(_ChartsDir))
                {
                    // depth 1 only: look for a JAPS file directly inside subDir
                    string japsPath = SongDataProvider.FindJapsFile(subDir);
                    if (japsPath == null)
                        continue;

                    found[HashChart(subDir, japsPath, newCache)] = new ChartEntry(subDir, japsPath);
                }
                _TargetCache = newCache;
            }
            catch (Exception e) { Debug.LogWarning($"Rescan failed: {e}"); }

            JAPSDirs = found;
        }

        // Hash the JAPS and every JAC it references: name + size + last-modified ticks.
        // The JAC list comes from the JAPS itself, so no directory listing is needed.
        ulong HashChart(string subDir, string japsPath, Dictionary<string, (long Length, long Ticks, string[] Targets)> newCache)
        {
            var japsInfo = new FileInfo(japsPath);
            // Seed with the folder name so copies of a chart folder (identical files) don't collide.
            ulong hash = (ulong)Path.GetFileName(subDir).GetHashCode();
            hash = MixFile(hash, japsInfo);

            foreach (string target in GetChartTargets(japsPath, japsInfo, newCache))
            {
                // Same resolution as PlayerScreen.LoadChart: Target may omit the .jac extension.
                string jacPath = Path.Combine(subDir, target);
                var jacInfo = new FileInfo(jacPath);
                if (!jacInfo.Exists && !jacPath.EndsWith(".jac", StringComparison.OrdinalIgnoreCase))
                    jacInfo = new FileInfo(jacPath + ".jac");
                if (jacInfo.Exists)
                    hash = MixFile(hash, jacInfo);
            }
            return hash;
        }

        // polynomial rolling hash
        static ulong MixFile(ulong hash, FileInfo info)
        {
            hash = hash * 131 + (ulong)info.Name.GetHashCode();
            hash = hash * 131 + (ulong)info.Length;
            hash = hash * 131 + (ulong)info.LastWriteTimeUtc.Ticks;
            return hash;
        }

        string[] GetChartTargets(string japsPath, FileInfo japsInfo, Dictionary<string, (long Length, long Ticks, string[] Targets)> newCache)
        {
            long length = japsInfo.Length;
            long ticks = japsInfo.LastWriteTimeUtc.Ticks;

            if (!_TargetCache.TryGetValue(japsPath, out var entry) || entry.Length != length || entry.Ticks != ticks)
            {
                string[] targets;
                try
                {
                    targets = JAPSDecoder.Decode(File.ReadAllText(japsPath)).Charts
                        .Select(c => c.Target)
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToArray();
                }
                catch (Exception e)
                {
                    // Undecodable JAPS: still tracked via its own hash; the import will report the failure.
                    Debug.LogWarning($"[ChartScanner] Could not read chart targets from {japsPath}: {e.Message}");
                    targets = Array.Empty<string>();
                }
                entry = (length, ticks, targets);
            }

            newCache[japsPath] = entry;
            return entry.Targets;
        }
    }
}
