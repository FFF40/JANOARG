using System;
using System.Collections.Generic;
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

        // Lets UpdateMesh skip the full recompute for lanes whose two nearest LaneSteps
        // both have zero scroll speed (position provably can't have changed) — see the
        // early-return in UpdateMesh for the full correctness reasoning.
        private bool     _HasBuiltMeshOnce;
        private LaneStep _LastCheckedLaneStep0;

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
        static readonly ProfilerMarker sr_HitObjectSpawn = new("Lane UpdateHitObjects: Spawn Loop");
        static readonly ProfilerMarker sr_HitObjectUpdate = new("Lane UpdateHitObjects: Active Update Loop");
        static readonly ProfilerMarker sr_HitPlayerUpdateSelf = new("HitPlayer.UpdateSelf");
        static readonly ProfilerMarker sr_HoldMeshUpdate = new("Lane UpdateHoldMesh");
        static readonly ProfilerMarker sr_LaneUpdateSelf = new("LanePlayer.UpdateSelf (Total)");
        static readonly ProfilerMarker sr_LaneUpdateSelfTail = new("LanePlayer.UpdateSelf: Transform + Activation");

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
            sr_LaneUpdateSelf.Begin();

            if (Current != null)
                Current.Advance(beat);
            else
                Current = (Lane)Original.GetStoryboardableObject(beat);

            UpdateMesh(time, beat);

            sr_LaneUpdateSelfTail.Begin();
            transform.localPosition = Current.Position;
            transform.localEulerAngles = Current.Rotation;
            Holder.localPosition = Vector3.back * CurrentPosition;
            bool inRange = CurrentPosition - PositionPoints[0] > -200;
            sr_LaneUpdateSelfTail.End();

            if (inRange)
            {
                if (!transform.gameObject.activeSelf)
                    transform.gameObject.SetActive(true);

                UpdateHitObjects(time, beat);
            }
            else if (transform.gameObject.activeSelf)
            {
                // Fail safe: a lane promoted too early (e.g. a very slow lane whose
                // distance-based cueTime lead time undershoots) should stay hidden
                // instead of rendering its mesh at whatever position CurrentPosition
                // happens to be before it has a meaningful trajectory to follow.
                transform.gameObject.SetActive(false);
            }

            sr_LaneUpdateSelf.End();
        }

        private void UpdateMesh(float time, float beat, float maxDistance = 200)
        {
            // No Mesh instantiation

            bool isInvisibleLaneMesh = PlayerScreen.sMain.TransparentMeshLaneIndexes.Contains(Current.StyleIndex) || Current.StyleIndex == -1;
            bool isInvisibleJudgeMesh = PlayerScreen.sMain.TransparentMeshJudgeIndexes.Contains(Current.StyleIndex) || Current.StyleIndex == -1;
            
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
            sr_TimestampRemove.End();

            // Attempt to cull finished lane. LaneSteps timing alone isn't a reliable "safe to
            // remove" signal for decorative Group-driven lanes (e.g. meteors): their LaneSteps
            // are just a timing trick, while what's actually keeping them visually relevant is
            // their own Position/Rotation storyboard. Also require that storyboard to have
            // fully finished advancing before culling.
            bool storyboardFinished = true;
            foreach (Timestamp ts in Current.Storyboard.Timestamps)
            {
                if (beat < ts.Offset + ts.Duration)
                {
                    storyboardFinished = false;
                    break;
                }
            }

            if (_Metronome.ToSeconds(Current.LaneSteps[^1].Offset) < time && HitObjects.Count == 0 && storyboardFinished)
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

            sr_MeshCalc.Begin();
            // Advance the two nearest lane steps
            // (both should either be one just before and one just after current time
            // or two after current time)
            Current.LaneSteps[0].Advance(beat);
            bool hasSecondLaneStep = Current.LaneSteps.Count > 1;
            if (hasSecondLaneStep)
                Current.LaneSteps[1].Advance(beat);

            // Consume dirty flags from storyboard changes as soon as we know about them, so
            // they're available before deciding whether to skip recompute below.
            bool laneStepDirtyThisFrame = false;
            if (Current.LaneSteps[0].IsDirty)
            {
                laneStepDirtyThisFrame = true;
                Current.LaneSteps[0].IsDirty = false;
            }
            if (hasSecondLaneStep && Current.LaneSteps[1].IsDirty)
            {
                laneStepDirtyThisFrame = true;
                Current.LaneSteps[1].IsDirty = false;
            }
            if (laneStepDirtyThisFrame)
                LaneStepDirty = true;

            // If both nearest lane steps have zero scroll speed, CurrentPosition and the
            // mesh geometry provably cannot have changed since the last full recompute —
            // unless a storyboard property changed (laneStepDirtyThisFrame) or we've
            // crossed into a new lane-step pair (sameStepWindow catches that transition,
            // since the trim above may have just shifted LaneSteps[0] to a new object).
            // Requiring more than 2 timestamps guarantees there's a future step boundary
            // left to cross, so that transition (and the JudgeLine enable/disable state
            // that depends on it) is always caught by the trim invalidating sameStepWindow
            // — right at the final 2-timestamp segment we always fully recompute instead.
            bool nearStepsStatic = TimeStamps.Count > 2 &&
                                    Current.LaneSteps[0].Speed == 0f &&
                                    (!hasSecondLaneStep || Current.LaneSteps[1].Speed == 0f);
            bool sameStepWindow = ReferenceEquals(Current.LaneSteps[0], _LastCheckedLaneStep0);

            if (nearStepsStatic && _HasBuiltMeshOnce && !laneStepDirtyThisFrame && sameStepWindow)
            {
                sr_MeshCalc.End();
                return;
            }

            // NOTE: _LastCheckedLaneStep0/_HasBuiltMeshOnce are set later, only once the
            // mesh is actually assigned — not here — so a lane that's currently invisible
            // (e.g. isInvisibleLaneMesh true because its style hasn't faded in yet) keeps
            // being fully recomputed every frame instead of getting permanently frozen as
            // invisible the instant this branch is reached once.

            // Z position a lane step's own speed would put us at by point x (seconds).
            float f_scrollZ(float x) => x * Current.LaneSteps[0].Speed * PlayerScreen.sMain.Speed;

            // Cache last position point (prevent ArgumentOutOfRangeException)
            float lastPositionPoints = PositionPoints.Count > 0 ? PositionPoints[^1] : 0;

            // Calculate the current Z position — clamp to the last remaining timestamp so a
            // lane kept alive past its LaneSteps (e.g. one still finishing its own storyboard)
            // freezes there instead of extrapolating its last step's speed forever.
            float scrollTime = Mathf.Min(time, TimeStamps[^1]);
            if (TimeStamps.Count <= 1 || TimeStamps[0] > time)
                CurrentPosition = f_scrollZ(time);
            else
                if (PositionPoints.Count != 0)
                    CurrentPosition = (scrollTime - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed + PositionPoints[0];
                else
                    CurrentPosition = (scrollTime - TimeStamps[0]) * Current.LaneSteps[1].Speed * PlayerScreen.sMain.Speed + lastPositionPoints;

            // Calculate the current progress between our two nearest lane step time
            float progress = TimeStamps.Count <= 1
                ? 0
                : Mathf.InverseLerp(TimeStamps[0], TimeStamps[1], time);

            // Seed the first position point (the Z position of LaneSteps[0]) and, until the
            // lane actually reaches that step, keep it synced to the step's *current*
            // storyboarded speed — a lane whose Speed storyboard changes before arrival (e.g.
            // scroll speed dropping to 0 right as a separate Position storyboard takes over)
            // would otherwise sit far behind a stale anchor and fail the in-range check.
            if (PositionPoints.Count == 0)
                PositionPoints.Add(f_scrollZ(TimeStamps[0]));
            else if (TimeStamps[0] > time)
                PositionPoints[0] = f_scrollZ(TimeStamps[0]);
            
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
                            !isInvisibleJudgeMesh && TimeStamps.Count >= 2 && time >= TimeStamps[0] && time < TimeStamps[1];
                
                // If the judgment line is enabled, update its current position
                Transform judgeLineTransform = JudgeLine.transform;
                judgeLineTransform.localPosition = (startPoint + endPoint) / 2;
                judgeLineTransform.localScale = new Vector3(f_vec2Distance(startPoint, endPoint), .05f, .05f);
                judgeLineTransform.localRotation = Quaternion.Euler(0, 0, f_signedAngle(Vector2.right, endPoint - startPoint));

                JudgePointLeft.transform.localPosition = startPoint;
                JudgePointRight.transform.localPosition = endPoint;
            }
            sr_MeshLerper.End();

            // (Dirty-flag handling for LaneSteps[0]/[1] now happens earlier, before the
            // static-lane skip check, so it's available before deciding to recompute.)

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
            if (isInvisibleLaneMesh && HitObjects.Count == 0)
                return;

            // Only now — once we know the mesh is actually about to be assigned — do we
            // record that this lane has a real built mesh, so the static-lane skip above
            // can never freeze a lane that's never actually been rendered.
            _LastCheckedLaneStep0 = Current.LaneSteps[0];
            _HasBuiltMeshOnce = true;

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
            sr_HitObjectSpawn.Begin();
            while (Current.Objects.Count > 0)
            {
                HitObject hit = Current.Objects[0];
                if (float.IsNaN(_HitObjectTime)) _HitObjectTime = PlayerScreen.sTargetSong.Timing.ToSeconds(hit.Offset);

                if (GetZPosition(_HitObjectTime) <= CurrentPosition + maxDistance)
                {
                    HitPlayer player = PlayerScreen.sMain.BorrowHitPlayer(Holder);

                    player.Original = Original.Objects[_HitObjectOffset];
                    player.Current = Current.Objects[0];

                    player.Time = _HitObjectTime;
                    player.EndTime = player.Current.HoldLength > 0 
                        ? PlayerScreen.sTargetSong.Timing.ToSeconds(hit.Offset + hit.HoldLength) : _HitObjectTime;
                    player.HitCoord = HitCoords[0];

                    // Always clear first: a reused (pooled) instance may carry ticks from its previous note.
                    player.HoldTicks.Clear();
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
            sr_HitObjectSpawn.End();

            var active = true;

            sr_HitObjectUpdate.Begin();
            foreach (HitPlayer hitObject in HitObjects)
            {
                if (active)
                {
                    sr_HitPlayerUpdateSelf.Begin();
                    hitObject.UpdateSelf(time, beat, LaneStepDirty);
                    sr_HitPlayerUpdateSelf.End();
                }

                if (active && hitObject.CurrentPosition > CurrentPosition + 200)
                    active = false;

                // HoldMesh is now a permanent (pooled) child, so its existence no longer
                // implies this note is a hold — gate on the actual note data instead.
                bool isHold = hitObject.Current.HoldLength > 0 && hitObject.HoldMesh != null;

                hitObject.gameObject.SetActive(active || (isHold && GetZPosition(hitObject.EndTime) <= CurrentPosition + 200) || (isHold && hitObject.HoldMesh.gameObject.activeSelf));

                if (isHold)
                    hitObject.HoldMesh.gameObject.SetActive(active || GetZPosition(hitObject.EndTime) <= CurrentPosition + 200);
            }
            sr_HitObjectUpdate.End();

            LaneStepDirty = false;
        }


        public double GetZPosition(double time)
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

        public void GetStartEndPosition(double time, out Vector2 start, out Vector2 end)
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
                float progress = Mathf.InverseLerp(TimeStamps[index - 1], TimeStamps[index], (float)time);

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
            sr_HoldMeshUpdate.Begin();
            try
            {
                UpdateHoldMeshInternal(hit);
            }
            finally
            {
                sr_HoldMeshUpdate.End();
            }
        }

        private void UpdateHoldMeshInternal(HitPlayer hit)
        {
            if (hit.HoldRenderer == null)
            {
                hit.HoldRenderer = Instantiate(PlayerScreen.sMain.HoldSample, Holder);
                hit.HoldMesh = hit.HoldRenderer.GetComponent<MeshFilter>();
            }
            else if (hit.HoldRenderer.transform.parent != Holder)
            {
                // A pooled HitPlayer's HoldRenderer is a sibling under the lane's Holder, not a
                // child of the HitPlayer itself, so it isn't reparented when the HitPlayer is
                // borrowed for a different lane. Only do this when the lane actually changed —
                // this runs every frame for every active hold, and SetParent's default
                // worldPositionStays:true fights the scroll (Holder's own position drives the
                // scroll every frame), so re-parenting unconditionally here would freeze/desync
                // the mesh's position instead of leaving it to move naturally with Holder.
                hit.HoldRenderer.transform.SetParent(Holder, false);
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

            double time = Math.Max(PlayerScreen.sMain.CurrentTime + PlayerScreen.sMain.Settings.VisualOffset, hit.Time);

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

            float progress = TimeStamps.Count <= 1 ? 0 : Mathf.InverseLerp(TimeStamps[index - 1], TimeStamps[index], (float)time);
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
                float segmentEndProgress = Mathf.Clamp01(endStepProgress);
                float endStepPosition = Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], segmentEndProgress);
                LaneStep currentStep = Current.LaneSteps[index];

                currentStepStartPointPosition = Vector3.LerpUnclamped(currentStep.StartPointPosition, currentStep.EndPointPosition, hit.Current.Position);
                currentStepEndPointPosition = Vector3.LerpUnclamped(currentStep.StartPointPosition, currentStep.EndPointPosition, hit.Current.Position + hit.Current.Length);

                if (currentStep.IsLinear)
                {
                    f_addLine(
                        Vector3.Lerp(previousStepStartPointPosition, currentStepStartPointPosition, segmentEndProgress) + Vector3.forward * endStepPosition,
                        Vector3.Lerp(previousStepEndPointPosition, currentStepEndPointPosition, segmentEndProgress) + Vector3.forward * endStepPosition
                    );
                }
                else
                {
                    LaneStep previousStep = Current.LaneSteps[index - 1];

                    for (float x = Mathf.Floor(progress * 16 + 1.01f) / 16;;)
                    {
                        float sampledProgress = Mathf.Min(segmentEndProgress, x);
                        f_addLine(
                            new Vector3(
                                Mathf.LerpUnclamped(previousStepStartPointPosition.x, currentStepStartPointPosition.x, currentStep.StartEaseX.Get(sampledProgress)),
                                Mathf.LerpUnclamped(previousStepStartPointPosition.y, currentStepStartPointPosition.y, currentStep.StartEaseY.Get(sampledProgress)),
                                Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], sampledProgress)),
                            new Vector3(
                                Mathf.LerpUnclamped(previousStepEndPointPosition.x, currentStepEndPointPosition.x, currentStep.EndEaseX.Get(sampledProgress)),
                                Mathf.LerpUnclamped(previousStepEndPointPosition.y, currentStepEndPointPosition.y, currentStep.EndEaseY.Get(sampledProgress)),
                                Mathf.Lerp(PositionPoints[index - 1], PositionPoints[index], sampledProgress))
                        );

                        if (sampledProgress >= segmentEndProgress)
                            break;

                        x = Mathf.Floor(sampledProgress * 16 + 1.01f) / 16;
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
