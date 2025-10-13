using System.IO;
using JANOARG.Shared.Data.ChartInfo;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace JANOARG.Shared.Data.Files.Editor
{
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
}