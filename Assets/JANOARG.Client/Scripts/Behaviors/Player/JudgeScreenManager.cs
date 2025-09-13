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
        private int _totalMaxInstances = 32;
        private int _totalInstances = 0;

        private void Start()
        {
            // Initialize judge screen effects
            if (judgeScreenEffects == null || judgeScreenEffects.Count == 0)
            {
                judgeScreenEffects = new Stack<JudgeScreenEffect>(_totalMaxInstances);
                for (int i = 0; i < _totalMaxInstances; i++)
                {
                    var effect = Instantiate(PlayerScreen.sMain.JudgeScreenSample, PlayerScreen.sMain.JudgeScreenHolder);
                    judgeScreenEffects.Push(effect);
                    effect.gameObject.SetActive(false);
                }
            }
        }

        private JudgeScreenEffect CreateEffect(float? accuracy, Color color)
        {
            var newEffect = Instantiate(PlayerScreen.sMain.JudgeScreenSample, PlayerScreen.sMain.JudgeScreen);
            newEffect.SetAccuracy(accuracy);
            newEffect.SetColor(color);
            newEffect.PlayOneShot();
            Debug.LogWarning("JudgeScreenManager pool exhausted, creating new instance.");
            
            return newEffect;
        }
        
        // Use effects in the pool
        public JudgeScreenEffect BorrowEffect(float? accuracy, Color color)
        {
            // Debug.Log($"Borrowing JudgeScreenEffect: Accuracy={accuracy}, Color={color}, TotalInstances={totalInstances}");
            if (_totalInstances > _totalMaxInstances)
            {
                return CreateEffect(accuracy, color);
            }

            // In case player calls Borrow too fast (when fast-forwarding in editor) it didn't have time to keep track
            try
            {
                JudgeScreenEffect effect = judgeScreenEffects.Pop();
                effect.transform.SetParent(PlayerScreen.sMain.JudgeScreen);
                effect.gameObject.SetActive(true);
                effect.SetAccuracy(accuracy);
                effect.SetColor(color);
                effect.Play();
                _totalInstances++;
                return effect;
            }
            catch (InvalidOperationException e)
            {
                Debug.LogWarning(e.Message + "Skipping effect.");

                return null;
            }
        }

        // Return when finish
        public void ReturnEffect(JudgeScreenEffect effect)
        {
            effect.gameObject.SetActive(false);
            effect.transform.SetParent(PlayerScreen.sMain.JudgeScreenHolder);
            judgeScreenEffects.Push(effect);
            _totalInstances--;
        }
    }
}