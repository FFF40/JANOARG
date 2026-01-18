using System;
using System.Collections;
using JANOARG.Client.Data.Constant;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Client.Data.Story;
using JANOARG.Client.Data.Story.Instructions;
using JANOARG.Client.Data.Story.TypeEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Storyteller
{
    public class Storyteller : MonoBehaviour
    {
        public static Storyteller sMain;

        public StoryConstants Constants;

        [Space]
        public StoryScript CurrentScript;

        public int CurrentChunkIndex;
        public int CurrentCharacterIndex;

        [Space]
        public TMP_Text DialogueLabel;

        public TMP_Text      NameLabel;
        public RectTransform NameLabelHolder;
        public CanvasGroup   NameLabelGroup;
        public Graphic       NextChunkIndicator;

        [Space]
        public float CharacterDuration = 0.01f;

        public float SpeedFactor = 1;

        public StoryTypeEffect CurrentRevealEffect;

        public int[] CurrentVertexIndexes;

        private                bool  _IsPlaying = false;
        [NonSerialized] public bool  IsMeshDirty;
        [NonSerialized] public float TimeBuffer       = 0;
        [NonSerialized] public int   ActiveCoroutines = 0;

        public void Awake()
        {
            sMain = this;
        }

        public void Start()
        {
            DialogueLabel.text = "";
        }

        public void Update()
        {
            if (_IsPlaying) TimeBuffer += Time.deltaTime * SpeedFactor;

            if (IsMeshDirty)
            {
                DialogueLabel.ForceMeshUpdate();
                ResetDialogueMesh();
                IsMeshDirty = false;
            }
        }

        public void LateUpdate()
        {
            if (IsMeshDirty) UpdateDialogueMesh();
        }

        public void PlayScript(StoryScript script)
        {
            CurrentScript = script;
            CurrentChunkIndex = 0;

            StartCoroutine(PlayChunk());
        }

        public void OnScreenClick()
        {
            if (_IsPlaying)
                SpeedFactor *= 5;
            else
                PlayNextChunk();
        }

        public void PlayNextChunk()
        {
            CurrentChunkIndex++;
            StartCoroutine(PlayChunk());
        }

        public IEnumerator PlayChunk()
        {
            _IsPlaying = true;
            StoryChunk chunk = CurrentScript.Chunks[CurrentChunkIndex];

            SetNextChunkIndicatorState(0);
            Vector2 dialoguePos = DialogueLabel.rectTransform.anchoredPosition;

            yield return Ease.Animate(0.2f, (x) =>
            {
                float ease = Ease.Get(x, EaseFunction.Quadratic, EaseMode.Out);
                DialogueLabel.color = new Color(1, 1, 1, 1 - ease);

                float ease2 = Ease.Get(x, EaseFunction.Cubic, EaseMode.In);
                DialogueLabel.rectTransform.anchoredPosition = dialoguePos + ease2 * 5 * Vector2.down;
            });

            DialogueLabel.text = "";
            DialogueLabel.color = Color.white;
            DialogueLabel.rectTransform.anchoredPosition = dialoguePos;

            foreach (StoryInstruction instruction in chunk.Instructions)
                instruction.OnTextBuild(this);

            CurrentCharacterIndex = 0;
            TimeBuffer = 0;
            SpeedFactor = 1;
            CurrentVertexIndexes = new int[DialogueLabel.textInfo.meshInfo.Length];
            CurrentRevealEffect = new FloatInTypeEffect();
            IsMeshDirty = true;
            ResetDialogueMesh();

            foreach (StoryInstruction instruction in chunk.Instructions)
            {
                IEnumerator chunkRoutine = instruction.OnTextReveal(this);

                if (chunkRoutine != null) yield return chunkRoutine;
            }

            yield return new WaitWhile(() => ActiveCoroutines > 0);

            _IsPlaying = false;
            SetNextChunkIndicatorState(1);
        }

        public void ResetDialogueMesh()
        {
            TMP_TextInfo textInfo = DialogueLabel.textInfo;

            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                for (int j = CurrentVertexIndexes[i]; j < textInfo.meshInfo[i].colors32.Length; j++)
                    textInfo.meshInfo[i]
                        .colors32[j].a = 0;

                textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                DialogueLabel.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }

        public void UpdateDialogueMesh()
        {
            TMP_TextInfo textInfo = DialogueLabel.textInfo;

            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                DialogueLabel.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }

        public void RegisterCoroutine(IEnumerator routine)
        {
            IEnumerator f_registration()
            {
                ActiveCoroutines++;

                yield return routine;

                ActiveCoroutines--;
            }

            StartCoroutine(f_registration());
        }

        private float     _CurrentNextChunkIndicatorPosition = 0;
        private Coroutine _CurrentNextChunkIndicatorRoutine  = null;

        public void SetNextChunkIndicatorState(float target)
        {
            if (_CurrentNextChunkIndicatorRoutine != null) StopCoroutine(_CurrentNextChunkIndicatorRoutine);
            _CurrentNextChunkIndicatorRoutine = StartCoroutine(NextChunkIndicatorAnim(target));
        }

        private IEnumerator NextChunkIndicatorAnim(float target)
        {
            float from = _CurrentNextChunkIndicatorPosition;

            yield return Ease.Animate(0.2f, (x) =>
            {
                x = Mathf.Lerp(from, target, x);
                _CurrentNextChunkIndicatorPosition = x;
                float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                NextChunkIndicator.color = new Color(1, 1, 1, x);
                NextChunkIndicator.rectTransform.anchoredPosition = 5 * (1 - ease) * Vector2.down;
            });
        }

        private Coroutine _CurrentNameFieldRoutine = null;

        public void SetNameLabelText(string label)
        {
            if (_CurrentNameFieldRoutine != null) StopCoroutine(_CurrentNameFieldRoutine);
            _CurrentNameFieldRoutine = StartCoroutine(NameLabelAnim(label));
        }

        private IEnumerator NameLabelAnim(string label)
        {
            float fromAlpha = NameLabelGroup.alpha;
            float toAlpha = string.IsNullOrWhiteSpace(label) ? 0 : 1;
            float fromXPos = NameLabelHolder.anchorMin.x;
            float toXPos = 0;

            if (toAlpha > 0)
            {
                NameLabel.text = label;
                NameLabelHolder.sizeDelta = new Vector2(NameLabel.preferredWidth, NameLabelHolder.sizeDelta.y);
            }

            if (fromAlpha == 0) fromXPos = toXPos;

            yield return Ease.Animate(0.2f, (x) =>
            {
                float ease = Ease.Get(x, EaseFunction.Quadratic, EaseMode.Out);
                NameLabelGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, ease);
                float xPos = Mathf.Lerp(fromXPos, toXPos, ease);
                NameLabelHolder.anchorMin = NameLabelHolder.anchorMax = NameLabelHolder.pivot = new Vector2(xPos, 0.5f);
                NameLabelHolder.anchoredPosition = new Vector2(0, (1 - NameLabelGroup.alpha) * -5);
            });
        }
    }
}
