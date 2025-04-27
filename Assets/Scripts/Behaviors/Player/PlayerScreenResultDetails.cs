using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerScreenResultDetails : MonoBehaviour
{
    public static PlayerScreenResultDetails main;

    public Graphic Container;
    [Space]
    public Graphic NormalPinSample;
    public Graphic MissPinSample;
    [Space]
    public RectTransform TimingPinHolder;
    public RectTransform TimingLabelHolder;
    public TMP_Text TimingTitle;
    public TMP_Text TimingEarlyTitle;
    public TMP_Text TimingLateTitle;
    public TMP_Text TimingBadCountLabel;
    public TMP_Text TimingEarlyCountLabel;
    public TMP_Text TimingCenterCountLabel;
    public TMP_Text TimingLateCountLabel;
    public TMP_Text TimingMissCountLabel;
    public TMP_Text TimingTotalCountLabel;
    [Space]
    public RectTransform CatchPinHolder;
    public RectTransform CatchLabelHolder;
    public TMP_Text CatchTitle;
    public TMP_Text CatchHitCountLabel;
    public TMP_Text CatchTotalCountLabel;
    [Space]
    public RectTransform FlickPinHolder;
    public RectTransform FlickLabelHolder;
    public TMP_Text FlickTitle;
    public TMP_Text FlickHitCountLabel;
    public TMP_Text FlickTotalCountLabel;
    [Space]
    public List<RectTransform> Lines;
    public RectTransform EarlyLine;
    public RectTransform LateLine;

    int HistoryIndex;
    List<GameObject> Pins = new();

    int TimingBadCount,
        TimingEarlyCount,
        TimingCenterCount,
        TimingLateCount,
        TimingMissCount,
        TimingTotalCount,
        CatchHitCount,
        CatchTotalCount,
        FlickHitCount,
        FlickTotalCount;

    void Awake() 
    {
        main = this;
    }

    void Start()
    {
        LerpDetailed(-1);
        float ratio = PlayerScreen.main.PerfectWindow / PlayerScreen.main.GoodWindow;
        EarlyLine.anchorMin *= new Vector2Frag(y: .5f + ratio / 2);
        EarlyLine.anchorMax *= new Vector2Frag(y: .5f + ratio / 2);
        LateLine.anchorMin *= new Vector2Frag(y: .5f - ratio / 2);
        LateLine.anchorMax *= new Vector2Frag(y: .5f - ratio / 2);
    }

    public void Reset()
    {
        LerpDetailed(-1);
        foreach (var pin in Pins) Destroy(pin);
        Pins.Clear();
        TimingBadCount = TimingEarlyCount = TimingCenterCount = TimingLateCount = TimingMissCount
            = TimingTotalCount = CatchHitCount = CatchTotalCount = FlickHitCount = FlickTotalCount
            = HistoryIndex = 0;
    }

    public void UpdateLabels()
    {
        TimingBadCountLabel.text = TimingBadCount.ToString("N0");
        TimingEarlyCountLabel.text = TimingEarlyCount.ToString("N0");
        TimingCenterCountLabel.text = TimingCenterCount.ToString("N0");
        TimingLateCountLabel.text = TimingLateCount.ToString("N0");
        TimingMissCountLabel.text = TimingMissCount.ToString("N0");
        TimingTotalCountLabel.text = "/ " + TimingTotalCount.ToString("N0");
        CatchHitCountLabel.text = CatchHitCount.ToString("N0");
        CatchTotalCountLabel.text = "/ " + CatchTotalCount.ToString("N0");
        FlickHitCountLabel.text = FlickHitCount.ToString("N0");
        FlickTotalCountLabel.text = "/ " + FlickTotalCount.ToString("N0");
    }

    public void SpawnPins(float progress) 
    {
        var history = PlayerScreen.main.HitObjectHistory;
        for (; HistoryIndex < history.Count * progress; HistoryIndex++)
        {
            var item = history[HistoryIndex];
            RectTransform parent = null;
            bool isMiss = item.Offset > PlayerScreen.main.GoodWindow;
            
            switch (item.Type) 
            {
                case HitObjectHistoryType.Timing: {
                    parent = TimingPinHolder; 
                    if (item.Offset < -PlayerScreen.main.GoodWindow) TimingBadCount++;
                    else if (item.Offset < -PlayerScreen.main.PerfectWindow) TimingEarlyCount++;
                    else if (item.Offset > PlayerScreen.main.GoodWindow) TimingMissCount++;
                    else if (item.Offset > PlayerScreen.main.PerfectWindow) TimingLateCount++;
                    else TimingCenterCount++;
                    TimingTotalCount++;
                    break;
                }
                case HitObjectHistoryType.Catch: {
                    parent = CatchPinHolder; 
                    if (!isMiss) CatchHitCount++;
                    CatchTotalCount++;
                    break;
                }
                case HitObjectHistoryType.Flick: {
                    parent = FlickPinHolder; 
                    if (!isMiss) FlickHitCount++;
                    FlickTotalCount++;
                    break;
                }
            };

            var pin = Instantiate(isMiss ? MissPinSample : NormalPinSample, parent);
            pin.rectTransform.anchorMin = pin.rectTransform.anchorMax = new (
                item.Time / PlayerScreen.TargetSong.Clip.length,
                item.Type == HitObjectHistoryType.Timing ? Mathf.Clamp01(.5f - item.Offset / PlayerScreen.main.GoodWindow / 2) : 0.5f
            );
            if (!isMiss) 
            {
                if (parent == CatchPinHolder) pin.color *= new ColorFrag(a: .25f);
                else if (parent == FlickPinHolder) pin.color *= new ColorFrag(a: .5f);
            }
            Pins.Add(pin.gameObject);
            StartCoroutine(AnimatePin(pin));
        }
    }

    IEnumerator AnimatePin(Graphic pin)
    {
        yield return Ease.Animate(.2f, (x) => {
            pin.rectTransform.localScale = Vector3.one * Ease.Get(x * 1.2f, EaseFunction.Back, EaseMode.Out);
            pin.rectTransform.localEulerAngles = Vector3.forward * (90 * Ease.Get(x, EaseFunction.Cubic, EaseMode.Out));
        });
    }

    public void LerpDetailed(float value) 
    {
        float abs = Mathf.Abs(value);

        float ease1 = Ease.Get(abs, EaseFunction.Exponential, EaseMode.In);
        Container.color *= new ColorFrag(a: Ease.Get(1 - ease1, EaseFunction.Cubic, EaseMode.InOut) * .8f);
        foreach (var line in Lines) 
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

        TimingLabelHolder.anchoredPosition *= new Vector2Frag(x: -1000 * ease1);
        CatchLabelHolder.anchoredPosition *= new Vector2Frag(x: -1000 * ease1);
        FlickLabelHolder.anchoredPosition *= new Vector2Frag(x: -1000 * ease1);

        float getLerp(float x) => Mathf.Clamp01(2 - Mathf.Abs(value + (value + 1) / 2 - x) * 2);
        TimingEarlyTitle.alpha = Ease.Get(getLerp(0),   EaseFunction.Cubic, EaseMode.Out) / 2;
        TimingLateTitle.alpha  = Ease.Get(getLerp(.2f), EaseFunction.Cubic, EaseMode.Out) / 2;
        TimingTitle.alpha      = Ease.Get(getLerp(.4f), EaseFunction.Cubic, EaseMode.Out);
        CatchTitle.alpha       = Ease.Get(getLerp(.6f), EaseFunction.Cubic, EaseMode.Out);
        FlickTitle.alpha       = Ease.Get(getLerp(.8f), EaseFunction.Cubic, EaseMode.Out);
    }
}
