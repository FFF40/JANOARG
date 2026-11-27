using System;
using System.Collections;
using JANOARG.Client.UI;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils;
using JANOARG.Shared.Utils.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace JANOARG.Client.Behaviors.Common
{
    public class LoadingBar : MonoBehaviour
    {
        public static LoadingBar sMain;

        public static readonly FlavorTextEntry[] srFlavorTextEntries =
        {
            /* ----- TIPS ----- */

            // Gameplay tips
            new(
                "<i>Gameplay tip:</i>\n<b>Use wired headphones for the developer-intended ✨quality✨rhythm✨game✨experience✨."),
            new(
                "<i>Gameplay tip:</i>\n<b>Don't think about it, just feel it. And if you can't feel it, memorize it."),
            new(
                "<i>Gameplay tip:</i>\n<b>Increase your Ability Rating to improve your place in leaderboard rankings and bragging rights."),

            /* ----- FACTS ----- */

            // General game facts
            new(
                "<i>Did you know:</i>\n<b>The name <i>JANOARG</i> stands for <i>Just Another Normal, Ordinary, Acceptable Rhythm Game</i>."),
            new(
                "<i>Did you know:</i>\n<b>."),
            new(
                "<i>Did you know:</i>\n<b>Despite having “ARG” in the name, we JANOARG developers actually don't host any ARGs... yet, for now."),

            // Gameplay facts
            new(
                "<i>Did you know:</i>\n<b>The maximum score that you can obtain on any song is 1000000ppm regardless of difficulty."),
            new(
                "<i>Did you know:</i>\n<b>Catch and Flickable Hit Objects can only give Flawless or Broken judgment. They are either hit or miss."),
            new(
                "<i>Did you know:</i>\n<b>Multiple overlapping Hold Tails can be hold with just one finger. Just remember to tap the Hit Objects beforehand."),
            new(
                "<i>Did you know:</i>\n<b>Directional Flick Hit Objects have their hit detection zone extend infinitely along the back and forward directions of the flick!"),

            /* ----- OTHER ----- */

            // 🐌 is love, 🐌 is life.
            new("<b>🐌")
        };

        public static readonly FlavorTextEntry[] srCompletedStatuses =
        {
            // Always shown
            new("LOADING COMPLETE"),
            new("LOADING SUCCESS"),
            new("APPROACHING DESTINATION"),
            new("CONNECTION ESTABLISHED")
        };

        [Space]
        public Image Background;

        public TMP_Text      StatusText;
        public Image         StatusLoaderImage;
        public GraphicWiper  StatusCompletedWiper;
        public TMP_Text      StatusCompletedText;

        [Space]
        public TMP_Text FlavorText;

        [Space]
        public Slider ProgressBar;

        public RectTransform ProgressBarHolder;
        public Image         ProgressBarFill;

        [HideInInspector] public bool IsAnimating;

        private RectTransform _Self;

        public void Awake()
        {
            sMain = this;
            _Self = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);

            SetFlavorText(
                FlavorTextEntry.GetRandom(srFlavorTextEntries)
                    .Message);

            StopCoroutine(HideAnim());
            StartCoroutine(ShowAnim());
        }

        public IEnumerator ShowAnim()
        {
            IsAnimating = true;
            StatusText.text = "NOW LOADING...";
            StatusCompletedWiper.FillAmount = 0;

            Color backgroundColor = CommonSys.sMain.MainCamera.backgroundColor;
            Color foregroundColor = backgroundColor.grayscale < 0.5f ? Color.white : Color.black;

            Background.color = backgroundColor;
            StatusText.color = FlavorText.color = StatusLoaderImage.color = foregroundColor;

            yield return Ease.Animate(
                2.5f, x =>
                {
                    float lerp = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                    Background.color *= new ColorFrag(a: lerp);

                    FlavorText.color *= new ColorFrag(a: Mathf.Clamp01(x * 8));
                    StatusText.color = StatusLoaderImage.color *= new ColorFrag(a: Mathf.Clamp01(x * 8 - 0.5f));
                });

            IsAnimating = false;
        }

        public void Hide()
        {
            StopCoroutine(ShowAnim());
            StartCoroutine(HideAnim());
        }

        public IEnumerator HideAnim()
        {
            IsAnimating = true;

            StatusCompletedText.text = FlavorTextEntry.GetRandom(srCompletedStatuses).Message;
            Coroutine c1 = StartCoroutine(Ease.Animate(
                0.5f, x =>
                {
                    float lerp = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                    Background.color *= new ColorFrag(a: 1 - lerp);
                    FlavorText.color = StatusText.color = StatusLoaderImage.color *= new ColorFrag(a: 1 - lerp);
                }
            ));

            StatusCompletedWiper.Angle = 15;
            yield return Ease.Animate(
                0.4f, x =>
                {
                    float lerp = Ease.Get(x, EaseFunction.Exponential, EaseMode.Out);
                    StatusCompletedWiper.FillAmount = lerp;
                });
            StatusCompletedWiper.Angle = 195;
            yield return Ease.Animate(
                0.4f, x =>
                {
                    float lerp = Ease.Get(x, EaseFunction.Exponential, EaseMode.In);
                    StatusCompletedWiper.FillAmount = 1 - lerp;
                });

            yield return c1;

            gameObject.SetActive(false);
            IsAnimating = false;
        }

        public void Load(Func<bool> isLoaded, Action onLoad)
        {
            StartCoroutine(LoadAnim(isLoaded, onLoad));
        }

        public IEnumerator LoadAnim(Func<bool> isLoaded, Action onLoad)
        {
            yield return ShowAnim();
            yield return new WaitWhile(isLoaded);
            yield return HideAnim();
        }

        public void SetFlavorText(string text)
        {
            FlavorText.text = text;
        }
    }

    public class FlavorTextEntry
    {
        public Func<bool> Condition = () => true;
        public string     Message;

        public FlavorTextEntry(string message)
        {
            Message = message;
        }

        public FlavorTextEntry(string message, Func<bool> condition)
        {
            Message = message;
            Condition = condition;
        }

        public static FlavorTextEntry GetRandom(FlavorTextEntry[] entries)
        {
            FlavorTextEntry entry = entries[Random.Range(0, entries.Length)];

            for (var a = 0; a < 1000; a++)
            {
                if (entry.Condition()) break;

                entry = entries[Random.Range(0, entries.Length)];
            }

            return entry;
        }
    }
}
