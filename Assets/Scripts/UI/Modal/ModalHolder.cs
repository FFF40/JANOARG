using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalHolder : MonoBehaviour
{
    public static ModalHolder main;

    public List<Modal> Modals;

    void Awake() 
    {
        main = this;
    }

    public T Spawn<T>() where T : Modal
    {
        foreach (Modal modal in Modals)
        {
            if (modal is T)
            {
                return Instantiate(modal, transform) as T;
            }
        }
        throw new System.Exception();
    }
}
