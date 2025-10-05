using JANOARG.Client.Data.Storage;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Common
{
    public class StorageManager : MonoBehaviour
    {
        public static StorageManager sMain;

        public ScoreStore Scores = new();
        public FlagStore Flags = new();

        private void Awake()
        {
            sMain = this;
            Load();
        }

        public void Load()
        {
            Scores.Load();
        }

        public void Save()
        {
            Scores.Save();
            CommonSys.sMain.Storage.Save();
        }
    }
}