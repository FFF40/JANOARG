using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Options
{
    public class OptionInputListItem : MonoBehaviour
    {
        public TMP_Text Text;
        public Graphic  Background;

        public Button Button;

        public Action OnSelect;
    }
}