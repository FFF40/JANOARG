using System.Collections;
using System.Collections.Generic;
using JANOARG.Client.Utils;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Player
{
    public class PlayerScreenResultDetails : MonoBehaviour
    {
        public static PlayerScreenResultDetails sMain;

        public Graphic Container;

        [Space]
        public Graphic NormalPinSample;

        public Graphic MissPinSample;

        [Space]
        public RectTransform TimingPinHolder;

        public RectTransform TimingLabelHolder;
        public TMP_Text      TimingTitle;
        public TMP_Text      TimingEarlyTitle;
        public TMP_Text      TimingLateTitle;
        public TMP_Text      TimingBadCountLabel;
        public TMP_Text      TimingEarlyCountLabel;
        public TMP_Text      TimingCenterCountLabel;
        public TMP_Text      TimingLateCountLabel;
        public TMP_Text      TimingMissCountLabel;
        public TMP_Text      TimingTotalCountLabel;

        [Space]
        public RectTransform CatchPinHolder;

        public RectTransform CatchLabelHolder;
        public TMP_Text      CatchTitle;
        public TMP_Text      CatchHitCountLabel;
        public TMP_Text      CatchTotalCountLabel;

        [Space]
        public RectTransform FlickPinHolder;

        public RectTransform FlickLabelHolder;
        public TMP_Text      FlickTitle;
        public TMP_Text      FlickHitCountLabel;
        public TMP_Text      FlickTotalCountLabel;

        [Space]
        public List<RectTransform> Lines;

        public RectTransform EarlyLine;
        public RectTransform LateLine;

        private          int              _HistoryIndex;
        private readonly List<GameObject> _Pins = new();

        private int _BadCount,
            _EarlyCount,
            _CenterCount,
            _LateCount,
            _MissCount,
            _TotalCount,
            _CatchHitCount,
            _CatchTotalCount,
            _FlickHitCount,
            _FlickTotalCount;

        private void Awake()
        {
            sMain = this;
        }

        private void Start()
        {
            LerpDetailed(-1);
            float ratio = PlayerScreen.sMain.PerfectWindow / PlayerScreen.sMain.GoodWindow;

            EarlyLine.anchorMin *= new Vector2Frag(y: .5f + ratio / 2);
            EarlyLine.anchorMax *= new Vector2Frag(y: .5f + ratio / 2);
            LateLine.anchorMin *= new Vector2Frag(y: .5f - ratio / 2);
            LateLine.anchorMax *= new Vector2Frag(y: .5f - ratio / 2);
        }

        public void Reset()
        {
            LerpDetailed(-1);
            foreach (GameObject pin in _Pins) Destroy(pin);
            _Pins.Clear();

            _BadCount =
                _EarlyCount =
                    _CenterCount =
                        _LateCount =
                            _MissCount =
                                _TotalCount =
                                    _CatchHitCount =
                                        _CatchTotalCount =
                                            _FlickHitCount =
                                                _FlickTotalCount =
                                                    _HistoryIndex = 0;
        }

        public void UpdateLabels()
        {
            // Current score count
            TimingBadCountLabel.text = _BadCount.ToString("N0");
            TimingEarlyCountLabel.text = _EarlyCount.ToString("N0");
            TimingCenterCountLabel.text = _CenterCount.ToString("N0");
            TimingLateCountLabel.text = _LateCount.ToString("N0");
            TimingMissCountLabel.text = _MissCount.ToString("N0");

            // Per chart's total notes
            TimingTotalCountLabel.text = "/ " + _TotalCount.ToString("N0");
            CatchHitCountLabel.text = _CatchHitCount.ToString("N0");
            CatchTotalCountLabel.text = "/ " + _CatchTotalCount.ToString("N0");
            FlickHitCountLabel.text = _FlickHitCount.ToString("N0");
            FlickTotalCountLabel.text = "/ " + _FlickTotalCount.ToString("N0");
        }

        public void SpawnPins(float progress)
        {
            List<HitObjectHistoryItem> history = PlayerScreen.sMain.HitObjectHistory;

            for (; _HistoryIndex < history.Count * progress; _HistoryIndex++)
            {
                HitObjectHistoryItem item = history[_HistoryIndex];
                RectTransform parent = null;
                bool isMiss = item.Offset > PlayerScreen.sMain.GoodWindow;

                switch (item.Type)
                {
                    case HitObjectHistoryType.Timing:
                    {
                        parent = TimingPinHolder;

                        if (item.Offset < -PlayerScreen.sMain.GoodWindow) _BadCount++;
                        else if (item.Offset < -PlayerScreen.sMain.PerfectWindow) _EarlyCount++;
                        else if (item.Offset > PlayerScreen.sMain.GoodWindow) _MissCount++;
                        else if (item.Offset > PlayerScreen.sMain.PerfectWindow) _LateCount++;
                        else _CenterCount++;

                        _TotalCount++;

                        break;
                    }
                    case HitObjectHistoryType.Catch:
                    {
                        parent = CatchPinHolder;

                        if (!isMiss)
                            _CatchHitCount++;

                        _CatchTotalCount++;

                        break;
                    }
                    case HitObjectHistoryType.Flick:
                    {
                        parent = FlickPinHolder;

                        if (!isMiss)
                            _FlickHitCount++;

                        _FlickTotalCount++;

                        break;
                    }
                }


                Graphic pin = Instantiate(
                    isMiss ? MissPinSample : NormalPinSample,
                    parent);

                pin.rectTransform.anchorMin =
                    pin.rectTransform.anchorMax =
                        new Vector2(
                            item.Time / PlayerScreen.sTargetSong.Clip.length,
                            item.Type == HitObjectHistoryType.Timing
                                ? Mathf.Clamp01(.5f - item.Offset / PlayerScreen.sMain.GoodWindow / 2)
                                : 0.5f
                        );

                if (!isMiss)
                {
                    if (parent == CatchPinHolder)
                        pin.color *= new ColorFrag(a: .25f);
                    else if (parent == FlickPinHolder)
                        pin.color *= new ColorFrag(a: .5f);
                }

                _Pins.Add(pin.gameObject);
                StartCoroutine(AnimatePin(pin));
            }
        }

        private IEnumerator AnimatePin(Graphic pin)
        {
            yield return Ease.Animate(.2f, (x) =>
            {
                pin.rectTransform.localScale = Vector3.one * Ease.Get(x * 1.2f, EaseFunction.Back, EaseMode.Out);
                pin.rectTransform.localEulerAngles = Vector3.forward * (90 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));
            });
        }

        public void LerpDetailed(float value)
        {
            float abs = Mathf.Abs(value);

            float ease1 = Ease.Get(abs, EaseFunction.Exponential, EaseMode.In);

            Container.color *= new ColorFrag(a: Ease.Get(1 - ease1, EaseFunction.Cubic, EaseMode.InOut) * .8f);

            foreach (RectTransform line in Lines)
            {
                line.anchorMin *= new Vector2Frag(x: -2 * ease1);
                line.anchorMax *= new Vector2Frag(x: 1 + 2 * ease1);
            }

            Container.rectTransform.sizeDelta *= new Vector2Frag(y: 110 - 60 * ease1);
            Container.rectTransform.anchoredPosition *= new Vector2Frag(y: 25 * (1 - ease1));
            TimingPinHolder.sizeDelta = new Vector2(100 * ease1 - 120, -80 + 40 * ease1);
            CatchPinHolder.anchoredPosition *= new Vector2Frag(y: 35 - 20 * ease1);
            CatchPinHolder.sizeDelta = new Vector2(100 * ease1 - 120, 20 - 10 * ease1);
            FlickPinHolder.anchoredPosition *= new Vector2Frag(y: 10 - 5 * ease1);
            FlickPinHolder.sizeDelta = new Vector2(100 * ease1 - 120, 20 - 10 * ease1);

            var anchoredPositionFrag = new Vector2Frag(x: -1000 * ease1);
            TimingLabelHolder.anchoredPosition *= anchoredPositionFrag;
            CatchLabelHolder.anchoredPosition *= anchoredPositionFrag;
            FlickLabelHolder.anchoredPosition *= anchoredPositionFrag;

            float f_getLerp(float x)
            {
                return Mathf.Clamp01(2 - Mathf.Abs(value + (value + 1) / 2 - x) * 2);
            }

            TimingEarlyTitle.alpha = Ease.Get(f_getLerp(0), EaseFunction.Cubic, EaseMode.Out) / 2;
            TimingLateTitle.alpha = Ease.Get(f_getLerp(.2f), EaseFunction.Cubic, EaseMode.Out) / 2;
            TimingTitle.alpha = Ease.Get(f_getLerp(.4f), EaseFunction.Cubic, EaseMode.Out);
            CatchTitle.alpha = Ease.Get(f_getLerp(.6f), EaseFunction.Cubic, EaseMode.Out);
            FlickTitle.alpha = Ease.Get(f_getLerp(.8f), EaseFunction.Cubic, EaseMode.Out);
        }
    }
}
