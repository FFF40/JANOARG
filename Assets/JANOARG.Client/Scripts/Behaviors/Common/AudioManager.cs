using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.Audio;

namespace JANOARG.Client.Behaviors.Common
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager sMain;


        [Space] public AudioMixer AudioMixer;

        private EaseEnumerator _SceneLowPassCutoffRoutine;

        public void Awake()
        {
            sMain = this;
        }

        public void SetSceneLayerLowPassCutoff(float value, float duration)
        {
            _SceneLowPassCutoffRoutine?.Skip();
            _SceneLowPassCutoffRoutine = StartCoroutine(SetSceneLayerLowPassCutoffAnim(value, duration));
        }

        private EaseEnumerator SetSceneLayerLowPassCutoffAnim(float value, float duration)
        {
            AudioMixer.GetFloat("SceneLayerLowPassCutoff", out float fromLog);
            fromLog = Mathf.Log10(fromLog);
            float toLog = Mathf.Log10(value);

            return Ease.EnumAnimate(
                duration, a =>
                {
                    float cutoffLog = Mathf.Lerp(fromLog, toLog, a);
                    float cutoff = Mathf.Pow(10, cutoffLog);
                    AudioMixer.SetFloat("SceneLayerLowPassCutoff", cutoff);

                    AudioMixer.SetFloat(
                        "SceneLayerVolume",
                        cutoff <= 10
                            ? -80
                            : Mathf.Clamp01(
                                  Mathf.Pow(
                                      cutoffLog /
                                      Mathf.Log10(
                                          22000),
                                      .2f)) *
                              80 -
                              80);
                });
        }
    }
}