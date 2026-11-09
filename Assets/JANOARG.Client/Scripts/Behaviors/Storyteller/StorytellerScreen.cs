using System.Collections;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Data.Story;
using UnityEngine;
using UnityEngine.UIElements;

namespace JANOARG.Client.Behaviors.Storyteller
{
    public class StorytellerScreen : MonoBehaviour
    {
        public static StorytellerScreen sMain;
        public static StoryScript sScriptToPlay;
        public bool IsInit;

        private void Awake()
        {
            sMain = this;
            IsInit = false;
            CommonScene.Load();
        }

        // Start is called before the first frame update
        private IEnumerator Start()
        {
            if (sScriptToPlay == null)
                yield break;

            yield return null; 

            if (Storyteller.sMain == null)
                yield break;

            Storyteller.sMain.CurrentScript = sScriptToPlay;
            Storyteller.sMain.PlayScript(sScriptToPlay);

            IsInit = true;
        }
    }
}
