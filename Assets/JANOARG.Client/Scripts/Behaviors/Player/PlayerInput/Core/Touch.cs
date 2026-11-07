using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    public partial class PlayerInputEngine
    {
    /// <summary>
    ///     Records one touch event for the next input frame. Multiple events can arrive per finger
    ///     between two frames, so each is buffered with its own timestamp rather than collapsed into
    ///     whichever position happened to be current when the frame ran.
    /// </summary>
        private void RecordSample(Finger finger)
        {
            // Input is only judged while a chart is running. Dropping events outside it also keeps the
            // buffer from accumulating a pause's worth of stale samples.
            if (!PlayerScreen.sMain || !PlayerScreen.sMain.IsPlaying) return;

            Touch touch = finger.lastTouch;

            if (!touch.valid) return;

            _PendingSamples.Add(new TouchSample
            {
                FingerIndex = finger.index,
                Time        = touch.time,
                Position    = touch.screenPosition,
                Phase       = touch.phase,
            });
        }
    /// <summary>
    ///     Whether a touch-down arrived for this finger since the last input frame. Used to tell a
    ///     gesture that fitted entirely between two frames (which must be reconstructed and judged)
    ///     from one whose down happened while input was not being tracked (which must be ignored).
    /// </summary>
        private bool HasPendingDown(int fingerIndex)
        {
            for (var i = 0; i < _PendingSamples.Count; i++)
                if (_PendingSamples[i].FingerIndex == fingerIndex &&
                    _PendingSamples[i].Phase == TouchPhase.Began)
                    return true;

            return false;
        }
    /// <summary>
    ///     Feeds every recorded event for this finger into its tracker, in order.
    /// </summary>
    /// <remarks>
    ///     One sample per rendered frame averaged the whole frame's motion into a single speed figure:
    ///     a short, sharp flick was flattened below the fire threshold, and the same gesture read
    ///     differently at 30 fps than at 120. Per-event samples measure the movement at the touch
    ///     panel's own resolution, with the event's real timestamp as <c>dt</c>.
    /// </remarks>
        private void DrainFlickSamples(TouchClass touch, int fingerIndex)
        {
            for (var i = 0; i < _PendingSamples.Count; i++)
            {
                TouchSample sample = _PendingSamples[i];

                if (sample.FingerIndex != fingerIndex || sample.Time <= touch.LastFlickSampleTime)
                    continue;

                touch.LastFlickSampleTime = sample.Time;
                touch.FlickTracker.Push(sample.Time, sample.Position);

                // Set the first latch, and refresh it on a re-aim: otherwise the invalidator can reap a
                // flick the player has just corrected back onto the note.
                if (touch.FlickTracker.IsFlicked &&
                    (!touch.Flicked || touch.FlickTracker.ReLatched))
                {
                    touch.Flicked = true;
                    touch.FlickTime = Player.CurrentTime;
                }
            }
        }
        private TouchClass FindTouchByFinger(int fingerIndex)
        {
            for (var i = 0; i < TouchClasses.Count; i++)
                if (TouchClasses[i].Touch.finger.index == fingerIndex)
                    return TouchClasses[i];

            return null;
        }
    /// <summary>
    ///     Logs a message only once when the PlayerScreen is initialized.
    /// </summary>
    /// <param name = "msg"> The message to log. </param>
        private void InitLogger(string msg)
        {
            if (_InitLog) Debug.Log(msg);
        }

    }
}
