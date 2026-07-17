


using System;
using JANOARG.Client.Behaviors.Common;

namespace JANOARG.Client.Data.Playlist.Conditionals
{
    [Serializable]
    public class FlagStoreGameConditional : GameConditional
    {
        public string Flag;

        public override bool Test()
        {
            return StorageManager.sMain.Flags.Test(Flag);
        }

        public override string GetDisplayInstructionString()
        {
            return $"Player save contains flag \"{Flag}\"";
        }
    }
}