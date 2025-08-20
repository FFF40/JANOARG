using System;
using System.Collections;
using JANOARG.Client.Behaviors.Storyteller;
using TMPro;
using UnityEngine;

namespace JANOARG.Shared.Data.Story.TypeEffects
{
    [Serializable]
    public class FloatInTypeEffect : StoryTypeEffect
    {
        const float Duration = 0.1f;

        public override IEnumerator OnCharacterReveal(Storyteller teller, TMP_Text text, int index, float startTime) 
        {
            var charInfo = text.textInfo.characterInfo[index];
            if (charInfo.vertexIndex < 0) yield break;
        
            for (float timer = startTime; timer < Duration; timer += Time.deltaTime * teller.SpeedFactor) 
            {
                SetLerp(teller, text, index, timer / Duration);
                yield return null;
            }
            SetLerp(teller, text, index, 1);
        }

        public void SetLerp(Storyteller teller, TMP_Text text, int index, float lerp)
        {
            var charInfo = text.textInfo.characterInfo[index];
            var meshInfo = text.textInfo.meshInfo[charInfo.materialReferenceIndex];

            var vertIndex = charInfo.vertexIndex;
            Vector3 vertOffset = (1 - lerp) * (1 - lerp) * 5 * Vector3.down;

            for (int a = 0; a < 4; a ++)
            {
                meshInfo.colors32[a + vertIndex].a = (byte)(charInfo.color.a * Mathf.Clamp01(lerp * 2));
                meshInfo.vertices[a + vertIndex] += vertOffset;
            }

            teller.IsMeshDirty = true;
        }
    }
}