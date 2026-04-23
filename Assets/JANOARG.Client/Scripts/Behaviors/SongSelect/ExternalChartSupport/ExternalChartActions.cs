using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using static NativeFilePicker;
using JANOARG.Client.Data.Playlist;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItems;

namespace JANOARG.Client.Behaviors.SongSelect
{
    //TODO: Add Modal for importing song    
    //TODO: Add Android support for importing song
    //TODO: Add validation for imported files
    //TODO: Automate populate the Playlist
    //TODO: Add feedback modals

    //FIXME: This abomination make this more readable and maintainable
    public class ExternalChartActions : MonoBehaviour
    {
        public ExternalPlaylist ExternalPlaylist;
        public void Awake()
        {
            Debug.Log("ExternalChartActions Awake");
            // sMain = this;
        }
        #region Importing Charts
        //Get ZIP file from user
        
        // Button
        public void PickZip()
        {
            NativeFilePicker.PickFile(OnFilePicked, new[] { "application/zip" });
        }

        private void OnFilePicked(string path)
        {
            if (path == null)
            {
                Debug.Log("User cancelled file picker");
                return;
            }

            CopyAndImport(path);
        }

        void CopyAndImport(string sourcePath)
        {
            string chartsDir = Path.Combine(Application.persistentDataPath, "Charts");
            Directory.CreateDirectory(chartsDir);

            string destZip = Path.Combine(
                chartsDir,
                $"{Path.GetFileNameWithoutExtension(sourcePath)}.zip"
            );

            File.Copy(sourcePath, destZip, true);
            ImportZip(destZip);
        }

        void ImportZip(string zipPath)
        {
            string chartName = Path.GetFileNameWithoutExtension(zipPath);
            string extractPath = Path.Combine(Application.persistentDataPath, "Charts", chartName);

            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            Directory.CreateDirectory(extractPath);

            Debug.Log("[Chart Import] Extracting ZIP: " + zipPath);
            Debug.Log("[Chart Import] Extract path: " + extractPath);

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    string fullPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                    // if (!fullPath.StartsWith(extractPath))
                    // {
                    //     Debug.LogWarning("[Chart Import] Skipped unsafe entry: " + entry.FullName);
                    //     continue;
                    // }

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(fullPath);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    entry.ExtractToFile(fullPath, true);

                    Debug.Log("[Chart Import] Extracted file: " + fullPath);
                }
            }

            if (!IsValidChart(extractPath))
            {
                Debug.LogError("[Chart Import] Invalid chart format, cleaning up");

                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
                return;
            }

            if (AddToPlaylist(chartName, ExternalPlaylist))
            {

                Debug.Log("[Chart Import] Chart imported successfully: " + chartName);
            } 

            if (File.Exists(zipPath))
                File.Delete(zipPath);
            Debug.Log("[Chart Import] Deleted ZIP: " + zipPath);
        }

        bool IsValidChart(string chartPath)
        {
            Debug.Log("[Chart Import] Checking chart validity at: " + chartPath);

            // string[] japsFiles = Directory.GetFiles(
            //     chartPath,
            //     "*.japs",
            //     SearchOption.TopDirectoryOnly
            // );

            // foreach (var file in japsFiles)
            // {
            //     Debug.Log("[Chart Import] Found chart file: " + file);
            // }

            // return japsFiles.Length > 0;
            return true; // Placeholder, implement actual validation logic
        }

        #endregion

        #region Playlist Management
        public bool AddToPlaylist(string chartID, ExternalPlaylist playlist)
        {  
            Debug.Log("[Playlist Management] Adding chart to playlist: " + chartID);
            Debug.Log("[Playlist Management] Current playlist song count: " + playlist.Songs.Length);
            PlaylistSong newSong = new PlaylistSong
            {
                ID = chartID,
                RevealConditions = new GameConditional[0],
                UnlockConditions = new GameConditional[0]
            };
            playlist.AddSong(newSong);
            UpdateScene();
            
            return true;
        }

        public void RefreshPlaylist(ExternalPlaylist playlist)
        {
            Debug.Log("[Playlist Management] Refreshing playlist with " + playlist.Songs.Length + " songs");
            
            UpdateScene();
        }

        public void UpdateScene()
        {   
            // Get Scene
            string sceneName = SongSelectScreen.sMain.Playlist.MapName + " Map";
            Scene MapScene = SceneManager.GetSceneByName(sceneName);

            if (!MapScene.isLoaded)
            {
                Debug.LogError("[Playlist Management] Map scene is not loaded: " + sceneName);
                return;
            }

            // Get External Song Item 
            GameObject parent = GameObject.Find("External Song Items");

            if (parent == null)
            {
                Debug.LogError("External Song Items not found in scene " + sceneName + "!");
                return;
            }

            int index = parent.transform.childCount;

            foreach (PlaylistSong song in ExternalPlaylist.Songlist)
            {
                Debug.Log("[Playlist Management] Adding song in map: " + song.ID);

                GameObject child = new GameObject($"{song.ID}");
                child.transform.SetParent(parent.transform, false);

                // Incremental positioning
                child.transform.localPosition = new Vector3(index * 5f, 0f, 0f);
                index++;

                // Add + initialize safely
                SongMapItem item = child.AddComponent<SongMapItem>();
                item.Initialize(song);
            }
        }

        #endregion
    }
}