

using System;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs;
using JANOARG.Client.Data.Playlist;
using JANOARG.Client.Data.Story;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapItems
{
    public class StoryMapItem : MapItem
    {

        [Space]
        public StoryScript TargetStory;
        public string ID;
        [NonSerialized]
        public PlaylistStory Target;
        [NonSerialized]
        public StoryMapItemUI ItemUI;

        public void Start()
        {
            ID = TargetStory.name;
            Debug.Log($"{ID}");
            MapManager.sStoryMapItemsByID.Add(ID, this);
            UpdateStatus();
        }

        public override void UpdateStatus()
        {
            if (Target == null)
            {
                Target = Array.Find(SongSelectScreen.sMain.Playlist.Stories, x => x.ID == TargetStory);
                if (Target == null) throw new Exception($"Couldn't find playlist \"{ID}\" in the current playlist");
            }

            // Update unlock status
            isRevealed = GameConditional.TestAll(Target.RevealConditions);
            isUnlocked = isRevealed && GameConditional.TestAll(Target.UnlockConditions);
            
            Debug.Log($"ItemUI == {ItemUI == null}");
            // Update item UI
            if (ItemUI == null)
            {
                ItemUI = MakeItemUI<StoryMapItemUI, StoryMapItem>();
                Debug.Log($"ItemUI == {ItemUI == null}");
                MapManager.sStoryMapItemUIsByID.Add(ID, ItemUI);
            }
            Debug.Log("Test, should ADD");
        }
        
        public void OnDestroy()
        {
            print("on destroy called");
            MapManager.sItemUIs.Remove(ItemUI);
            MapManager.sStoryMapItemsByID.Remove(ID);
            MapManager.sStoryMapItemUIsByID.Remove(ID);
            if (ItemUI) Destroy(ItemUI.gameObject);        
        }
    }
}