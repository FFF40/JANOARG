
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.Map
{
    public abstract class MapProp : MonoBehaviour
    {
        public abstract IEnumerable<MapItem> GetDependencies();

        [NonSerialized] public bool IsDirty;

        protected virtual void OnEnable()
        {
            RegisterSelf();
        }

        protected virtual void OnDisable()
        {
            UnregisterSelf();
        }

        protected virtual void OnDirtyUpdate()
        {
            IsDirty = false;
        }

        protected void RegisterSelf()
        {
            MapManager.sProps.Add(this);
            foreach (var dependency in GetDependencies())
            {
                if (!MapManager.sPropsByDependency.ContainsKey(dependency))
                {
                    MapManager.sPropsByDependency[dependency] = new();
                }
                MapManager.sPropsByDependency[dependency].Add(this);
            }
        }

        protected void UnregisterSelf()
        {
            MapManager.sProps.Remove(this);
            foreach (var dependency in GetDependencies())
            {
                if (!MapManager.sPropsByDependency.ContainsKey(dependency))
                {
                    continue;
                }
                MapManager.sPropsByDependency[dependency].Remove(this);
                if (MapManager.sPropsByDependency[dependency].Count == 0)
                {
                    MapManager.sPropsByDependency.Remove(dependency);
                }
            }
        }

        public void SetDirty()
        {
            if (!IsDirty)
            {
                IEnumerator ScheduleDirty()
                {
                    yield return null;
                    OnDirtyUpdate();   
                }
                StartCoroutine(ScheduleDirty());
                IsDirty = true;
            }
        }
    }
}