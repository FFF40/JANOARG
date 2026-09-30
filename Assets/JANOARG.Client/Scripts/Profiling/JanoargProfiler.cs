using Unity.Profiling;

namespace JANOARG.Client.Profiling
{
    /// <summary>
    ///     Definitions for JANOARG's custom Profiler counters. Values are written once a frame by
    ///     <see cref="JanoargProfilerSampler"/>; the editor-side Profiler modules that chart them are
    ///     in <c>Editor/Profiling/JanoargProfilerModules.cs</c>.
    /// </summary>
    /// <remarks>
    ///     Unity compiles <see cref="ProfilerCounterValue{T}"/> out of non-development builds, so this
    ///     costs nothing in release. The counter <em>names</em> are shared constants so the editor
    ///     module descriptors can't drift out of sync with the counters.
    /// </remarks>
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static class JanoargProfiler
    {
        public static readonly ProfilerCategory Category = new ProfilerCategory("JANOARG");
        // ---- Active Gameplay Objects ----------------------------------------------------- //

        public const string ActiveLanesName      = "Active Lanes";
        public const string PendingLanesName     = "Pending Lanes";
        public const string ActiveLaneGroupsName = "Active Lane Groups";
        public const string ActiveHitObjectsName = "Active Hit Objects";
        public const string ActiveHoldNotesName  = "Active Hold Notes";
        public const string ActiveTouchesName    = "Active Touches";

        public static readonly ProfilerCounterValue<int> ActiveLanes      = new(Category, ActiveLanesName,      ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> PendingLanes     = new(Category, PendingLanesName,     ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> ActiveLaneGroups = new(Category, ActiveLaneGroupsName, ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> ActiveHitObjects = new(Category, ActiveHitObjectsName, ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> ActiveHoldNotes  = new(Category, ActiveHoldNotesName,  ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> ActiveTouches    = new(Category, ActiveTouchesName,    ProfilerMarkerDataUnit.Count);

        // ---- Gameplay Rendering ---------------------------------------------------------- //
        // Game-side geometry rather than UnityStats (which isn't available in the player
        // assembly). Batches/triangles are in the built-in Rendering module; these track what
        // the gameplay itself is feeding the renderer.

        public const string ActiveLaneRenderersName = "Active Lane Renderers";
        public const string LaneVerticesName        = "Lane Vertices";
        public const string LaneTrianglesName       = "Lane Triangles";
        public const string ActiveHoldMeshesName    = "Active Hold Meshes";

        public static readonly ProfilerCounterValue<int> ActiveLaneRenderers = new(Category, ActiveLaneRenderersName, ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> LaneVertices        = new(Category, LaneVerticesName,        ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> LaneTriangles       = new(Category, LaneTrianglesName,       ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> ActiveHoldMeshes    = new(Category, ActiveHoldMeshesName,    ProfilerMarkerDataUnit.Count);

        // ---- Gameplay Object Pools ------------------------------------------------------- //

        public const string HitPlayerPoolName      = "Hit Player Pool";
        public const string HitPlayersInUseName    = "Hit Players In Use";
        public const string JudgeEffectPoolName    = "Judge Effect Pool";
        public const string JudgeEffectsInUseName  = "Judge Effects In Use";

        public static readonly ProfilerCounterValue<int> HitPlayerPool     = new(Category, HitPlayerPoolName,     ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> HitPlayersInUse   = new(Category, HitPlayersInUseName,   ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> JudgeEffectPool   = new(Category, JudgeEffectPoolName,   ProfilerMarkerDataUnit.Count);
        public static readonly ProfilerCounterValue<int> JudgeEffectsInUse = new(Category, JudgeEffectsInUseName, ProfilerMarkerDataUnit.Count);

        /// <summary>Zero every counter. Called when no PlayerScreen is active so the graphs drop
        /// to zero on exit instead of freezing at their last values.</summary>
        public static void Reset()
        {
            ActiveLanes.Value = PendingLanes.Value = ActiveLaneGroups.Value = 0;
            ActiveHitObjects.Value = ActiveHoldNotes.Value = ActiveTouches.Value = 0;
            ActiveLaneRenderers.Value = LaneVertices.Value = LaneTriangles.Value = ActiveHoldMeshes.Value = 0;
            HitPlayerPool.Value = HitPlayersInUse.Value = JudgeEffectPool.Value = JudgeEffectsInUse.Value = 0;
        }
    }
#endif
}
