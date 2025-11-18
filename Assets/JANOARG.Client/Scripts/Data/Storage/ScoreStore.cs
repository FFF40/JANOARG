using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JANOARG.Client.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Client.Data.Storage
{
    public class ScoreStore
    {
        public Dictionary<string, ScoreStoreEntry> entries { get; private set; } = new();

        public void Load()
        {
            ScoreStoreEntry[] entryList = CommonSys.sMain.Storage.Get("ScoreStore", Array.Empty<ScoreStoreEntry>());

            foreach (ScoreStoreEntry entry in entryList) entries[entry.SongID + "/" + entry.ChartID] = entry;
        }

        public void Save()
        {
            CommonSys.sMain.Storage.Set("ScoreStore", entries.Values.ToArray());
        }

        public ScoreStoreEntry Get(string songID, string chartID)
        {
            ScoreStoreEntry value;

            if (entries.TryGetValue(songID + "/" + chartID, out value))
                return value;

            return null;
        }

        public void Register(ScoreStoreEntry entry)
        {
            ScoreStoreEntry oldEntry = Get(entry.SongID, entry.ChartID);

            if (oldEntry == null || oldEntry.Score < entry.Score)
            {
                string id = entry.SongID + "/" + entry.ChartID;
                entries[id] = entry;
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
        [XmlAttribute("SongID")]       public string SongID;
        [XmlAttribute("ChartID")]      public string ChartID;
        [XmlAttribute("Index")]        public int ChartIndex;

        [XmlAttribute("Score")]        public int Score;
        [XmlAttribute("PerfectCount")] public int PerfectCount;
        [XmlAttribute("GoodCount")]    public int GoodCount;
        [XmlAttribute("BadCount")]     public int BadCount;
        [XmlAttribute("MaxCombo")]     public int MaxCombo;

        [XmlAttribute("Rating")]       public float Rating;
    }
}