
using System.Collections;
using System.Data;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItems;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs
{
    public class StoryMapItemUI : MapItemUI<StoryMapItem>
    {
        public RawImage CoverImage;
        public GameObject LockedIndicator;

        public override void SetParent(StoryMapItem parent)
        {
            base.SetParent(parent);
            UpdateStatus();
        }

        public void UpdateStatus()
        {
            if (!parent.isRevealed)
            {
                gameObject.SetActive(false);
                return;
            }
            if (!parent.isUnlocked)
            {
                LockedIndicator.SetActive(true);
                CoverImage.gameObject.SetActive(false);
                return;
            }

            LockedIndicator.SetActive(false);
            CoverImage.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        public void OnClick()
        {
            StartCoroutine(MapManager.sMain.LoadStory(parent));
        }
    }
}