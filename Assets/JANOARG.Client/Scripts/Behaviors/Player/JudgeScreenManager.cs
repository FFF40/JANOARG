using System;
using System.Collections.Generic;
using JANOARG.Client.UI;
using JANOARG.Shared.Data.ChartInfo;
using Unity.Profiling;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Player
{
    /// <summary>
    /// Manages the pool of <see cref="JudgeScreenEffect"/> instances and drives their animation
    /// from a single Update loop — no per-hit coroutine or closure allocation.
    /// </summary>
    public class JudgeScreenManager : MonoBehaviour
    {
        static readonly ProfilerMarker sr_BorrowEffect = new("JudgeScreenManager.BorrowEffect");
        static readonly ProfilerMarker sr_SetColor = new("JudgeScreenManager.BorrowEffect: SetColor");
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
        public JudgeScreenEffect BorrowEffect(HitPlayer hitobject, float? accuracy, Color color)
        {
            sr_BorrowEffect.Begin();
            try
            {
                if (_totalInstances >= _totalMaxInstances)
                    return CreateEffect(hitobject, accuracy, color);

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
                DefineJudgeScreenEffect(hitobject, ref effect, accuracy);
                effect.gameObject.SetActive(true);

                sr_SetColor.Begin();
                effect.SetColor(color);
                sr_SetColor.End();

                effect.Tick(0); // initialise visual state before first Update

                _active.Add(new ActiveEffect { Effect = effect, Elapsed = 0f, IsOneShot = false });
                _totalInstances++;
                return effect;
            }
            finally
            {
                sr_BorrowEffect.End();
            }
        }
        
        void DefineJudgeScreenEffect(HitPlayer hitobject, ref JudgeScreenEffect effect, float? accuracy)
        {
            var rt = (RectTransform)effect.transform;
            var hold = hitobject.Current.Length > 0;
            
            
            if (hitobject.Current.Flickable)
            {
                effect.SetShapeAccuracy(false);
                effect.Size = 150;

                return;
            }
            
            if (accuracy == null && hold) 
            {
                effect.SetShapeAccuracy(false);
                effect.Size = 40;
                
                return;
            }

            if (hitobject.Current.Type == HitObject.HitType.Catch)
            {
                effect.SetShapeAccuracy(false);
                effect.Size = 70;
                
                return;
            }

            if (hitobject.Current.Type == HitObject.HitType.Normal)
            {
                effect.SetShapeAccuracy(true, accuracy);
                effect.Size = 120;
                return;
            }

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
        // Dedicated (caller-driven) instances — for callers that need to hold onto
        // and restart an effect's animation themselves (e.g. hold-tick pulsing)
        // instead of a single fire-and-forget animation. Not added to _active, so
        // JudgeScreenManager.Update() never touches these; the caller is responsible
        // for calling Tick() itself and for calling ReturnDedicated() when done.
        // -----------------------------------------------------------------

        /// <summary>
        /// Borrow an effect for caller-managed (repeated/restartable) animation.
        /// Returns null only if the pool is exhausted and overflow instantiation also fails.
        /// </summary>
        public JudgeScreenEffect BorrowDedicated(HitPlayer hitobject, float? accuracy, Color color, out bool isOverflow)
        {
            isOverflow = _totalInstances >= _totalMaxInstances;

            JudgeScreenEffect effect;
            if (isOverflow)
            {
                Debug.LogWarning("JudgeScreenManager pool exhausted, creating new dedicated instance.");
                effect = Instantiate(PlayerScreen.sMain.JudgeScreenSample, PlayerScreen.sMain.JudgeScreen);
            }
            else
            {
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
                _totalInstances++;
            }

            DefineJudgeScreenEffect(hitobject, ref effect, accuracy);
            effect.gameObject.SetActive(true);
            effect.SetColor(color);
            effect.Tick(0);
            return effect;
        }

        /// <summary>
        /// Return an effect borrowed via BorrowDedicated. Overflow instances (created
        /// when the pool was exhausted) are destroyed instead of pushed back.
        /// </summary>
        public void ReturnDedicated(JudgeScreenEffect effect, bool isOverflow)
        {
            if (isOverflow)
            {
                Destroy(effect.gameObject);
                return;
            }

            ReturnEffect(effect);
        }

        // -----------------------------------------------------------------
        // Overflow — pool exhausted
        // -----------------------------------------------------------------
        private JudgeScreenEffect CreateEffect(HitPlayer hitobject, float? accuracy, Color color)
        {
            Debug.LogWarning("JudgeScreenManager pool exhausted, creating new instance.");
            var newEffect = Instantiate(PlayerScreen.sMain.JudgeScreenSample, PlayerScreen.sMain.JudgeScreen);
            DefineJudgeScreenEffect(hitobject, ref newEffect, accuracy);
            newEffect.SetColor(color);
            newEffect.Tick(0);
            _active.Add(new ActiveEffect { Effect = newEffect, Elapsed = 0f, IsOneShot = true });
            return newEffect;
        }
    }
}
