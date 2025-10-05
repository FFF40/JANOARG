
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

        public override void SetParent(SongMapItem parent)
        {
            base.SetParent(parent);
            UpdateStatus();
            LoadCoverImage();
        }


        public void UpdateStatus()
        {
            if (!Parent.IsRevealed)
            {
                if (CoverImage.texture) SongSelectCoverManager.sMain.UnregisterUse(CoverImage);
                gameObject.SetActive(false);
                return;
            }

            if (!CoverImage.texture && CoverLoadRoutine == null) LoadCoverImage();
            gameObject.SetActive(true);
        }



        public Coroutine CoverLoadRoutine = null;
        public void LoadCoverImage()
        {
            if (!Parent.IsRevealed) return;
            if (CoverLoadRoutine != null) StopCoroutine(CoverLoadRoutine);
            CoverLoadRoutine = StartCoroutine(LoadCoverImageRoutine());
        }
        private IEnumerator LoadCoverImageRoutine()
        {
            SongSelectCoverManager.sMain.UnregisterUse(CoverImage);
            yield return SongSelectCoverManager.sMain.RegisterUse(CoverImage, Parent.TargetID);
        }

        public void LerpToListItem(RectTransform cover, float t)
        {
            Vector3 fromPos = CommonSys.sMain.MainCamera.WorldToScreenPoint(Parent.transform.position);
            Vector3 toPos = cover.position;
            (transform as RectTransform).position = Vector3.Lerp(fromPos, toPos, t);
        }

        public void OnClick()
        {
            MapManager.main.SelectSong(Parent);
        }
    }
}