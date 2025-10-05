using System;
using UnityEngine;

namespace JANOARG.Client.UI.Modal
{
    public class ModalAction
    {
        public string Name;
        public Sprite Icon;
        public ModalActionType Type = ModalActionType.Regular;
        public Action Action;
        public bool ClosesModal = true;
    }

    public enum ModalActionType
    {
        Regular = 0,
        Primary = 1,
    }
}