using System;
using System.Globalization;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Utils;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Debugger
{
    public class FPSCounter : MonoBehaviour
    {
        public TMP_Text   Text;
        public float      Timer;
        public float      Interval = .2f;

        private int      _SampleSize = 150;
        private double[] _AverageSampleFPS;
        private double[] _AverageSamplesDelta;
        private int      _AverageSampleIndex;
        bool             _AverageFilled = false;
        
        private CultureInfo _Invariant;

        private FrameTiming[] _FrameTimes = new FrameTiming[10];

        // Start is called before the first frame update
        private void Start()
        {
            _AverageSampleFPS = new double[_SampleSize];
            _AverageSamplesDelta = new double[_SampleSize];
            
            _Invariant = CultureInfo.InvariantCulture;
        }

        // Update is called once per frame
        private void Update()
        {
            // Keeps contrast well
            Text.color = (Color.white - CommonSys.sMain.MainCamera.backgroundColor) * new ColorFrag(a: 1);
            
            Timer += Time.deltaTime;

            FrameTimingManager.CaptureFrameTimings();

            if (Timer >= Interval)
            {
                uint frameTiming = FrameTimingManager.GetLatestTimings((uint)_FrameTimes.Length, _FrameTimes);

                double frame = Time.smoothDeltaTime;
                double cpuStrain = 0, gpuStrain = 0;

                if (frameTiming > 0)
                {
                    for (var a = 0; a < frameTiming; a++)
                    {
                        FrameTiming frameTime = _FrameTimes[a];
                        cpuStrain += frameTime.cpuFrameTime;
                        gpuStrain += frameTime.gpuFrameTime;
                    }

                    cpuStrain *= 1e3 / frameTiming;
                    gpuStrain *= 1e3 / frameTiming;
                }

                double fps = 1 / frame;
                double delta = 1e3 * frame;

                if (_AverageSampleIndex < _AverageSampleFPS.Length)
                {
                    _AverageSampleFPS[_AverageSampleIndex] = fps;
                    _AverageSamplesDelta[_AverageSampleIndex] = delta;
                    _AverageSampleIndex++;
                }
                else
                {
                    _AverageFilled = true;
                    _AverageSampleIndex = 0;
                }

                // Incomplete average will be inaccurate; use current data meanwhile
                double avgFPS = _AverageFilled 
                    ? _AverageSampleFPS.Average() : Double.NaN;
                
                double avgDelta = _AverageFilled 
                    ? _AverageSamplesDelta.Average() : Double.NaN;

                Text.text = $"frame: {fps:0}fps / {(delta):0.0}ms, " + $"avg: {avgFPS:0.0}fps / {avgDelta:0.0}ms \n" +
                            $"strain: {(frameTiming > 0 ? $"{cpuStrain:0.0}ms cpu, {gpuStrain:0.0}ms gpu" : "(unavailable)")}";
                Timer -= Interval;
            }
        }
    }
}