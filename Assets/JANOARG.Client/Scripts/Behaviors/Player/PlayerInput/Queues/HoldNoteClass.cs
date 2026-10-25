using System;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.Player;
using JANOARG.Shared.Data.ChartInfo;
using Unity.Collections;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class HoldNoteClass
{
    /// <summary>
    ///     Backing field for the <see cref = "holdPassDrainValue"/> property.
    /// </summary>
    private float _HoldPassDrainValue;

    /// <summary>
    ///     The touch that is currently assigned to this hold note.
    /// </summary>
    /// <remarks>
    ///     This property is used to track which touch is currently interacting with the hold note.
    /// </remarks>
    public TouchClass AssignedTouch;

    /// <summary>
    ///     The values associated with the hit player object.
    /// </summary>
    public HitPlayer HitObject;

    /// <summary>
    ///     Indicates if the player is currently holding the note.
    /// </summary>
    public bool IsPlayerHolding;

    /// <summary>
    ///     Indicates whether the note would give a score on hold ticks (would hit or would miss).
    /// </summary>
    public bool IsScoring;

    /// <summary>
    ///     The HP Drain-like system for judgement of the player's
    ///     holding performance, ranging from 0 to 1.
    /// </summary>
    public float holdPassDrainValue
    {
        get =>
            _HoldPassDrainValue;

        set
        {
            if (value > 1)
            {
                Debug.LogWarning($"HoldPassDrainValue was above 1 ({value}), clamping to 1");
                value = 1;
            }
            else if (value < 0)
            {
                Debug.LogWarning($"HoldPassDrainValue was below 0 ({value}), clamping to 0");
                value = 0;
            }

            _HoldPassDrainValue = value;
        }
    }

    // -----------------------------------------------------------------
    // Dedicated hold-tick judge-effect pool.
    //
    // Hold ticks fire every 0.5 *beats*, not 0.5 seconds — at high BPM (e.g. 414)
    // that's well under the 0.4s effect animation duration, so borrowing/returning
    // a fresh JudgeScreenManager effect on every tick both churns the shared pool
    // and (via each borrow's SetVerticesDirty) forces a full Canvas rebatch every
    // single tick. Instead, borrow a small dedicated set once per hold, sized to
    // roughly how many ticks' worth of animation can be in flight at once, and
    // just restart whichever instance is free (or closest to finishing) per tick.
    // -----------------------------------------------------------------
    private const float DedicatedEffectDuration = 0.4f; // matches JudgeScreenManager.ActiveEffect.Duration
    private const int   DedicatedEffectMaxPoolSize = 8;

    private JudgeScreenEffect[] _DedicatedEffects;
    private bool[]              _DedicatedIsOverflow;
    private float[]             _DedicatedElapsed;
    private bool[]              _DedicatedAnimating;

    /// <summary>
    /// Lazily borrows a small dedicated pool of effects for this hold, sized from the
    /// BPM at its start time. No-op if already created.
    /// </summary>
    public void EnsureDedicatedEffects(Color color)
    {
        if (_DedicatedEffects != null) return;

        float bpm = GetBpmAt(HitObject.Time);
        float secondsPerTick = 30f / Mathf.Max(bpm, 1f); // 0.5 beats at this BPM
        int poolSize = Mathf.Clamp(
            Mathf.CeilToInt(DedicatedEffectDuration / secondsPerTick),
            1, DedicatedEffectMaxPoolSize);

        _DedicatedEffects = new JudgeScreenEffect[poolSize];
        _DedicatedIsOverflow = new bool[poolSize];
        _DedicatedElapsed = new float[poolSize];
        _DedicatedAnimating = new bool[poolSize];

        for (int i = 0; i < poolSize; i++)
            _DedicatedEffects[i] = PlayerScreen.sMain.JudgeScreenManager.BorrowDedicated(
                HitObject, null, color, out _DedicatedIsOverflow[i]);
    }

    private static float GetBpmAt(float timeSeconds)
    {
        List<BPMStop> stops = PlayerScreen.sTargetSong.Timing.Stops;
        float bpm = stops.Count > 0 ? stops[0].BPM : 60f;

        for (var i = 0; i < stops.Count; i++)
        {
            if (stops[i].Offset > timeSeconds)
                break;
            bpm = stops[i].BPM;
        }

        return bpm;
    }

    /// <summary>
    /// Restarts whichever dedicated slot is free (or, failing that, closest to
    /// finishing) to visually represent one hold tick. Returns the restarted
    /// effect so the caller can (re)position it, or null if none are available.
    /// </summary>
    public JudgeScreenEffect PulseDedicatedEffect()
    {
        if (_DedicatedEffects == null) return null;

        int target = -1;
        float highestElapsed = -1f;

        for (var i = 0; i < _DedicatedEffects.Length; i++)
        {
            if (!_DedicatedAnimating[i])
            {
                target = i;
                break;
            }

            if (_DedicatedElapsed[i] > highestElapsed)
            {
                highestElapsed = _DedicatedElapsed[i];
                target = i;
            }
        }

        if (target < 0 || _DedicatedEffects[target] == null) return null;

        _DedicatedElapsed[target] = 0f;
        _DedicatedAnimating[target] = true;
        _DedicatedEffects[target].Tick(0);
        return _DedicatedEffects[target];
    }

    /// <summary>
    /// Advances any currently-animating dedicated slots. Idle (finished) slots are
    /// left alone entirely — no Tick() call — so they stop dirtying the canvas once
    /// their animation completes, instead of continuing to redraw a frozen frame.
    /// </summary>
    public void UpdateDedicatedEffects(float deltaTime)
    {
        if (_DedicatedEffects == null) return;

        for (var i = 0; i < _DedicatedEffects.Length; i++)
        {
            if (!_DedicatedAnimating[i] || _DedicatedEffects[i] == null)
                continue;

            _DedicatedElapsed[i] += deltaTime;
            float x = Mathf.Clamp01(_DedicatedElapsed[i] / DedicatedEffectDuration);
            _DedicatedEffects[i].Tick(x);

            if (x >= 1f)
                _DedicatedAnimating[i] = false;
        }
    }

    /// <summary>
    /// Returns all dedicated effects to the shared pool. Must be called when this
    /// hold note is removed from the queue, or the instances leak permanently.
    /// </summary>
    public void ReturnDedicatedEffects()
    {
        if (_DedicatedEffects == null) return;

        for (var i = 0; i < _DedicatedEffects.Length; i++)
            if (_DedicatedEffects[i] != null)
                PlayerScreen.sMain.JudgeScreenManager.ReturnDedicated(_DedicatedEffects[i], _DedicatedIsOverflow[i]);

        _DedicatedEffects = null;
        _DedicatedIsOverflow = null;
        _DedicatedElapsed = null;
        _DedicatedAnimating = null;
    }
}

