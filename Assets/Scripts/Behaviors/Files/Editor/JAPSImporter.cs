using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using UnityEditor;

[ScriptedImporter(1, "japs")]
public class JAPSImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        PlayableSong song = JAPSDecoder.Decode(File.ReadAllText(ctx.assetPath));

        song.Clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(Path.GetDirectoryName(ctx.assetPath), song.ClipPath));

        ctx.AddObjectToAsset("main obj", song);
        ctx.SetMainObject(song);
    }
}