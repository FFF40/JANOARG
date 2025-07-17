using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

public class StoryDecoder
{
    public const int FormatVersion = 1;
    public const int IndentSize = 2;
    static Dictionary<string, StoryInstruction> InstructionList = new Dictionary<string, StoryInstruction>();

    static Dictionary<string, StoryTagInfo> StoryTags;

#if UNITY_EDITOR
    [DidReloadScripts]
#endif
    public static void InitiateStoryTags()
    {
        StoryTags = new();
        var asm = Assembly.GetAssembly(typeof(StoryInstruction));
        foreach (var cls in asm.GetTypes())
        {
            if (!typeof(StoryInstruction).IsAssignableFrom(cls)) continue;
            foreach (var cons in cls.GetConstructors())
            {
                foreach (var attr in cons.GetCustomAttributes())
                {
                    if (attr is not StoryTagAttribute tagAttr) continue;
                    StoryTags[tagAttr.Keyword] = new()
                    {
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

            // Skip comments
            if (line.StartsWith("#"))
            {
                continue;
            }

            // Add substitute control tags
            if (line.StartsWith("[["))
            {
                Match match = instructionParseRegex.Match(line);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    string instructionText = match.Groups[2].Value;


                    string[] parts = instructionText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0)
                    {
                        Debug.LogWarning($"No keyword in instruction: {line}");
                    }

                    string keyword = parts[0];
                    string[] args = parts.Skip(1).ToArray();

                    if (!StoryTags.ContainsKey(keyword))
                    {
                        Debug.LogWarning($"Unknown story tag keyword \"{keyword}\" in line: {line}");
                    }

                    var tagInfo = StoryTags[keyword];
                    var parameters = tagInfo.Constructor.GetParameters();
                    List<string> stringParams = new();

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        stringParams.Add(i < args.Length ? args[i] : "");
                    }

                    Debug.Log($"Instruction ID: {id}, Keyword: {keyword}, Params: {string.Join(", ", stringParams)}");

                    try
                    {
                        var instruction = (StoryInstruction)tagInfo.Constructor.Invoke(stringParams.ToArray());
                        InstructionList[id] = instruction;

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error constructing instruction [[{id}]] {keyword}: {ex.Message}");
                    }
                    
                }
                else
                {
                    Debug.LogWarning($"Line format not recognized: {line}");
                }
                continue;
            }

            // Add Decisions
            if (line.StartsWith("!")) {
                continue;
            }

            //Add Decision Checks
            if (line.StartsWith("!"))
            {
                continue;
            }

            // Add actors
            List<string> currentChunkActors = new List<string>();
            if (currentChunk.Instructions.Count == 0)
            {
                Match match;
                var authorIns = new SetActorStoryInstruction();
                currentChunk.Instructions.Add(authorIns);
                if ((match = actorParseRegex.Match(line)).Success)
                {
                    index = match.Groups["content"].Index;
                    authorIns.Actors.AddRange(match.Groups["actor"].Value.Split(','));
                    currentChunkActors.AddRange(match.Groups["actor"].Value.Split(','));
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

                //Parse reference tags
                if (line[index] == '[' && line[index + 1] == '[')
                {
                    Match match = instructionRefRegex.Match(line, index);
                    if (match.Success)
                    {
                        string id = match.Groups[1].Value;

                        try
                        {
                            if (InstructionList.TryGetValue(id, out var instruction))
                            {
                                currentChunk.Instructions.Add(instruction);
                            }
                            else
                            {
                                Debug.LogWarning($"Instruction ID not found: [[{id}]]");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Error adding instruction [[{id}]] from line: {line}");
                            Debug.LogError(ex);
                        }

                        index = match.Index + match.Length; // Move index past match
                        
                    }
                }
                // Parse control tags
                else if (line[index] == '[')
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
                        sb = new();
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

                    Debug.Log($"Line: {line} \n{keyword} : {string.Join(", ", stringParams)}");

                    try
                    {
                        currentChunk.Instructions.Add(
                            (StoryInstruction)tagInfo.Constructor.Invoke(stringParams.ToArray())
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"StoryInstruction in line {line} have Error: {keyword},{stringParams[0]},{currentChunkActors[0]}");
                        Debug.LogError(ex);
                    }
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
                        new TextPrintStoryInstruction()
                        {
                            Text = sb.ToString()
                        }
                    );
                }
            }
        }

        if (currentChunk.Instructions.Count == 0) script.Chunks.Remove(currentChunk);

        return script;
    }

    static readonly Regex actorParseRegex           = new(@"^(?<actor>(?:[0-9a-zA-Z]+,)*[0-9a-zA-Z]+)\s*>\s+(?<content>.*)");
    static readonly Regex instructionParseRegex = new(@"\[\[([a-zA-Z0-9_]+)\]\]\s*(.+)");
    static readonly Regex instructionRefRegex = new(@"\[\[([a-zA-Z0-9_]+)\]\]");

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