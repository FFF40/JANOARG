using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public HitPlayer HitObjectValues;

    /// <summary>
    /// Indicates if the player is currently holding the note.
    /// </summary>
    public bool IsPlayerHolding;

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
        get => _HoldPassDrainValue;
        set
        {
            if (value is > 1 or < 0)
            {
                throw new ArgumentOutOfRangeException(value.ToString(CultureInfo.InvariantCulture),
                    "HoldPassDrainValue must be between 0 and 1.");
            }
            _HoldPassDrainValue = value;
        }
    }

    /// <summary>
    /// Backing field for the <see cref="HoldPassDrainValue"/> property.
    /// </summary>
    private float _HoldPassDrainValue;

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

            if (value < 0 || value >= 360) // Range check
            {
                throw new ArgumentOutOfRangeException(nameof(value), "FlickDirection must be between 0 and 360 degrees.");
            }

            if (!this.Flicked) // Only set if the touch is flicked
            {
                throw new InvalidOperationException("Cannot set FlickDirection unless Flicked is true.");
            }

            _flickDirection = value;
        }
    }

    /// <summary>
    /// Backing field for the <see cref="FlickDirection"/> property.
    /// </summary>
    private float _flickDirection;

    /// <summary>
    /// Indicates whether the touch is currently being held.
    /// </summary>
    public bool IsHolding;

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

    [HideInInspector] public List<TouchClass> TouchClasses = new();
    [HideInInspector] public List<HitPlayer> HitQueue = new();
    [HideInInspector] public List<HoldNoteClass> HoldQueue = new();

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
    /// <returns>true if the within a reasonable range, false otherwise.</returns>
    public bool ValidateFlickPass(float expected, float actual)
    {
        float angularDifference = ((expected - actual) % 360 + 360) % 360;
        return angularDifference < (0+25) || angularDifference > (360-25); // ±25 degrees
    }

    private void ValidateTouchClass(TouchClass handler) // Implement this somehow
    {
        if (handler == null) return;

        if (handler.Touch.phase is TouchPhase.Ended or TouchPhase.Canceled)
        {
            handler.IsHolding = false;
        }
    }

    /// <summary>
    /// Adds the given <see cref="HitPlayer"/> object to the player's hit queue, sorted by time.
    /// </summary>
    /// <param name="hit">The <see cref="HitPlayer"/> object to add to the queue.</param>
    public void AddToQueue(HitPlayer hit)
    {
        int index = HitQueue.FindLastIndex(x => x.Time < hit.Time);
        HitQueue.Insert(index + 1, hit);
    }

    public void UpdateInput() // Main input thread
    {
        if (!Autoplay) // Player input
        {
            int inputCount = Touch.activeTouches.Count;

            float screenDpi =
                Screen.dpi > 0
                ? Screen.dpi
                : 200; // Minimum DPI

            float flickThreshold = screenDpi * 0.2f; //20% of the screen's DPI

            
            for (int a = 0; a < inputCount; a++) // Process Touch entries and pass to TouchClasses as TouchClass entries
            {
                Touch inputEntry = Touch.activeTouches[a]; // Iterate on all touch inputs in single frame

                if (!inputEntry.isInProgress && (inputEntry.phase is TouchPhase.Ended or TouchPhase.Canceled)) // Remove TouchClass entry containing destroyed touch input
                {
                    TouchClasses.RemoveAll(input => input.Touch.finger.index == inputEntry.finger.index); // Remove all matching entries in one operation
                }

                if (!TouchClasses.Any(handler => handler.Touch.finger.index == inputEntry.finger.index)) // Duplicate input check
                {
                    TouchClasses.Add(new TouchClass
                    {
                        Touch = inputEntry,
                        StartTime = (float)inputEntry.startTime - (Time.realtimeSinceStartup + Player.CurrentTime),
                        FlickCenter = inputEntry.startScreenPosition
                    });
                }
            }

            for (int a = 0; a < TouchClasses.Count; a++) // TouchClass processor
            {
                var iteratingTouch = TouchClasses[a];
                Touch touch = iteratingTouch.Touch;

                // Leave no redundant TouchClass entries
                if (!touch.isInProgress)
                {
                    TouchClasses.RemoveAt(a);
                    a--;
                    continue;
                }

                // Flick checker
                if (touch.phase == TouchPhase.Moved)
                {
                    if (
                        Vector2.Distance(touch.screenPosition, iteratingTouch.FlickCenter)
                        > flickThreshold
                    )
                    {
                        iteratingTouch.Flicked = true;
                        iteratingTouch.FlickTime = Player.CurrentTime;
                        iteratingTouch.FlickDirection =
                            Vector2.SignedAngle(
                                Vector2.up, touch.screenPosition - iteratingTouch.FlickCenter
                            );
                        iteratingTouch.FlickCenter = touch.screenPosition; // Update flick center after flick
                    }

                    if (Player.CurrentTime - iteratingTouch.FlickTime > Player.PassWindow) // Reset flick state after pass window
                    {
                        iteratingTouch.Flicked = false;
                    }
                }

                // Hold checker
                if (
                    TouchClasses.Any(handler =>
                        handler.Touch.finger.index == iteratingTouch.Touch.finger.index // Have we checked this entry already?
                    )
                )
                {
                    iteratingTouch.IsHolding = true; // Consider it holding in the next frame
                }
            }

            float judgementTime = Player.CurrentTime + Player.Settings.JudgmentOffset; // Judgement offset

            float beat = PlayerScreen.TargetSong.Timing.ToBeat(judgementTime); // Get current BPM

            
            for (int a = 0; a < HitQueue.Count; a++) // Chart HitObject queue processor
            {
                HitPlayer hitIteration = HitQueue[a];

                if (!hitIteration) // If the hitobject has already been destroyed in runtime
                {
                    HitQueue.RemoveAt(a);
                    a--;
                    continue; // Go check the next hitobject
                }

                float offsetedHit = judgementTime - hitIteration.Time; // Hit time adjusted by judgement offset

                bool haveDiscreteTiming =
                    (hitIteration.Current.Type == HitObject.HitType.Catch)
                    || hitIteration.Current.Flickable;

                float window = haveDiscreteTiming // Timing window per hitobject type
                    ? Player.PassWindow // You either hit it or miss it
                    : Player.GoodWindow; // Only hit as far as the MISALIGNED timing window

                bool alreadyHit = false;

                if (offsetedHit >= -window && !hitIteration.IsHit)
                {

                    if (haveDiscreteTiming) // Discrete hitobject is within the timing window and not already hit
                    {

                        if (hitIteration.Current.Flickable && !(hitIteration.IsHit || hitIteration.InDiscreteHitQueue))
                        {
                            float distance = 0;

                            /// <summary>
                            /// Determines if the given screen position is within a valid range for hitting the target.
                            /// </summary>
                            /// <param name="screenPosition">The position on the screen to check.</param>
                            /// <returns>True if the position is within range, false otherwise.</returns>
                            bool IsInRange(Vector2 screenPosition)
                            {
                                if (hitIteration.Current.Type == HitObject.HitType.Normal &&
                                    float.IsFinite(hitIteration.Current.FlickDirection))
                                {
                                    // Calculate the distance in the x-axis after applying the flick direction rotation
                                    distance = Math.Abs(
                                        (Quaternion.Euler(0, 0, hitIteration.Current.FlickDirection) *
                                        (screenPosition - hitIteration.HitCoord.Position)).x
                                    );

                                    // Check if distance is within the hit coordinate radius
                                    return distance < hitIteration.HitCoord.Radius;
                                }
                                else
                                {
                                    // Calculate the Euclidean distance between the screen position and the hit coordinate
                                    distance = Vector2.Distance(screenPosition, hitIteration.HitCoord.Position);

                                    // Check if distance is within the hit coordinate radius and additional DPI offset
                                    return distance < (hitIteration.HitCoord.Radius + screenDpi * 1f);
                                }
                            }

                            if (hitIteration.Current.Type == HitObject.HitType.Normal &&
                                !hitIteration.IsTapped) // Hey you haven't tapped already? Checking time
                            {
                                foreach (TouchClass touch in TouchClasses)
                                {
                                    float timeDifference;

                                    if (
                                        touch.Tapped &&
                                        IsInRange(touch.Touch.screenPosition) &&
                                        (!touch.QueuedHit ||
                                        (timeDifference = hitIteration.Time - touch.QueuedHit.Time) < -1e-3f ||
                                        (timeDifference < 1e-3f && distance < touch.QueuedHitDistance)
                                        )
                                    )
                                    {
                                        touch.QueuedHit = hitIteration;
                                        touch.QueuedHitDistance = distance;
                                        hitIteration.IsTapped = true;
                                        alreadyHit = true;
                                    }
                                }
                            }
                            else // Catch flicks
                            {
                                foreach (TouchClass touch in TouchClasses)
                                {
                                    if (
                                        touch.Flicked &&
                                        (
                                            hitIteration.Current.Type == HitObject.HitType.Normal
                                            ? touch.QueuedHit == hitIteration
                                            : IsInRange(touch.Touch.screenPosition)
                                        ) &&
                                        (
                                            !float.IsFinite(hitIteration.Current.FlickDirection) ||
                                            ValidateFlickPass(hitIteration.Current.FlickDirection, touch.FlickDirection)
                                        )
                                    )
                                    {
                                        touch.Flicked = false; // Reset flick state (otherwise you can suddenly flick more than one note at once)
                                        hitIteration.InDiscreteHitQueue = true;
                                        alreadyHit = true;
                                    }
                                }
                            }
                        }
                        else // Normal catch note
                        {
                            foreach (TouchClass touch in TouchClasses)
                            {
                                if (Vector2.Distance(touch.Touch.screenPosition, hitIteration.HitCoord.Position) <
                                    hitIteration.HitCoord.Radius
                                )
                                {
                                    hitIteration.InDiscreteHitQueue = true;
                                    alreadyHit = true;
                                }
                            }
                        }
                    }
                    else // Normal tap note
                    {
                        foreach (TouchClass touch in TouchClasses)
                        {
                            float distance,
                                timeDifference;

                            if (
                                touch.Tapped &&
                                (
                                    distance = Vector2.Distance(
                                        touch.Touch.screenPosition,
                                        hitIteration.HitCoord.Position
                                    )
                                ) < hitIteration.HitCoord.Radius &&
                                (
                                    !touch.QueuedHit ||
                                    (timeDifference = hitIteration.Time - touch.QueuedHit.Time) < -1e-3f ||
                                    (timeDifference < 1e-3f && distance < touch.QueuedHitDistance)
                                )
                            )
                            {
                                touch.QueuedHit = hitIteration;
                                touch.QueuedHitDistance = distance;
                                alreadyHit = true;
                            }
                        }
                    }
                }

                // Wait for discrete hitobject to reach judgement line before clearing (for satisfaction)
                if (hitIteration.InDiscreteHitQueue && offsetedHit > 0)
                {
                    hitIteration.InDiscreteHitQueue = false;
                    Player.Hit(hitIteration, 0);
                }
                else if (!alreadyHit && offsetedHit > window) // Didn't hit the hitobject within the timing window
                {
                    Player.Hit(hitIteration, float.PositiveInfinity, false);
                }


                if (hitIteration.Current.HoldLength > 0)  // Pass to HoldNoteClass if there's hold length
                {
                    HoldQueue.Add(new HoldNoteClass
                    {
                        HitObjectValues = hitIteration,
                        HoldPassDrainValue = 1,
                        IsPlayerHolding = true
                    });

                    HitQueue.RemoveAt(a);
                    a--; // Pointer rollback
                    continue;
                }

                if (offsetedHit < -Math.Max(Player.PassWindow, Player.GoodWindow)) // Too damn early
                {
                    Player.Hit(hitIteration, float.NegativeInfinity, false); // Miss it
                }
            }

            if (HoldQueue.Count > 0 && TouchClasses.Count > 0) // Hold note processor
            {

                //// Camera handling are done here to calculate hold note hitboxes on the fly
                //// As it has dynamic attributes as it progresses, unlike normal hitobjects.

                // Camera handling
                CameraController camera = (CameraController)PlayerScreen.TargetChart.Data.Camera.Get(beat); // Get camera data for the current beat
                // Update transforms
                Player.Pseudocamera.transform.position = camera.CameraPivot;
                Player.Pseudocamera.transform.eulerAngles = camera.CameraRotation;
                Player.Pseudocamera.transform.Translate(Vector3.back * camera.PivotDistance);


                for (int a = 0; a < HoldQueue.Count; a++)
                {
                    HoldNoteClass holdNote_entry = HoldQueue[a];

                    Vector3 startHoldPosition, endHoldPosition;

                    // Note position
                    Lane lane_HoldNote = (Lane)holdNote_entry.HitObjectValues.Lane.Original.Get(beat); // Which lane is the hold note on?
                    LanePosition step = lane_HoldNote.GetLanePosition(beat, beat, PlayerScreen.TargetSong.Timing); // Get the lane position for the current beat
                    startHoldPosition = Quaternion.Euler(lane_HoldNote.Rotation) * step.StartPos + lane_HoldNote.Position;
                    endHoldPosition = Quaternion.Euler(lane_HoldNote.Rotation) * step.EndPos + lane_HoldNote.Position;
                    LaneGroupPlayer currentHold_GroupPlayer = holdNote_entry.HitObjectValues.Lane.Group;

                    // Apply transforms in group
                    while (currentHold_GroupPlayer) // Current LaneGroupPlayer still exists
                    {
                        LaneGroup currentLaneGroup = (LaneGroup)currentHold_GroupPlayer.Original.Get(beat); // Get the current lane group
                        startHoldPosition = currentHold_GroupPlayer.transform.TransformPoint(startHoldPosition); // Apply transform
                        endHoldPosition = currentHold_GroupPlayer.transform.TransformPoint(endHoldPosition);
                        currentHold_GroupPlayer = currentHold_GroupPlayer.Parent; // Go to the parent LaneGroupPlayer
                    }

                    HitObject hitObject = (HitObject)holdNote_entry.HitObjectValues.Original.Get(beat); // Get the hitobject data for the current beat

                    // Calculate hitbox positions
                    // I dunno what lerp is but just go with it I guess
                    Vector2 holdNote_hitbox_start = Player.Pseudocamera.WorldToScreenPoint(
                        Vector3.Lerp(
                            startHoldPosition,
                            endHoldPosition,
                            hitObject.Position
                        )
                    );

                    Vector2 holdNote_hitbox_end = Player.Pseudocamera.WorldToScreenPoint(
                        Vector3.Lerp(
                            startHoldPosition,
                            endHoldPosition,
                            hitObject.Position + hitObject.HoldLength
                        )
                    );

                    holdNote_entry.HitObjectValues.HitCoord.Position = (holdNote_hitbox_start + holdNote_hitbox_end) / 2;
                    holdNote_entry.HitObjectValues.HitCoord.Radius = Vector2.Distance(holdNote_hitbox_start, holdNote_hitbox_end) / 2;

                    // HItbox checker
                    foreach (TouchClass touch in TouchClasses)
                    {
                        if (Vector2.Distance(
                            touch.Touch.screenPosition,
                            holdNote_entry.HitObjectValues.HitCoord.Position
                            ) < holdNote_entry.HitObjectValues.HitCoord.Radius
                        )
                        {
                            holdNote_entry.AssignedTouch = touch; // Assign the touch to the hold note entry
                            holdNote_entry.IsPlayerHolding = true; // Player is holding the note
                        }
                    }

                    // Invalidate holding state
                    if (holdNote_entry.AssignedTouch.Touch.phase is TouchPhase.Ended or TouchPhase.Canceled)
                    {
                        holdNote_entry.IsPlayerHolding = false; // Player is not holding the note anymore
                        holdNote_entry.AssignedTouch = null; // Clear the assigned touch
                    }

                    bool HoldEligible = false; // If the hold note currently eligible for scoring

                    // Update Drain value
                    if (holdNote_entry.IsPlayerHolding)
                    {
                        holdNote_entry.HoldPassDrainValue = Mathf.Clamp01(
                        holdNote_entry.HoldPassDrainValue + Time.deltaTime / Player.PassWindow * .1f
                        );
                    }
                    else
                    {
                        if (holdNote_entry.HoldPassDrainValue > 0)
                        {
                            holdNote_entry.HoldPassDrainValue -= Time.deltaTime / Player.PassWindow * .1f;
                        }
                    }

                    // Check if the hold note is eligible for scoring
                    if (holdNote_entry.HoldPassDrainValue == 0)
                    {
                        HoldEligible = false; // Missed the opportunity to recover :(
                    }
                    else if (holdNote_entry.HoldPassDrainValue > 0)
                    {
                        HoldEligible = true;
                    }


                    // Hold ticks processing
                    while (holdNote_entry.HitObjectValues.HoldTicks.Count > 0 &&
                           holdNote_entry.HitObjectValues.HoldTicks[0] <= Player.CurrentTime)
                    {
                        Player.AddScore(
                            HoldEligible // Score 1 if the HoldEligible is true, otherwise 0
                            ? 1
                            : 0,
                            float.NaN // Hold ticks don't really have 'accuracy' as they are just a part of the hold note
                        );

                        Player.HitObjectHistory.Add(
                            new(
                                holdNote_entry.HitObjectValues.HoldTicks[0],
                                HitObjectHistoryType.Catch,
                                HoldEligible ? 0 : float.PositiveInfinity // Catch note-like hitobject history
                            )
                        );

                        holdNote_entry.HitObjectValues.HoldTicks.RemoveAt(0); // Remove the first hold tick (and pretty much shift the next tick in)

                        // Handle hold tick just like how HitPlayer does
                        if (HoldEligible)
                        {
                            var effect = Instantiate(Player.JudgeScreenSample, Player.JudgeScreenHolder);
                            effect.SetAccuracy(null);
                            effect.SetColor(PlayerScreen.CurrentChart.Palette.InterfaceColor);
                            var rt = (RectTransform)effect.transform;

                            rt.position = Common.main.MainCamera.WorldToScreenPoint(
                                holdNote_entry.HitObjectValues.transform.position
                            );
                        }
                        else
                        {
                            // Missed hold tick, no effect
                        }
                    }

                    if (holdNote_entry.HitObjectValues.HoldTicks.Count <= 0) // No hold ticks left
                    {
                        Player.RemoveHitPlayer(holdNote_entry.HitObjectValues); // Terminate the hold note

                        HoldQueue.RemoveAt(a);
                        a--; // Pointer rollback
                    }

                }
            }
            
            for (int a = 0; a < TouchClasses.Count; a++) // Queued hitobject (non-hold) processor
            {
                TouchClass touch = TouchClasses[a];

                if (
                    touch.QueuedHit && // if the input in question interacted with a hitobject
                    !touch.QueuedHit.IsHit && // Haven't already hit
                                              // And is just a tap note
                    touch.QueuedHit.Current.Type == HitObject.HitType.Normal &&
                    !touch.QueuedHit.Current.Flickable
                )
                {
                    Player.Hit(
                        touch.QueuedHit,
                        (touch.StartTime + Player.Settings.JudgmentOffset) - touch.QueuedHit.Time
                    );
                    touch.Tapped = false; // Tap only lasts for a single frame

                    if (touch.QueuedHit.Current.HoldLength <= 0)
                    {
                        touch.QueuedHit = null; // Clear queued hitobject    
                    }
                }
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
                else if (currentHit.IsHit) // Autoplay's hold note processor
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
    }
}