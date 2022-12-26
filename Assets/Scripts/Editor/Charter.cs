using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEditor;

public class Charter : EditorWindow
{
    [MenuItem("J.A.N.O.A.R.G./Open Charter", false, 0)]
    public static void Open()
    {
        Charter wnd = GetWindow<Charter>();
        wnd.titleContent = new GUIContent("Charter");
        wnd.minSize = new Vector2(960, 600);
    }

    public static void Open(PlayableSong target)
    {
        Charter wnd = GetWindow<Charter>();
        wnd.titleContent = new GUIContent("Charter");
        wnd.minSize = new Vector2(960, 600);
        wnd.TargetSong = target;
    }

    public PlayableSong TargetSong;
    public ExternalChartMeta TargetChartMeta;
    public ExternalChart TargetChart;
    public Lane TargetLane;
    public object TargetThing;
    public object DeletingThing;
    public object ClipboardThing;
    public Timestamp TargetTimestamp;

    public AudioSource CurrentAudioSource;
    public Camera CurrentCamera;
    public AudioClip MetronomeSound;
    public AudioClip NormalHitSound;
    public AudioClip CatchHitSound;

    public bool PlayMetronome;
    public bool SeparateUnits;
    public bool PlayHitsounds;
    public bool FollowSeekLine;
    public int WaveformMode;

    public List<LaneStyleManager> LaneStyleManagers = new List<LaneStyleManager>();
    public List<HitStyleManager> HitStyleManagers = new List<HitStyleManager>();
    public List<Mesh> Meshes = new List<Mesh>();

    CultureInfo invariant = CultureInfo.InvariantCulture;
    public CharterHistory History = new CharterHistory();
    public ChartManager Manager;

    float ScrollSpeed = 121;

    float width, height, pos, dec, beat, currentBeat, bar, min, sec, ms, preciseTime;

    Dictionary<Lane, LaneManager> LaneManagers = new Dictionary<Lane, LaneManager>();

    public void OnDestroy()
    {
        DestroyImmediate(CurrentAudioSource);
        DestroyImmediate(CurrentCamera);
    }

    ///////////////////////
    #region Mesh Generation
    ///////////////////////

    // Literally a miracle
    public Mesh MakeLaneMesh(Lane lane)
    {
        if (this.pos >= lane.LaneSteps[lane.LaneSteps.Count - 1].Offset) return null;

        Mesh mesh = new Mesh();

        float pos = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        void AddStep(Vector3 start, Vector3 end)
        {

            vertices.Add(start);
            vertices.Add(end);
            vertices.Add(start);
            vertices.Add(end);

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);

            if (vertices.Count >= 8)
            {
                tris.Add(vertices.Count - 1);
                tris.Add(vertices.Count - 5);
                tris.Add(vertices.Count - 6);

                tris.Add(vertices.Count - 6);
                tris.Add(vertices.Count - 2);
                tris.Add(vertices.Count - 1);

                tris.Add(vertices.Count - 8);
                tris.Add(vertices.Count - 7);
                tris.Add(vertices.Count - 3);

                tris.Add(vertices.Count - 3);
                tris.Add(vertices.Count - 4);
                tris.Add(vertices.Count - 8);
            }
        }

        float curtime = preciseTime;

        float lastP = 0;

        List<LaneStep> steps = new List<LaneStep>();

        for (int a = 0; a < lane.LaneSteps.Count; a++)
            steps.Add((LaneStep)lane.LaneSteps[a].Get(this.pos));

        for (int a = 0; a < steps.Count; a++)
        {
            LaneStep step = steps[a];

            float time = TargetSong.Timing.ToSeconds(step.Offset);
            Vector3 start = step.StartPos;
            Vector3 end = step.EndPos;
            float p = 0;
            if (preciseTime > time)
            {
                if (a >= steps.Count - 1)
                {
                    lastP = 1;
                    continue;
                }
                LaneStep next = steps[a + 1];
                float nexttime = TargetSong.Timing.ToSeconds(next.Offset);
                if (preciseTime > nexttime)
                {
                    lastP = 1;
                    continue;
                }
                p = (curtime - time) / (nexttime - time);
                // Debug.Log("P " + a + " " + p);
                start = new Vector2(Mathf.LerpUnclamped(step.StartPos.x, next.StartPos.x, Ease.Get(p, next.StartEaseX, next.StartEaseXMode)),
                    Mathf.LerpUnclamped(step.StartPos.y, next.StartPos.y, Ease.Get(p, next.StartEaseY, next.StartEaseYMode)));
                end = new Vector2(Mathf.LerpUnclamped(step.EndPos.x, next.EndPos.x, Ease.Get(p, next.EndEaseX, next.EndEaseXMode)),
                    Mathf.LerpUnclamped(step.EndPos.y, next.EndPos.y, Ease.Get(p, next.EndEaseY, next.EndEaseYMode)));
            }

            float lPos = pos;
            pos += step.Speed * ScrollSpeed * (Mathf.Max(time, preciseTime) - curtime);
            curtime = Mathf.Max(time, preciseTime);
            if (a == 0)
            {
                AddStep(new Vector3(start.x, start.y, pos), new Vector3(end.x, end.y, pos));
            }
            else
            {
                LaneStep prev = steps[a - 1];
                if (lastP >= 1 || step.IsLinear)
                {
                    AddStep(new Vector3(start.x, start.y, pos), new Vector3(end.x, end.y, pos));
                }
                else
                {
                    // Debug.Log("T " + step.StartEaseX + " " + p);
                    for (float x = lastP; x <= 1; x = Mathf.Floor(x * 16 + 1.01f) / 16)
                    {
                        float cPos = Mathf.LerpUnclamped(lPos, pos, (x - lastP) / (1 - lastP));
                        start = new Vector3(Mathf.LerpUnclamped(prev.StartPos.x, step.StartPos.x, Ease.Get(x, step.StartEaseX, step.StartEaseXMode)),
                            Mathf.LerpUnclamped(prev.StartPos.y, step.StartPos.y, Ease.Get(x, step.StartEaseY, step.StartEaseYMode)), cPos);
                        end = new Vector3(Mathf.LerpUnclamped(prev.EndPos.x, step.EndPos.x, Ease.Get(x, step.EndEaseX, step.EndEaseXMode)),
                            Mathf.LerpUnclamped(prev.EndPos.y, step.EndPos.y, Ease.Get(x, step.EndEaseY, step.EndEaseYMode)), cPos);
                        AddStep(start, end);
                    }
                }
            }

            lastP = p;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public Mesh MakeJudgeMesh(Lane lane)
    {
        throw new NotImplementedException();
    }

    public Mesh MakeHitMesh(HitObject hit, Lane lane, out Vector3 startPos, out Vector3 endPos)
    {
        LaneStep step = lane.GetLaneStep(hit.Offset, pos, TargetSong.Timing);
        float len = Mathf.Max(hit.Length, .2f / Vector3.Distance(step.StartPos, step.EndPos));
        startPos = Vector3.LerpUnclamped(step.StartPos, step.EndPos, hit.Position) + Vector3.forward * (step.Offset * (ScrollSpeed - 1));
        endPos = Vector3.LerpUnclamped(step.StartPos, step.EndPos, hit.Position + len) + Vector3.forward * (step.Offset * (ScrollSpeed - 1));
        if (Mathf.Abs(step.Offset) > 5) return null;

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        void AddStep(Vector3 start, Vector3 end, bool addTris = true)
        {

            vertices.Add(start);
            vertices.Add(end);
            vertices.Add(start);
            vertices.Add(end);

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);

            if (addTris && vertices.Count >= 8)
            {
                tris.Add(vertices.Count - 8);
                tris.Add(vertices.Count - 7);
                tris.Add(vertices.Count - 3);

                tris.Add(vertices.Count - 3);
                tris.Add(vertices.Count - 4);
                tris.Add(vertices.Count - 8);
            }
        }
        float angle = Vector2.SignedAngle(step.EndPos - step.StartPos, Vector2.left);
        Vector3 afwd = Quaternion.Euler(0, 0, -angle) * Vector3.left;
        Vector3 fwd = Vector3.forward * step.Offset;
        if (hit.Type == HitObject.HitType.Normal)
        {
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = Quaternion.Euler(0, 0, -angle)
                    * new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .2f;
                AddStep((Vector3)startPos + afwd * .2f + ofs + fwd, (Vector3)endPos - afwd * .2f + ofs + fwd);
            }
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = Quaternion.Euler(0, 0, -angle)
                    * new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .2f;
                AddStep((Vector3)startPos + ofs + fwd, (Vector3)startPos + afwd * .1f + ofs + fwd, angle != 45);
            }
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = Quaternion.Euler(0, 0, -angle)
                    * new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .2f;
                AddStep((Vector3)endPos - afwd * .1f + ofs + fwd, (Vector3)endPos + ofs + fwd, angle != 45);
            }
        }
        else if (hit.Type == HitObject.HitType.Catch)
        {
            /*vertices.Add((Vector3)startPos + fwd);
            uvs.Add(Vector2.zero);
            vertices.Add((Vector3)endPos + fwd);
            uvs.Add(Vector2.zero);
            for (float ang = 45; ang <= 405; ang += 90) 
            {
                Vector3 ofs = Quaternion.Euler(0, 0, -angle) 
                    * new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)) 
                    * .3f;
                vertices.Add((Vector3)(startPos + endPos) / 2 + ofs + fwd);
                uvs.Add(Vector2.zero);
            }
            for (int a = 0; a < 4; a++) 
            {
                tris.Add(0);
                tris.Add(2 + (a % 4));
                tris.Add(2 + ((a + 1) % 4));
                tris.Add(1);
                tris.Add(2 + ((a + 1) % 4));
                tris.Add(2 + (a % 4));
            }*/
            for (float ang = 45; ang <= 405; ang += 90)
            {
                Vector3 ofs = Quaternion.Euler(0, 0, -angle)
                    * new Vector3(0, Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad))
                    * .12f;
                AddStep((Vector3)startPos + ofs + fwd, (Vector3)endPos + ofs + fwd);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public Mesh MakeHoldMesh(HitObject hit, Lane lane)
    {
        if (this.pos >= hit.Offset + hit.HoldLength) return null;

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        void AddStep(Vector3 start, Vector3 end)
        {

            vertices.Add(start);
            vertices.Add(end);
            vertices.Add(start);
            vertices.Add(end);

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);

            if (vertices.Count >= 8)
            {
                tris.Add(vertices.Count - 1);
                tris.Add(vertices.Count - 5);
                tris.Add(vertices.Count - 6);

                tris.Add(vertices.Count - 6);
                tris.Add(vertices.Count - 2);
                tris.Add(vertices.Count - 1);

                tris.Add(vertices.Count - 8);
                tris.Add(vertices.Count - 7);
                tris.Add(vertices.Count - 3);

                tris.Add(vertices.Count - 3);
                tris.Add(vertices.Count - 4);
                tris.Add(vertices.Count - 8);
            }
        }

        float startP = TargetSong.Timing.ToSeconds(hit.Offset);
        float endP = TargetSong.Timing.ToSeconds(hit.Offset + hit.HoldLength);

        float curtime = preciseTime;

        List<LaneStep> steps = new List<LaneStep>();

        for (int a = 0; a < lane.LaneSteps.Count; a++)
            steps.Add((LaneStep)lane.LaneSteps[a].Get(this.pos));


        float p = Mathf.Max(TargetSong.Timing.ToSeconds(steps[0].Offset) - curtime, 0) * steps[0].Speed * ScrollSpeed;

        for (int a = 0; a < steps.Count - 1; a++)
        {
            LaneStep step = steps[a];
            LaneStep next = steps[a + 1];

            float sTime = TargetSong.Timing.ToSeconds(step.Offset);
            float nTime = TargetSong.Timing.ToSeconds(next.Offset);
            Vector3 start = step.StartPos;
            Vector3 end = step.EndPos;
            float startPos = (Math.Max(curtime, startP) - sTime) / (nTime - sTime);
            float endPos = (endP - sTime) / (nTime - sTime);

            float nextP = p + Mathf.Max(nTime - Mathf.Max(sTime, curtime), 0) * step.Speed * ScrollSpeed;

            if (startPos >= 1)
            {
                curtime = Mathf.Max(nTime, curtime);
                p = nextP;
                continue;
            }


            if (vertices.Count == 0)
            {
                if (next.IsLinear)
                {
                    start = Vector2.LerpUnclamped(step.StartPos, next.StartPos, startPos);
                    end = Vector2.LerpUnclamped(step.EndPos, next.EndPos, startPos);
                }
                else
                {
                    start = new Vector3(Mathf.LerpUnclamped(step.StartPos.x, next.StartPos.x, Ease.Get(Mathf.Clamp01(startPos), next.StartEaseX, next.StartEaseXMode)),
                        Mathf.LerpUnclamped(step.StartPos.y, next.StartPos.y, Ease.Get(Mathf.Clamp01(startPos), next.StartEaseY, next.StartEaseYMode)));
                    end = new Vector3(Mathf.LerpUnclamped(step.EndPos.x, next.EndPos.x, Ease.Get(Mathf.Clamp01(startPos), next.EndEaseX, next.EndEaseXMode)),
                        Mathf.LerpUnclamped(step.EndPos.y, next.EndPos.y, Ease.Get(Mathf.Clamp01(startPos), next.EndEaseY, next.EndEaseYMode)));
                }
                Vector2 s = Vector2.LerpUnclamped(start, end, hit.Position);
                Vector2 e = Vector2.LerpUnclamped(start, end, hit.Position + hit.Length);
                float pp = p + ((nTime - sTime) * startPos - Math.Max(curtime - sTime, 0)) * step.Speed * ScrollSpeed;
                AddStep(new Vector3(s.x, s.y, pp), new Vector3(e.x, e.y, pp));
            }
            {
                if (next.IsLinear)
                {
                    start = Vector2.LerpUnclamped(step.StartPos, next.StartPos, endPos);
                    end = Vector2.LerpUnclamped(step.EndPos, next.EndPos, endPos);
                    Vector2 s = Vector2.LerpUnclamped(start, end, hit.Position);
                    Vector2 e = Vector2.LerpUnclamped(start, end, hit.Position + hit.Length);
                    float pp = p + ((nTime - sTime) * endPos - Math.Max(curtime - sTime, 0)) * step.Speed * ScrollSpeed;
                    AddStep(new Vector3(s.x, s.y, pp), new Vector3(e.x, e.y, pp));
                }
                else
                {

                    void Add(float pos)
                    {
                        start = new Vector3(Mathf.LerpUnclamped(step.StartPos.x, next.StartPos.x, Ease.Get(pos, next.StartEaseX, next.StartEaseXMode)),
                            Mathf.LerpUnclamped(step.StartPos.y, next.StartPos.y, Ease.Get(pos, next.StartEaseY, next.StartEaseYMode)));
                        end = new Vector3(Mathf.LerpUnclamped(step.EndPos.x, next.EndPos.x, Ease.Get(pos, next.EndEaseX, next.EndEaseXMode)),
                            Mathf.LerpUnclamped(step.EndPos.y, next.EndPos.y, Ease.Get(pos, next.EndEaseY, next.EndEaseYMode)));
                        Vector2 s = Vector2.LerpUnclamped(start, end, hit.Position);
                        Vector2 e = Vector2.LerpUnclamped(start, end, hit.Position + hit.Length);
                        float pp = p + ((nTime - sTime) * pos - Math.Max(curtime - sTime, 0)) * step.Speed * ScrollSpeed;
                        AddStep(new Vector3(s.x, s.y, pp), new Vector3(e.x, e.y, pp));
                    }
                    for (float x = Mathf.Floor(Mathf.Clamp01(startPos) * 16 + 1.01f) / 16; x < Mathf.Clamp01(endPos); x = Mathf.Floor(x * 16 + 1.01f) / 16) Add(x);
                    Add(Mathf.Clamp01(endPos));
                }
            }

            curtime = Mathf.Max(nTime, curtime);
            p = nextP;

            if (endPos <= 1) break;

        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public Mesh MakeFlickMesh(float direction, float size = .5f)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        if (direction >= 0)
        {
            float dir = (float)direction * Mathf.Deg2Rad;

            vertices.Add(new Vector3(Mathf.Sin(dir), Mathf.Cos(dir)) * 3 * size);
            vertices.Add(new Vector3(Mathf.Cos(dir), -Mathf.Sin(dir)) * size);
            vertices.Add(new Vector3(-Mathf.Sin(dir), -Mathf.Cos(dir)) * size);
            vertices.Add(new Vector3(-Mathf.Cos(dir), Mathf.Sin(dir)) * size);

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);

            tris.Add(0);
            tris.Add(2);
            tris.Add(3);

            tris.Add(0);
            tris.Add(1);
            tris.Add(2);
        }
        else
        {
            vertices.Add(new Vector3(0, 2 * size));
            vertices.Add(new Vector3(1 * size, 0));
            vertices.Add(new Vector3(0, -1 * size));
            vertices.Add(new Vector3(0, -2 * size));
            vertices.Add(new Vector3(-1 * size, 0));
            vertices.Add(new Vector3(0, 1 * size));

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);

            tris.Add(0);
            tris.Add(1);
            tris.Add(2);

            tris.Add(3);
            tris.Add(4);
            tris.Add(5);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    #endregion

    /////////////////////
    #region Main GUI Loop
    /////////////////////

    int NormalCount, CatchCount;

    string GizmoMode = "";
    Rect startRect, midRect, endRect;
    long delta, now, pass;
    string strain;

    Vector3? gizmoAnchor, gizmoAnchorDrag;

    Color backgroundColor, interfaceColor;

    public void OnGUI()
    {
        long strainNow = DateTime.Now.Ticks;

        CharterSettings.InitSettings();
        if (!CurrentCamera)
        {
            CurrentCamera = new GameObject("Charter Camera").AddComponent<Camera>();
            CurrentCamera.clearFlags = CameraClearFlags.SolidColor;
            CurrentCamera.targetDisplay = 8;
            CurrentCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
        if (!CurrentAudioSource)
        {
            CurrentAudioSource = new GameObject("Charter Audio").AddComponent<AudioSource>();
            CurrentAudioSource.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
        if (!MetronomeSound)
        {
            MetronomeSound = Resources.Load<AudioClip>("Sounds/Metronome");
        }
        if (!NormalHitSound)
        {
            NormalHitSound = Resources.Load<AudioClip>("Sounds/Normal Hit");
        }
        if (!CatchHitSound)
        {
            CatchHitSound = Resources.Load<AudioClip>("Sounds/Catch Hit");
        }

        width = position.width;
        height = position.height;

        if (TargetSong)
        {
            preciseTime = CurrentAudioSource.timeSamples / (float)TargetSong.Clip.frequency;
            pos = TargetSong.Timing.ToBeat(preciseTime);
            dec = Mathf.Floor((pos % 1) * 1000);
            beat = Mathf.Floor(TargetSong.Timing.ToDividedBeat(preciseTime));
            bar = Mathf.Floor(TargetSong.Timing.ToBar(preciseTime));

            min = Mathf.Floor(preciseTime / 60);
            sec = Mathf.Floor(preciseTime % 60);
            ms = Mathf.Floor((preciseTime % 1) * 1000);

            if ((TargetThing is PlayableSong && TargetThing != (object)TargetSong) ||
                (TargetThing is Chart && TargetThing != (object)TargetChart?.Data))
                TargetThing = null;

            if (TargetChartMeta != null && TargetSong.Charts.IndexOf(TargetChartMeta) < 0)
            {
                TargetChartMeta = TargetSong.Charts.Find(x => x.Target == TargetChartMeta.Target);
                if (TargetChartMeta == null) TargetChart = null;
            }

            if (TargetChart == null || TargetChart.Data.Lanes.IndexOf(TargetLane) < 0) TargetLane = null;

            if (TargetThing == null || !(TargetThing is IStoryboardable) ||
                ((IStoryboardable)TargetThing).Storyboard.Timestamps.IndexOf(TargetTimestamp) < 0)
                TargetTimestamp = null;

            if (TargetSong.ChartsOld != null && TargetSong.ChartsOld.Count > 0)
                extrasmode = "migrate_charts";

            Rect bound = new Rect(45, 35, width - 320, height - 222);
            if (bound.width / bound.height > 3 / 2f)
            {
                float width = (bound.height * 3 / 2);
                bound.x = bound.x + (bound.width - width) / 2;
                bound.width = width;
            }
            else
            {
                float height = (bound.width / 3 * 2);
                bound.y = bound.y + (bound.height - height) / 2;
                bound.height = height;
            }

            float camLeft = (bound.center.x - (width - bound.center.x));
            float camRatio = (bound.height / (height - 184));

            int ncount = 0, ccount = 0;

            if (TargetChartMeta != null && TargetChart != null)
            {
                Chart chart = (Chart)TargetChart.Data.Get(pos);
                Pallete pal = (Pallete)chart.Pallete.Get(pos);

                CurrentCamera.transform.position = chart.CameraPivot;
                CurrentCamera.transform.eulerAngles = chart.CameraRotation;
                CurrentCamera.transform.Translate(Vector3.back * 10);
                CurrentCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;
                backgroundColor = RenderSettings.fogColor = CurrentCamera.backgroundColor = pal.BackgroundColor;
                interfaceColor = pal.BackgroundColor.grayscale > .5f ? Color.black : Color.white;
                CurrentCamera.Render();

                for (int i = 0; i < pal.LaneStyles.Count; i++)
                {
                    LaneStyle style = (LaneStyle)pal.LaneStyles[i].Get(pos);
                    if (LaneStyleManagers.Count <= i) LaneStyleManagers.Add(new LaneStyleManager(style));
                    else LaneStyleManagers[i].Update(style);
                }
                while (LaneStyleManagers.Count > pal.LaneStyles.Count)
                {
                    LaneStyleManagers[pal.LaneStyles.Count].Dispose();
                    LaneStyleManagers.RemoveAt(pal.LaneStyles.Count);
                }

                for (int i = 0; i < pal.HitStyles.Count; i++)
                {
                    HitStyle style = (HitStyle)pal.HitStyles[i].Get(pos);
                    if (HitStyleManagers.Count <= i) HitStyleManagers.Add(new HitStyleManager(style));
                    else HitStyleManagers[i].Update(style);
                }
                while (HitStyleManagers.Count > pal.HitStyles.Count)
                {
                    HitStyleManagers[pal.HitStyles.Count].Dispose();
                    HitStyleManagers.RemoveAt(pal.HitStyles.Count);
                }


                if (Manager == null)
                {
                    Manager = new ChartManager(TargetSong, TargetChart.Data, ScrollSpeed, preciseTime, pos);
                }
                if (Event.current.type == EventType.Repaint)
                {
                    Manager.CurrentSpeed = ScrollSpeed;
                    Manager.Update(preciseTime, pos);
                }

                foreach (LaneManager l in Manager.Lanes)
                {
                    Lane lane = l.CurrentLane;

                    if (Event.current.type == EventType.Repaint && lane.StyleIndex >= 0 && lane.StyleIndex < LaneStyleManagers.Count)
                    {
                        bool valid = Event.current.type == EventType.Repaint && lane.StyleIndex >= 0 && lane.StyleIndex < LaneStyleManagers.Count;
                        if (valid)
                        {
                            if (LaneStyleManagers[lane.StyleIndex].LaneMaterial)
                            {
                                Quaternion rot = Quaternion.Euler(lane.OffsetRotation);
                                Vector3 pos = lane.Offset + rot * Vector3.back * l.CurrentDistance;
                                Graphics.DrawMesh(l.CurrentMesh, pos, rot, LaneStyleManagers[lane.StyleIndex].LaneMaterial, 0, CurrentCamera);
                            }
                        }
                    }
                    foreach (HitObjectManager h in l.Objects)
                    {
                        HitObject hit = h.CurrentHit;
                        bool valid = Event.current.type == EventType.Repaint && hit.StyleIndex >= 0 && hit.StyleIndex < HitStyleManagers.Count;
                        if (hit.Offset > pos)
                        {
                            if (valid)
                            {
                                Material mat = HitStyleManagers[hit.StyleIndex].NormalMaterial;
                                if (hit.Type == HitObject.HitType.Catch) mat = HitStyleManagers[hit.StyleIndex].CatchMaterial;
                                if (mat)
                                {
                                    if (h.CurrentMesh != null)
                                    {
                                        Graphics.DrawMesh(h.CurrentMesh, h.Position, h.Rotation, mat, 0, CurrentCamera);
                                    }
                                    if (hit.Flickable)
                                    {
                                        Mesh fmesh = MakeFlickMesh(hit.FlickDirection);
                                        Graphics.DrawMesh(fmesh, h.Position, CurrentCamera.transform.rotation, mat, 0, CurrentCamera);
                                        Meshes.Add(fmesh);
                                    }
                                }
                            }
                            if (hit.Type == HitObject.HitType.Catch) ccount++;
                            else ncount++;
                        }
                        if (hit.HoldLength > 0 && hit.Offset + hit.HoldLength > pos)
                        {
                            if (valid && HitStyleManagers[hit.StyleIndex].HoldTailMaterial)
                            {
                                Mesh mesh = MakeHoldMesh(hit, lane);
                                if (mesh != null)
                                {
                                    Graphics.DrawMesh(mesh, lane.Offset, Quaternion.Euler(lane.OffsetRotation), HitStyleManagers[hit.StyleIndex].HoldTailMaterial, 0, CurrentCamera);
                                    Meshes.Add(mesh);
                                }
                            }
                            if (hit.Type == HitObject.HitType.Catch) ccount++;
                            else ncount++;
                        }
                    }
                }
                EditorGUI.DrawRect(new Rect(0, 0, width, height), CurrentCamera.backgroundColor);
                Handles.DrawGizmos(CurrentCamera);
                Handles.DrawCamera(new Rect(0, 26, width + camLeft, height - 184), CurrentCamera);
                Handles.color = pal.InterfaceColor;
                Handles.DrawAAPolyLine(4, new Vector2(bound.x, bound.y), new Vector2(bound.x + bound.width, bound.y),
                    new Vector2(bound.x + bound.width, bound.y + bound.height), new Vector2(bound.x, bound.y + bound.height),
                    new Vector2(bound.x, bound.y));

                // Handles
                if (CurrentAudioSource.isPlaying)
                {
                    // Don't show anything in play mode
                }
                else
                {
                    wantsMouseMove = false;

                    if (TargetLane != null && TargetLane.LaneSteps.Count > 0 && TargetLane.LaneSteps[TargetLane.LaneSteps.Count - 1].Offset >= pos)
                    {
                        LaneManager man = Manager.Lanes[TargetChart.Data.Lanes.IndexOf(TargetLane)];

                        Vector3 startPos = man.StartPos;
                        Vector3 endPos = man.EndPos;

                        Vector3 startPosScr = WorldToScreen(startPos);
                        Vector3 endPosScr = WorldToScreen(endPos);

                        Handles.color = Color.black;
                        Handles.DrawLine(startPosScr + Vector3.back, endPosScr + Vector3.back, 3);
                        Handles.color = Color.white;
                        Handles.DrawLine(startPosScr, endPosScr, 1);
                    }


                    if (pickermode == "cursor")
                    {
                        if (TargetThing is Lane)
                        {
                            Lane thing = (Lane)TargetThing;
                            if (thing.LaneSteps.Count > 0 && thing.LaneSteps[thing.LaneSteps.Count - 1].Offset >= pos)
                            {
                                LaneManager man = Manager.Lanes[TargetChart.Data.Lanes.IndexOf(thing)];

                                Vector3 startPos = man.StartPos;
                                Vector3 endPos = man.EndPos;
                                Vector3 midPos = (startPos + endPos) / 2;

                                Vector2 startPosScr = WorldToScreen(startPos);
                                Vector2 endPosScr = WorldToScreen(endPos);
                                Vector2 midPosScr = WorldToScreen(midPos);
                                
                                Vector2 targetPosScr = 
                                    GizmoMode == "start" ? startPosScr : 
                                    GizmoMode == "mid" ? midPosScr : 
                                    GizmoMode == "end" ? endPosScr :
                                    Vector2.zero;
                                    
                                Vector2 fwd = Vector3.Normalize(endPosScr - startPosScr);
                                
                                bool startPosHover = Vector3.Distance(Event.current.mousePosition, startPosScr) < 6;
                                bool midPosHover = Vector3.Distance(Event.current.mousePosition, midPosScr) < 8;
                                bool endPosHover = Vector3.Distance(Event.current.mousePosition, endPosScr) < 6;

                                Handles.color = Color.black;
                                Handles.DrawLine((Vector3)startPosScr + Vector3.back, (Vector3)endPosScr + Vector3.back, 4);
                                Handles.color = Color.white;
                                Handles.DrawLine(startPosScr, endPosScr, 2);

                                if (midPosHover || GizmoMode == "mid") DrawGrid(midPosScr, Vector3.forward * midPos.z, Quaternion.identity);
                                else if (startPosHover || GizmoMode == "start") DrawGrid(startPosScr, man.CurrentLane.Offset, Quaternion.Euler(man.CurrentLane.OffsetRotation));
                                else if (endPosHover || GizmoMode == "end") DrawGrid(endPosScr, man.CurrentLane.Offset, Quaternion.Euler(man.CurrentLane.OffsetRotation));


                                Handles.color = Color.black;
                                if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosScr, Vector3.back, fwd, 360 * 59 / 4, 8);
                                if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosScr, Vector3.back, Vector3.up, 360, 10);
                                if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosScr, Vector3.back, fwd, 360 * 59 / 3, 9);

                                if (gizmoAnchor != null)
                                {
                                    Vector2 newPosScr = WorldToScreen(gizmoAnchor ?? Vector3.zero);
                                    Handles.color = Color.black;
                                    Handles.DrawLine((Vector3)targetPosScr + Vector3.back, (Vector3)newPosScr + Vector3.back, 3);
                                    Handles.DrawSolidArc(newPosScr, Vector3.back, fwd, 360, 4);
                                    Handles.color = Color.yellow;
                                    Handles.DrawLine(targetPosScr, newPosScr, 1);
                                    Handles.DrawSolidArc(newPosScr, Vector3.back, fwd, 360, 3);
                                }

                                Handles.color = startPosHover || GizmoMode == "start" ? Color.yellow : Color.white;
                                if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosScr, Vector3.back, fwd, 360 * 59 / 4, 6);
                                Handles.color = midPosHover || GizmoMode == "mid" ? Color.yellow : Color.white;
                                if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosScr, Vector3.back, Vector3.up, 360, 8);
                                Handles.color = endPosHover || GizmoMode == "end" ? Color.yellow : Color.white;
                                if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosScr, Vector3.back, fwd, 360 * 59 / 3, 6);

                                if (Event.current.type == EventType.MouseDown)
                                {
                                    if (startPosHover) 
                                    {
                                        GizmoMode = "start";
                                        gizmoAnchor = startPos;
                                    }
                                    else if (midPosHover) 
                                    {
                                        GizmoMode = "mid";
                                        gizmoAnchor = midPos;
                                    }
                                    else if (endPosHover) 
                                    {
                                        GizmoMode = "end";
                                        gizmoAnchor = endPos;
                                    }
                                    
                                    Repaint();
                                }
                                else if (Event.current.type == EventType.MouseDrag)
                                {
                                    Vector3 inv(Vector3 x) => Quaternion.Inverse(Quaternion.Euler(man.CurrentLane.OffsetRotation)) * (x - man.CurrentLane.Offset);

                                    if (GizmoMode != "")
                                    {
                                        Vector3? dragPos = GizmoMode == "mid" ? 
                                            RaycastScreenToPlane(Event.current.mousePosition, Vector3.forward * midPos.z, Quaternion.identity) :
                                            RaycastScreenToPlane(Event.current.mousePosition, man.CurrentLane.Offset, Quaternion.Euler(man.CurrentLane.OffsetRotation));
                                        if (dragPos != null)
                                        {
                                            if (GizmoMode == "start" || GizmoMode == "end") dragPos = inv((Vector3)dragPos);
                                            if (Event.current.shift) dragPos = new Vector3(Mathf.Round(dragPos?.x ?? 0), Mathf.Round(dragPos?.y ?? 0), Mathf.Round(dragPos?.z ?? 0));
                                        }
                                        else
                                        {
                                            dragPos = gizmoAnchor;
                                        }
                                    
                                        if (GizmoMode == "start") DoMove<CharterMoveLaneStartAction, Lane>(thing, (Vector3)dragPos - inv(startPos));
                                        else if (GizmoMode == "mid") DoMove<CharterMoveLaneAction, Lane>(thing, (Vector3)dragPos - midPos);
                                        else if (GizmoMode == "end") DoMove<CharterMoveLaneEndAction, Lane>(thing, (Vector3)dragPos - inv(endPos));
                                    }
                                    Repaint();
                                }
                                else if (Event.current.type == EventType.MouseUp)
                                {
                                    GizmoMode = "";
                                    gizmoAnchor = null;
                                    Repaint();
                                }

                                wantsMouseMove = true;
                                if (Event.current.type == EventType.MouseMove)
                                    Repaint();
                            }
                        }
                        if (TargetThing is LaneStep)
                        {
                            LaneStep thing = (LaneStep)TargetThing;
                            if (thing.Offset >= pos)
                            {
                                LaneManager lman = Manager.Lanes[TargetChart.Data.Lanes.IndexOf(TargetLane)];
                                LaneStepManager man = lman.Steps[TargetLane.LaneSteps.IndexOf(thing)];

                                Vector3 distOffset = Quaternion.Euler(lman.CurrentLane.OffsetRotation) * (Vector3.forward * (man.Distance - lman.CurrentDistance));

                                Vector3 startPos = Quaternion.Euler(lman.CurrentLane.OffsetRotation) * (Vector3)man.CurrentStep.StartPos  + distOffset + lman.CurrentLane.Offset;
                                Vector3 endPos = Quaternion.Euler(lman.CurrentLane.OffsetRotation) * (Vector3)man.CurrentStep.EndPos + distOffset + lman.CurrentLane.Offset;
                                Vector3 midPos = (startPos + endPos) / 2;

                                Vector2 startPosScr = WorldToScreen(startPos);
                                Vector2 endPosScr = WorldToScreen(endPos);
                                Vector2 midPosScr = WorldToScreen(midPos);
                                
                                Vector2 targetPosScr = 
                                    GizmoMode == "start" ? startPosScr : 
                                    GizmoMode == "mid" ? midPosScr : 
                                    GizmoMode == "end" ? endPosScr :
                                    Vector2.zero;
                                    
                                Vector2 fwd = Vector3.Normalize(endPosScr - startPosScr);
                                
                                bool startPosHover = Vector3.Distance(Event.current.mousePosition, startPosScr) < 6;
                                bool midPosHover = Vector3.Distance(Event.current.mousePosition, midPosScr) < 8;
                                bool endPosHover = Vector3.Distance(Event.current.mousePosition, endPosScr) < 6;

                                Handles.color = Color.black;
                                Handles.DrawLine((Vector3)startPosScr + Vector3.back, (Vector3)endPosScr + Vector3.back, 4);
                                Handles.color = Color.white;
                                Handles.DrawLine(startPosScr, endPosScr, 2);

                                if (midPosHover || GizmoMode == "mid") DrawGrid(midPosScr, lman.CurrentLane.Offset + distOffset, Quaternion.Euler(lman.CurrentLane.OffsetRotation));
                                else if (startPosHover || GizmoMode == "start") DrawGrid(startPosScr, lman.CurrentLane.Offset + distOffset, Quaternion.Euler(lman.CurrentLane.OffsetRotation));
                                else if (endPosHover || GizmoMode == "end") DrawGrid(endPosScr, lman.CurrentLane.Offset + distOffset, Quaternion.Euler(lman.CurrentLane.OffsetRotation));


                                Handles.color = Color.black;
                                if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosScr, Vector3.back, fwd, 360 * 59 / 4, 8);
                                if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosScr, Vector3.back, Vector3.up, 360, 10);
                                if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosScr, Vector3.back, fwd, 360 * 59 / 3, 9);

                                if (gizmoAnchor != null)
                                {
                                    Vector2 newPosScr = WorldToScreen(gizmoAnchor ?? Vector3.zero);
                                    Handles.color = Color.black;
                                    Handles.DrawLine((Vector3)targetPosScr + Vector3.back, (Vector3)newPosScr + Vector3.back, 3);
                                    Handles.DrawSolidArc(newPosScr, Vector3.back, fwd, 360, 4);
                                    Handles.color = Color.yellow;
                                    Handles.DrawLine(targetPosScr, newPosScr, 1);
                                    Handles.DrawSolidArc(newPosScr, Vector3.back, fwd, 360, 3);
                                }

                                Handles.color = startPosHover || GizmoMode == "start" ? Color.yellow : Color.white;
                                if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosScr, Vector3.back, fwd, 360 * 59 / 4, 6);
                                Handles.color = midPosHover || GizmoMode == "mid" ? Color.yellow : Color.white;
                                if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosScr, Vector3.back, Vector3.up, 360, 8);
                                Handles.color = endPosHover || GizmoMode == "end" ? Color.yellow : Color.white;
                                if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosScr, Vector3.back, fwd, 360 * 59 / 3, 6);

                                if (Event.current.type == EventType.MouseDown)
                                {
                                    if (startPosHover) 
                                    {
                                        GizmoMode = "start";
                                        gizmoAnchor = startPos;
                                    }
                                    else if (midPosHover) 
                                    {
                                        GizmoMode = "mid";
                                        gizmoAnchor = midPos;
                                    }
                                    else if (endPosHover) 
                                    {
                                        GizmoMode = "end";
                                        gizmoAnchor = endPos;
                                    }
                                    
                                    Repaint();
                                }
                                else if (Event.current.type == EventType.MouseDrag)
                                {
                                    Vector3 inv(Vector3 x) => Quaternion.Inverse(Quaternion.Euler(lman.CurrentLane.OffsetRotation)) * (x - lman.CurrentLane.Offset);

                                    if (GizmoMode != "")
                                    {
                                        Vector3? dragPos = RaycastScreenToPlane(Event.current.mousePosition, lman.CurrentLane.Offset + distOffset, Quaternion.Euler(lman.CurrentLane.OffsetRotation));
                                        if (dragPos != null)
                                        {
                                            dragPos = inv((Vector3)dragPos);
                                            if (Event.current.shift) dragPos = new Vector3(Mathf.Round(dragPos?.x ?? 0), Mathf.Round(dragPos?.y ?? 0), Mathf.Round(dragPos?.z ?? 0));
                                        }
                                        else
                                        {
                                            dragPos = gizmoAnchor;
                                        }
                                    
                                        if (GizmoMode == "start") DoMove<CharterMoveLaneStepStartAction, LaneStep>(thing, (Vector3)dragPos - inv(startPos));
                                        else if (GizmoMode == "mid") DoMove<CharterMoveLaneStepAction, LaneStep>(thing, (Vector3)dragPos - inv(midPos));
                                        else if (GizmoMode == "end") DoMove<CharterMoveLaneStepEndAction, LaneStep>(thing, (Vector3)dragPos - inv(endPos));
                                    }
                                    Repaint();
                                }
                                else if (Event.current.type == EventType.MouseUp)
                                {
                                    GizmoMode = "";
                                    gizmoAnchor = null;
                                    Repaint();
                                }

                                wantsMouseMove = true;
                                if (Event.current.type == EventType.MouseMove)
                                    Repaint();
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (ExternalChartMeta meta in TargetSong.Charts)
                {
                    if (meta.Target == "")
                    {
                        TargetSong.Charts.Remove(meta);
                        break;
                    }
                }
            }

            if (NormalCount > ncount && PlayHitsounds && CurrentAudioSource.isPlaying)
            {
                for (int a = 0; a < NormalCount - ncount; a++) CurrentAudioSource.PlayOneShot(NormalHitSound);
            }
            NormalCount = ncount;
            if (CatchCount > ccount && PlayHitsounds && CurrentAudioSource.isPlaying)
            {
                for (int a = 0; a < CatchCount - ccount; a++) CurrentAudioSource.PlayOneShot(CatchHitSound);
            }
            CatchCount = ccount;
        }
        else
        {
            EditorGUI.DrawRect(new Rect(0, 0, width, height), Color.black);
        }

        BeginWindows();
        if (TargetSong)
        {
            if (extrasmode != "")
            {
                Rect rect = new Rect();
                if (extrasmode == "migrate_charts") rect = new Rect(width / 2 - 200, height / 2 - 110, 400, 220);
                else if (extrasmode == "chart_create") rect = new Rect(width / 2 - 200, height / 2 - 110, 400, 220);
                else if (extrasmode == "chart_delete") rect = new Rect(width / 2 - 200, height / 2 - 60, 400, 120);
                else if (extrasmode == "play_options") rect = new Rect(width / 2 + 17, 30, 300, 120);
                else if (extrasmode == "timeline_options") rect = new Rect(width - 304, height - 144, 300, 120);

                GUIStyle exStyle = new GUIStyle("window");
                exStyle.focused = exStyle.normal;

                GUI.Window(10, rect, Extras, "", exStyle);
                if (Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))
                {
                    extrasmode = "";
                    Repaint();
                }
            }

            GUI.Button(new Rect(0, 0, width, 30), "", "toolbar");
            GUI.Button(new Rect(0, 6, width, 30), "", "toolbar");
            GUI.Window(1, new Rect(-2, -2, width + 4, 30), Toolbar, "", "toolbar");

            GUI.Window(2, new Rect(0, height - 184, width, 26), TimelineMode, "", new GUIStyle("button"));
            GUI.BringWindowToBack(2);
            GUI.Window(3, new Rect(-2, height - 158, width + 4, 160), Timeline, "");

            GUI.Window(4, new Rect(width - 270, 36, height - 204, height - 224), InspectMode, "", new GUIStyle("button"));
            GUI.BringWindowToBack(4);
            GUI.Window(5, new Rect(width - 245, 32, 240, height - 216), Inspector, "");

            GUI.Window(6, new Rect(5, 32, 32, height - 216), Picker, "");
        }
        else
        {
            TargetChartMeta = null;
            TargetChart = null;
            TargetThing = null;
            TargetTimestamp = null;
            GUI.Window(1, new Rect(width / 2 - 250, height / 2 - 110, 500, 220), CharterInit, "");
        }

        if (TutorialStage >= 0)
        {
            Rect rect = new Rect(
                width * TutorialPopupAnchor.x + TutorialPopupPosition.x - 170,
                height * TutorialPopupAnchor.y + TutorialPopupPosition.y - 90,
                340, 180);

            GUI.Window(20, rect, Tutorial, "");
            GUI.BringWindowToFront(20);
        }
        EndWindows();

        if (CurrentAudioSource.isPlaying)
        {
            if (currentBeat != Mathf.Floor(pos))
            {
                currentBeat = Mathf.Floor(pos);
                if (PlayMetronome) CurrentAudioSource.PlayOneShot(MetronomeSound);
            }
            Repaint();
        }

        if (Event.current.type == EventType.Repaint)
        {
            long n = DateTime.Now.Ticks;
            delta = n - now;
            now += delta;
            strain = ((n - strainNow) / 1e4).ToString("N0", invariant);
            pass = 1;
        }
        else
        {
            long n = DateTime.Now.Ticks;
            strain += " " + ((n - strainNow) / 1e4).ToString("N0", invariant);
            pass++;
        }

        foreach (Mesh mesh in Meshes) DestroyImmediate(mesh);
        Meshes = new List<Mesh>();

        HandleKeybinds();
    }

    #endregion

    ///////////////////
    #region Keybindings
    ///////////////////

    void HandleKeybinds()
    {
        if (Event.current.type == EventType.KeyDown)
        {
            // YandereDev intensifies

            if (Event.current == CharterSettings.Keybinds["General/Toggle Play/Pause"])
            {
                if (CurrentAudioSource.isPlaying)
                {
                    CurrentAudioSource.Pause();
                }
                else
                {
                    CurrentAudioSource.clip = TargetSong.Clip;
                    CurrentAudioSource.Play();
                }
            }
            else if (Event.current == CharterSettings.Keybinds["General/Play Chart in Player"])
            {
                OpenInPlayMode();
            }
            else if (Event.current == CharterSettings.Keybinds["File/Save"])
            {
                SaveSong();
            }
            else if (Event.current == CharterSettings.Keybinds["Edit/Undo"])
            {
                History.Undo();
            }
            else if (Event.current == CharterSettings.Keybinds["Edit/Redo"])
            {
                History.Redo();
            }
            else if (Event.current == CharterSettings.Keybinds["Edit/Copy"])
            {
                CopySelection();
            }
            else if (Event.current == CharterSettings.Keybinds["Edit/Paste"])
            {
                PasteSelection();
            }
            else if (Event.current == CharterSettings.Keybinds["Edit/Delete"])
            {
                DeleteSelection();
            }
            else if (Event.current == CharterSettings.Keybinds["Picker/Cursor"])
            {
                pickermode = "cursor";
            }
            else if (Event.current == CharterSettings.Keybinds["Picker/Select"])
            {
                pickermode = "select";
            }
            else if (Event.current == CharterSettings.Keybinds["Picker/Delete"])
            {
                pickermode = "delete";
            }
            else if (Event.current == CharterSettings.Keybinds["Picker/1st Item"])
            {
                if (timelineMode == "story") pickermode = "timestamp";
                else if (timelineMode == "timing") pickermode = "bpmstop";
                else if (timelineMode == "lane") pickermode = "lane";
                else if (timelineMode == "hit") pickermode = "hit_normal";
            }
            else if (Event.current == CharterSettings.Keybinds["Picker/2nd Item"])
            {
                if (timelineMode == "hit") pickermode = "hit_catch";
            }
            else if (Event.current == CharterSettings.Keybinds["Selection/Previous Item"])
            {
                if (TargetThing is Lane) TargetThing =
                    TargetChart.Data.Lanes[Math.Max(TargetChart.Data.Lanes.IndexOf((Lane)TargetThing) - 1, 0)];
                else if (TargetThing is HitObject) TargetThing =
                    TargetLane.Objects[Math.Max(TargetLane.Objects.IndexOf((HitObject)TargetThing) - 1, 0)];
            }
            else if (Event.current == CharterSettings.Keybinds["Selection/Next Item"])
            {
                if (TargetThing is Lane) TargetThing =
                    TargetChart.Data.Lanes[Math.Min(TargetChart.Data.Lanes.IndexOf((Lane)TargetThing) + 1, TargetChart.Data.Lanes.Count - 1)];
                else if (TargetThing is HitObject) TargetThing =
                    TargetLane.Objects[Math.Min(TargetLane.Objects.IndexOf((HitObject)TargetThing) + 1, TargetLane.Objects.Count - 1)];
            }
            else if (Event.current == CharterSettings.Keybinds["Selection/Previous Lane"])
            {
                if (TargetLane != null) TargetThing = TargetLane =
                    TargetChart.Data.Lanes[Math.Max(TargetChart.Data.Lanes.IndexOf(TargetLane) - 1, 0)];
            }
            else if (Event.current == CharterSettings.Keybinds["Selection/Next Lane"])
            {
                if (TargetLane != null) TargetThing = TargetLane =
                    TargetChart.Data.Lanes[Math.Min(TargetChart.Data.Lanes.IndexOf(TargetLane) + 1, TargetChart.Data.Lanes.Count - 1)];
            }
            else if (Event.current == CharterSettings.Keybinds["Misc./Show Keybindings"])
            {
                CharterSettings.Open(1);
            }
            Event.current.Use();
        }
    }

    #endregion

    /////////////////
    #region Functions
    /////////////////

    public void LoadChart(ExternalChartMeta data)
    {

        string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(TargetSong)).Replace('\\', '/') + "/" + data.Target;
        int resIndex = path.IndexOf("Resources/");
        if (resIndex >= 0) path = path.Substring(resIndex + 10);
        TargetChart = Resources.Load<ExternalChart>(path);

        History = new CharterHistory();
        Refresh();
    }

    public void CreateChart(ExternalChartMeta data)
    {
        string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(TargetSong)).Replace('\\', '/');
        path = AssetDatabase.GenerateUniqueAssetPath(path + "/" + data.Target);
        data.Target = Path.GetFileNameWithoutExtension(path);

        ExternalChart chart = ScriptableObject.CreateInstance<ExternalChart>();
        chart.Data = new Chart()
        {
            DifficultyIndex = data.DifficultyIndex,
            DifficultyName = data.DifficultyName,
            DifficultyLevel = data.DifficultyLevel,
            ChartConstant = data.ChartConstant,
        };

        AssetDatabase.CreateAsset(chart, path + ".asset");
        AssetDatabase.SaveAssets();

        TargetSong.Charts.Add(data);
        TargetChartMeta = data;
        TargetChart = chart;

        SaveSong();
    }

    public void DeleteChart(ExternalChartMeta data)
    {
        string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(TargetSong)).Replace('\\', '/') + "/" + data.Target + ".asset";

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();

        TargetSong.Charts.Remove(data);
        if (TargetChartMeta == data)
        {
            TargetChartMeta = null;
            TargetSong = null;
            History = new CharterHistory();
        }
        if (!(TargetThing is PlayableSong))
        {
            TargetThing = null;
        }

        SaveSong();
    }

    public void Refresh()
    {
        if (Manager != null) Manager.Dispose();
        Manager = new ChartManager(TargetSong, TargetChart.Data, ScrollSpeed, preciseTime, pos);
        if (LaneStyleManagers != null) foreach (LaneStyleManager style in LaneStyleManagers) style.Dispose();
        LaneStyleManagers = new List<LaneStyleManager>();
        if (HitStyleManagers != null) foreach (HitStyleManager style in HitStyleManagers) style.Dispose();
        HitStyleManagers = new List<HitStyleManager>();
    }

    static public string GetItemName(object item)
    {
        string name = item.ToString();
        if (item is Chart) name = "Chart";
        else if (item is BPMStop) name = "BPM Stop";
        else if (item is List<BPMStop>) name = ((IList)item).Count + " BPM Stops";
        else if (item is HitStyle) name = "Hit Style";
        else if (item is LaneStyle) name = "Lane Style";
        else if (item is Lane) name = "Lane";
        else if (item is List<Lane>) name = ((IList)item).Count + " Lanes";
        else if (item is LaneStep) name = "Lane Step";
        else if (item is List<LaneStep>) name = ((IList)item).Count + " Lane Steps";
        else if (item is HitObject) name = "Hit Object";
        else if (item is List<HitObject>) name = ((IList)item).Count + " Hit Objects";
        return name;
    }

    public void SaveSong()
    {
        EditorUtility.SetDirty(TargetSong);
        AssetDatabase.SaveAssetIfDirty(TargetSong);
        EditorUtility.SetDirty(TargetChart);
        AssetDatabase.SaveAssetIfDirty(TargetChart);
    }

    public void OpenInPlayMode()
    {
        if (TargetSong == null)
        {
            Debug.LogError("Please select a Chart first!");
            return;
        }
        ChartPlayer player = GameObject.FindObjectOfType<ChartPlayer>();
        if (!player)
        {
            Debug.LogError("Couldn't find any Chart Player in the scene. Please make sure the Scenes/Player scene in the Project window is opened.");
            return;
        }
        string path = AssetDatabase.GetAssetPath(TargetSong);
        path = path.Remove(path.Length - 6);
        int resIndex = path.IndexOf("Resources/");
        if (resIndex >= 0) path = path.Substring(resIndex + 10);
        player.SongPath = path;
        player.ChartPosition = TargetSong.Charts.FindIndex(x => x.Target == TargetChart.name);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        EditorApplication.isPlaying = true;
        EditorApplication.Beep();
    }

    public void HistoryAdd(IList list, object item)
    {
        if (item is IList) foreach (object i in (IList)item) list.Add(i);
        else list.Add(item);
        History.ActionsBehind.Add(new CharterAddAction()
        {
            Target = list,
            Item = item
        });
        History.ActionsAhead.Clear();
    }
    public void HistoryDelete(IList list, object item)
    {
        if (item is IList) foreach (object i in (IList)item) list.Remove(i);
        else list.Remove(item);
        History.ActionsBehind.Add(new CharterDeleteAction()
        {
            Target = list,
            Item = item
        });
        History.ActionsAhead.Clear();
    }

    public void CopySelection()
    {
        if (TargetThing is string) return;
        ClipboardThing = TargetTimestamp ?? TargetThing;
    }

    public void CutSelection()
    {
        PasteSelection();
        DeleteSelection();
    }

    public void PasteSelection()
    {
        if (ClipboardThing is BPMStop || ClipboardThing is List<BPMStop>)
        {
            if (timelineMode != "timing") return;

            List<BPMStop> list = (ClipboardThing is List<BPMStop> ? (List<BPMStop>)ClipboardThing :
                new List<BPMStop>(new[] { (BPMStop)ClipboardThing })).ConvertAll<BPMStop>(x => x.DeepClone()); ;

            float offset = pos - list[0].Offset;

            foreach (BPMStop item in list) item.Offset += offset;
            HistoryAdd(TargetSong.Timing.Stops, list);

            TargetSong.Timing.Stops.Sort((x, y) => x.Offset.CompareTo(y.Offset));
        }
        else if (ClipboardThing is Lane || ClipboardThing is List<Lane>)
        {
            if (timelineMode != "lane") return;

            List<Lane> list = (ClipboardThing is List<Lane> ? (List<Lane>)ClipboardThing :
                new List<Lane>(new[] { (Lane)ClipboardThing })).ConvertAll<Lane>(x => x.DeepClone()); ;

            float offset = pos - list[0].LaneSteps[0].Offset;

            foreach (Lane item in list)
            {
                foreach (LaneStep step in item.LaneSteps) step.Offset += offset;
                foreach (HitObject hit in item.Objects) hit.Offset += offset;
                foreach (Timestamp ts in item.Storyboard.Timestamps) ts.Time += offset;
            }
            HistoryAdd(TargetChart.Data.Lanes, list);

            TargetChart.Data.Lanes.Sort((x, y) => x.LaneSteps[0].Offset.CompareTo(y.LaneSteps[0].Offset));
        }
        else if (ClipboardThing is LaneStep || ClipboardThing is List<LaneStep>)
        {
            if (timelineMode != "step") return;

            List<LaneStep> list = (ClipboardThing is List<LaneStep> ? (List<LaneStep>)ClipboardThing :
                new List<LaneStep>(new[] { (LaneStep)ClipboardThing })).ConvertAll<LaneStep>(x => x.DeepClone()); ;

            float offset = pos - list[0].Offset;

            foreach (LaneStep item in list) item.Offset += offset;
            HistoryAdd(TargetLane.LaneSteps, list);

            TargetChart.Data.Lanes.Sort((x, y) => x.LaneSteps[0].Offset.CompareTo(y.LaneSteps[0].Offset));
        }
        else if (ClipboardThing is HitObject || ClipboardThing is List<HitObject>)
        {
            if (timelineMode != "hit" || TargetLane == null) return;

            List<HitObject> list = (ClipboardThing is List<HitObject> ? (List<HitObject>)ClipboardThing :
                new List<HitObject>(new[] { (HitObject)ClipboardThing })).ConvertAll<HitObject>(x => x.DeepClone());

            float offset = pos - list[0].Offset;

            foreach (HitObject item in list) item.Offset += offset;
            HistoryAdd(TargetLane.Objects, list);

            TargetLane.Objects.Sort((x, y) => x.Offset.CompareTo(y.Offset));
        }
        else if (ClipboardThing is Timestamp || ClipboardThing is List<Timestamp>)
        {
            if (timelineMode != "story" || !(TargetThing is IStoryboardable)) return;

            IStoryboardable sb = (IStoryboardable)TargetThing;

            List<Timestamp> list = (ClipboardThing is List<Timestamp> ? (List<Timestamp>)ClipboardThing :
                new List<Timestamp>(new[] { (Timestamp)ClipboardThing })).ConvertAll<Timestamp>(x => x.DeepClone());

            float offset = pos - list[0].Time;

            foreach (Timestamp item in list) item.Time += offset;
            HistoryAdd(sb.Storyboard.Timestamps, list);

            sb.Storyboard.Timestamps.Sort((x, y) => x.Time.CompareTo(y.Time));
        }
    }

    public Vector3 WorldToScreen(Vector3 pos)
    {
        pos = CurrentCamera.WorldToScreenPoint(pos);
        return new Vector3(pos.x, height - pos.y + 2);
    }

    public void DeleteSelection()
    {
        if (TargetTimestamp != null)
        {
            IStoryboardable sb = (IStoryboardable)TargetThing;
            HistoryDelete(sb.Storyboard.Timestamps, TargetTimestamp);
            TargetTimestamp = null;
        }
        else if (TargetThing is BPMStop || TargetThing is List<BPMStop>)
        {
            HistoryDelete(TargetSong.Timing.Stops, TargetThing);
            TargetThing = null;
        }
        else if (TargetThing is Lane || TargetThing is List<Lane>)
        {
            HistoryDelete(TargetChart.Data.Lanes, TargetThing);
            TargetThing = null;
        }
        else if (TargetThing is HitObject || TargetThing is List<HitObject>)
        {
            HistoryDelete(TargetLane.Objects, TargetThing);
            TargetThing = null;
        }
        else if (TargetThing is Timestamp || TargetThing is List<Timestamp>)
        {
            IStoryboardable sb = (IStoryboardable)TargetThing;
            HistoryDelete(sb.Storyboard.Timestamps, TargetThing);
            TargetThing = null;
        }
    }

    public Vector3? RaycastScreenToPlane(Vector3 pos, Vector3 center, Quaternion rotation, float radius = 4)
    {
        Plane plane = new Plane(rotation * Vector3.back, center);
        Ray ray = CurrentCamera.ScreenPointToRay(new Vector2(pos.x, height - pos.y));
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return null;
    }

    public void DrawGrid(Vector3 pos, Vector3 center, Quaternion rotation, float radius = 4)
    {
        Plane plane = new Plane(rotation * Vector3.back, center);
        Ray ray = CurrentCamera.ScreenPointToRay(new Vector2(pos.x, height - pos.y));
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 point = ray.GetPoint(enter);
            Vector2 normal = Quaternion.Inverse(rotation) * (point - center);

            for (float x = Mathf.Ceil(normal.x) - radius; x < normal.x + radius; x += 1f)
            {
                float thick = IsDivisible(x, 10) ? 2 : 1;
                float length = Mathf.Sqrt(1 - Mathf.Pow(Math.Abs(x - normal.x) / radius, 2)) * radius;
                Vector3 start = WorldToScreen(rotation * new Vector2(x, normal.y - length) + center) + Vector3.back * thick / 2;
                Vector3 end = WorldToScreen(rotation * new Vector2(x, normal.y + length) + center) + Vector3.back * thick / 2;
                Handles.color = new Color(interfaceColor.r, interfaceColor.g, interfaceColor.b, IsDivisible(x, 5) ? .5f : .25f);
                Handles.DrawLine(start, end, thick);
            }

            for (float y = Mathf.Ceil(normal.y) - radius; y < normal.y + radius; y += 1f)
            {
                float thick = IsDivisible(y, 10) ? 2 : 1;
                float length = Mathf.Sqrt(1 - Mathf.Pow(Math.Abs(y - normal.y) / radius, 2)) * radius;
                Vector3 start = WorldToScreen(rotation * new Vector2(normal.x - length, y) + center) + Vector3.back * thick / 2;
                Vector3 end = WorldToScreen(rotation * new Vector2(normal.x + length, y) + center) + Vector3.back * thick / 2;
                Handles.color = new Color(interfaceColor.r, interfaceColor.g, interfaceColor.b, IsDivisible(y, 5) ? .5f : .25f);
                Handles.DrawLine(start, end, thick);
            }
        }
        wantsMouseMove = true;
    }

    public void DoMove<TAction, TTarget>(TTarget item, Vector3 offset) where TAction : CharterMoveAction<TTarget>, new()
    {
        TAction action = null;
        if (History.ActionsBehind.Count > 0 && History.ActionsBehind[History.ActionsBehind.Count - 1] is TAction)
        {
            action = (TAction)History.ActionsBehind[History.ActionsBehind.Count - 1];
            if (!action.Item.Equals(item)) action = null;
        }

        if (action == null)
        {
            action = new TAction();
            action.Item = item;
            History.ActionsBehind.Add(action);
        }
        History.ActionsAhead.Clear();

        action.Undo();
        action.Offset += offset;
        action.Redo();
    }

    public void MigrateCharts()
    {
        foreach (Chart chart in TargetSong.ChartsOld)
        {
            string path = AssetDatabase.GetAssetPath(TargetSong);
            if (!Directory.Exists(path)) path = Path.GetDirectoryName(path);
            path = AssetDatabase.GenerateUniqueAssetPath(path + "/" + chart.DifficultyName + " " + chart.DifficultyLevel + ".asset");

            ExternalChartMeta meta = new ExternalChartMeta()
            {
                Target = Path.GetFileNameWithoutExtension(path),
                DifficultyName = chart.DifficultyName,
                DifficultyLevel = chart.DifficultyLevel,
                DifficultyIndex = chart.DifficultyIndex,
                ChartConstant = chart.ChartConstant,
            };

            ExternalChart exchart = ScriptableObject.CreateInstance<ExternalChart>();
            exchart.Data = chart;

            AssetDatabase.CreateAsset(exchart, path);
            TargetSong.Charts.Add(meta);
        }
        TargetSong.ChartsOld = null;
        AssetDatabase.SaveAssets();

        SaveSong();
    }

    #endregion

    ///////////////////
    #region Init Window
    ///////////////////

    string initName, initArtist;
    AudioClip initClip;

    public void CharterInit(int id)
    {

        GUIStyle title = new GUIStyle(EditorStyles.largeLabel);
        title.fontSize = 20;
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(0, 5, 500, 40), "Welcome to J.A.N.O.A.R.G. Charter Engine", title);

        EditorGUIUtility.labelWidth = 50;

        title = new GUIStyle("boldLabel");
        title.alignment = TextAnchor.MiddleCenter;

        GUI.Label(new Rect(20, 45, 210, 40), "Edit an existing playable song:", title);
        TargetSong = (PlayableSong)EditorGUI.ObjectField(new Rect(20, 80, 210, 20), TargetSong, typeof(PlayableSong), false);

        GUI.Label(new Rect(20, 111, 210, 40), "Stuck/First time user?", title);

        if (GUI.Button(new Rect(20, 146, 210, 20), "Open Interactive Tutorial (BETA)"))
        {
            TutorialStage = 0;
            TutorialPopupAnchor = TutorialSteps[0].PopupAnchor;
            TutorialPopupPosition = TutorialSteps[0].PopupPosition;
            TutorialLerp = 1;
        }

        GUI.Label(new Rect(270, 45, 210, 40), "or create a new one:", title);
        initName = EditorGUI.TextField(new Rect(270, 80, 210, 20), "Name", initName);
        initArtist = EditorGUI.TextField(new Rect(270, 102, 210, 20), "Artist", initArtist);
        initClip = (AudioClip)EditorGUI.ObjectField(new Rect(270, 124, 210, 20), "Clip", initClip, typeof(AudioClip), false);

        if (GUI.Button(new Rect(270, 146, 210, 20), "Create Playable Song"))
        {
            PlayableSong song = ScriptableObject.CreateInstance<PlayableSong>();
            song.SongName = initName;
            song.SongArtist = initArtist;
            song.Clip = initClip;

            string path = AssetDatabase.GetAssetPath(initClip);
            if (!Directory.Exists(path)) path = Path.GetDirectoryName(path);

            AssetDatabase.CreateAsset(song, AssetDatabase.GenerateUniqueAssetPath(path + "/" + initName + " - " + initArtist + ".asset"));
            AssetDatabase.SaveAssets();

            TargetSong = song;
        }

        GUIStyle label = new GUIStyle("miniLabel");
        label.alignment = TextAnchor.MiddleCenter;
        label.wordWrap = true;
        label.fontStyle = FontStyle.Italic;
        GUI.Label(new Rect(0, 190, 500, 20), "J.A.N.O.A.R.G.    © 2022-2022    by FFF40 Studios", label);
    }

    #endregion

    //////////////////////
    #region Toolbar Window
    //////////////////////

    public void Toolbar(int id)
    {
        // -------------------- Song selection

        TargetSong = (PlayableSong)EditorGUI.ObjectField(new Rect(155, 5, 21, 20), TargetSong, typeof(PlayableSong), false);

        if (GUI.Toggle(new Rect(27, 5, 130, 20), TargetThing == (object)TargetSong, TargetSong.SongName, "buttonLeft") && TargetThing != (object)TargetSong)
        {
            TargetThing = TargetSong;
        }

        // -------------------- Chart selection

        List<string> sels = new List<string>();
        foreach (ExternalChartMeta chart in TargetSong.Charts) sels.Add(chart.DifficultyName + " " + chart.DifficultyLevel);
        int sel = TargetChart != null ? EditorGUI.Popup(new Rect(309, 5, 18, 20), -1, sels.ToArray(), "buttonRight") :
            EditorGUI.Popup(new Rect(179, 5, 148, 20), -1, sels.ToArray(), "button");
        if (TargetChart == null)
        {
            GUIStyle style = new GUIStyle("label");
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(179, 5, 148, 20), "Select Chart...", style);
        }
        if (sel >= 0 && TargetChartMeta != TargetSong.Charts[sel])
        {
            TargetChartMeta = TargetSong.Charts[sel];
            LoadChart(TargetSong.Charts[sel]);
        }

        if (TargetChart != null && GUI.Toggle(new Rect(179, 5, 130, 20), TargetThing == (object)TargetChart.Data, TargetChart.Data.DifficultyName + " " + TargetChart.Data.DifficultyLevel, "buttonLeft") && TargetThing != (object)TargetChart.Data)
        {
            TargetThing = TargetChart.Data;
        }

        // -------------------- Player

        if (GUI.Button(new Rect(position.width / 2 - 20, 1, 40, 28), EditorGUIUtility.IconContent(CurrentAudioSource.isPlaying ? "PauseButton" : "PlayButton"), "buttonMid"))
        {
            if (CurrentAudioSource.isPlaying)
            {
                CurrentAudioSource.Pause();
            }
            else
            {
                CurrentAudioSource.clip = TargetSong.Clip;
                CurrentAudioSource.Play();
            }
        }

        // -------------------- Menu

        if (GUI.Button(new Rect(5, 5, 20, 20), EditorGUIUtility.IconContent("_Menu"),
            new GUIStyle("button") { padding = new RectOffset(0, 0, 0, 0) }))
        {
            GenericMenu menu = new GenericMenu();

            // -------------------- File
            if (TargetChartMeta != null && TargetChart != null)
                menu.AddItem(new GUIContent("File/Play Chart in Player " + CharterSettings.Keybinds["General/Play Chart in Player"].ToUnityHotkeyString()), false, OpenInPlayMode);
            else menu.AddDisabledItem(new GUIContent("File/Play Chart in Player " + CharterSettings.Keybinds["General/Play Chart in Player"].ToUnityHotkeyString()));

            menu.AddSeparator("File/");
            menu.AddItem(new GUIContent("File/Save " + CharterSettings.Keybinds["File/Save"].ToUnityHotkeyString()), false, SaveSong);

            menu.AddSeparator("File/");
            menu.AddItem(new GUIContent("File/Refresh"), false, Refresh);
            menu.AddItem(new GUIContent("File/Close Song"), false, () => TargetSong = null);

            // -------------------- Edit
            if (History.ActionsBehind.Count > 0)
                menu.AddItem(new GUIContent("Edit/Undo " + History.ActionsBehind[History.ActionsBehind.Count - 1].GetName() + " " + CharterSettings.Keybinds["Edit/Undo"].ToUnityHotkeyString()), false, () => History.Undo());
            else menu.AddDisabledItem(new GUIContent("Edit/Undo " + CharterSettings.Keybinds["Edit/Undo"].ToUnityHotkeyString()), false);
            if (History.ActionsAhead.Count > 0)
                menu.AddItem(new GUIContent("Edit/Redo " + History.ActionsAhead[History.ActionsAhead.Count - 1].GetName() + " " + CharterSettings.Keybinds["Edit/Redo"].ToUnityHotkeyString()), false, () => History.Redo());
            else menu.AddDisabledItem(new GUIContent("Edit/Redo " + CharterSettings.Keybinds["Edit/Redo"].ToUnityHotkeyString()), false);
            menu.AddItem(new GUIContent("Edit/Edit History"), false, () => inspectMode = "history");

            menu.AddSeparator("Edit/");
            if (TargetThing != null)
            {
                menu.AddItem(new GUIContent("Edit/Cut " + CharterSettings.Keybinds["Edit/Cut"].ToUnityHotkeyString()), false, CutSelection);
                menu.AddItem(new GUIContent("Edit/Copy " + CharterSettings.Keybinds["Edit/Copy"].ToUnityHotkeyString()), false, CopySelection);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Edit/Cut " + CharterSettings.Keybinds["Edit/Cut"].ToUnityHotkeyString()), false);
                menu.AddDisabledItem(new GUIContent("Edit/Copy " + CharterSettings.Keybinds["Edit/Copy"].ToUnityHotkeyString()), false);
            }
            if (ClipboardThing != null)
                menu.AddItem(new GUIContent("Edit/Paste " + GetItemName(ClipboardThing) + " " + CharterSettings.Keybinds["Edit/Paste"].ToUnityHotkeyString()), false, PasteSelection);
            else menu.AddDisabledItem(new GUIContent("Edit/Paste " + CharterSettings.Keybinds["Edit/Paste"].ToUnityHotkeyString()), false);

            // -------------------- Options
            menu.AddItem(new GUIContent("Options/Charter Settings"), false, CharterSettings.Open);
            menu.AddItem(new GUIContent("Options/Show Keybindings " + CharterSettings.Keybinds["Misc./Show Keybindings"].ToUnityHotkeyString()),
                false, () => CharterSettings.Open(1));

            // -------------------- Help
            menu.AddItem(new GUIContent("Help/Interactive Tutorial (closes song)"), false, () => { TargetSong = null; TutorialStage = 0; });

            menu.AddSeparator("Help/");
            menu.AddItem(new GUIContent("Help/Source Code on GitHub"), false, () => Application.OpenURL("https://github.com/ducdat0507/JANOARG"));
            menu.AddItem(new GUIContent("Help/FFF40 Studios Discord Server"), false, () => Application.OpenURL("https://discord.com/invite/vXJTPFQBHm"));

            menu.AddSeparator("Help/");
            menu.AddItem(new GUIContent("Help/Debug Stats"), false, () => inspectMode = "debug");

            menu.ShowAsContext();
        }

        // -------------------- Options

        if (GUI.Button(new Rect(position.width / 2 - 66, 5, 40, 20), new GUIContent("Save", "Save Chart")))
        {
            SaveSong();
        }
        if (GUI.Toggle(new Rect(width / 2 + 21, 5, 18, 20), extrasmode == "play_options", EditorGUIUtility.IconContent("icon dropdown"),
            new GUIStyle("buttonRight") { padding = new RectOffset(0, 0, 0, 0) }) ^ (extrasmode == "play_options"))
        {
            extrasmode = extrasmode == "play_options" ? "" : "play_options";
        }

        // -------------------- Timers

        GUIStyle counter = new GUIStyle("label");
        counter.alignment = TextAnchor.MiddleCenter;
        counter.fontStyle = FontStyle.Italic;
        counter.fontSize = 14;

        string ctText = SeparateUnits ? min.ToString("00", invariant) + ":" + sec.ToString("00", invariant) + "s" + ms.ToString("000", invariant) : preciseTime.ToString("0.000", invariant).Replace(".", "s");
        float counterX = width - 84;
        for (int a = ctText.Length - 1; a >= 0; a--)
        {
            GUI.Label(new Rect(counterX, 6, 15, 20), ctText[a].ToString(), counter);
            counterX -= 8;
        }

        counterX -= 10;

        ctText = SeparateUnits ? bar.ToString("0", invariant) + ":" + beat.ToString("00", invariant) + "b" + dec.ToString("000", invariant) : pos.ToString("0.000", invariant).Replace(".", "b");
        counter.fontSize = 18;
        for (int a = ctText.Length - 1; a >= 0; a--)
        {
            GUI.Label(new Rect(counterX, 5, 15, 20), ctText[a].ToString(), counter);
            counterX -= 10;
        }

        // -------------------- Metronome thing

        BPMStop bstop = TargetSong.Timing.GetStop(preciseTime, out int index);
        Color color = Color.black;
        if (index <= 0)
        {

        }
        else if (TargetSong.Timing.Stops[index - 1].BPM < bstop.BPM)
        {
            float time = 1 - (preciseTime - bstop.Offset);
            color = new Color(time * .8f, 0, 0);
        }
        else if (TargetSong.Timing.Stops[index - 1].BPM > bstop.BPM)
        {
            float time = 1 - (preciseTime - bstop.Offset);
            color = new Color(time * .1f, time * .1f, time);
        }

        EditorGUI.DrawRect(new Rect(width - 64, 6, 62, 18), color);
        if (beat >= 0)
        {
            EditorGUI.DrawRect(new Rect(width - 63 + beat * 60 / bstop.Signature, 7, 60 / bstop.Signature, 16), new Color(1, 1, 1, (1 - dec / 1000) * (1 - dec / 1000)));
        }


    }

    #endregion

    ////////////////////////
    #region Timeline Mode Window
    ////////////////////////

    public string timelineMode = "lane";

    public void TimelineMode(int id)
    {
        GUIStyle bgLabel = new GUIStyle("label");
        bgLabel.alignment = TextAnchor.MiddleCenter;
        bgLabel.normal.textColor = backgroundColor.grayscale < .5 ? Color.white : Color.black;


        string oldMode = timelineMode;

        if (GUI.Toggle(timelineMode == "story" ? new Rect(5, 3, 80, 25) : new Rect(5, 3, 80, 20), timelineMode == "story", "Storyboard", "button"))
            timelineMode = "story";

        GUI.Label(new Rect(85, 3, 12, 20), "|", bgLabel);

        if (GUI.Toggle(timelineMode == "timing" ? new Rect(97, 3, 80, 25) : new Rect(97, 3, 80, 20), timelineMode == "timing", "Timing", "buttonLeft"))
            timelineMode = "timing";
        if (GUI.Toggle(timelineMode == "lane" ? new Rect(178, 3, 80, 25) : new Rect(178, 3, 80, 20), timelineMode == "lane", "Lanes", "buttonRight"))
            timelineMode = "lane";

        if (TargetLane != null)
        {
            GUI.Label(new Rect(259, 3, 21, 20), "▶", bgLabel);

            if (GUI.Toggle(timelineMode == "step" ? new Rect(280, 3, 80, 25) : new Rect(280, 3, 80, 20), timelineMode == "step", "Steps", "buttonLeft"))
                timelineMode = "step";
            if (GUI.Toggle(timelineMode == "hit" ? new Rect(361, 3, 80, 25) : new Rect(361, 3, 80, 20), timelineMode == "hit", "Hits", "buttonRight"))
                timelineMode = "hit";
        }

        if (timelineMode != oldMode)
        {
            if (pickermode != "cursor" && pickermode != "select" && pickermode != "delete")
                pickermode = "cursor";
        }

        bool palleteSel = TargetChart != null && TargetThing == TargetChart.Data.Pallete;
        if (GUI.Toggle(palleteSel ? new Rect(width - 84, -2, 80, 25) : new Rect(width - 84, 3, 80, 20), palleteSel, "Palette", "buttonRight")
            && TargetChart != null && TargetThing != TargetChart.Data.Pallete)
            TargetThing = TargetChart.Data.Pallete;
    }

    #endregion

    ///////////////////////
    #region Timeline Window
    ///////////////////////

    public float seekStart, seekEnd;
    public float? selectStart, selectEnd;

    public string dragMode = "";
    public bool dragged = false;

    public int verSeek = 0;
    int mouseBtn = -1;
    int timelineSep = 2;

    public bool IsTargeted(object thing)
    {
        return TargetThing == thing || (TargetThing is IList && ((IList)TargetThing).Contains(thing));
    }

    bool IsDivisible(float a, float b)
    {
        return a == 0 || Math.Abs(a - Mathf.Round(a / b) * b) <= Math.Abs(a / 1e5);
    }

    public void Timeline(int id)
    {
        float seekLimitStart = TargetSong.Timing.ToBeat(0) - 4;
        float seekLimitEnd = TargetSong.Timing.ToBeat(TargetSong.Clip.length) + 4;
        float seekTime = TargetSong.Timing.ToBeat(preciseTime);
        if (seekEnd == seekStart && seekStart == 0)
        {
            seekEnd = Mathf.Min(width / 100, seekLimitEnd);
        }

        if (CurrentAudioSource.isPlaying && FollowSeekLine)
        {
            float seekRange = seekEnd - seekStart;
            seekStart = Mathf.Clamp(seekTime - seekRange / 2, seekLimitStart, seekLimitEnd - seekRange);
            seekEnd = seekStart + seekRange;
        }

        GUIStyle iconButton = new GUIStyle("button") { padding = new RectOffset(0, 0, 0, 0) };
        GUIStyle iconButtonMid = new GUIStyle("buttonMid") { padding = new RectOffset(0, 0, 0, 0) };
        GUIStyle iconButtonLeft = new GUIStyle("buttonLeft") { padding = new RectOffset(0, 0, 0, 0) };
        GUIStyle iconButtonRight = new GUIStyle("buttonRight") { padding = new RectOffset(0, 0, 0, 0) };
        GUIStyle leftButton = new GUIStyle("button") { alignment = TextAnchor.MiddleLeft };

        if (GUI.Button(new Rect(5, 136, 46, 19), "Undo", iconButtonLeft))
            History.Undo();
        if (GUI.Button(new Rect(52, 136, 44, 19), "Redo", iconButtonRight))
            History.Redo();

        if (GUI.Button(new Rect(99, 136, 46, 19), "Cut", iconButtonLeft))
            CutSelection();
        if (GUI.Button(new Rect(146, 136, 44, 19), "Copy", iconButtonMid))
            CopySelection();
        if (GUI.Button(new Rect(191, 136, 44, 19), "Paste", iconButtonRight))
            PasteSelection();

        if (GUI.Toggle(new Rect(width - 22, 136, 21, 19), extrasmode == "timeline_options", EditorGUIUtility.IconContent("icon dropdown"), iconButton) ^ (extrasmode == "timeline_options"))
            extrasmode = extrasmode == "timeline_options" ? "" : "timeline_options";

        GUI.Label(new Rect(width - 101, 136, 40, 19), "sep", leftButton);
        timelineSep = EditorGUI.IntField(new Rect(width - 71, 136, 46, 19), timelineSep);


        GUIStyle label = new GUIStyle("miniLabel");
        label.alignment = TextAnchor.MiddleCenter;

        float zoom = width / (seekEnd - seekStart);
        float sep = Mathf.Log(zoom / 20, timelineSep);
        float opa = ((sep % 1) + 1) % 1;
        sep = Mathf.Pow(timelineSep, Mathf.Floor(-sep));

        float[] list = new float[32];
        Color waveColor = EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .3f) : new Color(1, 1, 1, .3f);

        if (Event.current.type == EventType.Repaint && WaveformMode >= (CurrentAudioSource.isPlaying ? 2 : 1))
        {
            for (int a = 0; a < width; a++)
            {
                float time = TargetSong.Timing.ToSeconds(seekStart + a * (seekEnd - seekStart) / width);
                if (time < 0) continue;

                int samPos = (int)(time * TargetSong.Clip.frequency);
                if (samPos >= TargetSong.Clip.samples - 32) break;

                TargetSong.Clip.GetData(list, samPos);

                float height = 0;
                for (int i = 0; i < 32; i++) height += Math.Abs(list[i]);
                height /= 32;
                EditorGUI.DrawRect(new Rect(a + 2, 50 - height * 50, 1, height * 100), waveColor);
            }
        }

        EditorGUI.DrawRect(new Rect(0, 100, width + 4, 1), EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .5f) : new Color(1, 1, 1, .5f));
        for (float a = Mathf.Ceil(seekStart / sep) * sep; a < seekEnd; a += sep)
        {
            // Infinite loop handler
            if (a + sep == a) break;

            float pos = (a - seekStart) / (seekEnd - seekStart) * width;

            float op = .5f;
            if (!IsDivisible(a, sep * timelineSep)) op *= opa;

            float op2 = 1;
            if (!IsDivisible(a, sep * timelineSep * 4)) op2 = 0;
            else if (!IsDivisible(a, sep * timelineSep * 8)) op2 *= opa;

            if (IsDivisible(TargetSong.Timing.ToBar(0, a), 1))
            {
                EditorGUI.DrawRect(new Rect(pos + 1, 0, 1, 115), new Color(.6f, .6f, .4f, op));
            }
            else
            {
                EditorGUI.DrawRect(new Rect(pos + 1.5f, 0, 1, 100), new Color(.5f, .5f, .5f, .8f * op));
            }

            label.normal.textColor = new Color(label.normal.textColor.r, label.normal.textColor.g, label.normal.textColor.b, op2);
            if (op2 > 0) GUI.Label(new Rect(pos - 48, 100, 100, 15),
                SeparateUnits ? Mathf.Floor(TargetSong.Timing.ToBar(0, a)).ToString("0", invariant) + ":" +
                Mathf.Abs(TargetSong.Timing.ToDividedBeat(0, a)).ToString("00.###", invariant) : a.ToString("0.###", invariant), label);
            //GUI.Label(new Rect(pos - 48, 100, 100, 15), , label);
        }

        EditorGUI.DrawRect(new Rect(0, 115, width + 4, 18), EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .5f) : new Color(1, 1, 1, .5f));
        EditorGUI.MinMaxSlider(new Rect(2, 118, width, 15), ref seekStart, ref seekEnd, seekLimitStart, seekLimitEnd);

        EditorGUI.DrawRect(new Rect((seekTime - seekLimitStart) / (seekLimitEnd - seekLimitStart) * (width - 12) + 7, 116, 1, 14),
            EditorGUIUtility.isProSkin ? Color.white : Color.black);
        EditorGUI.DrawRect(new Rect((seekTime - seekLimitStart) / (seekLimitEnd - seekLimitStart) * (width - 12) + 7, 122, 1, 10),
            (EditorGUIUtility.isProSkin ^ (seekTime >= seekStart && seekTime < seekEnd)) ? new Color(.9f, .9f, .9f, .75f) : new Color(.2f, .2f, .2f, .75f));

        GUIStyle itemStyle = new GUIStyle("button");
        itemStyle.fontSize = 12;
        itemStyle.padding = new RectOffset(1, 0, 0, 0);

        if (TargetChartMeta != null && TargetChart != null)
        {

            List<float> Times = new List<float>();
            int AddTime(float pos, float size)
            {
                for (int a = 0; a < Times.Count; a++)
                {
                    if (pos > Times[a])
                    {
                        Times[a] = pos + size;
                        return a;
                    }
                }
                Times.Add(pos + size);
                return Times.Count - 1;
            }

            if (dragMode == "select" && selectStart != null && selectEnd != null)
            {
                float posStart = ((float)selectStart - seekStart) / (seekEnd - seekStart) * width;
                float posEnd = ((float)selectEnd - seekStart) / (seekEnd - seekStart) * width;
                if (posStart > posEnd)
                {
                    float tmp = posStart;
                    posStart = posEnd;
                    posEnd = tmp;
                }

                EditorGUI.DrawRect(new Rect(posStart + 2, 0, posEnd - posStart, 115), new Color(.5f, .5f, 1, .2f));
                Color color = EditorGUIUtility.isProSkin ? new Color(.5f, .5f, 1) : new Color(.3f, .3f, .6f);
                EditorGUI.DrawRect(new Rect(posStart + 1, 0, 2, 115), color);
                EditorGUI.DrawRect(new Rect(posEnd + 1, 0, 2, 115), color);
            }

            if (timelineMode == "story")
            {
                if (TargetThing is IStoryboardable)
                {
                    IStoryboardable thing = (IStoryboardable)TargetThing;
                    Storyboard sb = thing.Storyboard;

                    List<string> tst = new List<string>();
                    List<string> tso = new List<string>();
                    foreach (TimestampType type in (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null))
                    {
                        tso.Add(type.ID);
                        tst.Add(type.Name);
                        Times.Add(0);
                    }

                    GUIStyle left = new GUIStyle("label") { alignment = TextAnchor.MiddleLeft };
                    GUIStyle right = new GUIStyle("label") { alignment = TextAnchor.MiddleRight };

                    GUIStyle inv = new GUIStyle("label");
                    inv.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .4f) : new Color(1, 1, 1, .4f);

                    for (int a = verSeek; a < Math.Min(tst.Count, verSeek + 5); a++)
                    {
                        GUI.Label(new Rect(9, 4 + (a - verSeek) * 22, 120, 20), tst[a], inv);
                        GUI.Label(new Rect(8, 3 + (a - verSeek) * 22, 120, 20), tst[a]);
                    }

                    foreach (Timestamp ts in sb.Timestamps)
                    {
                        float a = ts.Time;
                        float pos = (a - seekStart) / (seekEnd - seekStart) * width;
                        float b = ts.Time + ts.Duration;
                        float pos2 = (b - seekStart) / (seekEnd - seekStart) * width;

                        float time = tso.IndexOf(ts.ID) - verSeek;
                        if (time < 0 || time >= 5) continue;

                        if (b > seekStart && a < seekEnd)
                        {
                            EditorGUI.DrawRect(new Rect(pos + 2, 3 + time * 22, pos2 - pos, 20), new Color(0, 1, 0, .2f));

                            float rpos = Mathf.Min(Mathf.Max((a - seekStart) / (seekEnd - seekStart) * width, 5), (b - seekStart) / (seekEnd - seekStart) * width);
                            if (TargetTimestamp == ts || IsTargeted(ts)) GUI.Label(new Rect(rpos - 2, 2 + time * 22, 8, 22), "", "button");
                            if (GUI.Button(new Rect(rpos - 1, 3 + time * 22, 6, 20), DeletingThing == ts ? "?" : "┃", itemStyle))
                            {

                                if (pickermode == "delete")
                                {

                                }
                                else
                                {
                                    TargetTimestamp = ts;
                                    DeletingThing = null;
                                }
                            }
                            if (!float.IsNaN(ts.From)) GUI.Label(new Rect(rpos + 6, 2 + time * 22, 60, 22), ts.From.ToString(invariant), left);

                            float epos = Mathf.Min(Mathf.Max(rpos, width - 8), (b - seekStart) / (seekEnd - seekStart) * width);
                            GUI.Label(new Rect(epos - 62, 2 + time * 22, 60, 22), ts.Target.ToString(invariant), right);
                        }
                    }
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(0, 0, width + 4, 115), EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .4f) : new Color(1, 1, 1, .4f));
                    GUIStyle center = new GUIStyle("label");
                    center.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(0, 0, width + 4, 115), TargetThing == null ? "Please select an object to start editing." :
                        (TargetThing is IList ? "Can not edit storyboard of multiple objects" : "This object is not storyboardable."), center);
                }
            }
            if (timelineMode == "timing")
            {
                foreach (BPMStop stop in TargetSong.Timing.Stops)
                {
                    float a = TargetSong.Timing.ToBeat(stop.Offset);
                    float pos = (a - seekStart) / (seekEnd - seekStart) * width;
                    int time = AddTime(pos, 61) - verSeek;
                    if (time < 0 || time >= 5) continue;
                    if (a > seekStart && a < seekEnd)
                    {
                        if (IsTargeted(stop)) GUI.Label(new Rect(pos - 30, 2 + time * 22, 62, 22), "", "button");
                        if (GUI.Button(new Rect(pos - 29, 3 + time * 22, 60, 20), DeletingThing == stop ? "?" : stop.BPM.ToString("F2", invariant), itemStyle))
                        {
                            if (pickermode == "delete")
                            {
                                if (DeletingThing == stop)
                                {
                                    HistoryDelete(TargetSong.Timing.Stops, stop);
                                    TargetThing = null;
                                    break;
                                }
                                else
                                {
                                    DeletingThing = stop;
                                }
                            }
                            else
                            {
                                TargetThing = stop;
                                DeletingThing = null;
                            }
                        }
                    }
                }
            }
            else if (timelineMode == "lane")
            {
                foreach (Lane lane in TargetChart.Data.Lanes)
                {
                    if (lane.LaneSteps.Count > 0)
                    {
                        float a = lane.LaneSteps[0].Offset;
                        float pos = (a - seekStart) / (seekEnd - seekStart) * width;
                        float b = lane.LaneSteps[lane.LaneSteps.Count - 1].Offset;
                        float pos2 = (b - seekStart) / (seekEnd - seekStart) * width;
                        int time = AddTime(pos, Mathf.Max(pos2 - pos + 14, 21)) - verSeek;
                        if (time < 0 || time >= 5) continue;
                        if (b > seekStart && a < seekEnd)
                        {
                            EditorGUI.DrawRect(new Rect(pos + 2, 3 + time * 22, pos2 - pos, 20), TargetLane == lane ? new Color(1, 1, 0, .2f) : new Color(0, 1, 0, .2f));
                        }
                        for (int x = 1; x < lane.LaneSteps.Count; x++)
                        {
                            float c = lane.LaneSteps[x].Offset;
                            if (c > seekStart && c < seekEnd)
                            {
                                float pos3 = (c - seekStart) / (seekEnd - seekStart) * width;
                                if (IsTargeted(lane.LaneSteps[x])) GUI.Label(new Rect(pos3 - 2, 2 + time * 22, 8, 22), "", "button");
                                if (GUI.Button(new Rect(pos3 - 1, 3 + time * 22, 6, 20), DeletingThing == lane.LaneSteps[x] ? "?" : "┃", itemStyle))
                                {
                                    if (pickermode == "delete")
                                    {
                                        if (DeletingThing == lane.LaneSteps[x])
                                        {
                                            HistoryDelete(lane.LaneSteps, lane.LaneSteps[x]);
                                            break;
                                        }
                                        else
                                        {
                                            DeletingThing = lane.LaneSteps[x];
                                        }
                                    }
                                    else
                                    {
                                        TargetLane = lane;
                                        TargetThing = lane.LaneSteps[x];
                                        DeletingThing = null;
                                    }
                                }
                            }
                        }
                        if (b > seekStart && a < seekEnd)
                        {
                            float rpos = (a - seekStart) / (seekEnd - seekStart) * width;
                            rpos = Math.Min(Math.Max(rpos, 13), Math.Max(rpos, (b - seekStart) / (seekEnd - seekStart) * width - 15));
                            if (IsTargeted(lane)) GUI.Label(new Rect(rpos - 10, 2 + time * 22, 22, 22), "", "button");
                            if (GUI.Button(new Rect(rpos - 9, 3 + time * 22, 20, 20), DeletingThing == lane ? "?" : "┃", itemStyle))
                            {
                                if (pickermode == "delete")
                                {
                                    if (DeletingThing == lane)
                                    {
                                        HistoryDelete(TargetChart.Data.Lanes, lane);
                                        TargetThing = TargetLane = null;
                                        break;
                                    }
                                    else
                                    {
                                        DeletingThing = lane;
                                    }
                                }
                                else
                                {
                                    TargetThing = TargetLane = lane;
                                    DeletingThing = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        HistoryDelete(TargetChart.Data.Lanes, lane);
                    }
                }
            }
            else if (timelineMode == "step")
            {
                if (TargetLane != null)
                {
                    float a = TargetLane.LaneSteps[0].Offset;
                    float b = TargetLane.LaneSteps[TargetLane.LaneSteps.Count - 1].Offset;
                    if (a > seekStart)
                    {
                        float pos = (a - seekStart) / (seekEnd - seekStart) * width;
                        EditorGUI.DrawRect(new Rect(0, 0, pos + 2, 115), new Color(0, 0, 0, .25f));
                    }
                    if (b < seekEnd)
                    {
                        float pos = (b - seekStart) / (seekEnd - seekStart) * width;
                        EditorGUI.DrawRect(new Rect(pos + 2, 0, width - pos + 2, 115), new Color(0, 0, 0, .25f));
                    }
                    foreach (LaneStep step in TargetLane.LaneSteps)
                    {
                        float x = step.Offset;
                        float pos = (x - seekStart) / (seekEnd - seekStart) * width;

                        if (step.Offset > seekStart && step.Offset < seekEnd)
                        {
                            if (IsTargeted(step)) GUI.Label(new Rect(pos - 2, 2, 8, 96), "", "button");
                            if (GUI.Button(new Rect(pos - 1, 3, 6, 94), DeletingThing == step ? "?" : "┃", itemStyle))
                            {
                                if (pickermode == "delete")
                                {
                                    if (DeletingThing == step)
                                    {
                                        HistoryDelete(TargetLane.LaneSteps, step);
                                        TargetThing = null;
                                        break;
                                    }
                                    else
                                    {
                                        DeletingThing = step;
                                    }
                                }
                                else
                                {
                                    TargetThing = step;
                                    DeletingThing = null;
                                }
                            }
                        }
                    }
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(0, 0, width + 4, 115), EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .4f) : new Color(1, 1, 1, .4f));
                    GUIStyle center = new GUIStyle("label");
                    center.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(0, 0, width + 4, 115), "Please select a lane to start editing.", center);
                }
            }
            else if (timelineMode == "hit")
            {
                if (TargetLane != null)
                {
                    float a = TargetLane.LaneSteps[0].Offset;
                    float b = TargetLane.LaneSteps[TargetLane.LaneSteps.Count - 1].Offset;
                    if (a > seekStart)
                    {
                        float pos = (a - seekStart) / (seekEnd - seekStart) * width;
                        EditorGUI.DrawRect(new Rect(0, 0, pos + 2, 115), new Color(0, 0, 0, .25f));
                    }
                    if (b < seekEnd)
                    {
                        float pos = (b - seekStart) / (seekEnd - seekStart) * width;
                        EditorGUI.DrawRect(new Rect(pos + 2, 0, width - pos + 2, 115), new Color(0, 0, 0, .25f));
                    }
                    foreach (HitObject hit in TargetLane.Objects)
                    {
                        float x = hit.Offset;
                        float pos = (x - seekStart) / (seekEnd - seekStart) * width;
                        float y = hit.Offset + hit.HoldLength;
                        float pos2 = (y - seekStart) / (seekEnd - seekStart) * width;

                        int time = AddTime(pos, Mathf.Max(pos2 - pos + 14, 21)) - verSeek;
                        if (time < 0 || time >= 5) continue;


                        if (x != y)
                        {
                            GUI.Label(new Rect(pos + 2, 3 + time * 22, pos2 - pos, 20), "", "button");
                        }
                        if (hit.Offset > seekStart && hit.Offset < seekEnd)
                        {
                            if (IsTargeted(hit)) GUI.Label(new Rect(pos - 10, 2 + time * 22, 22, 22), "", "button");
                            if (GUI.Button(new Rect(pos - 9, 3 + time * 22, 20, 20), DeletingThing == hit ? "?" : "┃", itemStyle))
                            {
                                if (pickermode == "delete")
                                {
                                    if (DeletingThing == hit)
                                    {
                                        HistoryDelete(TargetLane.Objects, hit);
                                        TargetThing = null;
                                        break;
                                    }
                                    else
                                    {
                                        DeletingThing = hit;
                                    }
                                }
                                else
                                {
                                    TargetThing = hit;
                                    DeletingThing = null;
                                }
                            }
                        }
                    }
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(0, 0, width + 4, 115), EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .4f) : new Color(1, 1, 1, .4f));
                    GUIStyle center = new GUIStyle("label");
                    center.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(0, 0, width + 4, 115), "Please select a lane to start editing.", center);
                }
            }

            if (Times.Count > 5)
            {
                verSeek = Mathf.RoundToInt(GUI.VerticalScrollbar(new Rect(width - 8, 0, 10, 115), verSeek, 4f / Times.Count, 0, Times.Count - 4));
            }
            if (Event.current.type == EventType.ScrollWheel)
            {
                Vector2 mPos = Event.current.mousePosition;
                if (mPos.y > 0 && mPos.y < 115)
                {
                    if (Event.current.shift)
                    {
                        verSeek = verSeek + Math.Sign(Event.current.delta.y);
                    }
                    else
                    {
                        float seekRange = seekEnd - seekStart;
                        seekStart = Mathf.Clamp(seekStart + sep * Event.current.delta.y, seekLimitStart, seekLimitEnd - seekRange);
                        seekEnd = seekStart + seekRange;
                    }
                    Repaint();
                }
            }
            verSeek = Mathf.Max(Mathf.Min(verSeek, Times.Count - 5), 0);
        }


        // Click events
        if (Event.current.type == EventType.MouseDown && mouseBtn < 0)
        {
            Vector2 mPos = Event.current.mousePosition;
            mouseBtn = Event.current.button;
            if (mPos.x < width - 10)
            {
                float sPos = mPos.x * (seekEnd - seekStart) / width + seekStart;
                if (mPos.y > 100 && mPos.y < 115)
                {
                    CurrentAudioSource.time = Mathf.Clamp(TargetSong.Timing.ToSeconds(sPos), 0, TargetSong.Clip.length - .0001f);
                    dragMode = "seek";
                    Repaint();
                }
                else if (mPos.y > 0 && mPos.y < 100)
                {
                    if (pickermode == "select" || mouseBtn == 1)
                    {
                        selectStart = sPos;
                        dragMode = "select";
                        Repaint();
                    }
                    else
                    {
                        selectStart = Mathf.Round(sPos / sep) * sep;
                        CurrentAudioSource.time = Mathf.Clamp(TargetSong.Timing.ToSeconds(Mathf.Round(sPos / sep) * sep), 0, TargetSong.Clip.length - .0001f);
                        dragMode = "seeksnap";
                        Repaint();
                    }
                }
            }
            dragged = false;
        }
        else if (Event.current.type == EventType.MouseDrag)
        {
            Vector2 mPos = Event.current.mousePosition;
            float sPos = mPos.x * (seekEnd - seekStart) / width + seekStart;
            if (dragMode == "select")
            {
                selectEnd = sPos;
                Repaint();
            }
            else if (dragMode == "seek")
            {
                CurrentAudioSource.time = Mathf.Clamp(TargetSong.Timing.ToSeconds(sPos), 0, TargetSong.Clip.length - .0001f);
                Repaint();
            }
            else if (dragMode == "seeksnap")
            {
                selectEnd = Mathf.Round(sPos / sep) * sep;
                CurrentAudioSource.time = Mathf.Clamp(TargetSong.Timing.ToSeconds(Mathf.Round(sPos / sep) * sep), 0, TargetSong.Clip.length - .0001f);
                Repaint();
            }

            if (mPos.x < 50)
            {
                float drift = Mathf.Min((50 - mPos.x) / 10000 * (seekEnd - seekStart), seekStart - seekLimitStart);
                seekStart -= drift;
                seekEnd -= drift;
                Repaint();
            }
            if (mPos.x > width - 50)
            {
                float drift = Mathf.Min((mPos.x - width + 50) / 10000 * (seekEnd - seekStart), seekLimitEnd - seekEnd);
                seekStart += drift;
                seekEnd += drift;
                Repaint();
            }
            dragged = true;
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == mouseBtn)
        {
            if (dragMode == "select")
            {
                if (selectEnd != null)
                {
                    if (selectStart > selectEnd)
                    {
                        float? tmp = selectStart;
                        selectStart = selectEnd;
                        selectEnd = tmp;
                    }

                    if (timelineMode == "story" && TargetThing is IStoryboardable)
                    {
                        List<Timestamp> sel = ((IStoryboardable)TargetThing).Storyboard.Timestamps.FindAll(x =>
                        {
                            return x.Time >= selectStart && x.Time <= selectEnd;
                        });
                        if (sel.Count == 1) TargetTimestamp = sel[0];
                        else if (sel.Count > 1) TargetThing = sel;
                    }
                    else if (timelineMode == "timing")
                    {
                        List<BPMStop> sel = TargetSong.Timing.Stops.FindAll(x =>
                        {
                            float rofs = TargetSong.Timing.ToBeat(x.Offset);
                            return rofs >= selectStart && rofs <= selectEnd;
                        });
                        if (sel.Count == 1) TargetThing = sel[0];
                        else if (sel.Count > 1) TargetThing = sel;
                    }
                    else if (timelineMode == "lane")
                    {
                        List<Lane> sel = TargetChart.Data.Lanes.FindAll(x =>
                        {
                            return x.LaneSteps[0].Offset >= selectStart && x.LaneSteps[0].Offset <= selectEnd;
                        });
                        if (sel.Count == 1) { TargetThing = TargetLane = sel[0]; }
                        else if (sel.Count > 1) { TargetThing = sel; TargetLane = null; }
                    }
                    else if (timelineMode == "step")
                    {
                        List<LaneStep> sel = TargetLane.LaneSteps.FindAll(x =>
                        {
                            return x.Offset >= selectStart && x.Offset <= selectEnd;
                        });
                        if (sel.Count == 1) TargetThing = sel[0];
                        else if (sel.Count > 1) TargetThing = sel;
                    }
                    else if (timelineMode == "hit")
                    {
                        List<HitObject> sel = TargetLane.Objects.FindAll(x =>
                        {
                            return x.Offset >= selectStart && x.Offset <= selectEnd;
                        });
                        if (sel.Count == 1) TargetThing = sel[0];
                        else if (sel.Count > 1) TargetThing = sel;
                    }
                }
                Repaint();
            }
            else if (!CurrentAudioSource.isPlaying)
            {
                if (dragged)
                {
                    Vector2 mPos = Event.current.mousePosition;
                    if (selectStart > selectEnd)
                    {
                        float? tmp = selectStart;
                        selectStart = selectEnd;
                        selectEnd = tmp;
                    }

                    if (dragMode == "seeksnap" && pickermode == "timestamp" && TargetThing is IStoryboardable)
                    {
                        IStoryboardable thing = (IStoryboardable)TargetThing;
                        TimestampType[] types = (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null);
                        int index = Mathf.FloorToInt(mPos.y / 22) + verSeek;
                        TimestampType type = types[Mathf.Clamp(index, 0, types.Length - 1)];

                        Timestamp ts = new Timestamp()
                        {
                            ID = type.ID,
                            Time = (float)selectStart,
                            Duration = (float)(selectEnd - selectStart)
                        };

                        HistoryAdd(thing.Storyboard.Timestamps, ts);
                        thing.Storyboard.Timestamps.Sort((x, y) => x.Time.CompareTo(y.Time));
                        TargetSong.Timing.Stops.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                        TargetTimestamp = ts;
                        Repaint();
                    }
                }
                else
                {
                    if (dragMode == "seeksnap" && pickermode == "bpmstop")
                    {
                        BPMStop stop = new BPMStop(TargetSong.Timing.GetStop(preciseTime, out _).BPM, Mathf.Round(preciseTime * 1000) / 1000);
                        HistoryAdd(TargetSong.Timing.Stops, stop);
                        TargetSong.Timing.Stops.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                        Repaint();
                    }
                    else if (dragMode == "seeksnap" && pickermode == "lane")
                    {
                        Lane lane = new Lane();
                        lane.Offset = new Vector3(0, -3);

                        LaneStep step = new LaneStep();
                        step.Offset = (float)(Math.Ceiling(pos * 1e5) / 1e5);
                        step.StartPos = new Vector2(-6, 0);
                        step.EndPos = new Vector2(6, 0);
                        lane.LaneSteps.Add(step);

                        LaneStep next = step.DeepClone();
                        next.Offset = step.Offset + 1;
                        lane.LaneSteps.Add(next);

                        HistoryAdd(TargetChart.Data.Lanes, lane);
                        TargetChart.Data.Lanes.Sort((x, y) => x.LaneSteps[0].Offset.CompareTo(y.LaneSteps[0].Offset));
                        TargetThing = TargetLane = lane;
                        Repaint();
                    }
                    else if (dragMode == "seeksnap" && pickermode == "step" && TargetLane != null)
                    {
                        float p = (float)(Math.Ceiling(pos * 1e5) / 1e5);
                        LaneStep cur = TargetLane.GetLaneStep(CurrentAudioSource.time, CurrentAudioSource.time, TargetSong.Timing);

                        LaneStep step = new LaneStep();
                        step.Offset = p;
                        step.StartPos = cur.StartPos;
                        step.EndPos = cur.EndPos;

                        HistoryAdd(TargetLane.LaneSteps, step);
                        TargetLane.LaneSteps.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                        TargetThing = step;
                        Repaint();
                    }
                    else if (dragMode == "seeksnap" && pickermode.StartsWith("hit_") && TargetLane != null)
                    {
                        HitObject hit = new HitObject();
                        hit.Offset = (float)(Math.Ceiling(pos * 1e5) / 1e5);
                        hit.Type = pickermode == "hit_catch" ? HitObject.HitType.Catch : HitObject.HitType.Normal;
                        if (TargetThing is HitObject)
                        {
                            HitObject thing = (HitObject)TargetThing;
                            hit.Position = thing.Position;
                            hit.Length = thing.Length;
                        }
                        else
                        {
                            hit.Length = 1;
                        }
                        HistoryAdd(TargetLane.Objects, hit);
                        TargetLane.Objects.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                        TargetThing = hit;
                        Repaint();
                    }
                }
            }

            dragMode = "";
            mouseBtn = -1;
            selectStart = selectEnd = null;
        }

        if (seekTime >= seekStart && seekTime < seekEnd)
        {
            float pos = (seekTime - seekStart) / (seekEnd - seekStart) * width;
            EditorGUI.DrawRect(new Rect(pos + 1, 0, 2, 115), EditorGUIUtility.isProSkin ? Color.white : Color.black);
        }
    }

    #endregion

    ////////////////////////
    #region Inspect Mode Window
    ////////////////////////

    string inspectMode = "properties";

    public void InspectMode(int id)
    {
        GUIUtility.RotateAroundPivot(-90, Vector2.one * (height / 2 - 102));
        if (GUI.Toggle(new Rect(27, 0, 80, 28), inspectMode == "properties", "Properties", "button")) inspectMode = "properties";
        if (GUI.Toggle(new Rect(109, 0, 80, 28), inspectMode == "storyboard", "Storyboard", "button")) inspectMode = "storyboard";
    }

    #endregion

    ////////////////////////
    #region Inspector Window
    ////////////////////////

    Vector2 scrollPos = Vector2.zero;

    long BPMTapStart, BPMTapEnd, BPMTapCount;

    public void Inspector(int id)
    {
        GUI.Label(new Rect(0, 0, 240, 24), "", "button");
        EditorGUIUtility.labelWidth = 80;
        if (inspectMode == "debug")
        {
            GUI.Label(new Rect(7, 2, 226, 20), "Debug Stats", "boldLabel");
            GUILayout.Space(8);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Performance", "boldLabel");
            GUILayout.Label(
                "loop: " + (1e7d / delta).ToString("N0", invariant) + "fps " + (delta / 1e4d).ToString("N0", invariant) + "ms" +
                "\npass count: " + (pass).ToString("N0", invariant) +
                "\nstrain: " + strain
            );

            GUILayout.Space(8);
            GUILayout.Label("Geometry", "boldLabel");
            int v = 0, t = 0;
            foreach (Mesh mesh in Meshes)
            {
                v += mesh.vertexCount;
                t += mesh.triangles.Length / 3;
            }
            GUILayout.Label(
                "mesh count: " + Meshes.Count.ToString("N0", invariant) +
                "\nv: " + v.ToString("N0", invariant) + " t: " + t.ToString("N0", invariant)

            );

            GUILayout.EndScrollView();
        }
        else if (inspectMode == "history")
        {
            GUI.Label(new Rect(7, 2, 226, 20), "Edit History", "boldLabel");
            GUILayout.Space(8);

            scrollPos = GUILayout.BeginScrollView(scrollPos);


            for (int i = 0; i < History.ActionsAhead.Count; i++)
            {
                ICharterAction action = History.ActionsAhead[i];
                GUILayout.Button(action.GetName());
            }

            GUILayout.Toggle(true, "↑ Ahead  |  Behind ↓", "button");

            for (int i = History.ActionsBehind.Count - 1; i >= 0; i--)
            {
                ICharterAction action = History.ActionsBehind[i];
                GUILayout.Button(action.GetName());
            }

            GUILayout.EndScrollView();
        }
        else if (TargetThing == null)
        {
            GUI.Label(new Rect(7, 2, 226, 20), "No object selected", "boldLabel");
            GUILayout.Space(8);
            GUILayout.Label("Please select an object to start editing.");
        }
        else if (inspectMode == "properties")
        {
            if (TargetThing is IList)
            {
                IList thing = (IList)TargetThing;

                GUIStyle bStyle = new GUIStyle("textField");
                bStyle.fontStyle = FontStyle.Bold;

                GUI.Label(new Rect(7, 2, 226, 20), "Multi-select", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                string name = "items";
                if (thing is List<Timestamp>) name = "Timestamps";
                else if (thing is List<BPMStop>) name = "BPM Stops";
                else if (thing is List<Lane>) name = "Lanes";
                else if (thing is List<LaneStep>) name = "Lane Steps";
                else if (thing is List<HitObject>) name = "Hit Objects";
                GUILayout.Label(thing.Count + " " + name, "boldLabel");

                GUILayout.EndScrollView();
            }
            else if (TargetTimestamp != null)
            {
                IStoryboardable thing = (IStoryboardable)TargetThing;
                Timestamp ts = TargetTimestamp;
                History.StartRecordItem(ts);

                GUIStyle bStyle = new GUIStyle("textField");
                bStyle.fontStyle = FontStyle.Bold;

                GUIStyle rightStyle = new GUIStyle("label");
                rightStyle.alignment = TextAnchor.UpperRight;
                rightStyle.normal.textColor = new Color(rightStyle.normal.textColor.r,
                    rightStyle.normal.textColor.g, rightStyle.normal.textColor.b, .5f);


                List<string> tst = new List<string>();
                List<string> tso = new List<string>();
                foreach (TimestampType t in (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null))
                {
                    tso.Add(t.ID);
                    tst.Add(t.Name);
                }

                List<string> est = new List<string>();
                List<string> eso = new List<string>();
                foreach (Ease ease in Ease.Eases.Values)
                {
                    eso.Add(ease.ID);
                    est.Add(ease.Name);
                }

                GUI.Label(new Rect(7, 2, 226, 20), "Timestamp", "boldLabel");
                GUILayout.Space(8);
                ts.Time = EditorGUI.FloatField(new Rect(163, 2, 75, 20), ts.Time);
                GUI.Label(new Rect(163, 2, 75, 20), "b", rightStyle);

                scrollPos = GUILayout.BeginScrollView(scrollPos);

                int type = tso.IndexOf(ts.ID);
                int newType = EditorGUILayout.Popup("Type", type, tst.ToArray());
                if (newType != type) ts.ID = tso[newType];
                ts.Duration = EditorGUILayout.FloatField("Duration", ts.Duration);
                GUILayout.Space(8);

                ts.From = EditorGUILayout.Toggle("From", !float.IsNaN(ts.From))
                    ? (float.IsNaN(ts.From) ? 0 : EditorGUILayout.FloatField(" ", ts.From))
                    : float.NaN;
                ts.Target = EditorGUILayout.FloatField("To", ts.Target);

                int eas = eso.IndexOf(ts.Easing);
                int newEas = EditorGUILayout.Popup("Easing", eas, est.ToArray());
                if (newEas != eas) ts.Easing = eso[newEas];
                ts.EaseMode = (EaseMode)EditorGUILayout.EnumPopup(" ", ts.EaseMode);

                GUILayout.EndScrollView();
                History.EndRecordItem(ts);
            }
            else if (TargetThing is PlayableSong)
            {
                PlayableSong thing = (PlayableSong)TargetThing;
                History.StartRecordItem(TargetThing);

                GUIStyle bStyle = new GUIStyle("textField");
                bStyle.fontStyle = FontStyle.Bold;

                GUI.Label(new Rect(7, 2, 226, 20), "Song Details", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                GUILayout.Label("Metadata", "boldLabel");
                thing.SongName = EditorGUILayout.TextField("Song Name", thing.SongName, bStyle);
                thing.AltSongName = EditorGUILayout.TextField("Alt Name", thing.AltSongName, bStyle);
                thing.SongArtist = EditorGUILayout.TextField("Song Artist", thing.SongArtist);
                thing.AltSongArtist = EditorGUILayout.TextField("Alt Artist", thing.AltSongArtist);
                thing.Location = EditorGUILayout.TextField("Location", thing.Location);
                thing.Genre = EditorGUILayout.TextField("Genre", thing.Genre);
                GUILayout.Space(8);
                GUILayout.Label("Charts", "boldLabel");
                foreach (ExternalChartMeta chart in TargetSong.Charts)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Toggle(TargetChartMeta == chart, chart.DifficultyName + " " + chart.DifficultyLevel, "ButtonLeft") && TargetChartMeta != chart)
                    {
                        TargetChartMeta = chart;
                        LoadChart(chart);
                    }
                    if (GUILayout.Button(DeletingThing == chart ? "?" : "x", "ButtonRight", GUILayout.MaxWidth(18)) && TargetChartMeta != chart)
                    {
                        DeletingThing = TempChartMeta = chart;
                        extrasmode = "chart_delete";
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Create New Chart"))
                {
                    TempChartMeta = new ExternalChartMeta();
                    extrasmode = "chart_create";
                }
                GUILayout.EndScrollView();
                History.EndRecordItem(TargetThing);
            }
            else if (TargetThing is BPMStop)
            {
                BPMStop thing = (BPMStop)TargetThing;
                History.StartRecordItem(TargetThing);

                GUIStyle rightStyle = new GUIStyle("label");
                rightStyle.alignment = TextAnchor.UpperRight;
                rightStyle.normal.textColor = new Color(rightStyle.normal.textColor.r,
                    rightStyle.normal.textColor.g, rightStyle.normal.textColor.b, .5f);

                GUI.Label(new Rect(7, 2, 226, 20), "BPM Stop", "boldLabel");
                thing.Offset = EditorGUI.FloatField(new Rect(163, 2, 75, 20), thing.Offset);
                GUI.Label(new Rect(163, 2, 75, 20), "s", rightStyle);

                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                GUILayout.Label("Timing", "boldLabel");
                thing.BPM = EditorGUILayout.FloatField("BPM", thing.BPM);
                thing.Signature = EditorGUILayout.IntField("Signature", thing.Signature);

                GUILayout.BeginHorizontal();
                GUILayout.Label(
                    BPMTapCount >= 4 ? "BPM = " + thing.BPM.ToString("F2", invariant) :
                    (BPMTapCount > 0 ? "Keep tapping... " + BPMTapCount.ToString(invariant) + " / 4" : "Tap BPM"),
                    "button"
                );
                Event current = Event.current;
                if (current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(current.mousePosition))
                {
                    BPMTapEnd = now;
                    if (BPMTapCount <= 0) BPMTapStart = now;
                    BPMTapCount++;
                    if (BPMTapCount >= 4)
                    {
                        thing.BPM = 6e8f / (BPMTapEnd - BPMTapStart) * (BPMTapCount - 1);
                    }
                    Repaint();
                }
                if (GUILayout.Button("Reset", GUILayout.Width(60)))
                {
                    BPMTapStart = BPMTapEnd = BPMTapCount = 0;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                History.EndRecordItem(TargetThing);
            }
            else if (TargetThing is Chart)
            {
                Chart thing = TargetChart.Data;
                History.StartRecordItem(TargetThing);

                GUI.Label(new Rect(7, 2, 226, 20), "Chart Details", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                GUILayout.Label("Difficulty", "boldLabel");
                TargetChartMeta.DifficultyIndex = thing.DifficultyIndex = EditorGUILayout.IntField("Index", thing.DifficultyIndex);
                TargetChartMeta.DifficultyName = thing.DifficultyName = EditorGUILayout.TextField("Name", thing.DifficultyName);
                TargetChartMeta.DifficultyLevel = thing.DifficultyLevel = EditorGUILayout.TextField("Level", thing.DifficultyLevel);
                TargetChartMeta.ChartConstant = thing.ChartConstant = EditorGUILayout.IntField("Constant", thing.ChartConstant);
                GUILayout.Space(8);
                GUILayout.Label("Camera", "boldLabel");
                thing.CameraPivot = EditorGUILayout.Vector3Field("Pivot", thing.CameraPivot);
                thing.CameraRotation = EditorGUILayout.Vector3Field("Rotation", thing.CameraRotation);
                GUILayout.EndScrollView();
                History.EndRecordItem(TargetThing);
            }
            else if (TargetThing is Pallete)
            {
                Pallete thing = (Pallete)TargetThing;
                History.StartRecordItem(TargetThing);

                GUI.Label(new Rect(7, 2, 226, 20), "Palette", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);

                GUILayout.Label("Appearance", "boldLabel");
                thing.BackgroundColor = EditorGUILayout.ColorField("Background Color", thing.BackgroundColor);
                thing.InterfaceColor = EditorGUILayout.ColorField("Interface Color", thing.InterfaceColor);

                GUILayout.Space(8);
                GUILayout.Label("Lane Styles", "boldLabel");
                GUIStyle leftStyle = new GUIStyle("ButtonLeft") { alignment = TextAnchor.MiddleLeft };
                for (int i = 0; i < thing.LaneStyles.Count; i++)
                {
                    LaneStyle style = thing.LaneStyles[i];
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("ID " + i, leftStyle))
                    {
                        TargetThing = style;
                    }
                    if (GUILayout.Button(DeletingThing == style ? "?" : "x", "ButtonRight", GUILayout.MaxWidth(18)))
                    {
                        if (DeletingThing == style)
                        {
                            HistoryDelete(thing.LaneStyles, style);
                            break;
                        }
                        else
                        {
                            DeletingThing = style;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Create New Style"))
                {
                    LaneStyle style = new LaneStyle();
                    HistoryAdd(thing.LaneStyles, style);
                    TargetThing = style;
                }

                GUILayout.Space(8);
                GUILayout.Label("Hit Styles", "boldLabel");
                for (int i = 0; i < thing.HitStyles.Count; i++)
                {
                    HitStyle style = thing.HitStyles[i];
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("ID " + i, leftStyle))
                    {
                        TargetThing = style;
                    }
                    if (GUILayout.Button(DeletingThing == style ? "?" : "x", "ButtonRight", GUILayout.MaxWidth(18)))
                    {
                        if (DeletingThing == style)
                        {
                            HistoryDelete(thing.HitStyles, style);
                            break;
                        }
                        else
                        {
                            DeletingThing = style;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Create New Style"))
                {
                    HitStyle style = new HitStyle();
                    HistoryAdd(thing.HitStyles, style);
                    TargetThing = style;
                }

                GUILayout.EndScrollView();
                History.EndRecordItem(TargetThing);
            }
            else if (TargetThing is LaneStyle)
            {
                LaneStyle thing = (LaneStyle)TargetThing;
                History.StartRecordItem(TargetThing);

                GUI.Label(new Rect(7, 2, 226, 20), "Lane Style", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);

                GUILayout.Label("Lane", "boldLabel");
                thing.LaneMaterial = (Material)EditorGUILayout.ObjectField("Lane Material", thing.LaneMaterial, typeof(Material), false);
                thing.LaneColorTarget = EditorGUILayout.TextField("Lane Color Target", thing.LaneColorTarget);
                thing.LaneColor = EditorGUILayout.ColorField("Lane Color", thing.LaneColor);

                GUILayout.Space(8);
                GUILayout.Label("Judge", "boldLabel");
                thing.JudgeMaterial = (Material)EditorGUILayout.ObjectField("Judge Material", thing.JudgeMaterial, typeof(Material), false);
                thing.JudgeColorTarget = EditorGUILayout.TextField("Judge Color Target", thing.JudgeColorTarget);
                thing.JudgeColor = EditorGUILayout.ColorField("Judge Color", thing.JudgeColor);

                GUILayout.EndScrollView();
                History.EndRecordItem(TargetThing);
            }
            else if (TargetThing is HitStyle)
            {
                HitStyle thing = (HitStyle)TargetThing;
                History.StartRecordItem(TargetThing);

                GUI.Label(new Rect(7, 2, 226, 20), "Lane Style", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);

                GUILayout.Label("Body", "boldLabel");
                thing.MainMaterial = (Material)EditorGUILayout.ObjectField("Body Material", thing.MainMaterial, typeof(Material), false);
                thing.MainColorTarget = EditorGUILayout.TextField("Body Color Target", thing.MainColorTarget);
                thing.NormalColor = EditorGUILayout.ColorField("Normal Color", thing.NormalColor);
                thing.CatchColor = EditorGUILayout.ColorField("Catch Color", thing.CatchColor);

                GUILayout.Space(8);
                GUILayout.Label("Hold Tail", "boldLabel");
                thing.HoldTailMaterial = (Material)EditorGUILayout.ObjectField("Hold Tail Material", thing.HoldTailMaterial, typeof(Material), false);
                thing.HoldTailColorTarget = EditorGUILayout.TextField("Hold Tail Color Target", thing.HoldTailColorTarget);
                thing.HoldTailColor = EditorGUILayout.ColorField("Hold Tail Color", thing.HoldTailColor);

                GUILayout.EndScrollView();
                History.EndRecordItem(TargetThing);
            }
            else if (TargetThing is Lane)
            {
                Lane thing = (Lane)TargetThing;
                History.StartRecordItem(TargetThing);

                GUI.Label(new Rect(7, 2, 226, 20), "Lane", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);

                GUIStyle labelStyle = new GUIStyle("label");
                labelStyle.padding = new RectOffset(3, 3, 1, 1);
                labelStyle.fontSize = 10;

                GUIStyle rightStyle = new GUIStyle(labelStyle);
                rightStyle.alignment = TextAnchor.UpperRight;
                rightStyle.normal.textColor = new Color(rightStyle.normal.textColor.r,
                    rightStyle.normal.textColor.g, rightStyle.normal.textColor.b, .5f);

                GUIStyle fieldStyle = new GUIStyle("textField");
                fieldStyle.padding = new RectOffset(3, 3, 1, 1);
                fieldStyle.fontSize = 10;

                GUIStyle buttonStyle = new GUIStyle("button");
                buttonStyle.padding = new RectOffset(3, 3, 1, 1);
                buttonStyle.fontSize = 10;

                GUIStyle buttonLeftStyle = new GUIStyle(buttonStyle);
                buttonLeftStyle.alignment = TextAnchor.UpperLeft;

                GUIStyle bStyle = new GUIStyle(fieldStyle);
                bStyle.fontStyle = FontStyle.Bold;

                GUILayout.Label("Transform", "boldLabel");
                thing.Offset = EditorGUILayout.Vector3Field("Offset", thing.Offset);
                thing.OffsetRotation = EditorGUILayout.Vector3Field("Rotation", thing.OffsetRotation);

                GUILayout.Space(8);
                GUILayout.Label("Appearance", "boldLabel");
                thing.StyleIndex = EditorGUILayout.IntField("Style Index", thing.StyleIndex);

                History.EndRecordItem(TargetThing);

                GUILayout.Space(8);
                GUILayout.Label("Steps", "boldLabel");
                float h = 0;
                float o = GUILayoutUtility.GetLastRect().yMax;
                float a = thing.LaneSteps[0].Offset;

                List<string> est = new List<string>();
                List<string> eso = new List<string>();
                foreach (Ease ease in Ease.Eases.Values)
                {
                    eso.Add(ease.ID);
                    est.Add(ease.Name);
                }

                foreach (LaneStep step in thing.LaneSteps)
                {
                    History.StartRecordItem(step);
                    GUI.Label(new Rect(19, h + o + 2, 187, 48), "", "buttonMid");

                    step.Offset = EditorGUI.FloatField(new Rect(20, h + o + 4, 40, 14), step.Offset, bStyle);
                    GUI.Label(new Rect(20, h + o + 4, 40, 14), "b", rightStyle);
                    step.Speed = EditorGUI.FloatField(new Rect(61, h + o + 4, 40, 14), step.Speed, fieldStyle);
                    GUI.Label(new Rect(61, h + o + 4, 40, 14), "x", rightStyle);

                    {
                        step.StartPos.x = EditorGUI.FloatField(new Rect(20, h + o + 19, 40, 14), step.StartPos.x, fieldStyle);
                        GUI.Label(new Rect(20, h + o + 19, 40, 14), "x0", rightStyle);
                        int easeX = eso.IndexOf(step.StartEaseX);
                        step.StartEaseXMode = (EaseMode)EditorGUI.EnumPopup(new Rect(61, h + o + 19, 17, 14), step.StartEaseXMode, buttonStyle);
                        GUI.Button(new Rect(61, h + o + 19, 17, 14), new[] { "I", "O", "IO" }[(int)step.StartEaseXMode], buttonStyle);
                        int newEaseX = EditorGUI.Popup(new Rect(79, h + o + 19, 30, 14), easeX, est.ToArray(), buttonLeftStyle);
                        if (newEaseX != easeX) step.StartEaseX = eso[newEaseX];

                        step.StartPos.y = EditorGUI.FloatField(new Rect(110, h + o + 19, 40, 14), step.StartPos.y, fieldStyle);
                        GUI.Label(new Rect(110, h + o + 19, 40, 14), "y0", rightStyle);
                        int easeY = eso.IndexOf(step.StartEaseY);
                        step.StartEaseYMode = (EaseMode)EditorGUI.EnumPopup(new Rect(151, h + o + 19, 17, 14), step.StartEaseYMode, buttonStyle);
                        GUI.Button(new Rect(151, h + o + 19, 17, 14), new[] { "I", "O", "IO" }[(int)step.StartEaseYMode], buttonStyle);
                        int newEaseY = EditorGUI.Popup(new Rect(169, h + o + 19, 30, 14), easeY, est.ToArray(), buttonLeftStyle);
                        if (newEaseY != easeY) step.StartEaseY = eso[newEaseY];
                    }
                    {
                        step.EndPos.x = EditorGUI.FloatField(new Rect(20, h + o + 34, 40, 14), step.EndPos.x, fieldStyle);
                        GUI.Label(new Rect(20, h + o + 34, 40, 14), "x1", rightStyle);
                        int easeX = eso.IndexOf(step.EndEaseX);
                        step.EndEaseXMode = (EaseMode)EditorGUI.EnumPopup(new Rect(61, h + o + 34, 17, 14), step.EndEaseXMode, buttonStyle);
                        GUI.Button(new Rect(61, h + o + 34, 17, 14), new[] { "I", "O", "IO" }[(int)step.EndEaseXMode], buttonStyle);
                        int newEaseX = EditorGUI.Popup(new Rect(79, h + o + 34, 30, 14), easeX, est.ToArray(), buttonLeftStyle);
                        if (newEaseX != easeX) step.EndEaseX = eso[newEaseX];

                        step.EndPos.y = EditorGUI.FloatField(new Rect(110, h + o + 34, 40, 14), step.EndPos.y, fieldStyle);
                        GUI.Label(new Rect(110, h + o + 34, 40, 14), "y1", rightStyle);
                        int easeY = eso.IndexOf(step.EndEaseY);
                        step.EndEaseYMode = (EaseMode)EditorGUI.EnumPopup(new Rect(151, h + o + 34, 17, 14), step.EndEaseYMode, buttonStyle);
                        GUI.Button(new Rect(151, h + o + 34, 17, 14), new[] { "I", "O", "IO" }[(int)step.EndEaseYMode], buttonStyle);
                        int newEaseY = EditorGUI.Popup(new Rect(169, h + o + 34, 30, 14), easeY, est.ToArray(), buttonLeftStyle);
                        if (newEaseY != easeY) step.EndEaseY = eso[newEaseY];
                    }

                    if (GUI.Button(new Rect(3, h + o + 2, 16, 48), "⋮", "buttonLeft"))
                    {
                        TargetThing = step;
                    }
                    if (GUI.Button(new Rect(202, h + o + 2, 16, 48), "x", "buttonRight") && thing.LaneSteps.Count > 1)
                    {
                        HistoryDelete(thing.LaneSteps, step);
                        break;
                    }
                    History.EndRecordItem(step);
                    h += 50;
                }
                GUILayout.Space(h);
                if (GUILayout.Button("Create New Step"))
                {
                    LaneStep step = new LaneStep();
                    step.Offset = Mathf.Round(pos * 1000) / 1000;
                    step.StartPos = thing.LaneSteps[thing.LaneSteps.Count - 1].StartPos;
                    step.EndPos = thing.LaneSteps[thing.LaneSteps.Count - 1].EndPos;
                    HistoryAdd(thing.LaneSteps, step);
                    thing.LaneSteps.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                }
                GUILayout.EndScrollView();

                if (thing.LaneSteps[0].Offset != a)
                {
                    TargetChart.Data.Lanes.Sort((x, y) => x.LaneSteps[0].Offset.CompareTo(y.LaneSteps[0].Offset));
                }
            }
            else if (TargetThing is LaneStep)
            {
                LaneStep thing = (LaneStep)TargetThing;
                History.StartRecordItem(TargetThing);

                GUIStyle rightStyle = new GUIStyle("label");
                rightStyle.alignment = TextAnchor.UpperRight;
                rightStyle.normal.textColor = new Color(rightStyle.normal.textColor.r,
                    rightStyle.normal.textColor.g, rightStyle.normal.textColor.b, .5f);

                List<string> est = new List<string>();
                List<string> eso = new List<string>();
                foreach (Ease ease in Ease.Eases.Values)
                {
                    eso.Add(ease.ID);
                    est.Add(ease.Name);
                }

                GUI.Label(new Rect(7, 2, 226, 20), "Lane Step", "boldLabel");
                thing.Offset = EditorGUI.FloatField(new Rect(163, 2, 75, 20), thing.Offset);
                GUI.Label(new Rect(163, 2, 75, 20), "b", rightStyle);
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);

                GUILayout.Label("Transform", "boldLabel");
                {
                    thing.StartPos = EditorGUILayout.Vector2Field("Start Position", thing.StartPos);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(17);
                    int easeX = eso.IndexOf(thing.StartEaseX);
                    int newEaseX = EditorGUILayout.Popup(easeX, est.ToArray());
                    if (newEaseX != easeX) thing.StartEaseX = eso[newEaseX];
                    int easeY = eso.IndexOf(thing.StartEaseY);
                    int newEaseY = EditorGUILayout.Popup(easeY, est.ToArray());
                    if (newEaseY != easeX) thing.StartEaseY = eso[newEaseY];
                    GUILayout.Space(1);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(17);
                    thing.StartEaseXMode = (EaseMode)EditorGUILayout.EnumPopup(thing.StartEaseXMode);
                    thing.StartEaseYMode = (EaseMode)EditorGUILayout.EnumPopup(thing.StartEaseYMode);
                    GUILayout.Space(1);
                    GUILayout.EndHorizontal();
                }
                {
                    thing.EndPos = EditorGUILayout.Vector2Field("End Position", thing.EndPos);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(17);
                    int easeX = eso.IndexOf(thing.EndEaseX);
                    int newEaseX = EditorGUILayout.Popup(easeX, est.ToArray());
                    if (newEaseX != easeX) thing.EndEaseX = eso[newEaseX];
                    int easeY = eso.IndexOf(thing.EndEaseY);
                    int newEaseY = EditorGUILayout.Popup(easeY, est.ToArray());
                    if (newEaseY != easeX) thing.EndEaseY = eso[newEaseY];
                    GUILayout.Space(1);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(17);
                    thing.EndEaseXMode = (EaseMode)EditorGUILayout.EnumPopup(thing.EndEaseXMode);
                    thing.EndEaseYMode = (EaseMode)EditorGUILayout.EnumPopup(thing.EndEaseYMode);
                    GUILayout.Space(1);
                    GUILayout.EndHorizontal();
                }
                thing.Speed = EditorGUILayout.FloatField("Speed", thing.Speed);

                GUILayout.EndScrollView();
                History.EndRecordItem(TargetThing);
            }
            else if (TargetThing is HitObject)
            {
                HitObject thing = (HitObject)TargetThing;
                History.StartRecordItem(TargetThing);

                GUIStyle rightStyle = new GUIStyle("label");
                rightStyle.alignment = TextAnchor.UpperRight;
                rightStyle.normal.textColor = new Color(rightStyle.normal.textColor.r,
                    rightStyle.normal.textColor.g, rightStyle.normal.textColor.b, .5f);

                GUI.Label(new Rect(7, 2, 226, 20), "Hit Object", "boldLabel");
                thing.Offset = EditorGUI.FloatField(new Rect(163, 2, 75, 20), thing.Offset);
                GUI.Label(new Rect(163, 2, 75, 20), "b", rightStyle);
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);

                thing.Type = (HitObject.HitType)EditorGUILayout.EnumPopup("Type", (System.Enum)thing.Type);
                GUILayout.Label("Transform", "boldLabel");
                thing.Position = EditorGUILayout.FloatField("Position", thing.Position);
                thing.Length = EditorGUILayout.FloatField("Length", thing.Length);
                thing.HoldLength = EditorGUILayout.FloatField("Hold Length", thing.HoldLength);

                float start, end;
                float startR = start = thing.Position;
                float endR = end = thing.Position + thing.Length;
                EditorGUILayout.MinMaxSlider(ref start, ref end, 0, 1);
                if (startR != start || endR != end)
                {
                    thing.Length = Mathf.Round((end - start) / .05f) * .05f;
                    thing.Position = Mathf.Round(start / .05f) * .05f;
                }

                GUILayout.Space(8);
                thing.Flickable = EditorGUILayout.Toggle("Flickable", thing.Flickable);
                if (thing.Flickable)
                {
                    thing.FlickDirection = EditorGUILayout.Toggle("Directional", thing.FlickDirection >= 0)
                        ? EditorGUILayout.Slider(" ", thing.FlickDirection, 0, 360)
                        : -1;
                }

                GUILayout.Space(8);
                GUILayout.Label("Appearance", "boldLabel");
                thing.StyleIndex = EditorGUILayout.IntField("Style Index", thing.StyleIndex);

                GUILayout.EndScrollView();
                History.EndRecordItem(TargetThing);
            }
        }
        else if (inspectMode == "storyboard")
        {
            GUI.Label(new Rect(7, 2, 226, 20), "Storyboard", "boldLabel");
            GUILayout.Space(8);
            if (TargetThing is IStoryboardable)
            {
                IStoryboardable thing = (IStoryboardable)TargetThing;
                Storyboard sb = thing.Storyboard;

                GUIStyle labelStyle = new GUIStyle("label");
                labelStyle.padding = new RectOffset(3, 3, 1, 1);
                labelStyle.fontSize = 10;

                GUIStyle rightStyle = new GUIStyle(labelStyle);
                rightStyle.alignment = TextAnchor.UpperRight;
                rightStyle.normal.textColor = new Color(rightStyle.normal.textColor.r,
                    rightStyle.normal.textColor.g, rightStyle.normal.textColor.b, .5f);

                GUIStyle fieldStyle = new GUIStyle("textField");
                fieldStyle.padding = new RectOffset(3, 3, 1, 1);
                fieldStyle.fontSize = 10;

                GUIStyle buttonStyle = new GUIStyle("button");
                buttonStyle.padding = new RectOffset(3, 3, 1, 1);
                buttonStyle.fontSize = 10;

                GUIStyle buttonLeftStyle = new GUIStyle(buttonStyle);
                buttonLeftStyle.alignment = TextAnchor.UpperLeft;

                GUIStyle bStyle = new GUIStyle(fieldStyle);
                bStyle.fontStyle = FontStyle.Bold;

                List<string> tst = new List<string>();
                List<string> tso = new List<string>();
                foreach (TimestampType type in (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null))
                {
                    tso.Add(type.ID);
                    tst.Add(type.Name);
                }

                List<string> est = new List<string>();
                List<string> eso = new List<string>();
                foreach (Ease ease in Ease.Eases.Values)
                {
                    eso.Add(ease.ID);
                    est.Add(ease.Name);
                }


                int add = EditorGUI.Popup(new Rect(218, 2, 20, 20), -1, tst.ToArray(), "button");
                if (add != -1)
                {
                    HistoryAdd(sb.Timestamps, new Timestamp
                    {
                        ID = tso[add],
                        Time = pos,
                    });
                }
                GUI.Button(new Rect(218, 2, 20, 20), "+");

                scrollPos = GUILayout.BeginScrollView(scrollPos);

                float h = 0;
                float o = 0; // GUILayoutUtility.GetLastRect().yMax;

                foreach (Timestamp ts in sb.Timestamps)
                {
                    History.StartRecordItem(ts);
                    GUI.Label(new Rect(3, h + o + 2, 203, 33), "", "buttonLeft");

                    ts.Time = EditorGUI.FloatField(new Rect(5, h + o + 4, 40, 14), ts.Time, bStyle);
                    GUI.Label(new Rect(5, h + o + 4, 40, 14), "b", rightStyle);
                    GUI.Label(new Rect(45, h + o + 4, 30, 14), "time", labelStyle);

                    ts.Duration = EditorGUI.FloatField(new Rect(5, h + o + 19, 40, 14), ts.Duration, bStyle);
                    GUI.Label(new Rect(5, h + o + 19, 40, 14), "b", rightStyle);
                    GUI.Label(new Rect(45, h + o + 19, 30, 14), "dur", labelStyle);

                    int type = tso.IndexOf(ts.ID);
                    int newType = EditorGUI.Popup(new Rect(116, h + o + 4, 83, 14), type, tst.ToArray(), buttonLeftStyle);
                    if (newType != type) ts.ID = tso[newType];

                    ts.Target = EditorGUI.FloatField(new Rect(75, h + o + 4, 40, 14), ts.Target, bStyle);

                    ts.EaseMode = (EaseMode)EditorGUI.EnumPopup(new Rect(75, h + o + 19, 40, 14), ts.EaseMode, buttonStyle);

                    int ease = eso.IndexOf(ts.Easing);
                    int newEase = EditorGUI.Popup(new Rect(116, h + o + 19, 83, 14), ease, est.ToArray(), buttonLeftStyle);
                    if (newEase != ease) ts.Easing = eso[newEase];

                    if (GUI.Button(new Rect(202, h + o + 2, 16, 33), "x", "buttonRight"))
                    {
                        HistoryDelete(sb.Timestamps, ts);
                        break;
                    }
                    History.EndRecordItem(ts);
                    h += 35;
                }

                GUILayout.Space(h);
                GUILayout.EndScrollView();
            }
            else if (TargetThing is IList)
            {
                GUILayout.Label("Can not edit storyboard of multiple objects");
            }
            else
            {
                GUILayout.Label("This object is not storyboardable.");
            }
        }
    }

    #endregion

    /////////////////////
    #region Picker Window
    /////////////////////

    public string pickermode = "cursor";

    public void Picker(int id)
    {
        if (GUI.Toggle(new Rect(0, 0, 33, 33), pickermode == "cursor", EditorGUIUtility.IconContent("Grid.Default@2x", "|Cursor"), "button")) pickermode = "cursor";
        if (GUI.Toggle(new Rect(0, 32, 33, 33), pickermode == "select", EditorGUIUtility.IconContent("Selectable Icon", "|Select"), "button")) pickermode = "select";
        if (GUI.Toggle(new Rect(0, 64, 33, 33), pickermode == "delete", EditorGUIUtility.IconContent("winbtn_win_close@2x", "|Delete"), "button")) pickermode = "delete";

        if (timelineMode == "story")
        {
            if (GUI.Toggle(new Rect(0, 106, 33, 33), pickermode == "timestamp", new GUIContent("TMP", "Timestamp"), "button")) pickermode = "timestamp";
        }
        else if (timelineMode == "timing")
        {
            if (GUI.Toggle(new Rect(0, 106, 33, 33), pickermode == "bpmstop", new GUIContent("BPM", "BPM Stop"), "button")) pickermode = "bpmstop";
        }
        else if (timelineMode == "lane")
        {
            if (GUI.Toggle(new Rect(0, 106, 33, 33), pickermode == "lane", new GUIContent("LNE", "Lane"), "button")) pickermode = "lane";
        }
        else if (timelineMode == "step")
        {
            if (GUI.Toggle(new Rect(0, 106, 33, 33), pickermode == "step", new GUIContent("STP", "Lane Step"), "button")) pickermode = "step";
        }
        else if (timelineMode == "hit")
        {
            if (GUI.Toggle(new Rect(0, 106, 33, 33), pickermode == "hit_normal", new GUIContent("NOR", "Normal Hit"), "button")) pickermode = "hit_normal";
            if (GUI.Toggle(new Rect(0, 138, 33, 33), pickermode == "hit_catch", new GUIContent("CAT", "Catch Hit"), "button")) pickermode = "hit_catch";
        }
    }

    #endregion

    /////////////////////
    #region Extras Window
    /////////////////////

    public string extrasmode = "";
    public string mainmenutab = "file";

    ExternalChartMeta TempChartMeta;

    public void Extras(int id)
    {
        if (extrasmode == "migrate_charts")
        {
            GUIStyle midStyle = new GUIStyle("label");
            midStyle.wordWrap = true;
            midStyle.alignment = TextAnchor.MiddleLeft;

            GUI.Label(new Rect(20, 0, 360, 200),
                "Old charts format detected: The editor found " + TargetSong.ChartsOld.Count + " legacy chart(s) in the playable song file, which is deprecated and need to be migrated to the newer chart format in order to be used again.\n\n"
                + "This will create chart files within the folder containing the playable song file and should not modify the contents of the charts in any ways.\n\n"
                + "You can choose to do the migration now or later by closing the song in case you have something to do first before proceeding.", midStyle);

            if (GUI.Button(new Rect(5, 195, 195, 20), "Close Song", "buttonLeft")) TargetSong = null;
            if (GUI.Button(new Rect(200, 195, 195, 20), "Migrate Now", "buttonRight"))
            {
                MigrateCharts();
                extrasmode = "";
            }
        }
        if (extrasmode == "chart_create")
        {
            GUIStyle title = new GUIStyle("boldLabel");
            title.alignment = TextAnchor.MiddleCenter;

            GUIStyle placeholder = new GUIStyle("label");
            Color x = placeholder.normal.textColor;
            placeholder.normal.textColor = new Color(x.r, x.g, x.b, .5f);

            GUI.Label(new Rect(5, 6, 390, 18), "Create New Chart", title);

            GUI.Label(new Rect(5, 30, 120, 18), "Target");
            TempChartMeta.Target = EditorGUI.TextField(new Rect(125, 30, 270, 18), TempChartMeta.Target);
            if (string.IsNullOrWhiteSpace(TempChartMeta.Target)) GUI.Label(new Rect(126, 30, 270, 18), TempChartMeta.DifficultyName + " " + TempChartMeta.DifficultyLevel, placeholder);

            GUI.Label(new Rect(5, 55, 120, 18), "Difficulty Index");
            TempChartMeta.DifficultyIndex = EditorGUI.IntField(new Rect(125, 55, 270, 18), TempChartMeta.DifficultyIndex);
            GUI.Label(new Rect(5, 75, 120, 18), "Difficulty Name");
            TempChartMeta.DifficultyName = EditorGUI.TextField(new Rect(125, 75, 270, 18), TempChartMeta.DifficultyName);
            GUI.Label(new Rect(5, 95, 120, 18), "Difficulty Level");
            TempChartMeta.DifficultyLevel = EditorGUI.TextField(new Rect(125, 95, 270, 18), TempChartMeta.DifficultyLevel);
            GUI.Label(new Rect(5, 115, 120, 18), "Chart Constant");
            TempChartMeta.ChartConstant = EditorGUI.IntField(new Rect(125, 115, 270, 18), TempChartMeta.ChartConstant);


            if (GUI.Button(new Rect(5, 195, 195, 20), "Cancel", "buttonLeft")) extrasmode = "";
            if (GUI.Button(new Rect(200, 195, 195, 20), "Create", "buttonRight"))
            {
                TempChartMeta.Target = string.IsNullOrWhiteSpace(TempChartMeta.Target) ? TempChartMeta.DifficultyName + " " + TempChartMeta.DifficultyLevel : TempChartMeta.Target;
                CreateChart(TempChartMeta);
                extrasmode = "";
            }
        }
        if (extrasmode == "chart_delete")
        {
            GUIStyle midStyle = new GUIStyle("label");
            midStyle.wordWrap = true;
            midStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(20, 0, 360, 100),
                "Delete " + TempChartMeta.DifficultyName + " " + TempChartMeta.DifficultyLevel + "? This will also delete the chart file in the project folder! You won't be able to undo this!", midStyle);

            if (GUI.Button(new Rect(5, 95, 195, 20), "Cancel", "buttonLeft")) extrasmode = "";
            if (GUI.Button(new Rect(200, 95, 195, 20), "Delete", "buttonRight"))
            {
                DeleteChart(TempChartMeta);
                extrasmode = "";
            }
        }
        if (extrasmode == "play_options")
        {
            GUI.Label(new Rect(5, 6, 90, 18), "Play Speed");
            CurrentAudioSource.pitch = Mathf.Round(Mathf.Pow(10, GUI.HorizontalSlider(new Rect(95, 6, 150, 18), Mathf.Log10(CurrentAudioSource.pitch), Mathf.Log10(.05f), 0)) / .05f) * .05f;
            CurrentAudioSource.pitch = Mathf.Round(Mathf.Clamp(EditorGUI.FloatField(new Rect(252, 6, 43, 18), CurrentAudioSource.pitch), .05f, 1) / .05f) * .05f;

            GUI.Label(new Rect(5, 28, 90, 18), "Scroll Speed");
            float spd = Mathf.Sqrt(ScrollSpeed);
            spd = Mathf.Round(GUI.HorizontalSlider(new Rect(95, 28, 150, 18), spd, .5f, 20) / .5f) * .5f;
            spd = Mathf.Round(Mathf.Clamp(EditorGUI.FloatField(new Rect(252, 28, 43, 18), spd), .5f, 20) / .5f) * .5f;
            ScrollSpeed = spd * spd;

            PlayMetronome = GUI.Toggle(new Rect(5, 95, 145, 20), PlayMetronome, "Metronome", "buttonLeft");
            PlayHitsounds = GUI.Toggle(new Rect(150, 95, 145, 20), PlayHitsounds, "Hitsounds", "buttonRight");
        }
        else if (extrasmode == "timeline_options")
        {
            GUI.Label(new Rect(5, 6, 90, 18), "Waveform");
            if (GUI.Toggle(new Rect(95, 6, 66, 18), WaveformMode == 0, "Off", "buttonLeft")) WaveformMode = 0;
            if (GUI.Toggle(new Rect(162, 6, 66, 18), WaveformMode == 1, "Paused", "buttonMid")) WaveformMode = 1;
            if (GUI.Toggle(new Rect(229, 6, 66, 18), WaveformMode == 2, "On", "buttonRight")) WaveformMode = 2;

            SeparateUnits = GUI.Toggle(new Rect(5, 95, 145, 20), SeparateUnits, "Separate Units", "buttonLeft");
            FollowSeekLine = GUI.Toggle(new Rect(150, 95, 145, 20), FollowSeekLine, "Follow Seek Line", "buttonRight");
        }
    }

    #endregion

    ///////////////////////
    #region Tutorial Window
    ///////////////////////

    int TutorialStage = -1;
    Vector2 TutorialPopupAnchor = new Vector2(.5f, .5f);
    Vector2 TutorialPopupPosition = Vector2.zero;
    float TutorialLerp = 1;

    public class TutorialStep
    {
        public string Content;
        public string RequirementText;
        public Func<Charter, bool> RequirementFunction;
        public Vector2 PopupAnchor = new Vector2(.5f, .5f);
        public Vector2 PopupPosition = Vector2.zero;
    }

    public TutorialStep[] TutorialSteps = new TutorialStep[] {
        new TutorialStep()
        {
            Content = "Welcome to J.A.N.O.A.R.G. Charter Engine's Interactive Tutorial! This window will introduce and guide you to the basics of creating J.A.N.O.A.R.G. charts.\n\n"
                + "If you ever decided to skip this at any point in the future, you can access the tutorial again in the playable song selection screen, which uhh... is this one.",
        },
        new TutorialStep()
        {
            Content = "Before you can chart, you need to know that J.A.N.O.A.R.G. stores charts of each song inside a file called a \"Playable Song\". To be able to chart, you'll need to create one first.\n\n"
                + "J.A.N.O.A.R.G. charts/playable songs do not have special folder/file name requirements, but it is recommended that you create a folder for each song for ease of access.",
        },
        new TutorialStep()
        {
            Content = "To create a playable song, drag the song from the Project tab to the \"Clip\" field, enter song details, then press the \"Create New Chart\" button. The playable song file will be put in the same folder as the audio file.\n\n"
                + "Or alternatively, you can use the left field to open an already existing playable song to make edits.",
            PopupPosition = new Vector2(0, -160),
            RequirementText = "Create/Open a Playable Song to continue",
            RequirementFunction = x => x.TargetSong != null,
        },
        new TutorialStep()
        {
            Content = "Welcome to the charting screen! You'll be redirected here when a playable song is selected.\n\n"
                + "You'll spend a majority of the time spending on this screen when charting, so feel free to get yourself used to it.\n\n"
                + "Anyways, may I introduce the interface to you?",
        },
        new TutorialStep()
        {
            Content = "This is the Toolbar. Stuff that's pretty important and needs to be accessed often will be here.\n\n"
                + "Controls from left to right: Main menu, playable song data, chart data, save button, play/pause, play settings, timers and the metronome.\n\n"
                + "Note that charts are not auto-saved, which explains the presence of the \"Save\" button.",
            PopupPosition = new Vector2(0, 120),
            PopupAnchor = new Vector2(.5f, 0),
        },
        new TutorialStep()
        {
            Content = "This is the Inspector. This panel will display information and let you adjust settings about the thing that's currently selected.\n\n"
                + "The Inspector has two modes: Attributes and Storyboard. More on the Storyboard later, right now we only need the Attributes mode.",
            PopupPosition = new Vector2(-380, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "This is the Timeline. This panel will display selectable items that has a time position on the chart.\n\n"
                + "Below the Timelines are selectable modes on the left and general chart values on the right.",
            PopupPosition = new Vector2(0, -220),
            PopupAnchor = new Vector2(.5f, 1),
        },
        new TutorialStep()
        {
            Content = "This is the Picker. Items will appear here depending on the Timeline mode you're currently in.\n\n"
                + "Besides from Timeline-mode-specific items, there are three general commands that's available on every mode: Cursor, Select and Delete.",
            PopupPosition = new Vector2(215, -80),
            PopupAnchor = new Vector2(0, .5f),
        },
        new TutorialStep()
        {
            Content = "Enough talking, to be able to chart we'll need to have a chart first!\n\n"
                + "Click on the song name above to open the song settings.",
            PopupPosition = new Vector2(180, 120),
            PopupAnchor = new Vector2(0, 0),
            RequirementText = "Click on the song name above",
            RequirementFunction = x => x.TargetThing == (object)x.TargetSong,
        },
        new TutorialStep()
        {
            Content = "Good! The Inspector will now display the information about the Playable Song since you just selected it earlier.\n\n"
                + "The section above display global song information that persists within charts, you can edit it here if you make some mistakes earlier.\n\n"
                + "Press the Continue button once you finished editing.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "To create a chart, press the \"Create a Chart\" button on the right.\n\n"
                + "If you already have a chart however, you can select a chart difficulty on the list here.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
            RequirementText = "Open or create a Chart to continue",
            RequirementFunction = x => x.TargetChart != null,
        },
        new TutorialStep()
        {
            Content = "Now that we opened a chart, you'll now see your chart being displayed here on the middle of the screen (defaults to a black screen with a white border).\n\n"
                + "What you see here will be what the player will see when they play your chart!",
        },
        new TutorialStep()
        {
            Content = "And yes, charts have their own global data too!\n\n"
                + "Click on the chart difficulty to select the chart.\n\n"
                + "Also you can click on the small button on the right to select charts.",
            PopupPosition = new Vector2(180, 120),
            PopupAnchor = new Vector2(0, 0),
            RequirementText = "Click on the chart above to select",
            RequirementFunction = x => x.TargetThing is Chart,
        },
        new TutorialStep()
        {
            Content = "Here are the data of the chart. The Metadata contains data that will be used to separate chart of the same song.\n\n"
                + "Besides the difficulty name and rating, charts also have an Index number that will be referenced internally and a Constant number that will be used in skill rating calculations.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "The first thing we should do when creating a chart is to make sure everything is synced!\n\n"
                + "Click on the Timing tab here to open the BPM editor.",
            PopupPosition = new Vector2(180, -120),
            PopupAnchor = new Vector2(0, 1),
            RequirementText = "Open the Timing tab",
            RequirementFunction = x => x.timelineMode == "timing",
        },
        new TutorialStep()
        {
            Content = "The Timeline will now display the BPM stops of the song here.\n\n"
                + "If you just created the song, there should be a 140 BPM stop at the beginning of the song. We aren't sure if the song being charted is actually at 140 BPM, so it's good to know how to change it in case it isn't.",
            PopupPosition = new Vector2(0, -260),
            PopupAnchor = new Vector2(.5f, 1),
        },
        new TutorialStep()
        {
            Content = "You might also noticed the Picker now showing an item called \"STP\".\n\n"
                + "You can create another BPM stop by selecting it and placing it in the Timeline, in case you song has BPM changes. That should work for anything that'll be displayed here too.",
            PopupPosition = new Vector2(215, -80),
            PopupAnchor = new Vector2(0, .5f),
        },
        new TutorialStep()
        {
            Content = "To edit a BPM stop, bring it to the Inspector by selecting it on the Timeline. You'll also be doing this for anything that appears here too.",
            PopupPosition = new Vector2(0, -260),
            PopupAnchor = new Vector2(.5f, 1),
            RequirementText = "Select the BPM Stop on the Timeline",
            RequirementFunction = x => x.TargetThing is BPMStop,
        },
        new TutorialStep()
        {
            Content = "The Inspector will display information about the BPM stop here.\n\n"
                + "Since this item has a time placement, there'll be a number field in the top right corner to insert the time (in seconds). That's the \"offset\" value of the stop.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "The BPM field is the speed of the songs in beats per minute (duh!), and the Signature field indicates how many beats to form a bar.\n\n"
                + "Note that in the Signature field the lower number (beat unit) is unnecessary and only the upper number is needed (e.g. type 3 instead of 3/4).",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "Quick tip: You can enable the audio metronome by opening this drop down near the Play/Pause button and toggle the Metronome button.\n\n"
                + "There are also more features in the drop down that can help you in your charting too!",
            PopupPosition = new Vector2(25, 120),
            PopupAnchor = new Vector2(.5f, 0),
        },
        new TutorialStep()
        {
            Content = "Now go edit the BPM, offset and signature of the song!\n\n"
                + "Press the Play button to play the song. Click it again to pause it.\n\n"
                + "Click Continue if you think the song is synced (when the metronome sounds are in sync with the song when enabled).",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "Now that's we synced the music, let's populate the chart!\n\n"
                + "J.A.N.O.A.R.G. charts are made from lanes that can move, rotate, and resize, and notes are placed on them. Let's create one!",
            PopupPosition = new Vector2(180, -120),
            PopupAnchor = new Vector2(0, 1),
            RequirementText = "Click the Lane tab to open the Lane editor.",
            RequirementFunction = x => x.timelineMode == "lane",
        },
        new TutorialStep()
        {
            Content = "This is the Lane editor. All your Lanes will be displayed here.\n\n"
                + "To create a Lane, select \"LNE\" from the Picker and place it on the Timeline, or if a lane is present, you can click on it to select it.\n\n",
            RequirementText = "Create/Select a Lane",
            RequirementFunction = x => x.TargetThing is Lane,
            PopupPosition = new Vector2(0, -260),
            PopupAnchor = new Vector2(.5f, 1),
        },
        new TutorialStep()
        {
            Content = "This is the Lane's Inspector screen, which might be confusing at first, but we'll try to unterstand it anyways.\n\n"
                + "The Transform section contain rules that moves the entire lane at once, where the Lane Step section defines specific lane shapes.\n\n"
                + "The Appearance determines how the lane looks, we'll leave it as is for now.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "The Lane Step format looks like this:\n"
                + "Offset (beats) | Speed\n"
                + "Start X | Start X Ease | Start Y | Start Y Ease\n"
                + "End X | End X Ease | End Y | End Y Ease\n\n"
                + "You can click on the ⋮ to expand on that lane step or click x to delete it.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "About the coordination system: The game uses the left handed coordination system with positive X = right, positive Y = up, and positive Z = forward. "
                + "If you use Unity you probably won't need this but I heard that some people are having trouble with this so here you go.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "As before, I'll let you use you lanes to your liking.\n\n"
                + "Click Continue when you're finished.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "Now the lanes are completed, let's add some gameplay!\n\n"
                + "Here notes are called \"hit objects\" or just simply \"hits\" and these are instructions for players to hit them, just like other rhythm games. Use the Hits tab to create some of them, which just showed up because you selected a Lane.",
            PopupPosition = new Vector2(180, -120),
            PopupAnchor = new Vector2(0, 1),
            RequirementText = "Click the Hits tab to open the Hit editor.",
            RequirementFunction = x => x.timelineMode == "hit",
        },
        new TutorialStep()
        {
            Content = "The Hits tabs shows the Timeline of the hit objects that are on the Lane that you selected.\n\n"
                + "One again, you can select an item and place it here on the Timeline, but there will be different type of notes as well, so here are the abbreviations means:\n"
                + "NOR = Normal notes, CAT = Catch notes",
            PopupPosition = new Vector2(0, -260),
            PopupAnchor = new Vector2(.5f, 1),
            RequirementText = "Create/Select a Hit Object",
            RequirementFunction = x => x.TargetThing is HitObject
        },
        new TutorialStep()
        {
            Content = "The Hit Object also has its Inspector panel too, and it also has the time input on the top right corner to set its placement time.\n\n"
                + "The Position and Length parameters determines where and how long the note will be, or you can just use the knobs and the slider to change it.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "The Hold Length parameter determines how long the note needs to be hold to be fully cleared.\n\n"
                + "Similar to Lanes, Hit Objects also has an Appearance category that determines how they're shown, but we also leave that to you to change.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "It's time for you to do the charting! Use what you've learned to create a pattern that you like!\n\n"
                + "Hit \"Continue\" when you're ready to continue. There'll be more to discover.",
            PopupPosition = new Vector2(0, 120),
            PopupAnchor = new Vector2(.5f, 0),
        },
        new TutorialStep()
        {
            Content = "Let's take a tour on the more complex data of the chart.\n\n"
                + "Click on the \"Pallete\" button to open the Pallete.",
            PopupPosition = new Vector2(-175, -120),
            PopupAnchor = new Vector2(1, 1),
            RequirementText = "Open the Pallete",
            RequirementFunction = x => x.TargetThing is Pallete,
        },
        new TutorialStep()
        {
            Content = "The Pallete will determine how your chart will look.\n\n"
                + "You can set the Background and the Interface's color get access to the styles here.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
            RequirementText = "Open a Style by clicking on one to continue",
            RequirementFunction = x => x.TargetThing is LaneStyle || x.TargetThing is HitStyle,
        },
        new TutorialStep()
        {
            Content = "There are also style groups for Lanes and Hit Objects located in the Pallete. Here you can set the color scheme and even change the feel by using the Material system!\n\n"
                + "Note that Materials and Color Targets are advanced features for Unity users, if you don't know what you're doing you should probably leave them as is.",
            PopupPosition = new Vector2(-420, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "All right, I've been saving this one for the last!\n\n"
                + "Have you noticed a tab on the Inspector called \"Storyboard\"? Let's click on it →",
            PopupPosition = new Vector2(-445, -80),
            PopupAnchor = new Vector2(1, .5f),
            RequirementText = "Open the Storyboard tab on the Inspector",
            RequirementFunction = x => x.inspectMode == "storyboard"
        },
        new TutorialStep()
        {
            Content = "Most of the numbers on items can be animated! That'll surely add a lot of complexity and creativity to our charts!\n\n"
                + "You can add a new Timestamp by using the \"+\" on the top right corner...",
            PopupPosition = new Vector2(-440, -80),
            PopupAnchor = new Vector2(1, .5f),
        },
        new TutorialStep()
        {
            Content = "...or by using the dedicated Storyboard tab on the Timeline.\n\n"
                + "Either way, they will display the Timestamp controlled by the item that you selected earlier, so make sure you select an item first!",
            PopupPosition = new Vector2(180, -120),
            PopupAnchor = new Vector2(0, 1),
        },
        new TutorialStep()
        {
            Content = "That concludes the tutorial! I hope you've gotten familliar with the editor.\n\n"
                + "If you have any questions, feel free to leave it on our Discord server listed on the GitHub repository.\n"
                + "(You should know where our GitHub repo is since you downloaded the editor there, right?)",
        },
    };

    public void Tutorial(int id)
    {
        TutorialStep step = TutorialSteps[TutorialStage];

        if (TutorialStage > 0 && TutorialLerp < 1)
        {
            TutorialLerp += Time.deltaTime / 2;
            TutorialStep prev = TutorialSteps[TutorialStage - 1];
            float ease = Ease.Get(TutorialLerp, "Quadratic", EaseMode.Out);
            TutorialPopupAnchor = Vector2.LerpUnclamped(prev.PopupAnchor, step.PopupAnchor, ease);
            TutorialPopupPosition = Vector2.LerpUnclamped(prev.PopupPosition, step.PopupPosition, ease);
            Repaint();
        }
        else
        {
            TutorialPopupAnchor = step.PopupAnchor;
            TutorialPopupPosition = step.PopupPosition;
        }

        GUIStyle itemStyle = new GUIStyle("label");
        itemStyle.wordWrap = true;
        itemStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(20, 0, 300, 156), step.Content, itemStyle);

        if (GUI.Button(new Rect(4, 156, 68, 20), "Skip"))
        {
            TutorialStage = -1;
        }

        if (step.RequirementText != null)
        {
            GUI.Label(new Rect(76, 156, 260, 20), step.RequirementText, itemStyle);
            if (step.RequirementFunction(this))
            {
                TutorialStage = TutorialStage < TutorialSteps.Length - 1 ? TutorialStage + 1 : -1;
                TutorialLerp = 0;
            }
        }
        else if (GUI.Button(new Rect(76, 156, 260, 20), TutorialStage < TutorialSteps.Length - 1 ? "Continue →" : "Complete!"))
        {
            TutorialStage = TutorialStage < TutorialSteps.Length - 1 ? TutorialStage + 1 : -1;
            TutorialLerp = 0;
        }
    }

    #endregion
}
