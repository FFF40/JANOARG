
using System.Collections;
using System.Data;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItems;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs
{
    public class PlaylistMapItemUI : MapItemUI<PlaylistMapItem>
    {
        public RawImage CoverImage;

        public override void SetParent(PlaylistMapItem parent)
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

        public void OnClick()
        {
            // TODO show playlist info when clicked
        }
    }
}