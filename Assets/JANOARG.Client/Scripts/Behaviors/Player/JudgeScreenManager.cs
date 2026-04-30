using System;
using System.Collections.Generic;
using JANOARG.Client.UI;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Player
{
    /// <summary>
    /// Manages the pool of <see cref="JudgeScreenEffect"/> instances and drives their animation
    /// from a single Update loop — no per-hit coroutine or closure allocation.
    /// </summary>
    public class JudgeScreenManager : MonoBehaviour
    {
        // -----------------------------------------------------------------
        // Animation state — plain struct, lives on the stack / in the list.
        // No heap allocation per hit.
        // -----------------------------------------------------------------
        private struct ActiveEffect
        {
            public JudgeScreenEffect Effect;
            public float             Elapsed;
            public bool              IsOneShot; // if true, Destroy instead of Return on finish
            public const float       Duration = 0.4f;
        }

        // -----------------------------------------------------------------
        // Pool
        // -----------------------------------------------------------------
        public  Stack<JudgeScreenEffect> judgeScreenEffects;
        private int                      _totalMaxInstances = 32;
        private int                      _totalInstances    = 0;

        // Active animations driven this Update
        private readonly List<ActiveEffect> _active = new();

        // -----------------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------------
        private void Start()
        {
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

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ActiveEffect entry = _active[i];
                entry.Elapsed += dt;

                float x = Mathf.Clamp01(entry.Elapsed / ActiveEffect.Duration);
                entry.Effect.Tick(x);
                _active[i] = entry;

                if (x >= 1f)
                {
                    _active.RemoveAt(i);
                    if (entry.IsOneShot)
                        Destroy(entry.Effect.gameObject);
                    else
                        ReturnEffect(entry.Effect);
                }
            }
        }

        // -----------------------------------------------------------------
        // Pool API
        // -----------------------------------------------------------------

        /// <summary>
        /// Borrow an effect from the pool, configure it, and start its animation.
        /// Returns null only if the pool is exhausted and overflow instantiation also fails.
        /// </summary>
        public JudgeScreenEffect BorrowEffect(float? accuracy, Color color)
        {
            if (_totalInstances > _totalMaxInstances)
                return CreateEffect(accuracy, color);

            JudgeScreenEffect effect;
            try
            {
                effect = judgeScreenEffects.Pop();
            }
            catch (InvalidOperationException e)
            {
                Debug.LogWarning(e.Message + " Skipping effect.");
                return null;
            }

            effect.transform.SetParent(PlayerScreen.sMain.JudgeScreen);
            effect.gameObject.SetActive(true);
            effect.SetAccuracy(accuracy);
            effect.SetColor(color);
            effect.Tick(0); // initialise visual state before first Update

            _active.Add(new ActiveEffect { Effect = effect, Elapsed = 0f, IsOneShot = false });
            _totalInstances++;
            return effect;
        }

        /// <summary>
        /// Return a pooled effect to the stack. Called by the Update loop on completion.
        /// </summary>
        public void ReturnEffect(JudgeScreenEffect effect)
        {
            effect.gameObject.SetActive(false);
            effect.transform.SetParent(PlayerScreen.sMain.JudgeScreenHolder);
            judgeScreenEffects.Push(effect);
            _totalInstances--;
        }

        // -----------------------------------------------------------------
        // Overflow — pool exhausted
        // -----------------------------------------------------------------
        private JudgeScreenEffect CreateEffect(float? accuracy, Color color)
        {
            Debug.LogWarning("JudgeScreenManager pool exhausted, creating new instance.");
            var newEffect = Instantiate(PlayerScreen.sMain.JudgeScreenSample, PlayerScreen.sMain.JudgeScreen);
            newEffect.SetAccuracy(accuracy);
            newEffect.SetColor(color);
            newEffect.Tick(0);
            _active.Add(new ActiveEffect { Effect = newEffect, Elapsed = 0f, IsOneShot = true });
            return newEffect;
        }
    }
}
