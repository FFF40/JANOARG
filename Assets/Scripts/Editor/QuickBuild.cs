using UnityEditor;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Diagnostics;
using System;

public class QuickBuild
{
    [MenuItem("JANOARG/Quick Build", priority = 1000)]
    public static void Build ()
    {
        string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Builds");

        Directory.Delete(path, true);
        Directory.CreateDirectory(path);

        BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
            locationPathName = Path.Combine(path, "Chartmaker-win-x86_64/Chartmaker.exe"),
            target = BuildTarget.StandaloneWindows64
        });
        ZipFile.CreateFromDirectory(
            Path.Combine(path, "Chartmaker-win-x86_64"),
            Path.Combine(path, "Chartmaker-win-x86_64.zip")
        );

        BuildPipeline.BuildPlayer(new BuildPlayerOptions() {
            locationPathName = Path.Combine(path, "Chartmaker-linux-x86_64/Chartmaker.x86_64"),
            target = BuildTarget.StandaloneLinux64
        });
        // TODO: figure out how to automate creating a .tar.gz archive
        // UnityEngine.Debug.Log(await cmd("/bin/bash", "tar -czvf '" + Path.Combine(path, "Chartmaker-linux-x86_64.tar.gz").Replace("'", "''") + "' '"
        //     + Path.Combine(path, "Chartmaker-linux-x86_64/").Replace("'", "''")));

        Application.OpenURL("file://" + path);
    }
}