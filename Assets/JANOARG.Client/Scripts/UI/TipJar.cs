using JANOARG.Client.Behaviors.Options.Input_Types;
using UnityEngine;

namespace JANOARG.Client.UI
{
    public class TipJar : MonoBehaviour
    {
        [SerializeField] private OptionButton Kofi;
        [SerializeField] private OptionButton Patreon;
        [SerializeField] private OptionButton LiberaPay;
        [SerializeField] private OptionButton UnityAd;

        private void Start()
        {
            Kofi.Action    = () => Application.OpenURL("https://ko-fi.com/duducat");
            //Patreon.Action = () => Application.OpenURL("");
            LiberaPay.Action = () => Application.OpenURL("https://en.liberapay.com/ducdat0507");
            UnityAd.Action = () => throw new System.NotImplementedException("Unity Ad not installed yet (use another PR)");
        }
        
    }
}