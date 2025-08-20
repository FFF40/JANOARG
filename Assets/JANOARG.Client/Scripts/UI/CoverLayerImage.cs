using System;
using JANOARG.Shared.Script.Data.ChartInfo;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Scripts.UI
{
    public class CoverLayerImage : MonoBehaviour
    {
        [NonSerialized] public CoverLayer Layer;
        public RawImage Image;
    }
}