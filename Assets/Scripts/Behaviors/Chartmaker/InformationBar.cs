using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InformationBar : MonoBehaviour
{
    public static InformationBar main;

    [Header("Objects")]
    public Button SongButton;
    public TMP_Text SongNameLabel;
    public Button ChartButton;
    public TMP_Text ChartNameLabel;
    public GameObject PlayIcon;
    public GameObject PauseIcon;
    public TMP_Text BeatTimeLabel;
    public TMP_Text SecondTimeLabel;
    [Space]
    public RectTransform ChartButtonTransform;
    public RectTransform ChartDropdownButton;
    [Space]
    public AudioSource SoundPlayer;
    public AudioClip MetronomeSoundMain;
    public AudioClip MetronomeSoundSub;
    [Space]
    public RectTransform Visualizer;
    public Image VisualizerBarSample;
    public List<Image> VisualizerBars;
    public VisualizerMode VisualizerMode;
    [Space]
    public PlayOptionsPanel PlayOptions;

    public float sec { get; private set; }
    public float beat { get; private set; }
    public float barPos { get; private set; }

    public float MetronomeIndex { get; private set; }

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
        UpdateSongButton();
        UpdateChartButton();
        UpdateButtonActivity();
    }

    public void Update()
    {
        UpdatePlayButton();
        if (Chartmaker.main?.CurrentSong != null)
        {
            sec = Chartmaker.main.SongSource.timeSamples / (float)Chartmaker.main.SongSource.clip.frequency;
            beat = Chartmaker.main.CurrentSong.Timing.ToBeat(sec);
            barPos = Chartmaker.main.CurrentSong.Timing.ToDividedBeat(sec);
            SecondTimeLabel.text = Mathf.Floor(sec / 60).ToString("00") + ":" + Mathf.Floor(sec % 60).ToString("00") + "s" + Mathf.Floor(sec * 1000 % 1000).ToString("000");
            BeatTimeLabel.text = beat.ToString("0.000").Replace('.', 'b');

            if (!TimelinePanel.main.isDragged && PlayOptions.MetronomeVolume > 0 && Mathf.Floor(beat) > MetronomeIndex)
            {
                SoundPlayer.PlayOneShot(barPos < 1 ? MetronomeSoundMain : MetronomeSoundSub, PlayOptions.MetronomeVolume);
            }
            MetronomeIndex = Mathf.Floor(beat);

            UpdateVisualizer();
        }
    }

    public void UpdateButtonActivity() 
    {
        SongButton.interactable = HierarchyPanel.main.CurrentMode != HierarchyMode.PlayableSong;
        ChartButton.interactable = Chartmaker.main?.CurrentChart == null || HierarchyPanel.main.CurrentMode != HierarchyMode.Chart;
    }

    public void UpdateSongButton()
    {
        
        if (Chartmaker.main?.CurrentSong != null)
        {
            SongNameLabel.text = Chartmaker.main.CurrentSong.SongName;
        }
    }

    public void UpdateChartButton()
    {
        Chart chart = Chartmaker.main?.CurrentChart;

        if (chart != null)
        {
            ChartNameLabel.text = chart.DifficultyName + " " + chart.DifficultyLevel;
            ChartDropdownButton.sizeDelta = new Vector2(24, ChartDropdownButton.sizeDelta.y);
            ChartDropdownButton.anchoredPosition = new Vector2(333, ChartDropdownButton.anchoredPosition.y); 
            ChartButtonTransform.gameObject.SetActive(true);
        }
        else 
        {
            ChartNameLabel.text = "Select Chart...";
            ChartDropdownButton.sizeDelta = new Vector2(160, ChartDropdownButton.sizeDelta.y);
            ChartDropdownButton.anchoredPosition = new Vector2(174, ChartDropdownButton.anchoredPosition.y);
            ChartButtonTransform.gameObject.SetActive(false);
        }
    }

    public void FocusSong()
    {
        TimelinePanel.main.SetTabMode(TimelineMode.Timing);
        HierarchyPanel.main.SetMode(HierarchyMode.PlayableSong);
    }

    public void FocusChart()
    {
        if (Chartmaker.main.CurrentChart == null)
        {
            ShowChartPopup(ChartButtonTransform);
        }
        else
        {
            HierarchyPanel.main.SetMode(HierarchyMode.Chart);
            TimelinePanel.main.SetTabMode(TimelineMode.Lanes);
        }
    }

    public void ShowChartPopup()
    {
        ShowChartPopup(ChartDropdownButton);
    }

    public void ShowChartPopup(RectTransform rt)
    {
        ContextMenuList list = new ContextMenuList();
        foreach(ExternalChartMeta chart in Chartmaker.main?.CurrentSong.Charts)
        {
            ExternalChartMeta _chart = chart;
            list.Items.Add(new ContextMenuListAction(chart.DifficultyName + " " + chart.DifficultyLevel, () => {
                StartCoroutine(Chartmaker.main.OpenChartRoutine(_chart));
            }));
        }
        if (list.Items.Count > 0) list.Items.Add(new ContextMenuListSeparator());
        list.Items.Add(new ContextMenuListAction("Create Chart...", () => { ModalHolder.main.Spawn<NewChartModal>(); }));
        ContextMenuHolder.main.OpenRoot(list, rt);
    }
    
    public void UpdatePlayButton()
    {
        if (Chartmaker.main != null)
        {
            PlayIcon.SetActive(!Chartmaker.main.SongSource.isPlaying);
            PauseIcon.SetActive(Chartmaker.main.SongSource.isPlaying);
        }
    }
    
    public void ToggleSong()
    {
        if (Chartmaker.main.SongSource.isPlaying)
        {
            Chartmaker.main.SongSource.Pause();
            if (Chartmaker.Preferences.MaximizeOnPlay)
            {
                TimelinePanel.main.Restore();
                HierarchyPanel.main.Restore();
                InspectorPanel.main.Restore();
            }
        }
        else 
        {
            Chartmaker.main.SongSource.Play();
            if (Chartmaker.Preferences.MaximizeOnPlay)
            {
                TimelinePanel.main.Collapse();
                HierarchyPanel.main.Collapse();
                InspectorPanel.main.Collapse();
            }
        }
        UpdatePlayButton();
    }
    

    public void UpdateVisualizer()
    {
        int count = 0;

        Image AddBar()
        {
            Image bar = null;
            if (count < VisualizerBars.Count) bar = VisualizerBars[count];
            else VisualizerBars.Add(bar = Instantiate(VisualizerBarSample, Visualizer));
            count++;
            return bar;
        }

        Rect rect = Visualizer.rect;
        Color color = Themer.main.Keys["ControlContent"];
        AudioClip clip = Chartmaker.main.SongSource.clip;
        int sampleOffset = (int)(sec * clip.frequency);

        if (VisualizerMode == VisualizerMode.Metronome)
        {
            BPMStop stop = Chartmaker.main.CurrentSong.Timing.GetStop(sec, out _);
            float sizeX = rect.width / stop.Signature;

            Image bar = AddBar();         
            bar.rectTransform.sizeDelta = new Vector2(sizeX, rect.height);
            bar.rectTransform.anchoredPosition = new Vector2(Mathf.Floor(barPos) * sizeX, 0);
            bar.color = color * new Color(1, 1, 1, Mathf.Pow(1 - (barPos % 1), 2));

            Image bar2 = AddBar();
            bar2.rectTransform.sizeDelta = new Vector2(sizeX, rect.height * bar.color.a);
            bar2.rectTransform.anchoredPosition = new Vector2(Mathf.Floor(barPos) * sizeX, 0);
            bar2.color = color * new Color(1, 1, 1, .5f);

            Image bar3 = AddBar();
            bar3.rectTransform.sizeDelta = new Vector2(sizeX, 1);
            bar3.rectTransform.anchoredPosition = new Vector2(Mathf.Floor(barPos) * sizeX, rect.height - 1);
            bar3.color = color * new Color(1, 1, 1, Mathf.Pow(2 - (barPos % 1) * 2, 2));
        }
        else if (VisualizerMode == VisualizerMode.FrequencyBars)
        {
            const int barCount = 8;
            const int fftCount = 2 << barCount;
            const float riseTime = .05f, fallTime = .5f;

            float[] fftL = new float[fftCount], fftR = new float[fftCount];
            float[] data = new float[fftCount * clip.channels];
            clip.GetData(data, Mathf.Clamp(sampleOffset - fftCount / 2, 0, clip.samples - fftCount));
            for (int a = 0; a < data.Length; a++) 
            {
                switch ((a + sampleOffset) % clip.channels)
                {
                    case 0: fftL[a / clip.channels] = data[a]; break;
                    case 1: fftR[a / clip.channels] = data[a]; break;
                }
            }
            FFT.Transform(fftL, Chartmaker.Preferences.FFTWindow);
            if (clip.channels == 1) fftR = fftL;
            else FFT.Transform(fftR, Chartmaker.Preferences.FFTWindow);

            float[] barsL = new float[barCount], barsR = new float[barCount];
            for (int a = 0; a < barCount; a++) 
            {
                for (int i = 1 << a; i < 1 << (a + 1); i++) 
                {
                    barsL[a] += fftL[i] * (a + 1);
                    barsR[a] += fftR[i] * (a + 1);
                }
                barsL[a] *= Mathf.Pow(1 / (1 - (float)a / barCount), 2) * (a + 1) / fftCount / 25;
                barsR[a] *= Mathf.Pow(1 / (1 - (float)a / barCount), 2) * (a + 1) / fftCount / 25;
            }
            float sizeX = rect.width / barCount;    
            for (int a = 0; a < barCount; a++) 
            {
                Image barL = AddBar();
                
                float heightL = barL.rectTransform.rect.height / rect.height;
                heightL = rect.height * Mathf.Min(Mathf.Clamp(Mathf.Sqrt(barsL[a] / (1 << a) / 2), heightL - Time.deltaTime / fallTime, heightL + Time.deltaTime / riseTime), 1);
                barL.rectTransform.sizeDelta = new Vector2(sizeX, heightL);
                barL.rectTransform.anchoredPosition = new Vector2(a * sizeX, 0);
                barL.color = color * new Color(1, 1, 1, .5f);

                Image barR = AddBar();
                float heightR = barR.rectTransform.rect.height / rect.height;
                heightR = rect.height * Mathf.Min(Mathf.Clamp(Mathf.Sqrt(barsR[a] / (1 << a) / 2), heightR - Time.deltaTime / fallTime, heightR + Time.deltaTime / riseTime), 1);
                barR.rectTransform.sizeDelta = new Vector2(sizeX, heightR);
                barR.rectTransform.anchoredPosition = new Vector2(a * sizeX, 0);
                barR.color = color * new Color(1, 1, 1, .5f);

                Image barC = AddBar();
                float heightC = barC.rectTransform.anchoredPosition.y; 
                heightC = Mathf.Min(Mathf.Max(heightL, heightR, heightC - Time.deltaTime / fallTime / 2 * rect.height), rect.height - 1);
                barC.rectTransform.sizeDelta = new Vector2(sizeX, 1);
                barC.rectTransform.anchoredPosition = new Vector2(a * sizeX, heightC);
                barC.color = color * new Color(1, 1, 1, 1);
            }
        }
        else if (VisualizerMode == VisualizerMode.SoundWaves)
        {
            int dataCount = (int)rect.width + 1;

            float[] dataL = new float[dataCount], dataR = new float[dataCount];
            float[] data = new float[dataCount * clip.channels];
            clip.GetData(data, Mathf.Clamp(sampleOffset - dataCount / 2, 0, clip.samples - dataCount));
            for (int a = 0; a < data.Length; a++) 
            {
                switch ((a + sampleOffset) % clip.channels)
                {
                    case 0: dataL[a / clip.channels] = data[a]; break;
                    case 1: dataR[a / clip.channels] = data[a]; break;
                }
            }
            if (clip.channels == 1) dataR = dataL;

            for (int a = 0; a < dataCount - 1; a++) 
            {
                Image barL = AddBar();
                float minL = Mathf.Min(dataL[a], dataL[a + 1]);
                float maxL = Mathf.Max(dataL[a], dataL[a + 1]);
                barL.rectTransform.sizeDelta = new Vector2(1, (maxL - minL) / 2 * (rect.height - 1) + 1);
                barL.rectTransform.anchoredPosition = new Vector2(a, (minL / 2 + .5f) * (rect.height - 1));
                barL.color = color * new Color(1, 1, 1, .5f);

                Image barR = AddBar();
                float minR = Mathf.Min(dataR[a], dataR[a + 1]);
                float maxR = Mathf.Max(dataR[a], dataR[a + 1]);
                barR.rectTransform.sizeDelta = new Vector2(1, (maxR - minR) / 2 * (rect.height - 1) + 1);
                barR.rectTransform.anchoredPosition = new Vector2(a, (minR / 2 + .5f) * (rect.height - 1));
                barR.color = color * new Color(1, 1, 1, .5f);
            }
        }
        else if (VisualizerMode == VisualizerMode.PitchLines)
        {
            const int fftCount = 1 << 8;

            float[] fftL = new float[fftCount], fftR = new float[fftCount];
            float[] data = new float[fftCount * clip.channels];
            clip.GetData(data, Mathf.Clamp(sampleOffset - fftCount / 2, 0, clip.samples - fftCount));
            for (int a = 0; a < data.Length; a++) 
            {
                switch ((a + sampleOffset) % clip.channels)
                {
                    case 0: fftL[a / clip.channels] = data[a]; break;
                    case 1: fftR[a / clip.channels] = data[a]; break;
                }
            }
            FFT.Transform(fftL, Chartmaker.Preferences.FFTWindow);
            if (clip.channels == 1) fftR = fftL;
            else FFT.Transform(fftR, Chartmaker.Preferences.FFTWindow);

            float[] notes = new float[12];
            for (int a = 0; a < fftCount / 2; a++) 
            {
                int note = (Mathf.RoundToInt(Mathf.Log((a + 1f) / fftCount / 440 * Chartmaker.main.CurrentSong.Clip.frequency, 2) * 12) % 12 + 12) % 12;
                float mul = (a + 1) / (float)fftCount * 2;
                notes[note] = Mathf.Max(notes[note], Mathf.Sqrt(fftL[a]) * mul, Mathf.Sqrt(fftR[a]) * mul);
            }
            float sizeX = rect.width / 12;  
            int target = 0;  
            float[] sems = {1, 3, 6, 8, 10};
            for (int a = 0; a < 12; a++) 
            {
                Image barL = AddBar();
                barL.rectTransform.sizeDelta = new Vector2(sizeX, rect.height * (Array.IndexOf(sems, a) >= 0 ? .5f : 1));
                barL.rectTransform.anchoredPosition = new Vector2(a * sizeX, 0);
                barL.color = color * new Color(1, 1, 1, .25f) * notes[a];
                if (notes[target] < notes[a]) target = a;
            }
            Image freqBar = AddBar();
            freqBar.rectTransform.sizeDelta = new Vector2(sizeX, 2);
            freqBar.rectTransform.anchoredPosition = new Vector2(target * sizeX, 0);
            freqBar.color = color * new Color(1, 1, 1);
        }

        while (VisualizerBars.Count > count)
        {
            Destroy(VisualizerBars[count].gameObject);
            VisualizerBars.RemoveAt(count);
        }
    }

    public void SwitchVisualizer()
    {
        VisualizerMode = (VisualizerMode)(((int)VisualizerMode + 1) % 5);
    }

    public void SwitchVisualizer(VisualizerMode mode)
    {
        VisualizerMode = mode;
    }

    public void ShowVisualizerMenu()
    {
        ContextMenuListAction VisItem (string name, VisualizerMode mode) 
            => new ContextMenuListAction(name, () => SwitchVisualizer(mode), _checked: VisualizerMode == mode);

        print("Showing visualizer menu");
        ContextMenuHolder.main.OpenRoot(new ContextMenuList(
            VisItem("Metronome", VisualizerMode.Metronome),
            new ContextMenuListSeparator(),
            VisItem("Classic Bars", VisualizerMode.FrequencyBars),
            VisItem("Oscilloscope", VisualizerMode.SoundWaves),
            new ContextMenuListSeparator(),
            VisItem("Glowing Piano", VisualizerMode.PitchLines),
            new ContextMenuListSeparator(),
            VisItem("Disabled", VisualizerMode.None)
        ), Visualizer, offset: new (2, -4));
    }
}

public enum VisualizerMode
{
    Metronome,
    FrequencyBars,
    SoundWaves,
    PitchLines,
    None,
}
