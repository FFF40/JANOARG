using UnityEngine;


namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    public partial class PlayerInputEngine
    {
        private bool ValidateFlickDirection(float expected, float actual) =>
            Mathf.Abs(Mathf.DeltaAngle(expected, actual)) <= FlickAngleTolerance;
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
        private static bool IsDirectionalFlick(float flickDirection) =>
            float.IsFinite(flickDirection);
        private static float FlickPerpendicular(float flickDirection, Vector2 offset) =>
            Mathf.Abs((Quaternion.Euler(0, 0, flickDirection) * offset).x);
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
