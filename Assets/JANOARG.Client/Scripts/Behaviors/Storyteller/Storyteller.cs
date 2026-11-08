using System;
using System.Collections;
using JANOARG.Client.Data.Constant;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Client.Data.Story;
using JANOARG.Client.Data.Story.Instructions;
using JANOARG.Client.Data.Story.TypeEffects;
using JANOARG.Shared.Utils.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        public RectTransform Interface;
        public TMP_Text DialogueLabel;

        public TMP_Text      NameLabel;
        public RectTransform NameLabelHolder;
        public CanvasGroup   NameLabelGroup;
        public Graphic       NextChunkIndicator;

        [Space]
        public TMP_Text PlaceText;
        public TMP_Text TimeText;
        
        public StoryActorStage ActorStage;
        public Image BackgroundImage;

        public CanvasGroup FullScreenNarrationGroup;
        public TMP_Text FullScreenNarrationText;

        public AudioSource BackgroundMusicPlayer;
        public AudioSource SoundEffectsPlayer;
        [NonSerialized] public float MaxVolume;

        public RectTransform ActorHolder;

        public string PendingSectionJump;
        public List<int> CallStack = new();
        [NonSerialized] public int? PendingChunkIndex;

        // public List<DecisionItem> CurrentDecisionItems = new List<DecisionItem>();
        // public List<DecisionItem> CurrentFlagChecks = new List<DecisionItem>();
        // public bool AreConditionsMet = false;
        // public GameObject DecisionHolder;
        // public GameObject DecisionItemPrefab;

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
            if (Constants == null)
            {
                Debug.LogError($"Storyteller: No Story Constants assigned!");
                return;
            }
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

            StartCoroutine(PlayChunk(CurrentScript.Chunks[CurrentChunkIndex]));
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
            if (_IsPlaying) return;
            StartCoroutine(PlayUntilInput());
        }

        private IEnumerator PlayUntilInput()
        {
            _IsPlaying = true;
            SetNextChunkIndicatorState(0);

            while (CurrentChunkIndex < CurrentScript.Chunks.Count)
            {
                // Increment BEFORE playing, so CurrentChunkIndex == "the chunk after this one".
                // Call stores that value as its return point.
                StoryChunk chunk = CurrentScript.Chunks[CurrentChunkIndex++];

                yield return PlayChunk(chunk);

                if (!chunk.isSilent) yield break; // wait for the player's click
            }

            Debug.Log("END OF STORY");
            _IsPlaying = false;
        }

        public IEnumerator PlayChunk(StoryChunk chunk)
        {
            bool silent = chunk.isSilent;
            if (!silent)
            {
                _IsPlaying = true;
                
                SetNextChunkIndicatorState(0);
                Vector2 dialoguePos = DialogueLabel.rectTransform.anchoredPosition;

                yield return Ease.Animate(0.2f, (x) =>
                {
                    float ease = Ease.Get(x, EaseFunction.Quadratic, EaseMode.Out);
                    DialogueLabel.color = new Color(1, 1, 1, 1 - ease);

                    DialogueLabel.rectTransform.anchoredPosition = dialoguePos + EaseUtils.FromZero(5, x, EaseFunction.Cubic, EaseMode.In) * Vector2.down;
                });

                // Reset Dialog Label and Name Label
                SetNameLabelText("");
                DialogueLabel.text = "";
                DialogueLabel.color = Color.white;
                DialogueLabel.rectTransform.anchoredPosition = dialoguePos;
                
                // Prefilling the labels
                foreach (StoryInstruction instruction in chunk.Instructions)
                    instruction.OnTextBuild(this);

                CurrentCharacterIndex = 0;
                TimeBuffer = 0;
                SpeedFactor = 1;
                CurrentVertexIndexes = new int[DialogueLabel.textInfo.meshInfo.Length];
                CurrentRevealEffect = new FloatInTypeEffect();
                IsMeshDirty = true;
                ResetDialogueMesh();

                Debug.Log($"Dialog: {NameLabel.text} : {DialogueLabel.text}");
                // If DialogLabel and NameLabel is empty, dock the textbox
                if (string.IsNullOrEmpty(NameLabel.text) && string.IsNullOrEmpty(DialogueLabel.text))
                {
                    yield return DockInterface();
                }
                else
                {
                    yield return UndockInterface();
                }
            }
            // Order
            yield return RunPhase(chunk, i => i.OnSceneSet(this));
            yield return RunPhase(chunk, i => i.OnBeforeTextReveal(this));
            yield return RunPhase(chunk, i => i.OnTextReveal(this));
            yield return RunPhase(chunk, i => i.OnAfterTextReveal(this));

            yield return new WaitWhile(() => ActiveCoroutines > 0);

            if (PendingChunkIndex.HasValue)
            {
                CurrentChunkIndex = PendingChunkIndex.Value;
                PendingChunkIndex = null;
            }
            else if (!string.IsNullOrEmpty(PendingSectionJump))
            {
                ResolvePendingJump();
            }

            if (!silent)
            {
                _IsPlaying = false;
                SetNextChunkIndicatorState(1);
            }

        }

        private IEnumerator RunPhase(StoryChunk chunk, Func<StoryInstruction, IEnumerator> hook)
        {
            foreach (StoryInstruction ins in chunk.Instructions)
            {
                IEnumerator routine = hook(ins);
                if (routine != null) yield return routine;
            }
        }

        public void ResolvePendingJump()
        {
            if (string.IsNullOrWhiteSpace(PendingSectionJump)) return;

            if (CurrentScript.TryGetLabel(PendingSectionJump, out int index))
                CurrentChunkIndex = index;
            else
                Debug.LogError($"Jump Section: {PendingSectionJump} not found in script.");

            PendingSectionJump = null;
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

                try
                {
                    yield return routine;
                }
                finally
                {
                    ActiveCoroutines--;
                }
            }

            StartCoroutine(f_registration());
        }

        private float          _CurrentNextChunkIndicatorPosition = 0;
        private EaseEnumerator _CurrentNextChunkIndicatorRoutine  = null;

        public void SetNextChunkIndicatorState(float target)
        {
            _CurrentNextChunkIndicatorRoutine?.Skip();
            _CurrentNextChunkIndicatorRoutine = Ease.EnumAnimate(0.2f, x =>
            {
                x = Mathf.Lerp(_CurrentNextChunkIndicatorPosition, target, x);
                _CurrentNextChunkIndicatorPosition = x;
                NextChunkIndicator.color = new Color(1, 1, 1, x);
                NextChunkIndicator.rectTransform.anchoredPosition = 5 * EaseUtils.ToZero(1, x, EaseFunction.Cubic, EaseMode.Out) * Vector2.down;
            });
            StartCoroutine(_CurrentNextChunkIndicatorRoutine);
        }

        private EaseEnumerator _CurrentNameFieldRoutine = null;

        public void SetNameLabelText(string label)
        {
            _CurrentNameFieldRoutine?.Skip();
            NameLabel.text = "";
            float fromAlpha = NameLabelGroup.alpha;
            float toAlpha = string.IsNullOrWhiteSpace(label) ? 0 : 1;
            float fromXPos = NameLabelHolder.anchorMin.x;
            float toXPos = 0;

            // Reveal / Persist
            if (toAlpha > 0)
            {
                NameLabel.text = label;
                NameLabelHolder.sizeDelta = new Vector2(NameLabel.preferredWidth, NameLabelHolder.sizeDelta.y);
            }

            if (fromAlpha == 0) fromXPos = toXPos;

            _CurrentNameFieldRoutine = Ease.EnumAnimate(0.2f, x =>
            {
                float ease = Ease.Get(x, EaseFunction.Quadratic, EaseMode.Out);
                NameLabelGroup.alpha = EaseUtils.LerpTo(fromAlpha, toAlpha, ease);
                float xPos = EaseUtils.LerpTo(fromXPos, toXPos, ease);
                NameLabelHolder.anchorMin = NameLabelHolder.anchorMax = NameLabelHolder.pivot = new Vector2(xPos, 0.5f);
                NameLabelHolder.anchoredPosition = new Vector2(0, (1 - NameLabelGroup.alpha) * -5);
            });
            StartCoroutine(_CurrentNameFieldRoutine);
        }
    
        public IEnumerator DockInterface()
        {
            Vector2 start = Interface.offsetMax;
            Vector2 end = start;
            end.y = -490f;

            if (Interface.offsetMax.y == end.y) yield break;
            yield return Ease.Animate(0.5f, a =>
            {
                float ease = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                Interface.offsetMax = Vector2.Lerp(start, end, ease);
            });
        }

        public IEnumerator UndockInterface()
        {
            Vector2 start = Interface.offsetMax;
            Vector2 end = start;
            end.y = -400f;
            
            if (Interface.offsetMax.y == end.y) yield break;
            yield return Ease.Animate(0.5f, a =>
            {
                float ease = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                Interface.offsetMax = Vector2.Lerp(start, end, ease);

                // To make text is still invis when animating and make the text anim not break 
                // idk why this works :sob:
                DialogueLabel.ForceMeshUpdate();
                ResetDialogueMesh();
            });
        }
    
    }
}
