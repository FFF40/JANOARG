using System.Collections;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.Audio;

namespace JANOARG.Client.Behaviors.Common
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager sMain;


        [Space] public AudioMixer AudioMixer;

        private Coroutine _SceneLowPassCutoffRoutine;

        public void Awake()
        {
            sMain = this;
        }

        public void SetSceneLayerLowPassCutoff(float value, float duration)
        {
            if (_SceneLowPassCutoffRoutine != null) StopCoroutine(_SceneLowPassCutoffRoutine);
            _SceneLowPassCutoffRoutine = StartCoroutine(SetSceneLayerLowPassCutoffAnim(value, duration));
        }

        private IEnumerator SetSceneLayerLowPassCutoffAnim(float value, float duration)
        {
            AudioMixer.GetFloat("SceneLayerLowPassCutoff", out float fromLog);
            fromLog = Mathf.Log10(fromLog);
            float toLog = Mathf.Log10(value);

            yield return Ease.Animate(
                duration, a =>
                {
                    float cutoffLog = Mathf.Lerp(fromLog, toLog, a);
                    float cutoff = Mathf.Pow(10, cutoffLog);
                    AudioMixer.SetFloat("SceneLayerLowPassCutoff", cutoff);

                    AudioMixer.SetFloat("SceneLayerVolume", cutoff <= 10 
                            ? -80 : Mathf.Clamp01(Mathf.Pow(cutoffLog / Mathf.Log10(22000), .2f)) * 80 - 80);
                });
        }
    }
}