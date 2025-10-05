using System;
using System.Collections;
using JANOARG.Client.Behaviors.Storyteller;
using TMPro;

namespace JANOARG.Client.Data.Story.Instructions
{
    [Serializable]
    public class TextPrintStoryInstruction : StoryInstruction
    {
        public string Text;

        private int _StopPoint;

        public string name => $"Say \"{Text}\"";

        public override void OnTextBuild(Storyteller teller)
        {
            teller.DialogueLabel.text += Text;
            teller.DialogueLabel.ForceMeshUpdate();
            _StopPoint = teller.DialogueLabel.textInfo.characterCount;
        }

        public override IEnumerator OnTextReveal(Storyteller teller)
        {
            var isWait = false;

            while (teller.CurrentCharacterIndex < _StopPoint)
            {
                int index = teller.CurrentCharacterIndex;
                TMP_CharacterInfo charInfo = teller.DialogueLabel.textInfo.characterInfo[index];

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

                    teller.RegisterCoroutine(
                        teller.CurrentRevealEffect.OnCharacterReveal(
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