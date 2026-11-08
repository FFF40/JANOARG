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
            new(@"^(?<actor>(?:[0-9a-zA-Z]+\s,\s)*[0-9a-zA-Z]+)\s*::\s*(?<content>.*)");
        private static readonly Regex StoryTagRegex = new(
            @"^\[(?<tag>\w+)\s*\((?<arguments>.*?)\)\]"
        );

        private static readonly Regex ArgumentRegex = new(
            @"[^,]+"
        );

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

            string[] lines = str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                var index = 0;

                // Skip if line is null and empty
                if (string.IsNullOrEmpty(line))
                {
                    if (currentChunk.Instructions.Count > 0)
                    {
                        currentChunk = new StoryChunk();
                        script.Chunks.Add(currentChunk);
                    }

                    continue;
                } else
                {
                    Debug.Log($"Line {line} in Decoder");
                }

                // Skip if comment
                if (line.StartsWith("#")) continue;

                // Parse Scene Setting 
                // SetBackground :: BackgroundID
                if (line.StartsWith("SetBackground"))
                {
                    Debug.Log($"Line {line} in SetBackground");
                    Match match;
                    Regex separator = new(@"(?<action>.+?)\s*::\s*(?<content>.+)");
                    if ((match = separator.Match(line)).Success)
                    {
                        SetBackgroundStoryInstruction backgroundSetIns =
                         new SetBackgroundStoryInstruction(match.Groups["content"].ToString());
                        Debug.Log($"'{match.Groups["content"]}' is the Id");
                        currentChunk.Instructions.Add(backgroundSetIns);
                    }
                    continue;
                }

                // SetMusic :: MusicID
                if (line.StartsWith("SetMusic"))
                {
                    Debug.Log($"Line {line} in SetMusic");
                    Match match;
                    Regex separator = new(@"(?<action>.+?)\s*::\s*(?<content>.+)");
                    if ((match = separator.Match(line)).Success)
                    {
                        SetMusicStoryInstruction musicSetIns =
                         new SetMusicStoryInstruction(match.Groups["content"].ToString());
                        Debug.Log($"'{match.Groups["content"]}' is the Id");
                        currentChunk.Instructions.Add(musicSetIns);
                    }
                    continue;
                }

                // PlaceSet :: Text
                if (line.StartsWith("#")) continue;

                // TimeDateSet :: 16-06-3280, 12:45AM
                if (line.StartsWith("#")) continue;


                // Check if there is a Actor 
                if (currentChunk.Instructions.Count == 0)
                {
                    Match match;
                    SetActorStoryInstruction authorIns = new();
                    currentChunk.Instructions.Add(authorIns);
                    Debug.Log($"Checking if line {line} has actors");
                    // Regex
                    if ((match = sr_ActorParseRegex.Match(line)).Success)
                    {
                        index = match.Groups["content"].Index;
                        Debug.Log($"{match.Groups["actor"]} in Decoder");
                        // Split actor if there is a comma
                        authorIns.Actors.AddRange(
                            match.Groups["actor"]
                                .Value.Split(" , "));
                    }
                }

                // Skip whitespace
                while (index < line.Length && char.IsWhiteSpace(line[index])) index++;

                // Parse text and control tags
                while (index < line.Length)
                {
                    // Parse control tags
                    if (line[index] == '[')
                    {
                        Match match = StoryTagRegex.Match(line.Substring(index));

                        if (match.Success)
                        {
                            string keyword = match.Groups["tag"].Value;
                            string arguments = match.Groups["arguments"].Value;

                            if (!s_StoryTags.TryGetValue(keyword, out StoryTagInfo tagInfo))
                            {
                                Debug.LogWarning($"Unknown story tag \"{keyword}\"");
                                index += match.Length;
                                continue;
                            }

                            ParameterInfo[] parameters = tagInfo.Constructor.GetParameters();
                            List<string> stringParams = new();

                            foreach (Match argMatch in ArgumentRegex.Matches(arguments))
                            {
                                stringParams.Add(argMatch.Value.Trim());
                            }

                            if (stringParams.Count != parameters.Length)
                            {
                                Debug.LogWarning(
                                    $"Tag \"{keyword}\" expects {parameters.Length} arguments, " +
                                    $"but received {stringParams.Count}. {String.Join(",",stringParams)}"
                                );

                                index += match.Length;
                                continue;
                            }

                            currentChunk.Instructions.Add(
                                (StoryInstruction)tagInfo.Constructor.Invoke(stringParams.ToArray())
                            );

                            index += match.Length;
                            continue;
                        } 

                        // Debug.LogWarning($"Invalid story tag format: {line}");
                        // index++;
                        
                    }

                    // Parse ordinary text
                    StringBuilder storyboard = new();

                    while (index < line.Length && line[index] != '[')
                    {
                        if (line[index] == '\\' && index + 1 < line.Length)
                            index++;

                        storyboard.Append(line[index]);
                        index++;
                    }

                    if (index < line.Length && line[index] == '[' && storyboard.Length == 0)
                    {
                        storyboard.Append(line[index]);
                        index++;
                    }

                    if (storyboard.Length > 0)
                    {
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