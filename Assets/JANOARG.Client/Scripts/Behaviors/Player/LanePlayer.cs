using System.Collections.Generic;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Client.Behaviors.Player
{
    /// <summary>
    /// Represent a Lane on the game field.
    /// </summary>
    public class LanePlayer : MonoBehaviour
    {
        public Lane Original;
        public Lane Current;

        [Space]
        public Transform Holder;

        public MeshFilter   MeshFilter;
        public MeshRenderer MeshRenderer;

        public LaneGroupPlayer Group;

        [Space]
        public MeshRenderer JudgeLine;

        public MeshRenderer JudgePointLeft;
        public MeshRenderer JudgePointRight;

        public List<float> PositionPoints = new();
        public List<float> TimeStamps     = new();
        public float       CurrentPosition;

        public List<HitPlayer>      HitObjects = new();
        public List<HitScreenCoord> HitCoords  = new();

        public bool LaneStepDirty = false;
        
        private Mesh          _Mesh;
        private List<Vector3> _Verts = new();
        private List<int>     _Tris  = new();

        public void Init()
        {
            Metronome metronome = PlayerScreen.sTargetSong.Timing;
            foreach (LaneStep step in Current.LaneSteps) TimeStamps.Add(metronome.ToSeconds(step.Offset));

            if (Current.StyleIndex >= 0 && Current.StyleIndex < PlayerScreen.sMain.LaneStyles.Count)
            {
                LaneStyleManager style = PlayerScreen.sMain.LaneStyles[Current.StyleIndex];
                MeshRenderer.sharedMaterial = style.LaneMaterial;

                JudgeLine.sharedMaterial =
                    JudgePointLeft.sharedMaterial =
                        JudgePointRight.sharedMaterial =
                            style.JudgeMaterial;
            }
            else
            {
                MeshRenderer.enabled = false;
                JudgeLine.gameObject.SetActive(false);
                JudgePointLeft.gameObject.SetActive(false);
                JudgePointRight.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Update this lane and its child hit objects.
        /// </summary>
        /// <param name="time">Current song time in seconds.</param>
        /// <param name="beat">Current song time in beats.</param>
        public void UpdateSelf(float time, float beat, float maxDistance = 200)
        {
            if (Current != null)
                Current.Advance(beat);
            else
                Current = (Lane)Original.GetStoryboardableObject(beat);

            UpdateMesh(time, beat);

            transform.localPosition = Current.Position;
            transform.localEulerAngles = Current.Rotation;
            Holder.localPosition = Vector3.back * CurrentPosition;

            if (CurrentPosition - PositionPoints[0] > -200)
            {
                transform.gameObject.SetActive(true);
                UpdateHitObjects(time, beat, maxDistance);
            }
            else
            {
                transform.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Update this lane's mesh and position points.
        /// </summary>
        /// <param name="time">Current song time in seconds.</param>
        /// <param name="beat">Current song time in beats.</param>
        /// <param name="maxDistance">Max distance to render in Unity units.</param>
        private void UpdateMesh(float time, float beat, float maxDistance = 200)
        {
            // New mesh if MeshFilter doesn't have one
            Mesh mesh = MeshFilter.mesh ?? new Mesh();

            _Verts.Clear();
            _Tris.Clear();

            void f_addLine(Vector3 start, Vector3 end)
            {
                // No AddRange here because the alloc overhead adds up
                _Verts.Add(start);
                _Verts.Add(end);

                if (_Verts.Count > 2)
                {
                    _Tris.Add(_Verts.Count - 4);
                    _Tris.Add(_Verts.Count - 2);
                    _Tris.Add(_Verts.Count - 3);
                    _Tris.Add(_Verts.Count - 2);
                    _Tris.Add(_Verts.Count - 1);
                    _Tris.Add(_Verts.Count - 3);
                }
            }

            while (TimeStamps.Count > 2 && TimeStamps[1] < time)
            {
                TimeStamps.RemoveAt(0);
                PositionPoints.RemoveAt(0);
                Current.LaneSteps.RemoveAt(0);
            }

            if (Current.LaneSteps.Count < 1)
            {
                if (TimeStamps[0] < time)
                    Destroy(mesh);

                return;
            }

            Current.LaneSteps[0]
                .Advance(beat);

            if (Current.LaneSteps.Count > 1)
                Current.LaneSteps[1]
                    .Advance(beat);

            Current.LaneSteps[0]
                .Advance(beat);

            if (Current.LaneSteps.Count > 1)
                Current.LaneSteps[1]
                    .Advance(beat);

            CurrentPosition = TimeStamps.Count <= 1 || TimeStamps[0] > time
                ? time * Current.LaneSteps[0].Speed * PlayerScreen.sMain.Speed
                : (time - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed + PositionPoints[0];

            float progress = TimeStamps.Count <= 1
                ? 0
                : Mathf.InverseLerp(TimeStamps[0], TimeStamps[1], time);

            if (PositionPoints.Count <= 1)
                PositionPoints.Add(TimeStamps[0] * Current.LaneSteps[0].Speed * PlayerScreen.sMain.Speed);

            if (TimeStamps.Count <= 1)
                return;

            if (PositionPoints.Count <= 2)
                PositionPoints.Add((TimeStamps[1] - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed + PositionPoints[0]);
            else
                PositionPoints[1] = PositionPoints[0] + (TimeStamps[1] - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed;

            {
                float position = Mathf.Lerp(PositionPoints[0], PositionPoints[1], progress);
                LaneStep currentLaneStep = Current.LaneSteps[1];
                Vector3 startPoint, endPoint;

                if (currentLaneStep.isLinear)
                {
                    startPoint = Vector3.Lerp(Current.LaneSteps[0].StartPointPosition, Current.LaneSteps[1].StartPointPosition, progress) + Vector3.forward * position;
                    endPoint = Vector3.Lerp(Current.LaneSteps[0].EndPointPosition, Current.LaneSteps[1].EndPointPosition, progress) + Vector3.forward * position;
                }
                else
                {
                    startPoint = new Vector3(
                        Mathf.LerpUnclamped(Current.LaneSteps[0].StartPointPosition.x, Current.LaneSteps[1].StartPointPosition.x, currentLaneStep.StartEaseX.Get(progress)),
                        Mathf.LerpUnclamped(Current.LaneSteps[0].StartPointPosition.y, Current.LaneSteps[1].StartPointPosition.y, currentLaneStep.StartEaseY.Get(progress)),
                        position
                    );

                    endPoint = new Vector3(
                        Mathf.LerpUnclamped(Current.LaneSteps[0].EndPointPosition.x, Current.LaneSteps[1].EndPointPosition.x, currentLaneStep.EndEaseX.Get(progress)),
                        Mathf.LerpUnclamped(Current.LaneSteps[0].EndPointPosition.y, Current.LaneSteps[1].EndPointPosition.y, currentLaneStep.EndEaseY.Get(progress)),
                        position
                    );
                }

                f_addLine(startPoint, endPoint);

                JudgeLine.enabled =
                    JudgePointLeft.enabled =
                        JudgePointRight.enabled =
                            TimeStamps.Count >= 2 && time >= TimeStamps[0] && time < TimeStamps[1];

                if (JudgeLine.enabled && JudgeLine.gameObject.activeSelf)
                {
                    JudgeLine.transform.localPosition = (startPoint + endPoint) / 2;
                    JudgeLine.transform.localScale = new Vector3(Vector2.Distance(startPoint, endPoint), .05f, .05f);
                    JudgeLine.transform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, endPoint - startPoint));

                    JudgePointLeft.transform.localPosition = startPoint;
                    JudgePointRight.transform.localPosition = endPoint;
                }

                if (!JudgeLine.enabled)
                    transform.gameObject.SetActive(false);
            }


            if (Current.LaneSteps[0].IsDirty)
            {
                LaneStepDirty = true;
                Current.LaneSteps[0].IsDirty = false;
            }
            if (Current.LaneSteps[1].IsDirty)
            {
                LaneStepDirty = true;
                Current.LaneSteps[1].IsDirty = false;
            }

            for (var currentTimestamp = 1; currentTimestamp < TimeStamps.Count; currentTimestamp++)
            {
                LaneStep currentLaneStep = Current.LaneSteps[currentTimestamp];

                if (currentTimestamp > 1)
                {
                    currentLaneStep.Advance(beat);

                    if (currentLaneStep.IsDirty)
                    {
                        LaneStepDirty = true;
                        currentLaneStep.IsDirty = false;
                    }
                }

                float calculatedPosition = PositionPoints[currentTimestamp - 1] + (TimeStamps[currentTimestamp] - TimeStamps[currentTimestamp - 1]) * currentLaneStep.Speed * PlayerScreen.sMain.Speed;

                if (PositionPoints.Count <= currentTimestamp)
                    PositionPoints.Add(calculatedPosition);
                else
                    PositionPoints[currentTimestamp] = calculatedPosition;

                if (currentLaneStep.isLinear)
                {
                    f_addLine(
                        (Vector3)currentLaneStep.StartPointPosition + Vector3.forward * calculatedPosition,
                        (Vector3)currentLaneStep.EndPointPosition + Vector3.forward * calculatedPosition
                    );
                }
                else
                {
                    LaneStep previousStep = Current.LaneSteps[currentTimestamp - 1];

                    for (float x = Mathf.Floor(progress * 16 + 1.01f) / 16; x <= 1; x = Mathf.Floor(x * 16 + 1.01f) / 16)
                        f_addLine(
                            new Vector3(
                                Mathf.LerpUnclamped(previousStep.StartPointPosition.x, currentLaneStep.StartPointPosition.x, currentLaneStep.StartEaseX.Get(x)),
                                Mathf.LerpUnclamped(previousStep.StartPointPosition.y, currentLaneStep.StartPointPosition.y, currentLaneStep.StartEaseY.Get(x)),
                                Mathf.Lerp(PositionPoints[currentTimestamp - 1], calculatedPosition, x)),
                            new Vector3(
                                Mathf.LerpUnclamped(previousStep.EndPointPosition.x, currentLaneStep.EndPointPosition.x, currentLaneStep.EndEaseX.Get(x)),
                                Mathf.LerpUnclamped(previousStep.EndPointPosition.y, currentLaneStep.EndPointPosition.y, currentLaneStep.EndEaseY.Get(x)),
                                Mathf.Lerp(PositionPoints[currentTimestamp - 1], calculatedPosition, x))
                        );
                }

                progress = 0;

                if (currentTimestamp >= PositionPoints.Count && calculatedPosition - CurrentPosition > 200)
                    break;
            }

            mesh.Clear();
            mesh.SetVertices(_Verts);
            mesh.SetTriangles(_Tris, 0);
            MeshFilter.mesh = mesh;
        }

        private float _HitObjectTime   = float.NaN;
        private int   _HitObjectOffset = 0;

        /// <summary>
        /// Update this lane's hit objects.
        /// </summary>
        /// <param name="time">Current song time in seconds.</param>
        /// <param name="beat">Current song time in beats.</param>
        /// <param name="maxDistance">Max distance to render in Unity units.</param>
        private void UpdateHitObjects(float time, float beat, float maxDistance = 200)
        {
            while (Current.Objects.Count > 0)
            {
                HitObject hit = Current.Objects[0];
                if (float.IsNaN(_HitObjectTime)) _HitObjectTime = PlayerScreen.sTargetSong.Timing.ToSeconds(hit.Offset);

                if (GetZPosition(_HitObjectTime) <= CurrentPosition + maxDistance) // While hit object is in view
                {
                    HitPlayer player = Instantiate(PlayerScreen.sMain.HitSample, Holder);

                    player.Original = Original.Objects[_HitObjectOffset];
                    player.Current = Current.Objects[0];

                    player.Time = _HitObjectTime;
                    player.EndTime = player.Current.HoldLength > 0 ? PlayerScreen.sTargetSong.Timing.ToSeconds(hit.Offset + hit.HoldLength) : _HitObjectTime;
                    player.HitCoord = HitCoords[0];

                    if (player.Current.HoldLength > 0)
                    {
                        for (var a = 0.5f; a < player.Current.HoldLength; a += 0.5f) player.HoldTicks.Add(PlayerScreen.sTargetSong.Timing.ToSeconds(hit.Offset + a));
                        player.HoldTicks.Add(player.EndTime);
                        UpdateHoldMesh(player);
                    }

                    player.Lane = this;
                    HitObjects.Add(player);

                    // PlayerInputManager.main.AddToQueue(player);
                    PlayerInputManager.sInstance.AddToQueue(player);
                    player.Init();

                    Current.Objects.RemoveAt(0);
                    HitCoords.RemoveAt(0);
                    _HitObjectTime = float.NaN;
                    _HitObjectOffset++;
                }
                else
                {
                    break;
                }
            }

            var active = true;

            foreach (HitPlayer hitObject in HitObjects)
            {
                if (active)
                    hitObject
                        .UpdateSelf(time, beat, LaneStepDirty);

                if (active && hitObject.CurrentPosition > CurrentPosition + 200)
                    active = false;

                hitObject.gameObject.SetActive(active);

                if (hitObject.HoldMesh)
                    hitObject.HoldMesh.gameObject.SetActive(active);
            }

            LaneStepDirty = false;
        }

        /// <summary>
        /// Converts a given song time to the corresponding Z position on this lane.
        /// </summary>
        /// <remarks>
        /// This function uses the internal current time as storyboard time.
        /// </remarks>
        /// <param name="time">Song time in seconds.</param>
        /// <returns>The Z position the judgement line will be at the given song time.</returns>
        public float GetZPosition(float time)
        {
            int index = TimeStamps.FindIndex(x => x >= time);

            if (index < 0)
                return PositionPoints[^1] + (time - TimeStamps[PositionPoints.Count - 1]) * Current.LaneSteps[PositionPoints.Count - 1].Speed * PlayerScreen.sMain.Speed;

            index = Mathf.Min(index, PositionPoints.Count - 1);

            return PositionPoints[index] + (time - TimeStamps[index]) * Current.LaneSteps[index].Speed * PlayerScreen.sMain.Speed;
        }

        /// <summary>
        /// Calculates this lane's judgment line's start and end position at the given song time.
        /// </summary>
        /// <remarks>
        /// This function uses the internal current time as storyboard time.
        /// </remarks>
        /// <param name="time">Song time in seconds.</param>
        /// <param name="start">The resulting start position.</param>
        /// <param name="end">The resulting end position.</param>
        public void GetStartEndPosition(float time, out Vector2 start, out Vector2 end)
        {
            int index = TimeStamps.FindIndex(x => x >= time);

            if (index < 0)
            {
                start = Current.LaneSteps[^1].StartPointPosition;
                end = Current.LaneSteps[^1].EndPointPosition;
            }
            else if (index == 0)
            {
                start = Current.LaneSteps[0].StartPointPosition;
                end = Current.LaneSteps[0].EndPointPosition;
            }
            else
            {
                LaneStep currentStep = Current.LaneSteps[index];
                LaneStep previousStep = Current.LaneSteps[index - 1];
                float progress = Mathf.InverseLerp(TimeStamps[index - 1], TimeStamps[index], time);

                if (currentStep.isLinear)
                {
                    start = Vector2.Lerp(previousStep.StartPointPosition, currentStep.StartPointPosition, progress);
                    end = Vector2.Lerp(previousStep.EndPointPosition, currentStep.EndPointPosition, progress);
                }
                else
                {
                    start = new Vector2(
                        Mathf.LerpUnclamped(previousStep.StartPointPosition.x, currentStep.StartPointPosition.x, currentStep.StartEaseX.Get(progress)),
                        Mathf.LerpUnclamped(previousStep.StartPointPosition.y, currentStep.StartPointPosition.y, currentStep.StartEaseY.Get(progress)));

                    end = new Vector2(
                        Mathf.LerpUnclamped(previousStep.EndPointPosition.x, currentStep.EndPointPosition.x, currentStep.EndEaseX.Get(progress)),
                        Mathf.LerpUnclamped(previousStep.EndPointPosition.y, currentStep.EndPointPosition.y, currentStep.EndEaseY.Get(progress)));
                }
            }
        }


        /// <summary>
        /// Updates a given hit object's hold tail mesh.
        /// </summary>
        /// <param name="hit">The hit object to update the hold tail.</param>
        public void UpdateHoldMesh(HitPlayer hit)
        {
            if (hit.HoldRenderer == null)
            {
                hit.HoldRenderer = Instantiate(PlayerScreen.sMain.HoldSample, Holder);
                hit.HoldMesh = hit.HoldRenderer.GetComponent<MeshFilter>();
            }

            if (hit.HoldMesh.mesh == null) hit.HoldMesh.mesh = new Mesh();

            Mesh mesh = hit.HoldMesh.mesh;

            _Verts.Clear();
            _Tris.Clear();

            void f_addLine(Vector3 start, Vector3 end)
            {
                // No AddRange here because the alloc overhead adds up
                _Verts.Add(start);
                _Verts.Add(end);

                if (_Verts.Count > 2)
                {
                    _Tris.Add(_Verts.Count - 4);
                    _Tris.Add(_Verts.Count - 2);
                    _Tris.Add(_Verts.Count - 3);
                    _Tris.Add(_Verts.Count - 2);
                    _Tris.Add(_Verts.Count - 1);
                    _Tris.Add(_Verts.Count - 3);
                }
            }

            float time = Mathf.Max(PlayerScreen.sMain.CurrentTime + PlayerScreen.sMain.Settings.VisualOffset, hit.Time);

            int index = TimeStamps.FindIndex(x => x > time);

            if (index <= 0 || PositionPoints.Count <= index) return;

            index = Mathf.Max(index, 1);

            float progress = TimeStamps.Count <= 1 ? 0 : Mathf.InverseLerp(TimeStamps[index - 1], TimeStamps[index], time);
            Vector3 previousStepStartPointPosition, previousStepEndPointPosition, currentStepStartPointPosition, currentStepEndPointPosition;

            {
                float position = Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], progress);
                LaneStep previousStep = Current.LaneSteps[index - 1];
                LaneStep currentStep = Current.LaneSteps[index];

                previousStepStartPointPosition = Vector3.LerpUnclamped(previousStep.StartPointPosition, previousStep.EndPointPosition, hit.Current.Position);
                previousStepEndPointPosition = Vector3.LerpUnclamped(previousStep.StartPointPosition, previousStep.EndPointPosition, hit.Current.Position + hit.Current.Length);
                currentStepStartPointPosition = Vector3.LerpUnclamped(currentStep.StartPointPosition, currentStep.EndPointPosition, hit.Current.Position);
                currentStepEndPointPosition = Vector3.LerpUnclamped(currentStep.StartPointPosition, currentStep.EndPointPosition, hit.Current.Position + hit.Current.Length);

                if (currentStep.isLinear)
                    f_addLine(
                        Vector3.Lerp(previousStepStartPointPosition, currentStepStartPointPosition, progress) + Vector3.forward * position,
                        Vector3.Lerp(previousStepEndPointPosition, currentStepEndPointPosition, progress) + Vector3.forward * position);
                else
                    f_addLine(
                        new Vector3(
                            Mathf.LerpUnclamped(previousStepStartPointPosition.x, currentStepStartPointPosition.x, currentStep.StartEaseX.Get(progress)),
                            Mathf.LerpUnclamped(previousStepStartPointPosition.y, currentStepStartPointPosition.y, currentStep.StartEaseY.Get(progress)),
                            position),
                        new Vector3(
                            Mathf.LerpUnclamped(previousStepEndPointPosition.x, currentStepEndPointPosition.x, currentStep.EndEaseX.Get(progress)),
                            Mathf.LerpUnclamped(previousStepEndPointPosition.y, currentStepEndPointPosition.y, currentStep.EndEaseY.Get(progress)),
                            position)
                    );
            }

            var failsafeIteration = 0;

            for (; index < Mathf.Min(PositionPoints.Count, TimeStamps.Count); index++)
            {
                float endStepProgress = InverseLerpUnclamped(TimeStamps[index - 1], TimeStamps[index], hit.EndTime);
                float endStepPosition = Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], endStepProgress);
                LaneStep currentStep = Current.LaneSteps[index];

                currentStepStartPointPosition = Vector3.LerpUnclamped(currentStep.StartPointPosition, currentStep.EndPointPosition, hit.Current.Position);
                currentStepEndPointPosition = Vector3.LerpUnclamped(currentStep.StartPointPosition, currentStep.EndPointPosition, hit.Current.Position + hit.Current.Length);

                if (currentStep.isLinear)
                {
                    f_addLine(
                        Vector3.Lerp(previousStepStartPointPosition, currentStepStartPointPosition, endStepProgress) + Vector3.forward * endStepPosition,
                        Vector3.Lerp(previousStepEndPointPosition, currentStepEndPointPosition, endStepProgress) + Vector3.forward * endStepPosition
                    );
                }
                else
                {
                    LaneStep previousStep = Current.LaneSteps[index - 1];

                    for (float x = Mathf.Floor(progress * 16 + 1.01f) / 16;; x = Mathf.Min(endStepProgress, Mathf.Floor(x * 16 + 1.01f) / 16))
                    {
                        f_addLine(
                            new Vector3(
                                Mathf.LerpUnclamped(previousStepStartPointPosition.x, currentStepStartPointPosition.x, currentStep.StartEaseX.Get(x)),
                                Mathf.LerpUnclamped(previousStepStartPointPosition.y, currentStepStartPointPosition.y, currentStep.StartEaseY.Get(x)),
                                Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], x)),
                            new Vector3(
                                Mathf.LerpUnclamped(previousStepEndPointPosition.x, currentStepEndPointPosition.x, currentStep.EndEaseX.Get(x)),
                                Mathf.LerpUnclamped(previousStepEndPointPosition.y, currentStepEndPointPosition.y, currentStep.EndEaseY.Get(x)),
                                Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], x))
                        );

                        if (x >= Mathf.Min(endStepProgress, 1))
                            break;
                    }
                }

                progress = 0;

                failsafeIteration++;

                if (endStepProgress <= 1 || failsafeIteration > 1000)
                    break;

                previousStepStartPointPosition = currentStepStartPointPosition;
                previousStepEndPointPosition = currentStepEndPointPosition;
            }

            mesh.Clear();
            mesh.SetVertices(_Verts);
            mesh.SetTriangles(_Tris, 0);
            hit.HoldMesh.mesh = mesh;
        }

        private float InverseLerpUnclamped(float start, float end, float val)
        {
            return (val - start) / (end - start);
        }
    }

    [System.Serializable]
    public struct HitScreenCoord
    {
        public Vector2 Position;
        public float   Radius;
    }
}