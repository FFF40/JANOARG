using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Scripts.Behaviors.Options
{
    public class OptionInputListItem : MonoBehaviour
    {
        public TMP_Text Text;
        public Graphic Background;

        public Action OnSelect;

        public Button Button;
    }
}