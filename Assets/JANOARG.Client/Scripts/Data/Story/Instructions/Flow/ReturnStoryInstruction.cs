using UnityEngine;
using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Client.Data.Story.Instructions;
using System.Collections;
using JANOARG.Client.Data.Files;

public class ReturnStoryInstruction : StoryInstruction
{
    public override bool isSilent  => true;
    public override bool endsChunk => true;

    [StoryTag("Return")]
    public ReturnStoryInstruction(){}
    
    public override IEnumerator OnAfterTextReveal(Storyteller storyteller)
    {
        if (storyteller.CallStack.Count == 0)
        {
            Debug.LogWarning("Return called without an active call.");
            yield break;
        }

        int stackIndex = storyteller.CallStack.Count - 1;

        storyteller.PendingChunkIndex = storyteller.CallStack[stackIndex];

        storyteller.CallStack.RemoveAt(stackIndex);

        yield return null;
    }
}