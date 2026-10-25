using System;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;


namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    public partial class PlayerInputEngine
    {
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
}
