using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Client.UI
{
    public class LoadingSpinner : MonoBehaviour
    {
        public RectTransform Spinner;

        private float _Timer;

        private IEaseDirective _SpinEasing = new CubicBezierEaseDirective(.2f, 1.5f, 0, 1);

        private void Start()
        {
            _Timer = 0;
        }

        private void Update()
        {
            _Timer += Time.deltaTime;
            var interval = .8f;
            Spinner.localEulerAngles =
                (Mathf.Floor(_Timer / interval) + _SpinEasing.Get(_Timer % interval))
                * 45 * Vector3.back;
        }
    }
}