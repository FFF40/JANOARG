using JANOARG.Scripts.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Scripts.Behaviors.Intro
{
    public class IntroScene : MonoBehaviour
    {

        public void Awake()
        {
            CommonScene.Load();
        }
    }
}