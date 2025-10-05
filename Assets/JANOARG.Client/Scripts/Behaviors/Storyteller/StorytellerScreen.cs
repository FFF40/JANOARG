using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Data.Story;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Storyteller
{
    public class StorytellerScreen : MonoBehaviour
    {
        public static StorytellerScreen sMain;

        public StoryScript ScriptToPlay;

        public void Awake()
        {
            sMain = this;
            CommonScene.Load();
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (ScriptToPlay) Storyteller.sMain.PlayScript(ScriptToPlay);
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}
