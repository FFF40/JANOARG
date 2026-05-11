using JANOARG.Client.Behaviors.Options.Input_Types;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.UI
{
    public class TipJar : MonoBehaviour
    {
        public Button KofiButton;
        public Button PatreonButton;
        public Button LiberapayButton;
        public Button UnityAdButton;

        private void Start()
        {
            KofiButton.onClick.AddListener(     () => Application.OpenURL("https://ko-fi.com/duducat"));
            PatreonButton.onClick.AddListener(  () => Application.OpenURL(""));
            LiberapayButton.onClick.AddListener(() => Application.OpenURL("https://en.liberapay.com/ducdat0507"));
            UnityAdButton.onClick.AddListener(  () => throw new System.NotImplementedException("Unity Ad not installed yet (use another PR)"));
        }
        
    }
}