using UnityEngine;

namespace JANOARG.Chartmaker.UI.Themeable
{
    [ExecuteAlways]
    public abstract class Themeable : MonoBehaviour
    {
        public virtual void SetColors() {}
    }

    public class Themeable<T> : Themeable where T : MonoBehaviour
    {
        public T Target;

        public void OnEnable()
        {
            if (Application.IsPlaying(gameObject) && Themer.sMain)
                SetColors();
            else if (!Target)
                Target = GetComponent<T>();
        }
    }
}