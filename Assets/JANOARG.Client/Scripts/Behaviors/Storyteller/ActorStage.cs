using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Data.Constant;
using JANOARG.Shared.Utils.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Storyteller
{
    public class StoryActorStage : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _StageRoot;
        [SerializeField] public CanvasGroup CanvasGroup;
        [SerializeField] private Image _ActorPrefab;
        [SerializeField] private StoryActorConstants _ActorConstants;

        [Header("Position Markers")]
        [SerializeField] private RectTransform _LeftPoint;
        [SerializeField] private RectTransform _MiddlePoint;
        [SerializeField] private RectTransform _RightPoint;
        [SerializeField] private RectTransform _OutLeftPoint;
        [SerializeField] private RectTransform _OutRightPoint;

        [Header("Actor Spacing")]
        [SerializeField] private float _ActorSpacing = 150f;
        [SerializeField] private float _RearrangeDuration = 0.35f;
        private readonly Dictionary<string, Image> _Actors = new();
        private readonly Dictionary<string, Coroutine> _Animations = new();
        private readonly Dictionary<string, ActorPosition> _Assignments = new();
        private readonly Dictionary<ActorPosition, StagePosition> _StagePositions = new();

        private void Awake()
        {
            foreach (ActorPosition position in Enum.GetValues(typeof(ActorPosition)))
                _StagePositions[position] = new StagePosition(position);
        }

        private static bool IsOffstage(ActorPosition p) =>
            p == ActorPosition.OutLeft || p == ActorPosition.OutRight;

        private RectTransform GetPoint(ActorPosition position)
        {
            return position switch
            {
                ActorPosition.Left => _LeftPoint,
                ActorPosition.Middle => _MiddlePoint,
                ActorPosition.Right => _RightPoint,
                ActorPosition.OutLeft => _OutLeftPoint,
                ActorPosition.OutRight => _OutRightPoint,
                _ => null
            };
        }

        private Vector2 GetPosition(ActorPosition position)
        {
            RectTransform point = GetPoint(position);

            if (point == null) throw new InvalidOperationException($"No marker assigned for {position}");

            // Convert the marker's world position into stage-local coordinates.
            return _StageRoot.InverseTransformPoint(point.position);
        }

        private Image CreateActor(string alias)
        {
            Image image = Instantiate(_ActorPrefab, _StageRoot);
            image.name = $"Actor_{alias}";
            image.preserveAspect = true;
            image.color = new Color(1f, 1f, 1f, 0f);
            image.gameObject.SetActive(true);

            _Actors.Add(alias, image);
            return image;
        }

        private void DestroyActor(string alias)
        {
            StopTween(alias);

            if (_Actors.TryGetValue(alias, out Image image))
            {
                _Actors.Remove(alias);
                if (image != null) Destroy(image.gameObject);
            }
        }

        /// <summary>
        ///  Add actor to a position group
        /// </summary>
        private void Attach(string alias, ActorPosition position)
        {
            _StagePositions[position].Add(alias);
            _Assignments[alias] = position;
        }

        /// <summary>
        /// Remove actor from its group and slides the remaning actors to thier new position
        /// </summary>
        private void Detach(string alias, float duration)
        {
            if (!_Assignments.TryGetValue(alias, out ActorPosition previous))
                return;

            _Assignments.Remove(alias);
            _StagePositions[previous].Remove(alias);
            Layout(previous, duration);
        }

        // Adjust actor's position in every stage position
        private void Layout(ActorPosition position, float duration, string newActorAlias = null)
        {
            StagePosition group = _StagePositions[position];
            int count = group.count;
            if (count == 0) return;

            Vector2 center = GetPosition(position);

            for (int i = 0; i < count; i++)
            {
                string alias = group.aliases[i];
                float offset = (i - (count - 1) / 2f) * _ActorSpacing;
                Vector2 target = center + Vector2.right * offset;

                // Insta-teleport if actor is new to the stage
                StartTween(alias, target, alias == newActorAlias ? 0f : duration);
            }
        }

        private void StartTween(string alias, Vector2 target, float duration, Action onComplete = null)
        {
            StopTween(alias);

            if (!_Actors.TryGetValue(alias, out Image image) || image == null)
                return;

            RectTransform rect = image.rectTransform;

            // Snap to position
            if (duration <= 0f || Vector2.Distance(rect.anchoredPosition, target) < 0.01f)
            {
                rect.anchoredPosition = target;
                onComplete?.Invoke();
                return;
            }

            _Animations[alias] = StartCoroutine(TweenRoutine(alias, rect, target, duration, onComplete));
        }

        private IEnumerator TweenRoutine(string alias, RectTransform rect, Vector2 target, float duration, Action onComplete)
        {
            Vector2 start = rect.anchoredPosition;

            yield return Ease.Animate(duration, a =>
            {
                if (rect == null) return;
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                rect.anchoredPosition = Vector2.Lerp(start, target, lerp);
            });

            if (rect == null) yield break;

            rect.anchoredPosition = target;
            _Animations.Remove(alias);
            onComplete?.Invoke();
        }

        private void StopTween(string alias)
        {
            if (_Animations.TryGetValue(alias, out Coroutine routine))
            {
                if (routine != null) StopCoroutine(routine);
                _Animations.Remove(alias);
            }
        }

        public IEnumerator ShowActor(string alias, string spriteID, ActorPosition position, bool bringToFront = true)
        {
            ActorInfo actor = _ActorConstants.GetActor(alias);

            if (actor == null)
            {
                Debug.LogWarning($"Actor not found: {alias}");
                yield break;
            }

            ActorSprite sprite = Array.Find(actor.Sprites, s => s.ID == spriteID);

            if (sprite == null || sprite.Sprite == null)
            {
                Debug.LogWarning($"Sprite '{spriteID}' not found for '{alias}'");
                yield break;
            }

            if (!_Actors.TryGetValue(alias, out Image image))
                image = CreateActor(alias);

            StopTween(alias);

            image.sprite = sprite.Sprite;
            image.enabled = true;

            if (bringToFront)
                image.transform.SetAsLastSibling();

            if (IsOffstage(position))
            {
                Detach(alias, _RearrangeDuration);
                image.rectTransform.anchoredPosition = GetPosition(position);
                image.color = Color.white;
                yield break;
            }

            bool alreadyThere = _Assignments.TryGetValue(alias, out ActorPosition current) && current == position;

            if (alreadyThere)
            {
                // Only the sprite changed. Make sure the actor sits in its slot.
                Layout(position, _RearrangeDuration);
            }
            else
            {
                Detach(alias, _RearrangeDuration);
                Attach(alias, position);
                // The newcomer pops into its slot (and fades in) while others slide apart.
                Layout(position, _RearrangeDuration, newActorAlias: alias);
            }

            // Fade in from whatever alpha the actor currently has.
            float startAlpha = image.color.a;
            if (startAlpha >= 1f)
                yield break;

            yield return Ease.Animate(1f, a =>
            {
                if (image == null) return;
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                image.color = new Color(1f, 1f, 1f, Mathf.Lerp(startAlpha, 1f, lerp));
            });
        }

        public IEnumerator MoveActor(string alias, ActorPosition destination, float duration = 0.5f, bool bringToFront = false)
        {
            if (!_Actors.TryGetValue(alias, out Image image))
            {
                Debug.LogWarning($"Actor is not on stage: {alias}");
                yield break;
            }

            if (IsOffstage(destination))
            {
                // Leave the old group (others close the gap), walk off, then destroy.
                Detach(alias, _RearrangeDuration);
                StartTween(alias, GetPosition(destination), duration, () => DestroyActor(alias));
            }
            else
            {
                bool alreadyThere = _Assignments.TryGetValue(alias, out ActorPosition current) && current == destination;

                if (!alreadyThere)
                {
                    Detach(alias, duration);
                    Attach(alias, destination);
                }

                // Everyone in the destination group (including the mover) tweens to their slot.
                Layout(destination, duration);
            }

            if (bringToFront)
                image.transform.SetAsLastSibling();
        }

        public void HideActor(string alias)
        {
            StopTween(alias);
            Detach(alias, _RearrangeDuration);
            DestroyActor(alias);
        }

        public void ClearActors()
        {
            foreach (string alias in new List<string>(_Actors.Keys))
            {
                StopTween(alias);
                _Assignments.Remove(alias);
                DestroyActor(alias);
            }

            foreach (StagePosition group in _StagePositions.Values)
                group.aliases.ToList().ForEach(a => group.Remove(a));
        }

        public void BringToFront(string alias)
        {
            if (_Actors.TryGetValue(alias, out Image image))
                image.transform.SetAsLastSibling();
        }

        public void SendToBack(string alias)
        {
            if (_Actors.TryGetValue(alias, out Image image))
                image.transform.SetAsFirstSibling();
        }

        public IEnumerator HideAllActors() => FadeGroup(1f, 0f);
        public IEnumerator ShowAllActors() => FadeGroup(0f, 1f);

        private IEnumerator FadeGroup(float from, float to)
        {
            yield return Ease.Animate(1f, a =>
            {
                float lerp = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                CanvasGroup.alpha = Mathf.Lerp(from, to, lerp);
            });
        }

        public IEnumerator Flip(string alias, float duration = 0.25f)
        {
            if (!_Actors.TryGetValue(alias, out Image image))
                yield break;

            RectTransform rect = image.rectTransform;

            float startX = rect.localScale.x;
            float endX = startX >= 0f ? -Mathf.Abs(startX) : Mathf.Abs(startX);

            yield return Ease.Animate(duration, a =>
            {
                if (rect == null) return;

                float t = Ease.Get(a, EaseFunction.Cubic, EaseMode.Out);
                Vector3 scale = rect.localScale;
                scale.x = Mathf.Lerp(startX, endX, t);
                rect.localScale = scale;
            });
        }

    }

    public enum ActorPosition
    {
        Left,
        Middle,
        Right,
        OutLeft,
        OutRight
    }

    public class StagePosition
    {
        private readonly List<string> _Aliases = new();

        public ActorPosition actorPosition { get; }
        public IReadOnlyList<string> aliases => _Aliases;
        public int count => _Aliases.Count;

        public StagePosition(ActorPosition position) => actorPosition = position;

        public void Add(string alias)
        {
            if (!_Aliases.Contains(alias))
                _Aliases.Add(alias);
        }

        public bool Remove(string alias) => _Aliases.Remove(alias);
        public bool Contains(string alias) => _Aliases.Contains(alias);
    }
    

}