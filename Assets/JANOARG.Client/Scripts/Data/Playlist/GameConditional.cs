


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace JANOARG.Client.Data.Playlist
{
    [Serializable]
    public abstract class GameConditional
    {
        public abstract bool Test();
        public abstract string GetDisplayInstructionString();

        public static bool TestAll(IEnumerable<GameConditional> conditionals)
        {
            return conditionals.All(x => x.Test());
        }

        public static string GetDisplayInstructionString(IEnumerable<GameConditional> conditionals)
        {
            StringBuilder builder = new();
            foreach (var conditional in conditionals)
            {
                bool completed = conditional.Test();

                if (completed) builder.Append("<s><alpha=#77>");
                else builder.Append("");
                builder.Append("• <indent=1.2em>");
                builder.Append(conditional.GetDisplayInstructionString());
                builder.Append("</indent>");
                if (completed) builder.Append("</s></alpha>");

                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}