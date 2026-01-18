using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JANOARG.Client.Data.Story;
using JANOARG.Client.Data.Story.Instructions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

namespace JANOARG.Client.Data.Files
{
    public class StoryDecoder
    {
        public const int FORMAT_VERSION = 1;
        public const int INDENT_SIZE    = 2;

        private static Dictionary<string, StoryTagInfo> s_StoryTags;

        private static readonly Regex sr_ActorParseRegex =
            new(@"^(?<actor>(?:[0-9a-zA-Z]+,)*[0-9a-zA-Z]+)\s*>\s+(?<content>.*)");

#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        public static void InitiateStoryTags()
        {
            s_StoryTags = new Dictionary<string, StoryTagInfo>();
            var asm = Assembly.GetAssembly(typeof(StoryInstruction));

            foreach (Type cls in asm.GetTypes())
            {
                if (!typeof(StoryInstruction).IsAssignableFrom(cls)) continue;

                foreach (ConstructorInfo cons in cls.GetConstructors())
                foreach (Attribute attr in cons.GetCustomAttributes())
                {
                    if (attr is not StoryTagAttribute tagAttr) continue;

                    s_StoryTags[tagAttr.Keyword] = new StoryTagInfo
                    {
                        Keyword = tagAttr.Keyword,
                        DefaultParameters = tagAttr.DefaultParameters,
                        Constructor = cons
                    };
                }
            }
        }

        public static StoryScript Decode(string str)
        {
            if (s_StoryTags == null) InitiateStoryTags();

            var script = ScriptableObject.CreateInstance<StoryScript>();
            StoryChunk currentChunk = new();
            script.Chunks.Add(currentChunk);

            string[] lines = str.Split("\n");

            foreach (string line in lines)
            {
                var index = 0;

                if (string.IsNullOrEmpty(line))
                {
                    if (currentChunk.Instructions.Count > 0)
                    {
                        currentChunk = new StoryChunk();
                        script.Chunks.Add(currentChunk);
                    }

                    continue;
                }

                if (line.StartsWith("#")) continue;

                if (currentChunk.Instructions.Count == 0)
                {
                    Match match;
                    SetActorStoryInstruction authorIns = new();
                    currentChunk.Instructions.Add(authorIns);

                    if ((match = sr_ActorParseRegex.Match(line)).Success)
                    {
                        index = match.Groups["content"].Index;

                        authorIns.Actors.AddRange(
                            match.Groups["actor"]
                                .Value.Split(','));
                    }
                }

                // Skip white space
                while (index < line.Length && char.IsWhiteSpace(line[index])) index++;

                while (index < line.Length)
                {
                    StringBuilder storyboard;

                    // Parse control tags
                    if (line[index] == '[')
                    {
                        storyboard = new StringBuilder();
                        index++;

                        while (index < line.Length && char.IsLetterOrDigit(line[index]))
                        {
                            storyboard.Append(line[index]);
                            index++;
                        }

                        var keyword = storyboard.ToString();

                        if (!s_StoryTags.ContainsKey(keyword))
                        {
                            Debug.LogWarning($"Unknown story tag \"{keyword}\"");
                            while (index < line.Length && line[index] != ']') index++;
                            index++;

                            continue;
                        }

                        StoryTagInfo tagInfo = s_StoryTags[keyword];
                        ParameterInfo[] parameters = tagInfo.Constructor.GetParameters();

                        List<string> stringParams = new();

                        // Parse parameters
                        foreach (ParameterInfo param in parameters)
                        {
                            while (index < line.Length && char.IsWhiteSpace(line[index])) index++;
                            storyboard = new StringBuilder();

                            if (line[index] is '"' or '\'')
                            {
                                char bound = line[index];
                                index++;

                                while (index < line.Length && line[index] != bound)
                                {
                                    if (line[index] == '\\') index++;
                                    storyboard.Append(line[index]);
                                    index++;
                                }
                            }
                            else
                            {
                                while (index < line.Length && !char.IsWhiteSpace(line[index]) && line[index] != ']')
                                {
                                    storyboard.Append(line[index]);
                                    index++;
                                }
                            }

                            Debug.Log($"Param \"{param.Name}\": \"{storyboard}\"");
                            stringParams.Add(storyboard.ToString());
                            index++;
                        }

                        Debug.Log(string.Join(", ", stringParams));

                        currentChunk.Instructions.Add(
                            (StoryInstruction)tagInfo.Constructor.Invoke(
                                stringParams
                                    .ToArray())
                        );
                    }

                    // Parse text
                    else
                    {
                        storyboard = new StringBuilder();

                        while (index < line.Length && line[index] != '[')
                        {
                            if (line[index] == '\\')
                            {
                                index++;
                                storyboard.Append(line[index]);
                            }
                            else
                            {
                                storyboard.Append(line[index]);
                            }

                            index++;
                        }

                        currentChunk.Instructions.Add(
                            new TextPrintStoryInstruction
                            {
                                Text = storyboard.ToString()
                            }
                        );
                    }
                }
            }

            if (currentChunk.Instructions.Count == 0) script.Chunks.Remove(currentChunk);

            return script;
        }
    }

    [AttributeUsage(AttributeTargets.Constructor)]
    public class StoryTagAttribute : Attribute
    {
        public readonly string[] DefaultParameters;
        public readonly string   Keyword;

        public StoryTagAttribute(string keyword, params string[] defaultParameters)
        {
            Keyword = keyword;
            DefaultParameters = defaultParameters;
        }
    }

    public class StoryTagInfo
    {
        public ConstructorInfo Constructor;
        public string[]        DefaultParameters;
        public string          Keyword;
    }
}