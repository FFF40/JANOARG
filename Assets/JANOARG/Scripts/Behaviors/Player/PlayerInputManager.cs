using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Unity.VisualScripting;

public class PlayerInputManager : UnityEngine.MonoBehaviour
{
    public static PlayerInputManager main;

    public PlayerScreen Player;
    [Space]
    public bool Autoplay;

    [HideInInspector]
    public List<FingerHandler> Fingers = new();
    [HideInInspector]
    public List<HitPlayer> HitQueue = new();
    [HideInInspector]
    public List<HoldHandler> HoldQueue = new();

    public void Awake()
    {
        EnhancedTouchSupport.Enable();
        main = this;
    }

    public void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }

    public void UpdateTouches()
    {
        if (Autoplay)
        {
            for (int a = 0; a < HitQueue.Count; a++)
            {
                HitPlayer hit = HitQueue[a];
                if (!hit)
                {
                    HitQueue.RemoveAt(a);
                    a--;
                }
                else if (hit.IsHit)
                {
                    while (hit.HoldTicks.Count > 0 && hit.HoldTicks[0] <= Player.CurrentTime)
                    {
                        Player.AddScore(1, null);
                        Player.HitObjectHistory.Add(new(hit.HoldTicks[0], HitObjectHistoryType.Catch, 0));
                        hit.HoldTicks.RemoveAt(0);

                        var effect = Instantiate(Player.JudgeScreenSample, Player.JudgeScreenHolder);
                        effect.SetAccuracy(null);
                        effect.SetColor(PlayerScreen.CurrentChart.Palette.InterfaceColor);
                        var rt = (RectTransform)effect.transform;
                        rt.position = Common.main.MainCamera.WorldToScreenPoint(hit.transform.position);
                    }
                    if (hit.HoldTicks.Count <= 0)
                    {
                        Player.RemoveHitPlayer(hit);
                        HitQueue.RemoveAt(a);
                        a--;
                    }
                }
                else if (hit.Time <= Player.CurrentTime)
                {
                    Player.Hit(hit, 0);
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            int fingerCount = Touch.activeFingers.Count;
            float dpi = Screen.dpi > 0 ? Screen.dpi : 200;
            float flickThres = dpi * 0.2f;

            for (int a = 0; a < fingerCount; a++)
            {
                Finger finger = Touch.activeFingers[a];
                if (!finger.isActive) continue;
                Touch touch = finger.currentTouch;
                if (Fingers.Find(x => x.Finger.index == finger.index) == null)
                {
                    Fingers.Add(new FingerHandler
                    {
                        Finger = finger,
                        StartTime = (float)touch.startTime - Time.realtimeSinceStartup + Player.CurrentTime,
                        FlickCenter = touch.startScreenPosition,
                    });
                }
            }

            for (int a = 0; a < Fingers.Count; a++)
            {
                var finger = Fingers[a];
                Touch touch = finger.Finger.currentTouch;
                if (!touch.inProgress) continue;
                if (touch.phase == TouchPhase.Moved)
                {
                    if (Vector2.Distance(touch.screenPosition, finger.FlickCenter) > flickThres)
                    {
                        finger.FlickEligible = true;
                        finger.FlickTime = Player.CurrentTime;
                        finger.FlickDirection = Vector2.SignedAngle(Vector2.up, touch.screenPosition - finger.FlickCenter);
                        finger.FlickCenter = touch.screenPosition;
                    }
                    if (Player.CurrentTime - finger.FlickTime > Player.PassWindow)
                    {
                        finger.FlickEligible = false;
                    }
                }
            }

            float judgTime = Player.CurrentTime + Player.Settings.JudgmentOffset;

            for (int a = 0; a < HitQueue.Count; a++)
            {
                HitPlayer hit = HitQueue[a];
                if (!hit)
                {
                    HitQueue.RemoveAt(a);
                    a--;
                    continue;
                }

                float offset = judgTime - hit.Time;
                bool isDiscrete = (hit.Current.Type == HitObject.HitType.Catch) || hit.Current.Flickable;
                float window = isDiscrete ? Player.PassWindow : Player.GoodWindow;

                bool isHit = false;

                if (offset >= -window)
                {
                    if (hit.IsHit)
                    {

                    }
                    else
                    {
                        if (isDiscrete)
                        {
                            if (hit.IsHit || hit.IsQueuedHit)
                            {

                            }
                            else if (hit.Current.Flickable)
                            {
                                float distance = 0;
                                bool IsInRange(Vector2 screenPos) =>
                                    hit.Current.Type == HitObject.HitType.Normal && float.IsFinite(hit.Current.FlickDirection)
                                        ? (distance = Mathf.Abs((Quaternion.Euler(0, 0, hit.Current.FlickDirection) * (screenPos - hit.HitCoord.Position)).x)) < hit.HitCoord.Radius
                                        : (distance = Vector2.Distance(screenPos, hit.HitCoord.Position)) < hit.HitCoord.Radius + dpi * 1f;

                                if (hit.Current.Type == HitObject.HitType.Normal && !hit.IsTapped)
                                {
                                    foreach (FingerHandler finger in Fingers)
                                    {
                                        float timeDiff = 0;
                                        if
                                        (
                                            finger.TapEligible &&
                                            IsInRange(finger.Finger.screenPosition) &&
                                            (!finger.QueuedHit ||
                                                (timeDiff = hit.Time - finger.QueuedHit.Time) < -0.0001f ||
                                                (timeDiff < 0.0001f && distance < finger.QueuedHitDistance)
                                            )
                                        )
                                        {
                                            finger.QueuedHit = hit;
                                            finger.QueuedHitDistance = distance;
                                            hit.IsTapped = true;
                                            isHit = true;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (FingerHandler finger in Fingers)
                                    {

                                        if
                                        (
                                            finger.FlickEligible &&
                                            ((hit.Current.Type == HitObject.HitType.Normal) ? finger.QueuedHit == hit : IsInRange(finger.Finger.screenPosition)) &&
                                            (!float.IsFinite(hit.Current.FlickDirection) || CheckFlickDirection(hit.Current.FlickDirection, finger.FlickDirection))
                                        )
                                        {
                                            hit.IsQueuedHit = true;
                                            finger.FlickEligible = false;
                                            isHit = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (FingerHandler finger in Fingers)
                                {
                                    if
                                    (
                                        Vector2.Distance(finger.Finger.screenPosition, hit.HitCoord.Position) < hit.HitCoord.Radius
                                    )
                                    {
                                        hit.IsQueuedHit = true;
                                        isHit = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (FingerHandler finger in Fingers)
                            {
                                float distance = 0, timeDiff = 0;
                                if
                                (
                                    finger.TapEligible &&
                                    (distance = Vector2.Distance(finger.Finger.screenPosition, hit.HitCoord.Position)) < hit.HitCoord.Radius &&
                                    (!finger.QueuedHit ||
                                        (timeDiff = hit.Time - finger.QueuedHit.Time) < -0.0001f ||
                                        (timeDiff < 0.0001f && distance < finger.QueuedHitDistance)
                                    )
                                )
                                {
                                    finger.QueuedHitDistance = distance;
                                    finger.QueuedHit = hit;
                                    isHit = true;
                                }
                            }
                        }

                        if (hit.IsQueuedHit && offset > 0)
                        {
                            hit.IsQueuedHit = false;
                            Player.Hit(hit, 0);
                        }
                        else if (!isHit && offset > window)
                        {
                            Player.Hit(hit, float.PositiveInfinity, false);
                        }

                        if (hit.IsHit)
                        {
                            HoldQueue.Add(new HoldHandler
                            {
                                Hit = hit,
                            });
                            HitQueue.RemoveAt(a);
                            a--;
                        }
                    }
                }

                if (offset < -Mathf.Max(Player.PassWindow, Player.GoodWindow))
                {
                    break;
                }
            }

            // Process hold notes 
            if (HoldQueue.Count > 0)
            {
                float time = Player.CurrentTime + Player.Settings.JudgmentOffset;
                float beat = PlayerScreen.TargetSong.Timing.ToBeat(time);

                CameraController camera = (CameraController)PlayerScreen.TargetChart.Data.Camera.Get(beat);
                Player.Pseudocamera.transform.position = camera.CameraPivot;
                Player.Pseudocamera.transform.eulerAngles = camera.CameraRotation;
                Player.Pseudocamera.transform.Translate(Vector3.back * camera.PivotDistance);

                for (int a = 0; a < HoldQueue.Count; a++)
                {
                    var hold = HoldQueue[a];
                    Vector3 startPos, endPos;

                    // Calculate note position
                    Lane lane = (Lane)hold.Hit.Lane.Original.Get(beat);
                    LanePosition step = lane.GetLanePosition(beat, beat, PlayerScreen.TargetSong.Timing);
                    startPos = Quaternion.Euler(lane.Rotation) * step.StartPos + lane.Position;
                    endPos = Quaternion.Euler(lane.Rotation) * step.EndPos + lane.Position;
                    LaneGroupPlayer gp = hold.Hit.Lane.Group;
                    while (gp)
                    {
                        LaneGroup laneGroup = (LaneGroup)gp.Original.Get(beat);
                        startPos = Quaternion.Euler(laneGroup.Rotation) * startPos + laneGroup.Position;
                        endPos = Quaternion.Euler(laneGroup.Rotation) * endPos + laneGroup.Position;
                        gp = gp.Parent;
                    }

                    HitObject hit = (HitObject)hold.Hit.Original.Get(beat);
                    Vector2 hitStart = Player.Pseudocamera.WorldToScreenPoint(Vector3.Lerp(startPos, endPos, hit.Position));
                    Vector2 hitEnd = Player.Pseudocamera.WorldToScreenPoint(Vector3.Lerp(startPos, endPos, hit.Position + hit.Length));
                    hold.Hit.HitCoord.Position = (hitStart + hitEnd) / 2;
                    hold.Hit.HitCoord.Radius = Vector2.Distance(hitStart, hitEnd) / 2;

                    // Determine if the player is currently holding the note
                    if (hold.IsHolding)
                    {
                        bool isHoldingNow = false;
                        foreach (FingerHandler finger in Fingers)
                        {
                            if
                            (
                                Vector2.Distance(finger.Finger.screenPosition, hold.Hit.HitCoord.Position) < hold.Hit.HitCoord.Radius
                            )
                            {
                                isHoldingNow = true;
                                break;
                            }
                        }

                        if (isHoldingNow)
                        {
                            hold.HoldValue = Mathf.Clamp01(hold.HoldValue + Time.deltaTime / Player.PassWindow * .1f);
                        }
                        else
                        {
                            hold.HoldValue = hold.HoldValue - Time.deltaTime / Player.PassWindow;
                            if (hold.HoldValue <= 0) hold.IsHolding = false;
                        }
                    }
                    else
                    {
                        if (hold.HoldValue >= 1)
                        {
                            foreach (FingerHandler finger in Fingers)
                            {
                                if
                                (
                                    Vector2.Distance(finger.Finger.screenPosition, hold.Hit.HitCoord.Position) < hold.Hit.HitCoord.Radius
                                )
                                {
                                    hold.IsHolding = true;
                                    break;
                                }
                            }
                        }
                    }

                    while (hold.Hit.HoldTicks.Count > 0 && hold.Hit.HoldTicks[0] <= Player.CurrentTime)
                    {
                        Player.AddScore(hold.IsHolding ? 1 : 0, null);
                        Player.HitObjectHistory.Add(new(hold.Hit.HoldTicks[0], HitObjectHistoryType.Catch, hold.IsHolding ? 0 : float.PositiveInfinity));
                        hold.Hit.HoldTicks.RemoveAt(0);
                        if (hold.IsHolding)
                        {
                            var effect = Instantiate(Player.JudgeScreenSample, Player.JudgeScreenHolder);
                            effect.SetAccuracy(null);
                            effect.SetColor(PlayerScreen.CurrentChart.Palette.InterfaceColor);
                            var rt = (RectTransform)effect.transform;
                            rt.position = hold.Hit.HitCoord.Position;
                        }
                        else
                        {
                            hold.HoldValue = 1;
                        }
                    }

                    if (hold.Hit.HoldTicks.Count <= 0)
                    {
                        Player.RemoveHitPlayer(hold.Hit);
                        HoldQueue.RemoveAt(a);
                        a--;
                    }
                }
            }

            // Process queued hit objects
            for (int a = 0; a < Fingers.Count; a++)
            {
                var finger = Fingers[a];
                if (
                    finger.QueuedHit &&
                    !finger.QueuedHit.IsHit &&
                    finger.QueuedHit.Current.Type == HitObject.HitType.Normal &&
                    !finger.QueuedHit.Current.Flickable
                )
                {
                    Player.Hit(finger.QueuedHit, finger.StartTime + Player.Settings.JudgmentOffset - finger.QueuedHit.Time);

                    if (finger.QueuedHit.IsHit)
                    {
                        HoldQueue.Add(new HoldHandler
                        {
                            Hit = finger.QueuedHit,
                            IsHolding = true,
                            HoldValue = 1,
                        });
                        HitQueue.Remove(finger.QueuedHit);
                    }
                    finger.QueuedHit.IsHit = true;
                    finger.QueuedHit = null;
                }
                finger.TapEligible = false;

                var touch = finger.Finger.currentTouch;
                if (touch.phase is TouchPhase.Ended or TouchPhase.Canceled)
                {
                    Fingers.RemoveAt(a);
                    a--;
                }
            }
        }
    }

    public void AddToQueue(HitPlayer hit)
    {
        int index = HitQueue.FindLastIndex(x => x.Time < hit.Time);
        HitQueue.Insert(index + 1, hit);
    }

    public bool CheckFlickDirection(float expected, float actual)
    {
        float dist = ((expected - actual) % 360 + 360) % 360;
        return dist < 25 || dist > 335;
    }
}

public class FingerHandler
{
    public Finger Finger;
    public float StartTime;
    public bool TapEligible = true;
    public bool FlickEligible;
    public float FlickDirection;
    public float FlickTime;
    public Vector2 FlickCenter;
    public HitPlayer QueuedHit;
    public float QueuedHitDistance;
}

public class HoldHandler
{
    public HitPlayer Hit;
    public bool IsHolding;
    public float HoldValue;
}