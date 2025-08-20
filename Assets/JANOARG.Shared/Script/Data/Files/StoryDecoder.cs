using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JANOARG.Shared.Script.Data.Story;
using JANOARG.Shared.Script.Data.Story.Instructions;
using UnityEditor.Callbacks;
using UnityEngine;

namespace JANOARG.Shared.Script.Data.Files
{
#endif

    public class StoryDecoder
    {
        public const int FormatVersion = 1;
        public const int IndentSize = 2;

        static Dictionary<string, StoryTagInfo> StoryTags;

#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        public static void InitiateStoryTags() 
        {
            StoryTags = new ();
            var asm = Assembly.GetAssembly(typeof(StoryInstruction));
            foreach (var cls in asm.GetTypes())
            {
                if (!typeof(StoryInstruction).IsAssignableFrom(cls)) continue;
                foreach (var cons in cls.GetConstructors())
                {
                    foreach (var attr in cons.GetCustomAttributes()) 
                    {
                        if (attr is not StoryTagAttribute tagAttr) continue;
                        StoryTags[tagAttr.Keyword] = new () {
                            Keyword = tagAttr.Keyword,
                            DefaultParameters = tagAttr.DefaultParameters,
                            Constructor = cons,
                        };
                    }
                }
            }
        }

        public static StoryScript Decode(string str)
        {
            if (StoryTags == null) InitiateStoryTags();

            StoryScript script = ScriptableObject.CreateInstance<StoryScript>();
            StoryChunk currentChunk = new();
            script.Chunks.Add(currentChunk);

            string[] lines = str.Split("\n");
            foreach (var line in lines)
            {
                int index = 0;
                if (string.IsNullOrEmpty(line)) 
                {
                    if (currentChunk.Instructions.Count > 0) 
                    {
                        currentChunk = new();
                        script.Chunks.Add(currentChunk);
                    }
                    continue;
                }
                if (line.StartsWith("#")) 
                {
                    continue;
                }

                if (currentChunk.Instructions.Count == 0)
                {
                    Match match;
                    var authorIns = new SetActorStoryInstruction();
                    currentChunk.Instructions.Add(authorIns);
                    if ((match = actorParseRegex.Match(line)).Success) 
                    {
                        index = match.Groups["content"].Index;
                        authorIns.Actors.AddRange(match.Groups["actor"].Value.Split(','));
                    }
                }

                // Skip white space
                while (index < line.Length && char.IsWhiteSpace(line[index]))
                {
                    index++;
                }

                while (index < line.Length)
                {
                    StringBuilder sb;
                    // Parse control tags
                    if (line[index] == '[')
                    {
                        sb = new();
                        index++;
                        while (index < line.Length && char.IsLetterOrDigit(line[index])) 
                        {
                            sb.Append(line[index]);
                            index++;
                        }
                        string keyword = sb.ToString();
                        if (!StoryTags.ContainsKey(keyword)) 
                        {
                            Debug.LogWarning($"Unknown story tag \"{keyword}\"");
                            while (index < line.Length && line[index] != ']') index++; 
                            index++;
                            continue;
                        }
                        var tagInfo = StoryTags[keyword];
                        var parameters = tagInfo.Constructor.GetParameters();

                        List<string> stringParams = new();

                        // Parse parameters
                        foreach (var param in parameters) 
                        {
                            while (index < line.Length && char.IsWhiteSpace(line[index])) index++; 
                            sb = new ();
                            if (line[index] is '"' or '\'') 
                            {
                                char bound = line[index];
                                index++;
                                while (index < line.Length && line[index] != bound) 
                                {
                                    if (line[index] == '\\') index++;
                                    sb.Append(line[index]);
                                    index++;
                                }
                            }
                            else 
                            {
                                while (index < line.Length && !char.IsWhiteSpace(line[index]) && line[index] != ']') 
                                {
                                    sb.Append(line[index]);
                                    index++;
                                }
                            }
                            Debug.Log($"Param \"{param.Name}\": \"{sb}\"");
                            stringParams.Add(sb.ToString());
                            index++;
                        }

                        Debug.Log(string.Join(", ", stringParams));
                        currentChunk.Instructions.Add(
                            (StoryInstruction)tagInfo.Constructor.Invoke(stringParams.ToArray())
                        );
                    }
                    // Parse text
                    else 
                    {
                        sb = new();
                        while (index < line.Length && line[index] != '[') 
                        {
                            if (line[index] == '\\') 
                            {
                                index++;
                                sb.Append(line[index]);
                            }
                            else 
                            {
                                sb.Append(line[index]);
                            }
                            index++;
                        }

                        currentChunk.Instructions.Add(
                            new TextPrintStoryInstruction() {
                                Text = sb.ToString()
                            }
                        );
                    }
                }
            } 

            if (currentChunk.Instructions.Count == 0) script.Chunks.Remove(currentChunk);

            return script;
        }

        static readonly Regex actorParseRegex = new(@"^(?<actor>(?:[0-9a-zA-Z]+,)*[0-9a-zA-Z]+)\s*>\s+(?<content>.*)");
    }

    [AttributeUsage(AttributeTargets.Constructor)]
    public class StoryTagAttribute : Attribute
    {
        public readonly string Keyword;
        public readonly string[] DefaultParameters;

        public StoryTagAttribute(string keyword, params string[] defaultParameters)
        {
            Keyword = keyword;
            DefaultParameters = defaultParameters;
        }
    }

    public class StoryTagInfo
    {
        public string Keyword;
        public string[] DefaultParameters;
        public ConstructorInfo Constructor;
    }
}