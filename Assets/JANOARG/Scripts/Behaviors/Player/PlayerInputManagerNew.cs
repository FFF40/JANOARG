using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

// ReSharper disable once CheckNamespace
/// <summary>
/// Represents a class that holds information about the hold note in the game.
/// </summary>
public class HoldNoteClass
{
    /// <summary>
    /// The values associated with the hit player object.
    /// </summary>
    public HitPlayer HitObject;

    /// <summary>
    /// Indicates if the player is currently holding the note.
    /// </summary>
    public bool IsPlayerHolding;

    /// <summary>
    /// Indicates whether the note would give a score on hold ticks (would hit or would miss).
    /// </summary>
    public bool IsScoring;

    /// <summary>
    /// The touch that is currently assigned to this hold note.
    /// </summary>
    /// <remarks>
    /// This property is used to track which touch is currently interacting with the hold note.
    /// </remarks>
    public TouchClass AssignedTouch;

    /// <summary>
    /// The HP Drain-like system for judgement of the player's 
    /// holding performance, ranging from 0 to 1.
    /// </summary>
    public float HoldPassDrainValue
    {
        get => _holdPassDrainValue;
        set
        {
            if (value > 1)
            {
                Debug.LogWarning($"HoldPassDrainValue was above 1 ({value}), clamping to 1");
                value = 1;
            }
            else if (value < 0)
            {
                Debug.LogWarning($"HoldPassDrainValue was below 0 ({value}), clamping to 0");
                value = 0;
            }
            _holdPassDrainValue = value;
        }
    }

    /// <summary>
    /// Backing field for the <see cref="HoldPassDrainValue"/> property.
    /// </summary>
    private float _holdPassDrainValue;

}

/// <summary>
/// Represents a touch event and its associated state and metadata for player input handling.
/// </summary>
public class TouchClass
{
    /// <summary>
    /// The Unity <see cref="Touch"/> structure representing the current touch event.
    /// </summary>
    public Touch Touch;

    /// <summary>
    /// The time (in seconds) when the touch started.
    /// </summary>
    public float StartTime;

    /// <summary>
    /// Indicates whether the touch is considered a tap (true at the moment the finger touches the screen).
    /// </summary>
    public bool Tapped = true;

    /// <summary>
    /// Indicates whether the touch has been recognized as a flick gesture.
    /// </summary>
    public bool Flicked;

    /// <summary>
    /// The direction of the flick gesture in degrees, if applicable.
    /// </summary>
    /// <remarks>
    /// This property can only be set if <see cref="Flicked"/> is true.
    /// </remarks>
    public float FlickDirection
    {
        get => _flickDirection;

        set
        {
            if (!float.IsFinite(value)) // Finite check
            {
                throw new ArgumentOutOfRangeException(nameof(value), "FlickDirection must be a finite number.");
            }

            _flickDirection = value;
        }
    }

    public bool Initial;

    /// <summary>
    /// Backing field for the <see cref="FlickDirection"/> property.
    /// </summary>
    private float _flickDirection;

    /// <summary>
    /// Indicates whether the touch is currently being held.
    /// </summary>
    public bool IsHolding;
    
    /// <summary>
    /// If the touch is within the PassWindow range of a Discrete Hitobject.
    /// </summary>
    /// <remarks>
    /// This is to prevent additional inputs from being passed to a non-discrete hitobject, which results in unexpected early judgement.
    /// </remarks>
    public bool DiscreteHitobjectIsInRange;
    
    /// <summary>
    /// The nearest discrete hitobject to the touch.
    /// </summary>
    /// <remarks>
    /// This is to compare the attributes to the normal hitobject.
    /// </remarks>
    public HitPlayer NearestDiscreteHitobject;

    public float DiscreteHitobjectDistance;

    /// <summary>
    /// The initial position of the flick gesture.
    /// </summary>
    public Vector2 FlickCenter;

    /// <summary>
    /// The <see cref="HitPlayer"/> object that the touch interacted with, if any.
    /// </summary>
    public HitPlayer QueuedHit;

    /// <summary>
    /// The distance from the touch to the associated hit object.
    /// </summary>
    public float QueuedHitDistance;

    /// <summary>
    /// The time (in seconds) when the player flicked past the threshold.
    /// </summary>
    public float FlickTime;
}

public class PlayerInputManagerNew : MonoBehaviour
{
    public static PlayerInputManagerNew Instance;
    public PlayerScreen Player;
    [Space]
    public bool Autoplay;
    [Space]
    [ReadOnly] public float UpdatePerSecond = float.NaN;

    [ReadOnly] public string Delta  = _deltaTime.ToString("F3") + "ms";
    private static double _deltaTime;
    [ReadOnly] public int TouchClassesCount;
    [ReadOnly] public int HoldQueueCount;
    [ReadOnly] public int HitQueueCount;
    [ReadOnly] public int DiscreteHitQueueCount;

    public readonly List<TouchClass> TouchClasses = new();
    [HideInInspector] public List<HitPlayer> HitQueue = new();
    [HideInInspector] public List<HitPlayer> DiscreteHitQueue = new(); // Special queue for catch/flickable notes
    public readonly List<HoldNoteClass> HoldQueue = new();

    public void Awake() // Unity's version of constructor
    {
        EnhancedTouchSupport.Enable();
        Instance = this;
    }


    public void OnDestroy() // Deconstructor
    {
        EnhancedTouchSupport.Disable();
    }

    /// <summary>
    /// Determines whether the absolute angular difference between the expected and actual flick directions
    /// is within a reasonable range (±25 degrees) to be considered a valid flick gesture.
    /// </summary>
    /// <param name="expected">The expected flick direction from the hit object, in degrees.</param>
    /// <param name="actual">The actual flick direction done by the player, in degrees.</param>
    /// <returns>true if within a reasonable range, false otherwise.</returns>
    private bool ValidateFlickDirection(float expected, float actual)
    {
        float angularDifference = Mathf.DeltaAngle(expected, actual); // Signed difference (-180 to +180)
        float absDiff = Mathf.Abs(angularDifference);
        bool comparison = absDiff <= 25f;
        
        // More leniency
        bool closeEnough = absDiff <= 27.5f || Mathf.Approximately(absDiff, 25f);

        Debug.Log($"ValidatingFlickPass: Expected {expected}°, got {actual}°, Difference ~25/27.5 < ({angularDifference})° ({comparison || closeEnough})");
        return comparison || closeEnough; // ±25 degrees
    }

    /// <summary>
    /// Logs a message only once when the PlayerScreen is initialized.
    /// </summary>
    /// <param name="msg">The message to log.</param>
    private void InitLogger(string msg)
    {
        if (_initLog) Debug.Log(msg);
    }

    /// <summary>
    /// Adds the given <see cref="HitPlayer"/> object to the player's hit queue, sorted by time.
    /// </summary>
    /// <param name="hit">The <see cref="HitPlayer"/> object to add to the queue.</param>
    public void AddToQueue(HitPlayer hit)
    {
        int index = HitQueue.FindLastIndex(x => x.Time < hit.Time);
        Debug.Log($"Adding hit at time {hit.Time} to queue. Inserting at index {index + 1}.");
        HitQueue.Insert(index + 1, hit);
    }

    bool _initLog = true; // log only once on PlayerScreen initialization (we don't want this to spam every millisecond, no?)

    private double _lastTimeMs = -1;

    public void UpdateInput() // Main input thread
    {
        double currentTimeMs = Player.CurrentTime * 1000.0;

        #region Unity statistics

        TouchClassesCount = TouchClasses.Count;
        HoldQueueCount = HoldQueue.Count;
        HitQueueCount = HitQueue.Count;
        DiscreteHitQueueCount = DiscreteHitQueue.Count;

        #endregion

        if (!Autoplay) // Player input
        {
            InitLogger("Autoplay: OFF");

            int inputCount = Touch.activeTouches.Count;

            float screenDpi =
                Screen.dpi > 0
                ? Screen.dpi
                : 100; // Minimum DPI

            float flickThreshold = screenDpi * 0.4f; // 40% of screen dpi (about 1cm)
            InitLogger($"Set flick threshold to {flickThreshold}px (DPI: {screenDpi})");

            // Main touch iterator
            for (int a = 0; a < inputCount; a++)
            {
                Touch inputEntry = Touch.activeTouches[a];
                int fingerIndex = inputEntry.finger.index;

                // Handle touch end/cancel
                if (inputEntry is { isInProgress: false, phase: TouchPhase.Ended or TouchPhase.Canceled })
                {
                    TouchClasses.RemoveAll(input => input.Touch.finger.index == fingerIndex);
                    continue;
                }

                // Find existing touch or create new one
                TouchClass touchClass = TouchClasses.Find(t => t.Touch.finger.index == fingerIndex);
    
                if (touchClass == null) // New touch
                {
                    touchClass = new TouchClass
                    {
                        Touch = inputEntry,
                        StartTime = Player.CurrentTime,
                        FlickCenter = inputEntry.startScreenPosition,
                        Initial = true
                    };
                    TouchClasses.Add(touchClass);
                }
                
                else // Existing touch
                {
                    touchClass.Touch = inputEntry;

                    // Flick detector

                    // Pre-check for proximity to a flickable note
                    if (!touchClass.Flicked && inputEntry.phase == TouchPhase.Moved)
                    {
                        bool nearFlickable = HitQueue.Any(hit =>
                            hit.Current.Flickable &&
                            !hit.IsProcessed &&
                            Vector2.Distance(inputEntry.screenPosition, hit.HitCoord.Position) < hit.HitCoord.Radius * 1.5f
                        );

                        if (!touchClass.Flicked && inputEntry.phase == TouchPhase.Moved &&
                            touchClass.Initial &&
                            nearFlickable) // or use a bool flag like touchClass.FlickCenterWasSet
                        {
                                touchClass.FlickCenter = inputEntry.screenPosition;
                                Debug.Log($"[FlickCenter Reset] Initial set to {inputEntry.screenPosition}");
                        }

                    }

                    float flickDistance = Vector2.Distance(inputEntry.screenPosition, touchClass.FlickCenter);
                    
                    // Direction calculation; runs twice more often than the threshold checker
                    if (flickDistance >= flickThreshold / 2)
                        touchClass.FlickDirection = Vector2.SignedAngle(
                            Vector2.up,
                            inputEntry.screenPosition - touchClass.FlickCenter
                        );
                    
                    // Verifier
                    if (!touchClass.Flicked && (Mathf.Approximately(flickDistance, flickThreshold) || flickDistance > flickThreshold))
                    {
                        touchClass.Flicked = true;
                        touchClass.FlickTime = Player.CurrentTime;
                        Debug.Log($"[FlickDirection] From {touchClass.FlickCenter} to {inputEntry.screenPosition} => {touchClass.FlickDirection}°");
                    }

                    // Invalidator
                    if (touchClass.Flicked)
                    {
                        bool flickTimedOut = Math.Abs(Player.CurrentTime - touchClass.FlickTime) > Player.PerfectWindow;

                        bool nearAnyFlickable = HitQueue.Any(hit =>
                            hit.Current.Flickable &&
                            !hit.IsProcessed &&
                            Mathf.Abs(hit.Time - Player.CurrentTime) <= Player.PassWindow
                        );

                        if (flickTimedOut && nearAnyFlickable)
                        {
                            touchClass.Flicked = false;
                            touchClass.FlickCenter = inputEntry.screenPosition;
                        }

                    }

                    // Already handling the same touch on the second pass, consider it holding
                    touchClass.IsHolding = true;
                }
                
                touchClass.Initial = false;
            }

            float judgementOffsetTime = Player.CurrentTime + Player.Settings.JudgmentOffset; // Judgement offset
            InitLogger($"Judgement-offset time: {judgementOffsetTime} (Current time: {Player.CurrentTime}, Offset: {Player.Settings.JudgmentOffset})");


            for (int a = 0; a < HitQueue.Count; a++) // Chart HitObject queue processor
            {
                HitPlayer hitIteration = HitQueue[a];

                if (!hitIteration) // If the hitobject has already been destroyed in runtime
                {
                    //Debug.Log($"Removing destroyed HitPlayer {a} from queue.");
                    HitQueue.RemoveAt(a);
                    a--;
                    continue; // Go check the next hitobject
                }

                float hitobjectTimingDelta = judgementOffsetTime - hitIteration.Time; // Hit time adjusted by judgement offset

                bool isDiscreteHitObject =
                    (hitIteration.Current.Type == HitObject.HitType.Catch)
                    || hitIteration.Current.Flickable;

                float window = isDiscreteHitObject // Timing window per hitobject type
                    ? Player.PassWindow // You either hit it or miss it
                    : Player.GoodWindow; // Only hit as far as the MISALIGNED timing window


                // Draw debug hitboxes 
                // TODO remove when debug complete
                if (hitobjectTimingDelta >= -window && PlayerHitboxVisualizer.main)
                {
                    PlayerHitboxVisualizer.main.DrawHitScreenCoordDebug(
                        hitIteration.HitCoord,
                        Color.Lerp(
                            Color.clear,
                            hitobjectTimingDelta > 0 ? Color.red : PlayerScreen.CurrentChart.Palette.InterfaceColor * new ColorFrag(a: 1),
                            Mathf.Pow(1 - Mathf.Abs(hitobjectTimingDelta / Player.GoodWindow), 2)
                        )
                    );
                }


                bool alreadyHit = false;

                if (hitIteration.Current.HoldLength > 0 && !hitIteration.PendingHoldQueue)  // Pass to HoldNoteClass if there's a hold length
                {
                    hitIteration.PendingHoldQueue = true; // Mark as pending hold queue
                    Debug.Log($"Hitobject at {hitIteration.Time} is a hold note. Adding to hold queue soon.");
                }

                if (hitobjectTimingDelta >= -window && !hitIteration.IsProcessed)
                {
                    
                    if (hitIteration.Current.Flickable) // Flick notes (catch/tap)
                    {
                        float distance = 0;

                        foreach (TouchClass touch in TouchClasses)
                        {
                            if (touch.QueuedHit != null && !touch.Flicked) continue;

                            bool isValid = false;

                            switch (hitIteration.Current.Type)
                            {
                                case HitObject.HitType.Normal:
                                    isValid = TapFlickVerifier(hitIteration, touch);
                                    break;
                                
                                case HitObject.HitType.Catch:
                                    isValid = FlickVerifier(hitIteration, touch);
                                    break;
                            }

                            if (!isValid)
                                continue;

                            hitIteration.InDiscreteHitQueue = true;
                            
                            // Reset flick state
                            touch.Flicked = false;
                            touch.FlickCenter = touch.Touch.screenPosition;
                            hitIteration.IsProcessed = true;


                            if (hitIteration.Current.Type == HitObject.HitType.Normal)
                            {
                                hitIteration.IsTapped = true;
                                touch.QueuedHitDistance = distance;
                            }

                            if (hitIteration.Current.Type == HitObject.HitType.Catch)
                            {
                                touch.DiscreteHitobjectIsInRange = true;
                                touch.DiscreteHitobjectDistance = distance;
                            }
                        }

                        #region FLICK NOTE LOCAL FUNCTION
                        bool TapFlickVerifier(HitPlayer hitObject, TouchClass touch)
                        {
                            float timeDifference;

                            // Shared logic from the normal tap note's
                            if (
                                touch.Tapped &&
                                ( // Tap flick hitbox check

                                    ( // Directional tap flick
                                        float.IsFinite(hitObject.Current.FlickDirection) &&
                                        (distance = Mathf.Abs((Quaternion.Euler(0, 0, hitObject.Current.FlickDirection) * (touch.Touch.startScreenPosition - hitObject.HitCoord.Position)).x)
                                        ) < hitIteration.HitCoord.Radius + flickThreshold
                                    ) ||

                                    // Omnidirectional tap flick
                                    (distance = Vector2.Distance(touch.Touch.startScreenPosition, hitObject.HitCoord.Position)
                                    ) < hitIteration.HitCoord.Radius
                                ) &&
                                (
                                    !touch.QueuedHit ||
                                    (timeDifference = hitIteration.Time - touch.QueuedHit.Time) < -1e-3f ||
                                    (timeDifference < 1e-3f && distance < touch.QueuedHitDistance)
                                )
                            )
                            {
                                Debug.Log($"Touched {touch.Touch.finger.index} is a tap flick. Passing to main flick verifier with flick direction {touch.FlickDirection}.");
                                return FlickVerifier(hitObject, touch, hitObject.Current.FlickDirection);
                            }
                            else
                                return false;

                        }

                        // Applies to both tap and catch flick (main processor for distance and angle calculations)
                        bool FlickVerifier(HitPlayer hitObject, TouchClass touch, float? angle = null)
                        {

                            
                            if (!touch.Flicked ||
                                hitObject.Current.Type == HitObject.HitType.Catch && // Only do range check if it's a catch-flick
                                // None of the positions touched it :(
                               (distance = Vector2.Distance(touch.Touch.startScreenPosition, hitObject.HitCoord.Position)) > hitIteration.HitCoord.Radius &&
                               Vector2.Distance(touch.Touch.screenPosition, hitObject.HitCoord.Position) 
                               > hitIteration.HitCoord.Radius)
                                
                                return false;
                            
                            if (hitObject.Current.Type == HitObject.HitType.Normal){}

                            //Debug.Log($"Touch {touch.Touch.finger.index} is in range on FLICKABLE hitobject at {hitIteration.Time}. Adding to discrete hit queue.");
                            if (float.IsFinite(hitObject.Current.FlickDirection)) // Directional flick
                            {
                                float calculatedAngle = angle ?? touch.FlickDirection;

                                return ValidateFlickDirection(hitObject.Current.FlickDirection, calculatedAngle);
                            }
                            else
                                // Omnidirectional flick (doesn't give a fuck which direction you flick)
                                return true;
                        }
                        #endregion
                        
                    }
                    else
                        switch (hitIteration.Current.Type)
                        {
                            case HitObject.HitType.Normal:
                                foreach (TouchClass touch in TouchClasses)
                                {
                                    float distance;

                                    bool discreteTapProtectionPassed = false;

                                    if (
                                        touch.Tapped &&
                                        (
                                            distance = Vector2.Distance(touch.Touch.screenPosition, hitIteration.HitCoord.Position)
                                        ) < hitIteration.HitCoord.Radius &&
                                        (
                                            discreteTapProtectionPassed = !( // Safeguard to prevent false 'early' taps while the player intends to catch notes
                                                
                                                // Status check
                                                touch.DiscreteHitobjectIsInRange &&
                                                touch.NearestDiscreteHitobject != null &&
                                                touch.NearestDiscreteHitobject.Current.Type == HitObject.HitType.Catch &&
                                                
                                                // Only suppress if the catch note is EARLIER and likely to be triggered by this input
                                                touch.NearestDiscreteHitobject.Time < hitIteration.Time &&
                                                hitIteration.Time >= -Player.GoodWindow &&
                                                
                                                // Spatial distance comparison
                                                Vector2.Distance(touch.Touch.screenPosition, touch.NearestDiscreteHitobject.HitCoord.Position) < distance &&
                                                (hitIteration.Time - touch.NearestDiscreteHitobject.Time) <= Player.GoodWindow * 2
                                            )
                                            ||
                                            ( // Exception clause
                                                    
                                                touch.DiscreteHitobjectIsInRange &&
                                                touch.NearestDiscreteHitobject != null &&
                                                ( // Ways that won't break the player's expectation
                                                    Math.Abs(hitobjectTimingDelta) <= Player.PerfectWindow ||
                                                    Mathf.Approximately(hitIteration.Time, touch.NearestDiscreteHitobject.Time) ||
                                                    Mathf.Approximately(Vector3.Distance(hitIteration.HitCoord.Position, touch.NearestDiscreteHitobject.HitCoord.Position), hitIteration.HitCoord.Radius/2)
                                                )
                                            )
                                        )&&
                                        (
                                            !touch.QueuedHit ||
                                            hitIteration.Time < touch.QueuedHit.Time ||
                                            (
                                                Mathf.Approximately(hitIteration.Time, touch.QueuedHit.Time) &&
                                                distance < touch.QueuedHitDistance
                                            )
                                        )
                                    )
                                    {
                                        Debug.Log($"Touch {touch.Touch.finger.index} tapped on hitobject at {hitIteration.Time}. Adding to queue.");
                                        touch.QueuedHit = hitIteration;
                                        touch.QueuedHitDistance = distance;
                                        alreadyHit = true;
                                    }
                                    else if (!discreteTapProtectionPassed && touch.NearestDiscreteHitobject != null)
                                    {
                                        Debug.Log($"Tap suppressed for hitobject at {hitIteration.Time}. \n" +
                                                  $"At touch.NearestDiscreteHitobject.Time: {touch.NearestDiscreteHitobject.Time} < hitIteration.Time: {hitIteration.Time}. \n" +
                                                  $"At touch.NearestDiscreteHitobject.Type: {touch.NearestDiscreteHitobject.Current.Type} \n" +
                                                  $"At touch.NearestDiscreteHitobject.HitCoord.Position: {touch.NearestDiscreteHitobject.HitCoord.Position} < hitIteration.HitCoord.Position: {hitIteration.HitCoord.Position}.\n" +
                                                  $"At Hit Delta {hitobjectTimingDelta} >= -{Player.GoodWindow}. \n" +
                                                  $"At comparison of Discrete-Tap delta {hitIteration.Time - touch.NearestDiscreteHitobject.Time} < {Player.GoodWindow * 2}");
                                    }
                                }
                                break;
                            
                            case HitObject.HitType.Catch:
                                foreach (TouchClass touch in TouchClasses)
                                {
                                    float distance = Vector2.Distance(touch.Touch.screenPosition, hitIteration.HitCoord.Position);

                                    if (distance < hitIteration.HitCoord.Radius)
                                    {
                                        bool shouldAssign = !hitIteration.InDiscreteHitQueue;  // Slow down on the assigning, due to the nature of catch notes being able to be spammed at lightspeed
                                        // || hitIteration.Time < touch.QueuedHit.Time 
                                        // || (
                                        // Mathf.Approximately(hitIteration.Time, touch.QueuedHit.Time) &&
                                        // distance < touch.DiscreteHitobjectDistance
                                        // );

                                        if (shouldAssign)
                                        {
                                            Debug.Log($"[Catch Note Assign] Touch {touch.Touch.finger.index} queued catch note at {hitIteration.Time} (dist: {distance})");

                                            hitIteration.InDiscreteHitQueue = true;
                                            touch.DiscreteHitobjectDistance = distance;
                                            touch.DiscreteHitobjectIsInRange = true;
                                            alreadyHit = true;
                                        }
                                    }

                                }
                                break;
                        }

                    // For additional inputs
                    foreach (TouchClass touch in TouchClasses)
                    {
                        if (
                            isDiscreteHitObject &&
                            Vector2.Distance(touch.Touch.screenPosition, hitIteration.HitCoord.Position) <=
                            hitIteration.HitCoord.Radius
                        )
                        {
                            touch.DiscreteHitobjectIsInRange = true;
                            touch.NearestDiscreteHitobject = hitIteration;
                        }
                    }

                    // Pass to DiscreteHitQueue
                    if (hitIteration.InDiscreteHitQueue || alreadyHit && hitIteration.Current.Type == HitObject.HitType.Catch)
                    {
                        Player.Hit(hitIteration, offsetedHit);

                        hitIteration.InDiscreteHitQueue = false;

                        // Clear any touch that was assigned to this hit
                        foreach (var touch in TouchClasses)
                        {
                            if (touch.QueuedHit == hitIteration)
                            {
                                touch.QueuedHit = null;
                                touch.DiscreteHitobjectIsInRange = false;
                            }
                        }

                        if (hitIteration.PendingHoldQueue)
                        {
                            HoldQueue.Add(new HoldNoteClass
                            {
                                HitObject = hitIteration,
                                HoldPassDrainValue = 1
                            });
                            HitQueue.Remove(hitIteration);
                        }
                    }

                    if (!alreadyHit && hitobjectTimingDelta > window) // Didn't hit the hitobject within the timing window
                    {
                        Player.Hit(hitIteration, float.PositiveInfinity, false);

                        hitIteration.IsProcessed = true; // Prevent Hit function from triggering more than once

                        // Clear any touch that was assigned to this missed hit
                        foreach (var touch in TouchClasses)
                        {
                            if (touch.QueuedHit == hitIteration)
                            {
                                touch.QueuedHit = null;
                                touch.DiscreteHitobjectIsInRange = false;
                            }
                        }
                        Debug.Log($"Hitobject at {hitIteration.Time} ({hitIteration.Current.Type}) missed. Radius: {hitIteration.HitCoord.Radius}, Hold? {(hitIteration.PendingHoldQueue ? "Yes": "No")}");

                        if (hitIteration.PendingHoldQueue) //Pass to HoldQueue regardless
                        {
                            HoldQueue.Add(new HoldNoteClass
                            {
                                HitObject = hitIteration

                            });
                            HitQueue.Remove(hitIteration);

                            Debug.Log($"Hold hitobject head handled, passing to hold queue.");
                        }
                    }
                }

                // Skip checks if none of the hitobjects are even near window range
                if (hitobjectTimingDelta < -Math.Max(Player.PassWindow, Player.GoodWindow))
                {
                    break;
                }
            }

            if (HoldQueue.Count != 0) // Hold note processor
            {

                //// Camera handling is done here to calculate hold note hitboxes on the fly
                //// As it has dynamic attributes as it progresses, unlike normal hitobjects.

                float beat = PlayerScreen.TargetSong.Timing.ToBeat(judgementOffsetTime); // Get current BPM

                // Camera handling
                CameraController currentCamera = (CameraController)PlayerScreen.TargetChart.Data.Camera.Get(beat); // Get camera data for the current beat
                // Update transforms
                Player.Pseudocamera.transform.position = currentCamera.CameraPivot;
                Player.Pseudocamera.transform.eulerAngles = currentCamera.CameraRotation;
                Player.Pseudocamera.transform.Translate(Vector3.back * currentCamera.PivotDistance);

                for (int a = 0; a < HoldQueue.Count; a++)
                {
                    HoldNoteClass holdNoteEntry = HoldQueue[a];

                    Debug.Log($"Processing hold note entry {a} at time {holdNoteEntry.HitObject.Time}.");

                    // If the hold note doesn't exist (it's already completed)
                    if (!holdNoteEntry.HitObject)
                    {
                        HoldQueue.RemoveAt(a);
                        a--; // Pointer rollback
                        continue;
                    }

                    // Note position
                    Lane laneHoldNote = (Lane)holdNoteEntry.HitObject.Lane.Original.Get(beat); // Which lane is the hold note on?
                    LanePosition step = laneHoldNote.GetLanePosition(beat, beat, PlayerScreen.TargetSong.Timing); // Get the lane position for the current beat
                    Vector3 startHoldPosition = Quaternion.Euler(laneHoldNote.Rotation) * step.StartPos + laneHoldNote.Position;
                    Vector3 endHoldPosition = Quaternion.Euler(laneHoldNote.Rotation) * step.EndPos + laneHoldNote.Position;
                    LaneGroupPlayer currentHoldGroupPlayer = holdNoteEntry.HitObject.Lane.Group;

                    Debug.Log($"Got; Lane: {laneHoldNote.Name}, Start Position: {startHoldPosition}, End Position: {endHoldPosition}");

                    // Apply transforms in the group
                    while (currentHoldGroupPlayer) // Current LaneGroupPlayer still exists
                    {
                        LaneGroup currentLaneGroup = (LaneGroup)currentHoldGroupPlayer.Original.Get(beat); // Get the current lane group
                        startHoldPosition = Quaternion.Euler(currentLaneGroup.Rotation) * startHoldPosition + currentLaneGroup.Position; // Apply transform manually
                        endHoldPosition = Quaternion.Euler(currentLaneGroup.Rotation) * endHoldPosition + currentLaneGroup.Position;
                        currentHoldGroupPlayer = currentHoldGroupPlayer.Parent; // Go to the parent LaneGroupPlayer
                    }

                    Debug.Log($"Transformed; Start Position: {startHoldPosition}, End Position: {endHoldPosition}");

                    HitObject hitObject = (HitObject)holdNoteEntry.HitObject.Original.Get(beat); // Get the hitobject data for the current beat

                    Debug.Log($"Hold note hitobject data: Type: {hitObject.Type}, Hold Length: {hitObject.HoldLength}, Position: {hitObject.Position}");

                    // Calculate hitbox positions
                    // I dunno what lerp is but just go with it, I guess

                    var holdNoteLerpStart = Vector3.LerpUnclamped(
                        startHoldPosition,
                        endHoldPosition,
                        hitObject.Position
                    );

                    var holdNoteLerpEnd = Vector3.LerpUnclamped(
                        startHoldPosition,
                        endHoldPosition,
                        hitObject.Position + hitObject.Length
                    );
                    
                    Vector2 holdNoteHitboxStart = Player.Pseudocamera.WorldToScreenPoint(holdNoteLerpStart);

                    Vector2 holdNoteHitboxEnd = Player.Pseudocamera.WorldToScreenPoint(holdNoteLerpEnd);

                    Debug.Log($"Hold note hitbox start: {holdNoteHitboxStart}, end: {holdNoteHitboxEnd}");

                    holdNoteEntry.HitObject.HitCoord = new HitScreenCoord
                    {
                        Position = (holdNoteHitboxStart + holdNoteHitboxEnd) / 2,
                        Radius = Mathf.Max(
                            Vector2.Distance(holdNoteHitboxStart, holdNoteHitboxEnd) / 2 + Player.ScaledExtraRadius,
                            Player.ScaledMinimumRadius
                        )
                    };

                    // TODO remove when debug complete
                    
                    // Draw the hitobject radius
                    if (PlayerHitboxVisualizer.main)
                    {
                        PlayerHitboxVisualizer.main.DrawHitScreenCoordDebug(
                            holdNoteEntry.HitObject.HitCoord,
                            Color.green
                        );
                    }

                    Debug.Log($"Hold note hitbox position: {holdNoteEntry.HitObject.HitCoord.Position}, radius: {holdNoteEntry.HitObject.HitCoord.Radius}");

                    // Hitbox checker
                    holdNoteEntry.AssignedTouch = null;
                    
                    // Assigned a new touch
                    holdNoteEntry.AssignedTouch = TouchClasses.Find(touch => Vector2.Distance(
                            touch.Touch.screenPosition,
                            holdNoteEntry.HitObject.HitCoord.Position
                        ) <= holdNoteEntry.HitObject.HitCoord.Radius // Be careful, it's <= not <
                    );

                    // Taking advantage of inline checks, since List<T>.Find() can give null
                    holdNoteEntry.IsPlayerHolding = holdNoteEntry.AssignedTouch != null;
                    
                    // Update Drain value
                    holdNoteEntry.HoldPassDrainValue = Mathf.Clamp01(
                        holdNoteEntry.HoldPassDrainValue + Time.deltaTime / Player.PassWindow * 
                        (holdNoteEntry.IsPlayerHolding ? 1f : -1f)
                    );
                    
                    Debug.Log($"Updating drain value: {holdNoteEntry.HoldPassDrainValue}");


                    // Check if the hold note is eligible for scoring
                    if (!holdNoteEntry.IsScoring && holdNoteEntry.HoldPassDrainValue >= 1)
                        holdNoteEntry.IsScoring = true;
                    else if (holdNoteEntry.IsScoring && holdNoteEntry.HoldPassDrainValue == 0)
                        holdNoteEntry.IsScoring = false;


                    // Hold ticks processing
                    while (holdNoteEntry.HitObject.HoldTicks.Count > 0 &&
                           holdNoteEntry.HitObject.HoldTicks[0] <= judgementOffsetTime + Single.Epsilon)
                    {
                        Player.AddScore(
                            holdNoteEntry.IsScoring
                                ? 1
                                : 0,
                            null
                        );
                        Debug.Log($"Hold tick at {holdNoteEntry.HitObject.HoldTicks[0]} processed. " +
                                  $"HoldEligible: {holdNoteEntry.IsScoring}, Score: {(holdNoteEntry.IsScoring ? 1 : 0)}");

                        Player.HitObjectHistory.Add(new HitObjectHistoryItem(
                                holdNoteEntry.HitObject.HoldTicks[0],
                                HitObjectHistoryType.Catch,
                                holdNoteEntry.IsScoring ? 0 : float.PositiveInfinity // Catch note-like hitobject history
                            )
                        );

                        Debug.Log($"Hold tick at {holdNoteEntry.HitObject.HoldTicks[0]} removed from hold ticks list.");

                        // Remove the first hold tick (and pretty much shift the next tick in)
                        holdNoteEntry.HitObject.HoldTicks.RemoveAt(0);

                        // Handle hold tick just like how HitPlayer does
                        if (holdNoteEntry.IsScoring)
                        {
                            var effect = Instantiate(Player.JudgeScreenSample, Player.JudgeScreenHolder);
                            effect.SetAccuracy(null);
                            effect.SetColor(PlayerScreen.CurrentChart.Palette.InterfaceColor);
                            var rt = (RectTransform)effect.transform;

                            rt.position = Common.main.MainCamera.WorldToScreenPoint(holdNoteEntry.HitObject.transform.position);
                        }
                        else
                        {
                            // Missed hold tick, no effect
                        }
                    }

                    if (holdNoteEntry.HitObject.HoldTicks.Count <= 0) // No hold ticks left
                    {
                        Player.RemoveHitPlayer(holdNoteEntry.HitObject);
                        HoldQueue.RemoveAt(a);
                        a--; // Pointer rollback
                    }

                }
            }

            for (int i = 0; i < DiscreteHitQueue.Count; i++)
            {
                HitPlayer hitObject = DiscreteHitQueue[i];

                float time = judgementOffsetTime - hitObject.Time;

                if ((judgementOffsetTime >= hitObject.Time && hitObject.Current.Type == HitObject.HitType.Catch) 
                    || hitObject.Current.Flickable) // Immediate feedback on flicks
                {
                    if (hitObject.IsProcessed) // Just in case 
                        Player.Hit(hitObject, time);

                    hitObject.InDiscreteHitQueue = false;
                    hitObject.IsProcessed = true;

                    // Clear any touch that was assigned to this hit
                    foreach (var touch in TouchClasses)
                    {
                        if (touch.QueuedHit == hitObject)
                        {
                            touch.QueuedHit = null;
                            touch.DiscreteHitobjectIsInRange = false;
                            if (hitObject.Current.Flickable) touch.Flicked = false;
                        }
                    }

                    if (hitObject.PendingHoldQueue)
                    {
                        HoldQueue.Add(new HoldNoteClass
                        {
                            HitObject = hitObject,
                            HoldPassDrainValue = 1
                        });
                    }
                    
                    DiscreteHitQueue.Remove(hitObject);
                }
            }

            foreach (var touch in TouchClasses)
            {
                Debug.Log($"Processing queued hit for touch {touch.Touch.finger.index} at time {touch.StartTime}.");

                if (
                    touch.QueuedHit && // if the input in question interacted with a hitobject
                    !touch.QueuedHit.IsProcessed && // Haven't yet hit
                    // And is just a tap note
                    touch.QueuedHit.Current.Type == HitObject.HitType.Normal &&
                    !touch.QueuedHit.Current.Flickable
                )
                {
                    Player.Hit(
                        touch.QueuedHit,
                        touch.StartTime + Player.Settings.JudgmentOffset - touch.QueuedHit.Time
                    );
                    Debug.Log($"Hit queued hitobject at {touch.StartTime + Player.Settings.JudgmentOffset - touch.QueuedHit.Time} for touch {touch.Touch.finger.index}.");
                    touch.QueuedHit.IsProcessed = true; // Mark as hit

                    if (touch.QueuedHit.PendingHoldQueue)
                    {
                        HoldQueue.Add(new HoldNoteClass
                        {
                            HitObject = touch.QueuedHit,
                            IsPlayerHolding = touch.IsHolding,
                            HoldPassDrainValue = 1, // Little leniency won't hurt
                            AssignedTouch = touch
                        });

                        Debug.Log($"Hold hitobject head handled, passing to hold queue.");
                    }

                    touch.QueuedHit = null; // Clear the queued hit
                }
                touch.Tapped = false; // Tap only lasts for a single frame
            }


        }
        else // Autoplay, From old input manager since it works as is (for now)
        {
            for (int i = 0; i < HitQueue.Count; i++)
            {
                HitPlayer currentHit = HitQueue[i];

                if (!currentHit) // If the hitobject has already been destroyed in runtime
                {
                    HitQueue.RemoveAt(i);
                    i--;
                }
                else if (currentHit.IsProcessed) // Autoplay's hold note processor
                {
                    while (currentHit.HoldTicks.Count > 0 && currentHit.HoldTicks[0] <= Player.CurrentTime)
                    {
                        Player.AddScore(1, null);
                        Player.HitObjectHistory.Add(new(currentHit.HoldTicks[0], HitObjectHistoryType.Catch, 0));
                        currentHit.HoldTicks.RemoveAt(0);

                        var effect = Instantiate(Player.JudgeScreenSample, Player.JudgeScreenHolder);
                        effect.SetAccuracy(null);
                        effect.SetColor(PlayerScreen.CurrentChart.Palette.InterfaceColor);
                        var rectTransform = (RectTransform)effect.transform;
                        rectTransform.position = Common.main.MainCamera.WorldToScreenPoint(currentHit.transform.position);
                    }

                    if (currentHit.HoldTicks.Count == 0)
                    {
                        Player.RemoveHitPlayer(currentHit);
                        HitQueue.RemoveAt(i);
                        i--;
                    }
                }
                else if (currentHit.Time <= Player.CurrentTime) // Hit it
                {
                    Player.Hit(currentHit, 0);
                }
                else // Autoplay's job is done (final hitobject is destroyed)
                {
                    break;
                }
            }
        }
        _initLog = false; // Disable logging after the first initialization
        
        if (_lastTimeMs < 0)
        {
            _lastTimeMs = currentTimeMs; // First frame init
            _deltaTime = 16.666; // Fake 60fps to start
            UpdatePerSecond = 60f;
        }
        else
        {
            _deltaTime = currentTimeMs - _lastTimeMs;
            UpdatePerSecond = 1000f / (float)_deltaTime;
            _lastTimeMs = currentTimeMs;
            Delta = _deltaTime.ToString("F3") + "ms";
        }
    }
    
}