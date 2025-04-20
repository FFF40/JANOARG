using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectingParticle : MonoBehaviour
{
    public RectTransform Target;
    public Vector2 Velocity;
    public float Lifetime;
    public UnityEvent OnComplete;

    float time;
    RectTransform rt;

    void Awake()
    {
        rt = (RectTransform)transform;
    }

    public void Update()
    {
        time += Time.deltaTime;
        rt.anchoredPosition += Velocity * Time.deltaTime;
        Velocity += 300 * Time.deltaTime * Vector2.down;
        if (Lifetime - time <= 0) 
        {
            OnComplete.Invoke();
            Destroy(gameObject);
        }   
        else if (Lifetime - time < .5f) 
        {
            rt.position = Vector3.Lerp(
                Target.position, rt.position, 
                Mathf.Pow((Lifetime - time) / .5f, Time.deltaTime * 2)
            );
        }
    }
}
