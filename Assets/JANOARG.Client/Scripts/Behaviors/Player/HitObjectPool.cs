using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Player
{
    public class HitObjectPool : MonoBehaviour
    {
        internal Stack<HitPlayer> HitobjectPool;
        
        private readonly int _Size = 512;
        private int _Borrows = 0;
        
        private void Start()
        {
            if (HitobjectPool == null || HitobjectPool.Count == 0)
            {
                HitobjectPool = new Stack<HitPlayer>(_Size);

                for (int instance = 0; instance < _Size; instance++)
                {
                    var hitObject = Instantiate(PlayerScreen.sMain.HitSample, PlayerScreen.sMain.HitPool);
                    HitobjectPool.Push(hitObject);
                    hitObject.gameObject.SetActive(false);
                }
            }
        }

        public HitPlayer BorrowHitObject(LanePlayer lane)
        {
            HitPlayer hitObject;
        
            if (_Borrows > _Size)
            {
                // Pool exhausted, create new instance
                hitObject = Instantiate(PlayerScreen.sMain.HitSample, lane.Holder);
            }
            else
            {
                // Use pooled object
                hitObject = HitobjectPool.Pop();
                hitObject.transform.SetParent(lane.Holder);
            }
        
            hitObject.gameObject.SetActive(true);
            _Borrows++;
            return hitObject;
        }
        

        public void ReturnHitObject(HitPlayer hitObject)
        {
            // Clean up hold mesh
            if (hitObject.HoldMesh != null)
            {
                hitObject.HoldMesh.gameObject.SetActive(false);
                if (hitObject.HoldMesh.mesh != null)
                    hitObject.HoldMesh.mesh.Clear();
            }
    
            // Reset state
            ResetForPool(hitObject);
    
            hitObject.gameObject.SetActive(false);
            hitObject.transform.SetParent(PlayerScreen.sMain.HitPool);
    
            // Only return to pool if we have space
            if (HitobjectPool.Count < _Size)
            {
                HitobjectPool.Push(hitObject);
            }
            else
            {
                // Pool is full, destroy excess object
                Destroy(hitObject.gameObject);
            }
    
            _Borrows--;
        }

        private static void ResetForPool(HitPlayer hitObject)
        {
            
            // Clear object references
            hitObject.Original = null;
            hitObject.Current = null;
            hitObject.Lane = null;

            // Reset timing values
            hitObject.Time = 0f;
            hitObject.EndTime = 0f;
            hitObject.CurrentPosition = 0f;

            // Clear collections
            hitObject.HoldTicks?.Clear();

            // Reset state flags
            hitObject.InDiscreteHitQueue = false;
            hitObject.PendingHoldQueue = false;
            hitObject.IsProcessed = false;
            hitObject.IsTapped = false;

            // Reset/hide visual components
            // Clean up hold mesh without destroying it
            if (hitObject.HoldMesh != null && hitObject.HoldMesh.mesh != null)
                    hitObject.HoldMesh.mesh.Clear();
    
            if (hitObject.HoldRenderer != null)
                hitObject.HoldRenderer.enabled = false;

            // Clean up flick mesh
            if (hitObject.FlickMesh != null)
            {
                if (hitObject.FlickMesh.mesh != null)
                    hitObject.FlickMesh.mesh.Clear();
            }
            
            // Reset transform to default state
            hitObject.transform.localPosition = Vector3.zero;
            hitObject.transform.localRotation = Quaternion.identity;
            hitObject.transform.localScale = Vector3.one;
        }
    }
}