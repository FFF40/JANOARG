using System.Collections;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;
using JANOARG.Client.Data.Story.Instructions;

public class AddVariableStoryInstruction : StoryInstruction
{
    public string Name, Amount;

    [StoryTag("Add")]
    public AddVariableStoryInstruction(string name, string amount) { Name = name; Amount = amount; }

    public override bool isSilent => true;

    public override IEnumerator OnAfterTextReveal(Storyteller t)
    {
        t.Variables.TryGetValue(Name, out int cur);
        t.Variables[Name] = cur + t.ValueOf(Amount);
        yield break;
    }
}