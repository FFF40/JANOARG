using System.Collections;
using System.Collections.Generic;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;
using JANOARG.Client.Data.Story.Instructions;
using JANOARG.Client.Data.Story;
using System.Linq;
using UnityEngine;
public class ChoiceStoryInstruction : StoryInstruction
{
    public List<ChoiceOption> Options = new();

    public override bool endsChunk => true;

    public override IEnumerator OnAfterTextReveal(Storyteller t)
    {
        var visible = Options
            .Where(o => string.IsNullOrEmpty(o.Condition) || t.Evaluate(o.Condition))
            .ToList();

        if (visible.Count == 0)
        {
            Debug.LogWarning("Choice has no available options, continuing.");
            yield break;
        }

        ChoiceOption picked = null;
        yield return t.ShowChoices(visible, o => picked = o);
        t.PendingSectionJump = picked.Target;
    }
}