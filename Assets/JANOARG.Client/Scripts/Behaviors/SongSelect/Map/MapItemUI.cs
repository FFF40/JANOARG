using JANOARG.Client.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.Map
{
    public abstract class MapItemUI : MonoBehaviour
    {
        public MapItem parent { get; protected set; }

        public abstract void UpdatePosition();
    }

    public abstract class MapItemUI<T> : MapItemUI where T : MapItem
    {
        public new T parent
        {
            get { return (T)base.parent; }
            protected set { base.parent = value; }
        }

        public virtual void SetParent(T parent)
        {
            this.parent = parent;
            UpdatePosition();
        }

        public override void UpdatePosition()
        {
            (transform as RectTransform).position = CommonSys.sMain.MainCamera.WorldToScreenPoint(parent.transform.position);
        }
    }
}