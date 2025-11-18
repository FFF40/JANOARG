
using System.Collections;
using System.Data;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItems;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs
{
    public class SongMapItemUI : MapItemUI<SongMapItem>
    {
        public RawImage CoverImage;
        public GameObject LockedIndicator;

        public override void SetParent(SongMapItem parent)
        {
            base.SetParent(parent);
            UpdateStatus();
            LoadCoverImage();
        }


        public void UpdateStatus()
        {
            if (!parent.isRevealed)
            {
                if (CoverImage.texture) SongSelectCoverManager.sMain.UnregisterUse(CoverImage);
                gameObject.SetActive(false);
                return;
            }
            if (!parent.isUnlocked)
            {
                if (CoverImage.texture) SongSelectCoverManager.sMain.UnregisterUse(CoverImage);
                LockedIndicator.SetActive(true);
                CoverImage.gameObject.SetActive(false);
                return;
            }

            LockedIndicator.SetActive(false);
            CoverImage.gameObject.SetActive(true);
            if (!CoverImage.texture && CoverLoadRoutine == null) LoadCoverImage();
            gameObject.SetActive(true);
        }

        public Coroutine CoverLoadRoutine = null;
        public void LoadCoverImage()
        {
            if (!parent.isRevealed) return;
            if (CoverLoadRoutine != null) StopCoroutine(CoverLoadRoutine);
            CoverLoadRoutine = StartCoroutine(LoadCoverImageRoutine());
        }
        private IEnumerator LoadCoverImageRoutine()
        {
            yield return SongSelectCoverManager.sMain.RegisterUseSong(CoverImage, parent.TargetID);
        }

        public void LerpToListItem(RectTransform cover, float t)
        {
            Vector3 fromPos = CommonSys.sMain.MainCamera.WorldToScreenPoint(parent.transform.position);
            Vector3 toPos = cover.position;
            (transform as RectTransform).position = Vector3.Lerp(fromPos, toPos, t);
        }

        public void OnClick()
        {
            MapManager.sMain.SelectSong(parent);
        }
    }
}