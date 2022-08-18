using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Globalization;

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
    public Chart TargetChart;
    public Lane TargetLane;
    public object TargetThing;
    public object DeletingThing;
    public Timestamp TargetTimestamp;

    public AudioSource CurrentAudioSource;
    public Camera CurrentCamera;
    public AudioClip MetronomeSound;
    public AudioClip NormalHitSound;
    public AudioClip CatchHitSound;

    public bool PlayMetronome;
    public bool SeparateUnits;
    public bool PlayHitsounds;

    public List<LaneStyleManager> LaneStyleManagers = new List<LaneStyleManager>();
    public List<HitStyleManager> HitStyleManagers = new List<HitStyleManager>();
    public List<Mesh> Meshes = new List<Mesh>();

    CultureInfo invariant = CultureInfo.InvariantCulture;

    float ScrollSpeed = 121;

    float width, height, pos, dec, beat, currentBeat, bar, min, sec, ms;

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
        Mesh mesh = new Mesh();

        float pos = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        void AddStep(Vector3 start, Vector3 end) {

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

        float curtime = CurrentAudioSource.time;

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
            if (CurrentAudioSource.time > time) 
            {
                if (a >= steps.Count - 1) {
                    lastP = 1;
                    continue;
                }
                LaneStep next = steps[a + 1];
                float nexttime = TargetSong.Timing.ToSeconds(next.Offset);
                if (CurrentAudioSource.time > nexttime) 
                {
                    lastP = 1;
                    continue;
                }
                p = (curtime - time) / (nexttime - time);
                // Debug.Log("P " + a + " " + p);
                start = new Vector2(Mathf.Lerp(step.StartPos.x, next.StartPos.x, Ease.Get(p, next.StartEaseX, next.StartEaseXMode)),
                    Mathf.Lerp(step.StartPos.y, next.StartPos.y, Ease.Get(p, next.StartEaseY, next.StartEaseYMode)));
                end = new Vector2(Mathf.Lerp(step.EndPos.x, next.EndPos.x, Ease.Get(p, next.EndEaseX, next.EndEaseXMode)),
                    Mathf.Lerp(step.EndPos.y, next.EndPos.y, Ease.Get(p, next.EndEaseY, next.EndEaseYMode)));
            }

            float lPos = pos;
            pos += step.Speed * ScrollSpeed * (Mathf.Max(time, CurrentAudioSource.time) - curtime);
            curtime = Mathf.Max(time, CurrentAudioSource.time);
            if (a == 0) 
            {
                AddStep(new Vector3(start.x, start.y, pos), new Vector3(end.x, end.y, pos));
            }
            else 
            {
                LaneStep prev = steps[a - 1];
                if (lastP >= 1 || (step.StartEaseX == "Linear" && step.StartEaseY == "Linear" &&
                    step.EndEaseX == "Linear" && step.EndEaseY == "Linear"))
                {
                    AddStep(new Vector3(start.x, start.y, pos), new Vector3(end.x, end.y, pos));
                }
                else
                {
                    // Debug.Log("T " + step.StartEaseX + " " + p);
                    for (float x = lastP; x <= 1; x = Mathf.Floor(x * 16 + 1.01f) / 16)
                    {
                        float cPos = Mathf.Lerp(lPos, pos, (x - lastP) / (1 - lastP));
                        start = new Vector3(Mathf.Lerp(prev.StartPos.x, step.StartPos.x, Ease.Get(x, step.StartEaseX, step.StartEaseXMode)),
                            Mathf.Lerp(prev.StartPos.y, step.StartPos.y, Ease.Get(x, step.StartEaseY, step.StartEaseYMode)), cPos);
                        end = new Vector3(Mathf.Lerp(prev.EndPos.x, step.EndPos.x, Ease.Get(x, step.EndEaseX, step.EndEaseXMode)),
                            Mathf.Lerp(prev.EndPos.y, step.EndPos.y, Ease.Get(x, step.EndEaseY, step.EndEaseYMode)), cPos);
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

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        if (Mathf.Abs(step.Offset) > 5) return mesh;

        void AddStep(Vector3 start, Vector3 end, bool addTris = true) {

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
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        void AddStep(Vector3 start, Vector3 end) {

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

        float curtime = CurrentAudioSource.time;

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

            if (startPos >= 1) { 
                curtime = Mathf.Max(nTime, curtime);
                p = nextP;
                continue;
            }


            if (vertices.Count == 0)
            {
                if (next.StartEaseX == "Linear" && next.StartEaseY == "Linear" && next.EndEaseX == "Linear" && next.EndEaseY == "Linear")
                {
                    start = Vector2.Lerp(step.StartPos, next.StartPos, startPos);
                    end = Vector2.Lerp(step.EndPos, next.EndPos, startPos);
                }
                else 
                {
                    start = new Vector3(Mathf.Lerp(step.StartPos.x, next.StartPos.x, Ease.Get(Mathf.Clamp01(startPos), next.StartEaseX, next.StartEaseXMode)),
                        Mathf.Lerp(step.StartPos.y, next.StartPos.y, Ease.Get(Mathf.Clamp01(startPos), next.StartEaseY, next.StartEaseYMode)));
                    end = new Vector3(Mathf.Lerp(step.EndPos.x, next.EndPos.x, Ease.Get(Mathf.Clamp01(startPos), next.EndEaseX, next.EndEaseXMode)),
                        Mathf.Lerp(step.EndPos.y, next.EndPos.y, Ease.Get(Mathf.Clamp01(startPos), next.EndEaseY, next.EndEaseYMode)));
                }
                Vector2 s = Vector2.Lerp(start, end, hit.Position);
                Vector2 e = Vector2.Lerp(start, end, hit.Position + hit.Length);
                float pp = p + ((nTime - sTime) * startPos - Math.Max(curtime - sTime, 0)) * step.Speed * ScrollSpeed;
                AddStep(new Vector3(s.x, s.y, pp), new Vector3(e.x, e.y, pp));
            }
            {
                if (next.StartEaseX == "Linear" && next.StartEaseY == "Linear" && next.EndEaseX == "Linear" && next.EndEaseY == "Linear")
                {
                    start = Vector2.Lerp(step.StartPos, next.StartPos, endPos);
                    end = Vector2.Lerp(step.EndPos, next.EndPos, endPos);
                    Vector2 s = Vector2.Lerp(start, end, hit.Position);
                    Vector2 e = Vector2.Lerp(start, end, hit.Position + hit.Length);
                    float pp = p + ((nTime - sTime) * endPos - Math.Max(curtime - sTime, 0)) * step.Speed * ScrollSpeed;
                    AddStep(new Vector3(s.x, s.y, pp), new Vector3(e.x, e.y, pp));
                }
                else 
                {

                    void Add(float pos) 
                    {
                        start = new Vector3(Mathf.Lerp(step.StartPos.x, next.StartPos.x, Ease.Get(pos, next.StartEaseX, next.StartEaseXMode)),
                            Mathf.Lerp(step.StartPos.y, next.StartPos.y, Ease.Get(pos, next.StartEaseY, next.StartEaseYMode)));
                        end = new Vector3(Mathf.Lerp(step.EndPos.x, next.EndPos.x, Ease.Get(pos, next.EndEaseX, next.EndEaseXMode)),
                            Mathf.Lerp(step.EndPos.y, next.EndPos.y, Ease.Get(pos, next.EndEaseY, next.EndEaseYMode)));
                        Vector2 s = Vector2.Lerp(start, end, hit.Position);
                        Vector2 e = Vector2.Lerp(start, end, hit.Position + hit.Length);
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

    #endregion

    /////////////////////
    #region Main GUI Loop
    /////////////////////

    int NormalCount, CatchCount;

    string GizmoMode = "";

    public void OnGUI()
    {
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

        if (TargetSong) {
            pos = TargetSong.Timing.ToBeat(CurrentAudioSource.time);
            dec = Mathf.Floor((pos % 1) * 1000);
            beat = Mathf.Floor(TargetSong.Timing.ToDividedBeat(CurrentAudioSource.time));
            bar = Mathf.Floor(TargetSong.Timing.ToBar(CurrentAudioSource.time));
            
            min = Mathf.Floor(CurrentAudioSource.time / 60);
            sec = Mathf.Floor(CurrentAudioSource.time % 60);
            ms = Mathf.Floor((CurrentAudioSource.time % 1) * 1000);
            
            if ((TargetThing is PlayableSong && TargetThing != (object)TargetSong) || 
                (TargetThing is Chart && TargetThing != (object)TargetChart)) 
                TargetThing = null;
            if (TargetSong.Charts.IndexOf(TargetChart) < 0) TargetChart = null;
            if (TargetChart == null || TargetChart.Lanes.IndexOf(TargetLane) < 0) TargetLane = null;

            Rect bound = new Rect(45, 35, width - 320, height - 202);
            if (bound.width / bound.height > 3 / 2f) 
            {
                float width = (bound.height * 3 / 2);
                bound.x = bound.x + (bound.width - width) / 2;
                bound.width = width;
            } 
            else {
                float height = (bound.width / 3 * 2);
                bound.y = bound.y + (bound.height - height) / 2;
                bound.height = height;
            }

            float camLeft = (bound.center.x - (width - bound.center.x));
            float camRatio = (bound.height / (height - 184));

            int ncount = 0, ccount = 0;
            Vector3 startPos = Vector3.zero, endPos = Vector3.zero;

            if (TargetChart != null) 
            {
                Chart chart = (Chart)TargetChart.Get(pos);
                Pallete pal = (Pallete)chart.Pallete.Get(pos);

                CurrentCamera.transform.position = chart.CameraPivot;
                CurrentCamera.transform.eulerAngles = chart.CameraRotation;
                CurrentCamera.transform.Translate(Vector3.back * 10);
                CurrentCamera.fieldOfView = Mathf.Atan2(Mathf.Tan(30 * Mathf.Deg2Rad), camRatio) * 2 * Mathf.Rad2Deg;
                RenderSettings.fogColor = CurrentCamera.backgroundColor = pal.BackgroundColor;
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

                foreach (Lane lane in chart.Lanes)
                {
                    if (lane.StyleIndex >= 0 && lane.StyleIndex < LaneStyleManagers.Count) 
                    {
                        if (LaneStyleManagers[lane.StyleIndex].LaneMaterial)
                        {
                            Mesh mesh = MakeLaneMesh(lane);
                            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, LaneStyleManagers[lane.StyleIndex].LaneMaterial, 0, CurrentCamera);
                            Meshes.Add(mesh);
                        }
                    }
                    foreach (HitObject hit in lane.Objects)
                    {
                        bool valid = hit.StyleIndex >= 0 && hit.StyleIndex < HitStyleManagers.Count;
                        if (hit.Offset > pos)
                        {
                            if (valid) 
                            {
                                Material mat = HitStyleManagers[hit.StyleIndex].NormalMaterial;
                                if (hit.Type == HitObject.HitType.Catch) mat = HitStyleManagers[hit.StyleIndex].CatchMaterial;
                                if (mat) 
                                {
                                    Mesh mesh = MakeHitMesh(hit, lane, out Vector3 sp, out Vector3 ep);
                                    if (TargetThing == hit) {
                                        startPos = sp;
                                        endPos = ep;
                                    }
                                    Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, mat, 0, CurrentCamera);
                                    Meshes.Add(mesh);
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
                                Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, HitStyleManagers[hit.StyleIndex].HoldTailMaterial, 0, CurrentCamera);
                                Meshes.Add(mesh);
                            }
                            if (hit.Type == HitObject.HitType.Catch) ccount++;
                            else ncount++;
                        }
                    } 
                }
                EditorGUI.DrawRect(new Rect(0, 0, width, height), CurrentCamera.backgroundColor);
                Handles.DrawCamera(new Rect(0, 26, width + camLeft, height - 184), CurrentCamera);
                Handles.color = pal.InterfaceColor;
                Handles.DrawPolyLine(new Vector2(bound.x, bound.y), new Vector2(bound.x + bound.width, bound.y), 
                    new Vector2(bound.x + bound.width, bound.y + bound.height), new Vector2(bound.x, bound.y + bound.height),
                    new Vector2(bound.x, bound.y));

                // Handles
                if (CurrentAudioSource.isPlaying)
                {
                    // Don't show anything in play mode
                }
                else if (TargetThing is Lane) 
                {
                    Lane lane = (Lane)TargetThing;
                    LaneStep step = lane.LaneSteps[0];
                    if (step.Offset > pos)
                    {
                        LaneStep dat = TargetLane.GetLaneStep(step.Offset, pos, TargetSong.Timing);
                        float zPos = dat.Offset * ScrollSpeed;
                        startPos = (Vector3)step.StartPos + Vector3.forward * zPos;
                        endPos = (Vector3)step.EndPos + Vector3.forward * zPos;
                        Vector3 midPos = (startPos + endPos) / 2;
                        
                        Vector3 startPosCamera = CurrentCamera.WorldToScreenPoint(startPos);
                        startPosCamera = new Vector2(Mathf.Round(startPosCamera.x), Mathf.Round(height - startPosCamera.y));
                        Vector3 midPosCamera = CurrentCamera.WorldToScreenPoint(midPos);
                        midPosCamera = new Vector2(Mathf.Round(midPosCamera.x), Mathf.Round(height - midPosCamera.y));
                        Vector3 endPosCamera = CurrentCamera.WorldToScreenPoint(endPos);
                        endPosCamera = new Vector2(Mathf.Round(endPosCamera.x), Mathf.Round(height - endPosCamera.y));
                        
                        Vector2 fwd = Quaternion.Euler(0, 0, -Mathf.Atan2(endPosCamera.x - startPosCamera.x, endPosCamera.y - startPosCamera.y) * Mathf.Rad2Deg) * Vector3.up;

                        Handles.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                        Handles.DrawLine(startPosCamera + Vector3.back * 3, endPosCamera + Vector3.back * 3, 3);
                        Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        Handles.DrawLine(startPosCamera + Vector3.back * 1, endPosCamera + Vector3.back * 1, 1);

                        Handles.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                        if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosCamera, Vector3.forward, Vector3.up, 360 * 59 / 4, 9);
                        if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosCamera, Vector3.forward, Vector3.up, 360, 9);
                        if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosCamera, Vector3.forward, fwd, 360 * 59 / 3, 9.5f);

                        Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosCamera, Vector3.forward, Vector3.up, 360 * 59 / 4, 7);
                        if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosCamera, Vector3.forward, Vector3.up, 360, 7);
                        if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosCamera, Vector3.forward, fwd, 360 * 59 / 3, 7);
                    
                        
                        Rect startRect = new Rect(startPosCamera.x - 8, startPosCamera.y - 8, 16, 16);
                        Rect midRect = new Rect(midPosCamera.x - 8, midPosCamera.y - 8, 16, 16);
                        Rect endRect = new Rect(endPosCamera.x - 8, endPosCamera.y - 8, 16, 16);
                        Vector2 mousePos = Event.current.mousePosition;

                        if (Event.current.type == EventType.MouseDown) 
                        {
                            if (startRect.Contains(mousePos)) GizmoMode = "start";
                            else if (midRect.Contains(mousePos)) GizmoMode = "mid";
                            else if (endRect.Contains(mousePos)) GizmoMode = "end";
                            if (GizmoMode != "") Repaint();
                        }
                        else if (Event.current.type == EventType.MouseDrag) 
                        {
                            if (GizmoMode != "")
                            {
                                Vector2 curPos = Vector2.zero;
                                if (GizmoMode == "start")
                                {
                                    curPos = step.StartPos;
                                }
                                else if (GizmoMode == "mid") 
                                {
                                    curPos = (step.StartPos + step.EndPos) / 2;
                                }
                                else if (GizmoMode == "end") 
                                {
                                    curPos = step.EndPos;
                                }

                                Ray ray = CurrentCamera.ScreenPointToRay(new Vector2(mousePos.x, height - mousePos.y));
                                bool valid = new Plane(Vector3.back, (Vector3)curPos + Vector3.forward * zPos).Raycast(ray, out float enter);
                                if (valid) 
                                {
                                    Vector2 newPos = ray.GetPoint(enter);
                                    newPos = new Vector2(Mathf.Round(newPos.x / .001f) * .001f, Mathf.Round(newPos.y / .001f) * .001f);

                                    if (GizmoMode == "start")
                                    {
                                        step.StartPos = newPos;
                                    }
                                    else if (GizmoMode == "mid") 
                                    {
                                        Vector2 offset = newPos - curPos;
                                        step.StartPos += offset;
                                        step.EndPos += offset;
                                    }
                                    else if (GizmoMode == "end") 
                                    {
                                        step.EndPos = newPos;
                                    }

                                    Repaint();
                                }
                            }
                        } 
                        if (Event.current.type == EventType.MouseUp) 
                        {
                            if (GizmoMode != "") Repaint();
                            GizmoMode = "";
                        }
                    }
                }
                else if (TargetThing is LaneStep) 
                {
                    LaneStep step = (LaneStep)TargetThing;
                    if (step.Offset > pos)
                    {
                        LaneStep dat = TargetLane.GetLaneStep(step.Offset, pos, TargetSong.Timing);
                        float zPos = dat.Offset * ScrollSpeed;
                        startPos = (Vector3)step.StartPos + Vector3.forward * zPos;
                        endPos = (Vector3)step.EndPos + Vector3.forward * zPos;
                        Vector3 midPos = (startPos + endPos) / 2;
                        
                        Vector3 startPosCamera = CurrentCamera.WorldToScreenPoint(startPos);
                        startPosCamera = new Vector2(Mathf.Round(startPosCamera.x), Mathf.Round(height - startPosCamera.y));
                        Vector3 midPosCamera = CurrentCamera.WorldToScreenPoint(midPos);
                        midPosCamera = new Vector2(Mathf.Round(midPosCamera.x), Mathf.Round(height - midPosCamera.y));
                        Vector3 endPosCamera = CurrentCamera.WorldToScreenPoint(endPos);
                        endPosCamera = new Vector2(Mathf.Round(endPosCamera.x), Mathf.Round(height - endPosCamera.y));
                        
                        Vector2 fwd = Quaternion.Euler(0, 0, -Mathf.Atan2(endPosCamera.x - startPosCamera.x, endPosCamera.y - startPosCamera.y) * Mathf.Rad2Deg) * Vector3.up;

                        Handles.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                        Handles.DrawLine(startPosCamera + Vector3.back * 3, endPosCamera + Vector3.back * 3, 3);
                        Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        Handles.DrawLine(startPosCamera + Vector3.back * 1, endPosCamera + Vector3.back * 1, 1);

                        Handles.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                        if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosCamera, Vector3.forward, Vector3.up, 360 * 59 / 4, 9);
                        if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosCamera, Vector3.forward, Vector3.up, 360, 9);
                        if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosCamera, Vector3.forward, fwd, 360 * 59 / 3, 9.5f);

                        Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosCamera, Vector3.forward, Vector3.up, 360 * 59 / 4, 7);
                        if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosCamera, Vector3.forward, Vector3.up, 360, 7);
                        if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosCamera, Vector3.forward, fwd, 360 * 59 / 3, 7);
                    
                        
                        Rect startRect = new Rect(startPosCamera.x - 8, startPosCamera.y - 8, 16, 16);
                        Rect midRect = new Rect(midPosCamera.x - 8, midPosCamera.y - 8, 16, 16);
                        Rect endRect = new Rect(endPosCamera.x - 8, endPosCamera.y - 8, 16, 16);
                        Vector2 mousePos = Event.current.mousePosition;

                        if (Event.current.type == EventType.MouseDown) 
                        {
                            if (startRect.Contains(mousePos)) GizmoMode = "start";
                            else if (midRect.Contains(mousePos)) GizmoMode = "mid";
                            else if (endRect.Contains(mousePos)) GizmoMode = "end";
                            if (GizmoMode != "") Repaint();
                        }
                        else if (Event.current.type == EventType.MouseDrag) 
                        {
                            if (GizmoMode != "")
                            {
                                Vector2 curPos = Vector2.zero;
                                if (GizmoMode == "start")
                                {
                                    curPos = step.StartPos;
                                }
                                else if (GizmoMode == "mid") 
                                {
                                    curPos = (step.StartPos + step.EndPos) / 2;
                                }
                                else if (GizmoMode == "end") 
                                {
                                    curPos = step.EndPos;
                                }

                                Ray ray = CurrentCamera.ScreenPointToRay(new Vector2(mousePos.x, height - mousePos.y));
                                bool valid = new Plane(Vector3.back, (Vector3)curPos + Vector3.forward * zPos).Raycast(ray, out float enter);
                                if (valid) 
                                {
                                    Vector2 newPos = ray.GetPoint(enter);
                                    newPos = new Vector2(Mathf.Round(newPos.x / .001f) * .001f, Mathf.Round(newPos.y / .001f) * .001f);

                                    if (GizmoMode == "start")
                                    {
                                        step.StartPos = newPos;
                                    }
                                    else if (GizmoMode == "mid") 
                                    {
                                        Vector2 offset = newPos - curPos;
                                        step.StartPos += offset;
                                        step.EndPos += offset;
                                    }
                                    else if (GizmoMode == "end") 
                                    {
                                        step.EndPos = newPos;
                                    }

                                    Repaint();
                                }
                            }
                        } 
                        if (Event.current.type == EventType.MouseUp) 
                        {
                            if (GizmoMode != "") Repaint();
                            GizmoMode = "";
                        }
                    }
                }
                else if (TargetThing is HitObject) 
                {
                    HitObject hit = (HitObject)TargetThing;
                    if (hit.Offset > pos)
                    {
                        Vector3 midPos = (startPos + endPos) / 2;
                        
                        Vector3 startPosCamera = CurrentCamera.WorldToScreenPoint(startPos);
                        startPosCamera = new Vector2(Mathf.Round(startPosCamera.x), Mathf.Round(height - startPosCamera.y));
                        Vector3 midPosCamera = CurrentCamera.WorldToScreenPoint(midPos);
                        midPosCamera = new Vector2(Mathf.Round(midPosCamera.x), Mathf.Round(height - midPosCamera.y));
                        Vector3 endPosCamera = CurrentCamera.WorldToScreenPoint(endPos);
                        endPosCamera = new Vector2(Mathf.Round(endPosCamera.x), Mathf.Round(height - endPosCamera.y));

                        Vector2 fwd = Quaternion.Euler(0, 0, -Mathf.Atan2(endPosCamera.x - startPosCamera.x, endPosCamera.y - startPosCamera.y) * Mathf.Rad2Deg) * Vector3.up;

                        Handles.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                        if (GizmoMode == "") Handles.DrawWireArc((startPosCamera + endPosCamera) / 2 + Vector3.back * 3, Vector3.forward, Vector3.up, 360, Vector2.Distance(startPosCamera, endPosCamera) / 2, 3);
                        Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        if (GizmoMode == "") Handles.DrawWireArc((startPosCamera + endPosCamera) / 2 + Vector3.back * 1, Vector3.forward, Vector3.up, 360, Vector2.Distance(startPosCamera, endPosCamera) / 2, 1);

                        Handles.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                        if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosCamera, Vector3.forward, Vector3.up, 360 * 59 / 4, 9);
                        if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosCamera, Vector3.forward, Vector3.up, 360, 9);
                        if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosCamera, Vector3.forward, fwd, 360 * 59 / 3, 9.5f);

                        Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        if (GizmoMode == "" || GizmoMode == "start") Handles.DrawSolidArc(startPosCamera, Vector3.forward, Vector3.up, 360 * 59 / 4, 7);
                        if (GizmoMode == "" || GizmoMode == "mid") Handles.DrawSolidArc(midPosCamera, Vector3.forward, Vector3.up, 360, 7);
                        if (GizmoMode == "" || GizmoMode == "end") Handles.DrawSolidArc(endPosCamera, Vector3.forward, fwd, 360 * 59 / 3, 7);


                        Rect startRect = new Rect(startPosCamera.x - 8, startPosCamera.y - 8, 16, 16);
                        Rect midRect = new Rect(midPosCamera.x - 8, midPosCamera.y - 8, 16, 16);
                        Rect endRect = new Rect(endPosCamera.x - 8, endPosCamera.y - 8, 16, 16);
                        Vector2 mousePos = Event.current.mousePosition;

                        if (Event.current.type == EventType.MouseDown) 
                        {
                            if (startRect.Contains(mousePos)) GizmoMode = "start";
                            else if (midRect.Contains(mousePos)) GizmoMode = "mid";
                            else if (endRect.Contains(mousePos)) GizmoMode = "end";
                            if (GizmoMode != "") Repaint();
                        }
                        else if (Event.current.type == EventType.MouseDrag) 
                        {
                            if (GizmoMode != "")
                            {
                                LaneStep step = TargetLane.GetLaneStep(hit.Offset, pos, TargetSong.Timing);
                                Debug.Log("HO " + hit.Offset + " " + CurrentAudioSource.time + " " + step.Offset);
                                float stop = 0;
                                float stopDist = float.PositiveInfinity;

                                float curPos = 0, minPos = 0, maxPos = 0;
                                if (GizmoMode == "start")
                                {
                                    curPos = hit.Position;
                                    minPos = 0;
                                    maxPos = hit.Position + hit.Length;
                                }
                                else if (GizmoMode == "mid") 
                                {
                                    curPos = hit.Position + hit.Length / 2;
                                    minPos = hit.Length / 2;
                                    maxPos = 1 - hit.Length / 2;
                                }
                                else if (GizmoMode == "end") 
                                {
                                    curPos = hit.Position + hit.Length;
                                    minPos = hit.Position;
                                    maxPos = 1;
                                }

                                for (float p = Mathf.Ceil(minPos / .05f) * .05f + (curPos % .05f); p <= maxPos; p = (Mathf.Round(p / .05f) + 1) * .05f)
                                {
                                    Vector3 wldPos = Vector3.LerpUnclamped(step.StartPos, step.EndPos, p) + Vector3.forward * (startPos.z + endPos.z) / 2;
                                    Vector2 camPos = CurrentCamera.WorldToScreenPoint(wldPos);
                                    camPos = new Vector2(Mathf.Round(camPos.x), Mathf.Round(height - camPos.y));
                                    
                                    Handles.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                                    Handles.DrawSolidArc(startPosCamera, Vector3.forward, Vector3.up, 360, 2);
                                    Handles.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                                    Handles.DrawSolidArc(startPosCamera, Vector3.forward, Vector3.up, 360, 1);

                                    float dist = Vector2.Distance(Event.current.mousePosition, camPos);
                                    // Debug.Log(p + " " + dist + " " + wldPos + " " + camPos);
                                    if (dist < stopDist) 
                                    {
                                        stop = p;
                                        stopDist = dist;
                                    }
                                }

                                if (GizmoMode == "start")
                                {
                                    hit.Length = hit.Position - stop + hit.Length;
                                    hit.Position = stop;
                                }
                                else if (GizmoMode == "mid") 
                                {
                                    hit.Position = stop - hit.Length / 2;
                                }
                                else if (GizmoMode == "end") 
                                {
                                    hit.Length = stop - hit.Position;
                                }

                                hit.Position = Mathf.Round(hit.Position / .05f) * .05f;
                                hit.Length = Mathf.Round(hit.Length / .05f) * .05f;

                                Repaint();
                            }
                        } 
                        if (Event.current.type == EventType.MouseUp) 
                        {
                            if (GizmoMode != "") Repaint();
                            GizmoMode = "";
                        }
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

            foreach (Mesh mesh in Meshes) DestroyImmediate(mesh);
            Meshes = new List<Mesh>();
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
                if (extrasmode == "play_options") rect = new Rect(width / 2 + 17, 30, 300, 120);
                if (extrasmode == "main_menu") rect = new Rect(3, 30, 150, 300);

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

            GUI.Window(2, new Rect(-2, height - 158, width + 4, 160), Timeline, "");

            GUI.Window(3, new Rect(width - 270, 36, height - 204, height - 204), InspectMode, "", new GUIStyle("button") { clipping = TextClipping.Overflow });
            GUI.BringWindowToBack(3);

            GUI.Window(4, new Rect(width - 245, 32, 240, height - 196), Inspector, "");

            GUI.Window(5, new Rect(5, 32, 32, height - 196), Picker, "");
        }
        else 
        {
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

        if (CurrentAudioSource.isPlaying) {
            if (currentBeat != Mathf.Floor(pos)) {
                currentBeat = Mathf.Floor(pos);
                if (PlayMetronome) CurrentAudioSource.PlayOneShot(MetronomeSound);
            }
            Repaint();
        }

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
            else if (Event.current == CharterSettings.Keybinds["Selection/Previous Item"])
            {
                if (TargetThing is Lane) TargetThing = 
                    TargetChart.Lanes[Math.Max(TargetChart.Lanes.IndexOf((Lane)TargetThing) - 1, 0)];
                else if (TargetThing is HitObject) TargetThing = 
                    TargetLane.Objects[Math.Max(TargetLane.Objects.IndexOf((HitObject)TargetThing) - 1, 0)];
            }
            else if (Event.current == CharterSettings.Keybinds["Selection/Next Item"])
            {
                if (TargetThing is Lane) TargetThing = 
                    TargetChart.Lanes[Math.Min(TargetChart.Lanes.IndexOf((Lane)TargetThing) + 1, TargetChart.Lanes.Count - 1)];
                else if (TargetThing is HitObject) TargetThing = 
                    TargetLane.Objects[Math.Min(TargetLane.Objects.IndexOf((HitObject)TargetThing) + 1, TargetLane.Objects.Count - 1)];
            }
            else if (Event.current == CharterSettings.Keybinds["Selection/Previous Lane"])
            {
                if (TargetLane != null) TargetThing = TargetLane = 
                    TargetChart.Lanes[Math.Max(TargetChart.Lanes.IndexOf(TargetLane) - 1, 0)];
            }
            else if (Event.current == CharterSettings.Keybinds["Selection/Next Lane"])
            {
                if (TargetLane != null) TargetThing = TargetLane = 
                    TargetChart.Lanes[Math.Min(TargetChart.Lanes.IndexOf(TargetLane) + 1, TargetChart.Lanes.Count - 1)];
            }
            else if (Event.current == CharterSettings.Keybinds["Misc./Show Keybindings"])
            {
                CharterSettings.Open(1);
            }
            Event.current.Use();
        }
    }

    #endregion

    ///////////////////
    #region Init Window
    ///////////////////

    string initName, initArtist;
    AudioClip initClip;

    public void CharterInit(int id) {
        
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
            if (!System.IO.Directory.Exists(path)) path = System.IO.Path.GetDirectoryName(path);

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

        if (GUI.Toggle(new Rect(27, 5, 130, 20), TargetThing == (object)TargetSong, TargetSong.SongName, "buttonLeft") && TargetThing != (object)TargetSong) {
            TargetThing = TargetSong;
        }

        // -------------------- Chart selection
                
        List<string> sels = new List<string>();
        foreach (Chart chart in TargetSong.Charts) sels.Add(chart.DifficultyName + " " + chart.DifficultyLevel);
        int sel = TargetChart != null ? EditorGUI.Popup(new Rect(309, 5, 18, 20), -1, sels.ToArray(), "buttonRight") :
            EditorGUI.Popup(new Rect(179, 5, 148, 20), -1, sels.ToArray(), "button");
        if (TargetChart == null) 
        {
            GUIStyle style = new GUIStyle("label");
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(179, 5, 148, 20), "Select Chart...", style);
        }
        if (sel >= 0) TargetChart = TargetSong.Charts[sel];
        
        if (TargetChart != null && GUI.Toggle(new Rect(179, 5, 130, 20), TargetThing == (object)TargetChart, TargetChart.DifficultyName + " " + TargetChart.DifficultyLevel, "buttonLeft") && TargetThing != (object)TargetChart) {
            TargetThing = TargetChart;
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

        if (GUI.Toggle(new Rect(5, 5, 20, 20), extrasmode == "main_menu",  EditorGUIUtility.IconContent("_Menu"),
            new GUIStyle("button") { padding = new RectOffset(0, 0, 0, 0) }) ^ (extrasmode == "main_menu"))
        {
            extrasmode = extrasmode == "main_menu" ? "" : "main_menu";
        }

        // -------------------- Options

        if (GUI.Button(new Rect(position.width / 2 - 66, 5, 40, 20), new GUIContent("Save", "Save Chart")))
        {
            EditorUtility.SetDirty(TargetSong);
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
        
        string ctText = SeparateUnits ? min.ToString("00", invariant) + ":" + sec.ToString("00", invariant) + "s" + ms.ToString("000", invariant) : CurrentAudioSource.time.ToString("0.000", invariant).Replace(".", "s");
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

        BPMStop bstop = TargetSong.Timing.GetStop(CurrentAudioSource.time, out int index);
        Color color = Color.black;
        if (index <= 0)
        {

        }
        else if (TargetSong.Timing.Stops[index - 1].BPM < bstop.BPM)
        {
            float time = 1 - (CurrentAudioSource.time - bstop.Offset);
            color = new Color(time * .8f, 0, 0);
        }
        else if (TargetSong.Timing.Stops[index - 1].BPM > bstop.BPM)
        {
            float time = 1 - (CurrentAudioSource.time - bstop.Offset);
            color = new Color(time * .1f, time * .1f, time);
        }

        EditorGUI.DrawRect(new Rect(width - 64, 6, 62, 18), color);
        if (beat >= 0) 
        {
            EditorGUI.DrawRect(new Rect(width - 63 + beat * 60 / bstop.Signature, 7, 60 / bstop.Signature, 16), new Color(1, 1, 1, (1 - dec / 1000) * (1 - dec / 1000)));
        }
            

    }

    #endregion

    ///////////////////////
    #region Timeline Window
    ///////////////////////

    public float seekStart, seekEnd;

    public string dragMode = "";
    public bool dragged = false;
    public string timelineMode = "lane";

    public int verSeek = 0;

    public void Timeline(int id) {
        float seekLimitStart = TargetSong.Timing.ToBeat(0) - 4;
        float seekLimitEnd = TargetSong.Timing.ToBeat(TargetSong.Clip.length) + 4;
        float seekTime = TargetSong.Timing.ToBeat(CurrentAudioSource.time);
        if (seekEnd == seekStart && seekStart == 0) 
        {
            seekEnd = Mathf.Min(width / 100, seekLimitEnd);
        }

        // Category

        if (GUI.Toggle(timelineMode == "story" ? new Rect(5, 132, 80, 24) : new Rect(5, 136, 80, 20), timelineMode == "story", "Storyboard", "button")) 
            timelineMode = "story";

        if (GUI.Toggle(timelineMode == "timing" ? new Rect(87, 132, 80, 24) : new Rect(87, 136, 80, 20), timelineMode == "timing", "Timing", "buttonLeft")) 
            timelineMode = "timing";
        if (GUI.Toggle(timelineMode == "lane" ? new Rect(168, 132, 80, 24) : new Rect(168, 136, 80, 20), timelineMode == "lane", "Lanes", "buttonRight")) 
            timelineMode = "lane";

        if (TargetLane != null && (GUI.Toggle(timelineMode == "hit" ? new Rect(250, 132, 80, 24) : new Rect(250, 136, 80, 20), timelineMode == "hit", "Hits", "button"))) 
            timelineMode = "hit";
            
        if (GUI.Toggle(new Rect(width - 81, 136, 80, 20), TargetChart != null && TargetThing == TargetChart.Pallete, "Pallete", "buttonRight") 
            && TargetChart != null && TargetThing != TargetChart.Pallete) 
            TargetThing = TargetChart.Pallete;


        GUIStyle label = new GUIStyle("miniLabel");
        label.alignment = TextAnchor.MiddleCenter;

        float zoom = width / (seekEnd - seekStart);
        float sep = Mathf.Log(zoom / 20, 2);
        float opa = ((sep % 1) + 1) % 1;
        sep = Mathf.Pow(2, Mathf.Floor(-sep));

        EditorGUI.DrawRect(new Rect(0, 100, width + 4, 1), EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .5f) : new Color(1, 1, 1, .5f));
        for (float a = Mathf.Ceil(seekStart / sep) * sep; a < seekEnd; a += sep)
        {
            float pos = (a - seekStart) / (seekEnd - seekStart) * width;

            float op = .5f;
            if (a % (sep * 2) != 0) op *= opa;

            float op2 = 1;
            if (a % (sep * 8) != 0) op2 = 0;
            else if (a % (sep * 16) != 0) op2 *= opa;

            if (TargetSong.Timing.ToBar(0, a) % 1 == 0) 
            {
                EditorGUI.DrawRect(new Rect(pos + 1, 0, 1, 115), new Color(.6f, .6f, .4f, .5f * op));
            }
            else 
            {
                EditorGUI.DrawRect(new Rect(pos + 1.5f, 0, 1, 100), new Color(.5f, .5f, .5f, .5f * op));
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

        if (TargetChart != null) {

            List<float> Times = new List<float>();
            int AddTime(float pos, float size) {
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

            if (timelineMode == "story")
            {
                if (TargetThing is IStoryboardable)
                {
                    IStoryboardable thing = (IStoryboardable)TargetThing;
                    Storyboard sb = thing.Storyboard;

                    List<string> tst = new List<string>();
                    List<string> tso = new List<string>();
                    foreach (TimestampType type in (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null)) {
                        tso.Add(type.ID);
                        tst.Add(type.Name);
                        Times.Add(0);
                    }

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

                            float rpos = Math.Min(Math.Max((a - seekStart) / (seekEnd - seekStart) * width, 5), (b - seekStart) / (seekEnd - seekStart) * width);
                            if (GUI.Toggle(new Rect(rpos - 1, 3 + time * 22, 6, 20), TargetThing == ts && DeletingThing != ts, DeletingThing == ts ? "?" : " ", "button"))
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
                        }
                    }
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(0, 0, width + 4, 115), EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .4f) : new Color(1, 1, 1, .4f));
                    GUIStyle center = new GUIStyle("label");
                    center.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(0, 0, width + 4, 115), TargetThing == null ? "Please select an object to start editing." : "This object is not storyboardable.", center);
                }
            }
            if (timelineMode == "timing")
            {
                foreach (BPMStop stop in TargetSong.Timing.Stops) 
                {
                    float a = TargetSong.Timing.ToBeat(stop.Offset);
                    float pos = (a - seekStart) / (seekEnd - seekStart) * width;
                    int time = AddTime(pos, 21) - verSeek;
                    if (time < 0 || time >= 5) continue;
                    if (a > seekStart && a < seekEnd) 
                    {
                        if (GUI.Toggle(new Rect(pos - 29, 3 + time * 22, 60, 20), TargetThing == stop && DeletingThing != stop, DeletingThing == stop ? "?" : stop.BPM.ToString("F2", invariant), "button"))
                        {
                            if (pickermode == "delete")
                            {
                                if (DeletingThing == stop)
                                {
                                    TargetSong.Timing.Stops.Remove(stop);
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
                foreach (Lane lane in TargetChart.Lanes) 
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
                            EditorGUI.DrawRect(new Rect(pos + 2, 3 + time * 22, pos2 - pos, 20), new Color(0, 1, 0, .2f));
                        }
                        for (int x = 1; x < lane.LaneSteps.Count; x++) {
                            float c = lane.LaneSteps[x].Offset;
                            if (c > seekStart && c < seekEnd) 
                            {
                                float pos3 = (c - seekStart) / (seekEnd - seekStart) * width;
                                if (GUI.Toggle(new Rect(pos3 - 1, 3 + time * 22, 6, 20), TargetThing == lane.LaneSteps[x] && DeletingThing != lane.LaneSteps[x], DeletingThing == lane.LaneSteps[x] ? "?" : "|", "button"))
                                {
                                    if (pickermode == "delete")
                                    {
                                        if (DeletingThing == lane.LaneSteps[x])
                                        {
                                            lane.LaneSteps.Remove(lane.LaneSteps[x]);
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
                            float rpos = Math.Min(Math.Max((a - seekStart) / (seekEnd - seekStart) * width, 13), (b - seekStart) / (seekEnd - seekStart) * width - 15);
                            if (GUI.Toggle(new Rect(rpos - 9, 3 + time * 22, 20, 20), TargetThing == lane && DeletingThing != lane, DeletingThing == lane ? "?" : "|", "button"))
                            {
                                if (pickermode == "delete")
                                {
                                    if (DeletingThing == lane)
                                    {
                                        TargetChart.Lanes.Remove(lane);
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
                        TargetChart.Lanes.Remove(lane);
                    }
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
                            if (GUI.Toggle(new Rect(pos - 9, 3 + time * 22, 20, 20), TargetThing == hit && DeletingThing != hit, DeletingThing == hit ? "?" : "|", "button"))
                            {
                                if (pickermode == "delete")
                                {
                                    if (DeletingThing == hit)
                                    {
                                        TargetLane.Objects.Remove(hit);
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

                if (Event.current.type == EventType.ScrollWheel)
                {
                    Vector2 mPos = Event.current.mousePosition;
                    if (mPos.y > 0 && mPos.y < 115) 
                    {
                        verSeek = verSeek + Math.Sign(Event.current.delta.y);
                        Repaint();
                    }
                }
            }
            verSeek = Mathf.Max(Mathf.Min(verSeek, Times.Count - 5), 0);
        }

        
        // Click events
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) 
        {
            Vector2 mPos = Event.current.mousePosition;
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
                    CurrentAudioSource.time = Mathf.Clamp(TargetSong.Timing.ToSeconds(Mathf.Round(sPos / sep) * sep), 0, TargetSong.Clip.length - .0001f);
                    dragMode = "seeksnap";
                    Repaint();
                }
            }
            dragged = false;
        }
        else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0) 
        {
            Vector2 mPos = Event.current.mousePosition;
            float sPos = mPos.x * (seekEnd - seekStart) / width + seekStart;
            if (dragMode == "seek") 
            {
                CurrentAudioSource.time = Mathf.Clamp(TargetSong.Timing.ToSeconds(sPos), 0, TargetSong.Clip.length - .0001f);
                Repaint();
            }
            if (dragMode == "seeksnap") 
            {
                CurrentAudioSource.time = Mathf.Clamp(TargetSong.Timing.ToSeconds(Mathf.Round(sPos / sep) * sep), 0, TargetSong.Clip.length - .0001f);
                Repaint();
            }
            dragged = true;
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0) 
        {
            if (!dragged && !CurrentAudioSource.isPlaying) 
            {
                if (dragMode == "seeksnap" && pickermode == "bpmstop") 
                {
                    BPMStop stop = new BPMStop(TargetSong.Timing.GetStop(CurrentAudioSource.time, out _).BPM, Mathf.Round(CurrentAudioSource.time * 1000) / 1000);
                    TargetSong.Timing.Stops.Add(stop);
                    TargetSong.Timing.Stops.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                    Repaint();
                }
                if (dragMode == "seeksnap" && pickermode == "lane") 
                {
                    Lane lane = new Lane();

                    LaneStep step = new LaneStep();
                    step.Offset = Mathf.Round(pos * 1000) / 1000;
                    step.StartPos = new Vector2(-6, -3);
                    step.EndPos = new Vector2(6, -3);
                    lane.LaneSteps.Add(step);

                    TargetChart.Lanes.Add(lane);
                    TargetChart.Lanes.Sort((x, y) => x.LaneSteps[0].Offset.CompareTo(y.LaneSteps[0].Offset));
                    TargetThing = lane;
                    Repaint();
                }
                else if (dragMode == "seeksnap" && pickermode.StartsWith("hit_") && TargetLane != null) 
                {
                    HitObject hit = new HitObject();
                    hit.Offset = Mathf.Round(pos * 1000) / 1000;
                    if (TargetThing is HitObject)
                    {
                        HitObject thing = (HitObject)TargetThing;
                        hit.Position = thing.Position;
                        hit.Length = thing.Length;
                        hit.Type = pickermode == "hit_catch" ? HitObject.HitType.Catch : HitObject.HitType.Normal;
                    }
                    TargetLane.Objects.Add(hit);
                    TargetLane.Objects.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                    TargetThing = hit;
                    Repaint();
                }
            }
            dragMode = "";
        }

        if (seekTime >= seekStart && seekTime < seekEnd) {
            float pos = (seekTime - seekStart) / (seekEnd - seekStart) * width;
            EditorGUI.DrawRect(new Rect(pos + 1, 0, 2, 115), EditorGUIUtility.isProSkin ? Color.white : Color.black);
        }
    }

    #endregion

    ////////////////////////
    #region Inspect Mode Window
    ////////////////////////

    string inspectMode = "properties";

    public void InspectMode(int id) {
        GUIUtility.RotateAroundPivot(-90, Vector2.one * (height / 2 - 102));
        if (GUI.Toggle(new Rect(27, 0, 80, 28), inspectMode == "properties", "Properties", "button")) inspectMode = "properties";
        if (GUI.Toggle(new Rect(109, 0, 80, 28), inspectMode == "storyboard", "Storyboard", "button")) inspectMode = "storyboard";
    }

    #endregion

    ////////////////////////
    #region Inspector Window
    ////////////////////////

    Vector2 scrollPos = Vector2.zero;

    public void Inspector(int id) {
        GUI.Label(new Rect(0, 0, 240, 24), "", "button");
        EditorGUIUtility.labelWidth = 80;
        if (TargetThing == null) 
        {
            GUI.Label(new Rect(7, 2, 226, 20), "No object selected", "boldLabel");
            GUILayout.Space(8);
            GUILayout.Label("Please select an object to start editing.");
        }
        else if (inspectMode == "properties")
        {
            if (TargetThing is PlayableSong)
            {
                PlayableSong thing = (PlayableSong)TargetThing;

                GUIStyle bStyle = new GUIStyle("textField");
                bStyle.fontStyle = FontStyle.Bold;

                GUI.Label(new Rect(7, 2, 226, 20), "Song Details", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                GUILayout.Label("Metadata", "boldLabel");
                thing.SongName = EditorGUILayout.TextField("Song Name", thing.SongName, bStyle);
                thing.SongArtist = EditorGUILayout.TextField("Song Artist", thing.SongArtist);
                GUILayout.Space(8);
                GUILayout.Label("Charts", "boldLabel");
                foreach (Chart chart in TargetSong.Charts)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Toggle(TargetChart == chart, chart.DifficultyName + " " + chart.DifficultyLevel, "ButtonLeft"))
                    {
                        TargetChart = chart;
                    }
                    if (GUILayout.Button(DeletingThing == chart ? "?" : "x", "ButtonRight", GUILayout.MaxWidth(18)) && TargetChart != chart)
                    {
                        if (DeletingThing == chart) 
                        {
                            TargetSong.Charts.Remove(chart);
                            EditorUtility.SetDirty(TargetSong);
                            break;
                        }
                        else 
                        {
                            DeletingThing = chart;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Create New Chart"))
                {
                    Chart chart = new Chart();
                    TargetSong.Charts.Add(chart);
                    TargetChart = chart;
                    EditorUtility.SetDirty(TargetSong);
                }
                GUILayout.EndScrollView();
            }
            else if (TargetThing is BPMStop)
            {
                BPMStop thing = (BPMStop)TargetThing;

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
                GUILayout.EndScrollView();
            }
            else if (TargetThing is Chart)
            {
                Chart thing = (Chart)TargetThing;
                
                GUI.Label(new Rect(7, 2, 226, 20), "Chart Details", "boldLabel");
                GUILayout.Space(8);
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                GUILayout.Label("Difficulty", "boldLabel");
                thing.DifficultyIndex = EditorGUILayout.IntField("Index", thing.DifficultyIndex);
                thing.DifficultyName = EditorGUILayout.TextField("Name", thing.DifficultyName);
                thing.DifficultyLevel = EditorGUILayout.TextField("Level", thing.DifficultyLevel);
                thing.ChartConstant = EditorGUILayout.IntField("Constant", thing.ChartConstant);
                GUILayout.Space(8);
                GUILayout.Label("Layout", "boldLabel");
                thing.CameraPivot = EditorGUILayout.Vector3Field("Camera Pivot", thing.CameraPivot);
                thing.CameraRotation = EditorGUILayout.Vector3Field("Camera Rotation", thing.CameraRotation);
                GUILayout.Space(8);
                GUILayout.Label("Appearance (Legacy)", "boldLabel");
                thing.BackgroundColor = EditorGUILayout.ColorField("Background Color", thing.BackgroundColor);
                thing.InterfaceColor = EditorGUILayout.ColorField("Interface Color", thing.InterfaceColor);
                thing.LaneMaterial = (Material)EditorGUILayout.ObjectField("Lane Material", thing.LaneMaterial, typeof(Material), false);
                thing.HitMaterial = (Material)EditorGUILayout.ObjectField("Hit Material", thing.HitMaterial, typeof(Material), false);
                thing.HoldMaterial = (Material)EditorGUILayout.ObjectField("Hold Material", thing.HoldMaterial, typeof(Material), false);
                GUILayout.EndScrollView();
            }
            if (TargetThing is Pallete)
            {
                Pallete thing = (Pallete)TargetThing;

                GUI.Label(new Rect(7, 2, 226, 20), "Pallete", "boldLabel");
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
                            thing.LaneStyles.Remove(style);
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
                    thing.LaneStyles.Add(style);
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
                            thing.HitStyles.Remove(style);
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
                    thing.HitStyles.Add(style);
                    TargetThing = style;
                }

                GUILayout.EndScrollView();
            }
            else if (TargetThing is LaneStyle)
            {
                LaneStyle thing = (LaneStyle)TargetThing;

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
            }
            else if (TargetThing is HitStyle)
            {
                HitStyle thing = (HitStyle)TargetThing;

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
            }
            else if (TargetThing is Lane)
            {
                Lane thing = (Lane)TargetThing;
                
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

                GUILayout.Space(8);
                GUILayout.Label("Steps", "boldLabel");
                float h = 0;
                float o = GUILayoutUtility.GetLastRect().yMax;
                float a = thing.LaneSteps[0].Offset;

                List<string> est = new List<string>();
                List<string> eso = new List<string>();
                foreach (Ease ease in Ease.Eases) {
                    eso.Add(ease.ID);
                    est.Add(ease.Name);
                }

                foreach (LaneStep step in thing.LaneSteps)
                {
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
                        GUI.Button(new Rect(61, h + o + 19, 17, 14), new [] { "I", "O", "IO" }[(int)step.StartEaseXMode], buttonStyle );
                        int newEaseX = EditorGUI.Popup(new Rect(79, h + o + 19, 30, 14), easeX, est.ToArray(), buttonLeftStyle);
                        if (newEaseX != easeX) step.StartEaseX = eso[newEaseX];
                        
                        step.StartPos.y = EditorGUI.FloatField(new Rect(110, h + o + 19, 40, 14), step.StartPos.y, fieldStyle);
                        GUI.Label(new Rect(110, h + o + 19, 40, 14), "y0", rightStyle);
                        int easeY = eso.IndexOf(step.StartEaseY);
                        step.StartEaseYMode = (EaseMode)EditorGUI.EnumPopup(new Rect(151, h + o + 19, 17, 14), step.StartEaseYMode, buttonStyle);
                        GUI.Button(new Rect(151, h + o + 19, 17, 14), new [] { "I", "O", "IO" }[(int)step.StartEaseYMode], buttonStyle );
                        int newEaseY = EditorGUI.Popup(new Rect(169, h + o + 19, 30, 14), easeY, est.ToArray(), buttonLeftStyle);
                        if (newEaseY != easeY) step.StartEaseY = eso[newEaseY];
                    }
                    {
                        step.EndPos.x = EditorGUI.FloatField(new Rect(20, h + o + 34, 40, 14), step.EndPos.x, fieldStyle);
                        GUI.Label(new Rect(20, h + o + 34, 40, 14), "x1", rightStyle);
                        int easeX = eso.IndexOf(step.EndEaseX);
                        step.EndEaseXMode = (EaseMode)EditorGUI.EnumPopup(new Rect(61, h + o + 34, 17, 14), step.EndEaseXMode, buttonStyle);
                        GUI.Button(new Rect(61, h + o + 34, 17, 14), new [] { "I", "O", "IO" }[(int)step.EndEaseXMode], buttonStyle );
                        int newEaseX = EditorGUI.Popup(new Rect(79, h + o + 34, 30, 14), easeX, est.ToArray(), buttonLeftStyle);
                        if (newEaseX != easeX) step.EndEaseX = eso[newEaseX];
                        
                        step.EndPos.y = EditorGUI.FloatField(new Rect(110, h + o + 34, 40, 14), step.EndPos.y, fieldStyle);
                        GUI.Label(new Rect(110, h + o + 34, 40, 14), "y1", rightStyle);
                        int easeY = eso.IndexOf(step.EndEaseY);
                        step.EndEaseYMode = (EaseMode)EditorGUI.EnumPopup(new Rect(151, h + o + 34, 17, 14), step.EndEaseYMode, buttonStyle);
                        GUI.Button(new Rect(151, h + o + 34, 17, 14), new [] { "I", "O", "IO" }[(int)step.EndEaseYMode], buttonStyle );
                        int newEaseY = EditorGUI.Popup(new Rect(169, h + o + 34, 30, 14), easeY, est.ToArray(), buttonLeftStyle);
                        if (newEaseY != easeY) step.EndEaseY = eso[newEaseY];
                    }

                    if (GUI.Button(new Rect(3, h + o + 2, 16, 48), "⋮", "buttonLeft"))
                    {
                        TargetThing = step;
                    }
                    if (GUI.Button(new Rect(202, h + o + 2, 16, 48), "x", "buttonRight") && thing.LaneSteps.Count > 1)
                    {
                        thing.LaneSteps.Remove(step);
                        break;
                    }
                    h += 50;
                }
                GUILayout.Space(h);
                if (GUILayout.Button("Create New Step"))
                {
                    LaneStep step = new LaneStep();
                    step.Offset = Mathf.Round(pos * 1000) / 1000;
                    step.StartPos = thing.LaneSteps[thing.LaneSteps.Count - 1].StartPos;
                    step.EndPos = thing.LaneSteps[thing.LaneSteps.Count - 1].EndPos;
                    thing.LaneSteps.Add(step);
                    thing.LaneSteps.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                }
                GUILayout.EndScrollView();
                
                if (thing.LaneSteps[0].Offset != a) 
                {
                    TargetChart.Lanes.Sort((x, y) => x.LaneSteps[0].Offset.CompareTo(y.LaneSteps[0].Offset));
                }
            }
            else if (TargetThing is LaneStep)
            {
                LaneStep thing = (LaneStep)TargetThing;

                GUIStyle rightStyle = new GUIStyle("label");
                rightStyle.alignment = TextAnchor.UpperRight;
                rightStyle.normal.textColor = new Color(rightStyle.normal.textColor.r, 
                    rightStyle.normal.textColor.g, rightStyle.normal.textColor.b, .5f);

                List<string> est = new List<string>();
                List<string> eso = new List<string>();
                foreach (Ease ease in Ease.Eases) {
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
            }
            else if (TargetThing is HitObject)
            {
                HitObject thing = (HitObject)TargetThing;

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
                GUILayout.Label("Appearance", "boldLabel");
                thing.StyleIndex = EditorGUILayout.IntField("Style Index", thing.StyleIndex);

                GUILayout.EndScrollView();
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
                foreach (TimestampType type in (TimestampType[])thing.GetType().GetField("TimestampTypes").GetValue(null)) {
                    tso.Add(type.ID);
                    tst.Add(type.Name);
                }

                List<string> est = new List<string>();
                List<string> eso = new List<string>();
                foreach (Ease ease in Ease.Eases) {
                    eso.Add(ease.ID);
                    est.Add(ease.Name);
                }


                int add = EditorGUI.Popup(new Rect(218, 2, 20, 20), -1, tst.ToArray(), "button");
                if (add != -1) {
                    sb.Timestamps.Add(new Timestamp {
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
                        sb.Timestamps.Remove(ts);
                        break;
                    }
                    h += 35;
                }
                
                GUILayout.Space(h);
                GUILayout.EndScrollView();
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
        if (GUI.Toggle(new Rect(0, 0, 33, 33), pickermode == "cursor", EditorGUIUtility.IconContent("Grid.Default@2x", "Cursor"), "button")) pickermode = "cursor";
        if (GUI.Toggle(new Rect(0, 32, 33, 33), pickermode == "select", EditorGUIUtility.IconContent("Selectable Icon", "Select"), "button")) pickermode = "select";
        if (GUI.Toggle(new Rect(0, 64, 33, 33), pickermode == "delete", EditorGUIUtility.IconContent("winbtn_win_close@2x", "Select"), "button")) pickermode = "delete";

        if (timelineMode == "timing") 
        {
            if (GUI.Toggle(new Rect(0, 106, 33, 33), pickermode == "bpmstop", new GUIContent("STP", "BPM Stop"), "button")) pickermode = "bpmstop";
        }
        else if (timelineMode == "lane") 
        {
            if (GUI.Toggle(new Rect(0, 106, 33, 33), pickermode == "lane", new GUIContent("LNE", "Lane"), "button")) pickermode = "lane";
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

    public void Extras(int id) 
    {
        if (extrasmode == "main_menu") 
        {
            GUIStyle itemStyle = new GUIStyle("iconButton");
            itemStyle.fixedWidth = 0;
            itemStyle.fixedHeight = 0;
            itemStyle.alignment = TextAnchor.MiddleLeft;
            itemStyle.padding = new RectOffset(5, 5, 1, 2);

            GUI.Button(new Rect(2, 2, 147, 20), "Coming Soon...", itemStyle);
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

            PlayMetronome = GUI.Toggle(new Rect(5, 73, 145, 20), PlayMetronome, "Metronome", "buttonLeft");
            PlayHitsounds = GUI.Toggle(new Rect(150, 73, 145, 20), PlayHitsounds, "Hitsounds", "buttonRight");
            SeparateUnits = GUI.Toggle(new Rect(5, 95, 290, 20), SeparateUnits, "Separate Units", "button");
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

    public class TutorialStep {
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
            TutorialPopupAnchor = Vector2.Lerp(prev.PopupAnchor, step.PopupAnchor, ease);
            TutorialPopupPosition = Vector2.Lerp(prev.PopupPosition, step.PopupPosition, ease);
            Repaint();
        } else {
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
