using System;

namespace JANOARG.Client.Behaviors.Player
{
    /// <summary>
    ///     Everything <see cref="PlayerScreen.Update" /> needs to decide, per frame, about the song clock:
    ///     where the clock is now, whether the audio source needs restarting, and whether the run is over.
    /// </summary>
    /// <remarks>
    ///     Deliberately free of any UnityEngine dependency so the decision can be exercised as a table of
    ///     synthetic frames — offsets, buffer wraps, lag spikes — without a device, a chart or four minutes
    ///     of audio. The bug this was extracted for only ever reproduced on one player's phone.
    /// </remarks>
    public struct PlayerClockFrame
    {
        /// <summary>AudioSettings.dspTime this frame.</summary>
        public double DspNow;

        /// <summary>AudioSettings.dspTime as of last frame.</summary>
        public double LastDspTime;

        /// <summary>Chart time (playback position + AudioOffset) as of last frame.</summary>
        public double PrevChartTime;

        /// <summary>Time.unscaledDeltaTime, used only when the DSP clock reports no progress.</summary>
        public double FrameDelta;

        /// <summary>AudioSource.timeSamples.</summary>
        public int TimeSamples;

        /// <summary>AudioClip.frequency.</summary>
        public int Frequency;

        /// <summary>AudioSource.isPlaying.</summary>
        public bool IsPlaying;

        /// <summary>AudioClip.length.</summary>
        public double ClipLength;

        /// <summary>The player's audio calibration offset, in seconds. Applies to chart time only.</summary>
        public double AudioOffset;

        /// <summary>
        ///     The DSP time at which the currently scheduled playback is due to finish, or NaN if nothing is
        ///     scheduled. This is what separates "the source dropped out" from "the source finished".
        /// </summary>
        public double SongEndDSP;

        /// <summary>
        ///     The DSP time at which chart time zero falls — i.e. when the song's first scheduled playback
        ///     begins. NaN before the run starts. Set once and never moved by a mid-song restart, so the
        ///     lead-in stays anchored to a single point rather than accumulating per-frame deltas.
        /// </summary>
        public double SongStartDSP;

        /// <summary>Latched by a previous frame — once the song has ended it never un-ends.</summary>
        public bool SongEnded;

        public bool ResultExec;

        public int HitsRemaining;

        public int HoldQueueCount;

        /// <summary>The widest judgment window, i.e. how long a note stays resolvable after its time.</summary>
        public double GoodWindow;

        /// <summary>
        ///     The visual clock's value as of last frame, or NaN when there is no history to extrapolate
        ///     from (first frame, after a seek, after a resync) — in which case it simply snaps.
        /// </summary>
        public double PrevVisualTime;
    }

    public struct PlayerClockDecision
    {
        /// <summary>Chart time: what notes, judgment and visuals are timed against. Offset applied.</summary>
        public double ChartTime;

        /// <summary>Raw playback position within the clip. No offset. What song progress is measured against.</summary>
        public double PlaybackTime;

        /// <summary>The audio source genuinely dropped out mid-song and should be rescheduled.</summary>
        public bool RestartAudio;

        /// <summary>Position to seek to when <see cref="RestartAudio" /> is set.</summary>
        public double RestartSeekTime;

        /// <summary>Latched end of playback. Carry back into the next frame's <see cref="PlayerClockFrame.SongEnded" />.</summary>
        public bool SongEnded;

        /// <summary>Hand off to the result screen.</summary>
        public bool TriggerResult;

        /// <summary>
        ///     Chart time for <em>drawing only</em>, smoothed across the audio clock's update granularity.
        ///     <para>
        ///         AudioSource.timeSamples and AudioSettings.dspTime both advance once per mixer callback,
        ///         not once per frame. On a device whose negotiated buffer is large — Android, where both
        ///         the sample rate and the buffer are picked by the device — several consecutive frames
        ///         read the same position, so a chart drawn against <see cref="ChartTime" /> renders the
        ///         same pose repeatedly and appears to run at a fraction of the real frame rate while the
        ///         UI, which is driven by Time.deltaTime, stays smooth.
        ///     </para>
        ///     <para>
        ///         Deliberately kept out of <see cref="ChartTime" />: judgment stays sample-exact, so hit
        ///         windows and the recorded median offset behave identically on every device, fixed or not.
        ///     </para>
        /// </summary>
        public double VisualTime;
    }

    public static class PlayerClock
    {
        /// <summary>Frame deltas above this are a lag spike, not elapsed song time.</summary>
        public const double SpikeThreshold = 0.1;

        /// <summary>
        ///     timeSamples resets to 0 when a non-looping source finishes, and there is a window where
        ///     isPlaying still reads true while it has already wrapped. A sample-derived position that moves
        ///     the clock backwards by more than this is that wrap — not a seek — and accepting it would
        ///     teleport the run back to the start of the song.
        /// </summary>
        public const double MaxBackwardJump = 0.25;

        /// <summary>
        ///     How far the draw clock may separate from the audio clock before it is treated as a seek or a
        ///     restart rather than the audio clock's normal buffer-sized staircase. Comfortably wider than one
        ///     DSP buffer, narrow enough that a real seek is caught the frame it lands.
        /// </summary>
        public const double MaxVisualDrift = 0.15;

        /// <summary>
        ///     Fraction of the remaining error bled off per second. Small enough to be invisible against the
        ///     per-buffer sawtooth, large enough that a sub-threshold offset (a frame-step seek, for instance)
        ///     does not persist.
        /// </summary>
        public const double DriftCorrectionRate = 2;

        /// <summary>
        ///     Advances the draw clock by real elapsed time every frame, then bleeds off a fraction of its
        ///     separation from the audio clock. A jump wider than <see cref="MaxVisualDrift" /> snaps instead,
        ///     so a seek or a restart stays sample-accurate.
        /// </summary>
        /// <remarks>
        ///     The draw clock deliberately does not snap to the audio clock whenever the latter delivers a new
        ///     reading: the audio clock only moves once per mixer callback (a 1024-sample buffer is roughly
        ///     43–47 Hz), so snapping to it makes the visuals advance for a couple of frames and then stall
        ///     until the next callback. Rendering still runs at the target rate, which is why the frame
        ///     counter can read 120 while the motion appears to step at the buffer rate.
        /// </remarks>
        static double Interpolate(double authoritative, double prevVisual, double frameDelta)
        {
            if (double.IsNaN(prevVisual))
                return authoritative;

            if (Math.Abs(authoritative - prevVisual) > MaxVisualDrift)
                return authoritative;

            double frameStep = frameDelta > 0 ? frameDelta : 0;
            double next      = prevVisual + frameStep;

            // Clamp the correction so an unusually long frame cannot overshoot past the audio clock.
            next += (authoritative - next) * Math.Min(frameStep * DriftCorrectionRate, 1);

            return next;
        }

        public static PlayerClockDecision Advance(in PlayerClockFrame f)
        {
            double rawDelta     = f.DspNow - f.LastDspTime;
            double prevPlayback = f.PrevChartTime - f.AudioOffset;

            // The song finished on schedule. This is an exact question, not a proximity one: we scheduled
            // the playback ourselves, so we know precisely when it is due to end, on any device and at any
            // audio latency.
            bool scheduledEnded = !double.IsNaN(f.SongEndDSP) && f.DspNow >= f.SongEndDSP;
            bool ended          = f.SongEnded || scheduledEnded;

            // Lead-in: the source has not started yet, so the clock is simply how far we are from the
            // scheduled start. Anchored, not accumulated — at the moment of scheduling this reproduces the
            // serialized chart origin exactly, and it reaches 0 precisely when the audio begins, so the
            // handover to sample-derived time is continuous with nothing to snap.
            bool leadIn = !double.IsNaN(f.SongStartDSP) && f.DspNow < f.SongStartDSP;

            if (leadIn)
            {
                double leadPlayback = f.DspNow - f.SongStartDSP;
                double leadChart    = leadPlayback + f.AudioOffset;

                // dspTime is quantised to the mixer buffer exactly as timeSamples is, so the lead-in
                // staircases on the same devices for the same reason and gets the same treatment —
                // otherwise the countdown and intro visuals stutter and then smooth out once the song
                // starts, which reads as a worse bug than the one being fixed.
                return new PlayerClockDecision
                {
                    PlaybackTime    = leadPlayback,
                    ChartTime       = leadChart,
                    VisualTime      = Interpolate(leadChart, f.PrevVisualTime, f.FrameDelta),
                    RestartAudio    = false,
                    RestartSeekTime = 0,
                    SongEnded       = ended,
                    TriggerResult   = false,
                };
            }

            double sampleTime   = f.Frequency > 0 ? (double)f.TimeSamples / f.Frequency : 0;
            bool   sampleUsable = f.IsPlaying && f.PrevChartTime >= 0 && f.Frequency > 0;

            if (sampleUsable && sampleTime < prevPlayback - MaxBackwardJump)
            {
                // Wrapped, not seeked. Fall through to the free-running clock and latch the end.
                sampleUsable = false;
                ended        = true;
            }

            double newPlayback;

            if (rawDelta > SpikeThreshold)
                // Lag spike — resync to the audio if we still trust it, otherwise drop the frame rather
                // than rushing the chart forward by the whole stall.
                newPlayback = sampleUsable ? sampleTime : prevPlayback;
            else if (sampleUsable)
                // Audio is the source of truth during playback.
                newPlayback = sampleTime;
            else
                newPlayback = prevPlayback + (rawDelta > 0 ? rawDelta : f.FrameDelta);

            // Only restart a source that stopped *before* its scheduled end. Past that point it did not
            // drop out, it finished, and rescheduling it is what loops the song.
            bool restart = f is { IsPlaying: false, ResultExec: false }
                           && !ended
                           && newPlayback >= 0
                           && newPlayback < f.ClipLength;

            double chartTime = newPlayback + f.AudioOffset;

            return new PlayerClockDecision
            {
                PlaybackTime    = newPlayback,
                ChartTime       = chartTime,
                VisualTime      = Interpolate(chartTime, f.PrevVisualTime, f.FrameDelta),
                RestartAudio    = restart,
                RestartSeekTime = newPlayback,
                SongEnded       = ended,
                TriggerResult = ShouldTriggerResult(
                    newPlayback,
                    f.ClipLength,
                    f.GoodWindow,
                    ended,
                    f.HitsRemaining,
                    f.HoldQueueCount,
                    f.ResultExec),
            };
        }

        /// <summary>
        ///     Whether the run is over. Split out from <see cref="Advance" /> because the hit counts it reads
        ///     are only final after input has been processed for the frame, whereas the clock and the audio
        ///     lifecycle have to be settled before input runs.
        /// </summary>
        public static bool ShouldTriggerResult(
            double playbackTime,
            double clipLength,
            double goodWindow,
            bool   songEnded,
            int    hitsRemaining,
            int    holdQueueCount,
            bool   resultExec)
        {
            if (resultExec || playbackTime <= 0)
                return false;

            // The chart ran out of notes before the audio did — the common case. The ending animation
            // handles waiting for the music to finish from here.
            if (hitsRemaining <= 0 && holdQueueCount == 0)
                return true;

            // Otherwise the audio has finished with notes still pending. A note sitting at the very end of
            // the clip is only resolvable once the clock has run past it by the judgment window, which is
            // after the audio stopped; firing the instant the audio ends would strand those notes unjudged
            // and report a short score. Bounded, so a stranded hold cannot hold the run open forever.
            bool audioFinished = songEnded || playbackTime >= clipLength;

            return audioFinished && playbackTime >= clipLength + goodWindow;
        }
    }
}
