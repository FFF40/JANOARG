using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

public class DebugStatsInspector : MonoBehaviour
{
    public TMP_Text LaneCountLabel;
    public TMP_Text HitCountLabel;
    public TMP_Text VertsLabel;
    public TMP_Text TrisLabel;

    public TMP_Text FPSLabel;
    public TMP_Text MSLabel;
    public TMP_Text MinFPSLabel;
    public TMP_Text MaxFPSLabel;
    public TMP_Text AverageFPSLabel;
    public Graphic FPSGraph;

    float UpdateClock = 0;
    float UpdateMin = 0;
    float UpdateMax = float.PositiveInfinity;
    int UpdateFrames = 0;

    List<float> FrameHistory = new();
    List<float> FrameMin = new();
    List<float> FrameMax = new();
    
    [Space]
    public TMP_Text AllocatedMemoryLabel;
    public TMP_Text ReservedMemoryLabel;
    public TMP_Text MonoMemoryLabel;
    public Graphic MemoryGraph;

    List<float> AllocatedMemory = new();
    List<float> ReservedMemory = new();
    List<float> MonoMemory = new();

    Material FPSGraphMaterial, MemoryGraphMaterial;

    void Start()
    {
        FPSGraphMaterial = new Material(FPSGraph.material);
        MemoryGraphMaterial = new Material(MemoryGraph.material);
    }

    void OnDestroy()
    {
        Destroy(FPSGraphMaterial);
        Destroy(MemoryGraphMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateClock += Time.unscaledDeltaTime;
        UpdateMin = Mathf.Max(UpdateMin, Time.unscaledDeltaTime);
        UpdateMax = Mathf.Min(UpdateMax, Time.unscaledDeltaTime);
        UpdateFrames++;
        if (UpdateClock > 0.1f) 
        {
            LaneCountLabel.text = PlayerView.main.Manager?.ActiveLaneCount.ToString() ?? "-";
            HitCountLabel.text = PlayerView.main.Manager?.ActiveHitCount.ToString() ?? "-";
            VertsLabel.text = PlayerView.main.Manager?.ActiveLaneVerts.ToString() ?? "-";
            TrisLabel.text = PlayerView.main.Manager?.ActiveLaneTris.ToString() ?? "-";

            float msAvg = UpdateClock / UpdateFrames;
            UpdateClock = UpdateFrames = 0;

            FPSLabel.text = (1 / msAvg).ToString("0.0");
            MSLabel.text = (msAvg * 1000).ToString("0.00");

            FrameHistory.Add(msAvg);
            FrameMin.Add(UpdateMin);
            FrameMax.Add(UpdateMax);
            while (FrameHistory.Count > 64) FrameHistory.RemoveAt(0);
            while (FrameMin.Count > 64) FrameMin.RemoveAt(0);
            while (FrameMax.Count > 64) FrameMax.RemoveAt(0);
            float fpsheight = Mathf.Max(FrameMin.ToArray()); 
            float[] fpslist = new float[64], fpsmin = new float[64], fpsmax = new float[64];
            float fpsSum = 0;
            for (int a = 0; a < 64; a++) 
            {
                int i = FrameHistory.Count - 64 + a;
                fpslist[a] = i >= 0 ? FrameHistory[i] / fpsheight : -1e6f;
                fpsmin[a] = i >= 0 ? FrameMin[i] / fpsheight : -1e6f;
                fpsmax[a] = i >= 0 ? FrameMax[i] / fpsheight : -1e6f;
                fpsSum += i >= 0 ? FrameHistory[i] : 0;
            }
            FPSGraphMaterial.SetFloatArray("_Values", fpslist);
            FPSGraphMaterial.SetFloatArray("_ValuesMin", fpsmin);
            FPSGraphMaterial.SetFloatArray("_ValuesMax", fpsmax);
            FPSGraphMaterial.SetVector("_Resolution", new(FPSGraph.rectTransform.rect.width, FPSGraph.rectTransform.rect.height));
            
            FPSGraph.material = FPSGraphMaterial;
            FPSGraph.SetMaterialDirty();

            MinFPSLabel.text = (1 / Mathf.Max(FrameMin.ToArray())).ToString("0.0");
            MaxFPSLabel.text = (1 / Mathf.Min(FrameMax.ToArray())).ToString("0.0");
            AverageFPSLabel.text = (1 / fpsSum * FrameHistory.Count).ToString("0.0");
            
            UpdateMax = float.PositiveInfinity;
            UpdateMin = 0;
            

            AllocatedMemory.Add(Profiler.GetTotalAllocatedMemoryLong() / 1048576f);
            ReservedMemory.Add(Profiler.GetTotalReservedMemoryLong() / 1048576f);
            MonoMemory.Add(Profiler.GetMonoUsedSizeLong() / 1048576f);
            while (AllocatedMemory.Count > 64) AllocatedMemory.RemoveAt(0);
            while (ReservedMemory.Count > 64) ReservedMemory.RemoveAt(0);
            while (MonoMemory.Count > 64) MonoMemory.RemoveAt(0);
            float memheight = Mathf.Max(ReservedMemory.ToArray()); 
            float[] memall = new float[64], memres = new float[64], memmono = new float[64];
            for (int a = 0; a < 64; a++) 
            {
                int i = FrameHistory.Count - 64 + a;
                memall[a] = i >= 0 ? AllocatedMemory[i] / memheight : -1e6f;
                memres[a] = i >= 0 ? ReservedMemory[i] / memheight : -1e6f;
                memmono[a] = i >= 0 ? MonoMemory[i] / memheight : -1e6f;
            }
            MemoryGraphMaterial.SetFloatArray("_Values1", memall);
            MemoryGraphMaterial.SetFloatArray("_Values2", memmono);
            MemoryGraphMaterial.SetFloatArray("_Values3", memres);
            
            MemoryGraph.material = MemoryGraphMaterial;
            MemoryGraph.SetMaterialDirty();
            
            AllocatedMemoryLabel.text = AllocatedMemory[^1].ToString("0.0");
            ReservedMemoryLabel.text = ReservedMemory[^1].ToString("0.0");
            MonoMemoryLabel.text = MonoMemory[^1].ToString("0.0");
        }
    }
    
    public void OpenLogger()
    {
        ModalHolder.main.Spawn<LoggerModal>();
    }
}
