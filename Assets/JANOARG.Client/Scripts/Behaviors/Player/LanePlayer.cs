using System;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Shared.Data.ChartInfo;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Client.Behaviors.Player
{
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

        [FormerlySerializedAs("JudgeRight")] public MeshRenderer JudgePointLeft;
        [FormerlySerializedAs("JudgeLeft")]  public MeshRenderer JudgePointRight;

        public List<float> PositionPoints = new();
        public List<float> TimeStamps     = new();
        public float       CurrentPosition;

        public List<HitPlayer>      HitObjects = new();
        public List<HitScreenCoord> HitCoords  = new();

        public bool LaneStepDirty = false;
        private Mesh          _Mesh;
        
        public bool MarkedForRemoval = false; 

        // WARNING :
        // THIS IS NOT THREAD SAFE
        private readonly List<Vector3> _Verts = new(2048);
        private readonly List<int>     _Tris  = new(1024);
        
        static readonly ProfilerMarker sr_TimestampRemove = new("Lane UpdateMesh: Remove Timestamps");
        static readonly ProfilerMarker sr_MeshCalc = new("Lane UpdateMesh: Calculate advance");
        static readonly ProfilerMarker sr_MeshLerper = new("Lane UpdateMesh: Lerper");
        static readonly ProfilerMarker sr_MeshLaneStepLooper = new("Lane UpdateMesh: Lane Step Looper");
        static readonly ProfilerMarker sr_MeshUpdater = new("Lane UpdateMesh: Mesh Updater");

        private Metronome _Metronome;
        
        public void Init()
        {
            if (_Mesh == null)
            {
                _Mesh = new Mesh();
                MeshFilter.mesh = _Mesh;
            }
            
            _Mesh.MarkDynamic();
            
            _Metronome = PlayerScreen.sTargetSong.Timing;
            
            foreach (LaneStep step in Current.LaneSteps) 
                TimeStamps.Add(_Metronome.ToSeconds(step.Offset));

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

        public void UpdateSelf(float time, float beat)
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
                if (!transform.gameObject.activeSelf)
                    transform.gameObject.SetActive(true);
                
                UpdateHitObjects(time, beat);
            }
        }

        private void UpdateMesh(float time, float beat, float maxDistance = 200)
        {
            // No Mesh instantiation

            bool isInvisibleMesh = PlayerScreen.sMain.TransparentMeshLaneIndexes.Any(style => style == Current.StyleIndex);
            
            _Verts.Clear();
            _Tris.Clear();

            void f_addLine(Vector3 start, Vector3 end)
            {
                // No AddRange here because the alloc overhead adds up
                _Verts.Add(start);
                _Verts.Add(end);

                int vertCount = _Verts.Count;
                
                if (vertCount > 2)
                {
                    _Tris.Add(vertCount - 4);
                    _Tris.Add(vertCount - 2);
                    _Tris.Add(vertCount - 3);
                    _Tris.Add(vertCount - 2);
                    _Tris.Add(vertCount - 1);
                    _Tris.Add(vertCount - 3);
                }
            }
            
            float f_lerpUnclamped(float a, float b, float t) => a + (b - a) * t;
            float f_lerp(float a, float b, float t)          => (1 - t) * a + t * b;;
            float f_signedAngle(Vector2 a, Vector2 b)        => Vector2.Angle(a, b) * Math.Sign((float) (a.x * (double) b.y - a.y * (double) b.x));
            
            Vector3 f_vec3Lerp(Vector3 a, Vector3 b, float t)
            {
                t = t > 1 ? 1 : t < 0 ? 0 : t;
                return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
            }
            
            float f_vec2Distance(Vector2 a, Vector2 b)
            {
                float num1 = a.x - b.x;
                float num2 = a.y - b.y;
                return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
            }

            
            sr_TimestampRemove.Begin();
            // Remove timestamps and position points when we already move past them
            while (TimeStamps.Count > 2 && TimeStamps[1] < time)
            {
                TimeStamps.RemoveAt(0);
                PositionPoints.RemoveAt(0);
                Current.LaneSteps.RemoveAt(0);
            }
            
            // Attempt to cull finished lane
            if (_Metronome.ToSeconds(Current.LaneSteps[^1].Offset) < time) 
            {
                if (TimeStamps[^1] < time)
                {
                    if (_Mesh != null)
                        Destroy(_Mesh);
                    
                    if (gameObject != null)
                        Destroy(gameObject);
                    
                    MarkedForRemoval = true;
                }
                return;
            }
            
            sr_TimestampRemove.End();

            sr_MeshCalc.Begin();
            // Advance the two nearest lane steps 
            // (both should either be one just before and one just after current time
            // or two after current time)
            Current.LaneSteps[0].Advance(beat);
            if (Current.LaneSteps.Count > 1)
                Current.LaneSteps[1].Advance(beat);

            // Cache last position point (prevent ArgumentOutOfRangeException)
            float lastPositionPoints = PositionPoints.Count > 0 ? PositionPoints[^1] : 0;
            
            // Calculate the current Z position
            if (TimeStamps.Count <= 1 || TimeStamps[0] > time)
                CurrentPosition = time * Current.LaneSteps[0].Speed * PlayerScreen.sMain.Speed;
            else
                if (PositionPoints.Count != 0)
                    CurrentPosition = (time - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed + PositionPoints[0];
                else
                    CurrentPosition = (time - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed + lastPositionPoints;

            // Calculate the current progress between our two nearest lane step time
            float progress = TimeStamps.Count <= 1
                ? 0
                : Mathf.InverseLerp(TimeStamps[0], TimeStamps[1], time);

            // Since the game calculate the current distance scrolled by interpolating two position points
            // this ensures we have at least 2 position points
            if (PositionPoints.Count <= 1)
                PositionPoints.Add(TimeStamps[0] * Current.LaneSteps[0].Speed * PlayerScreen.sMain.Speed);
            
            sr_MeshCalc.End();

            // If there's only one lane step on the lane, the lane body would just be an infinitesimally thin line so
            // we can safely skip lane mesh generation
            if (TimeStamps.Count <= 1)
                return;
            
            sr_MeshCalc.Begin();
            // Calculate the Z position of the lane step at index 1
            if (PositionPoints.Count <= 2)
                PositionPoints.Add((TimeStamps[1] - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed + PositionPoints[0]);
            else
                PositionPoints[1] = PositionPoints[0] + (TimeStamps[1] - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed;
            sr_MeshCalc.End();
            
            if (!(CurrentPosition - PositionPoints[0] > -200))
            {
                // If the current Z position is further than our distance threshold,
                // assume all lane body from this point is further away from our sight
                // so we can skip lane mesh construction past this point.
                return;
            }

            sr_MeshLerper.Begin();
            // Updates the current judgment line position by using data from the two lane step nearest from current time
            {
                // The current Z position
                float position = f_lerp(PositionPoints[0], PositionPoints[1], progress);
                
                // The ending lane step, which determines current lane body shape and scroll speed
                LaneStep currentLaneStep = Current.LaneSteps[1];
                

                
                // Calculates start and end position
                Vector3 startPoint, endPoint;
                if (currentLaneStep.IsLinear)
                {
                    // Optimize for fully linear lane body
                    startPoint = f_vec3Lerp(Current.LaneSteps[0].StartPointPosition, Current.LaneSteps[1].StartPointPosition, progress) + Vector3.forward * position;
                    endPoint = f_vec3Lerp(Current.LaneSteps[0].EndPointPosition, Current.LaneSteps[1].EndPointPosition, progress) + Vector3.forward * position;
                }
                else
                {
                    startPoint = new Vector3(
                        f_lerpUnclamped(Current.LaneSteps[0].StartPointPosition.x, Current.LaneSteps[1].StartPointPosition.x, currentLaneStep.StartEaseX.Get(progress)),
                        f_lerpUnclamped(Current.LaneSteps[0].StartPointPosition.y, Current.LaneSteps[1].StartPointPosition.y, currentLaneStep.StartEaseY.Get(progress)),
                        position
                    );

                    endPoint = new Vector3(
                        f_lerpUnclamped(Current.LaneSteps[0].EndPointPosition.x, Current.LaneSteps[1].EndPointPosition.x, currentLaneStep.EndEaseX.Get(progress)),
                        f_lerpUnclamped(Current.LaneSteps[0].EndPointPosition.y, Current.LaneSteps[1].EndPointPosition.y, currentLaneStep.EndEaseY.Get(progress)),
                        position
                    );
                }

                // The current judgment line position marks the start of the lane body, 
                // so add its start and end point to the line list
                f_addLine(startPoint, endPoint);
                
                // Enable judgment line if it's scrolling inside this lane body
                JudgeLine.enabled =
                    JudgePointLeft.enabled =
                        JudgePointRight.enabled =
                            TimeStamps.Count >= 2 && time >= TimeStamps[0] && time < TimeStamps[1];
                
                // If the judgment line is enabled, update its current position
                Transform judgeLineTransform = JudgeLine.transform;
                judgeLineTransform.localPosition = (startPoint + endPoint) / 2;
                judgeLineTransform.localScale = new Vector3(f_vec2Distance(startPoint, endPoint), .05f, .05f);
                judgeLineTransform.localRotation = Quaternion.Euler(0, 0, f_signedAngle(Vector2.right, endPoint - startPoint));

                JudgePointLeft.transform.localPosition = startPoint;
                JudgePointRight.transform.localPosition = endPoint;
            }
            sr_MeshLerper.End();
            
            
            // If our two lane step nearest from current time has dirty values because of storyboard,
            // we mark our lane as dirty for update on the next frame and reset their dirty flags
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

            sr_MeshLaneStepLooper.Begin();
            // Loop through our lane step list
            // Skipping index 0 since it's the beginning of the lane or we have already moved past its body
            // The lane step time list length at this step should be equal to that of the lane step object list
            for (var currentTimestamp = 1; currentTimestamp < TimeStamps.Count; currentTimestamp++)
            {
                // Get current lane step
                LaneStep currentLaneStep = Current.LaneSteps[currentTimestamp];

                // Advance this lane step's storyboard, skipping index 1 because it's already updated
                if (currentTimestamp > 1)
                {
                    currentLaneStep.Advance(beat);

                    // Mark our lane as dirty if the storyboard of the current lane step changed something
                    if (currentLaneStep.IsDirty)
                    {
                        LaneStepDirty = true;
                        currentLaneStep.IsDirty = false;
                    }
                }

                // Calculate the Z position of this lane step
                float calculatedPosition = 
                    PositionPoints[currentTimestamp - 1] + (TimeStamps[currentTimestamp] - TimeStamps[currentTimestamp - 1]) * currentLaneStep.Speed * PlayerScreen.sMain.Speed;
                
                if (PositionPoints.Count <= currentTimestamp)
                    PositionPoints.Add(calculatedPosition);
                else
                    PositionPoints[currentTimestamp] = calculatedPosition;
                
                // Construct the lane body
                if (currentLaneStep.IsLinear)
                {
                    // If the lane body is linear, we only have to add 1 line
                    f_addLine(
                        (Vector3)currentLaneStep.StartPointPosition + Vector3.forward * calculatedPosition,
                        (Vector3)currentLaneStep.EndPointPosition + Vector3.forward * calculatedPosition
                    );
                }
                else
                {
                    // Otherwise we need to construct the body by interpolating from the previous lane step
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
                
                // If this lane step is further than our distance threshold,
                // assume all lane body from this point is further away from our sight
                // so we can skip lane mesh construction past this point.
                // The position point index check is probably redundant, I'm not sure
                if (currentTimestamp >= PositionPoints.Count && calculatedPosition - CurrentPosition > maxDistance)
                {
                    break;
                }


                // Since we haven't scroll past this lane step yet the progress from this lane step to the next one is 0
                progress = 0;
            }
            sr_MeshLaneStepLooper.End();
                            
            // Skip rendering for invisible lanes
            if (isInvisibleMesh && HitObjects.Count == 0)
                return;
            
            sr_MeshUpdater.Begin();
            // Actually update mesh data
            _Mesh.Clear(false);
            _Mesh.SetVertices(_Verts);
            _Mesh.SetTriangles(_Tris, 0, true);
            sr_MeshUpdater.End();
        }

        private float _HitObjectTime   = float.NaN;
        private int   _HitObjectOffset = 0;

        private void UpdateHitObjects(float time, float beat, float maxDistance = 200)
        {
            while (Current.Objects.Count > 0)
            {
                HitObject hit = Current.Objects[0];
                if (float.IsNaN(_HitObjectTime)) _HitObjectTime = PlayerScreen.sTargetSong.Timing.ToSeconds(hit.Offset);

                if (GetZPosition(_HitObjectTime) <= CurrentPosition + maxDistance)
                {
                    HitPlayer player = Instantiate(PlayerScreen.sMain.HitSample, Holder);

                    player.Original = Original.Objects[_HitObjectOffset];
                    player.Current = Current.Objects[0];

                    player.Time = _HitObjectTime;
                    player.EndTime = player.Current.HoldLength > 0 
                        ? PlayerScreen.sTargetSong.Timing.ToSeconds(hit.Offset + hit.HoldLength) : _HitObjectTime;
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
                    hitObject.UpdateSelf(time, beat, LaneStepDirty);

                if (active && hitObject.CurrentPosition > CurrentPosition + 200)
                    active = false;

                hitObject.gameObject.SetActive(active);

                if (hitObject.HoldMesh)
                    hitObject.HoldMesh.gameObject.SetActive(active);
            }

            LaneStepDirty = false;
        }


        public float GetZPosition(float time)
        {
            if (TimeStamps == null || TimeStamps.Count == 0 || PositionPoints == null || PositionPoints.Count == 0)
                return 0f; // failsafe

            int index = -1;
            for (int i = 0; i < TimeStamps.Count; i++){
                if (TimeStamps[i] >= time){
                    index = i;
                    break;
                }
            }
    
            if (index < 0) 
            {
                int lastIndex = Mathf.Min(PositionPoints.Count - 1, TimeStamps.Count - 1);
                if (lastIndex < 0) return 0f;
        
                return PositionPoints[lastIndex] + 
                       (time - TimeStamps[lastIndex]) * 
                       Current.LaneSteps[Mathf.Min(lastIndex, Current.LaneSteps.Count - 1)].Speed * 
                       PlayerScreen.sMain.Speed;
            }
    
            index = Mathf.Min(index, PositionPoints.Count - 1);
            index = Mathf.Min(index, Current.LaneSteps.Count - 1);

            return PositionPoints[index] + 
                   (time - TimeStamps[index]) * 
                   Current.LaneSteps[index].Speed * 
                   PlayerScreen.sMain.Speed;
        }

        public void GetStartEndPosition(float time, out Vector2 start, out Vector2 end)
        {
            int index = -1;
            for (int i = 0; i < TimeStamps.Count; i++){
                if (TimeStamps[i] >= time){
                    index = i;
                    break;
                }
            }
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

                if (currentStep.IsLinear)
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

        public void UpdateHoldMesh(HitPlayer hit)
        {
            if (hit.HoldRenderer == null)
            {
                hit.HoldRenderer = Instantiate(PlayerScreen.sMain.HoldSample, Holder);
                hit.HoldMesh = hit.HoldRenderer.GetComponent<MeshFilter>();
            }

            if (hit.HoldMesh.mesh == null) 
                hit.HoldMesh.mesh = new Mesh();

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

            int index = -1;
            for (int i = 0; i < TimeStamps.Count; i++)
            {
                if (TimeStamps[i] > time){
                    index = i;
                    break;
                }
            }
            if (index <= 0 || PositionPoints.Count <= index)
                return;

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

                if (currentStep.IsLinear)
                    f_addLine(
                        Vector3.Lerp(previousStepStartPointPosition, currentStepStartPointPosition, progress) + Vector3.forward * position,
                        Vector3.Lerp(previousStepEndPointPosition, currentStepEndPointPosition, progress) + Vector3.forward * position
                    );
                else
                    f_addLine(
                        new Vector3(
                            Mathf.LerpUnclamped(previousStepStartPointPosition.x, currentStepStartPointPosition.x, currentStep.StartEaseX.Get(progress)),
                            Mathf.LerpUnclamped(previousStepStartPointPosition.y, currentStepStartPointPosition.y, currentStep.StartEaseY.Get(progress)),
                            position
                        ),
                        new Vector3(
                            Mathf.LerpUnclamped(previousStepEndPointPosition.x, currentStepEndPointPosition.x, currentStep.EndEaseX.Get(progress)),
                            Mathf.LerpUnclamped(previousStepEndPointPosition.y, currentStepEndPointPosition.y, currentStep.EndEaseY.Get(progress)),
                            position
                        )
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

                if (currentStep.IsLinear)
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
            // hit.HoldMesh.mesh = mesh;
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