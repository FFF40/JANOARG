using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;
using JANOARG.Client.Behaviors.SongSelect.List.ListItems;

namespace JANOARG.Client.Behaviors.SongSelect.List.ListItemUIs
{
    public class SongSelectListHeaderUI : SongSelectItemUI<SongSelectListHeader>
    {
        public TMP_Text HeaderLabel;
        public CanvasGroup MainGroup;

        public void SetItem(SongSelectListHeader target) 
        {
            Target = target;
            HeaderLabel.text = target.Header;
        }

        public void SetVisibility(float a)
        {
            MainGroup.blocksRaycasts = a == 1;
            MainGroup.alpha = a;
        }
    }
}