using UnityEngine;


namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    public partial class PlayerInputEngine
    {
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

    }
}
