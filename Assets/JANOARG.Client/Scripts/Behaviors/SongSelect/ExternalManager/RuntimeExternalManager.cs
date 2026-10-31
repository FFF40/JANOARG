using System;
using System.Collections;
using System.IO;
using System.Linq;
using JANOARG.Client.Behaviors.SongSelect;
using JANOARG.Client.Behaviors.SongSelect.Map;
using JANOARG.Client.Data.Playlist;
using JANOARG.Client.UI.Modal;
using JANOARG.Shared.Data.ChartInfo;
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

        public void Awake() => sMain = this;

        public void Start()
        {
            if (ReloadButton != null && ReloadButton.Button != null)
                ReloadButton.Button.onClick.AddListener(Reload);
            StartCoroutine(Run());
        }

        public void Reload()
        {
            if (SongSelectScreen.sMain != null && SongSelectScreen.sMain.IsAnimating) return;
            if (ReloadButton != null && ReloadButton.Button != null)
                ReloadButton.Button.interactable = false;
            StopAllCoroutines();
            StartCoroutine(Run());
            StartCoroutine(ReenableReloadButton());
        }

        IEnumerator ReenableReloadButton()
        {
            yield return new WaitUntil(() => !SongSelectScreen.sMain.IsAnimating);
            yield return new WaitForSeconds(0.2f);
            if (ReloadButton != null && ReloadButton.Button != null)
                ReloadButton.Button.interactable = true;
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

            ForceListViewAndDisableMapButton(screen);

            ClearMessages();
            SetMessage($"Imported {pending.Count} chart(s)!");
            SetMessage("Open List View to select.");
        }

        static void ForceListViewAndDisableMapButton(SongSelectScreen screen)
        {
            if (MapManager.sMain == null || !MapManager.sMain.MapScene.IsValid())
                return;

            if (screen.IsMapView)
                screen.ToggleView();

            screen.MapViewButton.gameObject.SetActive(false);
            screen.UpdateButtons();
        }
    }
}
