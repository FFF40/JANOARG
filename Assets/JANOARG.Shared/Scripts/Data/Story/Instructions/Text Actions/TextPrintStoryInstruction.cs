using System;
using System.Collections;
using UnityEngine;

using JANOARG.Client.Behaviors.Storyteller;
using JANOARG.Shared.Data.ChartInfo;

namespace JANOARG.Shared.Data.Story.Instructions
{
    [Serializable]
    public class TextPrintStoryInstruction : StoryInstruction
    {
        public string name => $"Say \"{Text}\"";
        public string Text;

        private int _StopPoint;

        /// <summary>
        /// It appends the text to the dialogue label and updates the stop point.
        /// </summary>
        public override void OnTextBuild(Storyteller teller)
        {
            teller.DialogueLabel.text += Text;
            teller.DialogueLabel.ForceMeshUpdate();
            _StopPoint = teller.DialogueLabel.textInfo.characterCount;
        }

        public override IEnumerator OnTextAreaSwitch(Storyteller teller)
        {
            // Full Screen Narration is active, deactivate it
            if (teller.FullScreenNarrationGroup.alpha == 1f)
            {
                yield return Ease.Animate(1f, (a) =>
                {
                    float lerp = Ease.Get(1 - a, EaseFunction.Cubic, EaseMode.Out);
                    teller.FullScreenNarrationGroup.alpha = lerp;
                    teller.InterfaceGroup.alpha = 1 - lerp;
                });
            }
        }

        /// <summary>
        /// It reveals the text character by character with a delay based on punctuation.
        /// This method is called when the text is being revealed.
        /// It checks the current character index and updates the vertex indexes for the text mesh.
        /// </summary>
        public override IEnumerator OnTextReveal(Storyteller teller)
        {
            bool isWait = false;


            // Wait until the current character index is less than the stop point
            while (teller.CurrentCharacterIndex < _StopPoint)
            {
                // Current Character Info
                int index = teller.CurrentCharacterIndex;
                var charInfo = teller.DialogueLabel.textInfo.characterInfo[index];

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
                    teller.RegisterCoroutine(teller.CurrentRevealEffect.OnCharacterReveal(
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
}