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
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Player
{

    public class PlayerScreen : MonoBehaviour
    {
        public static PlayerScreen sMain;

        public static string            sTargetSongPath;
        public static PlayableSong      sTargetSong;
        public static ExternalChartMeta sTargetChartMeta;
        public static ExternalChart     sTargetChart;

        public static Chart sCurrentChart;

        [Header("Headless Initialisation")]
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

        public float CurrentTime = -5;

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

        private double _LastDSPTime;

        [NonSerialized]
        public PlayerSettings Settings = new();

        [NonSerialized]
        public float ScaledExtraRadius;

        [NonSerialized]
        public float ScaledMinimumRadius;
        
        internal List<int> TransparentMeshLaneIndexes = new();
        private List<LanePlayer> _LanesToRender = new();
        public void Awake()
        {
            sMain = this;
            CommonScene.Load();
        }

        public void Start()
        {
            InitFlickMeshes();
            SetInterfaceColor(Color.clear);
            SongProgress.value = 0;

            StartCoroutine(LoadChart());
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

                string path = Path.Combine(Path.GetDirectoryName(sTargetSongPath)!, sTargetChartMeta.Target);
                Debug.Log(path);
                ResourceRequest chartLoadRequest = Resources.LoadAsync<ExternalChart>(path);

                sTargetSong.Clip.LoadAudioData();

                yield return new WaitUntil(() => chartLoadRequest.isDone && sTargetSong.Clip.loadState != AudioDataLoadState.Loading);

                if (!sHeadlessInitialised)
                    // SongSelectReadyScreen.sMain.CurrentProgress.text += "Done.";

                sTargetChart = chartLoadRequest.asset as ExternalChart;

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


                Lanes.Clear();
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
                    Lanes.Add(instancedLane);

                    float time = float.NaN;

                    Vector3 startPos = Vector3.zero,
                        endPos = Vector3.zero; // Declare 2 points of lane show in screen

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

                        if (!Mathf.Approximately(time, laneHitobject.Offset))
                        {
                            // Set camera distance and rotation to hit time of the note?
                            CameraController hitObjectCamera = (CameraController)sTargetChart.Data.Camera.GetStoryboardableObject(laneHitobject.Offset);
                            Pseudocamera.transform.localPosition = hitObjectCamera.CameraPivot;
                            Pseudocamera.transform.localEulerAngles = hitObjectCamera.CameraRotation;
                            Pseudocamera.transform.Translate(Vector3.back * hitObjectCamera.PivotDistance);

                            // Set 2 points of lane?
                            Lane lane = (Lane)instancedLane.Original.GetStoryboardableObject(laneHitobject.Offset);
                            LanePosition positionStep = lane.GetLanePosition(laneHitobject.Offset, laneHitobject.Offset, sTargetSong.Timing);
                            startPos = Quaternion.Euler(lane.Rotation) * positionStep.StartPosition + lane.Position;
                            endPos = Quaternion.Euler(lane.Rotation) * positionStep.EndPosition + lane.Position;
                            LaneGroupPlayer group = laneInGroup;

                            // Loop to get localPosition of 2 points of lane?
                            while (group)
                            {
                                LaneGroup laneGroup = (LaneGroup)group.Original.GetStoryboardableObject(laneHitobject.Offset);
                                startPos = Quaternion.Euler(laneGroup.Rotation) * startPos + laneGroup.Position;
                                endPos = Quaternion.Euler(laneGroup.Rotation) * endPos + laneGroup.Position;
                                group = group.Parent;
                            }
                        }

                        HitObject hitObject = (HitObject)laneHitobject.GetStoryboardableObject(laneHitobject.Offset); // Get current HitObject?
                        Vector2 hitStart = Pseudocamera.WorldToScreenPoint(Vector3.LerpUnclamped(startPos, endPos, hitObject.Position));
                        Vector2 hitEnd = Pseudocamera.WorldToScreenPoint(Vector3.LerpUnclamped(startPos, endPos, hitObject.Position + laneHitobject.Length));

                        float radius = Vector2.Distance(hitStart, hitEnd) / 2 + ScaledExtraRadius;

                        // Add hit coords
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
                            errDetails.Add($"Lane {(string.IsNullOrEmpty(Lanes[i].Current.Name) ? i : Lanes[i].Current.Name)} depends on {Lanes[i].Current.Group}");
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

            _LoadState[0] = true;

            yield return new WaitForEndOfFrame();
        }
        
        private struct EventNote
        {
            public float      beat;
            public HitObject  hitObject;
            public LanePlayer lane;

        }

        private IEnumerator SimulNoteChecker()
        {
            Debug.Log("SimulNoteChecker started");
            
            
            var events = new List<EventNote>();
            
            foreach (var lane in Lanes)
            foreach (var hitObject in lane.Original.Objects)
            {
                events.Add(new EventNote(){beat = hitObject.Offset, hitObject = hitObject, lane = lane});
                hitObject.IsSimultaneous = false;
            }

            events.Sort((a, b) => a.beat.CompareTo(b.beat));
            
            for (int i = 0; i < events.Count;)
            {
                float currentBeat = events[i].beat;
                int start = i;

                // Move index i to the end of the group with the same beat
                // Using Mathf.Approximately just in case of float precision jitter, 
                // but for grid-based data, it works exactly like ==
                while (i < events.Count && Mathf.Approximately(events[i].beat, currentBeat))
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
                        events[j].hitObject.IsSimultaneous = true;
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
            _LastDSPTime = AudioSettings.dspTime;
        }

        public bool ResultExec = false;

        private Metronome _Met;
        
        public void Update()
        {
            if (!IsPlaying)
                return;

            double delta = Math.Min(AudioSettings.dspTime - _LastDSPTime, PerfectWindow);
            if (delta <= 0) delta = Time.unscaledDeltaTime;
            CurrentTime += (float)delta;
            _LastDSPTime += delta;

            // Audio sync
            if (CurrentTime >= 0 && CurrentTime < Music.clip.length)
            {
                if (Music.isPlaying)
                {
                    if (Mathf.Abs(CurrentTime - (float)Music.timeSamples / Music.clip.frequency) > SyncThreshold)
                    {
                        Music.time = CurrentTime;
                    }
                    else
                    {
                        // CurrentTime = (float)Music.timeSamples / Music.clip.frequency;
                    }
                }
                else
                {
                    Music.Play();
                    Music.time = CurrentTime;
                }
            }
            else if (Music.isPlaying)
                Music.Pause();
            StartCoroutine( // Check hit objects
                        CheckHitObjects());

            // Update song progress slider
            SongProgress.value = CurrentTime / Music.clip.length;


            // Prevents from going to negative values, which might break things
            // float ChartUpdateTime(float time) => time < 0 ? 0 : time ;

            // Your code break things, great job :thumbs_up:

            float visualTime = CurrentTime + Settings.VisualOffset;
            float visualBeat = sTargetSong.Timing.ToBeat(visualTime);


            // Update palette
            sCurrentChart.Palette.Advance(visualBeat);

            if (CommonSys.sMain.MainCamera.backgroundColor != sCurrentChart.Palette.BackgroundColor)
                SetBackgroundColor(sCurrentChart.Palette.BackgroundColor);

            if (SongNameLabel.color != sCurrentChart.Palette.InterfaceColor)
                SetInterfaceColor(sCurrentChart.Palette.InterfaceColor);

            for (var a = 0; a < LaneStyles.Count; a++)
            {
                sCurrentChart.Palette.LaneStyles[a].Advance(visualBeat);

                LaneStyles[a].Update(sCurrentChart.Palette.LaneStyles[a]);

                if (sCurrentChart.Palette.LaneStyles[a].LaneColor.a != 0 && TransparentMeshLaneIndexes.Contains(a))
                    TransparentMeshLaneIndexes.Remove(a);
                else if (sCurrentChart.Palette.LaneStyles[a].LaneColor.a == 0 && !TransparentMeshLaneIndexes.Contains(a))
                    TransparentMeshLaneIndexes.Add(a);

            }

            for (var a = 0; a < HitStyles.Count; a++)
            {
                sCurrentChart.Palette.HitStyles[a].Advance(visualBeat);

                HitStyles[a].Update(sCurrentChart.Palette.HitStyles[a]);
            }

            // Update camera
            sCurrentChart.Camera.Advance(visualBeat);
            Camera pseudoCamera = CommonSys.sMain.MainCamera;
            pseudoCamera.transform.position = sCurrentChart.Camera.CameraPivot;
            pseudoCamera.transform.eulerAngles = sCurrentChart.Camera.CameraRotation;
            pseudoCamera.transform.Translate(Vector3.back * sCurrentChart.Camera.PivotDistance);

            // Update scene
            foreach (LaneGroupPlayer group in LaneGroups)
                group.UpdateSelf(visualTime, visualBeat);

            StartCoroutine(f_laneUpdater(visualTime, visualBeat));

            // Show ending animation; the failsafe on bugs is on following:
            // Remaining total hitobject AND Current input's hold -> Remaining lane count -> End of song
            if (((HitsRemaining <= 0 && PlayerInputManager.sInstance.HoldQueue.Count == 0) || Lanes.Count == 0 || CurrentTime / Music.clip.length >= 1) && !ResultExec)
            {
                PlayerScreenResult.sMain.StartEndingAnim();
                ResultExec = true;
            }

            IEnumerator f_laneUpdater(float f, float visualBeat1)
            {
                for (int i = Lanes.Count - 1; i >= 0; i--) // Iterate backwards
                {
                    try
                    {
                        LanePlayer lane = Lanes[i];

                        if (lane.TimeStamps[0] - 5f > f)
                            continue;

                        lane.UpdateSelf(f, visualBeat1);

                        if (lane.MarkedForRemoval || lane == null) // Fixed logic
                        {
                            Lanes.RemoveAt(i); // More efficient than Remove()
                            Debug.Log($"[LaneRemove] Removed lane {i} from scene.");
                        }
                    }
                    catch (MissingReferenceException)
                    {
                        Debug.LogWarning($"[LaneRemove] Lane {i} is null.");
                        Lanes.RemoveAt(i);
                    }
                }

                yield return null;
            }
        }

        public void Resync()
        {
            _LastDSPTime = AudioSettings.dspTime;   
        }

        public IEnumerator CheckHitObjects()
        {
            foreach (LanePlayer lane in Lanes)
                foreach (HitPlayer hit in lane.HitObjects)
                {
                    if (hit.HoldMesh) 
                        lane.UpdateHoldMesh(hit);
                }

            // PlayerInputManager.main.UpdateTouches();
            PlayerInputManager.sInstance.UpdateInput();
            
            yield return null;
        }

        private Coroutine _JudgeAnimation;

        public void AddScore(float score, float? acc)
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

            string flawlessText = Settings.ShowFlawlessText ? "FLAWLESS" : "✓";
            if (acc.HasValue)
                JudgmentLabel.text = acc switch
                {
                    0 => flawlessText,
                    < 0 => score > 0 ? Settings.NoEarlyLateText ? "MISALIGNED" :"EARLY" : "BAD",
                    _ => score > 0 ? Settings.NoEarlyLateText ? "MISALIGNED" : "LATE" : "MISS"
                };
            else
                JudgmentLabel.text = score > 0
                    ? flawlessText : "MISS";

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
                float val2 = 1 - Ease.Get(x, EaseFunction.Quintic, EaseMode.In);

                ComboGroup.alpha = Combo == 0 ? 0 : val + 1;
                ComboLabel.rectTransform.anchoredPosition *= new Vector2Frag(y: -25 + 2 * val * val);
                JudgmentLabel.rectTransform.anchoredPosition *= new Vector2Frag(y: -25 + 2 * val * val);
                JudgmentGroup.alpha = val2;
            });
        }


        public void RemoveHitPlayer(HitPlayer hitObject)
        {
            if (hitObject.HoldMesh != null)
            {
                Destroy(hitObject.HoldMesh.mesh);
                Destroy(hitObject.HoldMesh.gameObject);
            }

            if (hitObject.gameObject != null)
                Destroy(hitObject.gameObject);

            hitObject.Lane.HitObjects.Remove(hitObject);

            HitsRemaining--;
        }

        public void Hit(HitPlayer hitObject, float offset, bool spawnEffect = true)
        {
            // In case of race condition
            if (!hitObject)
                return;
            
            var hitType = hitObject.Current.Type;
            bool isFlickable = hitObject.Current.Flickable;
            bool isCatchType = hitType == HitObject.HitType.Catch;
            
            // Calculate base score once
            int baseScore = CalculateBaseScore(hitType, isFlickable, hitObject.Current.FlickDirection);
            
            float offsetAbs = Mathf.Abs(offset);
            float? accuracy = null;
            int finalScore;

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
                // Gradual accuracy evaluation
                accuracy = CalculateAccuracy(offset, offsetAbs);
                finalScore = Mathf.RoundToInt(baseScore * (1 - Mathf.Abs(accuracy.Value)));
                
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

        private float CalculateAccuracy(float offset, float offsetAbs)
        {
            if (offsetAbs > GoodWindow)
                return Mathf.Sign(offset);
            
            if (offsetAbs > PerfectWindow)
                return Mathf.Sign(offset) * Mathf.InverseLerp(PerfectWindow, GoodWindow, offsetAbs);
            
            return 0f; // Perfect hit
        }

        private void SpawnHitEffect(HitPlayer hitObject, float? accuracy)
        {
            var effect = sMain.JudgeScreenManager.BorrowEffect(accuracy, sCurrentChart.Palette.InterfaceColor);
            var rt = (RectTransform)effect.transform;
            rt.position = hitObject.HitCoord.Position;
            rt.localScale = new Vector3(1, 1); // Making sure it's not affected from effects that were used in hold ticks
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

        public float JudgmentOffset;
        public float VisualOffset;
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
            
            BackgroundMusicVolume = prefs.Get("PLYR:BGMusicVolume", 100f) / 100;
            HitsoundVolume = prefs.Get("PLYR:HitsoundVolume", new[] { 60f });

            for (var a = 0; a < HitsoundVolume.Length; a++)
                HitsoundVolume[a] /= 100;

            HitObjectScale = prefs.Get("PLYR:HitScale", new[] { 1f });

            if (HitObjectScale.Length < 2)
                HitObjectScale = new[] { HitObjectScale[0], HitObjectScale[0] };

            FlickScale = prefs.Get("PLYR:FlickScale", 1f);

            JudgmentOffset = prefs.Get("PLYR:JudgmentOffset", 0f) / 1000;
            VisualOffset = prefs.Get("PLYR:VisualOffset", 0f) / 1000;
        }
    }

    public class HitObjectHistoryItem
    {
        public float                Time;
        public HitObjectHistoryType Type;
        public float                Offset;

        public HitObjectHistoryItem(HitPlayer hit, float offset)
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