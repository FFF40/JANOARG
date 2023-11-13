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
    public TMP_Text SongNameLabel;
    public TMP_Text ChartNameLabel;
    public GameObject PlayIcon;
    public GameObject PauseIcon;
    public TMP_Text BeatTimeLabel;
    public TMP_Text SecondTimeLabel;
    [Space]
    public RectTransform ChartButton;
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
            ChartButton.sizeDelta = new Vector2(140, ChartButton.sizeDelta.y);
            ChartDropdownButton.gameObject.SetActive(true);
        }
        else 
        {
            ChartNameLabel.text = "Select Chart...";
            ChartButton.sizeDelta = new Vector2(160, ChartButton.sizeDelta.y);
            ChartDropdownButton.gameObject.SetActive(false);
        }
    }

    public void FocusSong()
    {
        InspectorPanel.main.SetObject(Chartmaker.main.CurrentSong);
    }

    public void FocusChart()
    {
        if (Chartmaker.main.CurrentChart == null)
        {
            ShowChartPopup(ChartButton);
        }
        else
        {
            InspectorPanel.main.SetObject(Chartmaker.main.CurrentChart);
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
        }
        else 
        {
            Chartmaker.main.SongSource.Play();
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
            const int fftCount = 1 << barCount;
            const float riseTime = .05f, fallTime = .5f;

            float[] fftL = new float[fftCount], fftR = new float[fftCount];
            Chartmaker.main.SongSource.GetSpectrumData(fftL, 0, FFTWindow.Rectangular);
            Chartmaker.main.SongSource.GetSpectrumData(fftR, 1, FFTWindow.Rectangular);

            float[] barsL = new float[barCount], barsR = new float[barCount];
            for (int a = 0; a < barCount; a++) 
            {
                for (int i = 1 << a; i < 1 << (a + 1); i++) 
                {
                    barsL[a] += fftL[a] * (a + 1);
                    barsR[a] += fftR[a] * (a + 1);
                }
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

        while (VisualizerBars.Count > count)
        {
            Destroy(VisualizerBars[count].gameObject);
            VisualizerBars.RemoveAt(count);
        }
    }

    public void SwitchVisualizer()
    {
        VisualizerMode = (VisualizerMode)(((int)VisualizerMode + 1) % 3);
    }
}

public enum VisualizerMode
{
    Metronome,
    FrequencyBars,
    None,
}
