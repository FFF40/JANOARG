using System.IO;
using JANOARG.Shared.Script.Data.Story;
using UnityEditor.AssetImporters;

namespace JANOARG.Shared.Script.Data.Files.Editor
{
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
}