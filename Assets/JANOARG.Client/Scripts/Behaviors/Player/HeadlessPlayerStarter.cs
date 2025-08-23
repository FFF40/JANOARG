using System;
using System.IO;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.ChartInfo;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JANOARG.Client.Behaviors.Player
{
    public class HeadlessPlayerStarter : MonoBehaviour
    {
        public static ExternalPlayableSong sRunChart;
        public static ExternalChart        sChart;

        private static string            s_targetSongPath;
        private static PlayableSong      s_targetSong;
        private static ExternalChartMeta s_targetChartMeta;

        public void Start()
        {
            // Start check
            if (sRunChart == null || sChart == null)
                throw new NullReferenceException(
                    "RunChart or Chart must be assigned! Assign it from the editor in the PlayerScreen component AND enter playmode from there.");

            // Prepare variables
            string runChartPath = AssetDatabase.GetAssetPath(sRunChart);

            string directory = Path.Combine( // Get the last 2 directories in the path
                Path.GetFileName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            runChartPath))
                    !), // 'Songs'
                Path.GetFileName(
                    Path.GetDirectoryName(
                        runChartPath))
                ! // 'Chart folder name'
            );

            Debug.LogWarning("TargetSong is null, Loading RunChart from " + runChartPath);

            s_targetSong = sRunChart.Data;
            s_targetSongPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(runChartPath));
            s_targetChartMeta = s_targetSong.Charts.Find(meta => meta.DifficultyName == sChart.Data.DifficultyName);

            // Prepare scene
            PlayerScreen.sHeadlessInitialised = true;

            CommonSys.sMain.MainCamera.backgroundColor = s_targetSong.BackgroundColor;
            PlayerScreen.sTargetSong = s_targetSong;
            PlayerScreen.sTargetSongPath = s_targetSongPath;
            PlayerScreen.sTargetChartMeta = s_targetChartMeta;

            CommonSys.Load(
                "Player", () => PlayerScreen.sMain && PlayerScreen.sMain.IsReady,
                () => { PlayerScreen.sMain.BeginReadyAnim(); });

            SceneManager.UnloadSceneAsync("HeadlessPlayerStarter");
            Resources.UnloadUnusedAssets();
        }
    }
}