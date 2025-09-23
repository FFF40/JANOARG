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

        public List<ScoreStoreEntry> GetBestEntries(int count)
        {
            List<ScoreStoreEntry> bestEntries = new List<ScoreStoreEntry>(count);
            float minimum_Rating = 0.00f;
            
            Dictionary<string, ScoreStoreEntry> entries = StorageManager.sMain.Scores.entries;
            foreach (KeyValuePair<string, ScoreStoreEntry> entry in entries)
            {
                string key = entry.Key;
                int slashIndex = key.LastIndexOf('/');
                string songID = key.Substring(0, slashIndex);
                string chartID = key.Substring(slashIndex + 1);

                ScoreStoreEntry record = StorageManager.sMain.Scores.Get(songID, chartID);

                if (record == null)
                {
                    Debug.LogWarning("Record of " + key + " is missing!");

                    continue;
                }

                if (bestEntries.Count < count || record.Rating > minimum_Rating)
                    {
                        int index = bestEntries.FindIndex(e => record.Rating > e.Rating);

                        if (index == -1)
                        {
                            bestEntries.Add(record);
                        }
                        else
                        {
                            bestEntries.Insert(index, record);
                        }

                        if (bestEntries.Count > count)
                            bestEntries.RemoveAt(bestEntries.Count - 1);

                        minimum_Rating = bestEntries[^1].Rating;
                    }

            }

            bestEntries.Sort((a, b) => b.Rating.CompareTo(a.Rating)); // Sort in descending order by Rating

            Debug.Log("GetBestEntries complete.");
            Debug.Log("Best Entries Count: " + bestEntries.Count);
            return bestEntries;
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
        [XmlAttribute("SongID")]  public string SongID;
        [XmlAttribute("ChartID")] public string ChartID;

        [XmlAttribute("Score")]        public int Score;
        [XmlAttribute("PerfectCount")] public int PerfectCount;
        [XmlAttribute("GoodCount")]    public int GoodCount;
        [XmlAttribute("BadCount")]     public int BadCount;
        [XmlAttribute("MaxCombo")]     public int MaxCombo;

        [XmlAttribute("Rating")] public float Rating;
    }
}