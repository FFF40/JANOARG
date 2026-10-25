using System;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.ChartInfo;
using Unity.Collections;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    partial class PlayerInputEngine : MonoBehaviour
    {
        static readonly ProfilerMarker sr_TouchInputLoop       = new("UpdateInput: Touch Input Loop");
        static readonly ProfilerMarker sr_HitQueueLoop         = new("UpdateInput: HitQueue Processor Loop");
        static readonly ProfilerMarker sr_HoldQueueBlock       = new("UpdateInput: HoldQueue Processor Block");
        static readonly ProfilerMarker sr_DiscreteHitQueueLoop = new("UpdateInput: DiscreteHitQueue Processor Loop");
        static readonly ProfilerMarker sr_HitobjectProcessor   = new("HitobjectProcessor");
        static readonly ProfilerMarker sr_HoldQueueProcessor   = new("HoldQueue_Processor");

        public static  PlayerInputEngine sInstance;
        private static double                 s_deltaTime;
        public         PlayerScreen           Player;

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

        [ReadOnly]        public string              Delta = s_deltaTime.ToString("F3") + "ms";
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

        /// <summary>
        ///     Whether a touch-down arrived for this finger since the last input frame. Used to tell a
        ///     gesture that fitted entirely between two frames (which must be reconstructed and judged)
        ///     from one whose down happened while input was not being tracked (which must be ignored).
        /// </summary>

        /// <summary>
        ///     Feeds every recorded event for this finger into its tracker, in order.
        /// </summary>
        /// <remarks>
        ///     One sample per rendered frame averaged the whole frame's motion into a single speed figure:
        ///     a short, sharp flick was flattened below the fire threshold, and the same gesture read
        ///     differently at 30 fps than at 120. Per-event samples measure the movement at the touch
        ///     panel's own resolution, with the event's real timestamp as <c>dt</c>.
        /// </remarks>

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

        /// <summary>
        ///     Logs a message only once when the PlayerScreen is initialized.
        /// </summary>
        /// <param name = "msg"> The message to log. </param>

        // Allocation-free replacements for List<T>.Find/RemoveAll with lambdas. Those capture locals
        // (fingerIndex, holdNoteEntry), so each call allocated a closure plus a delegate — several per
        // frame while touches are active. These are plain loops over the same fields.


        /// <summary>
        ///     Adds the given <see cref = "HitPlayer"/> object to the player's hit queue, sorted by time.
        /// </summary>
        /// <param name = "hit"> The <see cref = "HitPlayer"/> object to add to the queue. </param>


        /// <summary>
        /// Unconditionally scrubs every reference to <paramref name="hit"/> from every queue/cache
        /// this manager holds. Called right before a HitPlayer is returned to PlayerScreen's pool,
        /// since a pooled instance can be handed out to a brand-new note as soon as the same frame
        /// it's returned — any stale reference left behind would silently start aliasing that new
        /// note (same underlying object, so flag-based "is this finished" checks stop working the
        /// instant it's reused).
        /// </summary>


    


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

        /// <summary>
        ///     Whether a note's flick is directional rather than omnidirectional. The sentinel for
        ///     omnidirectional is <c>NaN</c>, but a malformed chart can also carry an infinity; both are
        ///     treated as omnidirectional here so every judgement site agrees.
        /// </summary>

        /// <summary>
        ///     Perpendicular distance from <paramref name = "offset"/> to the beam through the note along
        ///     <paramref name = "flickDirection"/>.
        /// </summary>
        /// <remarks>
        ///     Rotating the offset by +FlickDirection maps the flick axis onto +Y, so <c>.x</c> is the
        ///     perpendicular component. <c>.y</c> is deliberately unused — the beam runs indefinitely
        ///     both ways, and which way the finger went is the angle check's job.
        /// </remarks>

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

    }
}
