using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.Story;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Storyteller
{
    public class StorytellerScreen : MonoBehaviour
    {
        public static StorytellerScreen main;

        public StoryScript ScriptToPlay;

        public void Awake()
        {
            main = this;
            CommonScene.Load();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (ScriptToPlay) 
            {
                global::JANOARG.Client.Behaviors.Storyteller.Storyteller.main.PlayScript(ScriptToPlay);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
