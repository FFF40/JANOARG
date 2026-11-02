using System;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public class HeadGatekeeper : MonoBehaviour
    {
        public GameObject ExternalMap;
        
        public void Awake() => 
            ExternalMap.SetActive(ChartDirProvider.ChartsDirectoryExists());
    }
}