using JANOARG.Client.Scripts.Behaviors.Common;
using JANOARG.Shared.Scripts.Data.Story;
using UnityEngine;

namespace JANOARG.Client.Scripts.Behaviors.Storyteller
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
                global::JANOARG.Client.Scripts.Behaviors.Storyteller.Storyteller.main.PlayScript(ScriptToPlay);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
