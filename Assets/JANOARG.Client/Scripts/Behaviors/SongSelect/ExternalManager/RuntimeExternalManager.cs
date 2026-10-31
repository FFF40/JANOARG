using System;
using System.Collections;
using System.IO;
using System.Linq;
using JANOARG.Client.Behaviors.SongSelect;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.SongSelect.Map;
using JANOARG.Client.Data.Playlist;
using JANOARG.Client.UI.Modal;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils.Animation;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public class RuntimeExternalManager : MonoBehaviour
    {
        public static RuntimeExternalManager sMain;

        // Text overlay handling
        public TMP_Text Messages;
        public TMP_Text ImportProgress;

        // Grouping for said overlay for unified hide/show animation
        public GameObject Texts;

        public ModalActionButton ReloadButton;

        public static System.Collections.Generic.HashSet<string> sExternalSongIDs = new();
        private static readonly System.Collections.Generic.Dictionary<string, ulong> sr_LastExternalHashes = new();
        // In-session cache of loaded external songs, keyed by folder id, so re-entering a playlist
        // can re-add them without a full reimport/reload of their assets.
        private static readonly System.Collections.Generic.Dictionary<string, (ulong Hash, PlayableSong Song)> sr_ExternalSongCache = new();

        private static readonly System.Collections.Generic.Queue<string> sr_BackendQueue = new();
        private static string s_currentBackendError;
        private static bool s_canReload;

        public static void BackendError(string msg)
        {
            sr_BackendQueue.Enqueue(msg);
            if (s_currentBackendError == null)
                ShowNextBackendError();
        }

        private static void ShowNextBackendError()
        {
            if (sr_BackendQueue.Count == 0) { s_currentBackendError = null; HideMessages(); return; }
            s_currentBackendError = sr_BackendQueue.Dequeue();
            if (sMain != null && sMain.Messages != null)
            {
                string block = $"[BACKEND]\n{s_currentBackendError}";
                if (string.IsNullOrEmpty(sMain.Messages.text))
                    sMain.Messages.text = block;
                else
                    sMain.Messages.text += "\n" + block;
                sMain.Messages.gameObject.SetActive(true);
                if (sMain.ImportProgress != null) sMain.ImportProgress.gameObject.SetActive(false);
            }
            Debug.LogWarning($"[BACKEND] {s_currentBackendError}");
        }

        public static void DismissBackendError()
        {
            s_currentBackendError = null;
            ShowNextBackendError();
        }

        public static void SetProgress(string text, bool newline = true)
        {
            if (sMain == null || sMain.ImportProgress == null) return;
            if (string.IsNullOrEmpty(sMain.ImportProgress.text))
                sMain.ImportProgress.text = text;
            else
                sMain.ImportProgress.text += newline ? "\n" + text : text;
            sMain.ImportProgress.gameObject.SetActive(true);
            HideMessages();
        }

        public static void ClearProgress()
        {
            if (sMain == null || sMain.ImportProgress == null) return;
            sMain.ImportProgress.text = "";
        }

        public static void SetMessage(string text)
        {
            if (sMain == null || sMain.Messages == null) return;
            if (string.IsNullOrEmpty(sMain.Messages.text))
                sMain.Messages.text = text;
            else
                sMain.Messages.text += "\n" + text;
            sMain.Messages.gameObject.SetActive(true);
            if (sMain.ImportProgress != null) sMain.ImportProgress.gameObject.SetActive(false);
        }

        public static void ClearMessages()
        {
            if (sMain == null || sMain.Messages == null) return;
            sMain.Messages.text = "";
            sMain.Messages.gameObject.SetActive(false);
        }

        private static void HideMessages()
        {
            if (sMain != null && sMain.Messages != null)
                sMain.Messages.gameObject.SetActive(false);
        }

        public static System.Collections.Generic.HashSet<AudioClip> sExternalClips = new();
        public static System.Collections.Generic.HashSet<Texture2D> sExternalTextures = new();

        private RectTransform _TextsRt;
        private CanvasGroup _TextsCg;
        private Coroutine _TextsAnim;
        private bool _TextsVisible;

        private RectTransform _ReloadRt;
        private CanvasGroup _ReloadCg;
        private Coroutine _ReloadAnim;
        private bool _ReloadVisible;

        public void Awake()
        {
            sMain = this;
            s_canReload = false;
            if (Messages != null) Messages.raycastTarget = false;
            if (ImportProgress != null) ImportProgress.raycastTarget = false;

            if (Texts != null)
            {
                _TextsRt = Texts.GetComponent<RectTransform>();
                _TextsCg = Texts.GetComponent<CanvasGroup>();
                if (_TextsCg == null) _TextsCg = Texts.AddComponent<CanvasGroup>();
            }

            if (ReloadButton != null)
            {
                _ReloadRt = ReloadButton.GetComponent<RectTransform>();
                _ReloadCg = ReloadButton.GetComponent<CanvasGroup>();
                if (_ReloadCg == null) _ReloadCg = ReloadButton.gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Start()
        {
            if (ReloadButton != null)
                ReloadButton.gameObject.SetActive(false);
            if (ReloadButton != null && ReloadButton.Button != null)
                ReloadButton.Button.onClick.AddListener(Reload);
            StartCoroutine(Run());
        }

        public void Update()
        {
            var screen = SongSelectScreen.sMain;
            bool isMapView = screen != null && screen.IsMapView;

            // Text overlay: animate show/hide on view toggle (mirrored horizontally vs Reload).
            bool wantTextsVisible = isMapView && Texts != null
                && ((Messages != null && Messages.text.Length > 0) || (ImportProgress != null && ImportProgress.text.Length > 0));
            if (wantTextsVisible != _TextsVisible)
            {
                _TextsVisible = wantTextsVisible;
                if (_TextsAnim != null) StopCoroutine(_TextsAnim);
                _TextsAnim = StartCoroutine(wantTextsVisible ? ShowTextsAnim() : HideTextsAnim());
            }

            // Reload button: also animate on view toggle (but only when reload is available).
            bool wantReloadVisible = isMapView && s_canReload && ReloadButton != null;
            if (wantReloadVisible != _ReloadVisible)
            {
                _ReloadVisible = wantReloadVisible;
                if (_ReloadAnim != null) StopCoroutine(_ReloadAnim);
                _ReloadAnim = StartCoroutine(wantReloadVisible ? ShowReloadButton() : HideReloadButton());
            }
        }

        private IEnumerator ShowTextsAnim()
        {
            if (Texts == null || _TextsRt == null || _TextsCg == null) yield break;

            Texts.SetActive(true);

            Vector2 originalPos = _TextsRt.anchoredPosition;
            _TextsRt.anchoredPosition = new Vector2(originalPos.x - 40, originalPos.y);
            _TextsCg.alpha = 0f;

            yield return Ease.Animate(0.3f, a =>
            {
                float e = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                _TextsRt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x - 40, originalPos.x, e), originalPos.y);
                _TextsCg.alpha = e;
            });
        }

        private IEnumerator HideTextsAnim()
        {
            if (Texts == null || _TextsRt == null || _TextsCg == null) yield break;
            if (!Texts.activeSelf) yield break;

            Vector2 originalPos = _TextsRt.anchoredPosition;
            float startAlpha = _TextsCg.alpha;

            yield return Ease.Animate(0.2f, EaseFunction.Cubic, EaseMode.In, a =>
            {
                _TextsRt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x, originalPos.x - 40, a), originalPos.y);
                _TextsCg.alpha = Mathf.Lerp(startAlpha, 0f, a);
            });

            Texts.SetActive(false);
            _TextsCg.alpha = startAlpha;
            _TextsRt.anchoredPosition = originalPos;
        }

        private Coroutine _ReimportCoroutine;

        public void Reload()
        {
            Debug.Log("[RuntimeExternalManager] Reload clicked");
            if (SongSelectScreen.sMain != null && SongSelectScreen.sMain.IsAnimating) return;

            // Only allow one reimport coroutine at a time.
            if (_ReimportCoroutine != null) StopCoroutine(_ReimportCoroutine);
            _ReimportCoroutine = StartCoroutine(ReimportRoutine());
        }

        private IEnumerator ReimportRoutine()
        {
            var screen = SongSelectScreen.sMain;
            if (screen == null) yield break;

            // Disable button immediately (even if we hide it due to list view).
            if (ReloadButton != null && ReloadButton.Button != null)
                ReloadButton.Button.interactable = false;

            // Hide actions + profile bar exactly like playlist switching.
            StartCoroutine(Ease.Animate(0.3f, EaseFunction.Cubic, EaseMode.Out, t =>
            {
                ProfileBar.sMain?.SetVisibility(1 - t);
                screen.LerpActions(1 - t);
            }));

            // Also hide the reload button with a matching slide+fade.
            yield return HideReloadButton();

            // External has no playlist map item target; best-effort target is this RuntimeExternalManager.
            if (MapManager.sMain != null)
                yield return screen.LeaveInAnim(transform);

            // Run reimport.
            yield return Run();

            // Show actions + profile bar like playlist switching.
            StartCoroutine(Ease.Animate(0.3f, EaseFunction.Cubic, EaseMode.Out, t =>
            {
                ProfileBar.sMain?.SetVisibility(t);
                screen.LerpActions(t);
            }));

            // Reload button "show" is already implemented.
            yield return ShowReloadButton();

            // Give the show animation a moment before re-enabling.
            yield return new WaitForSeconds(0.05f);

            if (ReloadButton != null && ReloadButton.Button != null)
                ReloadButton.Button.interactable = true;

            _ReimportCoroutine = null;
        }

        private IEnumerator HideReloadButton()
        {
            if (ReloadButton == null) yield break;

            // If we're in list view, the button is already hidden.
            if (SongSelectScreen.sMain != null && !SongSelectScreen.sMain.IsMapView)
            {
                ReloadButton.gameObject.SetActive(false);
                yield break;
            }

            if (_ReloadRt == null || _ReloadCg == null)
                yield break;

            Vector2 originalPos = _ReloadRt.anchoredPosition;

            // If it isn't currently active, nothing to animate.
            if (!ReloadButton.gameObject.activeSelf)
                yield break;

            float startAlpha = _ReloadCg.alpha;

            yield return Ease.Animate(0.2f, EaseFunction.Cubic, EaseMode.In, t =>
            {
                _ReloadRt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x, originalPos.x + 40, t), originalPos.y);
                _ReloadCg.alpha = Mathf.Lerp(startAlpha, 0f, t);
            });

            ReloadButton.gameObject.SetActive(false);
            _ReloadCg.alpha = startAlpha;
            _ReloadRt.anchoredPosition = originalPos;
        }



        IEnumerator ShowReloadButton()
        {
            if (ReloadButton == null || SongSelectScreen.sMain == null || !SongSelectScreen.sMain.IsMapView)
                yield break;

            if (_ReloadRt == null || _ReloadCg == null)
                yield break;

            Vector2 originalPos = _ReloadRt.anchoredPosition;
            ReloadButton.gameObject.SetActive(true);
            _ReloadRt.anchoredPosition = new Vector2(originalPos.x + 40, originalPos.y);
            _ReloadCg.alpha = 0f;

            yield return Ease.Animate(0.3f, a =>
            {
                float e = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                _ReloadRt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x + 40, originalPos.x, e), originalPos.y);
                _ReloadCg.alpha = e;
            });
        }

        IEnumerator Run()
        {
            // Wait for SongSelectScreen
            yield return new WaitUntil(() => SongSelectScreen.sMain != null);
            
            ClearProgress();
            ClearMessages();

            SetProgress("Checking file permissions - ");
            // Ensure access to charts directory
            bool? accessed = null;
            ChartDirProvider.EnsureAccess(r => { accessed = r;});
            yield return new WaitUntil(() => accessed.HasValue);

            if (accessed.HasValue && accessed.Value)
                SetProgress("OK", false);
            else if (accessed.HasValue && !accessed.Value)
            {
                SetProgress("FAIL", false);
                SetProgress("Unable to access the directory. Make sure the permission is sufficient");
                yield break;
            }

            string root = ChartDirProvider.GetChartsDirectoryPath();

            var screen = SongSelectScreen.sMain;

            SetProgress("Checking for directory existence - ");
            // Backend: permission/directory checks (high-priority, one at a time)
            if (!ChartDirProvider.ChartsDirectoryExists())
            {
                SetProgress("FAIL", false);
                SetProgress("Unable to access the directory. Make sure the permission is sufficient");
                yield break;
            }
            SetProgress("OK", false);
            SetProgress("Scanning directory.");

            // ChartScanner owns directory scanning; take its fresh result rather than sweeping the disk again.
            var dirs = new System.Collections.Generic.List<(string dir, string japsPath)>();
            if (ChartScanner.sMain != null)
            {
                foreach (var entry in ChartScanner.sMain.ScanNow())
                    dirs.Add((entry.Dir, entry.JapsPath));
            }
            else
            {
                // No scanner in the scene: fall back to a local sweep.
                foreach (string d in Directory.EnumerateDirectories(root))
                {
                    string japs = SongDataProvider.FindJapsFile(d);
                    if (japs != null) dirs.Add((d, japs));
                }
            }

            SetProgress($"Found {dirs.Count} chart(s). Importing,");

            // Build a current id -> hash map so we can detect deletions and "same but changed" charts.
            var curHashes = new System.Collections.Generic.Dictionary<string, ulong>();
            if (ChartScanner.sMain != null)
            {
                foreach (var kv in ChartScanner.sMain.JAPSDirs)
                {
                    string hid = Path.GetFileName(kv.Value.Dir);
                    if (!curHashes.ContainsKey(hid)) curHashes[hid] = kv.Key;
                }
            }
            else
            {
                foreach (var (dir, _) in dirs)
                {
                    string hid = Path.GetFileName(dir);
                    if (!curHashes.ContainsKey(hid)) curHashes[hid] = 0UL;
                }
            }

            // Handle deletions: charts that were external before but are gone from disk now.
            int deletedCount = 0;
            var deletedIds = new System.Collections.Generic.List<string>();
            foreach (var id in sr_LastExternalHashes.Keys)
                if (!curHashes.ContainsKey(id)) deletedIds.Add(id);
            foreach (var id in sExternalSongIDs)
                if (!curHashes.ContainsKey(id) && !deletedIds.Contains(id)) deletedIds.Add(id);
            foreach (var id in deletedIds)
            {
                deletedCount++;
                screen.LoadedSongs.RemoveAll(ps => ps.ID == id);
                screen.PlayableSongByID.Remove(id);
                screen.PlaylistSongByID.Remove(id);
                sExternalSongIDs.Remove(id);
                sr_LastExternalHashes.Remove(id);
                sr_ExternalSongCache.Remove(id);
            }

            var pending = new System.Collections.Generic.List<(string id, PlayableSong song, bool changed)>();
            int i = 0;
            int addedCount = 0;
            int changedCount = 0;
            int unchangedCount = 0;
            foreach (var (dir, japsPath) in dirs)
            {
                i++;

                string id = Path.GetFileName(dir);
                ulong curHash = curHashes.TryGetValue(id, out var ch) ? ch : 0UL;
                bool wasExternal = sr_LastExternalHashes.ContainsKey(id);

                if (wasExternal)
                {
                    ulong prevHash = sr_LastExternalHashes[id];
                    if (prevHash == curHash)
                    {
                        if (screen.PlayableSongByID.ContainsKey(id))
                        {
                            unchangedCount++;
                            SetProgress($"({i}/{dirs.Count}) Skipped {id} (already loaded)");
                            continue;
                        }
                        if (sr_ExternalSongCache.TryGetValue(id, out var cached) && cached.Hash == curHash)
                        {
                            unchangedCount++;
                            pending.Add((id, cached.Song, false));
                            SetProgress($"({i}/{dirs.Count}) Re-added {id} (cached)");
                            continue;
                        }
                        // Cache miss: fall through and reload.
                    }

                    // "same but changed": reload it.
                    PlayableSong loaded = null;
                    yield return SongDataProvider.sMain != null
                        ? StartCoroutine(SongDataProvider.sMain.LoadSongRoutine(dir, japsPath, s => loaded = s))
                        : null;

                    if (loaded == null)
                    {
                        BackendError($"Unable to import {id}");
                        continue;
                    }

                    changedCount++;
                    sr_ExternalSongCache[id] = (curHash, loaded);
                    pending.Add((id, loaded, true));
                    SetProgress($"({i}/{dirs.Count}) Imported {id}");
                }
                else if (screen.PlayableSongByID.ContainsKey(id))
                {
                    unchangedCount++;
                    SetProgress($"({i}/{dirs.Count}) Skipped {id} (already loaded)");
                    continue;
                }
                else
                {
                    PlayableSong loaded = null;
                    yield return SongDataProvider.sMain != null
                        ? StartCoroutine(SongDataProvider.sMain.LoadSongRoutine(dir, japsPath, s => loaded = s))
                        : null;

                    if (loaded == null)
                    {
                        BackendError($"Unable to import {id}");
                        continue;
                    }

                    addedCount++;
                    sr_ExternalSongCache[id] = (curHash, loaded);
                    pending.Add((id, loaded, false));
                    SetProgress($"({i}/{dirs.Count}) Imported {id}");
                }
            }

            SetProgress("Import complete. Updating Database.");

            foreach (var (id, song, changed) in pending)
            {
                if (changed)
                {
                    screen.LoadedSongs.RemoveAll(ps => ps.ID == id);
                    screen.PlayableSongByID.Remove(id);
                    screen.PlaylistSongByID.Remove(id);
                }
                var playlistSong = new PlaylistSong { ID = id, RevealConditions = Array.Empty<GameConditional>(), UnlockConditions = Array.Empty<GameConditional>() };
                screen.LoadedSongs.Add(playlistSong);
                screen.PlayableSongByID[id] = song;
                screen.PlaylistSongByID[id] = playlistSong;
                sExternalSongIDs.Add(id);
            }

            screen.ListView.UpdateSort();
            screen.ListView.UpdateListItems(screen);
            screen.ListView.IsDirty = false;

            s_canReload = true;

            yield return StartCoroutine(ShowReloadButton());

            ClearMessages();

            int totalFound = dirs.Count;

            SetMessage($"Found {totalFound} chart(s)!");

            var updateParts = new System.Collections.Generic.List<string>();
            if (deletedCount > 0)   updateParts.Add($"Deleted {deletedCount} chart(s)");
            if (changedCount > 0)   updateParts.Add($"{changedCount} chart(s) modified");
            if (addedCount > 0)     updateParts.Add($"Added {addedCount} chart(s)");
            
            if (unchangedCount > 0 && unchangedCount != totalFound)
                updateParts.Add($"{unchangedCount} chart(s) unchanged");

            string updateString;

            // Join all state changes into a single string if available;
            // otherwise "no changes".
            updateString = updateParts.Count != 0 
                ? string.Join(", ", updateParts) : "No changes";

            updateString += " since last reload.";
            

            SetMessage(updateString);

            sr_LastExternalHashes.Clear();
            foreach (var kv in curHashes) sr_LastExternalHashes[kv.Key] = kv.Value;

            SetMessage("Open List View to select.");
        }
    }
}
