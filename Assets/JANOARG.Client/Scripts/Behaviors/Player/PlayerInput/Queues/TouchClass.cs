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

public class TouchClass
{
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
    ///     Where this touch was when it first engaged its <see cref = "NearestDiscreteHitobject"/>,
    ///     used as the origin for that note's flick travel measurement.
    /// </summary>
    /// <remarks>
    ///     A catch-flick has no tap to measure travel from — the finger was already down, having
    ///     landed somewhere unrelated, possibly seconds earlier. This is the equivalent anchor:
    ///     the tap-flick path gets one free from Unity as <c>Touch.startScreenPosition</c>, and
    ///     this is the same idea for a gesture that does not begin with a touch-down.
    /// </remarks>
    public Vector2 DiscreteHitobjectAnchor;

    /// <summary>
    ///     The furthest this touch has been from <see cref = "DiscreteHitobjectAnchor"/> during the
    ///     current engagement — the apex of the gesture so far.
    /// </summary>
    /// <remarks>
    ///     Travel is net displacement, which resists a wiggle accumulating distance without going
    ///     anywhere, but collapses when the finger u-turns to correct a mis-aimed flick. The point
    ///     of maximum displacement is exactly the turning point, so measuring the return leg from
    ///     here recovers that case without any turn detection.
    /// </remarks>
    public Vector2 DiscreteHitobjectPeak;

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
    ///     decision that follows from it stays in <see cref = "PlayerInputEngine"/>.
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
    ///     Timestamp of the most recent touch event already fed to <see cref = "FlickTracker"/>.
    ///     A frame can carry several events per finger; this is what keeps each of them fed exactly
    ///     once and in order.
    /// </summary>
    public double LastFlickSampleTime = double.NegativeInfinity;

    /// <summary>
    ///     Set when the finger lifts. The class is kept until the end of the input frame so the
    ///     release sample can still be judged (a catch-flick can complete on the very motion that
    ///     lifts the finger), then swept.
    /// </summary>
    public bool PendingRemoval;
}

