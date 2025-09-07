using System;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.UI
{
    public class CoverLayerImage : MonoBehaviour
    {
        [NonSerialized] public CoverLayer Layer;
        public                 RawImage   Image;
    }
}