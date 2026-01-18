using System.Data;
using JANOARG.Client.Behaviors.SongSelect.Shared;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.Map
{
    public abstract class MapItem : MonoBehaviour, IHasConditional
    {
        public float SafeCameraDistance = 100;

        public bool isRevealed { get; protected set; }
        public bool isUnlocked { get; protected set; }

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