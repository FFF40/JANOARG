using System.Collections;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JANOARG.Client.UI
{
    [ExecuteAlways] [RequireComponent(typeof(CanvasRenderer))]
    public class AnimatedToggle : Selectable, IPointerClickHandler
    {
        private bool _Value;

        public RectTransform OuterBox;
        public RectTransform InnerBox;
        public Graphic       Checkmark;

        public UnityEvent<bool> OnValueChanged;

        public bool value
        {
            get =>
                _Value;

            set
            {
                _Value = value;
                if (_Routine != null) StopCoroutine(_Routine);
                SetToggleEase(_Value ? 1 : 0);
            }
        }

        public new void Start()
        {
            base.Start();
            SetToggleEase(_Value ? 1 : 0);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) Toggle();
        }

        public void Toggle()
        {
            _Value = !_Value;
            if (_Routine != null) StopCoroutine(_Routine);
            _Routine = StartCoroutine(ToggleAnim(_Value ? 1 : 0));
            OnValueChanged.Invoke(_Value);
        }

        private float     _Progress;
        private Coroutine _Routine;

        public IEnumerator ToggleAnim(float to)
        {
            float from = _Progress;

            yield return Ease.Animate(0.2f, x =>
            {
                float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                SetToggleEase(Mathf.Lerp(from, to, ease));
            });

            _Routine = null;
        }

        public void SetToggleEase(float ease)
        {
            _Progress = ease;
            OuterBox.anchorMin = OuterBox.anchorMax = OuterBox.pivot = new Vector2(ease, .5f);
            OuterBox.anchoredPosition = Vector2.zero;
            InnerBox.sizeDelta = (OuterBox.rect.size - new Vector2(6, 4)) * (1 - ease);
            Checkmark.color = new Color(Checkmark.color.r, Checkmark.color.g, Checkmark.color.b, ease);
        }
    }
}
