using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.Options.Input_Types;
using JANOARG.Shared.Data.ChartInfo;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace JANOARG.Client.Behaviors.Options
{
    public class OptionCalibrationWizard : MonoBehaviour
    {
        public AudioSource CalibrationLoopPlayer;
        public AudioClip   CalibrationLoop;
        public float       CalibrationLoopBPM;

        [Space] public FloatOptionInput CurrentOptionInput;

        public float CurrentTime;
        public float SyncThreshold  = 0.03f;
        public int   TrialThreshold = 4;
        public float SyncOffset;

        [Space] 
        public GameObject JudgmentOffsetHolder;
        public GameObject AverageOffsetHolder;
        public GameObject VisualOffsetHolder;

        [Space]
        public Image VisualOffsetLeft;
        public Image VisualOffsetRight;

        [Space]
        public Button AverageOffsetApplyButton;
        public TMP_Text AverageOffsetValueLabel;

        [Space] 
        public GameObject InputHolder;
        public TMP_InputField InputField;
        public TMP_Text       InputFieldUnit;

        [Space] 
        public  Image Background;
        public  CanvasGroup FaderGroup;
        public  CanvasGroup AverageOffsetCanvasGroup;
        public  TMP_Text    InfoLabel;
        public  TMP_Text    AverageOffsetInstructionLabel;
        public  TMP_Text    JudgmentOffsetInstructionLabel;
        public  TMP_Text    VisualOffsetInstructionLabel;
        private float       _CumulativeOffset;
        private List<float> _TrialOffsets = new();

        // Median of wizard taps — attach to settings scene to surface to the player.
        // Median from wizard taps — attach to settings scene.
        public double MedianTimingOffset;

        // Last gameplay-derived median passed in from PlayerScreen via prefs.
        // Separate from tap sampling — for display/comparison only, not directly applied.
        public double GameplayMedianOffset;

        private EaseEnumerator _CurrentAnim;

        private bool _IsActive;

        private double _SongStartDSP = 0.1f;
        
        private int    _LastTrialIndex;
        private int    _Samples;
        

        public void Start()
        {
            JudgmentOffsetHolder.SetActive(false);
            AverageOffsetHolder.SetActive(false);
            VisualOffsetHolder.SetActive(false);
            InputHolder.SetActive(false);
            gameObject.SetActive(false);
        }

        public void Update()
        {
            if (!_IsActive)
                return;

            double dspNow = AudioSettings.dspTime;

            // Absolute, drift-free timeline
            CurrentTime = (float)(dspNow - _SongStartDSP);

            if (CurrentOptionInput is AudioOffsetOptionInput)
            {
                float loopLength = 60f / CalibrationLoopBPM * 32f;

                // Use AUDIO as truth, not your timeline
                float audioTime =
                    (float)CalibrationLoopPlayer.timeSamples /
                    CalibrationLoopPlayer.clip.frequency;

                float loopTime = audioTime % loopLength;

                if (CurrentOptionInput is AudioOffsetOptionInput)
                {
                    Debug.Log(loopTime + "\n" + audioTime);
                    // Intentionally no correction here
                }
            }
            else if (CurrentOptionInput is VisualOffsetOptionInput)
            {
                float trialDuration = 60f / CalibrationLoopBPM * 4f;

                float time = (CurrentTime / trialDuration) % 1f;

                float pos = Ease.Get(time, EaseFunction.Quartic, EaseMode.InOut) * 400f - 200f;
                VisualOffsetLeft.rectTransform.anchoredPosition = new Vector2(-pos, 0);
                VisualOffsetRight.rectTransform.anchoredPosition = new Vector2(pos, 0);

                float opacity = 1 - Ease.Get(Mathf.Abs(time * 2f - 1f), EaseFunction.Cubic, EaseMode.In);
                VisualOffsetLeft.color = VisualOffsetRight.color = new Color(1, 1, 1, opacity);
            }
        }


        public void IntializeWizard(FloatOptionInput optionInput)
        {
            gameObject.SetActive(true);
            InputHolder.SetActive(true);
            EnhancedTouchSupport.Enable();
            CurrentOptionInput = optionInput;
            CurrentTime = _CumulativeOffset = 0;
            _Samples = 0;
            _LastTrialIndex = -1;
            _TrialOffsets.Clear();
            MedianTimingOffset = 0;
            // Pull the last gameplay-derived median written by PlayerScreen on each hit
            GameplayMedianOffset = CommonSys.sMain.Preferences.Get("PLYR:GameplayMedianOffset", 0f) / 1000.0;
            int gameeplayOffsetCounter = CommonSys.sMain.Preferences.Get("PLYR:GameplayMedianOffsetCounter", 0);
            InfoLabel.gameObject.SetActive(false);

            switch (optionInput){
                case AudioOffsetOptionInput:
                {
                    JudgmentOffsetHolder.SetActive(true);
                    AverageOffsetHolder.SetActive(true);
                    JudgmentOffsetInstructionLabel.gameObject.SetActive(true);
                
                    // Average offset initialisation
                    if (gameeplayOffsetCounter <= 3)
                    {
                        AverageOffsetValueLabel.text = "0";
                        AverageOffsetApplyButton.interactable = false;
                        AverageOffsetInstructionLabel.text = "Play more to get an average offset.";
                        break;
                    }

                    var culture = System.Globalization.CultureInfo.InvariantCulture;
                    
                    bool parsed = double.TryParse(InputField.text,
                        System.Globalization.NumberStyles.Float, culture, out double inputValue);

                    if (!parsed)
                    {
                        AverageOffsetApplyButton.interactable = false;
                        break;
                    }

                    bool isAlreadyPerfect = Math.Abs(GameplayMedianOffset - inputValue) < 0.001;
                    AverageOffsetApplyButton.interactable = !isAlreadyPerfect;
                    AverageOffsetInstructionLabel.text = isAlreadyPerfect
                        ? "Congratulations, your offset is already perfect!"
                        : $"Average offset from the last <b>{gameeplayOffsetCounter}</b> plays:";

                    if (!isAlreadyPerfect)
                        AverageOffsetValueLabel.text = (GameplayMedianOffset * 1000).ToString("0");

                    break;
                }
                case VisualOffsetOptionInput:
                    VisualOffsetHolder.SetActive(true);
                    VisualOffsetInstructionLabel.gameObject.SetActive(true);
                    VisualOffsetLeft.color = VisualOffsetRight.color = new Color(1, 1, 1, 0);

                    break;
            }

            _CurrentAnim?.Skip();
            StartCoroutine(InitializeWizardAnim());
        }

        public void ApplyAverageOffset()
        {
            //Apply
            InputField.text = AverageOffsetValueLabel.text;
            InputField.onEndEdit.Invoke(InputField.text);
            
            // Reset
            CommonSys.sMain.Preferences.Set("PLYR:GameplayMedianOffset", 0);
            CommonSys.sMain.Preferences.Set("PLYR:GameplayMedianOffsetCounter", 0);
            AverageOffsetValueLabel.text = "0";
            AverageOffsetApplyButton.interactable = false;
            AverageOffsetInstructionLabel.text = "Play more to get an average offset.";
        }

        public IEnumerator InitializeWizardAnim()
        {
            _CurrentAnim = Ease.EnumAnimate(
                .45f, x =>
                {
                    float ease = Ease.Get(x * 1.5f, EaseFunction.Cubic, EaseMode.Out);
                    Background.color = new Color(0, 0, 0, .5f * ease);

                    Background.rectTransform.sizeDelta =
                        new Vector2(Background.rectTransform.sizeDelta.x, ease * 100);

                    float ease2 = Ease.Get(
                        x * 1.5f - .5f, EaseFunction.Cubic,
                        EaseMode.Out);

                    FaderGroup.alpha = AverageOffsetCanvasGroup.alpha = ease2;
                });

            yield return _CurrentAnim;

            // --- DSP anchor setup ---
            double dspNow = AudioSettings.dspTime;
            double leadTime = 0.1; // safe scheduling margin

            _SongStartDSP = dspNow + leadTime;

            _IsActive = true;

            if (CurrentOptionInput is AudioOffsetOptionInput)
            {
                CalibrationLoopPlayer.clip = CalibrationLoop;

                // Schedule instead of immediate play
                CalibrationLoopPlayer.PlayScheduled(_SongStartDSP);
            }

            _CurrentAnim = null;
        }


        public void HideWizard()
        {
            _IsActive = false;
            EnhancedTouchSupport.Disable();
            InputField.onEndEdit.RemoveAllListeners();

            _CurrentAnim?.Skip();
            StartCoroutine(HideWizardAnim());
        }

        public IEnumerator HideWizardAnim()
        {
            CalibrationLoopPlayer.Pause();

            _CurrentAnim = Ease.EnumAnimate(
                .3f, x =>
                {
                    float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
                    Background.color = new Color(0, 0, 0, .5f * (1 - ease));

                    Background.rectTransform.sizeDelta =
                        new Vector2(
                            Background.rectTransform.sizeDelta.x,
                            100 * (1 - ease));

                    FaderGroup.alpha = AverageOffsetCanvasGroup.alpha = 1 - ease;
                });
            yield return _CurrentAnim;

            _CurrentAnim = null;
            JudgmentOffsetHolder.SetActive(false);
            AverageOffsetHolder.SetActive(false);
            VisualOffsetHolder.SetActive(false);
            InputHolder.SetActive(false);
            gameObject.SetActive(false);
        }


        public void OnPanelPointerDown(BaseEventData eventData)
        {
            OnPanelPointerDown((PointerEventData)eventData);
        }

        public void OnPanelPointerDown(PointerEventData eventData)
        {
            if (!_IsActive) return;

            if (Touch.activeTouches.Count <= 0) return;

            Touch touch = Touch.activeTouches[0];

            float trialTime = (float)touch.startTime - Time.realtimeSinceStartup + CurrentTime;

            if (CurrentOptionInput is AudioOffsetOptionInput)
                trialTime += SyncOffset;
            else
                trialTime -= CommonSys.sMain.Preferences.Get("PLYR:AudioOffset", 0f) / 1000;

            float trialDuration = 60 / CalibrationLoopBPM * 4;
            int trialIndex = Mathf.FloorToInt(trialTime / trialDuration);
            Debug.Log(trialIndex + " " + trialTime + " " + trialDuration);

            if (_LastTrialIndex == trialIndex)
                return;

            _LastTrialIndex = trialIndex;

            float trialOffset = trialTime - (trialIndex + .5f) * trialDuration;
            _CumulativeOffset += trialOffset;
            _TrialOffsets.Add(trialOffset);
            _Samples++;

            // Compute median from sorted trial offsets
            var sorted = _TrialOffsets.OrderBy(x => x).ToList();
            int mid = sorted.Count / 2;
            MedianTimingOffset = sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2.0
                : sorted[mid];

            if (_Samples < TrialThreshold)
            {
                InfoLabel.text = $"Press {TrialThreshold - _Samples} more times";
            }
            else
            {
                double medianMs = Math.Round(-MedianTimingOffset * 1000);
                InfoLabel.text = $"Median offset: {medianMs:0}ms";
                InputField.text = medianMs.ToString();
                InputField.onEndEdit.Invoke(InputField.text);
            }

            InfoLabel.gameObject.SetActive(true);

            TMP_Text targetLabel = CurrentOptionInput is AudioOffsetOptionInput
                ? JudgmentOffsetInstructionLabel
                : VisualOffsetInstructionLabel;

            InfoLabel.rectTransform.position = targetLabel.rectTransform.position;
            targetLabel.gameObject.SetActive(false);
        }

        public void AddOffset(float value)
        {
            InputField.text = (CurrentOptionInput.CurrentValue + value).ToString();
            InputField.onEndEdit.Invoke(InputField.text);
        }
    }
}