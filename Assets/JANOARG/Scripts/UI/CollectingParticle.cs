using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CollectingParticle : MonoBehaviour
{
    public RectTransform Target;
    public Vector2 Velocity;
    public float SpinVelocity;
    public float Lifetime;
    public UnityEvent OnComplete;
    public CanvasGroup Tail;

    bool IsCompleted;

    float time;
    RectTransform rt;
    float size = 0;

    void Awake()
    {
        rt = (RectTransform)transform;
        size = rt.sizeDelta.x;
    }

    public void Update()
    {
        time += Time.deltaTime;
        var oldPos = rt.anchoredPosition;
        rt.anchoredPosition += Velocity * Time.deltaTime;
        rt.localEulerAngles += SpinVelocity * Time.deltaTime * Vector3.forward;
        Velocity += 500 * Time.deltaTime * Vector2.down;

        RectTransform parent = (RectTransform)rt.parent;

        if (rt.anchoredPosition.x < 0)
        {
            rt.anchoredPosition = new (-rt.anchoredPosition.x, rt.anchoredPosition.y);
            Velocity = new (-0.5f * Velocity.x, Velocity.y);
        }
        else if (rt.anchoredPosition.x > parent.rect.width)
        {
            rt.anchoredPosition = new (parent.rect.width * 2 - rt.anchoredPosition.x, rt.anchoredPosition.y);
            Velocity = new (-0.5f * Velocity.x, Velocity.y);
        }
        if (rt.anchoredPosition.y < 0)
        {
            rt.anchoredPosition = new (rt.anchoredPosition.x, -rt.anchoredPosition.y);
            Velocity = new (Velocity.x, -0.5f * Velocity.y);
        }
        else if (rt.anchoredPosition.y > parent.rect.height)
        {
            rt.anchoredPosition = new (rt.anchoredPosition.x, parent.rect.height * 2 - rt.anchoredPosition.y);
            Velocity = new (Velocity.x, -0.5f * Velocity.y);
        }


        if (Lifetime - time <= 0) 
        {
            if (IsCompleted) Destroy(gameObject);
            else 
            {
                OnComplete.Invoke();
                IsCompleted = true;
            }
        }   
        if (Lifetime - time < .5f) 
        {
            float prog = Mathf.Max(0, (Lifetime - time) / .5f);
            rt.position = Vector3.Lerp(
                Target.position, rt.position, 
                prog == 0 ? 0 : Mathf.Pow(prog, Time.deltaTime * 2)
            );
            rt.sizeDelta = size * (1 - .5f * Ease.Get(1 - prog, EaseFunction.Exponential, EaseMode.In)) * Vector2.one;
            Tail.alpha = 1 - prog;
            ((RectTransform)Tail.transform).localEulerAngles = (Vector2.SignedAngle(Vector2.down, rt.anchoredPosition - oldPos) - rt.localEulerAngles.z) * Vector3.forward;
            ((RectTransform)Tail.transform).sizeDelta = new (0.5f * rt.sizeDelta.x, Vector2.Distance(oldPos, rt.anchoredPosition));
        }
    }

    public void Reset()
    {
        time = 0;
    }
}
