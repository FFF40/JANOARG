using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class TextPrintStoryInstruction : StoryInstruction 
{
    public string Name => $"Say \"{Text}\"";
    public string Text;

    private int StopPoint;

    public override void OnTextBuild(Storyteller teller)
    {
        teller.DialogueLabel.text += Text;
        teller.DialogueLabel.ForceMeshUpdate();
        StopPoint = teller.DialogueLabel.textInfo.characterCount;
    }

    public override IEnumerator OnTextReveal(Storyteller teller)
    {
        bool isWait = false;
        while (teller.CurrentCharacterIndex < StopPoint) 
        {
            int index = teller.CurrentCharacterIndex;
            var charInfo = teller.DialogueLabel.textInfo.characterInfo[index];

            float waitTime = teller.CharacterDuration;
            switch (charInfo.character)
            {
                case ',': case ';': case '.': case '?': case '!':
                case ')': case ']': case '}':
                    isWait = true;
                    break;
                case '’': case '”': case '\'': case '"':
                    break;
                default: 
                    if (isWait)
                    {
                        waitTime *= 3;
                        waitTime += 0.25f;
                    }
                    isWait = false;
                    break;
            }
            while (teller.TimeBuffer < waitTime) yield return null;
            teller.TimeBuffer -= waitTime;

            if (charInfo.isVisible)
            {
                teller.CurrentVertexIndexes[charInfo.materialReferenceIndex] = charInfo.vertexIndex + 4;
                teller.RegisterCoroutine(teller.currentRevealEffect.OnCharacterReveal(
                    teller,
                    teller.DialogueLabel,
                    index,
                    teller.TimeBuffer
                ));
            }

            teller.CurrentCharacterIndex++;
        }
    }
}