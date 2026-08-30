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

// ReSharper disable once CheckNamespace
/// <summary>
///     Represents a class that holds information about the hold note in the game.
/// </summary>
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

/// <summary>
///     Represents a touch event and its associated state and metadata for player input handling.
/// </summary>
public class TouchClass
{
    /// <summary>
    ///     Backing field for the <see cref = "flickDirection"/> property.
    /// </summary>
    private float _FlickDirection;

    public float DiscreteHitobjectDistance;

    /// <summary>
    ///     If the touch is within the PassWindow range of a Discrete Hitobject.
    /// </summary>
    /// <remarks>
    ///     This is to prevent additional inputs from being passed to a non-discrete hitobject, which results in unexpected
    ///     early judgement.
    /// </remarks>
    public bool DiscreteHitobjectIsInRange;

    /// <summary>
    ///     Indicates whether the touch has been recognized as a flick gesture.
    /// </summary>
    public bool Flicked;

    /// <summary>
    ///     The time (in seconds) when the player flicked past the threshold.
    /// </summary>
    public double FlickTime;

    public bool Initial;

    /// <summary>
    ///     Indicates whether the touch is currently being held.
    /// </summary>
    public bool IsHolding;

    /// <summary>
    ///     The nearest discrete hitobject to the touch.
    /// </summary>
    /// <remarks>
    ///     This is to compare the attributes to the normal hitobject.
    /// </remarks>
    public HitPlayer NearestDiscreteHitobject;

    /// <summary>
    ///     The <see cref = "HitPlayer"/> object that the touch interacted with, if any.
    /// </summary>
    public HitPlayer QueuedHit;

    /// <summary>
    ///     The distance from the touch to the associated hit object.
    /// </summary>
    public float QueuedHitDistance;

    /// <summary>
    ///     The time (in seconds) when the touch started.
    /// </summary>
    public double StartTime;

    /// <summary>
    ///     Flick gesture recognizer for this touch. Owns detection only; every judgement
    ///     decision that follows from it stays in <see cref = "PlayerInputManager"/>.
    /// </summary>
    public FlickTracker FlickTracker = FlickTracker.Create();

    /// <summary>
    ///     Indicates whether the touch is considered a tap (true at the moment the finger touches the screen).
    /// </summary>
    public bool Tapped = true;

    /// <summary>
    ///     The Unity <see cref = "Touch"/> structure representing the current touch event.
    /// </summary>
    public Touch Touch;

    /// <summary>
    ///     The direction of the flick gesture in degrees, if applicable.
    /// </summary>
    /// <remarks>
    ///     This property can only be set if <see cref = "Flicked"/> is true.
    /// </remarks>
    public float flickDirection
    {
        get =>
            _FlickDirection;

        set
        {
            // Only store finite values; NaN can arise from a degenerate SignedAngle call (zero-length delta)
            // and should be silently ignored rather than crashing the input update.
            if (!float.IsFinite(value)) return;

            _FlickDirection = value;
        }
    }
}

public class PlayerInputManager : MonoBehaviour
{
    static readonly ProfilerMarker sr_TouchInputLoop = new("UpdateInput: Touch Input Loop");
    static readonly ProfilerMarker sr_HitQueueLoop = new("UpdateInput: HitQueue Processor Loop");
    static readonly ProfilerMarker sr_HoldQueueBlock = new("UpdateInput: HoldQueue Processor Block");
    static readonly ProfilerMarker sr_DiscreteHitQueueLoop = new("UpdateInput: DiscreteHitQueue Processor Loop");
    static readonly ProfilerMarker sr_HitobjectProcessor = new("HitobjectProcessor");
    static readonly ProfilerMarker sr_HoldQueueProcessor = new("HoldQueue_Processor");

    public static  PlayerInputManager sInstance;
    private static double             s_DeltaTime;
    public         PlayerScreen       Player;

    [Space] public bool Autoplay;

    [Space] [ReadOnly] public float UpdatePerSecond = float.NaN;

    [ReadOnly]        public string              Delta = s_DeltaTime.ToString("F3") + "ms";
    [ReadOnly]        public int                 TouchClassesCount;
    [ReadOnly]        public int                 HoldQueueCount;
    [ReadOnly]        public int                 HitQueueCount;
    [ReadOnly]        public int                 DiscreteHitQueueCount;
    [HideInInspector] public List<HitPlayer>     HitQueue         = new();
    [HideInInspector] public List<HitPlayer>     DiscreteHitQueue = new(); // Special queue for catch/flickable notes
    public readonly          List<HoldNoteClass> HoldQueue        = new();

    public readonly List<TouchClass> TouchClasses = new();

    /// <summary>
    /// Fully clears input state for a chart retry/reload. Must be called before the
    /// player's lanes/HitPlayers are destroyed — a HoldNoteClass whose HitPlayer gets
    /// destroyed out from under it (e.g. by Destroy(lane.gameObject) on retry) never
    /// goes through PurgeHitPlayer, so its dedicated judge-effect pool would otherwise
    /// leak permanently instead of being returned.
    /// </summary>
    public void ResetForRetry()
    {
        foreach (HoldNoteClass holdNoteEntry in HoldQueue)
            holdNoteEntry.ReturnDedicatedEffects();

        HoldQueue.Clear();
        HitQueue.Clear();
        DiscreteHitQueue.Clear();
        TouchClasses.Clear();
    }

    private bool
        _InitLog = true; // log only once on PlayerScreen initialization (we don't want this to spam every millisecond,

    // no?)

    private double _LastTimeMs = -1;

    public void Awake() // Unity's version of constructor
    {
        EnhancedTouchSupport.Enable();
        sInstance = this;
    }


    public void OnDestroy() // Deconstructor
    {
        EnhancedTouchSupport.Disable();
    }

    /// <summary>
    ///     Determines whether the absolute angular difference between the expected and actual flick directions
    ///     is within a reasonable range (±25 degrees) to be considered a valid flick gesture.
    /// </summary>
    /// <param name = "expected"> The expected flick direction from the hit object, in degrees. </param>
    /// <param name = "actual"> The actual flick direction done by the player, in degrees. </param>
    /// <returns> true if within a reasonable range, false otherwise. </returns>

    private bool ValidateFlickDirection(float expected, float actual)
    {
        float angularDifference = Mathf.DeltaAngle(expected, actual); // Signed difference (-180 to +180)
        float absDiff = Mathf.Abs(angularDifference);
        bool comparison = absDiff <= 25f;

        // More leniency
        bool closeEnough = absDiff <= 27.5f || Mathf.Approximately(absDiff, 25f);

        //Debug.Log(
        //    $"ValidatingFlickPass: Expected {expected}°, got {actual}°, Difference ~25/27.5 < ({angularDifference})° ({comparison || closeEnough})");

        return comparison || closeEnough; // ±25 degrees
    }

    /// <summary>
    ///     Logs a message only once when the PlayerScreen is initialized.
    /// </summary>
    /// <param name = "msg"> The message to log. </param>
    private void InitLogger(string msg)
    {
        if (_InitLog) Debug.Log(msg);
    }

    /// <summary>
    ///     Adds the given <see cref = "HitPlayer"/> object to the player's hit queue, sorted by time.
    /// </summary>
    /// <param name = "hit"> The <see cref = "HitPlayer"/> object to add to the queue. </param>
    public void AddToQueue(HitPlayer hit)
    {
        int index = HitQueue.FindLastIndex(x => x.Time < hit.Time);
        //Debug.Log($"Adding hit at time {hit.Time} to queue. Inserting at index {index + 1}.");
        HitQueue.Insert(index + 1, hit);
    }

    public void RemoveFromQueue(HitPlayer hit)
    {
        // Don't remove the hit if it's not in the first entry yet (cutting lines)
        if (HitQueue.Contains(hit) && HitQueue[0] == hit)
        {
            //Debug.Log($"Removing hit at time {hit.Time} from queue.");
            HitQueue.Remove(hit);
        }
    }

    /// <summary>
    /// Unconditionally scrubs every reference to <paramref name="hit"/> from every queue/cache
    /// this manager holds. Called right before a HitPlayer is returned to PlayerScreen's pool,
    /// since a pooled instance can be handed out to a brand-new note as soon as the same frame
    /// it's returned — any stale reference left behind would silently start aliasing that new
    /// note (same underlying object, so flag-based "is this finished" checks stop working the
    /// instant it's reused).
    /// </summary>
    public void PurgeHitPlayer(HitPlayer hit)
    {
        HitQueue.RemoveAll(x => x == hit);
        DiscreteHitQueue.RemoveAll(x => x == hit);

        for (int i = HoldQueue.Count - 1; i >= 0; i--)
        {
            if (HoldQueue[i].HitObject != hit) continue;

            HoldQueue[i].ReturnDedicatedEffects();
            HoldQueue.RemoveAt(i);
        }

        foreach (TouchClass touch in TouchClasses)
        {
            if (touch.QueuedHit == hit) touch.QueuedHit = null;
            if (touch.NearestDiscreteHitobject == hit) touch.NearestDiscreteHitobject = null;
        }
    }

    private void EnqueueHoldNote(TouchClass touch, bool missed = false)
    {
        if (touch.QueuedHit.PendingHoldQueue)
        {
            HoldQueue.Add(
                new HoldNoteClass
                {
                    HitObject = touch.QueuedHit,
                    IsPlayerHolding = touch.IsHolding,
                    holdPassDrainValue = 1, // Little leniency won't hurt
                    AssignedTouch = touch
                });

            //Debug.Log("Hold hitobject head handled, passing to hold queue.");
        }
    }

    private void EnqueueHoldNote(HitPlayer hitObject, bool missed = false)
    {
        if (hitObject.PendingHoldQueue)
        {
            HoldQueue.Add(
                new HoldNoteClass
                {
                    HitObject = hitObject,
                    holdPassDrainValue = missed ? 0 : 1
                });

            //Debug.Log("Hold hitobject head handled, passing to hold queue.");
        }
    }
    
    public void UpdateInput() // Main input thread
    {

        double currentTimeMs = Player.CurrentTime * 1000.0;

        #region Unity statistics
        TouchClassesCount = TouchClasses.Count;
        HoldQueueCount = HoldQueue.Count;
        HitQueueCount = HitQueue.Count;
        DiscreteHitQueueCount = DiscreteHitQueue.Count;
        #endregion

        #if UNITY_EDITOR
        bool editorPref = EditorPrefs.GetBool("JANOARG/Enable Autoplay", false);

        if (editorPref != Autoplay)
        {
            Debug.Log(editorPref ? "Autoplay enabled in Editor Preferences." : "Autoplay disabled in Editor Preferences.");

            Autoplay = editorPref;
        }
        #endif
        
        if (!Autoplay) // Player input
        {
            InitLogger("Autoplay: OFF");

            int inputCount = Touch.activeTouches.Count;

            float screenDpi =
                Screen.dpi > 0
                    ? Screen.dpi
                    : 100; // Minimum DPI

            // Distance half of flick detection: how far a finger must travel before the motion
            // counts as a flick at all. The speed half lives in FlickTracker; both must pass.
            float flickDistanceThreshold = screenDpi * 0.2f; // 20% of screen dpi (about 0.5cm)
            InitLogger($"Set flick distance threshold to {flickDistanceThreshold}px (DPI: {screenDpi})");

            // Main touch iterator
            sr_TouchInputLoop.Begin();
            for (var a = 0; a < inputCount; a++)
            {
                Touch inputEntry = Touch.activeTouches[a];
                int fingerIndex = inputEntry.finger.index;

                // Handle touch end/cancel
                if (inputEntry is { isInProgress: false, phase: TouchPhase.Ended or TouchPhase.Canceled })
                {
                    // Flush any queued tap hit before removing the touch — a very fast tap can
                    // begin and end within a single UpdateInput call, so the normal queued-hit
                    // resolver at the bottom of the frame would never see it.
                    TouchClass endingTouch = TouchClasses.Find(t => t.Touch.finger.index == fingerIndex);

                    // A tap-flick is often completed by the same motion that lifts the finger, so
                    // give a pending claim the same last chance the tap path gets.
                    if (endingTouch?.QueuedHit != null &&
                        endingTouch.QueuedHit.Current.Flickable &&
                        endingTouch.QueuedHit.Current.Type == HitObject.HitType.Normal)
                        TryResolveTapFlick(endingTouch, endingTouch.QueuedHit, flickDistanceThreshold);

                    if (endingTouch?.QueuedHit != null &&
                        !endingTouch.QueuedHit.IsProcessed &&
                        endingTouch.QueuedHit.Current.Type == HitObject.HitType.Normal &&
                        !endingTouch.QueuedHit.Current.Flickable)
                    {
                        HitPlayer queuedHit = endingTouch.QueuedHit;

                        Player.Hit(
                            queuedHit,
                            endingTouch.StartTime + Player.Settings.JudgmentOffset - queuedHit.Time
                        );

                        // Player.Hit() can reenter PurgeHitPlayer (via RemoveHitPlayer, for any
                        // non-hold note) and null out endingTouch.QueuedHit out from under us —
                        // restore it so EnqueueHoldNote(endingTouch)'s own read still sees it.
                        endingTouch.QueuedHit = queuedHit;

                        queuedHit.IsProcessed = true;
                        EnqueueHoldNote(endingTouch);
                        endingTouch.QueuedHit = null;
                    }

                    TouchClass endedTouch = TouchClasses.Find(t => t.Touch.finger.index == fingerIndex);
                    endedTouch?.FlickTracker.Reset();
                    TouchClasses.RemoveAll(input => input.Touch.finger.index == fingerIndex);

                    continue;
                }

                // Find existing touch or create new one
                TouchClass touchClass = TouchClasses.Find(t => t.Touch.finger.index == fingerIndex);

                if (touchClass == null) // New touch
                {
                    touchClass = new TouchClass
                    {
                        Touch = inputEntry,
                        StartTime = Player.CurrentTime,
                        Initial = true
                    };

                    touchClass.FlickTracker.Push(Time.unscaledTime, inputEntry.startScreenPosition);
                    TouchClasses.Add(touchClass);
                }
                else // Existing touch
                {
                    touchClass.Touch = inputEntry;

                    // Flick detector — FlickTracker owns gesture recognition entirely; this
                    // only relays its verdict. Pushed on the wall clock rather than
                    // Player.CurrentTime, whose audio-callback granularity can repeat a
                    // timestamp across frames and make a stationary finger look infinitely fast.
                    touchClass.FlickTracker.Push(Time.unscaledTime, inputEntry.screenPosition);

                    if (touchClass.FlickTracker.IsFlicked && !touchClass.Flicked)
                    {
                        touchClass.Flicked = true;
                        touchClass.FlickTime = Player.CurrentTime;
                        touchClass.flickDirection = touchClass.FlickTracker.FlickAngle;
                    }

                    // Invalidator (policy unchanged): a flick left unclaimed for longer than the
                    // perfect window is dropped once a flickable is actually in range, so a stale
                    // gesture cannot clear the next note for free.
                    if (touchClass.Flicked)
                    {
                        bool flickTimedOut = Math.Abs(Player.CurrentTime - touchClass.FlickTime) >
                                             Player.PerfectWindow;

                        bool nearAnyFlickable = HitQueue.Any(hit =>
                            hit.Current.Flickable &&
                            !hit.IsProcessed &&
                            Math.Abs(hit.Time - Player.CurrentTime) <=
                            Player.PassWindow
                        );

                        if (flickTimedOut && nearAnyFlickable)
                        {
                            touchClass.Flicked = false;
                            touchClass.FlickTracker.ConsumeFlick();
                        }
                    }

                    // Already handling the same touch on the second pass, consider it holding
                    touchClass.IsHolding = true;
                }

                touchClass.Initial = false;
            }
            sr_TouchInputLoop.End();

            double judgementOffsetTime = Player.CurrentTime + Player.Settings.JudgmentOffset; // Judgement offset

            InitLogger(
                $"Judgement-offset time: {judgementOffsetTime} (Current time: {Player.CurrentTime}, Offset: {Player.Settings.JudgmentOffset})");


            sr_HitQueueLoop.Begin();
            for (var a = 0; a < HitQueue.Count; a++) // Chart HitObject queue processor
            {
                HitPlayer hitIteration = HitQueue[a];

                if (!hitIteration || hitIteration.IsReturned) // Already finished (destroyed, or returned to the pool)
                {
                    // Debug.Log($"Removing destroyed HitPlayer {a} from queue.");
                    HitQueue.RemoveAt(a);
                    a--;

                    continue; // Go check the next hitobject
                }

                double hitobjectTimingDelta =
                    judgementOffsetTime - hitIteration.Time; // Hit time adjusted by judgement offset

                bool isDiscreteHitObject =
                    hitIteration.Current.Type == HitObject.HitType.Catch || hitIteration.Current.Flickable;

                float window = isDiscreteHitObject // Timing window per hitobject type
                    ? Player.PassWindow // You either hit it or miss it
                    : Player.GoodWindow; // Only hit as far as the MISALIGNED timing window


                // Draw debug hitboxes 
                /*if (hitobjectTimingDelta >= -window && PlayerHitboxVisualizer.main)
                {
                    PlayerHitboxVisualizer.main.DrawHitScreenCoordDebug(
                        hitIteration.HitCoord,
                        Color.Lerp(
                            Color.clear,
                            hitobjectTimingDelta > 0 ? Color.red : PlayerScreen.CurrentChart.Palette.InterfaceColor *
                            new ColorFrag(a: 1),
                            Mathf.Pow(1 - Mathf.Abs(hitobjectTimingDelta / Player.GoodWindow), 2)
                        )
                    );
                }*/


                var alreadyHit = false;

                if (hitIteration.Current.HoldLength > 0 &&
                    !hitIteration.PendingHoldQueue) // Pass to HoldNoteClass if there's a hold length
                {
                    hitIteration.PendingHoldQueue = true; // Mark as pending hold queue
                    //Debug.Log($"Hitobject at {hitIteration.Time} is a hold note. Adding to hold queue soon.");
                }

                if (hitobjectTimingDelta >= -window && !hitIteration.IsProcessed)
                {
                    sr_HitobjectProcessor.Begin();
                    HitobjectProcessor(hitIteration, flickDistanceThreshold, hitobjectTimingDelta, ref alreadyHit);
                    sr_HitobjectProcessor.End();

                    // For additional inputs
                    foreach (TouchClass touch in TouchClasses)
                        if (
                            isDiscreteHitObject &&
                            Vector2.Distance(touch.Touch.screenPosition, hitIteration.HitCoord.Position) <=
                            hitIteration.HitCoord.Radius
                        )
                        {
                            touch.DiscreteHitobjectIsInRange = true;
                            touch.NearestDiscreteHitobject = hitIteration;
                        }

                    // Pass to DiscreteHitQueue
                    if (hitIteration.InDiscreteHitQueue ||
                        (alreadyHit && hitIteration.Current.Type == HitObject.HitType.Catch))
                    {
                        DiscreteHitQueue.Add(hitIteration);
                        hitIteration.InDiscreteHitQueue = false;

                        // Remove from the main queue
                        HitQueue.Remove(hitIteration);
                        a--; // Compensate for the removed element so the next entry isn't skipped
                    }

                    if (!alreadyHit &&
                        hitobjectTimingDelta > window) // Didn't hit the hitobject within the timing window
                    {
                        Player.Hit(hitIteration, float.PositiveInfinity, false);

                        hitIteration.IsProcessed = true; // Prevent Hit function from triggering more than once

                        // Clear any touch that was assigned to this missed hit
                        foreach (TouchClass touch in TouchClasses)
                            if (touch.QueuedHit == hitIteration)
                            {
                                touch.QueuedHit = null;
                                touch.DiscreteHitobjectIsInRange = false;
                            }

                        //Debug.Log(
                        //    $"Hitobject at {hitIteration.Time} ({hitIteration.Current.Type}) missed. Radius: {hitIteration.HitCoord.Radius}, Hold? {(hitIteration.PendingHoldQueue ? "Yes" : "No")}");

                        EnqueueHoldNote(hitIteration, true);
                    }
                }

                // Skip checks if none of the hitobjects are even near window range
                if (hitobjectTimingDelta < -Math.Max(Player.PassWindow, Player.GoodWindow)) break;
            }
            sr_HitQueueLoop.End();

            sr_HoldQueueBlock.Begin();
            if (HoldQueue.Count != 0) // Hold note processor
            {
                //// Camera handling and other extra stuff is done here to calculate hold note hitboxes and positions on the fly
                //// As it has dynamic attributes as it progresses, unlike normal hitobjects.

                float beat = PlayerScreen.sTargetSong.Timing.ToBeat((float)judgementOffsetTime); // Get current BPM

                // Camera handling
                var currentCamera =
                    (CameraController)PlayerScreen.sTargetChart.Data.Camera
                        .GetStoryboardableObject(
                            beat); // Get camera data for the current

                // beat

                // Update transforms
                Player.Pseudocamera.transform.position = currentCamera.CameraPivot;
                Player.Pseudocamera.transform.eulerAngles = currentCamera.CameraRotation;
                Player.Pseudocamera.transform.Translate(Vector3.back * currentCamera.PivotDistance);

                for (var a = 0; a < HoldQueue.Count; a++)
                {
                    HoldNoteClass holdNoteEntry = HoldQueue[a];

                    //Debug.Log($"Processing hold note entry {a} at time {holdNoteEntry.HitObject.Time}.");

                    // If the hold note doesn't exist (it's already completed)
                    sr_HoldQueueProcessor.Begin();
                    HoldQueue_Processor(holdNoteEntry, ref a, beat, judgementOffsetTime);
                    sr_HoldQueueProcessor.End();
                }
            }
            sr_HoldQueueBlock.End();

            sr_DiscreteHitQueueLoop.Begin();
            for (var i = 0; i < DiscreteHitQueue.Count; i++)
            {
                HitPlayer hitObject = DiscreteHitQueue[i];

                double time = judgementOffsetTime - hitObject.Time;

                // Flickables are now hit immediately in HitobjectProcessor — skip them here.
                if (hitObject.Current.Flickable)
                {
                    hitObject.InDiscreteHitQueue = false;
                    DiscreteHitQueue.Remove(hitObject);
                    continue;
                }

                if (judgementOffsetTime >= hitObject.Time && hitObject.Current.Type == HitObject.HitType.Catch)
                {
                    if (!hitObject.IsProcessed)
                        Player.Hit(hitObject, time);

                    hitObject.InDiscreteHitQueue = false;
                    hitObject.IsProcessed = true;

                    // Clear any touch that was assigned to this hit
                    foreach (TouchClass touch in TouchClasses)
                        if (touch.QueuedHit == hitObject)
                        {
                            touch.QueuedHit = null;
                            touch.DiscreteHitobjectIsInRange = false;
                        }

                    EnqueueHoldNote(hitObject: hitObject);

                    if (!hitObject || hitObject.IsReturned)
                        continue; // Already finished (destroyed, or returned to the pool)
                    DiscreteHitQueue.Remove(hitObject);
                }
            }
            sr_DiscreteHitQueueLoop.End();

            foreach (TouchClass touch in TouchClasses)
            {
                //Debug.Log($"Processing queued hit for touch {touch.Touch.finger.index} at time {touch.StartTime}.");

                if (
                    touch.QueuedHit && // if the input in question interacted with a hitobject
                    !touch.QueuedHit.IsProcessed && // Haven't yet hit
                    // And is just a tap note
                    touch.QueuedHit.Current.Type == HitObject.HitType.Normal &&
                    !touch.QueuedHit.Current.Flickable
                )
                {
                    HitPlayer queuedHit = touch.QueuedHit;

                    Player.Hit(
                        queuedHit,
                        touch.StartTime + Player.Settings.JudgmentOffset - queuedHit.Time
                    );

                    //Debug.Log(
                    //    $"Hit queued hitobject at {touch.StartTime + Player.Settings.JudgmentOffset - queuedHit.Time} for touch {touch.Touch.finger.index}.");

                    // Player.Hit() can reenter PurgeHitPlayer (via RemoveHitPlayer, for any
                    // non-hold note) and null out touch.QueuedHit out from under us — restore
                    // it so the rest of this block, and EnqueueHoldNote(touch)'s own read of
                    // touch.QueuedHit, still see the note that was actually just hit.
                    touch.QueuedHit = queuedHit;

                    queuedHit.IsProcessed = true; // Mark as hit

                    EnqueueHoldNote(touch);
                }

                // QueuedHit is normally a single-frame occupancy marker, cleared here so the touch
                // is free next frame without ever being claimable twice within this one. The one
                // exception is a tap-flick claim: the flick that resolves it necessarily lands on a
                // later frame, so that claim survives until it is either resolved or the note's
                // window expires (the miss branch in the HitQueue loop clears it).
                bool pendingTapFlick =
                    touch.QueuedHit &&
                    !touch.QueuedHit.IsProcessed &&
                    touch.QueuedHit.Current.Flickable &&
                    touch.QueuedHit.Current.Type == HitObject.HitType.Normal;

                if (!pendingTapFlick) touch.QueuedHit = null;

                touch.Tapped = false; // Tap only lasts for a single frame
            }
        }
        else // Autoplay, From old input manager since it works as is (for now)
        {
            for (var i = 0; i < HitQueue.Count; i++)
            {
                HitPlayer currentHit = HitQueue[i];

                if (!currentHit || currentHit.IsReturned) // Already finished (destroyed, or returned to the pool)
                {
                    HitQueue.RemoveAt(i);
                    i--;
                }
                else if (currentHit.IsProcessed) // Autoplay's hold note processor
                {
                    while (currentHit.HoldTicks.Count > 0 && currentHit.HoldTicks[0] <= Player.CurrentTime)
                    {
                        //// Special judgement handling for hold ticks (nothing else handles this)
                        Player.AddScore(1, null);

                        Color interfaceColor = new Color(PlayerScreen.sCurrentChart.Palette.InterfaceColor.r, PlayerScreen.sCurrentChart.Palette.InterfaceColor.g, PlayerScreen.sCurrentChart.Palette.InterfaceColor.b, 0.4f);
                        var effect = PlayerScreen.sMain.JudgeScreenManager.BorrowEffect(currentHit, null, interfaceColor);
                        var rectTransform = (RectTransform)effect.transform;
                        rectTransform.position = CommonSys.sMain.MainCamera.WorldToScreenPoint(currentHit.transform.position);
                        
                        currentHit.HoldTicks.RemoveAt(0);
                    }

                    if (currentHit.HoldTicks.Count == 0)
                    {
                        // RemoveHitPlayer already purges this entry from HitQueue (see
                        // PlayerInputManager.PurgeHitPlayer) — an explicit RemoveAt(i) here would
                        // now delete whatever shifted into index i instead, dropping an unrelated note.
                        Player.RemoveHitPlayer(currentHit);
                        i--;
                    }
                }
                else if (currentHit.Time <= Player.CurrentTime) // Hit it
                {
                    Player.Hit(currentHit, 0);
                }
                else // Autoplay's job is done (final hitobject is destroyed)
                {
                    break;
                }
            }
        }

        _InitLog = false; // Disable logging after the first initialization

        if (_LastTimeMs < 0)
        {
            _LastTimeMs = currentTimeMs; // First frame init
            s_DeltaTime = 16.666; // Fake 60fps to start
            UpdatePerSecond = 60f;
        }
        else
        {
            s_DeltaTime = currentTimeMs - _LastTimeMs;
            UpdatePerSecond = 1000f / (float)s_DeltaTime;
            _LastTimeMs = currentTimeMs;
            Delta = s_DeltaTime.ToString("F3") + "ms";
        }
    }

    private void HoldQueue_Processor(HoldNoteClass holdNoteEntry, ref int queuePtr, float beat, double judgementOffsetTime)
    {
        if (!holdNoteEntry.HitObject || holdNoteEntry.HitObject.IsReturned)
        {
            holdNoteEntry.ReturnDedicatedEffects();
            HoldQueue.RemoveAt(queuePtr);
            queuePtr--; // Pointer rollback

            return;
        }

        holdNoteEntry.UpdateDedicatedEffects(Time.deltaTime);

        // Note position
        var laneHoldNote =
            (Lane)holdNoteEntry.HitObject.Lane.Original.GetStoryboardableObject(beat); // Which lane is the hold note on?

        LanePosition
            step = laneHoldNote.GetLanePosition(beat, beat, PlayerScreen.sTargetSong.Timing); // Get the lane position for the current beat

        Vector3 startHoldPosition = laneHoldNote.Position +
                                    Quaternion.Euler(laneHoldNote.Rotation) * step.StartPosition;

        Vector3 endHoldPosition = laneHoldNote.Position +
                                  Quaternion.Euler(laneHoldNote.Rotation) * step.EndPosition;

        LaneGroupPlayer currentHoldGroupPlayer = holdNoteEntry.HitObject.Lane.Group;

        //Debug.Log(
        //    $"Got; Lane: {laneHoldNote.Name}, Start Position: {startHoldPosition}, End Position: {endHoldPosition}");

        // Apply transforms in the group
        while (currentHoldGroupPlayer) // Current LaneGroupPlayer still exists
        {
            var currentLaneGroup =
                (LaneGroup)currentHoldGroupPlayer.Original
                    .GetStoryboardableObject(
                        beat); // Get the current lanegroup

            startHoldPosition = currentLaneGroup.Position +
                                Quaternion.Euler(currentLaneGroup.Rotation) * startHoldPosition; // Apply transform manually

            endHoldPosition = currentLaneGroup.Position +
                              Quaternion.Euler(currentLaneGroup.Rotation) * endHoldPosition;

            currentHoldGroupPlayer = currentHoldGroupPlayer.Parent; // Go to the parent LaneGroupPlayer
        }

        //Debug.Log($"Transformed; Start Position: {startHoldPosition}, End Position: {endHoldPosition}");

        var hitObject =
            (HitObject)holdNoteEntry.HitObject.Original
                .GetStoryboardableObject(
                    beat); // Get the hitobject data for thecurrent beat

        //Debug.Log(
        //    $"Hold note hitobject data: Type: {hitObject.Type}, Hold Length: {hitObject.HoldLength}, Position: {hitObject.Position}");

        // Calculate hitbox positions
        // I dunno what lerp is but just go with it, I guess

        Vector3 holdNoteLerpStart = Vector3.LerpUnclamped(
            startHoldPosition,
            endHoldPosition,
            hitObject.Position
        );

        Vector3 holdNoteLerpEnd = Vector3.LerpUnclamped(
            startHoldPosition,
            endHoldPosition,
            hitObject.Position + hitObject.Length
        );

        Vector2 holdNoteHitboxStart = Player.Pseudocamera.WorldToScreenPoint(holdNoteLerpStart);

        Vector2 holdNoteHitboxEnd = Player.Pseudocamera.WorldToScreenPoint(holdNoteLerpEnd);

        //Debug.Log($"Hold note hitbox start: {holdNoteHitboxStart}, end: {holdNoteHitboxEnd}");

        holdNoteEntry.HitObject.HitCoord = new HitScreenCoord
        {
            Position = (holdNoteHitboxStart + holdNoteHitboxEnd) / 2,
            Radius = Mathf.Max( 
                Vector2.Distance(holdNoteHitboxStart, holdNoteHitboxEnd) / 2 + Player.ScaledExtraRadius,
                Player.ScaledMinimumRadius
            )
        };

        // Draw the hitobject radius
        /*if (PlayerHitboxVisualizer.main)
                    {
                        PlayerHitboxVisualizer.main.DrawHitScreenCoordDebug(
                            holdNoteEntry.HitObject.HitCoord,
                            Color.green
                        );
                    }*/

        //Debug.Log(
        //    $"Hold note hitbox position: {holdNoteEntry.HitObject.HitCoord.Position}, radius: {holdNoteEntry.HitObject.HitCoord.Radius}");

        // Hitbox checker
        holdNoteEntry.AssignedTouch = null;

        // Assigned a new touch
        holdNoteEntry.AssignedTouch = TouchClasses.Find(touch => 
                Vector2.Distance( touch.Touch .screenPosition, holdNoteEntry.HitObject .HitCoord .Position ) <= holdNoteEntry.HitObject.HitCoord .Radius // Be careful, it's <= not <
        );

        // Taking advantage of inline checks, since List<T>.Find() can give null
        holdNoteEntry.IsPlayerHolding = holdNoteEntry.AssignedTouch != null;

        // Update Drain value
        holdNoteEntry.holdPassDrainValue = Mathf.Clamp01( holdNoteEntry.holdPassDrainValue + Time.deltaTime / Player.PassWindow * (holdNoteEntry.IsPlayerHolding 
                ? 1f : -1f)
        );

        //Debug.Log($"Updating drain value: {holdNoteEntry.holdPassDrainValue}");


        // Check if the hold note is eligible for scoring
        if (!holdNoteEntry.IsScoring && holdNoteEntry.holdPassDrainValue >= 1)
            holdNoteEntry.IsScoring = true;
        else if (holdNoteEntry.IsScoring && holdNoteEntry.holdPassDrainValue == 0)
            holdNoteEntry.IsScoring = false;


        // Hold ticks processing
        while (holdNoteEntry.HitObject.HoldTicks.Count > 0 &&
               holdNoteEntry.HitObject.HoldTicks[0] <= judgementOffsetTime + float.Epsilon)
        {
            Player.AddScore(
                holdNoteEntry.IsScoring
                    ? 1 : 0,
                null
            );

            //Debug.Log(
            //    $"Hold tick at {holdNoteEntry.HitObject.HoldTicks[0]} processed. " +
            //    $"HoldEligible: {holdNoteEntry.IsScoring}, Score: {(holdNoteEntry.IsScoring ? 1 : 0)}");

            Player.HitObjectHistory.Add(
                new HitObjectHistoryItem( holdNoteEntry.HitObject.HoldTicks[0], HitObjectHistoryType.Catch, holdNoteEntry.IsScoring 
                        ? 0 : float .PositiveInfinity // Catch note-like hitobject history
                )
            );

            //Debug.Log(
            //    $"Hold tick at {holdNoteEntry.HitObject.HoldTicks[0]} removed from hold ticks list.");

            // Remove the first hold tick (and pretty much shift the next tick in)
            holdNoteEntry.HitObject.HoldTicks.RemoveAt(0);

            // Handle hold tick just like how HitPlayer does — reuses a small dedicated
            // pool of effects instead of borrowing/returning one per tick (see
            // HoldNoteClass.EnsureDedicatedEffects for why: at high BPM, ticks fire
            // far more often than a single effect's animation takes to finish).
            if (holdNoteEntry.IsScoring)
            {
                Color interfaceColor = new Color(PlayerScreen.sCurrentChart.Palette.InterfaceColor.r, PlayerScreen.sCurrentChart.Palette.InterfaceColor.g, PlayerScreen.sCurrentChart.Palette.InterfaceColor.b, 0.32f);
                holdNoteEntry.EnsureDedicatedEffects(interfaceColor);
                JudgeScreenEffect effect = holdNoteEntry.PulseDedicatedEffect();

                if (effect != null)
                {
                    var rt = (RectTransform)effect.transform;
                    rt.position = CommonSys.sMain.MainCamera.WorldToScreenPoint(holdNoteEntry.HitObject.transform.position);
                }
            }

            // Missed hold tick, no effect
        }

        if (holdNoteEntry.HitObject.HoldTicks.Count <= 0) // No hold ticks left
        {
            // RemoveHitPlayer already purges this entry from HoldQueue (see
            // PlayerInputManager.PurgeHitPlayer) — an explicit RemoveAt(queuePtr) here would
            // now delete whatever shifted into that index instead, dropping an unrelated hold note.
            Player.RemoveHitPlayer(holdNoteEntry.HitObject);
            queuePtr--; // Pointer rollback
        }
    }

    /// <summary>
    ///     Second stage of a tap-flick: the tap has already claimed <paramref name = "note"/> into
    ///     this touch's queue, and this decides whether the flick that followed satisfies it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Three independent gates, each doing exactly one job: speed (did the finger move fast
    ///         enough to be a flick — <see cref = "FlickTracker.IsFlicked"/>), distance (did it move
    ///         far enough to mean it), and containment (is it still on the note).
    ///     </para>
    ///     <para>
    ///         Containment for a directional note is the hit radius stretched indefinitely along
    ///         FlickDirection — a beam. The beam is symmetric, so a backwards flick sits inside it
    ///         and is rejected on angle instead; direction is never inferred from position.
    ///     </para>
    /// </remarks>
    /// <returns> true if the note was hit. </returns>
    private bool TryResolveTapFlick(TouchClass touch, HitPlayer note, float flickDistanceThreshold)
    {
        if (!note || note.IsReturned || note.IsProcessed) return false;

        if (!touch.FlickTracker.IsFlicked) return false;

        Vector2 current = touch.Touch.screenPosition;

        if (Vector2.Distance(current, touch.Touch.startScreenPosition) < flickDistanceThreshold)
            return false;

        Vector2 offset = current - note.HitCoord.Position;
        float radius = note.HitCoord.Radius;

        if (float.IsFinite(note.Current.FlickDirection)) // Directional
        {
            // Rotating the offset by +FlickDirection maps the flick axis onto +Y, so .x is the
            // perpendicular distance from the beam. .y is deliberately unused — the beam runs
            // indefinitely both ways, and which way the finger went is the angle check's job.
            float perpendicular = (Quaternion.Euler(0, 0, note.Current.FlickDirection) * offset).x;

            if (Mathf.Abs(perpendicular) >= radius) return false;

            if (!ValidateFlickDirection(note.Current.FlickDirection, touch.FlickTracker.FlickAngle))
                return false;
        }
        else if (offset.magnitude >= radius) // Omnidirectional: plain radius, no beam
        {
            return false;
        }

        // Graded off the tap rather than the flick: the tap is the rhythmic action and the flick
        // only confirms it, so an on-time tap isn't punished for a fractionally late confirmation.
        Player.Hit(note, touch.StartTime + Player.Settings.JudgmentOffset - note.Time);

        // Player.Hit() can reenter PurgeHitPlayer (via RemoveHitPlayer) and null QueuedHit out from
        // under us — restore it so EnqueueHoldNote's own read still sees the note just hit.
        touch.QueuedHit = note;
        note.IsProcessed = true;
        EnqueueHoldNote(note);

        touch.Flicked = false;
        touch.FlickTracker.ConsumeFlick();
        touch.flickDirection = 0;
        touch.QueuedHit = null; // claim discharged

        return true;
    }

    private void HitobjectProcessor(HitPlayer hitIteration, float flickDistanceThreshold, double hitobjectTimingDelta,
        ref bool alreadyHit)
    {
        if (hitIteration.Current.Flickable) // Flick notes (catch/tap)
        {
            // Tap-flicks are a two-stage gesture: the tap claims the note into the finger's
            // QueuedHit slot, and a flick on some LATER frame resolves that claim. It has to span
            // frames — Tapped is true only on the frame the finger lands, and FlickTracker needs at
            // least two samples before IsFlicked can be true, so the two can never both hold in a
            // single pass and the direction could never actually be judged from one.
            if (hitIteration.Current.Type == HitObject.HitType.Normal)
            {
                foreach (TouchClass touch in TouchClasses)
                {
                    // Stage 2 — this finger already claimed this note; see whether the flick lands.
                    if (touch.QueuedHit == hitIteration)
                    {
                        if (TryResolveTapFlick(touch, hitIteration, flickDistanceThreshold))
                            alreadyHit = true;

                        break;
                    }

                    // Stage 1 — claim it. Plain hit radius, identical for directional and
                    // omnidirectional notes; the beam governs only where the flick may travel,
                    // never where the tap may land.
                    if (!touch.Tapped) continue;

                    float tapDistance = Vector2.Distance(
                        touch.Touch.startScreenPosition,
                        hitIteration.HitCoord.Position);

                    if (tapDistance >= hitIteration.HitCoord.Radius) continue;

                    // Same note-priority rule the ordinary tap path uses: earliest note wins, and
                    // the closer one breaks a tie.
                    if (touch.QueuedHit &&
                        hitIteration.Time > touch.QueuedHit.Time &&
                        !(Mathf.Approximately(hitIteration.Time, touch.QueuedHit.Time) &&
                          tapDistance < touch.QueuedHitDistance))
                        continue;

                    touch.QueuedHit = hitIteration;
                    touch.QueuedHitDistance = tapDistance;
                    hitIteration.IsTapped = true;
                    alreadyHit = true;

                    break;
                }

                return;
            }

            // ── Catch-flicks: no tap to anchor to, so they stay single-stage. Behaviour is
            // unchanged; the branches removed from the verifier below were the Normal-only ones,
            // which were unreachable from here anyway.
            float distance = 0;

            foreach (TouchClass touch in TouchClasses)
            {
                distance = 0; // don't let a previous touch's value leak into this one

                if (touch.QueuedHit != null && !touch.Flicked) continue;

                if (!f_flickVerifier(hitIteration, touch)) continue;

                if (!hitIteration.IsProcessed)
                {
                    Player.Hit(hitIteration, hitobjectTimingDelta);
                    hitIteration.IsProcessed = true;
                    EnqueueHoldNote(hitIteration);
                }

                touch.Flicked = false;
                touch.FlickTracker.ConsumeFlick();
                touch.flickDirection = 0; // Clear stale angle so it doesn't falsely validate the next note

                // Mark the touch as occupied by this hitobject for the rest of the frame instead of
                // freeing it — otherwise a second close-in-time/position note processed later in
                // this same HitQueue pass would pass the "touch.QueuedHit != null" guard above and
                // get hit by the same physical touch.
                touch.QueuedHit = hitIteration;
                touch.DiscreteHitobjectIsInRange = true;
                touch.DiscreteHitobjectDistance = distance;

                alreadyHit = true;

                break; // First valid touch wins
            }

            #region CATCH-FLICK LOCAL FUNCTION
            bool f_flickVerifier(HitPlayer hitObject, TouchClass touch)
            {
                if (!touch.Flicked &&
                    (distance = Vector2.Distance(touch.Touch.startScreenPosition, hitObject.HitCoord.Position)
                    ) > hitObject.HitCoord.Radius &&
                    Vector2.Distance(touch.Touch.screenPosition, hitObject.HitCoord.Position)
                    > hitObject.HitCoord.Radius)
                    return false;

                if (!float.IsNaN(hitObject.Current.FlickDirection)) // Directional flick
                    return ValidateFlickDirection(hitObject.Current.FlickDirection, touch.flickDirection);

                return touch.Flicked;
            }
            #endregion

            return;
        }

        switch (hitIteration.Current.Type)
        {
            case HitObject.HitType.Normal:
                foreach (TouchClass touch in TouchClasses)
                {
                    float distance;

                    var discreteTapProtectionPassed = false;

                    if (
                        touch.Tapped &&
                        (
                            distance = Vector2.Distance(touch.Touch.screenPosition, hitIteration.HitCoord.Position)
                        ) < hitIteration.HitCoord.Radius &&
                        (
                            discreteTapProtectionPassed =
                                !( // Safeguard to prevent false 'early' taps while the player intends to catch notes

                                        // Status check
                                        touch.DiscreteHitobjectIsInRange &&
                                        touch.NearestDiscreteHitobject != null &&
                                        touch.NearestDiscreteHitobject.Current.Type == HitObject.HitType.Catch &&

                                        // Only suppress if the catch note is EARLIER and likely to be triggered by this
                                        // input
                                        touch.NearestDiscreteHitobject.Time < hitIteration.Time &&
                                        hitIteration.Time >= -Player.GoodWindow &&

                                        // Spatial distance comparison
                                        Vector2.Distance(
                                            touch.Touch.screenPosition,
                                            touch.NearestDiscreteHitobject.HitCoord.Position) <
                                        distance &&
                                        hitIteration.Time - touch.NearestDiscreteHitobject.Time <= Player.GoodWindow * 2
                                    ) || // Exception clause
                                (touch.DiscreteHitobjectIsInRange &&
                                 touch.NearestDiscreteHitobject != null &&
                                 ( // Ways that won't break the player's expectation
                                     Math.Abs(hitobjectTimingDelta) <= Player.PerfectWindow ||
                                     Mathf.Approximately(hitIteration.Time, touch.NearestDiscreteHitobject.Time) ||
                                     Mathf.Approximately(
                                         Vector3.Distance(
                                             hitIteration.HitCoord.Position,
                                             touch.NearestDiscreteHitobject.HitCoord
                                                 .Position),
                                         hitIteration.HitCoord.Radius / 2)
                                 ))
                        ) &&
                        (
                            !touch.QueuedHit ||
                            hitIteration.Time < touch.QueuedHit.Time ||
                            (Mathf.Approximately(hitIteration.Time, touch.QueuedHit.Time) &&
                             distance < touch.QueuedHitDistance)
                        )
                    )
                    {
                        //Debug.Log(
                        //    $"Touch {touch.Touch.finger.index} tapped on hitobject at {hitIteration.Time}. Adding to queue.");

                        touch.QueuedHit = hitIteration;
                        touch.QueuedHitDistance = distance;
                        alreadyHit = true;
                    }
                    else if (!discreteTapProtectionPassed && touch.NearestDiscreteHitobject != null)
                    {
                        //Debug.Log(
                        //    $"Tap suppressed for hitobject at {hitIteration.Time}. \n" +
                        //    $"At touch.NearestDiscreteHitobject.Time: {touch.NearestDiscreteHitobject.Time} < hitIteration.Time: {hitIteration.Time}. \n" +
                        //    $"At touch.NearestDiscreteHitobject.Type: {touch.NearestDiscreteHitobject.Current.Type} \n" +
                        //    $"At touch.NearestDiscreteHitobject.HitCoord.Position: {touch.NearestDiscreteHitobject.HitCoord.Position} < hitIteration.HitCoord.Position: {hitIteration.HitCoord.Position}.\n" +
                        //    $"At Hit Delta {hitobjectTimingDelta} >= -{Player.GoodWindow}. \n" +
                        //    $"At comparison of Discrete-Tap delta {hitIteration.Time - touch.NearestDiscreteHitobject.Time} < {Player.GoodWindow * 2}");
                    }
                }

                return;

            case HitObject.HitType.Catch:
                foreach (TouchClass touch in TouchClasses)
                {
                    float distance = Vector2.Distance(touch.Touch.screenPosition, hitIteration.HitCoord.Position);

                    if (distance < hitIteration.HitCoord.Radius)
                    {
                        bool shouldAssign =
                            !hitIteration.InDiscreteHitQueue && // Slow down on the assigning, due to the nature of catch notes
                            touch.QueuedHit == null; // This touch is already occupied by another hitobject this frame

                        // being able to be spammed at lightspeed

                        // || hitIteration.Time < touch.QueuedHit.Time 
                        // || (
                        // Mathf.Approximately(hitIteration.Time, touch.QueuedHit.Time) &&
                        // distance < touch.DiscreteHitobjectDistance
                        // );

                        if (shouldAssign)
                        {
                            //Debug.Log(
                            //    $"[Catch Note Assign] Touch {touch.Touch.finger.index} queued catch note at {hitIteration.Time} (dist: {distance})");

                            hitIteration.InDiscreteHitQueue = true;
                            touch.DiscreteHitobjectDistance = distance;
                            touch.DiscreteHitobjectIsInRange = true;
                            touch.QueuedHit = hitIteration; // Occupy this touch for the rest of the frame
                            alreadyHit = true;
                        }
                    }
                }

                return;
        }
    }
}