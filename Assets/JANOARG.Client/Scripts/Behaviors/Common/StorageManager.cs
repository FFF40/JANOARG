using JANOARG.Client.Data.Storage;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Common
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
            CommonSys.main.Storage.Save();
        }
    }
}