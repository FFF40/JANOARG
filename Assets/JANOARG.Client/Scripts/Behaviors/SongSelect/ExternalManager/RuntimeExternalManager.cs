using System;
using System.Collections;
using System.IO;
using System.Linq;
using JANOARG.Client.Behaviors.SongSelect;
using JANOARG.Client.Behaviors.SongSelect.Map;
using JANOARG.Client.Data.Playlist;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public class RuntimeExternalManager : MonoBehaviour
    {
        public static RuntimeExternalManager sMain;
        public static System.Collections.Generic.HashSet<string> ExternalSongIDs = new();
        public static System.Collections.Generic.HashSet<AudioClip> ExternalClips = new();
        public static System.Collections.Generic.HashSet<Texture2D> ExternalTextures = new();

        public void Awake() => sMain = this;

        public void Start() => StartCoroutine(Run());

        IEnumerator Run()
        {
            // Wait for SongSelectScreen
            yield return new WaitUntil(() => SongSelectScreen.sMain != null);

            // Ensure access to charts directory
            bool ok = false;
            ChartDirProvider.EnsureAccess(r => ok = r);
            yield return new WaitUntil(() => ok);

            string root = ChartDirProvider.GetChartsDirectoryPath();
            if (!Directory.Exists(root)) { Debug.LogWarning("[RuntimeExternalManager] Charts root missing"); yield break; }

            var screen = SongSelectScreen.sMain;
            Debug.Log($"[RuntimeExternalManager] Importing from {root}");

            int total = 0, imported = 0, skipped = 0;
            var pending = new System.Collections.Generic.List<(string id, PlayableSong song)>();
            foreach (string dir in Directory.GetDirectories(root))
            {
                total++;
                if (!Directory.GetFiles(dir, "*.japs", SearchOption.TopDirectoryOnly).Any())
                { skipped++; continue; }

                PlayableSong loaded = null;
                yield return SongDataProvider.sMain != null
                    ? StartCoroutine(SongDataProvider.sMain.LoadSongRoutine(dir, s => loaded = s))
                    : null;

                if (loaded == null) { Debug.LogWarning($"[RuntimeExternalManager] Failed to load {dir}"); skipped++; continue; }

                string id = Path.GetFileName(dir);
                if (screen.PlayableSongByID.ContainsKey(id)) { Debug.Log($"[RuntimeExternalManager] Already loaded: {id}"); skipped++; continue; }

                pending.Add((id, loaded));
                imported++;
                Debug.Log($"[RuntimeExternalManager] Loaded {id}");
            }

            // Inject all at once after import completes
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

            // Force list view + disable map view button (only in a map scene)
            ForceListViewAndDisableMapButton(screen);

            Debug.Log($"[RuntimeExternalManager] Done: {imported} imported, {skipped} skipped, {total} folders");
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
