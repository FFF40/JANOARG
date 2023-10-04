using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formmaker : MonoBehaviour
{
    public static Formmaker main;

    public List<FormEntry> FormEntries;

    void Awake() 
    {
        main = this;
    }

    public T Spawn<T>(RectTransform target, string title = "") where T : FormEntry
    {
        foreach (FormEntry entry in FormEntries)
        {
            if (entry is T)
            {
                T item = Instantiate(entry, target) as T;
                item.Title = title;
                return item;
            }
        }
        throw new System.Exception();
    }

    public T Spawn<T, U>(RectTransform target, string title, Func<U> get, Action<U> set) where T : FormEntry<U>
    {
        foreach (FormEntry entry in FormEntries)
        {
            if (entry is T)
            {
                T item = Instantiate(entry, target) as T;
                item.Title = title;
                item.OnGet = get;
                item.OnSet = set;
                return item;
            }
        }
        throw new System.Exception("Couldn't find a sample for the Form Entry type " + typeof(T) + ". Make sure you included it into the sample list.");
    }
}
