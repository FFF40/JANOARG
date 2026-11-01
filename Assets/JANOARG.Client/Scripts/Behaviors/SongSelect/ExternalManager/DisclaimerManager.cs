using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.UI.Modal;
using UnityEngine;

namespace JANOARG.Client.Behaviors.SongSelect.ExternalManager
{
    public class DisclaimerManager : MonoBehaviour
    {
        public void Awake()
        {
            if (!CommonSys.sMain.Storage.Get("EXTRNL:HasReadDisclaimer", false))
                SpawnDisclaimer();
        }

        public void SpawnDisclaimer()
        {
            const string BODY =
                "You've discovered the external chart loading feature!\n" +
                "This feature is intended for testing charts that you, or others you know, \n" +
                "have created using the JANOARG Chartmaker software.\n\n" +
                "However, this goes without saying that any songs submitted here DOES NOT mean it is officially licensed by us,\n" +
                "as it is added as <b>user-submitted</b> creations and is not subject to our management." +
                "\n\n" +
                "Scores for external charts will not be saved or uploaded to your profile." +
                "\n\n" +
                "Finally, we disclaim any responsibility for damages resulting from the use of this feature.";

            ModalManager.sInstance.Spawn(
                "DISCLAIMER: External Loader Discovered!",
                BODY,
                new[]
                {
                    new ModalAction()
                    {
                        Name = "Return",
                        Icon = ProfileBar.sMain.ArrowLeftIcon,
                        ClosesModal = true,
                        Action = () => SongSelectScreen.sMain.Return()
                    }
                },
                new[]
                {
                    new ModalAction()
                    {
                        Name = "Proceed",
                        Icon = Resources.Load<Sprite>("Sprites/Icons/Common/Checkmark"),
                        ClosesModal = true,
                        Type = ModalActionType.Primary,
                        Action = () =>
                        {
                            CommonSys.sMain.Storage.Set("EXTRNL:HasReadDisclaimer", true);
                            CommonSys.sMain.Storage.Save();
                        }
                    }
                }
            );
        }
    }
}