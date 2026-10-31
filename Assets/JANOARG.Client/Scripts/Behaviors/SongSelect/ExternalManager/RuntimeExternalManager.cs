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

        private RectTransform _textsRt;
        private CanvasGroup _textsCg;
        private Coroutine _textsAnim;
        private bool _textsVisible;

        private RectTransform _reloadRt;
        private CanvasGroup _reloadCg;
        private Coroutine _reloadAnim;
        private bool _reloadVisible;

        public void Awake()
        {
            sMain = this;
            _canReload = false;
            if (Messages != null) Messages.raycastTarget = false;
            if (ImportProgress != null) ImportProgress.raycastTarget = false;

            if (Texts != null)
            {
                _textsRt = Texts.GetComponent<RectTransform>();
                _textsCg = Texts.GetComponent<CanvasGroup>();
                if (_textsCg == null) _textsCg = Texts.AddComponent<CanvasGroup>();
            }

            if (ReloadButton != null)
            {
                _reloadRt = ReloadButton.GetComponent<RectTransform>();
                _reloadCg = ReloadButton.GetComponent<CanvasGroup>();
                if (_reloadCg == null) _reloadCg = ReloadButton.gameObject.AddComponent<CanvasGroup>();
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
            if (wantTextsVisible != _textsVisible)
            {
                _textsVisible = wantTextsVisible;
                if (_textsAnim != null) StopCoroutine(_textsAnim);
                _textsAnim = StartCoroutine(wantTextsVisible ? ShowTextsAnim() : HideTextsAnim());
            }

            // Reload button: also animate on view toggle (but only when reload is available).
            bool wantReloadVisible = isMapView && _canReload && ReloadButton != null;
            if (wantReloadVisible != _reloadVisible)
            {
                _reloadVisible = wantReloadVisible;
                if (_reloadAnim != null) StopCoroutine(_reloadAnim);
                _reloadAnim = StartCoroutine(wantReloadVisible ? ShowReloadButton() : HideReloadButton());
            }
        }

        private IEnumerator ShowTextsAnim()
        {
            if (Texts == null || _textsRt == null || _textsCg == null) yield break;

            Texts.SetActive(true);

            Vector2 originalPos = _textsRt.anchoredPosition;
            _textsRt.anchoredPosition = new Vector2(originalPos.x - 40, originalPos.y);
            _textsCg.alpha = 0f;

            yield return Ease.Animate(0.3f, a =>
            {
                float e = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                _textsRt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x - 40, originalPos.x, e), originalPos.y);
                _textsCg.alpha = e;
            });
        }

        private IEnumerator HideTextsAnim()
        {
            if (Texts == null || _textsRt == null || _textsCg == null) yield break;
            if (!Texts.activeSelf) yield break;

            Vector2 originalPos = _textsRt.anchoredPosition;
            float startAlpha = _textsCg.alpha;

            yield return Ease.Animate(0.2f, EaseFunction.Cubic, EaseMode.In, a =>
            {
                _textsRt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x, originalPos.x - 40, a), originalPos.y);
                _textsCg.alpha = Mathf.Lerp(startAlpha, 0f, a);
            });

            Texts.SetActive(false);
            _textsCg.alpha = startAlpha;
            _textsRt.anchoredPosition = originalPos;
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

            if (_reloadRt == null || _reloadCg == null)
                yield break;

            Vector2 originalPos = _reloadRt.anchoredPosition;

            // If it isn't currently active, nothing to animate.
            if (!ReloadButton.gameObject.activeSelf)
                yield break;

            float startAlpha = _reloadCg.alpha;

            yield return Ease.Animate(0.2f, EaseFunction.Cubic, EaseMode.In, t =>
            {
                _reloadRt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x, originalPos.x + 40, t), originalPos.y);
                _reloadCg.alpha = Mathf.Lerp(startAlpha, 0f, t);
            });

            ReloadButton.gameObject.SetActive(false);
            _reloadCg.alpha = startAlpha;
            _reloadRt.anchoredPosition = originalPos;
        }



        IEnumerator ShowReloadButton()
        {
            if (ReloadButton == null || SongSelectScreen.sMain == null || !SongSelectScreen.sMain.IsMapView)
                yield break;

            if (_reloadRt == null || _reloadCg == null)
                yield break;

            Vector2 originalPos = _reloadRt.anchoredPosition;
            ReloadButton.gameObject.SetActive(true);
            _reloadRt.anchoredPosition = new Vector2(originalPos.x + 40, originalPos.y);
            _reloadCg.alpha = 0f;

            yield return Ease.Animate(0.3f, a =>
            {
                float e = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                _reloadRt.anchoredPosition = new Vector2(Mathf.Lerp(originalPos.x + 40, originalPos.x, e), originalPos.y);
                _reloadCg.alpha = e;
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

            // Locate each chart's .japs once here and hand the path to the loader.
            var dirs = new System.Collections.Generic.List<(string dir, string japsPath)>();
            foreach (string d in Directory.EnumerateDirectories(root))
            {
                string japs = SongDataProvider.FindJapsFile(d);
                if (japs != null) dirs.Add((d, japs));
            }

            SetProgress($"Found {dirs.Count} chart(s). Importing,");

            var pending = new System.Collections.Generic.List<(string id, PlayableSong song)>();
            int i = 0;
            foreach (var (dir, japsPath) in dirs)
            {
                i++;
                PlayableSong loaded = null;
                yield return SongDataProvider.sMain != null
                    ? StartCoroutine(SongDataProvider.sMain.LoadSongRoutine(dir, japsPath, s => loaded = s))
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

            int totalFound = dirs.Count;
            int newLoaded = pending.Count;

            SetMessage($"Found {totalFound} chart(s)!");

            if (newLoaded > 0)
                SetMessage($"New {newLoaded} chart(s) since last reload.");
            else
                SetMessage("No new charts loaded.");
                
            SetMessage("Open List View to select.");
        }
    }
}
