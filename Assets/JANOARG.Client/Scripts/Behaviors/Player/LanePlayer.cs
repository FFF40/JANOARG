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
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;
        public LaneGroupPlayer Group;
        [Space]
        public MeshRenderer JudgeLine;
        [FormerlySerializedAs("JudgeLeft")] public MeshRenderer JudgePointLeft;
        [FormerlySerializedAs("JudgeRight")] public MeshRenderer JudgePointRight;

        public List<float> PositionPoints = new();
        public List<float> TimeStamps = new();
        public float CurrentPosition;

        public List<HitPlayer> HitObjects = new();
        public List<HitScreenCoord> HitCoords = new();

        public bool LaneStepDirty = false;

        private List<Vector3> _verts = new();
        private List<int> _tris = new();

        /// <summary>
        /// Initialize this lane, should be called when the lane is created.
        /// </summary>
        public void Init()
        {
            var metronome = PlayerScreen.TargetSong.Timing;
            foreach (LaneStep step in Current.LaneSteps)
            {
                TimeStamps.Add(metronome.ToSeconds(step.Offset));
            }
            if (Current.StyleIndex >= 0 && Current.StyleIndex < PlayerScreen.main.LaneStyles.Count)
            {
                LaneStyleManager style = PlayerScreen.main.LaneStyles[Current.StyleIndex];
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
            // Get the storyboarded lane
            if (Current != null)
                Current.Advance(beat);
            else
                Current = (Lane)Original.GetStoryboardableObject(beat);

            UpdateMesh(time, beat);

            // Update transform
            transform.localPosition = Current.Position;
            transform.localEulerAngles = Current.Rotation;
            Holder.localPosition = Vector3.back * CurrentPosition;

            // Update hit objects
            if (CurrentPosition - PositionPoints[0] > -maxDistance) // If lane is in view
            {
                transform.gameObject.SetActive(true);
                UpdateHitObjects(time, beat, maxDistance);
            }
            else
                transform.gameObject.SetActive(false);
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

            _verts.Clear();
            _tris.Clear();

            void AddLine(Vector3 start, Vector3 end)
            {
                // No AddRange here because the alloc overhead adds up
                _verts.Add(start);
                _verts.Add(end);

                if (_verts.Count > 2)
                {
                    _tris.Add(_verts.Count - 4);
                    _tris.Add(_verts.Count - 2);
                    _tris.Add(_verts.Count - 3);
                    _tris.Add(_verts.Count - 2);
                    _tris.Add(_verts.Count - 1);
                    _tris.Add(_verts.Count - 3);
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

            Current.LaneSteps[0].Advance(beat);

            if (Current.LaneSteps.Count > 1)
                Current.LaneSteps[1].Advance(beat);

            CurrentPosition = (TimeStamps.Count <= 1 || TimeStamps[0] > time)
                ? time * Current.LaneSteps[0].Speed * PlayerScreen.main.Speed
                : (time - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.main.Speed + PositionPoints[0];

            float progress = TimeStamps.Count <= 1
                ? 0
                : Mathf.InverseLerp(TimeStamps[0], TimeStamps[1], time);

            if (PositionPoints.Count <= 1)
                PositionPoints.Add(TimeStamps[0] * Current.LaneSteps[0].Speed * PlayerScreen.main.Speed);

            if (TimeStamps.Count <= 1)
                return;

            if (PositionPoints.Count <= 2)
                PositionPoints.Add((TimeStamps[1] - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.main.Speed + PositionPoints[0]);
            else
                PositionPoints[1] = PositionPoints[0] + (TimeStamps[1] - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.main.Speed;

            {

                float position = Mathf.Lerp(PositionPoints[0], PositionPoints[1], progress);
                LaneStep currentLaneStep = Current.LaneSteps[1];
                Vector3 start, end;

                if (currentLaneStep.IsLinear)
                {
                    start = Vector3.Lerp(Current.LaneSteps[0].StartPos, Current.LaneSteps[1].StartPos, progress) + Vector3.forward * position;
                    end = Vector3.Lerp(Current.LaneSteps[0].EndPos, Current.LaneSteps[1].EndPos, progress) + Vector3.forward * position;
                }
                else
                {
                    start = new Vector3(
                        Mathf.LerpUnclamped(Current.LaneSteps[0].StartPos.x, Current.LaneSteps[1].StartPos.x, currentLaneStep.StartEaseX.Get(progress)),
                        Mathf.LerpUnclamped(Current.LaneSteps[0].StartPos.y, Current.LaneSteps[1].StartPos.y, currentLaneStep.StartEaseY.Get(progress)),
                        position
                    );
                    end = new Vector3(
                        Mathf.LerpUnclamped(Current.LaneSteps[0].EndPos.x, Current.LaneSteps[1].EndPos.x, currentLaneStep.EndEaseX.Get(progress)),
                        Mathf.LerpUnclamped(Current.LaneSteps[0].EndPos.y, Current.LaneSteps[1].EndPos.y, currentLaneStep.EndEaseY.Get(progress)),
                        position
                    );
                }

                AddLine(start, end);

                JudgeLine.enabled =
                    JudgePointLeft.enabled =
                        JudgePointRight.enabled =
                            TimeStamps.Count >= 2 && time >= TimeStamps[0] && time < TimeStamps[1];

                if (JudgeLine.enabled && JudgeLine.gameObject.activeSelf)
                {
                    JudgeLine.transform.localPosition = (start + end) / 2;
                    JudgeLine.transform.localScale = new Vector3(Vector2.Distance(start, end), .05f, .05f);
                    JudgeLine.transform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, end - start));

                    JudgePointLeft.transform.localPosition = start;
                    JudgePointRight.transform.localPosition = end;
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

            for (int currentTimestamp = 1; currentTimestamp < TimeStamps.Count; currentTimestamp++)
            {
                var currentLaneStep = Current.LaneSteps[currentTimestamp];
                if (currentTimestamp > 1)
                {
                    currentLaneStep.Advance(beat);
                    if (currentLaneStep.IsDirty)
                    {
                        LaneStepDirty = true;
                        currentLaneStep.IsDirty = false;
                    }
                }
                float calculatedPosition = PositionPoints[currentTimestamp - 1] + (TimeStamps[currentTimestamp] - TimeStamps[currentTimestamp - 1]) * currentLaneStep.Speed * PlayerScreen.main.Speed;

                if (PositionPoints.Count <= currentTimestamp)
                    PositionPoints.Add(calculatedPosition);
                else
                    PositionPoints[currentTimestamp] = calculatedPosition;

                if (currentLaneStep.IsLinear)
                {
                    AddLine(
                        (Vector3)currentLaneStep.StartPos + Vector3.forward * calculatedPosition,
                        (Vector3)currentLaneStep.EndPos + Vector3.forward * calculatedPosition
                    );
                }
                else
                {
                    var previousStep = Current.LaneSteps[currentTimestamp - 1];
                    for (float x = Mathf.Floor(progress * 16 + 1.01f) / 16; x <= 1; x = Mathf.Floor(x * 16 + 1.01f) / 16)
                    {
                        AddLine(
                            new Vector3(
                                Mathf.LerpUnclamped(previousStep.StartPos.x, currentLaneStep.StartPos.x, currentLaneStep.StartEaseX.Get(x)),
                                Mathf.LerpUnclamped(previousStep.StartPos.y, currentLaneStep.StartPos.y, currentLaneStep.StartEaseY.Get(x)),
                                Mathf.Lerp(PositionPoints[currentTimestamp - 1], calculatedPosition, x)),

                            new Vector3(
                                Mathf.LerpUnclamped(previousStep.EndPos.x, currentLaneStep.EndPos.x, currentLaneStep.EndEaseX.Get(x)),
                                Mathf.LerpUnclamped(previousStep.EndPos.y, currentLaneStep.EndPos.y, currentLaneStep.EndEaseY.Get(x)),
                                Mathf.Lerp(PositionPoints[currentTimestamp - 1], calculatedPosition, x))
                        );
                    }
                }
                progress = 0;

                if (currentTimestamp >= PositionPoints.Count && calculatedPosition - CurrentPosition > 200)
                    break;
            }

            mesh.Clear();
            mesh.SetVertices(_verts);
            mesh.SetTriangles(_tris, 0);
            MeshFilter.mesh = mesh;
        }

        private float _hitObjectTime = float.NaN;
        private int _hitObjectOffset = 0;

        private void UpdateHitObjects(float time, float beat, float maxDistance = 200)
        {
            while (Current.Objects.Count > 0)
            {
                HitObject hit = Current.Objects[0];
                if (float.IsNaN(_hitObjectTime)) _hitObjectTime = PlayerScreen.TargetSong.Timing.ToSeconds(hit.Offset);
                if (GetZPosition(_hitObjectTime) <= CurrentPosition + maxDistance)
                {
                    HitPlayer player = Instantiate(PlayerScreen.main.HitSample, Holder);

                    player.Original = Original.Objects[_hitObjectOffset];
                    player.Current = Current.Objects[0];

                    player.Time = _hitObjectTime;
                    player.EndTime = player.Current.HoldLength > 0 ? PlayerScreen.TargetSong.Timing.ToSeconds(hit.Offset + hit.HoldLength) : _hitObjectTime;
                    player.HitCoord = HitCoords[0];
                    if (player.Current.HoldLength > 0)
                    {
                        for (float a = 0.5f; a < player.Current.HoldLength; a += 0.5f) player.HoldTicks.Add(PlayerScreen.TargetSong.Timing.ToSeconds(hit.Offset + a));
                        player.HoldTicks.Add(player.EndTime);
                        UpdateHoldMesh(player);
                    }

                    player.Lane = this;
                    HitObjects.Add(player);
                    //PlayerInputManager.main.AddToQueue(player);
                    PlayerInputManager.Instance.AddToQueue(player);
                    player.Init();

                    Current.Objects.RemoveAt(0);
                    HitCoords.RemoveAt(0);
                    _hitObjectTime = float.NaN;
                    _hitObjectOffset++;
                }
                else
                {
                    break;
                }
            }
            bool active = true;
            for (int a = 0; a < HitObjects.Count; a++)
            {
                if (active) HitObjects[a].UpdateSelf(time, beat, LaneStepDirty);
                if (active && HitObjects[a].CurrentPosition > CurrentPosition + 200) active = false;
                HitObjects[a].gameObject.SetActive(active);
                if (HitObjects[a].HoldMesh) HitObjects[a].HoldMesh.gameObject.SetActive(active);
            }
            LaneStepDirty = false;
        }

        /// <summary>
        /// Get the Z position at a given real time.
        /// </summary>
        /// <param name="time">The real time position, in seconds.</param>
        /// <returns>The Z position converted from the real time position.</returns>
        public float GetZPosition(float time)
        {
            int index = TimeStamps.FindIndex(x => x >= time);
            if (index < 0) return PositionPoints[^1] + (time - TimeStamps[PositionPoints.Count - 1]) * Current.LaneSteps[PositionPoints.Count - 1].Speed * PlayerScreen.main.Speed;
            index = Mathf.Min(index, PositionPoints.Count - 1);
            return PositionPoints[index] + (time - TimeStamps[index]) * Current.LaneSteps[index].Speed * PlayerScreen.main.Speed;
        }

        /// <summary>
        /// Get the start and end position of the lane at a given real time.
        /// </summary>
        /// <param name="time">The real time position, in seconds.</param>
        /// <param name="start">The start position of the lane.</param>
        /// <param name="end">The end position of the lane.</param>
        public void GetStartEndPosition(float time, out Vector2 start, out Vector2 end)
        {
            int index = TimeStamps.FindIndex(x => x >= time);
            if (index < 0)
            {
                start = Current.LaneSteps[^1].StartPos;
                end = Current.LaneSteps[^1].EndPos;
            }
            else if (index == 0)
            {
                start = Current.LaneSteps[0].StartPos;
                end = Current.LaneSteps[0].EndPos;
            }
            else
            {
                var cur = Current.LaneSteps[index];
                var pre = Current.LaneSteps[index - 1];
                float progress = Mathf.InverseLerp(TimeStamps[index - 1], TimeStamps[index], time);
                if (cur.IsLinear)
                {
                    start = Vector2.Lerp(pre.StartPos, cur.StartPos, progress);
                    end = Vector2.Lerp(pre.EndPos, cur.EndPos, progress);
                }
                else
                {
                    start = new Vector2(
                        Mathf.LerpUnclamped(pre.StartPos.x, cur.StartPos.x, cur.StartEaseX.Get(progress)),
                        Mathf.LerpUnclamped(pre.StartPos.y, cur.StartPos.y, cur.StartEaseY.Get(progress)));
                    end = new Vector2(
                        Mathf.LerpUnclamped(pre.EndPos.x, cur.EndPos.x, cur.EndEaseX.Get(progress)),
                        Mathf.LerpUnclamped(pre.EndPos.y, cur.EndPos.y, cur.EndEaseY.Get(progress)));
                }
            }
        }

        public void UpdateHoldMesh(HitPlayer hit)
        {
            if (hit.HoldRenderer == null)
            {
                hit.HoldRenderer = Instantiate(PlayerScreen.main.HoldSample, Holder);
                hit.HoldMesh = hit.HoldRenderer.GetComponent<MeshFilter>();
            }
            if (hit.HoldMesh.mesh == null)
            {
                hit.HoldMesh.mesh = new Mesh();
            }
            Mesh mesh = hit.HoldMesh.mesh;
            _verts.Clear();
            _tris.Clear();

            void AddLine(Vector3 start, Vector3 end)
            {
                // No AddRange here because the alloc overhead adds up
                _verts.Add(start);
                _verts.Add(end);

                if (_verts.Count > 2)
                {
                    _tris.Add(_verts.Count - 4);
                    _tris.Add(_verts.Count - 2);
                    _tris.Add(_verts.Count - 3);
                    _tris.Add(_verts.Count - 2);
                    _tris.Add(_verts.Count - 1);
                    _tris.Add(_verts.Count - 3);
                }
            }

            float time = Mathf.Max(PlayerScreen.main.CurrentTime + PlayerScreen.main.Settings.VisualOffset, hit.Time);

            int index = TimeStamps.FindIndex(x => x > time);
            if (index <= 0 || PositionPoints.Count <= index) return;
            index = Mathf.Max(index, 1);

            float progress = TimeStamps.Count <= 1 ? 0 : Mathf.InverseLerp(TimeStamps[index - 1], TimeStamps[index], time);
            Vector3 preStartPos, preEndPos, curStartPos, curEndPos;

            {
                float position = Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], progress);
                var pre = Current.LaneSteps[index - 1];
                var cur = Current.LaneSteps[index];

                preStartPos = Vector3.LerpUnclamped(pre.StartPos, pre.EndPos, hit.Current.Position);
                preEndPos = Vector3.LerpUnclamped(pre.StartPos, pre.EndPos, hit.Current.Position + hit.Current.Length);
                curStartPos = Vector3.LerpUnclamped(cur.StartPos, cur.EndPos, hit.Current.Position);
                curEndPos = Vector3.LerpUnclamped(cur.StartPos, cur.EndPos, hit.Current.Position + hit.Current.Length);

                if (cur.IsLinear)
                {
                    AddLine(
                        Vector3.Lerp(preStartPos, curStartPos, progress) + Vector3.forward * position,
                        Vector3.Lerp(preEndPos, curEndPos, progress) + Vector3.forward * position);
                }
                else
                {
                    AddLine(
                        new Vector3(
                            Mathf.LerpUnclamped(preStartPos.x, curStartPos.x, cur.StartEaseX.Get(progress)),
                            Mathf.LerpUnclamped(preStartPos.y, curStartPos.y, cur.StartEaseY.Get(progress)),
                            position),
                        new Vector3(
                            Mathf.LerpUnclamped(preEndPos.x, curEndPos.x, cur.EndEaseX.Get(progress)),
                            Mathf.LerpUnclamped(preEndPos.y, curEndPos.y, cur.EndEaseY.Get(progress)),
                            position));
                }
            }

            int ai = 0;
            for (; index < Mathf.Min(PositionPoints.Count, TimeStamps.Count); index++)
            {
                float endProg = InverseLerpUnclamped(TimeStamps[index - 1], TimeStamps[index], hit.EndTime);
                float endPos = Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], endProg);
                var cur = Current.LaneSteps[index];

                curStartPos = Vector3.LerpUnclamped(cur.StartPos, cur.EndPos, hit.Current.Position);
                curEndPos = Vector3.LerpUnclamped(cur.StartPos, cur.EndPos, hit.Current.Position + hit.Current.Length);

                if (cur.IsLinear)
                {
                    AddLine(
                        Vector3.Lerp(preStartPos, curStartPos, endProg) + Vector3.forward * endPos,
                        Vector3.Lerp(preEndPos, curEndPos, endProg) + Vector3.forward * endPos
                    );
                }
                else
                {
                    var pre = Current.LaneSteps[index - 1];
                    for (float x = Mathf.Floor(progress * 16 + 1.01f) / 16; ; x = Mathf.Min(endProg, Mathf.Floor(x * 16 + 1.01f) / 16))
                    {
                        AddLine(
                            new Vector3(
                                Mathf.LerpUnclamped(preStartPos.x, curStartPos.x, cur.StartEaseX.Get(x)),
                                Mathf.LerpUnclamped(preStartPos.y, curStartPos.y, cur.StartEaseY.Get(x)),
                                Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], x)),
                            new Vector3(
                                Mathf.LerpUnclamped(preEndPos.x, curEndPos.x, cur.EndEaseX.Get(x)),
                                Mathf.LerpUnclamped(preEndPos.y, curEndPos.y, cur.EndEaseY.Get(x)),
                                Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], x))
                        );
                        if (x >= Mathf.Min(endProg, 1)) break;
                    }
                }
                progress = 0;

                ai++;
                if (endProg <= 1 || ai > 1000) break;

                preStartPos = curStartPos;
                preEndPos = curEndPos;
            }

            mesh.Clear();
            mesh.SetVertices(_verts);
            mesh.SetTriangles(_tris, 0);
            hit.HoldMesh.mesh = mesh;
        }

        float InverseLerpUnclamped(float start, float end, float val) => (val - start) / (end - start);
    }

    [System.Serializable]
    public struct HitScreenCoord {
        public Vector2 Position;
        public float Radius;
    }
}