using JANOARG.Client.Scripts.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Client.Scripts.Behaviors.Intro
{
    public class IntroScene : MonoBehaviour
    {

        public void Awake()
        {
            CommonScene.Load();
        }
    }
}