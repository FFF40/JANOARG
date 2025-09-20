using System.IO;
using JANOARG.Shared.Data.ChartInfo;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;


    [ScriptedImporter(1, "japs", 1000)]
    public class JAPSImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            PlayableSong song = JAPSDecoder.Decode(File.ReadAllText(ctx.assetPath));

            var ext = ScriptableObject.CreateInstance<ExternalPlayableSong>();
            ext.Data = song;

            song.Clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                Path.Combine(
                    Path.GetDirectoryName(ctx.assetPath),
                    song.ClipPath));

            ctx.AddObjectToAsset("main obj", ext);
            ctx.SetMainObject(ext);
        }
    }