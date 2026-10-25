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




public class PlayerInputEngine : MonoBehaviour
{
    static readonly ProfilerMarker sr_TouchInputLoop = new("UpdateInput: Touch Input Loop");
    static readonly ProfilerMarker sr_HitQueueLoop = new("UpdateInput: HitQueue Processor Loop");
    static readonly ProfilerMarker sr_HoldQueueBlock = new("UpdateInput: HoldQueue Processor Block");
    static readonly ProfilerMarker sr_DiscreteHitQueueLoop = new("UpdateInput: DiscreteHitQueue Processor Loop");
    static readonly ProfilerMarker sr_HitobjectProcessor = new("HitobjectProcessor");
    static readonly ProfilerMarker sr_HoldQueueProcessor = new("HoldQueue_Processor");

    public static  PlayerInputEngine sInstance;
    private static double             s_DeltaTime;
    public         PlayerScreen       Player;

    [Space] public bool Autoplay;

    /// <summary>
    ///     How far a flick's containment circle grows once the finger is established on the note,
    ///     as a multiple of the note's hit radius.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Modelled on osu!'s slider follow circle: the region expands once you are inside it,
    ///         so it is hard to leave by accident. On the tap-flick path it also resolves a real
    ///         conflict — omnidirectional containment is a circle measured at the finger's current
    ///         position while the flick must travel at least flickDistanceThreshold, so without
    ///         expansion the two requirements pull against each other and a tap placed dead centre
    ///         is the hardest case rather than the easiest.
    ///     </para>
    ///     <para>
    ///         Directional tap-flicks need none of it: their containment is a beam, unbounded along
    ///         the flick axis, so travelling never threatens it.
    ///     </para>
    /// </remarks>
    [Space] public float FlickFollowScale = 2.4f;

    /// <summary>
    ///     How far a finger must travel for the motion to count as a flick, as a fraction of a note
    ///     radius. The speed half of detection lives in <see cref = "FlickTracker"/>; both must pass.
    /// </summary>
    /// <remarks>
    ///     Was <c>Screen.dpi * 0.2f</c> (about 5mm). Screen.dpi reports 0 on some devices, and the
    ///     fallback of 100 collapsed this gate to 20px — effectively no gate at all. Measuring
    ///     against the note instead removes the last dependence on it.
    /// </remarks>
    public float FlickTravelRatio = 0.375f;

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
    ///     Touch events recorded since the last input frame. The touch panel reports several events per
    ///     finger between two rendered frames, each with its own timestamp; feeding them all ties flick
    ///     detection to the panel's report rate instead of the render frame rate.
    /// </summary>
    private readonly List<TouchSample> _PendingSamples = new();

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
        _PendingSamples.Clear();
    }

    private bool
        _InitLog = true; // log only once on PlayerScreen initialization (we don't want this to spam every millisecond,

    // no?)

    private double _LastTimeMs = -1;

    public void Awake() // Unity's version of constructor
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += RecordSample;
        Touch.onFingerMove += RecordSample;
        Touch.onFingerUp   += RecordSample;
        sInstance = this;
    }


    public void OnDestroy() // Deconstructor
    {
        Touch.onFingerDown -= RecordSample;
        Touch.onFingerMove -= RecordSample;
        Touch.onFingerUp   -= RecordSample;
        EnhancedTouchSupport.Disable();
    }

    /// <summary>
    ///     Records one touch event for the next input frame. Multiple events can arrive per finger
    ///     between two frames, so each is buffered with its own timestamp rather than collapsed into
    ///     whichever position happened to be current when the frame ran.
    /// </summary>
    private void RecordSample(Finger finger)
    {
        // Input is only judged while a chart is running. Dropping events outside it also keeps the
        // buffer from accumulating a pause's worth of stale samples.
        if (!PlayerScreen.sMain || !PlayerScreen.sMain.IsPlaying) return;

        Touch touch = finger.lastTouch;

        if (!touch.valid) return;

        _PendingSamples.Add(new TouchSample
        {
            FingerIndex = finger.index,
            Time        = touch.time,
            Position    = touch.screenPosition,
            Phase       = touch.phase,
        });
    }

    /// <summary>
    ///     Whether a touch-down arrived for this finger since the last input frame. Used to tell a
    ///     gesture that fitted entirely between two frames (which must be reconstructed and judged)
    ///     from one whose down happened while input was not being tracked (which must be ignored).
    /// </summary>
    private bool HasPendingDown(int fingerIndex)
    {
        for (var i = 0; i < _PendingSamples.Count; i++)
            if (_PendingSamples[i].FingerIndex == fingerIndex &&
                _PendingSamples[i].Phase == TouchPhase.Began)
                return true;

        return false;
    }

    /// <summary>
    ///     Feeds every recorded event for this finger into its tracker, in order.
    /// </summary>
    /// <remarks>
    ///     One sample per rendered frame averaged the whole frame's motion into a single speed figure:
    ///     a short, sharp flick was flattened below the fire threshold, and the same gesture read
    ///     differently at 30 fps than at 120. Per-event samples measure the movement at the touch
    ///     panel's own resolution, with the event's real timestamp as <c>dt</c>.
    /// </remarks>
    private void DrainFlickSamples(TouchClass touch, int fingerIndex)
    {
        for (var i = 0; i < _PendingSamples.Count; i++)
        {
            TouchSample sample = _PendingSamples[i];

            if (sample.FingerIndex != fingerIndex || sample.Time <= touch.LastFlickSampleTime)
                continue;

            touch.LastFlickSampleTime = sample.Time;
            touch.FlickTracker.Push(sample.Time, sample.Position);

            // Set the first latch, and refresh it on a re-aim: otherwise the invalidator can reap a
            // flick the player has just corrected back onto the note.
            if (touch.FlickTracker.IsFlicked &&
                (!touch.Flicked || touch.FlickTracker.ReLatched))
            {
                touch.Flicked = true;
                touch.FlickTime = Player.CurrentTime;
            }
        }
    }

    /// <summary>
    ///     How far the player's flick may deviate from a note's FlickDirection, in degrees.
    /// </summary>
    private const float FlickAngleTolerance = 30f;

    /// <summary>
    ///     Whether the angle between a note's expected flick direction and the player's actual one
    ///     is inside <see cref = "FlickAngleTolerance"/>.
    /// </summary>
    /// <param name = "expected"> The expected flick direction from the hit object, in degrees. </param>
    /// <param name = "actual"> The actual flick direction done by the player, in degrees. </param>
    /// <returns> true if within tolerance, false otherwise. </returns>
    private bool ValidateFlickDirection(float expected, float actual) =>
        Mathf.Abs(Mathf.DeltaAngle(expected, actual)) <= FlickAngleTolerance;

    /// <summary>
    ///     Logs a message only once when the PlayerScreen is initialized.
    /// </summary>
    /// <param name = "msg"> The message to log. </param>
    private void InitLogger(string msg)
    {
        if (_InitLog) Debug.Log(msg);
    }

    // Allocation-free replacements for List<T>.Find/RemoveAll with lambdas. Those capture locals
    // (fingerIndex, holdNoteEntry), so each call allocated a closure plus a delegate — several per
    // frame while touches are active. These are plain loops over the same fields.
    private TouchClass FindTouchByFinger(int fingerIndex)
    {
        for (var i = 0; i < TouchClasses.Count; i++)
            if (TouchClasses[i].Touch.finger.index == fingerIndex)
                return TouchClasses[i];

        return null;
    }

    private static void RemoveAllFromQueue(List<HitPlayer> queue, HitPlayer hit)
    {
        for (var i = queue.Count - 1; i >= 0; i--)
            if (queue[i] == hit)
                queue.RemoveAt(i);
    }

    /// <summary>
    ///     Adds the given <see cref = "HitPlayer"/> object to the player's hit queue, sorted by time.
    /// </summary>
    /// <param name = "hit"> The <see cref = "HitPlayer"/> object to add to the queue. </param>
    public void AddToQueue(HitPlayer hit)
    {
        // Autoplay scores every note at its own chart time, so from the draw pass's point of
        // view every autoplay note is pending judgement from the moment it is queued. Input
        // sets the flag later, when a note is actually claimed (see the DiscreteHitQueue pass).
        if (Autoplay)
            hit.IsPendingJudgement = true;

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
        RemoveAllFromQueue(HitQueue, hit);
        RemoveAllFromQueue(DiscreteHitQueue, hit);

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
            touch.QueuedHit.IsPendingJudgement = true;

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
            // A held hold is finalised at its tail (EndTime). Flag it so the draw pass hides the
            // head and tail at the line instead of letting the tail scroll past it.
            if (!missed)
                hitObject.IsPendingJudgement = true;

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

            // Distance half of flick detection: how far a finger must travel before the motion
            // counts as a flick at all. The speed half lives in FlickTracker; both must pass.
            float flickDistanceThreshold = FlickTravelRatio * Player.ScaledMinimumRadius;
            // Guard the interpolated argument on _InitLog: it would otherwise be built (and
            // allocated) every frame even though InitLogger drops it after the first call.
            if (_InitLog)
                InitLogger(
                    $"Set flick distance threshold to {flickDistanceThreshold}px " +
                    $"(note scale: {Player.ScaledMinimumRadius}px)");

            // Main touch iterator
            sr_TouchInputLoop.Begin();
            for (var a = 0; a < inputCount; a++)
            {
                Touch inputEntry = Touch.activeTouches[a];
                int fingerIndex = inputEntry.finger.index;

                // Handle touch end/cancel
                if (inputEntry is { isInProgress: false, phase: TouchPhase.Ended or TouchPhase.Canceled })
                {
                    TouchClass endingTouch = FindTouchByFinger(fingerIndex);

                    if (endingTouch == null && HasPendingDown(fingerIndex))
                    {
                        // The whole gesture fitted between two frames, so no class was ever made for
                        // it. The buffered samples still hold it, and the ended Touch carries its
                        // start position/time, so reconstruct one and let it be judged like any other
                        // tap. StartTime is wound back by the gesture's own duration so a sub-frame
                        // tap is not recorded as happening at the lift frame.
                        endingTouch = new TouchClass
                        {
                            Touch     = inputEntry,
                            StartTime = Player.CurrentTime - (inputEntry.time - inputEntry.startTime),
                            Initial   = true,
                        };

                        TouchClasses.Add(endingTouch);
                    }

                    if (endingTouch != null)
                    {
                        // Refresh with the lift state and feed the release event (and anything since
                        // the last frame) before judging. The old code resolved tap-flicks against
                        // the previous frame's position and reset the tracker without ever seeing
                        // the lift, so the final stroke of the gesture could not count.
                        endingTouch.Touch = inputEntry;
                        DrainFlickSamples(endingTouch, fingerIndex);

                        // A tap-flick is often completed by the same motion that lifts the finger, so
                        // give a pending claim the same last chance the tap path gets.
                        if (endingTouch.QueuedHit != null &&
                            endingTouch.QueuedHit.Current.Flickable &&
                            endingTouch.QueuedHit.Current.Type == HitObject.HitType.Normal)
                            TryResolveTapFlick(endingTouch, endingTouch.QueuedHit, flickDistanceThreshold);

                        if (endingTouch.QueuedHit != null &&
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

                        // Keep the class through this frame's judgement passes — a catch-flick that
                        // completes on the lift (or entirely within one frame) is only visible while
                        // the touch still exists. The sweep at the end of UpdateInput resets and
                        // removes it.
                        endingTouch.PendingRemoval = true;
                    }

                    continue;
                }

                // Find existing touch or create new one
                TouchClass touchClass = FindTouchByFinger(fingerIndex);

                if (touchClass == null) // New touch
                {
                    touchClass = new TouchClass
                    {
                        Touch = inputEntry,
                        StartTime = Player.CurrentTime,
                        Initial = true
                    };

                    TouchClasses.Add(touchClass);
                }
                else // Existing touch
                {
                    touchClass.Touch = inputEntry;

                    // Already handling the same touch on the second pass, consider it holding
                    touchClass.IsHolding = true;
                }

                // Flick detector — FlickTracker owns gesture recognition entirely; this only relays
                // its verdict. Every event recorded since the last frame is fed, with the event's own
                // timestamp as dt (not Player.CurrentTime, whose audio-callback granularity can repeat
                // across events, nor the frame time, which coarse-grained the speed and made the same
                // gesture read differently at different frame rates). For a brand-new touch the buffer
                // necessarily carries the touch-down sample itself.
                DrainFlickSamples(touchClass, fingerIndex);

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
                        touchClass.FlickTracker.ClearFlick(); // expired, not spent — see ClearFlick
                    }
                }

                touchClass.Initial = false;
            }
            sr_TouchInputLoop.End();

            double judgementOffsetTime = Player.CurrentTime + Player.Settings.JudgmentOffset; // Judgement offset

            if (_InitLog)
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

                // A tap-flick is claimed by a tap and resolved by a flick on a later frame, so the
                // note has to stay interactable long enough for both. The tap gets GoodWindow to
                // land in; the flick it is actually judged on still has to fall inside PassWindow
                // (see TryResolveTapFlick). Catch-flicks keep PassWindow — their single gesture is
                // retryable across the whole window already, so they need no extra room.
                bool isTapFlick =
                    hitIteration.Current.Flickable &&
                    hitIteration.Current.Type == HitObject.HitType.Normal;

                float window = isDiscreteHitObject && !isTapFlick // Timing window per hitobject type
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
                            // Edge only. This block re-runs every frame the finger is in range, so
                            // assigning unconditionally would reset the anchor each frame and leave
                            // the travel gate measuring a single frame of movement. The first term
                            // catches entering range at all; the second covers an early flick, whose
                            // note is held in DiscreteHitQueue past the frame that nulls QueuedHit,
                            // so the clear at resolution never fires and the flag stays set.
                            if (!touch.DiscreteHitobjectIsInRange ||
                                touch.NearestDiscreteHitobject != hitIteration)
                                touch.DiscreteHitobjectAnchor =
                                    touch.DiscreteHitobjectPeak = touch.Touch.screenPosition;
                            else if (Vector2.Distance(touch.Touch.screenPosition, touch.DiscreteHitobjectAnchor) >
                                     Vector2.Distance(touch.DiscreteHitobjectPeak, touch.DiscreteHitobjectAnchor))
                                touch.DiscreteHitobjectPeak = touch.Touch.screenPosition;

                            touch.DiscreteHitobjectIsInRange = true;
                            touch.NearestDiscreteHitobject = hitIteration;
                        }

                    // Pass to DiscreteHitQueue
                    if (hitIteration.InDiscreteHitQueue ||
                        (alreadyHit && hitIteration.Current.Type == HitObject.HitType.Catch))
                    {
                        DiscreteHitQueue.Add(hitIteration);
                        hitIteration.InDiscreteHitQueue = false;
                        hitIteration.IsPendingJudgement = true;

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

                // Flickables are released a perfect window early, so feedback lands at
                // max(flick time, note time - PerfectWindow): a well-timed flick fires on the frame
                // it happens and still feels immediate, while an early one waits instead of
                // replying before the note reaches the line. Catch notes keep resolving at their
                // own time — they are caught as the note passes, not flicked at.
                double releaseTime = hitObject.Current.Flickable
                    ? hitObject.Time - Player.PerfectWindow
                    : hitObject.Time;

                if (judgementOffsetTime >= releaseTime &&
                    (hitObject.Current.Type == HitObject.HitType.Catch || hitObject.Current.Flickable))
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

            // Touches whose finger lifted this frame stayed around for the judgement passes above so
            // a flick completed on the lift could resolve. Drop them now, once this frame is done.
            for (var i = TouchClasses.Count - 1; i >= 0; i--)
            {
                if (!TouchClasses[i].PendingRemoval) continue;

                TouchClasses[i].FlickTracker.Reset();
                TouchClasses.RemoveAt(i);
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
                        // PlayerInputEngine.PurgeHitPlayer) — an explicit RemoveAt(i) here would
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

        // Every touch that could be matched this frame has now been drained; anything left (a finger
        // that vanished without a class, or autoplay's) is stale and must not leak into next frame.
        _PendingSamples.Clear();

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
            // PlayerInputEngine.PurgeHitPlayer) — an explicit RemoveAt(queuePtr) here would
            // now delete whatever shifted into that index instead, dropping an unrelated hold note.
            Player.RemoveHitPlayer(holdNoteEntry.HitObject);
            queuePtr--; // Pointer rollback
        }
    }

    /// <summary>
    ///     How far this touch has travelled for the purpose of qualifying a flick on
    ///     <paramref name = "note"/>.
    /// </summary>
    /// <remarks>
    ///     The larger of the displacement from where the finger engaged the note and the
    ///     displacement from the gesture's apex. The first is the normal case; the second recovers
    ///     a u-turn, where returning toward the anchor shrinks net displacement even though the
    ///     finger is moving decisively. Taking the larger means a correcting stroke is measured
    ///     from the turning point, while a wiggle still fails — its apex never gets far enough for
    ///     either term to clear the threshold.
    /// </remarks>
    private static float FlickTravel(TouchClass touch, HitPlayer note)
    {
        Vector2 current = touch.Touch.screenPosition;

        // A touch that never engaged the note has no anchor of its own; where it landed is all
        // there is to measure from.
        if (touch.NearestDiscreteHitobject != note)
            return Vector2.Distance(current, touch.Touch.startScreenPosition);

        return Mathf.Max(
            Vector2.Distance(current, touch.DiscreteHitobjectAnchor),
            Vector2.Distance(current, touch.DiscreteHitobjectPeak));
    }

    /// <summary>
    ///     Whether a note's flick is directional rather than omnidirectional. The sentinel for
    ///     omnidirectional is <c>NaN</c>, but a malformed chart can also carry an infinity; both are
    ///     treated as omnidirectional here so every judgement site agrees.
    /// </summary>
    private static bool IsDirectionalFlick(float flickDirection) =>
        float.IsFinite(flickDirection);

    /// <summary>
    ///     Perpendicular distance from <paramref name = "offset"/> to the beam through the note along
    ///     <paramref name = "flickDirection"/>.
    /// </summary>
    /// <remarks>
    ///     Rotating the offset by +FlickDirection maps the flick axis onto +Y, so <c>.x</c> is the
    ///     perpendicular component. <c>.y</c> is deliberately unused — the beam runs indefinitely
    ///     both ways, and which way the finger went is the angle check's job.
    /// </remarks>
    private static float FlickPerpendicular(float flickDirection, Vector2 offset) =>
        Mathf.Abs((Quaternion.Euler(0, 0, flickDirection) * offset).x);

    /// <summary>
    ///     The displacement vector a flick on <paramref name = "note"/> should be judged on.
    /// </summary>
    /// <remarks>
    ///     Taken over the same long baseline as <see cref = "FlickTravel"/> — where the finger engaged
    ///     the note (or where it landed, for a touch that never engaged) to where it is now — rather
    ///     than the single inter-sample delta that tripped the tracker. That delta is only a few
    ///     pixels once samples are per event, so its angle is dominated by touch jitter and by
    ///     whichever way the finger first flinched; the full stroke is what the player meant.
    /// </remarks>
    private static Vector2 FlickStrokeVector(TouchClass touch, HitPlayer note)
    {
        Vector2 current = touch.Touch.screenPosition;

        // A touch that never engaged the note has no anchor of its own; where it landed is all
        // there is to measure from.
        if (touch.NearestDiscreteHitobject != note)
            return current - touch.Touch.startScreenPosition;

        // Match FlickTravel's own choice of leg: the larger of the net displacement from the anchor
        // and the return from the apex, so a u-turn is judged on the leg that actually moved.
        Vector2 fromAnchor = current - touch.DiscreteHitobjectAnchor;
        Vector2 fromPeak   = current - touch.DiscreteHitobjectPeak;

        return fromAnchor.sqrMagnitude >= fromPeak.sqrMagnitude ? fromAnchor : fromPeak;
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

        // The tap only claims the note; the flick is the rhythmic action and is what gets judged,
        // so it has to land inside PassWindow. Outside it the claim is left standing rather than
        // consumed, so an early or late flick costs nothing and can simply be repeated until the
        // claim expires at GoodWindow.
        double flickTimingDelta = Player.CurrentTime + Player.Settings.JudgmentOffset - note.Time;

        if (Math.Abs(flickTimingDelta) > Player.PassWindow) return false;

        Vector2 current = touch.Touch.screenPosition;

        if (FlickTravel(touch, note) < flickDistanceThreshold)
            return false;

        Vector2 offset = current - note.HitCoord.Position;
        float radius = note.HitCoord.Radius;

        if (IsDirectionalFlick(note.Current.FlickDirection)) // Directional
        {
            // The beam runs indefinitely both ways, so containment only asks whether the finger is
            // still near the flick axis; which way it went is the angle check's job.
            if (FlickPerpendicular(note.Current.FlickDirection, offset) >= radius) return false;

            if (!ValidateFlickDirection(
                    note.Current.FlickDirection,
                    FlickTracker.AngleOf(FlickStrokeVector(touch, note))))
                return false;
        }
        else if (offset.magnitude >= radius * FlickFollowScale) // Omnidirectional: expanded circle
        {
            return false;
        }

        // Committed, not judged. The DiscreteHitQueue pass scores it once the note is within a
        // perfect window of its own time, and that pass runs later in this same frame — so a flick
        // landing inside the window resolves immediately, while an earlier one is held rather than
        // replying before the note reaches the line. Same path catch-flicks already take.
        note.InDiscreteHitQueue = true;

        touch.Flicked = false;
        touch.FlickTracker.ConsumeFlick();
        touch.QueuedHit = null; // claim discharged

        return true;
    }

    private void HitobjectProcessor(HitPlayer hitIteration, float flickDistanceThreshold, double hitobjectTimingDelta,
        ref bool alreadyHit)
    {
        if (hitIteration.Current.Flickable) // Flick notes (catch/tap)
        {
            // Tap-flicks are a two-stage gesture: the tap claims the note into the finger's
            // QueuedHit slot, and a flick resolves that claim. With per-event samples the flick can
            // already be latched on the same frame the tap is made, so stage 2 is attempted right
            // after a fresh claim below as well as on later frames — otherwise a flick that began and
            // ended inside one frame would be claimed and then swept unjudged.
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

                    // A flick that is already latched (the whole gesture fitted in this one frame)
                    // can be resolved now rather than waiting for a frame that will never come.
                    if (TryResolveTapFlick(touch, hitIteration, flickDistanceThreshold))
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

                // Per feedback request, catch flicks are queued rather than hit on the spot
                hitIteration.InDiscreteHitQueue = true;

                touch.Flicked = false;
                touch.FlickTracker.ConsumeFlick();

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
                // Containment, and unconditionally. This was gated on !touch.Flicked while a hit
                // required touch.Flicked, so the two were mutually exclusive and the check could
                // never affect the outcome — a flick anywhere on screen cleared the note. Either
                // end of the finger's travel counts, so a note swept through mid-flick still
                // registers.
                Vector2 center = hitObject.HitCoord.Position;
                float   radius = hitObject.HitCoord.Radius;

                float startDistance =
                    Vector2.Distance(touch.Touch.startScreenPosition, center);

                float currentDistance =
                    Vector2.Distance(touch.Touch.screenPosition, center);

                distance = Mathf.Min(startDistance, currentDistance);

                if (IsDirectionalFlick(hitObject.Current.FlickDirection))
                {
                    // Directional containment is the same beam the tap path uses, not a circle:
                    // either end of the stroke may sit on the note, but the finger has to stay near
                    // the flick axis.
                    float perpendicularStart = FlickPerpendicular(
                        hitObject.Current.FlickDirection, touch.Touch.startScreenPosition - center);

                    float perpendicularCurrent = FlickPerpendicular(
                        hitObject.Current.FlickDirection, touch.Touch.screenPosition - center);

                    if (perpendicularStart >= radius && perpendicularCurrent >= radius)
                        return false;
                }
                else
                {
                    // Follow expansion. Once the discrete-hitobject bookkeeping already believes this
                    // finger belongs to this note, widen the circle so the flick's own travel cannot
                    // shake it off. That bookkeeping is set after HitobjectProcessor runs, so a note
                    // can only expand from the second frame a finger is on it — in range first, then
                    // it grows, which is the order we want anyway.
                    float containment = radius;

                    if (touch.DiscreteHitobjectIsInRange && touch.NearestDiscreteHitobject == hitObject)
                        containment *= FlickFollowScale;

                    if (startDistance > containment && currentDistance > containment)
                        return false;
                }

                // With no tap frame to anchor to, the gesture is the entire confirmation.
                if (!touch.Flicked) return false;

                // Travel — the gate the tap path has always had and this one never did. Gated on
                // speed alone, a catch-flick only ever implied fireSpeed * dt of movement: about
                // 10px at 60fps, 2.5px at 240fps. A short sharp jab cleared the note, and it got
                // cheaper the faster the device rendered. Measured from where the finger engaged
                // this note, so each note in a stream demands its own movement rather than
                // inheriting a sweep's accumulated distance.
                if (FlickTravel(touch, hitObject) < flickDistanceThreshold)
                    return false;

                // Judge direction from the whole stroke (see FlickStrokeVector), not from the frozen
                // inter-sample delta that fired the tracker — that is a few pixels and its angle is
                // mostly touch jitter.
                if (IsDirectionalFlick(hitObject.Current.FlickDirection))
                    return ValidateFlickDirection(
                        hitObject.Current.FlickDirection,
                        FlickTracker.AngleOf(FlickStrokeVector(touch, hitObject)));

                return true;
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

                    // A discrete (catch) note under the finger blocks a nearby normal from being
                    // triggered prematurely — the touch is there for the catch, not this note. This is
                    // intentionally unconditional with respect to order, proximity and timing windows:
                    // if any catch owns the touch, normals wait their turn.
                    bool onCatch =
                        touch.DiscreteHitobjectIsInRange &&
                        touch.NearestDiscreteHitobject != null &&
                        touch.NearestDiscreteHitobject.Current.Type == HitObject.HitType.Catch;

                    // The one escape is a tap that is genuinely on the normal — flawless, or coincident
                    // with the catch, or one the original hitbox heuristic recognises. That is not a
                    // premature trigger, so it is allowed even while a catch owns the touch.
                    bool legitimateHit =
                        onCatch &&
                        (Math.Abs(hitobjectTimingDelta) <= Player.PerfectWindow ||
                         Mathf.Approximately(hitIteration.Time, touch.NearestDiscreteHitobject.Time) ||
                         Mathf.Approximately(
                             Vector3.Distance(
                                 hitIteration.HitCoord.Position,
                                 touch.NearestDiscreteHitobject.HitCoord.Position),
                             hitIteration.HitCoord.Radius / 2));

                    bool discreteTapProtectionPassed = !onCatch || legitimateHit;

                    if (
                        touch.Tapped &&
                        (
                            distance = Vector2.Distance(touch.Touch.screenPosition, hitIteration.HitCoord.Position)
                        ) < hitIteration.HitCoord.Radius &&
                        discreteTapProtectionPassed &&
                        (
                            !touch.QueuedHit ||
                            hitIteration.Time < touch.QueuedHit.Time ||
                            (Mathf.Approximately(hitIteration.Time, touch.QueuedHit.Time) &&
                             distance < touch.QueuedHitDistance)
                        )
                    )
                    {
                        touch.QueuedHit = hitIteration;
                        touch.QueuedHitDistance = distance;
                        alreadyHit = true;
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
                            touch.QueuedHitDistance = distance; // Keep the tie-break distance in sync with the occupancy
                            alreadyHit = true;
                        }
                    }
                }

                return;
        }
    }
}