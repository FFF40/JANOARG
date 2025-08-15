using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System;
using System.Linq;
using UnityEngine.Serialization;

public class PlayerScreen : MonoBehaviour
{
    public static PlayerScreen main;

    public static string TargetSongPath;
    public static PlayableSong TargetSong;
    public static ExternalChartMeta TargetChartMeta;
    public static ExternalChart TargetChart;

    public static Chart CurrentChart;

    [Space]
    public AudioSource Music;
    public AudioSource Hitsounds;
    public Camera Pseudocamera;
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
    public List<Graphic> ScoreDigits;
    [Space]
    public CanvasGroup JudgmentGroup;
    public TMP_Text JudgmentLabel;
    public CanvasGroup ComboGroup;
    public TMP_Text ComboLabel;
    public RectTransform JudgeScreenHolder;
    [Space]
    public TMP_Text PauseLabel;
    [Space]
    [Header("Samples")]
    public LaneGroupPlayer LaneGroupSample;
    public LanePlayer LaneSample;
    public HitPlayer HitSample;
    public MeshRenderer HoldSample;
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
    public float BadWindow = 0.2f;
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
    [FormerlySerializedAs("HasPlayedBefore")] public bool AlreadyInitialised;
    public float CurrentTime = -5;
    [Space]
    public float TotalExScore = 0;
    public float CurrentExScore = 0;
    [Space]
    public int PerfectCount = 0;
    public int GoodCount = 0;
    public int BadCount = 0;
    [Space]
    public int Combo = 0;
    public int MaxCombo = 0;
    public int TotalCombo = 0;
    [Space]
    public List<HitObjectHistoryItem> HitObjectHistory;
    [Space]
    public int HitsRemaining = 0;
    
    [HideInInspector]
    public List<LaneStyleManager> LaneStyles = new ();
    [HideInInspector]
    public List<HitStyleManager> HitStyles = new ();
    [HideInInspector]
    public List<LaneGroupPlayer> LaneGroups = new ();
    [HideInInspector]
    public List<LanePlayer> Lanes = new ();

    double lastDSPTime;

    [NonSerialized]
    public PlayerSettings Settings = new();


    List<Lane> _laneQueues;
    List<LaneGroup> _laneGroupQueues;

    [NonSerialized]
    public float ScaledExtraRadius;
    [NonSerialized]
    public float ScaledMinimumRadius;

    public void Awake()
    {
        main = this;
        CommonScene.Load();
    }

    public void Start()
    {
        InitFlickMeshes();
        SetInterfaceColor(Color.clear);
        SongProgress.value = 0;
        StartCoroutine(LoadChart());
    }

    private int _totalObjects;
    private int _loadedObjects;

    private void Update_LoadingBarHolder(int increments, string currentProgress)
    {
        _loadedObjects += increments;
        
        SongSelectReadyScreen.main.OverallProgress.text = $"{_loadedObjects}/{_totalObjects}";
        SongSelectReadyScreen.main.CurrentProgress.text = currentProgress;
        
        SongSelectReadyScreen.main.LoadProgress.value = (float)_loadedObjects / _totalObjects;
    }

    private IEnumerator LoadChart()
    {
        SongNameLabel.text       = TargetSong.SongName;
        SongArtistLabel.text     = TargetSong.SongArtist;
        
        DifficultyNameLabel.text = TargetChartMeta.DifficultyName;
        DifficultyLabel.text     = TargetChartMeta.DifficultyLevel;

        SongSelectReadyScreen.main.LoadingBarHolder.gameObject.SetActive(true);
        SongSelectReadyScreen.main.OverallProgress.text = "0/0";
        SongSelectReadyScreen.main.CurrentProgress.text = "Loading audio file...";
        
        string path = Path.Combine(Path.GetDirectoryName(TargetSongPath), TargetChartMeta.Target);
        Debug.Log(path);
        ResourceRequest thing = Resources.LoadAsync<ExternalChart>(path);
        
        TargetSong.Clip.LoadAudioData();

        yield return new WaitUntil(() => thing.isDone && TargetSong.Clip.loadState != AudioDataLoadState.Loading);

        SongSelectReadyScreen.main.CurrentProgress.text += "Done.";

        TargetChart = thing.asset as ExternalChart;

        yield return InitChart();
    }

    public IEnumerator InitChart()
    {
        
        CurrentChart = TargetChart.Data.DeepClone();
        HitObjectHistory = new();
        
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

            for (int a = 0; a < LaneStyles.Count; a++) 
                LaneStyles[a].Update(CurrentChart.Palette.LaneStyles[a]);
            
            for (int a = 0; a < HitStyles.Count; a++) 
                HitStyles[a].Update(CurrentChart.Palette.HitStyles[a]);
            
            for (int a = 0; a < TargetChart.Data.Groups.Count; a++) 
                LaneGroups[a].Current = CurrentChart.Groups[a];
            
            foreach (LanePlayer lane in Lanes) 
                Destroy(lane.gameObject);
            
            Lanes.Clear();
        } 
        else 
        {
            _totalObjects += CurrentChart.Palette.LaneStyles.Count + CurrentChart.Palette.HitStyles.Count + CurrentChart.Groups.Count + CurrentChart.Lanes.Count;

            foreach (LaneStyle style in CurrentChart.Palette.LaneStyles)
            {
                LaneStyles.Add(new(style));
                Update_LoadingBarHolder(1, $"Loading lanestyle {style.Name}...({LaneStyles.Count} of {CurrentChart.Palette.LaneStyles.Count})");;
            }

            foreach (HitStyle style in CurrentChart.Palette.HitStyles)
            {
                HitStyles.Add(new(style));
                Update_LoadingBarHolder(1, $"Loading hitstyle {style.Name}...({HitStyles.Count} of {CurrentChart.Palette.HitStyles.Count})");;
            }

            int loadedLaneGroups = 0;
            bool[] isLoaded = new bool[TargetChart.Data.Groups.Count];

            while (loadedLaneGroups < TargetChart.Data.Groups.Count)
            {
                bool progressMade = false;

                for (int i = 0; i < TargetChart.Data.Groups.Count; i++)
                {
                    if (isLoaded[i])
                        continue;

                    LaneGroup laneGroup = TargetChart.Data.Groups[i];
                    
                    bool hasNoParent = string.IsNullOrEmpty(laneGroup.Group);
                    bool parentFound = LaneGroups.Exists(x => x.Current.Name == laneGroup.Group); // Stays false until the parent is instantiated

                    if (hasNoParent || parentFound)
                    {
                        Transform parentTransform = hasNoParent
                            ? Holder
                            : LaneGroups.Find(x => x.Current.Name == laneGroup.Group).transform;

                        LaneGroupPlayer instancedLaneGroup = Instantiate(LaneGroupSample, parentTransform);

                        instancedLaneGroup.Original = TargetChart.Data.Groups[i];
                        instancedLaneGroup.Current = CurrentChart.Groups[i];
                        instancedLaneGroup.gameObject.name = instancedLaneGroup.Current.Name;

                        LaneGroups.Add(instancedLaneGroup);
                        isLoaded[i] = true;
                        loadedLaneGroups++;
                        progressMade = true;
                        
                        Update_LoadingBarHolder(1, $"Loading lanegroup {laneGroup.Name}...({loadedLaneGroups} of {TargetChart.Data.Groups.Count}");
                    }
                    
                    // Intentional throttling maybe idk
                    yield return new WaitForEndOfFrame();
                }

                if (!progressMade)
                {
                    int err = 0;
                    List<string> errDetails = new();
                    for (var i = 0; i < isLoaded.Length; i++)
                        if (!isLoaded[i])
                        {
                            err++;
                            errDetails.Add($"{LaneGroups[i].Current.Name} depends on {LaneGroups[i].Current.Group}");
                        }

                    string PrintDepDetails()
                    {
                        string final = String.Empty;
                        for (int a = 0; a < errDetails.Count; a++)
                        {
                            if (a == errDetails.Count - 1)
                                final += String.Concat(errDetails[a]);
                            else
                                final += String.Concat(errDetails[a], "\n");
                        }
                        return final;
                    }
                    
                    Debug.LogError($"Failed to resolve {err} lane group dependencies. Possible circular or missing references. {(errDetails.Count > 0 ? "\n" + PrintDepDetails() : String.Empty)}");
                    
                    Update_LoadingBarHolder(Math.Abs(TargetChart.Data.Groups.Count - loadedLaneGroups), $"Failed to load {err} lanegroups!");
                    
                    break;
                }
            }


        }

        ComboGroup.alpha = 
            JudgmentGroup.alpha = 0;

        float dpi = (Screen.dpi == 0 ? 100 : Screen.dpi);
        float screenUnit = Mathf.Min(Screen.width / ScreenUnit.x, Screen.height / ScreenUnit.y);

        ScaledExtraRadius = dpi * 0.2f;
        ScaledMinimumRadius = MinimumRadius * screenUnit;

        int loadedLanes = 0;
        bool[] instantiatedLane = new bool[TargetChart.Data.Lanes.Count];

        while (loadedLanes < TargetChart.Data.Lanes.Count)
        {
            bool progressMade = false;
            
            for (int a = 0; a < TargetChart.Data.Lanes.Count; a++)
            {
                if (instantiatedLane[a])
                {
                    Debug.Log($"[LaneInit] Skipping lane {a}, already instantiated.");
                    continue;
                }

                var laneData = TargetChart.Data.Lanes[a];
                bool noParent = string.IsNullOrEmpty(laneData.Group);
                LaneGroupPlayer laneInGroup = noParent ? null : LaneGroups.Find(x => x.Current.Name == laneData.Group);

                //Debug.Log($"[LaneInit] Processing lane {a}: group = '{laneData.Group}'");
                
                // Only continue if no parent is needed, or parent exists
                if (!noParent && laneInGroup == null)
                {
                    Debug.LogWarning($"Lane [{a}] references non-existent(yet) '{laneData.Group}' in LaneGroups. Delaying.");
                    continue;
                }

                LanePlayer instancedLane = Instantiate(
                    LaneSample,
                    noParent ? Holder : laneInGroup.transform
                );

                instancedLane.Group = laneInGroup;
                instancedLane.Original = TargetChart.Data.Lanes[a];
                instancedLane.Current = CurrentChart.Lanes[a];
                
                instancedLane.Init();
                Lanes.Add(instancedLane);
                
                float time = float.NaN;
                Vector3 startPos = Vector3.zero, 
                        endPos = Vector3.zero; //Declare 2 points of lane show in screen
                foreach (HitObject laneHitobject in instancedLane.Original.Objects)
                {
                    // Add ExScore by note type
                    TotalExScore += laneHitobject.Type == HitObject.HitType.Normal ? 3 : 1;
                    TotalExScore += Mathf.Ceil(laneHitobject.HoldLength / 0.5f);
                    if (laneHitobject.Flickable)
                    {
                        TotalExScore += 1;
                        if (!float.IsNaN(laneHitobject.FlickDirection)) TotalExScore += 1;
                    }
    
                    if (!Mathf.Approximately(time, laneHitobject.Offset))
                    {
                        // Set camera distance and rotation to hit time of the note?
                        CameraController hitObjectCamera = (CameraController)TargetChart.Data.Camera.GetStoryboardableObject(laneHitobject.Offset);
                        Pseudocamera.transform.localPosition = hitObjectCamera.CameraPivot;
                        Pseudocamera.transform.localEulerAngles = hitObjectCamera.CameraRotation;
                        Pseudocamera.transform.Translate(Vector3.back * hitObjectCamera.PivotDistance);
    
                        // Set 2 points of lane?
                        Lane lane = (Lane)instancedLane.Original.GetStoryboardableObject(laneHitobject.Offset);
                        LanePosition positionStep = lane.GetLanePosition(laneHitobject.Offset, laneHitobject.Offset, TargetSong.Timing);
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
    
                    HitObject hitObject = (HitObject)laneHitobject.GetStoryboardableObject(laneHitobject.Offset); //Get current HitObject?
                    Vector2 hitStart = Pseudocamera.WorldToScreenPoint(Vector3.LerpUnclamped(startPos, endPos, hitObject.Position));
                    Vector2 hitEnd = Pseudocamera.WorldToScreenPoint(Vector3.LerpUnclamped(startPos, endPos, hitObject.Position + laneHitobject.Length));
    
                    float radius = Vector2.Distance(hitStart, hitEnd) / 2 + ScaledExtraRadius;
    
                    //Add hit coords
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
                
                Update_LoadingBarHolder(1, $"Loading lane {a}...({loadedLanes} of {TargetChart.Data.Lanes.Count})");;
                
                yield return new WaitForEndOfFrame();
            }
            
            if (!progressMade)
            {
                int err = 0;
                List<string> errDetails = new();
                for (var i = 0; i < instantiatedLane.Length; i++)
                    if (!instantiatedLane[i])
                    {
                        err++;
                        errDetails.Add($"Lane {(String.IsNullOrEmpty(Lanes[i].Current.Name) ? i : Lanes[i].Current.Name)} depends on {Lanes[i].Current.Group}");
                    }

                string PrintDepDetails()
                {
                    string final = String.Empty;
                    for (int a = 0; a < errDetails.Count; a++)
                    {
                        if (a == errDetails.Count - 1)
                            final += String.Concat(errDetails[a]);
                        else
                            final += String.Concat(errDetails[a], "\n");
                    }
                    return final;
                }
                
                Debug.LogError($"Failed to resolve {err} lane dependencies. Possible circular or missing references. {(errDetails.Count > 0 ? "\n" + PrintDepDetails() : String.Empty)}");
                
                Update_LoadingBarHolder(Math.Abs(TargetChart.Data.Lanes.Count - loadedLanes), $"Failed to load {err} lanes!");
                
                break;
            }
        }

        Music.clip = TargetSong.Clip;
        Music.volume = Settings.BGMusicVolume;
        CurrentTime = -5;
        PlayerScreenPause.main.PauseTime = -10;
        IsReady = true;
        AlreadyInitialised = true;
        lastDSPTime = AudioSettings.dspTime;
        yield return new WaitForEndOfFrame();
    }

    public void BeginReadyAnim() 
    {
        StartCoroutine(ReadyAnim());
    }

    public IEnumerator ReadyAnim() 
    {
        for (int a = 0; a < ScoreCounter.Digits.Count; a++)
        {
            ScoreCounter.Digits[a].List.Clear();
            ScoreCounter.Digits[a].Speed = 3;
            for (int b = 9 - a; b <= 10; b++) 
            {
                ScoreCounter.Digits[a].List.Add((b % 10).ToString());
            }
        }
        yield return Ease.Animate(1.5f, (x) => {
            float ease = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
            SetInterfaceColor(TargetSong.InterfaceColor * new Color(1, 1, 1, ease));
            PlayerHUD.transform.localScale = Vector3.one * (ease * .05f + .95f);
        });
        for (int a = 0; a < ScoreCounter.Digits.Count; a++)
        {
            ScoreCounter.Digits[a].Speed = 9;
        }
        IsPlaying = true;
        lastDSPTime = AudioSettings.dspTime;
    }

    public bool ResultExec = false;
    
    public void Update()
    {
        if (IsPlaying)
        {
            double delta = Math.Min(AudioSettings.dspTime - lastDSPTime, PerfectWindow);
            if (delta <= 0) delta = Time.unscaledDeltaTime;
            CurrentTime += (float)delta;
            lastDSPTime += delta;
            
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
            else 
            {
                if (Music.isPlaying) Music.Pause();
            }
            
            // Check hit objects
            CheckHitObjects();
            
            // Update song progress slider
            SongProgress.value = CurrentTime / Music.clip.length;

            
            // Prevents from going to negative values, which might break things
            float ChartUpdateTime(float time) => time < 0 ? 0 : time ;
            
            float visualTime = ChartUpdateTime(CurrentTime + Settings.VisualOffset) ;
            float visualBeat = TargetSong.Timing.ToBeat(visualTime);
            
            
            // Update palette
            CurrentChart.Palette.Advance(visualBeat);
            if (Common.main.MainCamera.backgroundColor != CurrentChart.Palette.BackgroundColor) SetBackgroundColor(CurrentChart.Palette.BackgroundColor);
            if (SongNameLabel.color != CurrentChart.Palette.InterfaceColor) SetInterfaceColor(CurrentChart.Palette.InterfaceColor);
            for (int a = 0; a < LaneStyles.Count; a++)
            {
                CurrentChart.Palette.LaneStyles[a].Advance(visualBeat);
                LaneStyles[a].Update(CurrentChart.Palette.LaneStyles[a]);
            }
            for (int a = 0; a < HitStyles.Count; a++)
            {
                CurrentChart.Palette.HitStyles[a].Advance(visualBeat);
                HitStyles[a].Update(CurrentChart.Palette.HitStyles[a]);
            }

            // Update camera
            CurrentChart.Camera.Advance(visualBeat);
            Camera camera = Common.main.MainCamera;
            camera.transform.position = CurrentChart.Camera.CameraPivot;
            camera.transform.eulerAngles = CurrentChart.Camera.CameraRotation;
            camera.transform.Translate(Vector3.back * CurrentChart.Camera.PivotDistance);

            // Update scene
            foreach (LaneGroupPlayer group in LaneGroups) group.UpdateSelf(visualTime, visualBeat);
            foreach (LanePlayer lane in Lanes)            lane.UpdateSelf(visualTime, visualBeat);
            
            if ((HitsRemaining <= 0 && PlayerInputManager.Instance.HoldQueue.Count == 0 || CurrentTime/Music.clip.length >= 1) && !ResultExec) 
            {
                PlayerScreenResult.main.StartEndingAnim();
                ResultExec = true;
            }
        }
    }

    public void Resync() 
    {
        lastDSPTime = AudioSettings.dspTime;
    }

    public void CheckHitObjects() 
    {
        foreach (LanePlayer lane in Lanes)
        {
            for (int a = 0; a < lane.HitObjects.Count; a++)
            {
                HitPlayer hit = lane.HitObjects[a];
                if (hit.HoldMesh) 
                {
                    lane.UpdateHoldMesh(hit);
                }
            }
        }
        //PlayerInputManager.main.UpdateTouches();
        PlayerInputManager.Instance.UpdateInput();
    }

    Coroutine judgAnim;

    public void AddScore(float score, float? acc)
    {
        CurrentExScore += score;
        ScoreCounter.SetNumber(Mathf.RoundToInt(CurrentExScore / TotalExScore * 1e6f));

        if (score > 0) 
        {
            Combo++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);
            if (acc == null || acc == 0) PerfectCount++;
            else GoodCount++;
        }
        else 
        {
            Combo = 0;
            BadCount++;
        }
        
        ComboLabel.text = Helper.PadScore(Combo.ToString(), 4) + "<voffset=0.065em>×";
        if (acc.HasValue)
        {
            JudgmentLabel.text = acc == 0 ? "FLAWLESS" : 
                acc < 0 ? (score > 0 ? "EARLY" : "BAD") : 
                (score > 0 ? "LATE" : "MISS");
        }
        else 
        {
            JudgmentLabel.text = score > 0 ? "FLAWLESS" : "MISS";
        }

        if (judgAnim != null) StopCoroutine(judgAnim);
        judgAnim = StartCoroutine(JudgmentAnim());

        TotalCombo++;
    }

    IEnumerator JudgmentAnim() 
    {
        yield return Ease.Animate(.6f, (x) => {
            float val = Mathf.Pow(1 - x, 5);
            float val2 = 1 - Ease.Get(x, EaseFunction.Quintic, EaseMode.In);

            ComboGroup.alpha = Combo == 0 ? 0 : val + 1;
            ComboLabel.rectTransform.anchoredPosition *= new Vector2Frag(y: -25 + 2 * val * val);
            JudgmentLabel.rectTransform.anchoredPosition *= new Vector2Frag(y: -25 + 2 * val * val);
            JudgmentGroup.alpha = val2;
        });
    }



    public void RemoveHitPlayer(HitPlayer hit) 
    {
        if (hit.HoldMesh != null)
        {
            Destroy(hit.HoldMesh.mesh);
            Destroy(hit.HoldMesh.gameObject);
        }
        if (hit.gameObject != null)
            Destroy(hit.gameObject);
        
        hit.Lane.HitObjects.Remove(hit);

        HitsRemaining--;
    }

    public void Hit(HitPlayer hit, float offset, bool spawnEffect = true)
    {
        // Debug.Log((int)hit.Current.Type + ":" + hit.Current.Type + " " + offset);

        int score = hit.Current.Type == HitObject.HitType.Normal ? 3 : 1;
        if (hit.Current.Flickable)
        {
            score += 1;
            if (!float.IsNaN(hit.Current.FlickDirection)) // Directional
                score += 1;
        }
        
        float offsetAbs = Mathf.Abs(offset);
        float? acc = null;
        if (hit.Current.Flickable || hit.Current.Type == HitObject.HitType.Catch)
        {
            if (offsetAbs <= PassWindow) 
            {
                AddScore(score, null);
                HitObjectHistory.Add(new (hit, 0));
            }
            else 
            {
                AddScore(0, null);
                HitObjectHistory.Add(new (hit, float.PositiveInfinity));
            }
        }
        else 
        {
            acc = 0;
            if (offsetAbs > GoodWindow) acc = Mathf.Sign(offset);
            else if (offsetAbs > PerfectWindow) acc = Mathf.Sign(offset) * Mathf.InverseLerp(PerfectWindow, GoodWindow, offsetAbs);
            AddScore(score * (1 - Mathf.Abs((float)acc)), acc);
            HitObjectHistory.Add(new (hit, offset));
        }

        if (spawnEffect)
        {
            var effect = Instantiate(JudgeScreenSample, JudgeScreenHolder);
            effect.SetAccuracy(acc);
            effect.SetColor(CurrentChart.Palette.InterfaceColor);
            var rt = (RectTransform)effect.transform;
            rt.position = hit.HitCoord.Position;

            float hsVol = Settings.HitsoundVolume[0];
            if (Settings.HitsoundVolume.Length >= 3 && acc != null) 
            {
                if (Mathf.Abs((float)acc) >= 1) hsVol = Settings.HitsoundVolume[2];
                else if (Mathf.Abs((float)acc) > 0) hsVol = Settings.HitsoundVolume[1];
            }
            
            if (hit.Current.Type is HitObject.HitType.Normal) 
                Hitsounds.PlayOneShot(NormalHitsound, hsVol);
            else 
                Hitsounds.PlayOneShot(CatchHitsound, hsVol);
            
            if (hit.Current.Flickable) 
                Hitsounds.PlayOneShot(FlickHitsound, hsVol);
        }

        if (hit.Time == hit.EndTime)
        {
            RemoveHitPlayer(hit);
        }
        else 
        {
            hit.IsProcessed = true;
        }
    }
    public void SetBackgroundColor(Color color) 
    {
        Common.main.MainCamera.backgroundColor = color;
        RenderSettings.fogColor = color;
    }

    public void SetInterfaceColor(Color color) 
    {
        SongNameLabel.color = SongArtistLabel.color = DifficultyLabel.color = DifficultyNameLabel.color = 
            SongProgressTip.color = SongProgressGlow.color = JudgmentLabel.color = ComboLabel.color = 
            PauseLabel.color = color;
        foreach (Graphic g in ScoreDigits) g.color = color;
        SongProgressBody.color = color * new Color (1, 1, 1, .5f);
    }

    private void InitFlickMeshes()
    {
        return; //Asset already assigned
        if (!FreeFlickIndicator) 
        {
            Mesh mesh = new();
            List<Vector3> verts = new();
            List<int> tris = new();

            verts.AddRange(new Vector3[] { new(-1, 0), new(0, 2), new(0, -.5f), new(1, 0), new(0, -2), new(0, .5f) });
            tris.AddRange(new [] {0, 1, 2, 3, 4, 5});

            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] = verts[i] * Settings.FlickScale;
            }

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
            tris.AddRange(new [] {0, 1, 2, 2, 3, 0, 3, 4, 0, 4, 5, 0});

            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] *= Settings.FlickScale;
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            ArrowFlickIndicator = mesh;
        }
        
        AssetDatabase.CreateAsset(FreeFlickIndicator, "Assets/JANOARG/Resources/Meshes/FreeFlickIndicator.asset");
        AssetDatabase.CreateAsset(ArrowFlickIndicator, "Assets/JANOARG/Resources/Meshes/ArrowFlickIndicator.asset");
    }
}

public class PlayerSettings 
{
    public float BGMusicVolume;
    public float[] HitsoundVolume;

    public float[] HitObjectScale;
    public float FlickScale;

    public float JudgmentOffset;
    public float VisualOffset;

    public PlayerSettings ()
    {
        Storage prefs = Common.main != null ? Common.main.Preferences : null;
        if (prefs == null) return;
        BGMusicVolume = prefs.Get("PLYR:BGMusicVolume", 100f) / 100;
        HitsoundVolume = prefs.Get("PLYR:HitsoundVolume", new [] { 60f });
        for (int a = 0; a < HitsoundVolume.Length; a++) HitsoundVolume[a] /= 100;
        
        HitObjectScale = prefs.Get("PLYR:HitScale", new [] { 1f });
        if (HitObjectScale.Length < 2) HitObjectScale = new [] { HitObjectScale[0], HitObjectScale[0] };

        FlickScale = prefs.Get("PLYR:FlickScale", 1f);

        JudgmentOffset = prefs.Get("PLYR:JudgmentOffset", 0f) / 1000;
        VisualOffset = prefs.Get("PLYR:VisualOffset", 0f) / 1000;
    }
}

public class HitObjectHistoryItem
{
    public float Time;
    public HitObjectHistoryType Type;
    public float Offset;

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