using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;


namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    public partial class PlayerInputEngine
    {
        private void HoldQueue_Processor(HoldNoteClass holdNoteEntry, ref int queuePtr, float beat, double judgementOffsetTime)
        {
            if (!holdNoteEntry.HitObject || holdNoteEntry.HitObject.IsReturned)
            {
                holdNoteEntry.ReturnDedicatedEffects();
                HoldQueue.RemoveAt(queuePtr);
                queuePtr--; // Pointer rollback

                return;
            }

            holdNoteEntry.UpdateDedicatedEffects(Time.deltaTime);

            // Note position
            var laneHoldNote =
                (Lane)holdNoteEntry.HitObject.Lane.Original.GetStoryboardableObject(beat); // Which lane is the hold note on?

            LanePosition
                step = laneHoldNote.GetLanePosition(beat, beat, PlayerScreen.sTargetSong.Timing); // Get the lane position for the current beat

            Vector3 startHoldPosition = laneHoldNote.Position +
                                        Quaternion.Euler(laneHoldNote.Rotation) * step.StartPosition;

            Vector3 endHoldPosition = laneHoldNote.Position +
                                      Quaternion.Euler(laneHoldNote.Rotation) * step.EndPosition;

            LaneGroupPlayer currentHoldGroupPlayer = holdNoteEntry.HitObject.Lane.Group;

            //Debug.Log(
            //    $"Got; Lane: {laneHoldNote.Name}, Start Position: {startHoldPosition}, End Position: {endHoldPosition}");

            // Apply transforms in the group
            while (currentHoldGroupPlayer) // Current LaneGroupPlayer still exists
            {
                var currentLaneGroup =
                    (LaneGroup)currentHoldGroupPlayer.Original
                        .GetStoryboardableObject(
                            beat); // Get the current lanegroup

                startHoldPosition = currentLaneGroup.Position +
                                    Quaternion.Euler(currentLaneGroup.Rotation) * startHoldPosition; // Apply transform manually

                endHoldPosition = currentLaneGroup.Position +
                                  Quaternion.Euler(currentLaneGroup.Rotation) * endHoldPosition;

                currentHoldGroupPlayer = currentHoldGroupPlayer.Parent; // Go to the parent LaneGroupPlayer
            }

            //Debug.Log($"Transformed; Start Position: {startHoldPosition}, End Position: {endHoldPosition}");

            var hitObject =
                (HitObject)holdNoteEntry.HitObject.Original
                    .GetStoryboardableObject(
                        beat); // Get the hitobject data for thecurrent beat

            //Debug.Log(
            //    $"Hold note hitobject data: Type: {hitObject.Type}, Hold Length: {hitObject.HoldLength}, Position: {hitObject.Position}");

            // Calculate hitbox positions
            // I dunno what lerp is but just go with it, I guess

            Vector3 holdNoteLerpStart = Vector3.LerpUnclamped(
                startHoldPosition,
                endHoldPosition,
                hitObject.Position
            );

            Vector3 holdNoteLerpEnd = Vector3.LerpUnclamped(
                startHoldPosition,
                endHoldPosition,
                hitObject.Position + hitObject.Length
            );

            Vector2 holdNoteHitboxStart = Player.Pseudocamera.WorldToScreenPoint(holdNoteLerpStart);

            Vector2 holdNoteHitboxEnd = Player.Pseudocamera.WorldToScreenPoint(holdNoteLerpEnd);

            //Debug.Log($"Hold note hitbox start: {holdNoteHitboxStart}, end: {holdNoteHitboxEnd}");

            holdNoteEntry.HitObject.HitCoord = new HitScreenCoord
            {
                Position = (holdNoteHitboxStart + holdNoteHitboxEnd) / 2,
                Radius = Mathf.Max( 
                    Vector2.Distance(holdNoteHitboxStart, holdNoteHitboxEnd) / 2 + Player.ScaledExtraRadius,
                    Player.ScaledMinimumRadius
                )
            };

            // Draw the hitobject radius
            /*if (PlayerHitboxVisualizer.main)
                    {
                        PlayerHitboxVisualizer.main.DrawHitScreenCoordDebug(
                            holdNoteEntry.HitObject.HitCoord,
                            Color.green
                        );
                    }*/

            //Debug.Log(
            //    $"Hold note hitbox position: {holdNoteEntry.HitObject.HitCoord.Position}, radius: {holdNoteEntry.HitObject.HitCoord.Radius}");

            // Hitbox checker
            holdNoteEntry.AssignedTouch = null;

            // Assigned a new touch
            holdNoteEntry.AssignedTouch = TouchClasses.Find(touch => 
                    Vector2.Distance( touch.Touch .screenPosition, holdNoteEntry.HitObject .HitCoord .Position ) <= holdNoteEntry.HitObject.HitCoord .Radius // Be careful, it's <= not <
            );

            // Taking advantage of inline checks, since List<T>.Find() can give null
            holdNoteEntry.IsPlayerHolding = holdNoteEntry.AssignedTouch != null;

            // Update Drain value
            holdNoteEntry.holdPassDrainValue = Mathf.Clamp01( holdNoteEntry.holdPassDrainValue + Time.deltaTime / Player.PassWindow * (holdNoteEntry.IsPlayerHolding 
                    ? 1f : -1f)
            );

            //Debug.Log($"Updating drain value: {holdNoteEntry.holdPassDrainValue}");


            // Check if the hold note is eligible for scoring
            if (!holdNoteEntry.IsScoring && holdNoteEntry.holdPassDrainValue >= 1)
                holdNoteEntry.IsScoring = true;
            else if (holdNoteEntry.IsScoring && holdNoteEntry.holdPassDrainValue == 0)
                holdNoteEntry.IsScoring = false;


            // Hold ticks processing
            while (holdNoteEntry.HitObject.HoldTicks.Count > 0 &&
                   holdNoteEntry.HitObject.HoldTicks[0] <= judgementOffsetTime + float.Epsilon)
            {
                Player.AddScore(
                    holdNoteEntry.IsScoring
                        ? 1 : 0,
                    null
                );

                //Debug.Log(
                //    $"Hold tick at {holdNoteEntry.HitObject.HoldTicks[0]} processed. " +
                //    $"HoldEligible: {holdNoteEntry.IsScoring}, Score: {(holdNoteEntry.IsScoring ? 1 : 0)}");

                Player.HitObjectHistory.Add(
                    new HitObjectHistoryItem( holdNoteEntry.HitObject.HoldTicks[0], HitObjectHistoryType.Catch, holdNoteEntry.IsScoring 
                            ? 0 : float .PositiveInfinity // Catch note-like hitobject history
                    )
                );

                //Debug.Log(
                //    $"Hold tick at {holdNoteEntry.HitObject.HoldTicks[0]} removed from hold ticks list.");

                // Remove the first hold tick (and pretty much shift the next tick in)
                holdNoteEntry.HitObject.HoldTicks.RemoveAt(0);

                // Handle hold tick just like how HitPlayer does — reuses a small dedicated
                // pool of effects instead of borrowing/returning one per tick (see
                // HoldNoteClass.EnsureDedicatedEffects for why: at high BPM, ticks fire
                // far more often than a single effect's animation takes to finish).
                if (holdNoteEntry.IsScoring)
                {
                    Color interfaceColor = new Color(PlayerScreen.sCurrentChart.Palette.InterfaceColor.r, PlayerScreen.sCurrentChart.Palette.InterfaceColor.g, PlayerScreen.sCurrentChart.Palette.InterfaceColor.b, 0.32f);
                    holdNoteEntry.EnsureDedicatedEffects(interfaceColor);
                    JudgeScreenEffect effect = holdNoteEntry.PulseDedicatedEffect();

                    if (effect != null)
                    {
                        var rt = (RectTransform)effect.transform;
                        rt.position = CommonSys.sMain.MainCamera.WorldToScreenPoint(holdNoteEntry.HitObject.transform.position);
                    }
                }

                // Missed hold tick, no effect
            }

            if (holdNoteEntry.HitObject.HoldTicks.Count <= 0) // No hold ticks left
            {
                // RemoveHitPlayer already purges this entry from HoldQueue (see
                // PlayerInputEngine.PurgeHitPlayer) — an explicit RemoveAt(queuePtr) here would
                // now delete whatever shifted into that index instead, dropping an unrelated hold note.
                Player.RemoveHitPlayer(holdNoteEntry.HitObject);
                queuePtr--; // Pointer rollback
            }
        }

    }
}
