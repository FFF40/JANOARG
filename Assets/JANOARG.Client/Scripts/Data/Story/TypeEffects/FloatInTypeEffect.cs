using System;
using System.Collections;
using JANOARG.Client.Behaviors.Storyteller;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Data.Story.TypeEffects
{
    [Serializable]
    public class FloatInTypeEffect : StoryTypeEffect
    {
        private const float _DURATION = 0.1f;


        public override IEnumerator OnCharacterReveal(Storyteller teller, TMP_Text text, int index, float startTime)
        {
            TMP_CharacterInfo charInfo = text.textInfo.characterInfo[index];

            if (charInfo.vertexIndex < 0) yield break;

            for (float timer = startTime; timer < _DURATION; timer += Time.deltaTime * teller.SpeedFactor)
            {
                SetLerp(teller, text, index, timer / _DURATION);

                yield return null;
            }

            SetLerp(teller, text, index, 1);
        }

        public void SetLerp(Storyteller teller, TMP_Text text, int index, float lerp)
        {
            TMP_CharacterInfo charInfo = text.textInfo.characterInfo[index];
            TMP_MeshInfo meshInfo = text.textInfo.meshInfo[charInfo.materialReferenceIndex];

            int vertIndex = charInfo.vertexIndex;
            Vector3 vertOffset = (1 - lerp) * (1 - lerp) * 5 * Vector3.down;

            for (var a = 0; a < 4; a++)
            {
                meshInfo.colors32[a + vertIndex].a = (byte)(charInfo.color.a * Mathf.Clamp01(lerp * 2));
                meshInfo.vertices[a + vertIndex] += vertOffset;
            }

            teller.IsMeshDirty = true;
        }
    }
}