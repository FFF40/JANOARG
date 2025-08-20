using JANOARG.Scripts.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Scripts.Behaviors.Storyteller
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
                global::JANOARG.Scripts.Behaviors.Storyteller.Storyteller.main.PlayScript(ScriptToPlay);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
