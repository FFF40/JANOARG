using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeadlessPlayerStarter : MonoBehaviour
{
    public static ExternalPlayableSong RunChart;
    public static ExternalChart Chart;
    
    public static string TargetSongPath;
    public static PlayableSong TargetSong;
    public static ExternalChartMeta TargetChartMeta;

    public void Start()
    {
        // Start check
        if (RunChart == null || Chart == null)
            throw new NullReferenceException("RunChart or Chart must be assigned! Assign it from the editor in the PlayerScreen component AND enter playmode from there.");

        // Prepare variables
        string runChartPath = AssetDatabase.GetAssetPath(RunChart);
        string directory = Path.Combine( // Get the last 2 directories in the path
            Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(runChartPath))!), // 'Songs'
            Path.GetFileName(Path.GetDirectoryName(runChartPath))! // 'Chart folder name'
        );
        Debug.LogWarning("TargetSong is null, Loading RunChart from " + runChartPath);
            
        TargetSong = RunChart.Data;
        TargetSongPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(runChartPath));
        TargetChartMeta = TargetSong.Charts.Find(meta => meta.DifficultyName == Chart.Data.DifficultyName);
        
        // Prepare scene
        PlayerScreen player = PlayerScreen.main;
        
        Common.main.MainCamera.backgroundColor = TargetSong.BackgroundColor;
        PlayerScreen.TargetSong = TargetSong;
        PlayerScreen.TargetSongPath = TargetSongPath;
        PlayerScreen.TargetChartMeta = TargetChartMeta;
        
        Common.Load("Player", () => PlayerScreen.main && PlayerScreen.main.IsReady, () => { PlayerScreen.main.BeginReadyAnim(); }, false);
        
        SceneManager.UnloadSceneAsync("HeadlessPlayerStarter");
        Resources.UnloadUnusedAssets();
    }
}