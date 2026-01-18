

using System;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs;
using JANOARG.Client.Data.Playlist;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapItems
{
    public class SongMapItem : MapItem
    {

        [Space]
        public string TargetID;

        [NonSerialized]
        public PlaylistSong Target;
        [NonSerialized]
        public SongMapItemUI ItemUI;

        public void Start()
        {
            MapManager.sSongMapItemsByID.Add(TargetID, this);
            UpdateStatus();
        }

        public override void UpdateStatus()
        {
            if (Target == null)
            {
                Target = Array.Find(SongSelectScreen.sMain.Playlist.Songs, x => x.ID == TargetID);
                if (Target == null) throw new Exception($"Couldn't find song \"{TargetID}\" in the current playlist");
            }

            // Update unlock status
            isRevealed = GameConditional.TestAll(Target.RevealConditions);
            isUnlocked = isRevealed && GameConditional.TestAll(Target.UnlockConditions);

            // Update item UI
            if (ItemUI == null)
            {
                ItemUI = MakeItemUI<SongMapItemUI, SongMapItem>();
                MapManager.sSongMapItemUIsByID.Add(TargetID, ItemUI);
            }
        }

        public void OnDestroy()
        {
            print("on destroy called");
            MapManager.sSongMapItemsByID.Remove(TargetID);
            MapManager.sSongMapItemUIsByID.Remove(TargetID);
            if (ItemUI)
            {
                if (ItemUI.CoverImage.texture) SongSelectCoverManager.sMain.UnregisterUse(ItemUI.CoverImage);
                else SongSelectCoverManager.sMain.UnregisterUseSong(TargetID);
                MapManager.sItemUIs.Remove(ItemUI);
                Destroy(ItemUI.gameObject);
            }
        }
    }
}