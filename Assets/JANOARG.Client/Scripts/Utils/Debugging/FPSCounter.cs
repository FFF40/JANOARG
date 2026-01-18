using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Utils;
using JANOARG.Shared.Utils;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Utils.Debugging
{
    public class FPSCounter : MonoBehaviour
    {
        public TMP_Text Text;
        public float Timer;
        public float Interval = .2f;

        private int _SampleSize = 150;
        private ConcurrentQueue<double> _FPSSamples = new ConcurrentQueue<double>();
        private ConcurrentQueue<double> _DeltaSamples = new ConcurrentQueue<double>();
        
        private double _CachedAvgFPS = double.NaN;
        private double _CachedAvgDelta = double.NaN;
        private volatile bool _HasValidAverages = false;
        
        private CultureInfo _Invariant;
        private FrameTiming[] _FrameTimes = new FrameTiming[10];
        
        private CancellationTokenSource _CancellationTokenSource;
        private Task _BackgroundTask;

        private void Start()
        {
            _Invariant = CultureInfo.InvariantCulture;
            _CancellationTokenSource = new CancellationTokenSource();
            
            // Start background averaging task
            _BackgroundTask = Task.Run(BackgroundAveragingLoop, _CancellationTokenSource.Token);
        }

        private async Task BackgroundAveragingLoop()
        {
            var token = _CancellationTokenSource.Token;
            
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Calculate averages if we have enough samples
                    if (_FPSSamples.Count >= _SampleSize)
                    {
                        double fpsSum = 0, deltaSum = 0;
                        int fpsCount = 0, deltaCount = 0;
                        
                        // Convert queues to arrays for averaging (thread-safe snapshot)
                        var fpsArray = _FPSSamples.ToArray();
                        var deltaArray = _DeltaSamples.ToArray();
                        
                        foreach (var fps in fpsArray)
                        {
                            fpsSum += fps;
                            fpsCount++;
                        }
                        
                        foreach (var delta in deltaArray)
                        {
                            deltaSum += delta;
                            deltaCount++;
                        }
                        
                        if (fpsCount > 0 && deltaCount > 0)
                        {
                            _CachedAvgFPS = fpsSum / fpsCount;
                            _CachedAvgDelta = deltaSum / deltaCount;
                            _HasValidAverages = true;
                        }
                    }
                    
                    // Sleep to avoid burning CPU cycles
                    await Task.Delay(50, token); // Update averages every 50ms
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"FPS Counter background thread error: {ex.Message}");
                    await Task.Delay(100, token);
                }
            }
        }

        private void Update()
        {
            // Minimal main thread work
            Text.color = (Color.white - CommonSys.sMain.MainCamera.backgroundColor) * new ColorFrag(a: 1);
            
            Timer += Time.deltaTime;

            if (Timer >= Interval)
            {
                // Get current frame data (lightweight)
                double frame = Time.smoothDeltaTime;
                double fps = 1 / frame;
                double delta = 1e3 * frame;
                
                // Add samples to background queues (thread-safe)
                _FPSSamples.Enqueue(fps);
                _DeltaSamples.Enqueue(delta);
                
                // Trim queues to maintain sample size
                while (_FPSSamples.Count > _SampleSize)
                    _FPSSamples.TryDequeue(out _);
                while (_DeltaSamples.Count > _SampleSize)
                    _DeltaSamples.TryDequeue(out _);

                // Get GPU/CPU timing (keep on main thread as Unity APIs aren't thread-safe)
                double cpuStrain = 0, gpuStrain = 0;
                uint frameTiming = 0;
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                FrameTimingManager.CaptureFrameTimings();
                frameTiming = FrameTimingManager.GetLatestTimings((uint)_FrameTimes.Length, _FrameTimes);

                if (frameTiming > 0)
                {
                    for (var a = 0; a < frameTiming; a++)
                    {
                        FrameTiming frameTime = _FrameTimes[a];
                        cpuStrain += frameTime.cpuFrameTime;
                        gpuStrain += frameTime.gpuFrameTime;
                    }

                    cpuStrain *= 1d / frameTiming;
                    gpuStrain *= 1d / frameTiming;
                }
#endif

                // Use cached averages from background thread
                double avgFPS = _HasValidAverages ? _CachedAvgFPS : Double.NaN;
                double avgDelta = _HasValidAverages ? _CachedAvgDelta : Double.NaN;

                // Simple string formatting (reduced allocations)
                Text.text = $"frame: {fps:0}fps / {delta:0.00}ms, avg: {avgFPS:0.0}fps / {avgDelta:0.00}ms\n" +
                           $"strain: {(frameTiming > 0 ? $"{cpuStrain:0.00}ms cpu, {gpuStrain:0.00}ms gpu" : "(unavailable)")}";
                
                Timer -= Interval;
            }
        }

        private void OnDestroy()
        {
            // Clean up background thread
            _CancellationTokenSource?.Cancel();
            
            try
            {
                _BackgroundTask?.Wait(1000); // Wait up to 1 second for clean shutdown
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"FPS Counter cleanup warning: {ex.Message}");
            }
            finally
            {
                _CancellationTokenSource?.Dispose();
            }
        }
    }
}