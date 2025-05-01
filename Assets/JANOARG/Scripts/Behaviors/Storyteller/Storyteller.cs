using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Storyteller : MonoBehaviour
{
    public static Storyteller main;

    public StoryConstants Constants;
    [Space]
    public StoryScript CurrentScript;
    public int CurrentChunkIndex;
    public int CurrentCharacterIndex;
    [Space]
    public TMP_Text DialogueLabel;
    public TMP_Text NameLabel;
    public RectTransform NameLabelHolder;
    public CanvasGroup NameLabelGroup;
    public Graphic NextChunkIndicator;
    [Space]
    public float CharacterDuration = 0.01f;
    public float SpeedFactor = 1;

    public StoryTypeEffect currentRevealEffect;

    public int[] CurrentVertexIndexes;

    bool IsPlaying = false;
    [NonSerialized] public bool IsMeshDirty;
    [NonSerialized] public float TimeBuffer = 0;
    [NonSerialized] public int ActiveCoroutines = 0;

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
        DialogueLabel.text = "";
    }

    public void Update()
    {
        if (IsPlaying) 
        {
            TimeBuffer += Time.deltaTime * SpeedFactor;
        }
        if (IsMeshDirty) 
        {
            DialogueLabel.ForceMeshUpdate();
            ResetDialogueMesh();
            IsMeshDirty = false;
        }
    }

    public void LateUpdate()
    {
        if (IsMeshDirty) 
        {
            UpdateDialogueMesh();
        }
    }

    public void PlayScript(StoryScript script)
    {
        CurrentScript = script;
        CurrentChunkIndex = 0;

        StartCoroutine(PlayChunk());
    }

    public void OnScreenClick() 
    {
        if (IsPlaying) 
        {
            SpeedFactor *= 5;
        }
        else 
        {
            PlayNextChunk();
        }
    }

    public void PlayNextChunk() 
    {
        CurrentChunkIndex++;
        StartCoroutine(PlayChunk());
    }

    public IEnumerator PlayChunk() 
    {
        IsPlaying = true;
        StoryChunk chunk = CurrentScript.Chunks[CurrentChunkIndex];

        SetNextChunkIndicatorState(0);
        Vector2 dialoguePos = DialogueLabel.rectTransform.anchoredPosition;
        yield return Ease.Animate(0.2f, (x) => {
            float ease = Ease.Get(x, EaseFunction.Quadratic, EaseMode.Out);
            DialogueLabel.color = new Color(1, 1, 1, 1 - ease);
            float ease2 = Ease.Get(x, EaseFunction.Cubic, EaseMode.In);
            DialogueLabel.rectTransform.anchoredPosition = dialoguePos + ease2 * 5 * Vector2.down;
        });
        DialogueLabel.text = "";
        DialogueLabel.color = Color.white;
        DialogueLabel.rectTransform.anchoredPosition = dialoguePos;

        foreach (var ins in chunk.Instructions) ins.OnTextBuild(this);

        CurrentCharacterIndex = 0;
        TimeBuffer = 0;
        SpeedFactor = 1;
        CurrentVertexIndexes = new int[DialogueLabel.textInfo.meshInfo.Length];
        currentRevealEffect = new FloatInTypeEffect();
        IsMeshDirty = true;
        ResetDialogueMesh();

        foreach (var ins in chunk.Instructions)
        {
            var crt = ins.OnTextReveal(this);
            if (crt != null) yield return crt;
        }
        yield return new WaitWhile(() => ActiveCoroutines > 0);

        IsPlaying = false;
        SetNextChunkIndicatorState(1);
    }
    
    public void ResetDialogueMesh() 
    {
        var textInfo = DialogueLabel.textInfo;
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            for (int j = CurrentVertexIndexes[i]; j < textInfo.meshInfo[i].colors32.Length; j++)
                textInfo.meshInfo[i].colors32[j].a = 0;
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            DialogueLabel.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
    public void UpdateDialogueMesh() 
    {
        var textInfo = DialogueLabel.textInfo;
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            DialogueLabel.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    public void RegisterCoroutine(IEnumerator routine)
    {
        IEnumerator r() 
        {
            ActiveCoroutines++;
            yield return routine;
            ActiveCoroutines--;
        }
        StartCoroutine(r());
    }

    float currentNCIPos = 0;
    Coroutine currentNCIRoutine = null;
    public void SetNextChunkIndicatorState(float target)
    {
        if (currentNCIRoutine != null) StopCoroutine(currentNCIRoutine);
        currentNCIRoutine = StartCoroutine(NextChunkIndicatorAnim(target));
    }
    IEnumerator NextChunkIndicatorAnim(float target)
    {
        float from = currentNCIPos;
        yield return Ease.Animate(0.2f, (x) => {
            x = Mathf.Lerp(from, target, x);
            currentNCIPos = x;
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            NextChunkIndicator.color = new Color(1, 1, 1, x);
            NextChunkIndicator.rectTransform.anchoredPosition = 5 * (1 - ease) * Vector2.down;
        });
    }

    Coroutine currentNFRoutine = null;
    public void SetNameLabelText(string name)
    {
        if (currentNFRoutine != null) StopCoroutine(currentNFRoutine);
        currentNFRoutine = StartCoroutine(NameLabelAnim(name));
    }
    IEnumerator NameLabelAnim(string name)
    {
        float fromAlpha = NameLabelGroup.alpha;
        float toAlpha = string.IsNullOrWhiteSpace(name) ? 0 : 1;
        float fromXPos = NameLabelHolder.anchorMin.x;
        float toXPos = 0;

        if (toAlpha > 0) 
        {
            NameLabel.text = name;
            NameLabelHolder.sizeDelta = new (NameLabel.preferredWidth, NameLabelHolder.sizeDelta.y);
        }
        if (fromAlpha == 0) 
        {
            fromXPos = toXPos;
        }

        yield return Ease.Animate(0.2f, (x) => {
            float ease = Ease.Get(x, EaseFunction.Quadratic, EaseMode.Out);
            NameLabelGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, ease);
            float xPos = Mathf.Lerp(fromXPos, toXPos, ease);
            NameLabelHolder.anchorMin = NameLabelHolder.anchorMax = NameLabelHolder.pivot = new(xPos, 0.5f);
            NameLabelHolder.anchoredPosition = new (0, (1 - NameLabelGroup.alpha) * -5);
        });
    }
}
