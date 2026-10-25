using System;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.ChartInfo;
using UnityEditor;
using UnityEngine;
using Unity.Profiling;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    public partial class PlayerInputEngine
    {
        static readonly ProfilerMarker sr_TouchInputLoop       = new("UpdateInput: Touch Input Loop");
        static readonly ProfilerMarker sr_HitQueueLoop         = new("UpdateInput: HitQueue Processor Loop");
        static readonly ProfilerMarker sr_HoldQueueBlock       = new("UpdateInput: HoldQueue Processor Block");
        static readonly ProfilerMarker sr_DiscreteHitQueueLoop = new("UpdateInput: DiscreteHitQueue Processor Loop");
        static readonly ProfilerMarker sr_HitobjectProcessor   = new("HitobjectProcessor");
        static readonly ProfilerMarker sr_HoldQueueProcessor   = new("HoldQueue_Processor");
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

                        bool nearAnyFlickable = Enumerable.Any<HitPlayer>(HitQueue, hit =>
                            hit.Current.Flickable &&
                            !hit.IsProcessed &&
                            Math.Abs((double)(hit.Time - Player.CurrentTime)) <=
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
                    if (hitobjectTimingDelta < -Math.Max((float)Player.PassWindow, (float)Player.GoodWindow)) break;
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
                s_deltaTime = 16.666; // Fake 60fps to start
                UpdatePerSecond = 60f;
            }
            else
            {
                s_deltaTime = currentTimeMs - _LastTimeMs;
                UpdatePerSecond = 1000f / (float)s_deltaTime;
                _LastTimeMs = currentTimeMs;
                Delta = s_deltaTime.ToString("F3") + "ms";
            }
        }

    }
}
