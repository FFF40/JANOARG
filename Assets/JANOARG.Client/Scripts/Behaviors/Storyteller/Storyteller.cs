using System;
using System.Collections;
using System.Collections.Generic;
using JANOARG.Client.Behaviors.Player;
using JANOARG.Client.Data.Constant.Story;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Data.Story;
using JANOARG.Shared.Data.Story.Instructions;
using JANOARG.Shared.Data.Story.TypeEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Storyteller
{
    public class Storyteller : MonoBehaviour
    {
        public static Storyteller sMain;

        public StoryConstants Constants;
        public StoryAudioConstants AudioConstants;

        [Space]
        public StoryScript CurrentScript;

        public int CurrentChunkIndex;
        public int CurrentCharacterIndex;

        [Space]
        public CanvasGroup InterfaceGroup;
        public TMP_Text DialogueLabel;
        public TMP_Text NameLabel;
        public RectTransform NameLabelHolder;
        public CanvasGroup NameLabelGroup;
        public Graphic NextChunkIndicator;

        [Space]
        public TMP_Text PlaceText;
        public TMP_Text TimeText;
        public TMP_Text MusicCreditText;

        public Image BackgroundImage;

        public CanvasGroup FullScreenNarrationGroup;
        public TMP_Text FullScreenNarrationText;

        public AudioSource BackgroundMusicPlayer;
        public AudioSource SoundEffectsPlayer;
        [NonSerialized] public float MaxVolume;

        public RectTransform ActorHolder;
        public ActorSpriteHandler ActorSpriteItem;
        public List<ActorSpriteHandler> Actors;
        public List<ActorInfo> CurrentActors = new List<ActorInfo>();
        public float ActorSpriteBounceValue;

        public List<DecisionItem> CurrentDecisionItems = new List<DecisionItem>();
        public List<DecisionItem> CurrentFlagChecks = new List<DecisionItem>();
        public bool AreConditionsMet = false;
        public GameObject DecisionHolder;
        public GameObject DecisionItemPrefab;
        public float CharacterDuration = 0.01f;

        public float SpeedFactor = 1;

        public StoryTypeEffect CurrentRevealEffect;

        public int[] CurrentVertexIndexes;

        private                bool  _IsPlaying = false;
        [NonSerialized] public bool  IsMeshDirty;
        [NonSerialized] public float TimeBuffer       = 0;
        [NonSerialized] public int   ActiveCoroutines = 0;

        [NonSerialized] public PlayerSettings Settings = new();

        public void Awake()
        {
            sMain = this;
        }

        public void Start()
        {
            DialogueLabel.text = "";
            FullScreenNarrationText.text = "";
            MaxVolume = Settings.BackgroundMusicVolume;
        }

        public void Update()
        {
            if (_IsPlaying) TimeBuffer += Time.deltaTime * SpeedFactor;

            if (IsMeshDirty)
            {
                DialogueLabel.ForceMeshUpdate();
                ResetDialogueMesh();
                FullScreenNarrationText.ForceMeshUpdate();
                ResetNarrationTextMesh();
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
                if (CurrentChunkIndex + 1 < CurrentScript.Chunks.Count)
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

            // Dialogue fade out
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

            // Narration fade out
            Vector2 narrationPos = FullScreenNarrationText.rectTransform.anchoredPosition;
            yield return Ease.Animate(0.2f, (x) =>
            {
                float ease = Ease.Get(x, EaseFunction.Quadratic, EaseMode.Out);
                FullScreenNarrationText.color = new Color(1, 1, 1, 1 - ease);
                float ease2 = Ease.Get(x, EaseFunction.Cubic, EaseMode.In);
                FullScreenNarrationText.rectTransform.anchoredPosition = narrationPos + ease2 * 5 * Vector2.down;
            });
            FullScreenNarrationText.text = "";
            FullScreenNarrationText.color = Color.white;
            FullScreenNarrationText.rectTransform.anchoredPosition = narrationPos;

            // Background Change
        foreach (var ins in chunk.Instructions)
        {
            var crt = ins.OnBackgroundChange(this);
            if (crt != null) yield return crt;
        }
        yield return new WaitWhile(() => ActiveCoroutines > 0);

        // Change BG Music
        foreach (var instruction in chunk.Instructions)
        {
            var coroutine = instruction.OnMusicPlay(this);
            if (coroutine != null)
                yield return coroutine;
        }

        foreach (var ins in chunk.Instructions)
            ins.OnMusicChange(this);
        
        // Text Area Switch
        foreach (var instruction in chunk.Instructions)
        {
            var coroutine = instruction.OnTextAreaSwitch(this);
            if (coroutine != null)
                yield return coroutine;
        }

        // Setup Actor Name/Properties
        foreach (var ins in chunk.Instructions)
            ins.OnTextBuild(this);

        


        CurrentCharacterIndex = 0;
        TimeBuffer = 0;
        SpeedFactor = 1;
        CurrentVertexIndexes = new int[DialogueLabel.textInfo.meshInfo.Length];
        CurrentRevealEffect = new FloatInTypeEffect();
        IsMeshDirty = true;
        ResetDialogueMesh();
        ResetNarrationTextMesh();

        foreach (var instruction in chunk.Instructions)
        {
            var uiCoroutine = instruction.OnInterfaceChange(this);
            if (uiCoroutine != null) yield return uiCoroutine;
        }
        yield return new WaitWhile(() => ActiveCoroutines > 0);

        foreach (var instruction in chunk.Instructions)
        {
            var actorCoroutine = instruction.OnActorAction(this);
            var textCoroutine = instruction.OnTextReveal(this);
            var sfxCoroutine = instruction.OnSFXPlay(this);

            if (actorCoroutine != null) yield return actorCoroutine;
            if (textCoroutine != null) yield return textCoroutine;
            if (sfxCoroutine != null) yield return sfxCoroutine;
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

        public void ResetNarrationTextMesh()
        {
            // Make sure TMP has current geometry
            FullScreenNarrationText.ForceMeshUpdate();

            var textInfo  = FullScreenNarrationText.textInfo;
            int meshCount = textInfo.meshInfo.Length;

            // Ensure the per-mesh reveal indexes array is sized for this text object
            if (CurrentVertexIndexes == null || CurrentVertexIndexes.Length != meshCount)
                CurrentVertexIndexes = new int[meshCount];

            for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                var colors = textInfo.meshInfo[meshIndex].colors32;

                // Start from the tracked reveal index, but keep it in-bounds
                int start = CurrentVertexIndexes[meshIndex];
                if (start < 0) start = 0;
                if (start > colors.Length) start = colors.Length;

                for (int v = start; v < colors.Length; v++)
                    colors[v].a = 0;

                textInfo.meshInfo[meshIndex].mesh.colors32 = colors;
                FullScreenNarrationText.UpdateGeometry(textInfo.meshInfo[meshIndex].mesh, meshIndex);
            }
        }


            public void UpdateNarrationTextMesh()
            {
                var textInfo = FullScreenNarrationText.textInfo;
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                    FullScreenNarrationText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
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