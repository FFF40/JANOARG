

using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.List
{
    public abstract class SongSelectItemUI : MonoBehaviour
    {
        public SongSelectListItem Target { get; protected set; }

        public abstract void UpdatePosition(float offset);
    }

    public abstract class SongSelectItemUI<T> : SongSelectItemUI where T : SongSelectListItem
    {
        public new T Target
        {
            get { return (T)base.Target; }
            protected set { base.Target = value; }
        }

        public override void UpdatePosition(float offset)
        {
            RectTransform rt = (RectTransform)transform;
            rt.anchoredPosition = new Vector2(-.26795f, -1) * (Target.Position + Target.PositionOffset - offset);
        }
    }
}