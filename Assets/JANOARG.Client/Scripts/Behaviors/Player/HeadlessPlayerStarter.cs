using System;
using System.Collections;
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

        public IEnumerator Start()
        {
            // Start check
            if (sRunChart == null || sChart == null)
                throw new NullReferenceException(
                    "RunChart or Chart must be assigned! Assign it from the editor in the PlayerScreen component AND enter playmode from there.");

#if UNITY_EDITOR
            s_targetSong = null;
            s_targetSongPath = null;
            s_targetChartMeta = null;

            // Prepare variables
            string runChartPath = AssetDatabase.GetAssetPath(sRunChart);
            string chartPath = AssetDatabase.GetAssetPath(sChart);

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
            string chartTarget = Path.GetFileNameWithoutExtension(chartPath);
            s_targetChartMeta = s_targetSong.Charts.Find(meta =>
                meta.Target == chartTarget || meta.Target == Path.GetFileName(chartPath));

            if (s_targetSong == null)
                throw new Exception($"Failed to load song data from '{runChartPath}'.");

            if (string.IsNullOrEmpty(s_targetSongPath))
                throw new Exception($"Failed to build a Resources path for '{runChartPath}'.");

            if (s_targetChartMeta == null)
                throw new Exception($"Failed to find chart metadata targeting '{chartTarget}' in '{runChartPath}'.");
#endif

            // Prepare scene
            PlayerScreen.sHeadlessInitialised = true;

            CommonSys.sMain.MainCamera.backgroundColor = s_targetSong.BackgroundColor;
            PlayerScreen.sTargetSong = s_targetSong;
            PlayerScreen.sTargetSongPath = s_targetSongPath;
            PlayerScreen.sTargetChartMeta = s_targetChartMeta;
            PlayerScreen.sTargetChart = sChart;
            PlayerScreen.sMain = null;

            yield return new WaitUntil(() => !SceneManager.GetSceneByName("Player").isLoaded);
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return null;

            CommonSys.Load(
                "Player", () => PlayerScreen.sMain && PlayerScreen.sMain.IsReady,
                () =>
                {
                    PlayerScreen.sMain.BeginReadyAnim();
                    SceneManager.UnloadSceneAsync("HeadlessPlayerStarter");
                    Resources.UnloadUnusedAssets();
                });
        }
    }
}
