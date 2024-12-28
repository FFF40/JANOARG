using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmoothScrollRect : ScrollRect 
{
    [SerializeField]
    private float m_ScrollSpeed = 50;

    private Coroutine m_currentRoutine = null;
    private Vector2? m_targetScrollPos = null;

    private static IEaseDirective easeFunc = new BasicEaseDirective(EaseFunction.Cubic, EaseMode.Out);

    private IEnumerator ScrollAnimation(Vector2 from, Vector2 to, float duration, IEaseDirective easeFunc)
    {
        void set(float x) 
        {
            SetContentAnchoredPosition(Vector2.Lerp(from, to, x));
        }

        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            set(easeFunc.Get(t));
            yield return null;
        }
        set(1);
        m_targetScrollPos = null;
    }

    public void StartScrollAnimation(Vector2 from, Vector2 to, float duration, IEaseDirective easeFunc) 
    {
        velocity = Vector2.zero;
        if (m_currentRoutine != null) StopCoroutine(m_currentRoutine);
        if (from == to) return;
        m_currentRoutine = StartCoroutine(ScrollAnimation(from, to, duration, easeFunc));
    }

    public void StartScrollAnimation(Vector2 to, float duration, IEaseDirective easeFunc) 
    {
        StartScrollAnimation(content.anchoredPosition, to, duration, easeFunc);
    }

    public override void OnScroll(PointerEventData data)
    {
        Vector2 from = content.anchoredPosition;
        if (m_targetScrollPos != null) content.anchoredPosition = (Vector2)m_targetScrollPos;
        base.OnScroll(data);
        Vector2 to = content.anchoredPosition;
        m_targetScrollPos = to;
        SetContentAnchoredPosition(from = Vector2.Lerp(from, to, .1f));
        float duration = Mathf.Min(Mathf.Sqrt(Vector2.Distance(from, to)) / m_ScrollSpeed, 0.2f);
        StartScrollAnimation(from, to, duration, easeFunc);
    }
}