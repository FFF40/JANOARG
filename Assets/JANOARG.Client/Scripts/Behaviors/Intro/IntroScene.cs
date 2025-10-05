using JANOARG.Client.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Intro
{
    public class IntroScene : MonoBehaviour
    {
        public void Awake()
        {
            CommonScene.Load();
        }
    }
}