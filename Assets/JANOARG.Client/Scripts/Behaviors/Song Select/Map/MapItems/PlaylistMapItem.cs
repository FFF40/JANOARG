

using System;
using UnityEngine;

public class PlaylistMapItem : MapItem
{

    [Space]
    public string TargetID;

    [NonSerialized]
    public PlaylistReference Target;
    [NonSerialized]
    public SongMapItemUI ItemUI;

    public bool IsUnlocked { get; private set; }
    public bool IsRevealed { get; private set; }

    public void Start()
    {
        UpdateStatus();
    }

    public override void UpdateStatus()
    {
        if (Target == null)
        {
            Target = Array.Find(SongSelectScreen.main.Playlist.Playlists, x => x.Playlist.MapName == TargetID);
            if (Target == null) throw new Exception($"Couldn't find playlist \"{TargetID}\" in the current playlist");
        }

        // Update unlock status
        IsRevealed = GameConditional.TestAll(Target.RevealConditions);
        IsUnlocked = IsRevealed && GameConditional.TestAll(Target.UnlockConditions);
    }
}