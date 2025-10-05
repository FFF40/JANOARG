using System.Data;
using UnityEngine;

public abstract class MapItem : MonoBehaviour
{
    public float SafeCameraDistance = 100;

    protected void OnEnable()
    {
        MapManager.Items.Add(this);
    }

    protected void OnDisable()
    {
        MapManager.Items.Remove(this);
    }

    public virtual void UpdateStatus() { }



    public TItem MakeItemUI<TItem>() where TItem : MapItemUI
    {
        return MapManager.main.MakeItemUI<TItem>();
    }
    public TItem MakeItemUI<TItem, TParent>() where TParent : MapItem where TItem : MapItemUI<TParent>
    {
        return MapManager.main.MakeItemUI<TItem, TParent>((TParent)this);
    }

    
    
}