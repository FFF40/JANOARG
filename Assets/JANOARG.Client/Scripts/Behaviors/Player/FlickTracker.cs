using UnityEngine;

// ReSharper disable once CheckNamespace
/// <summary>
///     Detects flick gestures on a single touch and exposes the result as a latched
///     <see cref = "IsFlicked"/> flag. This type is deliberately dumb: it knows nothing about
///     hitobjects, hit radii, timing windows or note matching. Its entire job is to answer
///     "is this finger currently flicking", and the engine decides what that means.
/// </summary>
/// <remarks>
///     <para>
///         Ported from phira's <c>FlickTracker</c> (TeamFlos/phira, <c>prpr/src/judge.rs</c>),
///         which replaced prpr's earlier 10-sample quadratic-regression <c>VelocityTracker</c>.
///         The live behaviour there is a single-sample speed test with a latch that the judge
///         clears on hit — no windowed fit, no dpi term.
///     </para>
///     <para>
///         Two deliberate deviations from the reference, documented at their sites below: we refuse
///         to divide by a non-positive <c>dt</c>, and a consumed flick must be re-armed before it
///         can fire again. The latter is prpr's hysteresis, which phira dropped along with the
///         regression; without it one sustained swipe re-fires on every sample and clears a whole
///         chain of notes for the effort of a single gesture.
///     </para>
/// </remarks>
public class FlickTracker
{
    /// <summary>
    ///     phira's <c>FLICK_SPEED_THRESHOLD</c> (0.8), scaled by the reference DPI ratio it
    ///     hardcodes. Note that phira's <c>FlickTracker::new</c> accepts the device DPI and then
    ///     ignores it in favour of a flat 275 — <c>Screen.dpi</c> is not dependable enough to gate
    ///     gameplay on, and this port keeps that decision. Expressed in screen units per second,
    ///     where the reference screen extent spans 2 units (phira's NDC range of [-1, 1]).
    /// </summary>
    public const float SpeedThreshold = 0.8f * 275f / 386f; // ~0.5699 units/s

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
    ///     <see cref = "SpeedThreshold"/> converted into this device's pixels per second.
    /// </summary>
    /// <remarks>
    ///     phira normalizes x by screen width and y by screen height, which makes its threshold
    ///     anisotropic — the same physical swipe registers differently depending on its direction.
    ///     We normalize both axes by a single reference extent instead, so the gesture reads the
    ///     same in every direction. That also keeps <see cref = "FlickStroke"/> angle-preserving,
    ///     which matters because the caller measures a flick angle off it.
    /// </remarks>
    private readonly float _Threshold;

    private Vector2 _LastPoint;
    private float   _LastTime;
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

    /// <param name = "screenReferenceExtent">
    ///     Screen extent, in pixels, that spans phira's 2-unit normalized range. Both axes are
    ///     normalized by this, so the threshold stays direction-independent.
    /// </param>
    public FlickTracker(float screenReferenceExtent)
    {
        _Threshold = SpeedThreshold * (screenReferenceExtent / 2f);
    }

    /// <summary>
    ///     Builds a tracker referenced against the current screen height.
    /// </summary>
    public static FlickTracker Create() =>
        new(Screen.height);

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
    ///     calls; samples that are not are dropped rather than trusted.
    /// </summary>
    public void Push(float time, Vector2 position)
    {
        if (!_HasSample)
        {
            _LastPoint = position;
            _LastTime = time;
            _HasSample = true;

            return;
        }

        float dt = time - _LastTime;
        Vector2 delta = position - _LastPoint;

        _LastPoint = position;
        _LastTime = time;

        // phira divides by dt unguarded, which is safe there only because it synthesizes strictly
        // increasing timestamps. A repeated or non-monotonic clock yields an infinite or NaN speed
        // that trips the threshold on a stationary finger, so refuse the sample outright.
        if (dt <= 0f) return;

        float speed = delta.magnitude / dt;

        // Re-arm a consumed flick only once the gesture genuinely ends or turns: the finger slows to
        // half the firing speed, or changes direction by more than ~53 degrees. prpr re-arms at two
        // thirds of its firing speed rather than half, so this is a little stricter; the wider band
        // is what stops one sustained swipe clearing a chain of notes without further effort.
        if (_Wait &&
            (speed < _Threshold ||
             Vector2.Dot(_LastDirection, delta.normalized) < ReArmDotTolerance))
            _Wait = false;

        if (_Wait || IsFlicked || speed < _Threshold * FireMultiplier)
            return;

        IsFlicked = true;
        FlickStroke = delta;
    }

    /// <summary>
    ///     Clears the latch once the engine has acted on it.
    /// </summary>
    public void ConsumeFlick()
    {
        // Record which way the consumed flick went before clearing it — the re-arm check needs
        // something to compare a later direction against.
        _LastDirection = FlickStroke.normalized;
        _Wait = true;

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
        _LastTime = 0f;
        _LastPoint = _LastDirection = Vector2.zero;
    }
}
