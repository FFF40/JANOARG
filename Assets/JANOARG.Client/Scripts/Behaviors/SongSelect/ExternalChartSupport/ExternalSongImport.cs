using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JANOARG.Client.Data.Playlist;
using UnityEngine.SceneManagement;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItems;

namespace JANOARG.Client.Behaviors.SongSelect
{
   public class ExternalSongImport : MonoBehaviour
    {
        public ExternalPlaylist ExternalPlaylist;
        public bool isInitialized = false;
        public IEnumerator UpdateScene()
        {   
            isInitialized = false;
            
            // 1. Wait for the end of frame or a tiny delay to ensure the scene is ready
            yield return new WaitForEndOfFrame();

            GameObject parent = GameObject.Find("External Song Items");

            if (parent == null)
            {
                Debug.LogError("[Playlist Management] 'External Song Items' container not found!");
                yield break;
            }

            // 2. Clear existing children properly
            // We use a list to avoid modifying the collection while iterating
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                toDestroy.Add(child.gameObject);
            }
            
            foreach (GameObject obj in toDestroy)
            {
                Destroy(obj);
            }

            // 3. IMPORTANT: Wait for the next frame so the 'Destroyed' objects are actually gone
            // This prevents index or physics glitches.
            yield return null;

            int index = 0; 

            // Safely convert array to list if needed
            ExternalPlaylist.ArrayToList(); 

            if (ExternalPlaylist.Songlist == null)
            {
                Debug.LogError("[Playlist Management] Songlist is null!");
                yield break;
            }

            foreach (PlaylistSong song in ExternalPlaylist.Songlist)
            {
                Debug.Log("[Playlist Management] Adding song in map: " + song.ID);

                GameObject child = new GameObject($"Song_{song.ID}");
                child.transform.SetParent(parent.transform, false);

                // Incremental positioning (Spaced by 5 units)
                child.transform.localPosition = new Vector3(index * 5f, 0f, 0f);
                
                // Add + initialize
                SongMapItem item = child.AddComponent<SongMapItem>();
                item.Initialize(song);
                
                index++;
            }

            isInitialized = true;
        }
    }
}