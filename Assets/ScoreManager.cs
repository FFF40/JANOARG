using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager main;

    public SongSelectScreen SongSelect;
    public Storage scores;

    void Awake()
    {
        main = this;
        

    } 

    void Start()
    {
        StartCoroutine(Read());
    }

    public IEnumerator Read()
    {
        
        if (SongSelect == null)
        {
            Debug.LogError("Song selecct is null");
            yield break;
        }

        if (!SongSelect.IsPlaylistInitialized)
        {
            yield return new WaitUntil(() => SongSelect.IsPlaylistInitialized);
        }

        Storage scores = Common.main.Scores;
        Debug.Log("READ FUNCTION");

        List<TrackData> tracks = new List<TrackData>();

        Debug.Log("Adding data");
        Debug.Log(SongSelect.SongList.Count + "<---------- Count");

        foreach (PlayableSong song in SongSelect.SongList)
        {
            TrackData entry = new TrackData();

            entry.TrackId = song.SongName + "." + song.SongArtist;
            entry.TrackName = song.SongName;
            entry.isLocked = false;
            entry.difficulty = new List<DifficultyData>();
            Debug.Log("Adding song data");


            foreach (ExternalChartMeta chart in song.Charts)
            {
                DifficultyData diffEntry = new DifficultyData();

                Debug.Log("Adding" + song.SongName + "/" + chart.DifficultyName);

                diffEntry.DifficultyName = chart.DifficultyName;
                diffEntry.Level = chart.DifficultyLevel;
                diffEntry.ChartConstant = chart.ChartConstant;

                diffEntry.Highscore = 0;
                diffEntry.Rank = "?";
                diffEntry.Perfect_Count = 0;
                diffEntry.Good_Count = 0;
                diffEntry.Bad_Count = 0;
                diffEntry.MaxCombo = 0;
                diffEntry.Rating = 0;

                diffEntry.isLocked = false;

                entry.difficulty.Add(diffEntry);
            }

            tracks.Add(entry);
        }

        Debug.Log("End data");
        TrackData[] trackdata = tracks.ToArray();
        Storage.CollectionProxy tracklist = new Storage.CollectionProxy(trackdata);

        scores.Set("Tracklist", tracklist);
        scores.Save();

    

    }

}
[XmlRoot("TrackData")]
[XmlInclude(typeof(DifficultyData))]
public class TrackData 
{
    [XmlElement("TrackName")] public string TrackName;
    [XmlElement("TrackId")] public string TrackId; //concat name and artist name

    [XmlArray("Difficulties")] [XmlArrayItem("DifficultyData")] 
    public List<DifficultyData> difficulty = new();

    [XmlElement("IsLocked")] public bool isLocked; 
}

public class DifficultyData
{
    // INFO
    [XmlElement("DifficultyName")] public string DifficultyName;
    [XmlElement("Level")] public string Level;
    [XmlElement("ChartConstant")] public float ChartConstant;

    //BEST STATS
    [XmlElement("Highscore")] public int Highscore;
    [XmlElement("Rank")] public string Rank;
    [XmlElement("PerfectCount")] public int Perfect_Count;
    [XmlElement("GoodCount")] public int Good_Count;
    [XmlElement("BadCount")] public int Bad_Count;
    [XmlElement("MaxCombo")] public int MaxCombo;
    [XmlElement("Rating")] public float Rating;

    [XmlElement("isLocked")] public bool isLocked;
}