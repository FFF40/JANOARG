

using System.Collections.Generic;
using System.Xml.Serialization;

public class ScoreStore 
{
    private Dictionary<string, ScoreStoreEntry> Entries = new ();

    public void Load() 
    {
        List<ScoreStoreEntry> entryList = Common.main.Storage.Get("ScoreStore", new List<ScoreStoreEntry> {});
        foreach (ScoreStoreEntry entry in entryList) 
        {
            Entries[entry.SongID + "/" + entry.ChartID] = entry;
        }
    }

    public void Save() 
    {
        Common.main.Storage.Set("ScoreStore", Entries.Values);
    }

    public ScoreStoreEntry Get(string SongID, string ChartID)
    {
        if (Entries.TryGetValue(SongID + "/" + ChartID, out var value)) return value;
        return null;
    }

    public void Register(ScoreStoreEntry entry) 
    {
        ScoreStoreEntry oldEntry = Get(entry.SongID, entry.ChartID);
        if (
            oldEntry == null && oldEntry.Score < entry.Score
        ) 
        {
            Entries[entry.SongID + "/" + entry.ChartID] = entry;
        }
    }
}

public class ScoreStoreEntry
{
    [XmlAttribute("SongID")] public string SongID;
    [XmlAttribute("ChartID")] public string ChartID;

    [XmlAttribute("Score")] public int Score;
    [XmlAttribute("PerfectCount")] public int PerfectCount;
    [XmlAttribute("GoodCount")] public int GoodCount;
    [XmlAttribute("BadCount")] public int BadCount;
    [XmlAttribute("MaxCombo")] public int MaxCombo;
}