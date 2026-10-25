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




        ///     How far the player's flick may deviate from a note's FlickDirection, in degrees.
        /// </summary>



        // (fingerIndex, holdNoteEntry), so each call allocated a closure plus a delegate — several per
        // frame while touches are active. These are plain loops over the same fields.









    }
}
