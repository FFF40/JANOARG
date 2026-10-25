using System.Collections.Generic;


namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    public partial class PlayerInputEngine
    {
        private static void RemoveAllFromQueue(List<HitPlayer> queue, HitPlayer hit)
        {
            for (var i = queue.Count - 1; i >= 0; i--)
                if (queue[i] == hit)
                    queue.RemoveAt(i);
        }
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

    }
}
