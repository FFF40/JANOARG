using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.SongSelect;
using JANOARG.Client.UI;
using JANOARG.Client.Utils;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils;
using JANOARG.Shared.Utils.Animation;
using TMPro;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Player
{

    public class PlayerScreen : MonoBehaviour
    {
        static readonly ProfilerMarker sr_LaneStylesUpdate = new("PlayerScreen.LateUpdate: LaneStyles Loop");
        static readonly ProfilerMarker sr_HitStylesUpdate = new("PlayerScreen.LateUpdate: HitStyles Loop");
        static readonly ProfilerMarker sr_CameraAdvance = new("PlayerScreen.LateUpdate: Camera Advance");
        static readonly ProfilerMarker sr_SpawnHitEffect = new("PlayerScreen.SpawnHitEffect");
        static readonly ProfilerMarker sr_UpdateInputCall = new("PlayerScreen.Update: PlayerInputManager.UpdateInput");
        static readonly ProfilerMarker sr_HoldMeshLoop = new("PlayerScreen.Update: Hold Mesh Loop");
        static readonly ProfilerMarker sr_LanesLoop = new("PlayerScreen.LateUpdate: Lanes Loop (Total)");

        public static PlayerScreen sMain;

        public static string            sTargetSongPath;
        public static PlayableSong      sTargetSong;
        public static ExternalChartMeta sTargetChartMeta;
        public static ExternalChart     sTargetChart;

        public static Chart sCurrentChart;

        public ExternalPlayableSong RunChart;

        public ExternalChart Chart;

        [Space]
        public AudioSource Music;

        public AudioSource Hitsounds;
        public Camera      Pseudocamera;

        [Space]
        public Transform Holder;

        public GameObject PlayerHUD;

        [Space]
        public TMP_Text SongNameLabel;

        public TMP_Text SongArtistLabel;
        public TMP_Text DifficultyNameLabel;
        public TMP_Text DifficultyLabel;

        [Space]
        public Slider SongProgress;

        public Graphic SongProgressBody;
        public Graphic SongProgressTip;
        public Graphic SongProgressGlow;

        [Space]
        public ScrollingCounter ScoreCounter;

        public Graphic[] ScoreDigits;

        [Space]
        public CanvasGroup JudgmentGroup;

        public TMP_Text      JudgmentLabel;
        public CanvasGroup   ComboGroup;
        public TMP_Text      ComboLabel;
        public RectTransform JudgeScreenHolder;
        public RectTransform JudgeScreen;
        [Space]
        public JudgeScreenManager JudgeScreenManager;
        [Space]
        // Persistent parent for pooled HitPlayer instances awaiting reuse.
        // Must NOT be a child of any LanePlayer's Holder, since lanes get
        // wholesale-destroyed on retry (see LoadChart) and would take pooled
        // instances down with them.
        public Transform HitPlayerPoolHolder;
        [Space]
        public TMP_Text PauseLabel;

        [Space]
        [Header("Samples")]
        public LaneGroupPlayer LaneGroupSample;

        public LanePlayer        LaneSample;
        public HitPlayer         HitSample;
        public MeshRenderer      HoldSample;
        public JudgeScreenEffect JudgeScreenSample;

        [Space]
        public Mesh FreeFlickIndicator;

        public Mesh ArrowFlickIndicator;

        [Space]
        public AudioClip NormalHitsound;

        public AudioClip CatchHitsound;
        public AudioClip FlickHitsound;

        [Header("Values")]
        public float Speed = 121;

        [Space]
        public float PerfectWindow = 0.05f;

        public float GoodWindow = 0.15f;
        public float BadWindow  = 0.2f;
        public float PassWindow = 0.1f;

        [Space]
        public float SyncThreshold = 0.05f;

        [Space]
        public float MinimumRadius = 180f;

        public Vector2 ScreenUnit;

        [Space]
        [Header("Data")]
        public bool IsReady;

        public bool IsPlaying;

        [FormerlySerializedAs("HasPlayedBefore")]
        public bool AlreadyInitialised;

        // Double precision to avoid float drift on long songs
        public double CurrentTime = -5;

        [Space]
        public float TotalExScore = 0;

        public float CurrentExScore = 0;

        [Space]
        public int PerfectCount = 0;

        public int GoodCount = 0;
        public int BadCount  = 0;

        [Space]
        public int Combo = 0;

        public int MaxCombo   = 0;
        public int TotalCombo = 0;

        [Space]
        public List<HitObjectHistoryItem> HitObjectHistory;

        // Median of non-zero, non-infinity Timing offsets — attach to result screen in scene.
        // Updated after every qualifying hit so it's always current.
        public double MedianTimingOffset;

        [Space]
        public int HitsRemaining = 0;

        [HideInInspector]
        public List<LaneStyleManager> LaneStyles = new();

        [HideInInspector]
        public List<HitStyleManager> HitStyles = new();

        [HideInInspector]
        public List<LaneGroupPlayer> LaneGroups = new();

        [HideInInspector]
        public List<LanePlayer> Lanes = new();

        // Lanes not yet relevant enough to be updated every frame, sorted ascending by
        // CueTime. _PendingLaneCursor only ever moves forward — each lane is examined
        // exactly once, right when it becomes due, instead of re-scanning the whole
        // (potentially 1000+ lane) list every frame to find who's newly relevant.
        private readonly List<(float CueTime, LanePlayer Lane)> _PendingLanes = new();
        private int _PendingLaneCursor;

        private double _LastDSPTime;
        private double _MusicStartDSP;  // DSP time at which Music.PlayScheduled was called
        private const double _SPIKE_THRESHOLD = 0.1; // 100ms — above this, treat as a spike

        // Frame skipping: skip visual update when logic ran less than this ago
        private const float _FRAME_SKIP_THRESHOLD = 1f / 15f; // skip visual if >15 fps worth of logic work done
        private float _LastVisualTime = float.NegativeInfinity;

        // Visual lerp: smooth camera/lane group positions across frames
        private Vector3    _LerpedCameraPos;
        private Quaternion _LerpedCameraRot;
        private float      _LerpedCameraDist;
        private bool       _VisualLerpInitialised;

        [NonSerialized]
        public PlayerSettings Settings = new();

        [NonSerialized]
        public float ScaledExtraRadius;

        [NonSerialized]
        public float ScaledMinimumRadius;
        
        internal List<int>        TransparentMeshLaneIndexes  = new();
        internal List<int>        TransparentMeshJudgeIndexes = new();
        private  List<LanePlayer> _LanesToRender              = new();
        

        public void Awake()
        {
            sMain = this;
            CommonScene.Load();
        }

        public void OnDestroy()
        {
            if (sMain == this)
                sMain = null;
        }

        public void Start()
        {
            InitFlickMeshes();
            SetInterfaceColor(Color.clear);
            SongProgress.value = 0;
            PrewarmHitPlayerPool();

            StartCoroutine(LoadChart());
        }

        // -----------------------------------------------------------------
        // HitPlayer pool — avoids an Instantiate/Destroy pair per note.
        // Pre-warmed to a size that covers typical dense charts; grows on
        // demand (no hard cap) since note density varies a lot per chart.
        // -----------------------------------------------------------------
        private const int HitPlayerPoolPrewarm = 64;
        private readonly Stack<HitPlayer> _HitPlayerPool = new();

        private void PrewarmHitPlayerPool()
        {
            for (int i = 0; i < HitPlayerPoolPrewarm; i++)
            {
                HitPlayer player = Instantiate(HitSample, HitPlayerPoolHolder);
                player.gameObject.SetActive(false);
                _HitPlayerPool.Push(player);
            }
        }

        public HitPlayer BorrowHitPlayer(Transform parent)
        {
            HitPlayer player = _HitPlayerPool.Count > 0
                ? _HitPlayerPool.Pop()
                : Instantiate(HitSample, HitPlayerPoolHolder);

            player.transform.SetParent(parent);
            player.gameObject.SetActive(true);
            return player;
        }

        public void ReturnHitPlayer(HitPlayer player)
        {
            player.IsReturned = true;

            if (player.HoldMesh != null)
            {
                if (player.HoldMesh.mesh != null)
                    player.HoldMesh.mesh.Clear();
                player.HoldMesh.gameObject.SetActive(false);
            }

            player.gameObject.SetActive(false);
            player.transform.SetParent(HitPlayerPoolHolder);
            _HitPlayerPool.Push(player);
        }

        private         int  _TotalObjects;
        private         int  _LoadedObjects;
        internal static bool sHeadlessInitialised = false;

        private void Update_LoadingBarHolder(int increments, string currentProgress)
        {
            if (sHeadlessInitialised) return;

            _LoadedObjects += increments;

            // SongSelectReadyScreen.sMain.OverallProgress.text = $"{_LoadedObjects}/{_TotalObjects}";
            // SongSelectReadyScreen.sMain.CurrentProgress.text = currentProgress;

            // SongSelectReadyScreen.sMain.Bar.value = (float)_LoadedObjects / _TotalObjects;
        }

        private IEnumerator LoadChart()
        {
            // If TargetSong is empty, use Headless initialisation
            if (sTargetSong == null)
            {
                HeadlessPlayerStarter.sChart = Chart;
                HeadlessPlayerStarter.sRunChart = RunChart;

                CommonSys.Load("HeadlessPlayerStarter", null, null);

                SceneManager.UnloadSceneAsync("Player");
                Resources.UnloadUnusedAssets();
            }
            else
            {
                if (sTargetChartMeta == null)
                    throw new InvalidOperationException("Target chart metadata is missing before chart load.");

                SongNameLabel.text = sTargetSong.SongName;
                SongArtistLabel.text = sTargetSong.SongArtist;

                DifficultyNameLabel.text = sTargetChartMeta.DifficultyName;
                DifficultyLabel.text = sTargetChartMeta.DifficultyLevel;

                if (!sHeadlessInitialised)
                {
                    // SongSelectReadyScreen.sMain.LoadingBarHolder.gameObject.SetActive(true);
                    // SongSelectReadyScreen.sMain.OverallProgress.text = "0/0";
                    // SongSelectReadyScreen.sMain.CurrentProgress.text = "Loading audio file...";
                }

                if (sTargetSong.Clip == null)
                    throw new InvalidOperationException($"Target song '{sTargetSong.SongName}' is missing its audio clip.");

                sTargetSong.Clip.LoadAudioData();

                if (sHeadlessInitialised && sTargetChart != null)
                {
                    yield return new WaitUntil(() => sTargetSong.Clip.loadState == AudioDataLoadState.Loaded);
                }
                else
                {
                    string path = Path.Combine(Path.GetDirectoryName(sTargetSongPath)!, sTargetChartMeta.Target);
                    Debug.Log(path);
                    ResourceRequest chartLoadRequest = Resources.LoadAsync<ExternalChart>(path);

                    yield return new WaitUntil(() => chartLoadRequest.isDone && sTargetSong.Clip.loadState == AudioDataLoadState.Loaded);

                    sTargetChart = chartLoadRequest.asset as ExternalChart;

                    if (sTargetChart == null)
                        throw new InvalidOperationException($"Failed to load chart asset at Resources path '{path}'.");
                }

                if (!sHeadlessInitialised)
                    // SongSelectReadyScreen.sMain.CurrentProgress.text += "Done.";

                    if (sHeadlessInitialised)
                    {
                        yield return null;
                        yield return new WaitForEndOfFrame();
                    }

                yield return InitChart();
            }
        }

        private bool[] _LoadState = new bool[2]; // [IsFinished, WithErrors]

        public IEnumerator InitChart()
        {
            sCurrentChart = sTargetChart.Data.DeepClone();
            HitObjectHistory = new List<HitObjectHistoryItem>();
            
            _Met = sTargetSong.Timing;

            // Player retry reinitialisation
            if (AlreadyInitialised)
            {
                TotalExScore =
                    CurrentExScore = 0;

                PerfectCount =
                    GoodCount =
                        BadCount =
                            Combo =
                                MaxCombo =
                                    TotalCombo =
                                        HitsRemaining = 0;

                MedianTimingOffset = 0;

                ScoreCounter.SetNumber(0);
                SongProgress.value = 0;

                for (var a = 0; a < LaneStyles.Count; a++)
                    LaneStyles[a]
                        .Update(sCurrentChart.Palette.LaneStyles[a]);

                for (var a = 0; a < HitStyles.Count; a++)
                    HitStyles[a]
                        .Update(sCurrentChart.Palette.HitStyles[a]);

                for (var a = 0; a < sTargetChart.Data.Groups.Count; a++)
                    LaneGroups[a].Current = sCurrentChart.Groups.Find(x => x.Name == LaneGroups[a].name);

                foreach (LanePlayer lane in Lanes)
                {
                    Destroy(lane.gameObject);
                }

                foreach ((float _, LanePlayer lane) in _PendingLanes)
                {
                    if (lane != null)
                        Destroy(lane.gameObject);
                }

                Lanes.Clear();
                _PendingLanes.Clear();
                _PendingLaneCursor = 0;
            }
            else
            {
                _TotalObjects += sCurrentChart.Palette.LaneStyles.Count + sCurrentChart.Palette.HitStyles.Count + sCurrentChart.Groups.Count + sCurrentChart.Lanes.Count;

                foreach (LaneStyle style in sCurrentChart.Palette.LaneStyles)
                {
                    LaneStyles.Add(new LaneStyleManager(style));
                    Update_LoadingBarHolder(1, $"Loading lanestyle {style.Name}...({LaneStyles.Count} of {sCurrentChart.Palette.LaneStyles.Count})");
                }

                for (var i = 0; i < sCurrentChart.Palette.LaneStyles.Count; i++)
                {
                    LaneStyle laneStyle = sCurrentChart.Palette.LaneStyles[i];

                    // For mesh culling on invisible lanes
                    if (laneStyle.LaneColor.a == 0)
                        TransparentMeshLaneIndexes.Add(i);

                    TransparentMeshLaneIndexes.Remove(-1);
                }

                foreach (HitStyle style in sCurrentChart.Palette.HitStyles)
                {
                    HitStyles.Add(new HitStyleManager(style));
                    Update_LoadingBarHolder(1, $"Loading hitstyle {style.Name}...({HitStyles.Count} of {sCurrentChart.Palette.HitStyles.Count})");
                }

                StartCoroutine(LaneGroupLoader());

                yield return new WaitUntil(() => _LoadState[0]);
            }

            ComboGroup.alpha =
                JudgmentGroup.alpha = 0;

            _LoadState = new bool[2]; // reset

            float dpi = Screen.dpi == 0 ? 100 : Screen.dpi;
            float screenUnit = Mathf.Min(Screen.width / ScreenUnit.x, Screen.height / ScreenUnit.y);

            ScaledExtraRadius = dpi * 0.2f;
            ScaledMinimumRadius = MinimumRadius * screenUnit;

            StartCoroutine(LaneLoader());

            yield return new WaitUntil(() => _LoadState[0]);

            Update_LoadingBarHolder(0, $"Finished {(_LoadState[1] ? "with errors." : "!")}");

            Music.clip = sTargetSong.Clip;
            Music.volume = Settings.BackgroundMusicVolume;
            CurrentTime = -5;
            PlayerScreenPause.sMain.PauseTime = -10;
            IsReady = true;
            AlreadyInitialised = true;
            _LastDSPTime = AudioSettings.dspTime;
            _MusicStartDSP = 0; // Will be set when music is actually scheduled

            const float TARGET_ASPECT = 7 / 4f;
            float targetHeight = Mathf.Min(Screen.height, Screen.width / TARGET_ASPECT);
            float camRatio = targetHeight / Screen.height;
            CommonSys.sMain.MainCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator LaneLoader()
        {
            int loadedLanes = 0;
            bool[] instantiatedLane = new bool[sTargetChart.Data.Lanes.Count];

            while (loadedLanes < sTargetChart.Data.Lanes.Count)
            {
                var progressMade = false;

                for (var a = 0; a < sTargetChart.Data.Lanes.Count; a++)
                {
                    if (instantiatedLane[a])
                    {
                        Debug.Log($"[LaneInit] Skipping lane {a}, already instantiated.");

                        continue;
                    }

                    Lane laneData = sTargetChart.Data.Lanes[a];
                    bool noParent = string.IsNullOrEmpty(laneData.Group);

                    LaneGroupPlayer laneInGroup = noParent
                        ? null : LaneGroups.Find(x => x.Original.Name == laneData.Group);

                    // Debug.Log($"[LaneInit] Processing lane {a}: group = '{laneData.Group}'");

                    // Only continue if no parent is needed, or parent exists
                    if (!noParent && laneInGroup == null)
                    {
                        Debug.LogWarning($"Lane [{a}] references non-existent(yet) '{laneData.Group}' in LaneGroups. Delaying.");

                        continue;
                    }

                    LanePlayer instancedLane = Instantiate(
                        LaneSample,
                        noParent
                            ? Holder : laneInGroup.transform
                    );

                    instancedLane.Group = laneInGroup;
                    instancedLane.Original = sTargetChart.Data.Lanes[a];
                    instancedLane.Current = sCurrentChart.Lanes[a];

                    instancedLane.Init();

                    // Defer this lane until it's actually about to be relevant instead of
                    // updating it every frame from the moment it's loaded — see CueTime.
                    const float VISIBILITY_DISTANCE = 200f;
                    const float GRACE_TIME = 5f;
                    float laneSpeed = Math.Abs(instancedLane.Current.LaneSteps[0].Speed) * Speed;
                    float cueTime = laneSpeed > 0.0001f
                        ? instancedLane.TimeStamps[0] - VISIBILITY_DISTANCE / laneSpeed - GRACE_TIME
                        : float.NegativeInfinity;
                    _PendingLanes.Add((cueTime, instancedLane));

                    foreach (HitObject laneHitobject in instancedLane.Original.Objects)
                    {
                        // Add ExScore by note type
                        TotalExScore += laneHitobject.Type == HitObject.HitType.Normal
                            ? 3 : 1;

                        TotalExScore += Mathf.Ceil(laneHitobject.HoldLength / 0.5f);

                        if (laneHitobject.Flickable)
                        {
                            TotalExScore += 1;

                            if (!float.IsNaN(laneHitobject.FlickDirection))
                                TotalExScore += 1;
                        }

                        // Set camera to hitobject's beat position
                        CameraController hitObjectCamera = (CameraController)sTargetChart.Data.Camera.GetStoryboardableObject(laneHitobject.Offset);
                        Pseudocamera.transform.localPosition = hitObjectCamera.CameraPivot;
                        Pseudocamera.transform.localEulerAngles = hitObjectCamera.CameraRotation;
                        Pseudocamera.transform.Translate(Vector3.back * hitObjectCamera.PivotDistance);

                        // Get lane state at hitobject's beat
                        Lane lane = (Lane)instancedLane.Original.GetStoryboardableObject(laneHitobject.Offset);
                        LanePosition positionStep = lane.GetLanePosition(laneHitobject.Offset, laneHitobject.Offset, sTargetSong.Timing);

                        Quaternion laneRot = Quaternion.Euler(lane.Rotation);
                        Vector3 startLocal = laneRot * positionStep.StartPosition + lane.Position;
                        Vector3 endLocal   = laneRot * positionStep.EndPosition   + lane.Position;

                        var groupChain = new List<LaneGroupPlayer>();
                        LaneGroupPlayer g = laneInGroup;
                        while (g) { groupChain.Add(g); g = g.Parent; }
                        groupChain.Reverse();

                        foreach (var grp in groupChain)
                        {
                            LaneGroup sampled = (LaneGroup)grp.Original.GetStoryboardableObject(laneHitobject.Offset);
                            grp.transform.localPosition    = sampled.Position;
                            grp.transform.localEulerAngles = sampled.Rotation;
                        }

                        Vector3 startPos = laneInGroup != null ? laneInGroup.transform.TransformPoint(startLocal) : startLocal;
                        Vector3 endPos   = laneInGroup != null ? laneInGroup.transform.TransformPoint(endLocal)   : endLocal;

                        HitObject hitObject = (HitObject)laneHitobject.GetStoryboardableObject(laneHitobject.Offset);
                        Vector2 hitStart = Pseudocamera.WorldToScreenPoint(Vector3.LerpUnclamped(startPos, endPos, hitObject.Position));
                        Vector2 hitEnd   = Pseudocamera.WorldToScreenPoint(Vector3.LerpUnclamped(startPos, endPos, hitObject.Position + laneHitobject.Length));

                        float radius = Vector2.Distance(hitStart, hitEnd) / 2 + ScaledExtraRadius;

                        instancedLane.HitCoords.Add(new HitScreenCoord
                        {
                            Position = (hitStart + hitEnd) / 2,
                            Radius = Mathf.Max(radius, ScaledMinimumRadius)
                        });
                    }

                    HitsRemaining += instancedLane.Original.Objects.Count;

                    loadedLanes++;
                    instantiatedLane[a] = true;
                    progressMade = true;

                    Update_LoadingBarHolder(1, $"Loading lane {a}...({loadedLanes} of {sTargetChart.Data.Lanes.Count})");

                    // yield return new WaitForEndOfFrame();
                }

                if (!progressMade)
                {
                    // Try loading the lanegroup again once
                    yield return LaneGroupLoader();
                    yield return this;

                    var err = 0;
                    List<string> errDetails = new();

                    for (var i = 0; i < instantiatedLane.Length; i++)
                        if (!instantiatedLane[i])
                        {
                            err++;
                            // Use the source chart data here, not Lanes[i] — a lane that
                            // failed to instantiate was never added to Lanes, so Lanes[i]
                            // would refer to an unrelated (or out-of-range) lane.
                            Lane failedLane = sTargetChart.Data.Lanes[i];
                            errDetails.Add($"Lane {(string.IsNullOrEmpty(failedLane.Name) ? i.ToString() : failedLane.Name)} depends on {failedLane.Group}");
                        }

                    string f_printDepDetails()
                    {
                        var final = string.Empty;

                        for (var a = 0; a < errDetails.Count; a++)
                            if (a == errDetails.Count - 1)
                                final += string.Concat(errDetails[a]);
                            else
                                final += string.Concat(errDetails[a], "\n");

                        return final;
                    }

                    Debug.LogError($"Failed to resolve {err} lane dependencies. Possible circular or missing references. {(errDetails.Count > 0 ? "\n" + f_printDepDetails() : string.Empty)}");

                    Update_LoadingBarHolder(Math.Abs(sTargetChart.Data.Lanes.Count - loadedLanes), $"Failed to load {err} lanes!");
                    _LoadState[1] = true;

                    yield return new WaitForSeconds(0.5f);

                    break;
                }
                else if (Settings.HighlightSimulNotes)
                {
                    yield return SimulNoteChecker();
                }
            }

            // One-time sort by CueTime so LateUpdate's promotion cursor can just walk
            // forward from index 0 instead of scanning for the next-due lane every frame.
            _PendingLanes.Sort((a, b) => a.CueTime.CompareTo(b.CueTime));

            _LoadState[0] = true;

            yield return new WaitForEndOfFrame();
        }

        private struct EventNote
        {
            public float      Beat;
            public HitObject  HitObject;
            public LanePlayer Lane;

        }

        private IEnumerator SimulNoteChecker()
        {
            Debug.Log("SimulNoteChecker started");
            
            
            var events = new List<EventNote>();
            
            foreach (var lane in Lanes)
            foreach (var hitObject in lane.Original.Objects)
            {
                events.Add(new EventNote(){Beat = hitObject.Offset, HitObject = hitObject, Lane = lane});
                hitObject.IsSimultaneous = false;
            }

            events.Sort((a, b) => a.Beat.CompareTo(b.Beat));
            
            for (int i = 0; i < events.Count;)
            {
                float currentBeat = events[i].Beat;
                int start = i;

                // Move index i to the end of the group with the same beat
                // Using Mathf.Approximately just in case of float precision jitter, 
                // but for grid-based data, it works exactly like ==
                while (i < events.Count && Mathf.Approximately(events[i].Beat, currentBeat))
                {
                    i++;
                }

                int count = i - start;

                // If more than one note exists at this exact beat
                if (count > 1)
                {
                    Debug.Log($"> Simultaneous notes found at beat {currentBeat}: {count}");
                    for (int j = start; j < i; j++)
                    {
                        events[j].HitObject.IsSimultaneous = true;
                    }
                }
            }
// ... existing c


            yield break; // Satisfies the IEnumerator return requirement
        }
    
        private IEnumerator LaneGroupLoader()
        {
            var loadedLaneGroups = 0;
            var isLoaded = new bool[sTargetChart.Data.Groups.Count];

            while (loadedLaneGroups < sTargetChart.Data.Groups.Count)
            {
                var progressMade = false;

                for (var i = 0; i < sTargetChart.Data.Groups.Count; i++)
                {
                    if (isLoaded[i])
                        continue;

                    LaneGroup laneGroup = sTargetChart.Data.Groups[i];

                    bool hasNoParent = string.IsNullOrEmpty(laneGroup.Group);
                    bool parentFound = LaneGroups.Exists(x => x.Current.Name == laneGroup.Group); // Stays false until the parent is instantiated

                    if (hasNoParent || parentFound)
                    {
                        Transform parentTransform = hasNoParent
                            ? Holder
                            : LaneGroups.Find(x => x.Current.Name == laneGroup.Group)
                                .transform;

                        LaneGroupPlayer instancedLaneGroup = Instantiate(LaneGroupSample, parentTransform);

                        instancedLaneGroup.Original = sTargetChart.Data.Groups[i];
                        instancedLaneGroup.Current = sCurrentChart.Groups[i];
                        instancedLaneGroup.gameObject.name = instancedLaneGroup.Current.Name;
                        instancedLaneGroup.Parent = hasNoParent
                            ? null 
                            : LaneGroups.Find(x => x.Current.Name == laneGroup.Group);

                        LaneGroups.Add(instancedLaneGroup);
                        isLoaded[i] = true;
                        loadedLaneGroups++;
                        progressMade = true;

                        Update_LoadingBarHolder(1, $"Loading lanegroup {laneGroup.Name}...({loadedLaneGroups} of {sTargetChart.Data.Groups.Count})");
                    }

                    // Intentional throttling maybe idk
                    // yield return new WaitForEndOfFrame();
                }

                if (!progressMade)
                {
                    var err = 0;
                    List<string> errDetails = new();

                    for (var i = 0; i < isLoaded.Length; i++)
                        if (!isLoaded[i])
                        {
                            err++;
                            errDetails.Add($"{LaneGroups[i].Current.Name} depends on {LaneGroups[i].Current.Group}");
                        }

                    string f_printDepDetails()
                    {
                        var final = string.Empty;

                        for (var a = 0; a < errDetails.Count; a++)
                            if (a == errDetails.Count - 1)
                                final += string.Concat(errDetails[a]);
                            else
                                final += string.Concat(errDetails[a], "\n");

                        return final;
                    }

                    Debug.LogError($"Failed to resolve {err} lane group dependencies. Possible circular or missing references. {(errDetails.Count > 0 ? "\n" + f_printDepDetails() : string.Empty)}");

                    Update_LoadingBarHolder(Math.Abs(sTargetChart.Data.Groups.Count - loadedLaneGroups), $"Failed to load {err} lanegroups!");

                    _LoadState[1] = true;

                    break;
                }
            }

            _LoadState[0] = true;

            yield return new WaitForEndOfFrame();
        }

        public void BeginReadyAnim()
        {
            StartCoroutine(ReadyAnim());
        }

        public IEnumerator ReadyAnim()
        {
            for (var a = 0; a < ScoreCounter.Digits.Count; a++)
            {
                ScoreCounter.Digits[a]
                    .list.Clear();

                ScoreCounter.Digits[a].Speed = 3;

                for (int b = 9 - a; b <= 10; b++)
                    ScoreCounter.Digits[a]
                        .list.Add((b % 10).ToString());
            }

            yield return Ease.Animate(1.5f, (x) =>
            {
                float ease = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);

                SetInterfaceColor(sTargetSong.InterfaceColor * new Color(1, 1, 1, ease));

                PlayerHUD.transform.localScale = Vector3.one * (ease * .05f + .95f);
            });

            foreach (ScrollingCounterDigit digit in ScoreCounter.Digits)
                digit.Speed = 9;

            IsPlaying = true;

            // Idk why but songs are starting 3/4 of a second later than they's supposed to be 
            // so this will be here as a temporary hack before I figure out what is going on
            _MusicStartDSP = AudioSettings.dspTime + Math.Max(CurrentTime - 0.75, 0);
            Music.time = 0;
            Music.PlayScheduled(_MusicStartDSP);
            _LastDSPTime = AudioSettings.dspTime;
        }

        public bool ResultExec = false;

        private Metronome _Met;
        
        public void Update()
        {
            if (!IsPlaying)
                return;
            
            #if UNITY_EDITOR
            // Toolbar pause — freeze clock and audio, return early
            if (EditorApplication.isPaused)
            {
                Music.Pause();
                _LastDSPTime = AudioSettings.dspTime;
                return;
            }
            #endif

            double dspNow = AudioSettings.dspTime;
            double rawDelta = dspNow - _LastDSPTime;
            _LastDSPTime = dspNow;

            if (rawDelta > _SPIKE_THRESHOLD)
            {
                // Lag spike — resync clock to audio without rushing the chart
                if (Music.isPlaying)
                    CurrentTime = (double)Music.timeSamples / Music.clip.frequency + Settings.AudioOffset;
                // else: lead-in / post-song — just drop the frame, don't advance
            }
            else
            {
                if (Music.isPlaying && CurrentTime >= 0)
                    // Audio is the source of truth during playback
                    CurrentTime = (double)Music.timeSamples / Music.clip.frequency + Settings.AudioOffset;
                else
                    CurrentTime += rawDelta > 0 ? rawDelta : Time.unscaledDeltaTime;
            }

            // Audio lifecycle — use PlayScheduled on restart to avoid buffer-boundary snap
            if (CurrentTime >= 0 && CurrentTime < Music.clip.length)
            {
                if (!Music.isPlaying && !ResultExec)
                {
                    const double RESTART_LEAD_TIME = 0.05;
                    _MusicStartDSP = dspNow + RESTART_LEAD_TIME;
                    Music.time = (float)(CurrentTime - Settings.AudioOffset);
                    Music.PlayScheduled(_MusicStartDSP);
                }
                // No hard-seek sync corrector needed — audio IS the clock now
            }
            else if (Music.isPlaying)
                Music.Pause();

            // Process input directly — no coroutine, no +1 frame latency
            sr_HoldMeshLoop.Begin();
            foreach (LanePlayer lane in Lanes)
                foreach (HitPlayer hit in lane.HitObjects)
                    // HoldMesh is now a permanent (pooled) child, so its existence no longer
                    // implies this note is a hold — check the actual note data instead.
                    if (hit.Current.HoldLength > 0)
                        lane.UpdateHoldMesh(hit);
            sr_HoldMeshLoop.End();

            sr_UpdateInputCall.Begin();
            PlayerInputManager.sInstance.UpdateInput();
            sr_UpdateInputCall.End();

            // Show ending animation; the failsafe on bugs is on following:
            // Remaining total hitobject AND Current input's hold -> Remaining lane count -> End of song
            if (((HitsRemaining <= 0 && PlayerInputManager.sInstance.HoldQueue.Count == 0) || Lanes.Count == 0 || (float)CurrentTime / Music.clip.length >= 1) && !ResultExec)
            {
                ComputeAndSaveMedianOffset();
                PlayerScreenResult.sMain.StartEndingAnim();
                ResultExec = true;
            }
        }

        public void LateUpdate()
        {
            if (!IsPlaying)
                return;

#if UNITY_EDITOR
            if (EditorApplication.isPaused) return;
#endif

            SongProgress.value = (float)(CurrentTime / Music.clip.length);

            float visualTime = (float)CurrentTime + Settings.VisualOffset;
            float visualBeat = sTargetSong.Timing.ToBeat(visualTime);

            sCurrentChart.Palette.Advance(visualBeat);

            if (CommonSys.sMain.MainCamera.backgroundColor != sCurrentChart.Palette.BackgroundColor)
                SetBackgroundColor(sCurrentChart.Palette.BackgroundColor);

            if (SongNameLabel.color != sCurrentChart.Palette.InterfaceColor)
                SetInterfaceColor(sCurrentChart.Palette.InterfaceColor);

            sr_LaneStylesUpdate.Begin();
            for (var a = 0; a < LaneStyles.Count; a++)
            {
                sCurrentChart.Palette.LaneStyles[a].Advance(visualBeat);
                LaneStyles[a].Update(sCurrentChart.Palette.LaneStyles[a]);

                if (sCurrentChart.Palette.LaneStyles[a].LaneColor.a != 0 && TransparentMeshLaneIndexes.Contains(a))
                    TransparentMeshLaneIndexes.Remove(a);
                else if (sCurrentChart.Palette.LaneStyles[a].LaneColor.a == 0 && !TransparentMeshLaneIndexes.Contains(a))
                    TransparentMeshLaneIndexes.Add(a);

                if (sCurrentChart.Palette.LaneStyles[a].JudgeColor.a == 0 && !TransparentMeshJudgeIndexes.Contains(a))
                    TransparentMeshJudgeIndexes.Add(a);
                else if (sCurrentChart.Palette.LaneStyles[a].JudgeColor.a != 0 && TransparentMeshJudgeIndexes.Contains(a))
                    TransparentMeshJudgeIndexes.Remove(a);
            }
            sr_LaneStylesUpdate.End();

            sr_HitStylesUpdate.Begin();
            for (var a = 0; a < HitStyles.Count; a++)
            {
                sCurrentChart.Palette.HitStyles[a].Advance(visualBeat);
                HitStyles[a].Update(sCurrentChart.Palette.HitStyles[a]);
            }
            sr_HitStylesUpdate.End();

            sr_CameraAdvance.Begin();
            sCurrentChart.Camera.Advance(visualBeat);

            Camera pseudoCamera = CommonSys.sMain.MainCamera;
            pseudoCamera.transform.position = sCurrentChart.Camera.CameraPivot;
            pseudoCamera.transform.eulerAngles = sCurrentChart.Camera.CameraRotation;
            pseudoCamera.transform.Translate(Vector3.back * sCurrentChart.Camera.PivotDistance);
            sr_CameraAdvance.End();

            foreach (LaneGroupPlayer group in LaneGroups)
                group.UpdateSelf(visualTime, visualBeat);

            // Promote pending lanes into the active set once they're due — each lane is
            // examined exactly once here, right as it becomes relevant, instead of every
            // lane being rechecked every frame regardless of how far away it still is.
            while (_PendingLaneCursor < _PendingLanes.Count &&
                   _PendingLanes[_PendingLaneCursor].CueTime <= visualTime)
            {
                Lanes.Add(_PendingLanes[_PendingLaneCursor].Lane);
                _PendingLaneCursor++;
            }

            sr_LanesLoop.Begin();
            for (int i = Lanes.Count - 1; i >= 0; i--)
            {
                try
                {
                    LanePlayer lane = Lanes[i];

                    lane.UpdateSelf(visualTime, visualBeat);

                    if (lane == null || lane.MarkedForRemoval)
                    {
                        Lanes.RemoveAt(i);
                        Debug.Log($"[LaneRemove] Removed lane {i} from scene.");
                    }
                }
                catch (MissingReferenceException)
                {
                    Debug.LogWarning($"[LaneRemove] Lane {i} is null.");
                    Lanes.RemoveAt(i);
                }
            }
            sr_LanesLoop.End();
        }

        public void Resync()
        {
            _LastDSPTime = AudioSettings.dspTime;

            // Only anchor from timeSamples when music is actively playing.
            if (Music.isPlaying && CurrentTime >= 0)
                CurrentTime = (double)Music.timeSamples / Music.clip.frequency + Settings.AudioOffset;
        }

        // Call on pause or result entry — deferred so per-hit cost is just a list append.
        public void ComputeAndSaveMedianOffset()
        {
            var samples = HitObjectHistory
                .Where(h => h.Type == HitObjectHistoryType.Timing && !double.IsInfinity(h.Offset))
                .Select(h => h.Offset)
                .OrderBy(x => x)
                .ToList();

            if (samples.Count == 0) return;

            int mid = samples.Count / 2;
            MedianTimingOffset = samples.Count % 2 == 0
                ? (samples[mid - 1] + samples[mid]) / 2.0
                : samples[mid];

            CommonSys.sMain.Preferences.Set(
                "PLYR:GameplayMedianOffset", 
                (float)(MedianTimingOffset * 1000)
            );
            CommonSys.sMain.Preferences.Set(
                "PLYR:GameplayMedianOffsetCounter",
                CommonSys.sMain.Preferences.Get("PLYR:GameplayMedianOffsetCounter", 0) + 1
            );
        }

        // Input and hold mesh updates are now handled directly in Update to avoid +1 frame latency.
        // CheckHitObjects kept as a no-op stub in case external callers reference it.
        public IEnumerator CheckHitObjects() { yield return null; }

        // Tracks whether we auto-paused due to focus loss, so we don't stomp a manual pause.
        private bool _PausedByFocusLoss;

        public void OnApplicationFocus(bool hasFocus)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) return;
#endif

            if (!hasFocus)
            {
                if (IsPlaying && !ResultExec)
                {
                    _PausedByFocusLoss = true;
                    IsPlaying = false;
                    Music.Pause();
                    _LastDSPTime = AudioSettings.dspTime;
                    ComputeAndSaveMedianOffset();
                    PlayerScreenPause.sMain.Show();
                }

                return;
            }

            if (_PausedByFocusLoss)
            {
                _PausedByFocusLoss = false;
                PlayerScreenPause.sMain.Continue();
            }
        }

        // OnApplicationPause covers mobile app backgrounding and complements OnApplicationFocus.
        public void OnApplicationPause(bool isPaused)
        {
            #if UNITY_EDITOR
                return;
            #else
                if (isPaused)
                {
                    if (IsPlaying)
                    {
                        _PausedByFocusLoss = true;
                        IsPlaying = false;
                        Music.Pause();
                        _LastDSPTime = AudioSettings.dspTime;
                        ComputeAndSaveMedianOffset();
                        PlayerScreenPause.sMain.Show();
                    }
                }
                else if (_PausedByFocusLoss)
                {
                    _PausedByFocusLoss = false;
                    PlayerScreenPause.sMain.Continue();
                }
            #endif
        }

        private Coroutine _JudgeAnimation;

        public void AddScore(float score, float? acc, double? offset = null)
        {
            CurrentExScore += score;

            ScoreCounter.SetNumber(Mathf.RoundToInt(CurrentExScore / TotalExScore * 1e6f));

            if (score > 0)
            {
                Combo++;

                MaxCombo = Mathf.Max(MaxCombo, Combo);

                if (acc == null || acc == 0)
                    PerfectCount++;
                else
                    GoodCount++;
            }
            else
            {
                Combo = 0;
                BadCount++;
            }

            ComboLabel.text = Helper.PadScore(Combo.ToString(), 4) + "<voffset=0.065em>×";

            JudgmentLabel.text = FormatJudgmentLabel(acc, offset, score);

            if (_JudgeAnimation != null)
                StopCoroutine(_JudgeAnimation);

            _JudgeAnimation = StartCoroutine(JudgmentAnim());

            TotalCombo++;
        }

        private IEnumerator JudgmentAnim()
        {
            yield return Ease.Animate(.6f, (x) =>
            {
                float val = Mathf.Pow(1 - x, 5);

                ComboGroup.alpha = Combo == 0 ? 0 : val + 1;
                ComboLabel.rectTransform.anchoredPosition *= new Vector2Frag(y: -25 + 2 * val * val);
                JudgmentLabel.rectTransform.anchoredPosition *= new Vector2Frag(y: -25 + 2 * val * val);
                JudgmentGroup.alpha = EaseUtils.ToZero(1, x, EaseFunction.Quintic, EaseMode.In);
            });
        }


        public void RemoveHitPlayer(HitPlayer hitObject)
        {
            hitObject.Lane.HitObjects.Remove(hitObject);
            HitsRemaining--;

            // Scrub every queue/cache reference before returning to the pool — a pooled
            // instance can be re-borrowed as soon as this same frame's LateUpdate, and any
            // stale reference left behind would silently alias whatever note reuses it.
            PlayerInputManager.sInstance.PurgeHitPlayer(hitObject);

            ReturnHitPlayer(hitObject);
        }

        public void Hit(HitPlayer hitObject, double offset, bool spawnEffect = true)
        {
            // In case of race condition (also guards against a pooled instance already
            // returned by another code path this same frame)
            if (!hitObject || hitObject.IsReturned)
                return;
            
            var hitType = hitObject.Current.Type;
            bool isFlickable = hitObject.Current.Flickable;
            bool isCatchType = hitType == HitObject.HitType.Catch;
            
            // Calculate base score once
            int baseScore = CalculateBaseScore(hitType, isFlickable, hitObject.Current.FlickDirection);
            
            double offsetAbs = Math.Abs(offset);
            float? accuracy = isCatchType ? 0 : null;
            float finalScore;

            // Handle different hit evaluation types
            if (isFlickable || isCatchType)
            {
                // Binary hit/miss evaluation
                bool isHit = offsetAbs <= PassWindow;
                finalScore = isHit ? baseScore : 0;
                float historyOffset = isHit ? 0 : float.PositiveInfinity;
                
                AddScore(finalScore, null);
                HitObjectHistory.Add(new HitObjectHistoryItem(hitObject, historyOffset));
            }
            else
            {
                accuracy = CalculateAccuracy(offset, offsetAbs);
                finalScore = baseScore * (1 - Mathf.Abs(accuracy.Value));
                
                AddScore(finalScore, accuracy);
                
                HitObjectHistory.Add(new HitObjectHistoryItem(hitObject, offset));
            }

            // Spawn effects and play sounds
            if (spawnEffect)
            {
                SpawnHitEffect(hitObject, accuracy);
                PlayHitSounds(hitObject, accuracy);
            }

            // Handle hit object lifecycle
            HandleHitObjectCompletion(hitObject);
        }

        private int CalculateBaseScore(HitObject.HitType hitType, bool isFlickable, float flickDirection)
        {
            int score = hitType == HitObject.HitType.Normal ? 3 : 1;
            
            if (isFlickable)
            {
                score += 1;
                if (!float.IsNaN(flickDirection)) // Directional flick bonus
                    score += 1;
            }
            
            return score;
        }

        private float CalculateAccuracy(double offset, double offsetAbs)
        {
            if (offsetAbs > GoodWindow)
                return Math.Sign(offset);
            
            if (offsetAbs > PerfectWindow)
                return Math.Sign(offset) * Mathf.InverseLerp(PerfectWindow, GoodWindow, (float)offsetAbs);
            
            return 0f; // Perfect hit
        }

        private void SpawnHitEffect(HitPlayer hitObject, float? accuracy)
        {
            sr_SpawnHitEffect.Begin();
            var effect = sMain.JudgeScreenManager.BorrowEffect(hitObject, accuracy, sCurrentChart.Palette.InterfaceColor);
            var rt = (RectTransform)effect.transform;
            rt.position = hitObject.HitCoord.Position;
            sr_SpawnHitEffect.End();
        }

        private void PlayHitSounds(HitPlayer hitObject, float? accuracy)
        {
            float volume = GetHitSoundVolume(accuracy);
            
            // Play primary hit sound
            var primarySound = hitObject.Current.Type == HitObject.HitType.Normal 
                ? NormalHitsound 
                : CatchHitsound;
            
            Hitsounds.PlayOneShot(primarySound, volume);
            
            // Play flick sound if applicable
            if (hitObject.Current.Flickable)
                Hitsounds.PlayOneShot(FlickHitsound, volume);
        }

        private float GetHitSoundVolume(float? accuracy)
        {
            float[] volumes = Settings.HitsoundVolume;
            
            if (volumes.Length < 3 || accuracy == null)
                return volumes[0];
            
            float accValue = Mathf.Abs(accuracy.Value);
            
            if (accValue >= 1f)
                return volumes[2]; // Miss/Bad
            if (accValue > 0f)
                return volumes[1]; // Good
            
            return volumes[0]; // Perfect
        }

        private void HandleHitObjectCompletion(HitPlayer hitObject)
        {
            bool isHoldNote = hitObject.Current.HoldLength > 0;
            bool isHoldComplete = isHoldNote && Mathf.Approximately(hitObject.Time, hitObject.EndTime);
            
            if (!isHoldNote || isHoldComplete)
                RemoveHitPlayer(hitObject);
            else
            {
                hitObject.IsProcessed = true;
                hitObject.SimultaneousHighlight.gameObject.SetActive(false);
            }
        }

        private string FormatJudgmentLabel(float? acc, double? offset, float score)
        {
            string flawlessText = Settings.ShowFlawlessText ? "FLAWLESS" : "✓";

            double offsetValue = offset.HasValue ? offset.Value * 1000 : double.NaN;
            string text;

            if (!acc.HasValue)
                text = score > 0 ? flawlessText : "MISS";
            else if (acc == 0)
                text = flawlessText;
            else if (acc < 0)
                text = score > 0 ? (Settings.NoEarlyLateText ? "MISALIGNED" : "EARLY") : "BAD";
            else if (score > 0)
                text = Settings.NoEarlyLateText ? "MISALIGNED" : "LATE";
            else
                text = "MISS";

            if (offset != null && Settings.ShowValueText >= (acc == 0 ? 3 : 2) && !double.IsInfinity(offset.Value) && Math.Abs(offset.Value) >= 0.005)                                                                                        
                text += offset > 0 ? $"(+{offset.Value:0.##}ms)" : $"({offset.Value:0.##}ms)";                                                                                                                                                
            
            return text;
        }

        public void SetBackgroundColor(Color color)
        {
            CommonSys.sMain.MainCamera.backgroundColor = color;
            RenderSettings.fogColor = color;
        }

        public void SetInterfaceColor(Color color)
        {
            SongNameLabel.color = SongArtistLabel.color = DifficultyLabel.color = DifficultyNameLabel.color =
                SongProgressTip.color = SongProgressGlow.color = JudgmentLabel.color = ComboLabel.color =
                    PauseLabel.color = color;

            foreach (Graphic digit in ScoreDigits)
                digit.color = color;

            SongProgressBody.color = color * new Color(1, 1, 1, .5f);
        }

        private void InitFlickMeshes()
        {
            return; // Asset already assigned

            if (!FreeFlickIndicator)
            {
                Mesh mesh = new();
                List<Vector3> verts = new();
                List<int> tris = new();

                verts.AddRange(new Vector3[] { new(-1, 0), new(0, 2), new(0, -.5f), new(1, 0), new(0, -2), new(0, .5f) });
                tris.AddRange(new[] { 0, 1, 2, 3, 4, 5 });

                for (var i = 0; i < verts.Count; i++) verts[i] = verts[i] * Settings.FlickScale;

                mesh.SetVertices(verts);
                mesh.SetUVs(0, verts);
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals();
                FreeFlickIndicator = mesh;
            }

            if (!ArrowFlickIndicator)
            {
                Mesh mesh = new();
                List<Vector3> verts = new();
                List<int> tris = new();

                verts.AddRange(new Vector3[] { new(-1, 0), new(0, 2.2f), new(1, 0), new(.71f, -.71f), new(0, -1), new(-.71f, -.71f) });
                tris.AddRange(new[] { 0, 1, 2, 2, 3, 0, 3, 4, 0, 4, 5, 0 });

                for (var i = 0; i < verts.Count; i++) verts[i] *= Settings.FlickScale;

                mesh.SetVertices(verts);
                mesh.SetUVs(0, verts);
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateNormals();
                ArrowFlickIndicator = mesh;
            }

            //AssetDatabase.CreateAsset(FreeFlickIndicator, "Assets/JANOARG/Resources/Meshes/FreeFlickIndicator.asset");
            //AssetDatabase.CreateAsset(ArrowFlickIndicator, "Assets/JANOARG/Resources/Meshes/ArrowFlickIndicator.asset");
        }
    }

    public class PlayerSettings
    {
        public float   BackgroundMusicVolume;
        public float[] HitsoundVolume;

        public float[] HitObjectScale;
        public float   FlickScale;

        public float AudioOffset;
        public float JudgmentOffset;
        public float VisualOffset;
        public short ShowValueText;
        public bool  ShowFlawlessText;
        public bool  NoEarlyLateText;
        public bool  HighlightSimulNotes;


        public PlayerSettings()
        {
            Storage prefs = CommonSys.sMain != null ? CommonSys.sMain.Preferences : null;

            if (prefs == null) return;
            HighlightSimulNotes = CommonSys.sMain.Preferences.Get("PLYR:HighlightSimulNotes", true);
            ShowFlawlessText= CommonSys.sMain.Preferences.Get("PLYR:JudgementTextOnFlawless", true);
            NoEarlyLateText = CommonSys.sMain.Preferences.Get("PLYR:NoEarlyLateIndicator", false);
            ShowValueText = short.Parse(CommonSys.sMain.Preferences.Get("PLYR:ShowOffset", "1"));
            
            BackgroundMusicVolume = prefs.Get("PLYR:BGMusicVolume", 100f) / 100;
            HitsoundVolume = prefs.Get("PLYR:HitsoundVolume", new[] { 60f });

            for (var a = 0; a < HitsoundVolume.Length; a++)
                HitsoundVolume[a] /= 100;

            HitObjectScale = prefs.Get("PLYR:HitScale", new[] { 1f });

            if (HitObjectScale.Length < 2)
                HitObjectScale = new[] { HitObjectScale[0], HitObjectScale[0] };

            FlickScale = prefs.Get("PLYR:FlickScale", 1f);

            AudioOffset = prefs.Get("PLYR:AudioOffset", 0f) / 1000;
            JudgmentOffset = prefs.Get("PLYR:JudgmentOffset", 0f) / 1000;
            VisualOffset = prefs.Get("PLYR:VisualOffset", 0f) / 1000;
        }
    }

    public class HitObjectHistoryItem
    {
        public float                Time;
        public HitObjectHistoryType Type;
        public double                Offset;

        public HitObjectHistoryItem(HitPlayer hit, double offset)
        {
            Time = hit.Time;

            Type = hit.Current.Flickable ? HitObjectHistoryType.Flick :
                hit.Current.Type == HitObject.HitType.Catch ? HitObjectHistoryType.Catch :
                HitObjectHistoryType.Timing;

            Offset = offset;
        }

        public HitObjectHistoryItem(float time, HitObjectHistoryType type, float offset)
        {
            Time = time;
            Type = type;
            Offset = offset;
        }
    }

    public enum HitObjectHistoryType
    {
        Timing, Catch, Flick
    }
}