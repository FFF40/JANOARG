using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(1, "cgp")]
public class CGPImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        HelpPage ext = ScriptableObject.CreateInstance<HelpPage>();
        ext.Lines = File.ReadAllLines(ctx.assetPath);

        ctx.AddObjectToAsset("main obj", ext);
        ctx.SetMainObject(ext);
    }
}