
using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItems;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs
{
    public class PlaylistMapItemUI : MapItemUI<PlaylistMapItem>
    {
        public RawImage CoverImage;
        [Space]
        public RectTransform[] ParallaxLayers;
        
        public override void SetParent(PlaylistMapItem parent)
        {
            base.SetParent(parent);
            UpdateStatus();
            LoadCoverImage();
            CoverImage.color = parent.Target.Playlist.BackgroundColor;
        }


        public void UpdateStatus()
        {
            if (!parent.isRevealed)
            {
                if (CoverImage.texture) SongSelectCoverManager.sMain.UnregisterUse(CoverImage);
                gameObject.SetActive(false);
                return;
            }

            if (!CoverImage.texture && CoverLoadRoutine == null) LoadCoverImage();
            gameObject.SetActive(true);
        }

        public override void UpdatePosition()
        {
            base.UpdatePosition();
            Vector2 basePosition = (transform as RectTransform).anchoredPosition;
            float strength = 1;
            for (int i = 0; i < ParallaxLayers.Length; i++)
            {
                strength *= 0.98f;
                ParallaxLayers[i].anchoredPosition = basePosition * (strength - 1);
            }
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
            SongSelectCoverManager.sMain.UnregisterUse(CoverImage);
            yield return null; // SongSelectCoverManager.sMain.RegisterUseSong(CoverImage, parent.TargetID);
        }

        public void OnClick()
        {
            MapManager.sMain.NavigateToMap(parent.Target);
        }
    }
}