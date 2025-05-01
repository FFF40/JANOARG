using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;


public class PlaylistScrollItem : MonoBehaviour
{
    public Image MainImage;
    public TMP_Text SongNameLabel;
    public TMP_Text ArtistNameLabel;
    public Image CoverImage;

    public PlayableSong Song;
    public string Path;

    public string DataText;

    public void SetSong(PlayableSong song, string path, int altLevel)
    {
        Path = path;
        SongNameLabel.text = (altLevel >= 1 && !string.IsNullOrWhiteSpace(song.AltSongName)) ? song.AltSongName : song.SongName;
        ArtistNameLabel.text = (altLevel >= 2 && !string.IsNullOrWhiteSpace(song.AltSongArtist)) ? song.AltSongArtist : song.SongArtist;
        DataText = song.Location.ToUpper() + " • " + song.Genre.ToUpper() + 
            " • " + FormatBPM(song.Timing) + " • " + FormatDuration(song.Clip.length);
        
        Song = song;
    }

    string FormatBPM(Metronome timing)
    {
        float max = timing.Stops[0].BPM;
        float min = timing.Stops[0].BPM;

        for (int a = 1; a < timing.Stops.Count; a++)
        {
            if (timing.Stops[a].Significant)
            {
                max = Mathf.Max(max, timing.Stops[a].BPM);
                min = Mathf.Min(min, timing.Stops[a].BPM);
            }
        }

        string ans = min.ToString("0.##", CultureInfo.InvariantCulture);
        if (max != min) ans += "~" + max.ToString("0.##", CultureInfo.InvariantCulture);
        return ans + " BPM";
    }

    string FormatDuration(float seconds)
    {
        return Mathf.Floor(seconds / 60).ToString("0", CultureInfo.InvariantCulture) + "m "
            + Mathf.Floor(seconds % 60).ToString("0", CultureInfo.InvariantCulture) + "s";
    }
}
