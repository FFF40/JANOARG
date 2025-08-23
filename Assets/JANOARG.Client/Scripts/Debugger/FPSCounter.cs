using System.Globalization;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Debugger
{
    public class FPSCounter : MonoBehaviour
    {
        public TMP_Text Text;
        public float    Timer;
        public float    Interval = .2f;

        private CultureInfo _Invariant;

        private FrameTiming[] _FrameTimes = new FrameTiming[10];

        // Start is called before the first frame update
        private void Start()
        {
            _Invariant = CultureInfo.InvariantCulture;
        }

        // Update is called once per frame
        private void Update()
        {
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
                        FrameTiming ft = _FrameTimes[a];
                        cpuStrain += ft.cpuFrameTime;
                        gpuStrain += ft.gpuFrameTime;
                    }

                    cpuStrain *= 1e3 / frameTiming;
                    gpuStrain *= 1e3 / frameTiming;
                }

                Text.text = "frame: " + (1 / frame).ToString("0", _Invariant) + "fps " + (1e3 * frame).ToString("0.0", _Invariant) + "ms\n" +
                            "strain: " + (frameTiming > 0 ? cpuStrain.ToString("0.0", _Invariant) + "ms cpu, " + gpuStrain.ToString("0.0", _Invariant) + "ms gpu" : "(unavailable)");

                Timer -= Interval;
            }
        }
    }
}