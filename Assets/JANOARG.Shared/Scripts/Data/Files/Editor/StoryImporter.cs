using System.IO;
using JANOARG.Shared.Data.Story;
using UnityEditor.AssetImporters;


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