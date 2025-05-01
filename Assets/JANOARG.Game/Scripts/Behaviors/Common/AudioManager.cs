using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager main;


    [Space]
    public AudioMixer AudioMixer;

    public void Awake() 
    {
        main = this;
    }

    Coroutine SceneLowPassCutoffRoutine;
    public void SetSceneLayerLowPassCutoff(float value, float duration) 
    {
        if (SceneLowPassCutoffRoutine != null) StopCoroutine(SceneLowPassCutoffRoutine);
        SceneLowPassCutoffRoutine = StartCoroutine(SetSceneLayerLowPassCutoffAnim(value, duration));
    }

    IEnumerator SetSceneLayerLowPassCutoffAnim(float value, float duration)
    {
        AudioMixer.GetFloat("SceneLayerLowPassCutoff", out float fromLog);
        fromLog = Mathf.Log10(fromLog);
        float toLog = Mathf.Log10(value);
        yield return Ease.Animate(duration, a => {
            float cutoffLog = Mathf.Lerp(fromLog, toLog, a);
            float cutoff = Mathf.Pow(10, cutoffLog);
            AudioMixer.SetFloat("SceneLayerLowPassCutoff", cutoff);
            AudioMixer.SetFloat("SceneLayerVolume", cutoff <= 10 ? -80 : Mathf.Clamp01(Mathf.Pow(cutoffLog / Mathf.Log10(22000), .2f)) * 80 - 80);
        });
    }
}