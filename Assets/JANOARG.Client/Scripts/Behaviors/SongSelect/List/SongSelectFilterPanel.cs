using System;
using System.Collections;
using System.Collections.Generic;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.SongSelect.List
{
    public class SongSelectFilterPanel : MonoBehaviour
    {
        [Header("Data")]
        public SongSortCriteria CurrentSortCriteria;
        public bool SortReversed;

        [Header("Objects")]
        public SongSelectListView ListView;
        [Space]
        public CanvasGroup MainGroup;
        [Space]
        public TMP_Text SortButtonText;
        public Image SortButtonIcon;
        [Space]
        public RectTransform CriteriaHolder;
        public Button[] CriteriaButtons;
        public RectTransform CriteriaCheckmark;
        public Image CriteriaReverseIcon;

        public bool IsShowing { get; private set; }

        Coroutine CurrentAnim;

        public void Start()
        {
            for (int index = 0; index < CriteriaButtons.Length; index++)
            {
                if (!CriteriaButtons[index]) continue;
                var criteria = (SongSortCriteria)index;
                CriteriaButtons[index].onClick.AddListener(() => SetSortCriteria(criteria));
            }
            UpdateSortCriteria();
        }

        public void SetShowing(bool willShow)
        {
            if (SongSelectScreen.sMain.TargetSongAnim != null) return;
            IsShowing = willShow;
            if (CurrentAnim != null) StopCoroutine(CurrentAnim);
            CurrentAnim = StartCoroutine(UpdateShowingAnim(willShow));
        }

        public void UpdateSortCriteria()
        {
            int criteriaIndex = (int)CurrentSortCriteria;
            CriteriaCheckmark.SetParent(CriteriaButtons[criteriaIndex].transform);
            CriteriaCheckmark.anchoredPosition = Vector2.zero;

            string[] sortCriteriaNames = { "Appearance", "Title", "Artist", "Difficulty", "Performance" };
            SortButtonText.text = "Sort by\n" + sortCriteriaNames[criteriaIndex];

            int sortDirection = SortReversed ? -1 : 1;
            CriteriaReverseIcon.transform.localScale = SortButtonIcon.transform.localScale
                = new Vector3(1, sortDirection, 1);
        }

        public void SetSortCriteria(SongSortCriteria targetCriteria)
        {
            if (CurrentSortCriteria == targetCriteria) return;
            CurrentSortCriteria = targetCriteria;
            UpdateSortCriteria();
            ListView.DoSortAnim();
        }

        public void ToggleSortReverse()
        {
            SortReversed = !SortReversed;
            UpdateSortCriteria();
            ListView.DoSortAnim();
        }

        // -------------------- Animations

        public IEnumerator UpdateShowingAnim(bool willShow)
        {
            MainGroup.interactable = MainGroup.blocksRaycasts = willShow;

            float startHolderPivotY = CriteriaHolder.pivot.y;
            float endHolderPivotY = willShow ? 0 : 1;

            yield return Ease.Animate(.3f, (t) =>
            {
                float ease1 = Ease.Get(t, EaseFunction.Exponential, EaseMode.Out);
                CriteriaHolder.pivot *= new Vector2Frag(y: Mathf.Lerp(startHolderPivotY, endHolderPivotY, ease1));

                float ease2 = Ease.Get(t * 1.5f, EaseFunction.Cubic, EaseMode.Out);
                if (willShow) ease2 = 1 - ease2;
                SongSelectScreen.sMain.LerpUI(ease2);
            });

            CurrentAnim = null;
        }
    }

    public enum SongSortCriteria
    {
        Appearance = 0,
        Title = 1,
        Artist = 2,
        Difficulty = 3,
        Performance = 4
    }
}