using System;
using System.IO;
using JANOARG.Client.Scripts.Behaviors.Common;
using JANOARG.Shared.Script.Data.ChartInfo;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JANOARG.Client.Scripts.Behaviors.Player
{
    public class HeadlessPlayerStarter : MonoBehaviour
    {
        public static ExternalPlayableSong RunChart;
        public static ExternalChart Chart;

        private static string _targetSongPath;
        private static PlayableSong _targetSong;
        private static ExternalChartMeta _targetChartMeta;

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
            
            _targetSong = RunChart.Data;
            _targetSongPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(runChartPath));
            _targetChartMeta = _targetSong.Charts.Find(meta => meta.DifficultyName == Chart.Data.DifficultyName);
        
            // Prepare scene
            PlayerScreen.HeadlessInitialised = true;
        
            CommonSys.main.MainCamera.backgroundColor = _targetSong.BackgroundColor;
            PlayerScreen.TargetSong = _targetSong;
            PlayerScreen.TargetSongPath = _targetSongPath;
            PlayerScreen.TargetChartMeta = _targetChartMeta;
        
            CommonSys.Load("Player", () => PlayerScreen.main && PlayerScreen.main.IsReady, () => { PlayerScreen.main.BeginReadyAnim(); }, true);
        
            SceneManager.UnloadSceneAsync("HeadlessPlayerStarter");
            Resources.UnloadUnusedAssets();
        }
    }
}