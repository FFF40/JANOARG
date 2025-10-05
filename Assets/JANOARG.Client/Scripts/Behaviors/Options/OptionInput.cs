using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Options
{
    public class OptionItem : MonoBehaviour
    {
        public TMP_Text TitleLabel;
    }

    public class OptionInput<T> : OptionItem
    {
        public T CurrentValue;

        public Func<T>   OnGet;
        public Action<T> OnSet;

        public void Start()
        {
            UpdateValue();
        }

        public void Set(T value)
        {
            OnSet(value);
        }

        public void UpdateValue()
        {
            CurrentValue = OnGet();
        }
    }

    public enum MultiValueType { PerJudgment, PerHitType }

    public class MultiValueFieldData
    {
        public static Dictionary<MultiValueType, List<MultiValueFieldData>> sInfo = new()
        {
            {
                MultiValueType.PerJudgment, new List<MultiValueFieldData>
                {
                    new() { Name = "Flawless", Color = new Color(1, 1, .6f) },
                    new() { Name = "Misaligned", Color = new Color(.6f, .7f, 1) },
                    new() { Name = "Broken", Color = new Color(.6f, .6f, .6f) }
                }
            },
            {
                MultiValueType.PerHitType, new List<MultiValueFieldData>
                {
                    new() { Name = "Normal", Color = new Color(.8f, .9f, 1) },
                    new() { Name = "Catch", Color = new Color(1, 1, .8f) }
                }
            }
        };

        public Color  Color;
        public string Name;
    }
}
