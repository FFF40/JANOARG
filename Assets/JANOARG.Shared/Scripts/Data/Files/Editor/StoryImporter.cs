using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using UnityEditor;

[ScriptedImporter(1, "story")]
public class StoryImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        StoryScript script = StoryDecoder.Decode(File.ReadAllText(ctx.assetPath));

        ctx.AddObjectToAsset("main obj", script);
        ctx.SetMainObject(script);
    }
}