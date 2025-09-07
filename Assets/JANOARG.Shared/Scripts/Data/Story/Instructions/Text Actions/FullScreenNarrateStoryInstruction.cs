using System;
using System.Collections;
using UnityEngine;

//STILL IN PROGRESS <-------------------------------------------------------------
[Serializable]
public class FullScreenNarrateStoryInstruction : StoryInstruction 
{
    public string Name => $"Say \"{Text}\"";
    public string Text;

    private int StopPoint;

    public override void OnTextBuild(Storyteller teller)
    {
        teller.FullScreenNarrationText.text = Text;
        teller.FullScreenNarrationText.ForceMeshUpdate();
        StopPoint = teller.FullScreenNarrationText.textInfo.characterCount;
    }

    public override IEnumerator OnTextAreaSwitch(Storyteller teller)
    {
        // Full Screen Narration is not active, activate it
        if (teller.FullScreenNarrationGroup.alpha == 0f)
        {
            yield return Ease.Animate(1f, (a) =>
            {
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                teller.FullScreenNarrationGroup.alpha = lerp;
                teller.InterfaceGroup.alpha = 1-lerp;
            });
        }
    }

    public override IEnumerator OnTextReveal(Storyteller teller)
    {
        bool isWait = false;
        while (teller.CurrentCharacterIndex < StopPoint)
        {
            int index = teller.CurrentCharacterIndex;
            var charInfo = teller.FullScreenNarrationText.textInfo.characterInfo[index];

            var textInfo = teller.FullScreenNarrationText.textInfo;

            if (teller.CurrentVertexIndexes == null || teller.CurrentVertexIndexes.Length != textInfo.meshInfo.Length)
                teller.CurrentVertexIndexes = new int[textInfo.meshInfo.Length];


            if (!charInfo.isVisible)
            {
                teller.CurrentCharacterIndex++;
                continue;
            }

            float waitTime = teller.CharacterDuration;
            switch (charInfo.character)
            {
                case ',':
                case ';':
                case '.':
                case '?':
                case '!':
                case ')':
                case ']':
                case '}':
                    isWait = true;
                    break;
                case '’':
                case '”':
                case '\'':
                case '"':
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
                    teller.FullScreenNarrationText,
                    index,
                    teller.TimeBuffer
                ));
            }

            teller.CurrentCharacterIndex++;
        }
    }
}