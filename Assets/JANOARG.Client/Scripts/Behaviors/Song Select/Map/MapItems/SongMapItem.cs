

using System;
using UnityEngine;

public class SongMapItem : MapItem
{

    [Space]
    public string TargetID;

    [NonSerialized]
    public PlaylistSong Target;
    [NonSerialized]
    public SongMapItemUI ItemUI;

    public bool IsUnlocked { get; private set; }
    public bool IsRevealed { get; private set; }

    public void Start()
    {
        MapManager.SongMapItemsByID.Add(TargetID, this);
        UpdateStatus();
    }

    public override void UpdateStatus()
    {
        if (Target == null)
        {
            Target = Array.Find(SongSelectScreen.main.Playlist.Songs, x => x.ID == TargetID);
            if (Target == null) throw new Exception($"Couldn't find song \"{TargetID}\" in the current playlist");
        }

        // Update unlock status
        IsRevealed = GameConditional.TestAll(Target.RevealConditions);
        IsUnlocked = IsRevealed && GameConditional.TestAll(Target.UnlockConditions);

        // Update item UI
        if (ItemUI == null)
        {
            ItemUI = MakeItemUI<SongMapItemUI, SongMapItem>();
            MapManager.SongMapItemUIsByID.Add(TargetID, ItemUI);
        }
    }

    public void OnDestroy()
    {
        MapManager.ItemUIs.Remove(ItemUI);
        MapManager.SongMapItemsByID.Remove(TargetID);
        MapManager.SongMapItemUIsByID.Remove(TargetID);
        Destroy(ItemUI);        
    }
}