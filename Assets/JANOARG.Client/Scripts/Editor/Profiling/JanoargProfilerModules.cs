using JANOARG.Client.Profiling;
using Unity.Profiling;
using Unity.Profiling.Editor;

namespace JANOARG.Client.Editor.Profiling
{
    // Three custom Profiler modules charting JANOARG's gameplay counters. They show up in the
    // Profiler window's module dropdown once a session containing the counters has been captured.

    [System.Serializable]
    [ProfilerModuleMetadata("Active Gameplay Objects")]
    public class ActiveGameplayObjectsProfilerModule : ProfilerModule
    {
        static readonly ProfilerCounterDescriptor[] k_Counters =
        {
            new(JanoargProfiler.ActiveLanesName,      JanoargProfiler.Category),
            new(JanoargProfiler.PendingLanesName,     JanoargProfiler.Category),
            new(JanoargProfiler.ActiveLaneGroupsName, JanoargProfiler.Category),
            new(JanoargProfiler.ActiveHitObjectsName, JanoargProfiler.Category),
            new(JanoargProfiler.ActiveHoldNotesName,  JanoargProfiler.Category),
            new(JanoargProfiler.ActiveTouchesName,    JanoargProfiler.Category),
        };

        public ActiveGameplayObjectsProfilerModule() : base(k_Counters) { }
    }

    [System.Serializable]
    [ProfilerModuleMetadata("Gameplay Rendering")]
    public class GameplayRenderingProfilerModule : ProfilerModule
    {
        static readonly ProfilerCounterDescriptor[] k_Counters =
        {
            new(JanoargProfiler.ActiveLaneRenderersName, JanoargProfiler.Category),
            new(JanoargProfiler.LaneVerticesName,        JanoargProfiler.Category),
            new(JanoargProfiler.LaneTrianglesName,       JanoargProfiler.Category),
            new(JanoargProfiler.ActiveHoldMeshesName,    JanoargProfiler.Category),
        };

        public GameplayRenderingProfilerModule() : base(k_Counters) { }
    }

    [System.Serializable]
    [ProfilerModuleMetadata("Gameplay Object Pools")]
    public class GameplayObjectPoolsProfilerModule : ProfilerModule
    {
        static readonly ProfilerCounterDescriptor[] k_Counters =
        {
            new(JanoargProfiler.HitPlayerPoolName,     JanoargProfiler.Category),
            new(JanoargProfiler.HitPlayersInUseName,   JanoargProfiler.Category),
            new(JanoargProfiler.JudgeEffectPoolName,   JanoargProfiler.Category),
            new(JanoargProfiler.JudgeEffectsInUseName, JanoargProfiler.Category),
        };

        public GameplayObjectPoolsProfilerModule() : base(k_Counters) { }
    }
}
