using UnityEngine;

public abstract class MapItemUI : MonoBehaviour
{
    public abstract void UpdatePosition();
}

public abstract class MapItemUI<T> : MapItemUI where T : MapItem
{
    public T Parent;

    public virtual void SetParent(T parent)
    {
        Parent = parent;
        UpdatePosition();
    }

    public override void UpdatePosition()
    {
        (transform as RectTransform).position = Common.main.MainCamera.WorldToScreenPoint(Parent.transform.position);
    }
}