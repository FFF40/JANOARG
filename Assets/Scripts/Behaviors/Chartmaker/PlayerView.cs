using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IDragHandler, IEndDragHandler
{
    public static PlayerView main;

    public Camera MainCamera;
    public Image BoundingBox;
    [Space]
    public ChartManager Manager;
    [Space]
    [Header("Cover")]
    public CoverViewMode CurrentCoverViewMode;
    public GameObject DarkBackground;
    public Image CoverBackground;
    public RectMask2D CoverMask;
    public RawImage CoverLayerSample;
    public List<RawImage> CoverLayers { get; private set; } = new();
    public RectTransform IconRenderCanvas;
    [Space]
    public GameObject CoverToolbar;
    public GameObject MaskButtonHighlight;
    public GameObject PanoramaButtonHighlight;
    public GameObject IconButtonHighlight;
    [Space]
    [Header("World")]
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
    public AudioClip NormalFlickHitSound;
    public AudioClip CatchFlickHitSound;
    [Space]
    public Graphic NotificationText;
    public Graphic NotificationBox;
    [Space]
    public RectTransform CurrentLaneLine;
    public RectTransform SelectedItemLine;
    public RectTransform StartHandle;
    public RectTransform CenterHandle;
    public RectTransform EndHandle;
    [Space]
    public float[] GridSize = {0.5f};

    float CurrentTime;
    
    int[] HitObjectsRemaining = new [] { 0, 0 };

    public HandleDragMode CurrentDragMode;
    bool isDragged;
    bool isAnimating;
    float lastTargetAspect;
    Vector2 CoverPosition;

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

        float targetAspect;
        if (HierarchyPanel.main.CurrentMode == HierarchyMode.PlayableSong) 
        {
            safeZone.yMin += 32;
            switch (CurrentCoverViewMode)
            {
                case CoverViewMode.Panorama: targetAspect = 880 / 200f; break;
                case CoverViewMode.Icon: targetAspect = 1; break;
                default: targetAspect = 880 / 200f; break;
            }
            
        }
        else 
        {
            targetAspect = 3 / 2f;
        }

        if (safeZone.width / safeZone.height > targetAspect)
        {
            float width = safeZone.height * targetAspect;
            safeZone.x += (safeZone.width - width) / 2;
            safeZone.width = width;
        }
        else
        {
            float height = safeZone.width / targetAspect;
            safeZone.y += (safeZone.height - height) / 2;
            safeZone.height = height;
        }

        BoundingBox.rectTransform.sizeDelta = safeZone.size;
        float camRatio = safeZone.height / bound.height;
        MainCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;

        if (CurrentTime != Chartmaker.main.SongSource.time || targetAspect != lastTargetAspect) UpdateObjects();
        lastTargetAspect = targetAspect;
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
            BoundingBox.color = NotificationText.color = NotificationBox.color = Manager.PalleteManager.CurrentPallete.InterfaceColor;

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
            
            if (Chartmaker.main.SongSource.isPlaying && !TimelinePanel.main.isDragged && PlayOptions.HitsoundsVolume > 0)
            {
                if (Manager.HitObjectsRemaining[0] < HitObjectsRemaining[0])
                {
                    SoundPlayer.PlayOneShot(Chartmaker.Preferences.PerfectHitsounds ? NormalFlickHitSound : NormalHitSound, PlayOptions.HitsoundsVolume);
                }
                if (Manager.HitObjectsRemaining[1] < HitObjectsRemaining[1])
                {
                    SoundPlayer.PlayOneShot(Chartmaker.Preferences.PerfectHitsounds ? CatchFlickHitSound : CatchHitSound, PlayOptions.HitsoundsVolume);
                }
            }
            HitObjectsRemaining = Manager.HitObjectsRemaining;
        }

        UpdateHandles();

        if (HierarchyPanel.main.CurrentMode == HierarchyMode.PlayableSong) 
        {
            DarkBackground.SetActive(true);
            CoverToolbar.SetActive(true);

            switch (CurrentCoverViewMode) 
            {
                case CoverViewMode.Panorama:
                    CoverBackground.rectTransform.sizeDelta = new (880, 200);
                    break;
                case CoverViewMode.Icon:
                    CoverBackground.rectTransform.sizeDelta = Vector2.one * Chartmaker.main.CurrentSong.Cover.IconSize;
                    break;
            }

            float scale = CoverBackground.rectTransform.localScale.x;
            Vector2 parallaxOffset = CoverPosition / scale;
            if (CurrentCoverViewMode == CoverViewMode.Icon) parallaxOffset -= Chartmaker.main.CurrentSong.Cover.IconCenter / scale;

            BoundingBox.color = NotificationText.color = NotificationBox.color = Color.white;
            BoundingBox.rectTransform.anchoredPosition = new Vector2 (0, 16) + CoverPosition;
            CoverBackground.rectTransform.localScale = Vector3.one * (BoundingBox.rectTransform.rect.width / CoverBackground.rectTransform.sizeDelta.x);
            CoverBackground.rectTransform.anchoredPosition = BoundingBox.rectTransform.anchoredPosition;
            CoverBackground.color = Chartmaker.main.CurrentSong.Cover.BackgroundColor;

            int index = 0;
            foreach (CoverLayer layer in Chartmaker.main.CurrentSong.Cover.Layers) {
                RawImage image;
                if (CoverLayers.Count <= index)
                {
                    image = Instantiate(CoverLayerSample, CoverBackground.rectTransform);
                    CoverLayers.Add(image);
                }
                else 
                {
                    image = CoverLayers[index];
                }

                image.texture = layer.Texture;
                if (layer.Tiling)
                {
                    image.rectTransform.sizeDelta = CoverBackground.rectTransform.sizeDelta;
                    image.rectTransform.anchoredPosition = Vector2.zero;
                    Vector2 imgSize = new Vector2(1, layer.Texture.height / layer.Texture.width) * 880 * layer.Scale;
                    image.uvRect = Rect2UV(new (
                        -CoverBackground.rectTransform.sizeDelta * .5f,
                        CoverBackground.rectTransform.sizeDelta
                    ), new (
                        layer.Position - parallaxOffset * layer.ParallaxFactor - imgSize * .5f,
                        imgSize
                    ));
                }
                else 
                {
                    image.rectTransform.sizeDelta = new Vector2(1, layer.Texture.height / layer.Texture.width) * layer.Scale * 880;
                    image.rectTransform.anchoredPosition = layer.Position - parallaxOffset * layer.ParallaxFactor;
                    image.uvRect = new (0, 0, 1, 1);
                }

                index++;
            }

            while (CoverLayers.Count > Chartmaker.main.CurrentSong.Cover.Layers.Count)
            {
                Destroy(CoverLayers[^1].gameObject);
                CoverLayers.RemoveAt(CoverLayers.Count - 1);
            }

            UpdateCoverToolbar();
        }
        else 
        {
            BoundingBox.rectTransform.anchoredPosition = new (0, 0);
            DarkBackground.SetActive(false);
            CoverToolbar.SetActive(false);
        }
    }

    public void UpdateCoverToolbar()
    {
        MaskButtonHighlight.SetActive(CoverMask.enabled);

        PanoramaButtonHighlight.SetActive(CurrentCoverViewMode == CoverViewMode.Panorama);
        IconButtonHighlight.SetActive(CurrentCoverViewMode == CoverViewMode.Icon);
    }

    public void ToggleCoverMask()
    {
        CoverMask.enabled = !CoverMask.enabled;
        UpdateCoverToolbar();
    }

    public void UpdateHandles() 
    {
        CurrentLaneLine.gameObject.SetActive(false);
        SelectedItemLine.gameObject.SetActive(false);
        StartHandle.gameObject.SetActive(false);
        CenterHandle.gameObject.SetActive(false);
        EndHandle.gameObject.SetActive(false);

        if (Chartmaker.main.SongSource.isPlaying)
        {
            return;
        }
        if (HierarchyPanel.main.CurrentMode == HierarchyMode.PlayableSong)
        {
            switch (InspectorPanel.main.CurrentObject)
            {
                case CoverLayer layer: 
                {
                    float scale = CoverBackground.rectTransform.localScale.x;
                    Vector2 offset = new Vector2(0, 16) + CoverPosition * (1 - layer.ParallaxFactor);
                    if (CurrentCoverViewMode == CoverViewMode.Icon) offset -=
                        (1 - layer.ParallaxFactor) / scale * Chartmaker.main.CurrentSong.Cover.IconCenter;
                    
                    Vector2 center = layer.Position * scale + offset;
                    CenterHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Center);
                    CenterHandle.anchoredPosition = center;
                    
                    Vector2 left = Vector2.right * 440 * layer.Scale * scale + center;
                    StartHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Start);
                    StartHandle.anchoredPosition = left;
                    
                    SelectedItemLine.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Start);
                    SelectedItemLine.anchoredPosition = (center + left) / 2;
                    SelectedItemLine.sizeDelta = new(440 * layer.Scale * scale, SelectedItemLine.sizeDelta.y);
                    SelectedItemLine.eulerAngles = Vector2.zero;
                } break;
            }

        // TODO: Maybe implement this? What is this for?
#pragma warning disable CS0164 // This label has not been referenced
        endSel: ;
#pragma warning restore CS0164 // This label has not been referenced
        }
        if (HierarchyPanel.main.CurrentMode == HierarchyMode.Chart) 
        {
            {
                if (Chartmaker.main.CurrentChart != null && InspectorPanel.main.CurrentHierarchyObject is Lane currentLane)
                {
                    int index = Chartmaker.main.CurrentChart.Lanes.IndexOf(currentLane);
                    if (index < 0) goto endLane;
                    LaneManager man = Manager.Lanes[index];
                    if ((man.CurrentMesh?.vertexCount ?? 0) > 2)
                    {
                        Vector2 start = MainCamera.WorldToScreenPoint(man.StartPos);
                        Vector2 end = MainCamera.WorldToScreenPoint(man.EndPos);
                        CurrentLaneLine.gameObject.SetActive(true);
                        CurrentLaneLine.position = (start + end) / 2;
                        CurrentLaneLine.sizeDelta = new(Vector2.Distance(start, end), CurrentLaneLine.sizeDelta.y);
                        CurrentLaneLine.eulerAngles = new(0, 0, Vector2.SignedAngle(Vector2.left, end - start));
                    }
                }
            }

            endLane: 

            switch (InspectorPanel.main.CurrentObject)
            {
                case Lane lane: 
                {
                    int index = Chartmaker.main.CurrentChart.Lanes.IndexOf(lane);
                    if (index < 0) goto endSel;
                    LaneManager man = Manager.Lanes[index];
                    
                    Vector2 center = MainCamera.WorldToScreenPoint(man.FinalPosition);
                    CenterHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Center);
                    CenterHandle.position = center;

                    if ((man.CurrentMesh?.vertexCount ?? 0) > 2)
                    {
                        Vector2 start = MainCamera.WorldToScreenPoint(man.StartPos);
                        Vector2 end = MainCamera.WorldToScreenPoint(man.EndPos);
                        SelectedItemLine.gameObject.SetActive(true);
                        SelectedItemLine.position = (start + end) / 2;
                        SelectedItemLine.sizeDelta = new(Vector2.Distance(start, end), SelectedItemLine.sizeDelta.y);
                        SelectedItemLine.eulerAngles = new(0, 0, Vector2.SignedAngle(Vector3.left, end - start));
                        if (SelectedItemLine.sizeDelta.x > 20) 
                        {
                            StartHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Start);
                            StartHandle.position = start;
                            EndHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.End);
                            EndHandle.position = end;
                            EndHandle.eulerAngles = new(0, 0, Vector2.SignedAngle(Vector2.up, end - start));
                        }
                    }
                } break;
                case LaneStep step: 
                {
                    if (InspectorPanel.main.CurrentHierarchyObject is not Lane currentLane) return;

                    int lindex = Chartmaker.main.CurrentChart.Lanes.IndexOf(currentLane);
                    if (lindex < 0) goto endSel;
                    LaneManager lman = Manager.Lanes[lindex];

                    int index = currentLane.LaneSteps.IndexOf(step);
                    if (index < 0) goto endSel;
                    LaneStepManager man = lman.Steps[index];

                    if (man.Offset >= Chartmaker.main.SongSource.time)
                    {
                        Vector3 offset = lman.FinalRotation * Vector3.forward * (man.Distance - lman.CurrentDistance) + lman.FinalPosition;
                        Vector2 wcenter = (man.CurrentStep.StartPos + man.CurrentStep.EndPos) / 2;
                        Vector2 start = MainCamera.WorldToScreenPoint(lman.FinalRotation * man.CurrentStep.StartPos + offset);
                        Vector2 end = MainCamera.WorldToScreenPoint(lman.FinalRotation * man.CurrentStep.EndPos + offset);
                        Vector2 center = MainCamera.WorldToScreenPoint(lman.FinalRotation * wcenter + offset);
                        SelectedItemLine.gameObject.SetActive(true);
                        SelectedItemLine.position = (start + end) / 2;
                        SelectedItemLine.sizeDelta = new(Vector2.Distance(start, end), SelectedItemLine.sizeDelta.y);
                        SelectedItemLine.eulerAngles = new(0, 0, Vector2.SignedAngle(Vector3.left, end - start));
                        CenterHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Center);
                        CenterHandle.position = center;
                        if (SelectedItemLine.sizeDelta.x > 20) 
                        {
                            StartHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Start);
                            StartHandle.position = start;
                            EndHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.End);
                            EndHandle.position = end;
                            EndHandle.eulerAngles = new(0, 0, Vector2.SignedAngle(Vector2.up, end - start));
                            
                        }
                    }
                } break;
                case HitObject hit: 
                {
                    if (InspectorPanel.main.CurrentHierarchyObject is not Lane currentLane) return;

                    int lindex = Chartmaker.main.CurrentChart.Lanes.IndexOf(currentLane);
                    if (lindex < 0) goto endSel;
                    LaneManager lman = Manager.Lanes[lindex];

                    int index = currentLane.Objects.IndexOf(hit);
                    if (index < 0) goto endSel;
                    HitObjectManager man = lman.Objects[index];

                    if (man.TimeEnd >= Chartmaker.main.SongSource.time)
                    {
                        Vector2 start = MainCamera.WorldToScreenPoint(lman.FinalRotation * (man.StartPos + lman.CurrentDistance * Vector3.back) + lman.FinalPosition);
                        Vector2 end = MainCamera.WorldToScreenPoint(lman.FinalRotation * (man.EndPos + lman.CurrentDistance * Vector3.back) + lman.FinalPosition);
                        Vector2 center = MainCamera.WorldToScreenPoint(lman.FinalRotation * (man.Position + lman.CurrentDistance * Vector3.back) + lman.FinalPosition);
                        SelectedItemLine.gameObject.SetActive(true);
                        SelectedItemLine.position = (start + end) / 2;
                        SelectedItemLine.sizeDelta = new(Vector2.Distance(start, end), SelectedItemLine.sizeDelta.y);
                        SelectedItemLine.eulerAngles = new(0, 0, Vector2.SignedAngle(Vector3.left, end - start));
                        CenterHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Center);
                        CenterHandle.position = center;
                        if (SelectedItemLine.sizeDelta.x > 20) 
                        {
                            StartHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.Start);
                            StartHandle.position = start;
                            EndHandle.gameObject.SetActive(CurrentDragMode is HandleDragMode.None or HandleDragMode.End);
                            EndHandle.position = end;
                            EndHandle.eulerAngles = new(0, 0, Vector2.SignedAngle(Vector2.up, end - start));
                        }
                    }
                } break;
            }
            
            endSel: ;
        }
    }

    public void InitMeshes() 
    {
        if (!FreeFlickIndicator) 
        {
            Mesh mesh = new();
            List<Vector3> verts = new();
            List<int> tris = new();

            verts.AddRange(new Vector3[] { new(-1, 0), new(0, 2), new(0, -.5f), new(1, 0), new(0, -2), new(0, .5f) });
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isAnimating) return;

        bool contains(RectTransform rt) => rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.pressPosition, eventData.pressEventCamera);

        CurrentDragMode = HandleDragMode.None;

        if (contains((RectTransform)CoverToolbar.transform)) CurrentDragMode = HandleDragMode.None;
        else if (contains(StartHandle)) CurrentDragMode = HandleDragMode.Start;
        else if (contains(CenterHandle)) CurrentDragMode = HandleDragMode.Center;
        else if (contains(EndHandle)) CurrentDragMode = HandleDragMode.End;
        else if (HierarchyPanel.main.CurrentMode == HierarchyMode.PlayableSong) CurrentDragMode = HandleDragMode.Background;

        if (CurrentDragMode == HandleDragMode.None) return;

        if (HierarchyPanel.main.CurrentMode == HierarchyMode.PlayableSong && CurrentDragMode == HandleDragMode.Background)
        {
            OnDragEvent += (ev) => {
                CoverPosition += ev.delta;
            };
        }
        else switch (InspectorPanel.main.CurrentObject)
        {
            
            case CoverLayer layer:
            {
                float scale = CoverBackground.rectTransform.localScale.x;
                Vector2 offset = new (0, 16);

                OnDragEvent += (ev) => {
                    ChartmakerHistory history = Chartmaker.main.History;
                    if (CurrentDragMode == HandleDragMode.Center)
                    {
                        history.SetItem(layer, "Position", layer.Position + ev.delta / scale);
                    }
                    else if (CurrentDragMode == HandleDragMode.Start)
                    {
                        history.SetItem(layer, "Scale", layer.Scale + ev.delta.x / 440 / scale);
                    }
                    Chartmaker.main.OnHistoryUpdate();
                };
            }
            break;

            case Lane lane:
            {
                int index = Chartmaker.main.CurrentChart.Lanes.IndexOf(lane);
                if (index < 0) return;
                LaneManager man = Manager.Lanes[index];
                
                Vector3 inv(Vector3 x) => Quaternion.Inverse(Quaternion.Euler(man.CurrentLane.Rotation)) * (x - man.CurrentLane.Position);

                Func<Vector3> get = 
                    CurrentDragMode == HandleDragMode.Start ? (() => inv(man.StartPos)) : 
                    CurrentDragMode == HandleDragMode.Center ? (() => man.CurrentLane.Position) : 
                    CurrentDragMode == HandleDragMode.End ? (() => inv(man.EndPos)) : null;
                    
                Vector3 gizmoAnchor = get();
                
                OnDragEvent += (ev) => {
                    Vector3? dragPos = CurrentDragMode == HandleDragMode.Center ? 
                        RaycastScreenToPlane(ev.position, Vector3.forward * get().z, Quaternion.identity) :
                        RaycastScreenToPlane(ev.position, man.CurrentLane.Position, Quaternion.Euler(man.CurrentLane.Rotation));
                    if (dragPos != null)
                    {
                        if (CurrentDragMode is not HandleDragMode.Center) dragPos = inv((Vector3)dragPos);
                        if (GridSize[0] > 0)
                        {
                            Vector3 des = new Vector3();
                            for (int x = 0; x < 3; x++) des[x] = Mathf.Round((dragPos?[x] ?? 0) / GridSize[0]) * GridSize[0];
                            dragPos = des;
                        } 
                    }
                    else
                    {
                        dragPos = gizmoAnchor;
                    }
                
                    if (CurrentDragMode == HandleDragMode.Start) 
                        DoMove<ChartmakerMoveLaneStartAction, Lane>(lane, (Vector3)dragPos - get());
                    else if (CurrentDragMode == HandleDragMode.Center) 
                        DoMove<ChartmakerMoveLaneAction, Lane>(lane, (Vector3)dragPos - get());
                    else if (CurrentDragMode == HandleDragMode.End) 
                        DoMove<ChartmakerMoveLaneEndAction, Lane>(lane, (Vector3)dragPos - get());
                };                  
            } 
            break;
            
            case LaneStep step:
            {
                if (InspectorPanel.main.CurrentHierarchyObject is not Lane currentLane) return;

                int lindex = Chartmaker.main.CurrentChart.Lanes.IndexOf(currentLane);
                if (lindex < 0) return;
                LaneManager lman = Manager.Lanes[lindex];

                int index = currentLane.LaneSteps.IndexOf(step);
                if (index < 0) return;
                LaneStepManager man = lman.Steps[index];

                Vector3 inv(Vector3 x) => Quaternion.Inverse(lman.FinalRotation) * (x - lman.FinalPosition);

                Func<Vector3> get = 
                    CurrentDragMode == HandleDragMode.Start ? (() => man.CurrentStep.StartPos) : 
                    CurrentDragMode == HandleDragMode.Center ? (() => (man.CurrentStep.StartPos + man.CurrentStep.EndPos) / 2) : 
                    CurrentDragMode == HandleDragMode.End ? (() => man.CurrentStep.EndPos) : null;
                    
                Vector3 gizmoAnchor = get();

                OnDragEvent += (ev) => {
                    Vector3? dragPos = RaycastScreenToPlane(ev.position, lman.FinalPosition + lman.FinalRotation * Vector3.forward * (man.Distance - lman.CurrentDistance), lman.FinalRotation);
                    if (dragPos != null)
                    {
                        dragPos = inv((Vector3)dragPos);
                        if (GridSize[0] > 0)
                        {
                            Vector3 des = new();
                            for (int x = 0; x < 3; x++) des[x] = Mathf.Round((dragPos?[x] ?? 0) / GridSize[0]) * GridSize[0];
                            dragPos = des;
                        } 
                    }
                    else
                    {
                        dragPos = gizmoAnchor;
                    }
                
                    if (CurrentDragMode == HandleDragMode.Start) 
                        DoMove<ChartmakerMoveLaneStepStartAction, LaneStep>(step, (Vector3)dragPos - get());
                    else if (CurrentDragMode == HandleDragMode.Center) 
                        DoMove<ChartmakerMoveLaneStepAction, LaneStep>(step, (Vector3)dragPos - get());
                    else if (CurrentDragMode == HandleDragMode.End) 
                        DoMove<ChartmakerMoveLaneStepEndAction, LaneStep>(step, (Vector3)dragPos - get());
                };
            }
            break;
            
            case HitObject hit:
            {
                if (InspectorPanel.main.CurrentHierarchyObject is not Lane lane) return;

                int lindex = Chartmaker.main.CurrentChart.Lanes.IndexOf(lane);
                if (lindex < 0) return;
                LaneManager lman = Manager.Lanes[lindex];

                int index = lane.Objects.IndexOf(hit);
                if (index < 0) return;
                HitObjectManager man = lman.Objects[index];
                
                Vector3 inv(Vector3 x)
                {
                    Vector3 point = Quaternion.Inverse(lman.FinalRotation) * (x - lman.FinalPosition) - Vector3.forward * (man.Position.z - lman.CurrentDistance);
                    return Vector3.right * (Quaternion.Euler(0, 0, Vector2.SignedAngle(lman.EndPos - lman.StartPos, Vector2.right)) * (point - lman.StartPos)).x / Vector2.Distance(lman.StartPos, lman.EndPos);
                }

                Func<Vector3> get = 
                    CurrentDragMode == HandleDragMode.Start ? (() => Vector3.right * man.CurrentHit.Position) : 
                    CurrentDragMode == HandleDragMode.Center ? (() => Vector3.right * (man.CurrentHit.Position + man.CurrentHit.Length / 2)) : 
                    CurrentDragMode == HandleDragMode.End ? (() => Vector3.right * (man.CurrentHit.Position + man.CurrentHit.Length)) : null;
                    
                Vector3 gizmoAnchor = get();

                OnDragEvent += (ev) => {
                    Vector3? dragPos = RaycastScreenToPlane(ev.position, lman.FinalPosition + lman.FinalRotation * Vector3.forward * (man.Position.z - lman.CurrentDistance), lman.FinalRotation);
                    if (dragPos != null)
                    {
                        dragPos = inv((Vector3)dragPos);
                        if (GridSize[0] > 0)
                        {
                            Vector3 des = new();
                            des[0] = Mathf.Round((dragPos?[0] ?? 0) / 0.05f) * 0.05f;
                            dragPos = des;
                        } 
                    }
                    else
                    {
                        dragPos = gizmoAnchor;
                    }
                
                    if (CurrentDragMode == HandleDragMode.Start) 
                        DoMove<ChartmakerMoveHitObjectStartAction, HitObject>(hit, (Vector3)dragPos - get());
                    else if (CurrentDragMode == HandleDragMode.Center) 
                        DoMove<ChartmakerMoveHitObjectAction, HitObject>(hit, (Vector3)dragPos - get());
                    else if (CurrentDragMode == HandleDragMode.End) 
                        DoMove<ChartmakerMoveHitObjectEndAction, HitObject>(hit, (Vector3)dragPos - get());
                };
            }
            break;
        }
        
        UpdateHandles();
        UpdateCursor(eventData.position, eventData.pressEventCamera);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragged)
        {
            OnEndDrag(eventData);
        }
    }

    CursorType CurrentCursor = 0;

    public void UpdateCursor(Vector2 position, Camera eventCamera)
    {
        bool contains(RectTransform rt) => rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rt, position, eventCamera);

        CursorType Cursor = 0;

        if (CurrentDragMode != HandleDragMode.None) 
        {
           Cursor = CursorType.Grabbing;
        }
        else if (contains((RectTransform)transform)) 
        {
            if (
                (!contains((RectTransform)CoverToolbar.transform)) &&
                (contains(StartHandle) || contains(CenterHandle) || contains(EndHandle))
            ) Cursor = CursorType.Grab;
        }

        if (CurrentCursor != Cursor)
        {
            if (CurrentCursor != 0) CursorChanger.PopCursor();
            if (Cursor != 0) CursorChanger.PushCursor(Cursor);
            CurrentCursor = Cursor;
            BorderlessWindow.UpdateCursor();
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isDragged)
        {
            UpdateCursor(eventData.position, eventData.pressEventCamera);
        }
    }

    public delegate void PointerEvent(PointerEventData eventData);

    public void OnDrag(PointerEventData eventData) 
    {
        if (CurrentDragMode != HandleDragMode.None)
        {
            isDragged = true;
            OnDragEvent?.Invoke(eventData);
            UpdateObjects();
        }
    }

    public PointerEvent OnDragEvent;

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CurrentDragMode != HandleDragMode.None)
        {
            InspectorPanel.main.UpdateForm();
            TimelinePanel.main.UpdateItems();
        }
        isDragged = false;
        OnDragEvent = null;
        CurrentDragMode = HandleDragMode.None;
        UpdateHandles();
        UpdateCursor(eventData.position, eventData.pressEventCamera);
    }
    
    public Vector3? RaycastScreenToPlane(Vector3 pos, Vector3 center, Quaternion rotation)
    {
        Plane plane = new (rotation * Vector3.back, center);
        Ray ray = MainCamera.ScreenPointToRay(new Vector2(pos.x, pos.y));
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return null;
    }

    public Rect Rect2UV(Rect parent, Rect child) 
    {
        return new(
            (parent.xMin - child.xMin) / child.width,
            (parent.yMin - child.yMin) / child.height,
            parent.width / child.width,
            parent.height / child.height
        );
    }

    public void DoMove<TAction, TTarget>(TTarget item, Vector3 offset) where TAction : ChartmakerMoveAction<TTarget>, new()
    {
        if (offset == Vector3.zero) return;

        TAction action = null;
        ChartmakerHistory history = Chartmaker.main.History;

        if (history.ActionsBehind.Count > 0 && history.ActionsBehind.Peek() is TAction)
        {
            action = (TAction)history.ActionsBehind.Peek();
            if (!action.Item.Equals(item)) action = null;
        }

        if (action == null)
        {
            action = new()
            {
                Item = item
            };
            history.ActionsBehind.Push(action);
        }
        history.ActionsAhead.Clear();

        action.Undo();
        action.Offset += offset;
        action.Redo();

        Chartmaker.main.OnHistoryUpdate();
    }

    public void SetCoverViewMode(int mode) 
    {
        SetCoverViewMode((CoverViewMode)mode);
    }

    public void SetCoverViewMode(CoverViewMode mode) 
    {
        CurrentCoverViewMode = mode;
    }

    public void MoveCoverToCenter()
    {
        if (!isAnimating) StartCoroutine(MoveCoverToCenterAnim());
    }

    IEnumerator MoveCoverToCenterAnim()
    {
        isAnimating = true;

        Vector2 posStart = CoverPosition;

        void Animate1(float t) 
        {
            float ease = Ease.Get(t, EaseFunction.Cubic, EaseMode.Out);

            CoverPosition = posStart * (1 - ease);
            UpdateObjects();
        }
        for (float t = 0; t < 1; t += Time.deltaTime / .2f) { Animate1(t); yield return null; }
        Animate1(1);

        isAnimating = false;
    }

    public void UpdateIconFile() 
    {
        Transform originalParent = CoverBackground.rectTransform.parent;
        IconRenderCanvas.gameObject.SetActive(true);
        CoverBackground.rectTransform.SetParent(IconRenderCanvas);
        CoverBackground.rectTransform.sizeDelta = Vector2.one * Chartmaker.main.CurrentSong.Cover.IconSize;
        CoverBackground.rectTransform.localScale = Vector2.one * IconRenderCanvas.sizeDelta.x / Chartmaker.main.CurrentSong.Cover.IconSize;
        CoverBackground.rectTransform.anchoredPosition3D = Vector3.zero;
        
        Vector2 parallaxOffset = Chartmaker.main.CurrentSong.Cover.IconCenter;
        
        int index = 0;
        foreach (CoverLayer layer in Chartmaker.main.CurrentSong.Cover.Layers) {
            RawImage image = CoverLayers[index];

            image.texture = layer.Texture;
            
            if (layer.Tiling)
            {
                image.rectTransform.sizeDelta = CoverBackground.rectTransform.sizeDelta;
                image.rectTransform.anchoredPosition = Vector2.zero;
                Vector2 imgSize = new Vector2(1, layer.Texture.height / layer.Texture.width) * 880 * layer.Scale;
                image.uvRect = Rect2UV(new (
                    -CoverBackground.rectTransform.sizeDelta * .5f,
                    CoverBackground.rectTransform.sizeDelta
                ), new (
                    layer.Position - parallaxOffset * layer.ParallaxFactor - imgSize * .5f,
                    imgSize
                ));
            }
            else 
            {
                image.rectTransform.sizeDelta = new Vector2(1, layer.Texture.height / layer.Texture.width) * layer.Scale * 880;
                image.rectTransform.anchoredPosition = layer.Position - parallaxOffset * layer.ParallaxFactor;
                image.uvRect = new (0, 0, 1, 1);
            }

            index++;
        }

        Vector2Int resolution = new (128, 128);

        RenderTexture rtex = new (resolution.x, resolution.y, 24);
        RenderTexture.active = rtex;
        rtex.Create();

        Camera camera = Camera.main;
        camera.targetTexture = rtex;
        camera.rect = new Rect(0, 0, resolution.x, resolution.y);
        camera.Render();

        Texture2D tex = new (resolution.x, resolution.y);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        File.WriteAllBytes(
            Path.Combine(Path.GetDirectoryName(Chartmaker.main.CurrentSongPath), Chartmaker.main.CurrentSong.Cover.IconTarget), 
            tex.EncodeToPNG()
        );
        
        
        RenderTexture.active = camera.targetTexture = null;
        Destroy(tex);
        Destroy(rtex);

        IconRenderCanvas.gameObject.SetActive(false);
        CoverBackground.rectTransform.SetParent(originalParent);
        CoverBackground.rectTransform.anchoredPosition3D = Vector3.zero;
        UpdateObjects();
    }
}

public enum HandleDragMode
{
    None,
    Start,
    Center,
    End,
    Background,
}

public enum CoverViewMode 
{
    Panorama = 0,
    Icon = 1
}