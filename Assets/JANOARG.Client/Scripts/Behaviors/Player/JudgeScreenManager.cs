using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using JANOARG.Client.UI;
using JANOARG.Shared.Data.ChartInfo;

namespace JANOARG.Client.Behaviors.Player
{
    /// <summary>
    /// Use concept of object pooling
    /// There will be no more allocation for every judge screen effect
    /// </summary>
    public class JudgeScreenManager : MonoBehaviour
    {
        public Stack<JudgeScreenEffect> judgeScreenEffects;
        private int totalMaxInstances = 32;
        private int totalInstances = 0;

        private void Start()
        {
            // Initialize judge screen effects
            if (judgeScreenEffects == null || judgeScreenEffects.Count == 0)
            {
                judgeScreenEffects = new Stack<JudgeScreenEffect>(totalMaxInstances);
                for (int i = 0; i < totalMaxInstances; i++)
                {
                    var effect = Instantiate(PlayerScreen.main.JudgeScreenSample, PlayerScreen.main.JudgeScreenHolder);
                    judgeScreenEffects.Push(effect);
                    effect.gameObject.SetActive(false);
                }
            }
        }

        // Use effects in the pool
        public JudgeScreenEffect BorrowEffect(float? accuracy, Color color)
        {
            // Debug.Log($"Borrowing JudgeScreenEffect: Accuracy={accuracy}, Color={color}, TotalInstances={totalInstances}");
            if (totalInstances > totalMaxInstances)
            {
                var newEffect = Instantiate(PlayerScreen.main.JudgeScreenSample, PlayerScreen.main.JudgeScreenHolder);
                newEffect.SetAccuracy(accuracy);
                newEffect.SetColor(color);
                newEffect.PlayOneShot();
                Debug.LogWarning("JudgeScreenManager pool exhausted, creating new instance.");
                return newEffect;
            }

            JudgeScreenEffect effect = judgeScreenEffects.Pop();
            effect.gameObject.SetActive(true);
            effect.SetAccuracy(accuracy);
            effect.SetColor(color);
            effect.Play();
            totalInstances++;
            return effect;
        }

        // Return when finish
        public void ReturnEffect(JudgeScreenEffect effect)
        {
            effect.gameObject.SetActive(false);
            judgeScreenEffects.Push(effect);
            totalInstances--;
        }
    }
}