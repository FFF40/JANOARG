using TMPro;
using UnityEngine;

namespace JANOARG.Client.UI.Modal
{
    public class ModalManager : MonoBehaviour
    {
        public static ModalManager sInstance { get; private set; }

        public RectTransform ModalHolder;
        public TMP_Text LabelBody;
        public Modal ModalSample;
        public ModalActionButton[] LeftActionSamples;
        public ModalActionButton[] RightActionSamples;

        public void Awake()
        {
            sInstance = this;
        }

        public void Spawn(string title, string body, ModalAction[] leftActions = null, ModalAction[] rightActions = null)
        {
            LabelBody.text = body;
            Spawn(title, LabelBody.gameObject, leftActions, rightActions);
        }

        public void Spawn(string title, GameObject body, ModalAction[] leftActions = null, ModalAction[] rightActions = null, bool cloneBody = true)
        {
            Modal modal = Instantiate(ModalSample, ModalHolder);

            modal.TitleLabel.text = title;
            if (cloneBody) Instantiate(body, modal.BodyHolder);
            else body.transform.SetParent(modal.BodyHolder);

            if (leftActions != null) foreach (ModalAction action in leftActions)
            {
                ModalActionButton button = Instantiate(LeftActionSamples[(int)action.Type], modal.LeftActionsHolder);
                if (modal.LeftActionsHolder.childCount <= 1) button.ContentLayoutGroup.padding.left += 1000;
                SetupModalAction(modal, action, button);
            }
            if (rightActions != null) foreach (ModalAction action in rightActions)
            {
                ModalActionButton button = Instantiate(RightActionSamples[(int)action.Type], modal.RightActionsHolder);
                if (modal.RightActionsHolder.childCount <= 1) button.ContentLayoutGroup.padding.right += 1000;
                SetupModalAction(modal, action, button);
            }
        }

        private static void SetupModalAction(Modal modal, ModalAction action, ModalActionButton button)
        {
            button.IconImage.sprite = action.Icon;
            button.Label.text = action.Name;
            button.Button.onClick.AddListener(() =>
            {
                action.Action?.Invoke();
                if (action.ClosesModal) modal.Close();
            });
        }
    }
}