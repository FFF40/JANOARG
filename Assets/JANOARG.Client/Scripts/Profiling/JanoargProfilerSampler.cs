using JANOARG.Client.Behaviors.Player;
using UnityEngine;

namespace JANOARG.Client.Profiling
{
    /// <summary>
    ///     Samples JANOARG's gameplay counters once a frame into the Unity Profiler (see
    ///     <see cref="JanoargProfiler"/> for the counter definitions and the module names). Place it on
    ///     the PlayerScreen's GameObject; it no-ops when no PlayerScreen is active.
    /// </summary>
    public class JanoargProfilerSampler : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Auto-installs itself in dev builds/editor so no scene wiring is needed.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var go = new GameObject("[JanoargProfilerSampler]") { hideFlags = HideFlags.DontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<JanoargProfilerSampler>();
        }

        private void LateUpdate()
        {
            PlayerScreen player = PlayerScreen.sMain;
            if (player == null)
            {
                JanoargProfiler.Reset();
                return;
            }

            // ---- Active gameplay objects ------------------------------------------------ //
            JanoargProfiler.ActiveLanes.Value      = player.Lanes.Count;
            JanoargProfiler.PendingLanes.Value     = player.PendingLaneCount;
            JanoargProfiler.ActiveLaneGroups.Value = player.LaneGroups.Count;
            JanoargProfiler.ActiveHitObjects.Value = player.ActiveHitObjectCount;

            PlayerInputManager input = PlayerInputManager.sInstance;
            JanoargProfiler.ActiveHoldNotes.Value = input != null ? input.HoldQueue.Count    : 0;
            JanoargProfiler.ActiveTouches.Value   = input != null ? input.TouchClasses.Count : 0;

            // ---- Gameplay rendering ----------------------------------------------------- //
            JanoargProfiler.ActiveLaneRenderers.Value = player.ActiveLaneRendererCount;
            JanoargProfiler.LaneVertices.Value        = player.LaneVertexCount;
            JanoargProfiler.LaneTriangles.Value       = player.LaneTriangleCount;
            JanoargProfiler.ActiveHoldMeshes.Value    = player.ActiveHoldMeshCount;

            // ---- Object pools ----------------------------------------------------------- //
            JanoargProfiler.HitPlayerPool.Value   = player.HitPlayerPoolCount;
            JanoargProfiler.HitPlayersInUse.Value = player.HitPlayersInUseCount;

            JudgeScreenManager judge = player.JudgeScreenManager;
            if (judge != null)
            {
                JanoargProfiler.JudgeEffectPool.Value   = judge.JudgeScreenEffectsPoolCount;
                JanoargProfiler.JudgeEffectsInUse.Value = judge.TotalInstances;
            }
        }
#endif
    }
}
