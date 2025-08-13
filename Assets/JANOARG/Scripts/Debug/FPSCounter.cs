using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text Text;
    public float Timer;
    public float Interval = .2f;

    CultureInfo invariant;

    FrameTiming[] frameTimes = new FrameTiming[10];

    // Start is called before the first frame update
    void Start()
    {
        invariant = System.Globalization.CultureInfo.InvariantCulture;
    }

    // Update is called once per frame
    void Update()
    {

        Timer += Time.deltaTime;

        FrameTimingManager.CaptureFrameTimings();

        if (Timer >= Interval)
        {
            uint fr = FrameTimingManager.GetLatestTimings((uint)frameTimes.Length, frameTimes);

            double frame = Time.smoothDeltaTime;
            double cpuStrain = 0, gpuStrain = 0;
            if (fr > 0)
            {
                for (int a = 0; a < fr; a++)
                {
                    FrameTiming ft = frameTimes[a];
                    cpuStrain += ft.cpuFrameTime;
                    gpuStrain += ft.gpuFrameTime;
                }
                cpuStrain *= 1e3 / fr;
                gpuStrain *= 1e3 / fr;
            }

            Text.text = "frame: " + (1 / frame).ToString("0", invariant) + "fps " + (1e3 * frame).ToString("0.0", invariant) + "ms\n" + 
                "strain: " + (fr > 0 ? cpuStrain.ToString("0.0", invariant) + "ms cpu, " + gpuStrain.ToString("0.0", invariant) + "ms gpu" : "(unavailable)");
            Timer -= Interval;
        }
    }
}
