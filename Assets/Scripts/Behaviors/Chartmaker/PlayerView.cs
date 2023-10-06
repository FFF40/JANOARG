using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    public static PlayerView main;

    public Camera MainCamera;
    public Image BoundingBox;
    [Space]
    public ChartManager Manager;
    [Space]
    public Transform Holder;
    public CMLanePlayer LanePlayerSample;
    public List<CMLanePlayer> LanePlayers { get; private set; } = new();
    public CMHitPlayer HitPlayerSample;
    public MeshRenderer HoldMeshSample;
    [Space]
    public Mesh FreeFlickIndicator;
    public Mesh ArrowFlickIndicator;
    [Space]
    public PlayOptionsPanel PlayOptions;
    [Space]
    public AudioSource SoundPlayer;
    public AudioClip NormalHitSound;
    public AudioClip CatchHitSound;

    float CurrentTime;
    
    int[] HitObjectsRemaining = new [] { 0, 0 };

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
        InitMeshes();
    }


    public void Update()
    {
        RectTransform rt = (RectTransform)transform;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        
        Rect bound = new(
            corners[0].x,
            corners[0].y,
            corners[2].x - corners[0].x,
            corners[2].y - corners[0].y
        );

        MainCamera.rect = new(
            bound.x / Screen.width,
            bound.y / Screen.height,
            bound.width / Screen.width,
            bound.height / Screen.height
        );

        Rect safeZone = new(
            bound.x + 12,
            bound.y + 12,
            bound.width - 24,
            bound.height - 24
        );

        if (safeZone.width / safeZone.height > 3 / 2f)
        {
            float width = safeZone.height * 3 / 2;
            safeZone.x += (safeZone.width - width) / 2;
            safeZone.width = width;
        }
        else
        {
            float height = safeZone.width / 3 * 2;
            safeZone.y += (safeZone.height - height) / 2;
            safeZone.height = height;
        }

        BoundingBox.rectTransform.sizeDelta = safeZone.size;
        float camRatio = safeZone.height / bound.height;
        MainCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;

        if (CurrentTime != Chartmaker.main.SongSource.time) UpdateObjects();
    }

    public void UpdateObjects()
    {
        CurrentTime = Chartmaker.main.SongSource.time;

        if (Chartmaker.main.CurrentChart != null)
        {
            if (Chartmaker.main.CurrentChart != Manager?.CurrentChart) 
            {
                Manager = new ChartManager(
                    Chartmaker.main.CurrentSong, Chartmaker.main.CurrentChart,
                    121, InformationBar.main.sec, InformationBar.main.beat
                );
            } else {
                Manager.Update(
                    InformationBar.main.sec, InformationBar.main.beat
                );
            }
            
            MainCamera.transform.position = Manager.Camera.CameraPivot;
            MainCamera.transform.eulerAngles = Manager.Camera.CameraRotation; 
            MainCamera.transform.Translate(Vector3.back * Manager.Camera.PivotDistance);

            RenderSettings.fogColor = MainCamera.backgroundColor = Manager.PalleteManager.CurrentPallete.BackgroundColor;
            BoundingBox.color = Manager.PalleteManager.CurrentPallete.InterfaceColor;

            for (int a = 0; a < Manager.Lanes.Count; a++)
            {
                if (LanePlayers.Count <= a) LanePlayers.Add(Instantiate(LanePlayerSample, Holder));
                LanePlayers[a].UpdateObjects(Manager.Lanes[a]);
            }
            while (LanePlayers.Count > Manager.Lanes.Count)
            {
                Destroy(LanePlayers[Manager.Lanes.Count].gameObject);
                LanePlayers.RemoveAt(Manager.Lanes.Count);
            }
            
            if (!TimelinePanel.main.isDragged && PlayOptions.HitsoundsVolume > 0)
            {
                if (Manager.HitObjectsRemaining[0] < HitObjectsRemaining[0])
                {
                    SoundPlayer.PlayOneShot(NormalHitSound, PlayOptions.HitsoundsVolume);
                }
                if (Manager.HitObjectsRemaining[1] < HitObjectsRemaining[1])
                {
                    SoundPlayer.PlayOneShot(CatchHitSound, PlayOptions.HitsoundsVolume);
                }
            }
            HitObjectsRemaining = Manager.HitObjectsRemaining;

        }
    }

    public void InitMeshes() 
    {
        if (!FreeFlickIndicator) 
        {
            Mesh mesh = new();
            List<Vector3> verts = new();
            List<int> tris = new();

            verts.AddRange(new Vector3[] { new(0, 1.6f), new(1, 0), new(0, -1), new(0, -1.6f), new(-1, 0), new(0, 1) });
            tris.AddRange(new [] {0, 1, 2, 3, 4, 5});

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

            mesh.SetVertices(verts);
            mesh.SetUVs(0, verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            ArrowFlickIndicator = mesh;
        }
    }
}