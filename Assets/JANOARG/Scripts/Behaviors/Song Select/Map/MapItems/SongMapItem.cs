

using System;
using UnityEngine;

public class SongMapItem : MapItem
{
    [NonSerialized]
    public PlaylistSong Target;

    [Space]
    public string TargetID;
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
        }

        bool revealed = GameConditional.TestAll(Target.RevealConditions);
        bool unlocked = revealed && GameConditional.TestAll(Target.UnlockConditions);
    }

    public void Onestroy()
    {
        Destroy(ItemUI);        
    }
}