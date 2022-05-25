using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChartPlayer : MonoBehaviour
{
    public static ChartPlayer main;

    [Header("Data")]
    public PlayableSong Song;
    public int ChartPosition;
    [Space]
    public bool AutoPlay;
    public float CurrentTime;
    public float Score;
    public float MaxScore;
    public int Combo;

    [Header("Settings")]
    public float SyncThreshold = .05f;

    [Header("Objects")]
    public LanePlayer LanePlayerSample;
    public MeshFilter LaneMeshSample;
    public HitPlayer HitPlayerSample;
    public Camera MainCamera;

    [Header("Interface")]
    public AudioSource AudioPlayer;
    public TMP_Text SongNameLabel;
    public TMP_Text SongArtistLabel;
    public Image DifficultyBox;
    public List<Image> DifficultyIndicators;
    public TMP_Text DifficultyLabel;
    public TMP_Text ScoreText;
    public Slider SongProgressSlider;
    public Image SongProgressFill;
    public TMP_Text ComboText;

    [HideInInspector]
    public Chart CurrentChart;

    public void Awake()
    {
        main = this;
    }

    // Start is called before the first frame update
    public void Start()
    {
        CurrentChart = Instantiate(Song).Charts[ChartPosition];
        
        foreach (Timestamp ts in CurrentChart.Storyboard.Timestamps)
        {
            ts.Duration = Song.Timing.ToSeconds(ts.Time + ts.Duration);
            ts.Time = Song.Timing.ToSeconds(ts.Time);
            ts.Duration -= ts.Time;
        }

        foreach (Lane lane in CurrentChart.Lanes) {
            LanePlayer lp = Instantiate(LanePlayerSample, transform);
            lp.SetLane(lane);
        }

        SongNameLabel.text = Song.SongName;
        SongArtistLabel.text = Song.SongArtist;
        DifficultyLabel.text = CurrentChart.DifficultyLevel;
        AudioPlayer.clip = Song.Clip;

        MainCamera.backgroundColor = CurrentChart.BackgroundColor;
        SetInterfaceColor(CurrentChart.InterfaceColor);

        float camRatio = Mathf.Min(1, (3f * Screen.width) / (2f * Screen.height));
        MainCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;

        UpdateScore();
    }

    void SetInterfaceColor(Color color)
    {
        SongNameLabel.color = SongArtistLabel.color = DifficultyBox.color = DifficultyLabel.color = 
        ScoreText.color = ComboText.color = SongProgressFill.color = color;
        for (int a = 0; a < DifficultyIndicators.Count; a++) 
        {
            DifficultyIndicators[a].color = new Color(color.r, color.g, color.b, color.a * (20 - CurrentChart.DifficultyIndex + a) / 20) * Mathf.Min(CurrentChart.DifficultyIndex - a + 1, 1);
        }
    }

    void UpdateScore() 
    {
        ScoreText.text = ((int)(Score / Mathf.Max(MaxScore, 1) * 1e6)).ToString("D7") + "<size=12>ppm</size>";
        ComboText.text = Combo.ToString("0");
    }

    public void AddScore(float weight, bool combo)
    {
        Score += weight;
        Combo = combo ? Combo + 1 : 0;

        UpdateScore();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        CurrentChart.Advance(CurrentTime);
        if (CurrentTime > 0 && CurrentTime < AudioPlayer.clip.length) 
        {
            if (!AudioPlayer.isPlaying) AudioPlayer.Play();
            if (Mathf.Abs(AudioPlayer.time - CurrentTime) > SyncThreshold) AudioPlayer.time = CurrentTime;
        }
        SongProgressSlider.value = AudioPlayer.time / Song.Clip.length;
        
        MainCamera.transform.position = CurrentChart.CameraPivot;
        MainCamera.transform.eulerAngles = CurrentChart.CameraRotation;
        MainCamera.transform.Translate(Vector3.back * 10);
    }
}
