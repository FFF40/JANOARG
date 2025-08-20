using JANOARG.Client.Scripts.Data.Storage;
using UnityEngine;

namespace JANOARG.Client.Scripts.Behaviors.Common
{
    public class StorageManager : MonoBehaviour
    {
        public static StorageManager main;

        public ScoreStore Scores = new();

        void Awake()
        {
            main = this;
            Load();
        } 

        public void Load()
        {
            Scores.Load();
        }

        public void Save() 
        {
            Scores.Save();
            global::JANOARG.Client.Scripts.Behaviors.Common.CommonSys.main.Storage.Save();
        }
    }
}