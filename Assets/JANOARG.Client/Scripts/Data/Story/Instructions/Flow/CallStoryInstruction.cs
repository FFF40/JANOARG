using System;
using System.Collections;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Files;
using JANOARG.Client.Data.Story.Instructions;

public class CallStoryInstruction : StoryInstruction
{
    public override bool isSilent  => true;
    public override bool endsChunk => true;
    
    public string TargetLabel;

    [StoryTag("Call")]
    public CallStoryInstruction(string target)
    {
        TargetLabel = target;
    }

    public override IEnumerator OnAfterTextReveal(Storyteller storyteller)
    {
        storyteller.CallStack.Add(
            storyteller.CurrentChunkIndex
        );

        storyteller.PendingSectionJump = TargetLabel;
        yield return null;
    }
}