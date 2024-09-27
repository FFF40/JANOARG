using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class RenderModal : Modal
{
    public static RenderModal main;
    public RenderPrefs Prefs = new();
    public bool PrefsDirty;
    [Space]
    public string OutputPath;
    public Vector2 TimeRange;

    [Space]
    public RectTransform FormHolder;
    public VerticalLayoutGroup FormHolderLayout;

    [Space]
    public RectTransform FFmpegFieldHolder;
    [Space]
    public GameObject FFmpegDisclaimer;
    public GameObject BusyDisclaimer;
    public TMP_Text BusyLabel;

    [Space]
    public bool IsAnimating;

    string FFmpegVersion;

    RenderFormat[] formats = new [] {
        new RenderFormat () {
            Extension = "mp4",
            VideoFormat = "h264",
            AudioFormat = "mp3",
        },
        new RenderFormat () {
            Extension = "mp4",
            VideoFormat = "h264",
            AudioFormat = "aac",
        },
        new RenderFormat () {
            Extension = "webm",
            VideoFormat = "vp8",
            AudioFormat = "libvorbis",
        },
    };

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    public void OnDestroy()
    {
        if (PrefsDirty)
        {
            Prefs.Save(Chartmaker.PreferencesStorage);
            Chartmaker.main.StartSavePrefsRoutine();
        }
    }

    new void Start()
    {
        base.Start();
        Prefs.Load(Chartmaker.PreferencesStorage);

        TimeRange = new (-5, Chartmaker.main.CurrentSong.Clip.length + 5);
        if (!String.IsNullOrWhiteSpace(Prefs.FFmpegPath)) CheckFFmpeg();

        InitForm();
    }
    
    public void InitForm()
    {
        var ffmpeg = Formmaker.main.Spawn<FormEntryFile, string>(
            FFmpegFieldHolder,
            "FFmpeg Path", () => Prefs.FFmpegPath, x => {
                Prefs.FFmpegPath = x;
                PrefsDirty = true;
                CheckFFmpeg();
            }
        );
        ffmpeg.AcceptedTypes = new List<FileModalFileType> {
            new("FFmpeg executable", "exe"),
            new("All files"),
        };
        SpawnForm<FormEntryString, string>("Output", () => OutputPath, x => {
            OutputPath = x; 
        });
        var formatField = SpawnForm<FormEntryDropdown, object>("Format", () => Prefs.OutputType, x => {
            Prefs.OutputType = (int)x; 
        });
        for (int a = 0; a < formats.Length; a++) formatField.ValidValues.Add(a, formats[a].ToString());

        SpawnForm<FormEntryHeader>("Time");
        var timeField = SpawnForm<FormEntryVector2, Vector2>("Range (sec)", () => TimeRange, x => {
            TimeRange = new(x.x, Mathf.Max(x.x, x.y)); 
        });
        
        var timeActions = SpawnForm<FormEntryButton>("Set Full Song");
        // --
        timeActions.TitleLabel.text = "CT→Start";
        var startBtn = Instantiate(timeActions.Button, timeActions.transform);
        startBtn.GetComponent<LayoutElement>().preferredWidth = 15;
        startBtn.onClick.AddListener(() => {
            timeField.FieldX.text = InformationBar.main.sec.ToString();    
            timeField.FieldY.text = Mathf.Max(InformationBar.main.sec, TimeRange.y).ToString();    
        });
        // --
        timeActions.TitleLabel.text = "CT→End";
        var endBtn = Instantiate(timeActions.Button, timeActions.transform);
        endBtn.GetComponent<LayoutElement>().preferredWidth = 15;
        endBtn.onClick.AddListener(() => {   
            timeField.FieldY.text = InformationBar.main.sec.ToString();   
            timeField.FieldX.text = Mathf.Min(InformationBar.main.sec, TimeRange.x).ToString();     
        });
        // --
        timeActions.TitleLabel.text = "Set Full Song";
        timeActions.Button.GetComponent<LayoutElement>().flexibleWidth = 2;
        timeActions.Button.onClick.AddListener(() => {
            timeField.FieldX.text = (-5).ToString();    
            timeField.FieldY.text = (Chartmaker.main.CurrentSong.Clip.length + 5).ToString();    
        });

        SpawnForm<FormEntryHeader>("Quality");
        var resField = SpawnForm<FormEntryVector2, Vector2>("Resolution (px)", () => Prefs.Resolution, x => {
            Prefs.Resolution = new((int)x.x, (int)x.y); PrefsDirty = true;
        });
        var resActions = SpawnForm<FormEntryButton>("Resolution Presets");
        // --
        resActions.TitleLabel.text = "Asp. Ratio Presets";
        var ratioBtn = Instantiate(resActions.Button, resActions.transform);
        ratioBtn.onClick.AddListener(() => {
            void setRatio(float ratio) 
            {
                resField.FieldX.text = (Prefs.Resolution.y * ratio).ToString("0");
            }
            ContextMenuListAction getItem(string name, float ratio) 
                => new (name + " (" + ratio.ToString("0.####") + ")", () => setRatio(ratio), _checked: Math.Abs(ratio - Prefs.Resolution.x / (float)Prefs.Resolution.y) < 0.001f);

            ContextMenuHolder.main.OpenRoot(new ContextMenuList(
                new ContextMenuListAction("Standard", () => {}, _enabled: false),
                getItem(   "5:4", 5 / 4f),
                getItem(   "4:3", 4 / 3f),
                new ContextMenuListSeparator(),
                new ContextMenuListAction("Wide", () => {}, _enabled: false),
                getItem(   "16:10", 16 / 10f),
                getItem(   "16:9", 16 / 9f),
                new ContextMenuListSeparator(),
                new ContextMenuListAction("Ultra-wide", () => {}, _enabled: false),
                getItem(   "256:135", 256 / 135f),
                getItem(   "21:9", 21 / 9f),
                getItem(   "64:27", 64 / 27f),
                getItem(   "12:5", 12 / 5f),
                getItem(   "32:9", 32 / 9f)
            ), (RectTransform)ratioBtn.transform); 
        });
        // --
        resActions.TitleLabel.text = "Resolution Presets";
        resActions.Button.onClick.AddListener(() => {
            void setRes(float res) 
            {
                float ratio = Prefs.Resolution.x / (float)Prefs.Resolution.y;
                resField.FieldX.text = (res * ratio).ToString("0");
                resField.FieldY.text = (res).ToString("0");
            }
            ContextMenuListAction getItem(string name, float res) 
                => new (name, () => setRes(res), _checked: Prefs.Resolution.y == res);

            ContextMenuHolder.main.OpenRoot(new ContextMenuList(
                getItem(   "240p",          240),
                getItem(   "480p (SD)",     480),
                getItem(   "720p (HD)",     720),
                getItem(  "1080p (FHD)",    1080),
                getItem(  "1440p (QHD)",    1440),
                getItem(  "2160p (4K UHD)", 2160),
                getItem(  "2880p (5K)",     2880),
                getItem(  "4320p (8K UHD)", 4320)
            ), (RectTransform)resActions.Button.transform);     
        });

        var fpsField = SpawnForm<FormEntryFloat, float>("Frame Rate (fps)", () => Prefs.FrameRate, x => {
            Prefs.FrameRate = x; PrefsDirty = true;
        });
        var fpsPresets = SpawnForm<FormEntryButton>("Frame Rate Presets");
        fpsPresets.Button.onClick.AddListener(() => {
            void setFPS(float fps) 
            {
                fpsField.Field.text = fps.ToString();
            }
            ContextMenuListAction getItem(string name, float fps) 
                => new (name, () => setFPS(fps), _checked: Prefs.FrameRate == fps);

            ContextMenuHolder.main.OpenRoot(new ContextMenuList(
                getItem(   "16fps", 16),
                getItem(   "20fps", 20),
                getItem(   "24fps (Film)", 24),
                getItem(   "25fps (PAL)", 25),
                getItem("29.97fps (NTSC)", 29.97f),
                getItem(   "30fps (Standard SD)", 30),
                getItem(   "48fps (Film HD)", 48),
                getItem(   "50fps (PAL HD)", 50),
                getItem("59.94fps (NTSC HD)", 59.94f),
                getItem(   "60fps (Standard HD)", 60),
                getItem(   "72fps", 72),
                getItem(  "100fps", 100),
                getItem(  "120fps", 120),
                getItem(  "144fps", 144),
                getItem(  "240fps", 240),
                getItem(  "288fps", 288),
                getItem(  "300fps", 300)
            ), (RectTransform)fpsPresets.Button.transform);
        });

        SpawnForm<FormEntryHeader>("Other");
        SpawnForm<FormEntryBool, bool>("Open on Complete", () => Prefs.OpenOnComplete, x => {
            Prefs.OpenOnComplete = x; PrefsDirty = true;
        });
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(FormHolder);
    }

    public void DownloadFFmpeg() 
    {
        Application.OpenURL("https://www.gyan.dev/ffmpeg/builds/");
    }

    public void CheckFFmpeg()
    {
        if (!IsAnimating) StartCoroutine(CheckFFmpegRoutine());
    }

    public IEnumerator CheckFFmpegRoutine()
    {
        IsAnimating = true;
        string output = "";
        FFmpegDisclaimer.SetActive(false);
        BusyDisclaimer.SetActive(true);
        BusyLabel.text = "Checking FFmpeg...";
        Task task = Task.Run(async () => {
            output = await ffmpeg("-version");
            Debug.Log(output);
            Match m = Regex.Match(output, @"^ffmpeg version ([^\s]+)");
            if (!m.Success) throw new Exception("Executable doesn't seem to be FFmpeg");
            FFmpegVersion = m.Groups[1].Value;
        });
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.Exception != null) 
        {
            BusyLabel.text = "There was an error checking FFmpeg:\n" + task.Exception.Message;
            yield break;
        }
        BusyDisclaimer.SetActive(false);
        IsAnimating = false;
    }

    public void Render() 
    {
        transform.Translate(2 * Screen.height * Vector2.down);
        StartCoroutine(RenderRoutine());
    }

    public IEnumerator RenderRoutine() 
    {
        IsAnimating = true;

        Chartmaker.main.Loader.SetActive(true);
        Chartmaker.main.LoaderPanel.ActionLabel.text = "Rendering...";
        Chartmaker.main.LoaderPanel.ProgressBar.value = 0;

        Chartmaker.main.LoaderPanel.ProgressLabel.text = "Initializing...";
        yield return new WaitForSeconds(.5f);

        // Initiate camera
        RenderTexture rtex = new (Prefs.Resolution.x, Prefs.Resolution.y, 24);
        Camera camera = Camera.main;
        camera.targetTexture = rtex;
        rtex.Create();

        Texture2D tex = new (Prefs.Resolution.x, Prefs.Resolution.y);
        
        // Initiate temp folder
        long sessionID = Random.Range(0, 2147483647) << 32 | Random.Range(0, 2147483647);
        string sessionDir, framesDir, fragmentsDir;
        sessionDir = Path.Combine(Path.GetTempPath(), $"JANOARG Chartmaker/Render_{sessionID:X16}");
        Directory.CreateDirectory(framesDir = Path.Combine(sessionDir, $"Frames"));
        Directory.CreateDirectory(fragmentsDir = Path.Combine(sessionDir, $"Fragments"));

        // Render frames
        float time = TimeRange.x;
        int frames = 1, frags = 1;
        float delta = 1 / Prefs.FrameRate;
        float camHeight = Math.Min(1, 3 / 2f * tex.width / tex.height) * 0.9f;
        float fov = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camHeight) * 2 * Mathf.Rad2Deg;
        int queuedFrames = 1, busyFrags = 0;

        int totalFrames = (int)((TimeRange.y - TimeRange.x) * Prefs.FrameRate);

        Chartmaker.main.LoaderPanel.ProgressLabel.text = $"Rendering frames... ({frames}/{totalFrames})";

        IEnumerator makeFragment(int start, int end)
        {
            int frag = frags;
            frags++;
            busyFrags++;
            string args = 
                $"-start_number {start} -r {Prefs.FrameRate} "
                + "-i \"" + Path.Combine(framesDir, $"%d.png") + "\" "
                + $"-vframes {end - start + 1} -r {Prefs.FrameRate} " 
                + "\"" + Path.Combine(fragmentsDir, $"{frag}.mp4") + $"\" ";
            // Debug.Log(args);
            Task<string> task = ffmpeg(args);
            yield return new WaitUntil(() => task.IsCompleted);
            for (int a = start; a <= end; a++) File.Delete(Path.Combine(framesDir, $"{a}.png"));
            busyFrags--;
        }

        while (time < TimeRange.y)
        {
            Chartmaker.main.SongSource.time = time;
            InformationBar.main.Update();
            PlayerView.main.UpdateObjects();
            RenderTexture.active = rtex;
            camera.rect = new Rect(0, 0, tex.width, tex.height); 
            camera.fieldOfView = fov;
            camera.Render();
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();
            var task = File.WriteAllBytesAsync(Path.Combine(framesDir, $"{frames}.png"), tex.EncodeToPNG());

            yield return new WaitUntil(() => task.IsCompleted);
            time += delta;
            frames++;
            if (frames % 100 == 1 && frames != 1) 
            {
                if (busyFrags >= 3) yield return new WaitUntil(() => busyFrags < 3);
                StartCoroutine(makeFragment(frames - 100, frames - 1));
                queuedFrames = frames;
            }
            
            Chartmaker.main.LoaderPanel.ProgressLabel.text = $"Rendering frames... ({frames}/{totalFrames})";
            Chartmaker.main.LoaderPanel.ProgressBar.value = (float)frames / totalFrames;
        }
        StartCoroutine(makeFragment(queuedFrames, frames - 1));

        Chartmaker.main.LoaderPanel.ProgressLabel.text = $"Outputting video...";
        yield return new WaitUntil(() => busyFrags <= 0);
        
        RenderFormat format = formats[Prefs.OutputType];
        string folder = Path.Combine(Application.dataPath, "../Renders");
        Directory.CreateDirectory(folder);
        string outputPath = Path.Combine(
            folder, 
            (string.IsNullOrWhiteSpace(OutputPath) ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() : OutputPath) + "." + format.Extension
        );

        using(var stream = File.OpenWrite(Path.Combine(sessionDir, $"demux.txt")))
        using(var writer = new StreamWriter(stream))
        {
            for (int a = 1; a < frags; a++) writer.WriteLine("file '" + Path.Combine(fragmentsDir, $"{a}.mp4") + "'");
        }
        {
            string args = 
                $" -f concat -safe 0 " 
                + "-i \"" + Path.Combine(sessionDir, $"demux.txt") + "\" "
                + $"-ss {TimeRange.x} -t {TimeRange.y - TimeRange.x} " 
                + "-i \"" + Path.Combine(Path.GetDirectoryName(Chartmaker.main.CurrentSongPath), Chartmaker.main.CurrentSong.ClipPath) + "\" "
                + $"-r {Prefs.FrameRate} -vcodec {format.VideoFormat} -acodec {format.AudioFormat} "
                + "\"" + outputPath + $"\" ";
            // Debug.Log(args);
            Task<string> task = ffmpeg(args);
            yield return new WaitUntil(() => task.IsCompleted);
            Debug.Log(task.Result);
        }
        Directory.Delete(framesDir, true);
        Directory.Delete(fragmentsDir, true);


        // Clean up
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rtex);
        Close();
        Chartmaker.main.Loader.SetActive(false);
        if (Prefs.OpenOnComplete) Application.OpenURL("file://" + outputPath);
        IsAnimating = false;
    }
    async Task<string> cmd(string file, string args) 
    {
        ProcessStartInfo startInfo = new(file)
        {
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false
        };

        Process process = new()
        {
            StartInfo = startInfo
        };
        process.Start();

        string output = "";

        await Task.WhenAll(
            Task.Run(() => {
                string line = "";
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    // Debug.Log(line);
                    output += line;
                }     
            }),
            Task.Run(() => {
                string line = "";
                while ((line = process.StandardError.ReadLine()) != null)
                {
                    // Debug.Log(line);
                    output += line;
                }     
            })
        );
        
        return output;
    }

    async Task<string> ffmpeg(string args) 
    {
        return await cmd(Prefs.FFmpegPath, args);
    }

    T SpawnForm<T>(string title = "") where T : FormEntry
        => Formmaker.main.Spawn<T>(FormHolder, title);

    T SpawnForm<T, U>(string title, Func<U> get, Action<U> set) where T : FormEntry<U>
        => Formmaker.main.Spawn<T, U>(FormHolder, title, get, set);

}

public class RenderPrefs 
{
    public string FFmpegPath;
    public int OutputType;
    public Vector2Int Resolution = new(1024, 800);
    public float FrameRate = 30;
    public bool OpenOnComplete = true;

    public void Load(Storage storage)
    {
        FFmpegPath = storage.Get("RD:FFmpegPath", FFmpegPath);
        OutputType = storage.Get("RD:OutputType", OutputType);
        Resolution.x = storage.Get("RD:Resolution.X", Resolution.x);
        Resolution.y = storage.Get("RD:Resolution.Y", Resolution.y);
        FrameRate = storage.Get("RD:FrameRate", FrameRate);
        OpenOnComplete = storage.Get("RD:OpenOnComplete", OpenOnComplete);
    }

    public void Save(Storage storage)
    {
        storage.Set("RD:FFmpegPath", FFmpegPath);
        storage.Set("RD:OutputType", OutputType);
        storage.Set("RD:Resolution.X", Resolution.x);
        storage.Set("RD:Resolution.Y", Resolution.y);
        storage.Set("RD:FrameRate", FrameRate);
        storage.Set("RD:OpenOnComplete", OpenOnComplete);
    }
}

public class RenderFormat {
    public string Extension;
    public string AudioFormat;
    public string VideoFormat;

    public override string ToString() 
    {
        return $".{Extension} (audio {AudioFormat}, video {VideoFormat})";
    }
}