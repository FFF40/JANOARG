using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingSpinner : MonoBehaviour
{
    public RectTransform Spinner;

    float Timer;

    IEaseDirective SpinEasing = new CubicBezierEaseDirective(.2f, 1.5f, 0, 1);

    void Start()
    {
        Timer = 0;  
    }

    void Update()
    {
        Timer += Time.deltaTime;
        float interval = .8f;
        Spinner.localEulerAngles = 
            (Mathf.Floor(Timer / interval) + SpinEasing.Get(Timer % interval))
            * 45 * Vector3.back;  
    }
}