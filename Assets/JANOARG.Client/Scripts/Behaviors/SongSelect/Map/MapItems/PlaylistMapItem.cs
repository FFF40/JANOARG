

using System;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs;
using JANOARG.Client.Data.Playlist;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapItems
{
    public class PlaylistMapItem : MapItem
    {

        [Space]
        public string TargetID;

        [NonSerialized]
        public PlaylistReference Target;
        [NonSerialized]
        public PlaylistMapItemUI ItemUI;

        public void Start()
        {
            MapManager.sPlaylistMapItemsByID.Add(TargetID, this);
            UpdateStatus();
        }

        public override void UpdateStatus()
        {
            if (Target == null)
            {
                Target = Array.Find(SongSelectScreen.sMain.Playlist.Playlists, x => x.Playlist.MapName == TargetID);
                if (Target == null) throw new Exception($"Couldn't find playlist \"{TargetID}\" in the current playlist");
            }

            // Update unlock status
            isRevealed = GameConditional.TestAll(Target.RevealConditions);
            isUnlocked = isRevealed && GameConditional.TestAll(Target.UnlockConditions);

            // Update item UI
            if (ItemUI == null)
            {
                ItemUI = MakeItemUI<PlaylistMapItemUI, PlaylistMapItem>();
                MapManager.sPlaylistMapItemUIsByID.Add(TargetID, ItemUI);
            }
        }
        
        public void OnDestroy()
        {
            print("on destroy called");
            MapManager.sItemUIs.Remove(ItemUI);
            MapManager.sPlaylistMapItemsByID.Remove(TargetID);
            MapManager.sPlaylistMapItemUIsByID.Remove(TargetID);
            Destroy(ItemUI.gameObject);        
        }
    }
}