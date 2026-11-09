using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JANOARG.Client.Data.Story;
using JANOARG.Client.Data.Story.Instructions;
using UnityEngine;
using System.Linq;

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
        private static readonly Regex CommandRegex =
            new(@"^(?<action>Section|SetBackground|SetMusic|If)\s*::\s*(?<content>.+)$");
        private static readonly Regex ChoiceRegex =
            new(@"^\*\s*(?<text>.+?)\s*->\s*(?<target>\w+)(?:\s*\?\s*(?<cond>.+))?$");

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
            var currentChunk = new StoryChunk();
            script.Chunks.Add(currentChunk);

            var condStack = new List<StoryCondition>();
            ChoiceStoryInstruction openChoice = null;
            bool splitPending = false; // next instruction must start a new chunk
            bool actorChecked = false; // already looked for "Actor ::" in this chunk

            StoryChunk Chunk()
            {
                if (splitPending && currentChunk.Instructions.Count > 0)
                {
                    currentChunk = new StoryChunk();
                    script.Chunks.Add(currentChunk);
                    actorChecked = false;
                }

                if (currentChunk.Instructions.Count == 0)   // empty chunk: (re)stamp conditions
                    currentChunk.Conditions = condStack
                        .Select(c => new StoryCondition { Expression = c.Expression, Negate = c.Negate })
                        .ToList();

                splitPending = false;
                return currentChunk;
            }

            void Add(StoryInstruction ins)
            {
                Chunk().Instructions.Add(ins);
                if (ins.endsChunk) splitPending = true;
            }

            string[] lines = str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            foreach (string rawLine in lines)
            {
                // Trim: trailing spaces after a tag would otherwise become a text instruction
                string line = rawLine.Trim();

                if (line.Length == 0) { splitPending = true; continue; }
                if (line.StartsWith("#")) continue;
                
                // Else / EndIf
                if (line == "Else")
                {
                    if (condStack.Count == 0) Debug.LogWarning("Else without If.");
                    else condStack[^1].Negate = !condStack[^1].Negate;
                    splitPending = true; openChoice = null; continue;
                }
                if (line == "EndIf")
                {
                    if (condStack.Count == 0) Debug.LogWarning("EndIf without If.");
                    else condStack.RemoveAt(condStack.Count - 1);
                    splitPending = true; openChoice = null; continue;
                }

                // Choice options: "* Text -> section ? condition"
                if (line.StartsWith("*"))
                {
                    Match cm = ChoiceRegex.Match(line);
                    if (!cm.Success) { Debug.LogWarning($"Invalid choice line: {line}"); continue; }

                    if (openChoice == null)
                    {
                        openChoice = new ChoiceStoryInstruction();
                        Chunk().Instructions.Add(openChoice);
                    }

                    openChoice.Options.Add(new ChoiceOption
                    {
                        Text = cm.Groups["text"].Value,
                        Target = cm.Groups["target"].Value,
                        Condition = cm.Groups["cond"].Success ? cm.Groups["cond"].Value.Trim() : null
                    });
                    splitPending = true;   // whatever follows starts a new chunk
                    continue;
                }


                // Commands: Section / SetBackground / SetMusic
                Match cmd = CommandRegex.Match(line);
                if (cmd.Success)
                {
                    string content = cmd.Groups["content"].Value.Trim();
                    switch (cmd.Groups["action"].Value)
                    {
                        case "Section":
                        {
                            splitPending = true;              // a section always begins a chunk
                            StoryChunk chunk = Chunk();
                            var sec = new SectionStoryInstruction(content, script.Chunks.Count - 1);
                            chunk.Instructions.Add(sec);
                            script.Sections[sec.Name] = sec.Index;
                            break;
                        }
                        case "If":
                        {
                            condStack.Add(new StoryCondition { Expression = content });
                            splitPending = true;
                            break;
                        }
                        case "SetBackground": Add(new SetBackgroundStoryInstruction(content)); break;
                        case "SetMusic":      Add(new SetMusicStoryInstruction(content));      break;
                    }
                    continue;
                }

                int index = 0;

                // Actor prefix (first line of each chunk only)
                Chunk(); // may start a new chunk and reset actorChecked
                if (!actorChecked)
                {
                    actorChecked = true;
                    Match m = sr_ActorParseRegex.Match(line);
                    if (m.Success)
                    {
                        index = m.Groups["content"].Index;
                        var authorIns = new SetActorStoryInstruction();
                        authorIns.Actors.AddRange(m.Groups["actor"].Value.Split(" , "));
                        Add(authorIns);
                    }
                }

                while (index < line.Length && char.IsWhiteSpace(line[index])) index++;

                while (index < line.Length)
                {
                    if (line[index] == '[')
                    {
                        Match match = StoryTagRegex.Match(line.Substring(index));
                        if (match.Success)
                        {
                            string keyword   = match.Groups["tag"].Value;
                            string arguments = match.Groups["arguments"].Value;
                            index += match.Length;

                            if (!s_StoryTags.TryGetValue(keyword, out StoryTagInfo tagInfo))
                            {
                                Debug.LogWarning($"Unknown story tag \"{keyword}\"");
                                continue;
                            }

                            ParameterInfo[] parameters = tagInfo.Constructor.GetParameters();
                            var stringParams = new List<string>();
                            foreach (Match a in ArgumentRegex.Matches(arguments))
                                stringParams.Add(a.Value.Trim());

                            if (stringParams.Count != parameters.Length)
                            {
                                Debug.LogWarning($"Tag \"{keyword}\" expects {parameters.Length} arguments, " +
                                                $"but received {stringParams.Count}.");
                                continue;
                            }

                            Add((StoryInstruction)tagInfo.Constructor.Invoke(stringParams.ToArray()));
                            continue;
                        }
                    }

                    // Ordinary text
                    var sb = new StringBuilder();
                    while (index < line.Length && line[index] != '[')
                    {
                        if (line[index] == '\\' && index + 1 < line.Length) index++;
                        sb.Append(line[index]);
                        index++;
                    }
                    if (sb.Length == 0 && index < line.Length) { sb.Append(line[index]); index++; } // lone '['

                    if (sb.Length > 0)
                        Add(new TextPrintStoryInstruction { Text = sb.ToString() });
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