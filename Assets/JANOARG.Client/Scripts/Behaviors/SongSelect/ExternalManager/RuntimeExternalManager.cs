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

        public ModalActionButton ReloadButton;
        
        public static System.Collections.Generic.HashSet<string> ExternalSongIDs = new();

        private static readonly System.Collections.Generic.Queue<string> _backendQueue = new();
        private static string _currentBackendError;
        private static bool _canReload;

        public static void BackendError(string msg)
        {
            _backendQueue.Enqueue(msg);
            if (_currentBackendError == null)
                ShowNextBackendError();
        }

        private static void ShowNextBackendError()
        {
            if (_backendQueue.Count == 0) { _currentBackendError = null; HideMessages(); return; }
            _currentBackendError = _backendQueue.Dequeue();
            if (sMain != null && sMain.Messages != null)
            {
                sMain.Messages.text = _currentBackendError;
                sMain.Messages.gameObject.SetActive(true);
                if (sMain.ImportProgress != null) sMain.ImportProgress.gameObject.SetActive(false);
            }
            Debug.LogWarning($"[BACKEND] {_currentBackendError}");
        }

        public static void DismissBackendError()
        {
            _currentBackendError = null;
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

        public static System.Collections.Generic.HashSet<AudioClip> ExternalClips = new();
        public static System.Collections.Generic.HashSet<Texture2D> ExternalTextures = new();

        public void Awake()
        {
            sMain = this;
            _canReload = false;
            if (Messages != null) Messages.raycastTarget = false;
            if (ImportProgress != null) ImportProgress.raycastTarget = false;
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
            if (ReloadButton == null) return;

            bool visible = _canReload
                && SongSelectScreen.sMain != null
                && SongSelectScreen.sMain.IsMapView;

            if (ReloadButton.gameObject.activeSelf != visible)
                ReloadButton.gameObject.SetActive(visible);
        }

        private Coroutine _reimportCoroutine;

        public void Reload()
        {
            Debug.Log("[RuntimeExternalManager] Reload clicked");
            if (SongSelectScreen.sMain != null && SongSelectScreen.sMain.IsAnimating) return;

            // Only allow one reimport coroutine at a time.
            if (_reimportCoroutine != null) StopCoroutine(_reimportCoroutine);
            _reimportCoroutine = StartCoroutine(ReimportRoutine());
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

            _reimportCoroutine = null;
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

            var rt = ReloadButton.GetComponent<RectTransform>();
            var cg = ReloadButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = ReloadButton.gameObject.AddComponent<CanvasGroup>();

            Vector2 originalPos = rt.anchoredPosition;

            // If it isn't currently active, nothing to animate.
            if (!ReloadButton.gameObject.activeSelf)
                yield break;

            float startAlpha = cg.alpha;

            yield return Ease.Animate(0.2f, EaseFunction.Cubic, EaseMode.In, t =>
            {
                rt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x, originalPos.x + 40, t), originalPos.y);
                cg.alpha = Mathf.Lerp(startAlpha, 0f, t);
            });

            ReloadButton.gameObject.SetActive(false);
            cg.alpha = startAlpha;
            rt.anchoredPosition = originalPos;
        }



        IEnumerator ShowReloadButton()
        {
            if (ReloadButton == null || SongSelectScreen.sMain == null || !SongSelectScreen.sMain.IsMapView)
                yield break;

            var rt = ReloadButton.GetComponent<RectTransform>();
            var cg = ReloadButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = ReloadButton.gameObject.AddComponent<CanvasGroup>();

            Vector2 originalPos = rt.anchoredPosition;
            ReloadButton.gameObject.SetActive(true);
            rt.anchoredPosition = new Vector2(originalPos.x + 40, originalPos.y);
            cg.alpha = 0f;

            yield return Ease.Animate(0.3f, a =>
            {
                float e = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                rt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x + 40, originalPos.x, e), originalPos.y);
                cg.alpha = e;
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

            var dirs = Directory.GetDirectories(root)
                .Where(d => Directory.GetFiles(d, "*.japs", SearchOption.TopDirectoryOnly).Any())
                .ToList();

            SetProgress($"Found {dirs.Count} chart(s). Importing,");

            var pending = new System.Collections.Generic.List<(string id, PlayableSong song)>();
            int i = 0;
            foreach (string dir in dirs)
            {
                i++;
                PlayableSong loaded = null;
                yield return SongDataProvider.sMain != null
                    ? StartCoroutine(SongDataProvider.sMain.LoadSongRoutine(dir, s => loaded = s))
                    : null;

                if (loaded == null)
                {
                    BackendError($"Unable to import {Path.GetFileName(dir)}");
                    continue;
                }

                string id = Path.GetFileName(dir);
                if (screen.PlayableSongByID.ContainsKey(id))
                {
                    SetProgress($"({i}/{dirs.Count}) Skipped {id} (already loaded)");
                    continue;
                }

                pending.Add((id, loaded));
                SetProgress($"({i}/{dirs.Count}) Imported {id}");
            }

            SetProgress("Import complete. Updating Database.");

            foreach (var (id, song) in pending)
            {
                var playlistSong = new PlaylistSong { ID = id, RevealConditions = Array.Empty<GameConditional>(), UnlockConditions = Array.Empty<GameConditional>() };
                screen.LoadedSongs.Add(playlistSong);
                screen.PlayableSongByID[id] = song;
                screen.PlaylistSongByID[id] = playlistSong;
                ExternalSongIDs.Add(id);
            }

            screen.ListView.UpdateSort();
            screen.ListView.UpdateListItems(screen);
            screen.ListView.IsDirty = false;

            _canReload = true;

            yield return StartCoroutine(ShowReloadButton());

            ClearMessages();
            SetMessage($"Imported {pending.Count} chart(s)!");
            SetMessage("Open List View to select.");
        }
    }
}
