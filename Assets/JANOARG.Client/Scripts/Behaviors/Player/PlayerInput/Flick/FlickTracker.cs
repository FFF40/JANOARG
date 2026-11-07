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

public class FlickTracker
{
    /// <summary>
    ///     Base flick speed, as the fraction of a note radius the finger crosses in one perfect
    ///     window. A flick fires at <see cref = "FireMultiplier"/> times this and re-arms below it,
    ///     so the gesture that fires covers about 14% of a note radius per perfect window.
    /// </summary>
    /// <remarks>
    ///     Deliberately diverges from phira, which uses <c>0.8 * 275 / 386</c> — its own empirical
    ///     constant plus two generations of vestigial DPI arithmetic, left behind when the
    ///     regression they belonged to was replaced. Measuring against this project's own note
    ///     scale and timing window instead drops the dependence on <c>Screen.dpi</c>, makes flick
    ///     difficulty follow MinimumRadius and ScreenUnit automatically, and states something
    ///     arguable rather than inherited. Calibrated to reproduce the previous pixel values.
    /// </remarks>
    public const float SpeedThreshold = 0.07f; // radii per perfect window; fires at 0.14

    /// <summary>
    ///     phira fires the flick at twice the base threshold. Kept verbatim.
    /// </summary>
    public const float FireMultiplier = 2f;

    /// <summary>
    ///     How far a gesture must turn before a consumed flick re-arms, as a dot product between the
    ///     consumed direction and the current one. prpr writes the same test as
    ///     <c>|dot - 1| &gt; 0.4</c>, which for unit vectors is exactly this — about 53 degrees.
    /// </summary>
    public const float ReArmDotTolerance = 0.6f;

    /// <summary>
    ///     Speed below which a consumed flick re-arms, as a fraction of the firing speed. prpr
    ///     writes the same test as <c>threshold * (1.2 / 1.8)</c> against a threshold that is
    ///     itself its firing speed, so this is that ratio kept verbatim.
    /// </summary>
    public const float ReArmSpeedRatio = 1.2f / 1.8f; // ~0.667 of firing speed

    /// <summary>
    ///     <see cref = "SpeedThreshold"/> converted into this device's pixels per second.
    /// </summary>
    /// <remarks>
    ///     Isotropic: both axes share one scale, so a gesture reads the same in every direction and
    ///     <see cref = "FlickStroke"/> stays angle-preserving, which matters because the caller
    ///     measures a flick angle off it. (phira normalizes x by width and y by height, so the same
    ///     physical swipe registers differently depending on which way it points.)
    /// </remarks>
    private readonly float _Threshold;

    private Vector2 _LastPoint;
    private double  _LastTime;
    private bool    _HasSample;

    /// <summary>
    ///     Set when a flick is consumed; blocks further firing until the gesture ends or turns.
    ///     This is prpr's <c>wait</c>.
    /// </summary>
    private bool _Wait;

    /// <summary>
    ///     Direction of the flick that was consumed, to compare a later one against.
    /// </summary>
    private Vector2 _LastDirection;

    /// <summary>
    ///     True once this touch has flicked, and stays true until the engine calls
    ///     <see cref = "ConsumeFlick"/>. Latching is intentional: a flick that lands between two
    ///     engine passes must not be lost, and only the engine knows what consuming it means.
    /// </summary>
    public bool IsFlicked { get; private set; }

    /// <summary>
    ///     True on the one <see cref = "Push"/> where an already-latched flick was refreshed to a
    ///     genuinely different direction (a correction), false otherwise. The caller uses it to keep
    ///     the engine's expiry timer from reaping a flick the player just re-aimed.
    /// </summary>
    public bool ReLatched { get; private set; }

    /// <summary>
    ///     The screen-space movement that tripped <see cref = "IsFlicked"/>, in pixels.
    ///     Meaningless while <see cref = "IsFlicked"/> is false.
    /// </summary>
    /// <remarks>
    ///     Provided so the caller can gate a directional flick on its angle. This tracker does not
    ///     interpret it — see <see cref = "AngleOf"/> for the chart-space conversion.
    /// </remarks>
    public Vector2 FlickStroke { get; private set; }

    /// <summary>
    ///     Angle of <see cref = "FlickStroke"/> in chart convention. Only meaningful while
    ///     <see cref = "IsFlicked"/> is true.
    /// </summary>
    public float FlickAngle =>
        AngleOf(FlickStroke);

    /// <param name = "noteScale"> Note radius in pixels; the spatial scale a flick is measured against. </param>
    /// <param name = "timingWindow"> Judgement window in seconds; the temporal scale it is measured against. </param>
    public FlickTracker(float noteScale, float timingWindow)
    {
        _Threshold = SpeedThreshold * (noteScale / timingWindow);
    }

    /// <summary>
    ///     Builds a tracker scaled to the player's own note size and timing, so flick difficulty
    ///     tracks the game's tuning rather than a raw screen measurement.
    /// </summary>
    public static FlickTracker Create()
    {
        PlayerScreen player = PlayerScreen.sMain;

        // Fallbacks cover construction before PlayerScreen has finished loading. 0.2 * height is
        // what ScaledMinimumRadius works out to on any screen wider than ScreenUnit's 1.75:1.
        float noteScale = player && player.ScaledMinimumRadius > 0f
            ? player.ScaledMinimumRadius
            : 0.2f * Screen.height;

        float timingWindow = player && player.PerfectWindow > 0f
            ? player.PerfectWindow
            : 0.05f;

        return new FlickTracker(noteScale, timingWindow);
    }

    /// <summary>
    ///     Converts a screen-space delta into the chart's flick angle convention:
    ///     0 degrees is straight up (+Y), increasing clockwise, matching
    ///     <c>HitObject.FlickDirection</c> as consumed by the note renderer and the
    ///     directional corridor check.
    /// </summary>
    /// <remarks>
    ///     Taking <c>Atan2(x, y)</c> rather than the usual <c>Atan2(y, x)</c> is precisely the
    ///     rotation of the zero axis from +X to +Y, and it yields a clockwise-positive result with
    ///     no negation. Returns 0 for a zero-length delta rather than an undefined angle.
    /// </remarks>
    public static float AngleOf(Vector2 delta) =>
        delta.sqrMagnitude > 0f
            ? Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg
            : 0f;

    /// <summary>
    ///     Feeds one position sample. <paramref name = "time"/> must be strictly increasing across
    ///     calls; samples that are not are dropped rather than trusted. Callers pass the touch event's
    ///     own timestamp rather than the frame time, so one frame may contribute several samples.
    /// </summary>
    public void Push(double time, Vector2 position)
    {
        ReLatched = false;

        if (!_HasSample)
        {
            _LastPoint = position;
            _LastTime = time;
            _HasSample = true;

            return;
        }

        double dt = time - _LastTime;
        Vector2 delta = position - _LastPoint;

        _LastPoint = position;
        _LastTime = time;

        // phira divides by dt unguarded, which is safe there only because it synthesizes strictly
        // increasing timestamps. A repeated or non-monotonic clock yields an infinite or NaN speed
        // that trips the threshold on a stationary finger, so refuse the sample outright.
        if (dt <= 0d) return;

        float speed = (float)(delta.magnitude / dt);
        Vector2 direction = delta.normalized;

        // Re-arm a consumed flick only once the gesture genuinely ends or turns: the finger slows
        // to two thirds of the firing speed, or changes direction by more than ~53 degrees. Both
        // figures are prpr's. An earlier revision re-armed at half the firing speed instead, which
        // demanded a deeper slowdown than the reference and made same-direction flick runs harder
        // than they needed to be.
        if (_Wait &&
            (speed < _Threshold * FireMultiplier * ReArmSpeedRatio ||
             Vector2.Dot(_LastDirection, direction) < ReArmDotTolerance))
            _Wait = false;

        // A latched flick still updates if the gesture genuinely turns. Without this the stroke is
        // frozen at whatever fired it, so a player who flicks the wrong way and corrects without
        // lifting keeps being judged on the original direction until the engine's invalidator
        // clears the latch a perfect window later. Same turn test the re-arm uses.
        bool turned = IsFlicked &&
                      Vector2.Dot(FlickStroke.normalized, direction) < ReArmDotTolerance;

        if (_Wait || (IsFlicked && !turned) || speed < _Threshold * FireMultiplier)
            return;

        ReLatched = IsFlicked;
        IsFlicked = true;
        FlickStroke = delta;
    }

    /// <summary>
    ///     Clears the latch once the engine has acted on it.
    /// </summary>
    /// <remarks>
    ///     For a flick that actually landed on a note. A flick that expired unused must go through
    ///     <see cref = "ClearFlick"/> instead — prpr calls <c>consume_flick</c> only inside the
    ///     branch where a note is hit, and nothing there arms the wait on expiry.
    /// </remarks>
    public void ConsumeFlick()
    {
        // Record which way the consumed flick went before clearing it — the re-arm check needs
        // something to compare a later direction against.
        _LastDirection = FlickStroke.normalized;
        _Wait = true;

        ClearFlick();
    }

    /// <summary>
    ///     Drops a latched flick without arming the re-arm wait, for one that was never acted on.
    /// </summary>
    /// <remarks>
    ///     The latch still has to go, or the engine's own flag is re-set from it on the next frame
    ///     and a stale flick resurrects itself. But penalising a flick that missed with the same
    ///     re-arm requirement as one that landed blocks the retry at precisely the moment the
    ///     player is trying again. Chaining stays blocked regardless, since that is gated by the
    ///     hit path rather than this one.
    /// </remarks>
    public void ClearFlick()
    {
        IsFlicked = false;
        FlickStroke = Vector2.zero;
    }

    /// <summary>
    ///     Drops all accumulated state. Call when the finger lifts.
    /// </summary>
    public void Reset()
    {
        IsFlicked = false;
        FlickStroke = Vector2.zero;
        _HasSample = false;
        _Wait = false;
        _LastTime = 0d;
        _LastPoint = _LastDirection = Vector2.zero;
    }
}

