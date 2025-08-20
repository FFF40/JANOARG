using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Shared.Script.Data.ChartInfo
{
    [CreateAssetMenu(fileName = "New Playlist", menuName = "JANOARG/Playlist", order = 100)]
    public class Playlist : ScriptableObject
    {
        public List<string> ItemPaths;
    }
}


