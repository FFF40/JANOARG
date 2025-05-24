using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(2, "jac", 1001)]
public class JACImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        Chart chart = JACDecoder.Decode(File.ReadAllText(ctx.assetPath));

        ExternalChart ext = ScriptableObject.CreateInstance<ExternalChart>();
        ext.Data = chart;

        ctx.AddObjectToAsset("main obj", ext);
        ctx.SetMainObject(ext);
    }
}