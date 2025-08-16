using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class JudgeScreenManager : MonoBehaviour
{
    public static JudgeScreenManager main;
    public Stack<JudgeScreenEffect> JudgeScreenEffects;
    private int totalMaxInstances = 32;
    private int totalInstances = 0;

    private void Start()
    {
        main = this;
        if (JudgeScreenEffects == null || JudgeScreenEffects.Count == 0)
        {
            JudgeScreenEffects = new Stack<JudgeScreenEffect>(totalMaxInstances);
            for (int i = 0; i < totalMaxInstances; i++)
            {
                var effect = Instantiate(PlayerScreen.main.JudgeScreenSample, PlayerScreen.main.JudgeScreenHolder);
                JudgeScreenEffects.Push(effect);
                effect.gameObject.SetActive(false);
            }
        }
    }

    public JudgeScreenEffect BorrowEffect(float? accuracy, Color color, Transform JudgeScreenHolder)
    {
        // Debug.Log($"Borrowing JudgeScreenEffect: Accuracy={accuracy}, Color={color}, TotalInstances={totalInstances}");
        if (totalInstances > totalMaxInstances)
        {
            var newEffect = Instantiate(PlayerScreen.main.JudgeScreenSample, JudgeScreenHolder);
            newEffect.SetAccuracy(accuracy);
            newEffect.SetColor(color);
            newEffect.PlayOneShot();
            Debug.LogWarning("JudgeScreenEffect pool exhausted, creating new instance.");
            return newEffect;
        }

        JudgeScreenEffect effect = JudgeScreenEffects.Pop();
        effect.SetAccuracy(accuracy);
        effect.SetColor(color);
        effect.gameObject.SetActive(true);
        effect.Play();
        totalInstances++;
        return effect;
    }

    public void ReturnEffect(JudgeScreenEffect effect)
    {
        effect.gameObject.SetActive(false);
        JudgeScreenEffects.Push(effect);
        totalInstances--;
    }

    private void OnDestroy()
    {
        main = null;
    }
}