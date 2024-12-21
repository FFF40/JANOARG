using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class NewSongModal : Modal
{
    public static NewSongModal main;

    [Space]
    public LayoutElement FormLayout;
    public RectTransform FormHolder;
    public VerticalLayoutGroup FormHolderLayout;
    [Space]
    public AudioSource PreviewSource;
    public TMP_Text PreviewMessage;
    public GameObject PreviewPlayer;
    public Slider PreviewSlider;
    public TMP_Text PreviewSongPositionLabel;
    public TMP_Text PreviewSongLengthLabel;
    public GameObject PreviewPlayIcon;
    public GameObject PreviewPauseIcon;
    [Space]
    public TMP_Text SongFileSizeLabel;
    public TooltipTarget SongFileSizeTooltip;
    public TMP_Text SongInfoLabel;

    [Header("Data")]
    public string Codename;
    public string AudioPath;

    public MetadataReader SongMetadata;
    public PlayableSong InitialValues;

    Action<MetadataReader> OnMetadataRead;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    new void Start()
    {
        base.Start();
        InitForm();
        SetPlayerActive(false);
    }

    public void OnDestroy() 
    {
        Destroy(PreviewSource.clip);
    }

    public void Update() 
    {
        if (PreviewPlayer.activeSelf) 
        {
            static string formatTime(float seconds) => 
                        Mathf.Floor(seconds / 60).ToString("00") 
                + ":" + Mathf.Floor(seconds % 60).ToString("00") 
                + "s" + Mathf.Floor(seconds * 1000 % 1000).ToString("000");
            PreviewSlider.SetValueWithoutNotify(PreviewSource.time / PreviewSource.clip.length);
            PreviewSongLengthLabel.text = formatTime(PreviewSource.clip.length);
            PreviewSongPositionLabel.text = formatTime(PreviewSource.time);
            PreviewPlayIcon.SetActive(!PreviewSource.isPlaying);
            PreviewPauseIcon.SetActive(PreviewSource.isPlaying);
        }
    }

    public void InitForm()
    {
        FormEntryString title = null, artist = null, genre = null;
        FormEntryFloat bpm = null;

        var codename = SpawnForm<FormEntryString, string>("Codename", () => Codename, x => Codename = x);
        codename.Field.contentType = TMP_InputField.ContentType.Alphanumeric;
        var audio = SpawnForm<FormEntryFile, string>("Audio File", () => AudioPath, x => {
            StartCoroutine(LoadAudio(x));
            AudioPath = x;
        });
        audio.AcceptedTypes = new List<FileModalFileType> {
            new("Supported audio files", "mp3", "wav", "ogg"),
            new("All files"),
        };

        SpawnForm<FormEntryHeader>("Metadata");
        
        title = SpawnForm<FormEntryString, string>("Song Name", () => InitialValues.SongName, x => InitialValues.SongName = x);
        SpawnForm<FormEntryString, string>("Alt Song Name", () => InitialValues.AltSongName, x => InitialValues.AltSongName = x);
        artist = SpawnForm<FormEntryString, string>("Song Artist", () => InitialValues.SongArtist, x => InitialValues.SongArtist = x);
        SpawnForm<FormEntryString, string>("Alt Song Artist", () => InitialValues.AltSongArtist, x => InitialValues.AltSongArtist = x);
        genre = SpawnForm<FormEntryString, string>("Genre", () => InitialValues.Genre, x => InitialValues.Genre = x);
        SpawnForm<FormEntryString, string>("Location", () => InitialValues.Location, x => InitialValues.Location = x);
        
        SpawnForm<FormEntryHeader>("Colors");
        SpawnForm<FormEntryColor, Color>("Background", () => InitialValues.BackgroundColor, x => InitialValues.BackgroundColor = x);
        SpawnForm<FormEntryColor, Color>("Interface", () => InitialValues.InterfaceColor, x => InitialValues.InterfaceColor = x);
        
        SpawnForm<FormEntryHeader>("Timing");
        bpm = SpawnForm<FormEntryFloat, float>("Base BPM", () => InitialValues.Timing.Stops[0].BPM, x => InitialValues.Timing.Stops[0].BPM = x);
        SpawnForm<FormEntryFloat, float>("Base Offset", () => InitialValues.Timing.Stops[0].Offset, x => InitialValues.Timing.Stops[0].Offset = x);
        SpawnForm<FormEntryInt, int>("Base Signature", () => InitialValues.Timing.Stops[0].Signature, x => InitialValues.Timing.Stops[0].Signature = x);

        LayoutRebuilder.ForceRebuildLayoutImmediate(FormHolder);
        FormLayout.preferredHeight = FormHolderLayout.preferredHeight;

        OnMetadataRead = (SongMetadata) => {
            InitialValues.SongName = string.IsNullOrEmpty(SongMetadata.Title) ? "" : SongMetadata.Title;
            title.Start();
            InitialValues.SongArtist = string.IsNullOrEmpty(SongMetadata.Artist) ? "" : SongMetadata.Artist;
            artist.Start();
            InitialValues.Genre = string.IsNullOrEmpty(SongMetadata.Type) ? "Genreless" : SongMetadata.Type;
            genre.Start();
            InitialValues.Timing.Stops[0].BPM = SongMetadata.BeatsPerMinute <= 0 ? 140 : SongMetadata.BeatsPerMinute;
            bpm.Start();
        };
    }

    public string Execute()
    {
        if (string.IsNullOrWhiteSpace(Codename))
        {
            throw new Exception("Please specify a codename for the Song.");
        }
        if (string.IsNullOrWhiteSpace(AudioPath))
        {
            throw new Exception("Please specify an audio file.");
        }

        string path = Path.GetDirectoryName(Application.dataPath) + "/Songs";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        path += "/" + Codename;
        if (Directory.Exists(path)) 
        {
            if (File.Exists(path + "/" + Codename + ".japs")) 
            {
                throw new Exception("There is already a Playable Song with the same codename as you choose for this Song. Please change the codename to a different value.");
            }
        } 
        else 
        {
            Directory.CreateDirectory(path);
        }
        string audioPath = Path.ChangeExtension(path + "/" + Codename, Path.GetExtension(AudioPath));
        File.Copy(AudioPath, audioPath);
        InitialValues.ClipPath = Path.GetRelativePath(path, audioPath);
        InitialValues.Cover.BackgroundColor = InitialValues.BackgroundColor;
        File.WriteAllText(path + "/" + Codename + ".japs", JAPSEncoder.Encode(InitialValues, InitialValues.ClipPath));
        return path;
    }

    public Task<string> ExecuteAsync()
    {
        return Task.Run(() => Execute());
    }
    
    public IEnumerator ExecuteRoutine() {
        Chartmaker.main.Loader.SetActive(true);
        Chartmaker.main.LoaderPanel.SetSong(InitialValues);
        Chartmaker.main.LoaderPanel.ActionLabel.text = "Creating Playable Song...";
        Chartmaker.main.LoaderPanel.ProgressBar.value = 0;

        Chartmaker.main.LoaderPanel.ProgressLabel.text = "Initializing...";
        yield return new WaitForSeconds(0.5f);

        Chartmaker.main.LoaderPanel.ProgressLabel.text = "Creating .japs file...";

        Task<string> task = ExecuteAsync(); 
        yield return new WaitUntil(() => task.IsCompleted);
        if (!task.IsCompletedSuccessfully) 
        {
            Chartmaker.main.Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", task.Exception.Message, new string[] {"Ok"}, _ => {});
            Debug.LogException(task.Exception);
            yield break;
        }
        
        Chartmaker.main.StartCoroutine(Chartmaker.main.OpenSongRoutine(task.Result + "/" + Codename + ".japs"));
        Close();
    }
    
    public void StartExecuteRoutine() {
        StartCoroutine(ExecuteRoutine());
    }

    T SpawnForm<T>(string title = "") where T : FormEntry
        => Formmaker.main.Spawn<T>(FormHolder, title);

    T SpawnForm<T, U>(string title, Func<U> get, Action<U> set) where T : FormEntry<U>
        => Formmaker.main.Spawn<T, U>(FormHolder, title, get, set);

    public void SetPlayerActive(bool value) 
    {
        PreviewPlayer.SetActive(value);
        PreviewMessage.gameObject.SetActive(!value);
    }

    public IEnumerator LoadAudio (string path) 
    {
        Destroy(PreviewSource.clip);
        SetPlayerActive(false);
        PreviewMessage.text = "Loading audio...";

        UnityWebRequest stream = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.UNKNOWN);
        Debug.Log(stream.url);
        stream.SendWebRequest();
        while (!stream.isDone) 
        {
            yield return null;
        }

        if (stream.result != UnityWebRequest.Result.Success)
        {
            PreviewMessage.text = "Error: " + stream.error;
            yield break;
        }
        else
        {
            try
            {
                PreviewSource.clip = DownloadHandlerAudioClip.GetContent(stream);
                if (!PreviewSource.clip) throw new Exception(stream.error);
                SetPlayerActive(true);
            }
            catch (Exception e)
            {
                PreviewMessage.text = "Error: " + e.Message;
                yield break;
            }
        }

        if (PreviewSource.clip)
        {
            AudioClip clip = PreviewSource.clip;

            FileInfo fileInfo = new (path);
            long fileSize = fileInfo.Length;

            SongFileSizeLabel.text = Format.FileSize(fileSize);
            SongFileSizeTooltip.Text = fileSize.ToString("#,##0") + " bytes";
            SongInfoLabel.text = 
                (clip.channels == 1 ? "Mono" : clip.channels == 2 ? "Stereo" : clip.channels + " channels")
                + " / " + PreviewSource.clip.frequency + "Hz";

            OnMetadataRead(new (path));
        }
    }

    public void TogglePlay()
    {
        if (PreviewSource.isPlaying) PreviewSource.Pause();
        else PreviewSource.Play();
    }

    public void SetPlayerTime(float time) 
    {
        PreviewSource.time = time * PreviewSource.clip.length;
    }
}
