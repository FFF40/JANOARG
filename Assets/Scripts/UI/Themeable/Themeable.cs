using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Themeable : MonoBehaviour
{
    public virtual void SetColors() {}
}

public class Themeable<T> : Themeable where T : MonoBehaviour
{
    public T Target;

    public void OnEnable()
    {
        if (Application.IsPlaying(gameObject))
        {
            if (Themer.main) SetColors();
        }
        else
        {
            if (!Target) Target = GetComponent<T>();
        }
    }
}
