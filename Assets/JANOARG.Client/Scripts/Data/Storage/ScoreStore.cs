using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JANOARG.Client.Scripts.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Client.Scripts.Data.Storage
{
    public class ScoreStore 
    {
        public Dictionary<string, ScoreStoreEntry> Entries { get; private set; } = new();

        public void Load() 
        {
            ScoreStoreEntry[] entryList = CommonSys.main.Storage.Get("ScoreStore", new ScoreStoreEntry[0]);
            foreach (ScoreStoreEntry entry in entryList) 
            {
                Entries[entry.SongID + "/" + entry.ChartID] = entry;
            }
        }

        public void Save() 
        {
            CommonSys.main.Storage.Set("ScoreStore", Entries.Values.ToArray());
        }

        public ScoreStoreEntry Get(string SongID, string ChartID)
        {
            ScoreStoreEntry value;
            if (Entries.TryGetValue(SongID + "/" + ChartID, out value)) return value;
            return null;
        }

        public void Register(ScoreStoreEntry entry)
        {
            ScoreStoreEntry oldEntry = Get(entry.SongID, entry.ChartID);
            if (oldEntry == null || oldEntry.Score < entry.Score) 
            {
                string id = entry.SongID + "/" + entry.ChartID;
                Entries[id] = entry;
            }
            else 
            {
                oldEntry.Rating = Mathf.Max(oldEntry.Rating, entry.Rating);
                oldEntry.MaxCombo = Mathf.Max(oldEntry.MaxCombo, entry.MaxCombo);
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

        [XmlAttribute("Rating")] public float Rating;
    }
}