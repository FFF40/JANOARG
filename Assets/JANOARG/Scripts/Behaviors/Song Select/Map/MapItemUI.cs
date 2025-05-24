using UnityEngine;

public abstract class MapItemUI : MonoBehaviour
{

}

public abstract class MapItemUI<T> : MapItem where T : MapItem
{
    public T Parent;

    public virtual void SetParent(T parent)
    {
        Parent = parent;
    }
}