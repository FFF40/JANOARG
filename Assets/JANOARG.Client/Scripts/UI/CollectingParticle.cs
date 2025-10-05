using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.Events;

namespace JANOARG.Client.UI
{
    public class CollectingParticle : MonoBehaviour
    {
        public RectTransform Target;
        public Vector2       Velocity;
        public float         SpinVelocity;
        public float         Lifetime;
        public UnityEvent    OnComplete;
        public CanvasGroup   Tail;

        private bool _IsCompleted;

        private float         _Time;
        private RectTransform _RT;
        private float         _Size = 0;

        private void Awake()
        {
            _RT = (RectTransform)transform;
            _Size = _RT.sizeDelta.x;
        }

        public void Update()
        {
            _Time += Time.deltaTime;
            Vector2 oldPosition = _RT.anchoredPosition;
            _RT.anchoredPosition += Velocity * Time.deltaTime;
            _RT.localEulerAngles += SpinVelocity * Time.deltaTime * Vector3.forward;
            Velocity += 500 * Time.deltaTime * Vector2.down;

            var parent = (RectTransform)_RT.parent;

            if (_RT.anchoredPosition.x < 0)
            {
                _RT.anchoredPosition = new Vector2(-_RT.anchoredPosition.x, _RT.anchoredPosition.y);
                Velocity = new Vector2(-0.5f * Velocity.x, Velocity.y);
            }
            else if (_RT.anchoredPosition.x > parent.rect.width)
            {
                _RT.anchoredPosition = new Vector2(parent.rect.width * 2 - _RT.anchoredPosition.x, _RT.anchoredPosition.y);
                Velocity = new Vector2(-0.5f * Velocity.x, Velocity.y);
            }

            if (_RT.anchoredPosition.y < 0)
            {
                _RT.anchoredPosition = new Vector2(_RT.anchoredPosition.x, -_RT.anchoredPosition.y);
                Velocity = new Vector2(Velocity.x, -0.5f * Velocity.y);
            }
            else if (_RT.anchoredPosition.y > parent.rect.height)
            {
                _RT.anchoredPosition = new Vector2(_RT.anchoredPosition.x, parent.rect.height * 2 - _RT.anchoredPosition.y);
                Velocity = new Vector2(Velocity.x, -0.5f * Velocity.y);
            }


            if (Lifetime - _Time <= 0)
            {
                if (_IsCompleted)
                {
                    Destroy(gameObject);
                }
                else
                {
                    OnComplete.Invoke();
                    _IsCompleted = true;
                }
            }

            if (Lifetime - _Time < .5f)
            {
                float progress = Mathf.Max(0, (Lifetime - _Time) / .5f);

                _RT.position = Vector3.Lerp(
                    Target.position, _RT.position,
                    progress == 0 ? 0 : Mathf.Pow(progress, Time.deltaTime * 2)
                );

                _RT.sizeDelta = _Size * (1 - .5f * Ease.Get(1 - progress, EaseFunction.Exponential, EaseMode.In)) * Vector2.one;

                Tail.alpha = 1 - progress;

                ((RectTransform)Tail.transform).localEulerAngles = (Vector2.SignedAngle(Vector2.down, _RT.anchoredPosition - oldPosition) - _RT.localEulerAngles.z) * Vector3.forward;
                ((RectTransform)Tail.transform).sizeDelta = new Vector2(0.5f * _RT.sizeDelta.x, Vector2.Distance(oldPosition, _RT.anchoredPosition));
            }
        }

        public void Reset()
        {
            _Time = 0;
        }
    }
}
