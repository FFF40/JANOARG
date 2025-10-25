using System.Data;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.Map
{
    public abstract class MapItem : MonoBehaviour
    {
        public float SafeCameraDistance = 100;

        protected void OnEnable()
        {
            MapManager.sItems.Add(this);
        }

        protected void OnDisable()
        {
            MapManager.sItems.Remove(this);
        }

        public virtual void UpdateStatus() { }



        public TItem MakeItemUI<TItem>() where TItem : MapItemUI
        {
            return MapManager.sMain.MakeItemUI<TItem>();
        }
        public TItem MakeItemUI<TItem, TParent>() where TParent : MapItem where TItem : MapItemUI<TParent>
        {
            return MapManager.sMain.MakeItemUI<TItem, TParent>((TParent)this);
        }

    
    
    }
}