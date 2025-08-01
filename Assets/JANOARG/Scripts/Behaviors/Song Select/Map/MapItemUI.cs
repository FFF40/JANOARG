using UnityEngine;

public abstract class MapItemUI : MonoBehaviour
{
    public MapItem Parent { get; protected set; }

    public abstract void UpdatePosition();
}

public abstract class MapItemUI<T> : MapItemUI where T : MapItem
{
    public new T Parent
    {
        get { return (T)base.Parent; }
        protected set { base.Parent = value; }
    }

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