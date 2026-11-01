using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.ChartInfo;
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
                oldEntry.ChartIndex = entry.ChartIndex;
                oldEntry.Rating = Mathf.Max(oldEntry.Rating, entry.Rating);
                oldEntry.MaxCombo = Mathf.Max(oldEntry.MaxCombo, entry.MaxCombo);
            }
        }

        /// <summary>
        /// Re-homes score entries whose chart Target was renamed or reused for a different chart
        /// since the score was saved. The Target filename is mutable, but a chart's
        /// <see cref="ExternalChartMeta.DifficultyIndex"/> is its stable slot within the song, so
        /// entries are matched by index and merged when two of them end up on the same chart.
        /// Safe to run repeatedly: entries that already point at the right chart are left alone.
        /// </summary>
        /// <returns>True when at least one entry moved, meaning the store should be saved.</returns>
        public bool ReconcileCharts(IReadOnlyDictionary<string, PlayableSong> catalog)
        {
            if (catalog == null) return false;

            int movedCount = 0;

            foreach (KeyValuePair<string, PlayableSong> songPair in catalog)
            {
                string songID = songPair.Key;
                List<ExternalChartMeta> charts = songPair.Value?.Charts;

                if (charts == null || charts.Count == 0) continue;

                // Duplicate indices would make the mapping ambiguous, so leave those alone.
                Dictionary<int, ExternalChartMeta> byIndex = new();
                HashSet<int> ambiguous = new();

                foreach (ExternalChartMeta chart in charts)
                {
                    if (chart == null || chart.DifficultyIndex < 0) continue;

                    if (!byIndex.TryAdd(chart.DifficultyIndex, chart))
                        ambiguous.Add(chart.DifficultyIndex);
                }

                List<ScoreStoreEntry> songEntries = entries.Values
                    .Where(entry => entry.SongID == songID)
                    .ToList();

                foreach (ScoreStoreEntry entry in songEntries)
                {
                    ExternalChartMeta current = charts.Find(chart => chart != null && chart.Target == entry.ChartID);

                    // The target still names the same chart; nothing to do.
                    if (current != null && current.DifficultyIndex == entry.ChartIndex)
                        continue;

                    // Only indices are trusted; external/custom charts use scan-assigned negative
                    // indices which are not stable, so those are never re-homed.
                    if (entry.ChartIndex < 0 ||
                        ambiguous.Contains(entry.ChartIndex) ||
                        !byIndex.TryGetValue(entry.ChartIndex, out ExternalChartMeta moved) ||
                        string.IsNullOrEmpty(moved.Target) ||
                        moved.Target == entry.ChartID)
                        continue;

                    // Re-key onto the chart that still occupies this difficulty slot. Register()
                    // creates the new key or merges into an existing entry for that chart.
                    entries.Remove(songID + "/" + entry.ChartID);
                    entry.ChartID = moved.Target;
                    entry.ChartIndex = moved.DifficultyIndex;
                    Register(entry);
                    movedCount++;
                }
            }

            if (movedCount > 0)
                Debug.Log($"[ScoreStore] Re-homed {movedCount} score {(movedCount == 1 ? "entry" : "entries")} after chart metadata changes.");

            return movedCount > 0;
        }

        // Returns best n (default 30) rating entries
        public List<ScoreStoreEntry> GetBestEntries(int count = 30)
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