using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


namespace JANOARG.Client.Behaviors.Player.PlayerInput.Core
{
    public partial class PlayerInputEngine
    {
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
        private bool HasPendingDown(int fingerIndex)
        {
            for (var i = 0; i < _PendingSamples.Count; i++)
                if (_PendingSamples[i].FingerIndex == fingerIndex &&
                    _PendingSamples[i].Phase == TouchPhase.Began)
                    return true;

            return false;
        }
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
        private void InitLogger(string msg)
        {
            if (_InitLog) Debug.Log(msg);
        }

    }
}
