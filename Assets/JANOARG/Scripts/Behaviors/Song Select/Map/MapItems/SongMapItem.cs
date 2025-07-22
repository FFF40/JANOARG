

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

    public void Start()
    {
        UpdateStatus();
    }

    public override void UpdateStatus()
    {
        if (Target == null)
        {
            Target = Array.Find(SongSelectScreen.main.Playlist.Songs, x => x.ID == TargetID);
            if (Target == null) throw new Exception($"Couldn't find song \"{TargetID}\" in the current playlist");
        }
        if (ItemUI == null)
        {
            ItemUI = MakeItemUI<SongMapItemUI, SongMapItem>();
            MapManager.SongMapItemUIsByID.Add(TargetID, ItemUI);
        }

        bool revealed = GameConditional.TestAll(Target.RevealConditions);
        bool unlocked = revealed && GameConditional.TestAll(Target.UnlockConditions);
    }

    public void OnDestroy()
    {
        MapManager.ItemUIs.Remove(ItemUI);
        MapManager.SongMapItemUIsByID.Remove(TargetID);
        Destroy(ItemUI);        
    }
}