using System.Collections;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;
using JANOARG.Client.Data.Story.Instructions;

public class SetVariableStoryInstruction : StoryInstruction
{
    public string Name, Value;

    [StoryTag("Set")]
    public SetVariableStoryInstruction(string name, string value) { Name = name; Value = value; }

    public override bool isSilent => true;

    public override IEnumerator OnAfterTextReveal(Storyteller t)
    {
        t.Variables[Name] = t.ValueOf(Value);
        yield break;
    }
}