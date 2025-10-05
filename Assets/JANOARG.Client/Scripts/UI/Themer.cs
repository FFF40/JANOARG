using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.UI
{
    public class Themer : MonoBehaviour
    {
        public static Dictionary<string, List<Themer>> sThemers = new();
        public static Dictionary<string, Color>        sColors  = new();

        public string    Key;
        public Graphic[] Targets;

        private void OnEnable()
        {
            if (!sThemers.ContainsKey(Key)) sThemers[Key] = new List<Themer>();

            sThemers[Key]
                .Add(this);

            UpdateColors();
        }

        private void OnDisable()
        {
            sThemers[Key]
                .Remove(this);
        }

        public static void SetColor(string key, Color color)
        {
            sColors[key] = color;

            if (!sThemers.ContainsKey(key)) return;

            foreach (Themer t in sThemers[key]) t.UpdateColors();
        }

        public void UpdateColors()
        {
            if (!sColors.ContainsKey(Key)) return;

            Color color = sColors[Key];
            foreach (Graphic g in Targets) g.color = color;
        }
    }
}
