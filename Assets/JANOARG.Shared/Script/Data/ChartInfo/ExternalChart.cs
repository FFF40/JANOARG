using UnityEngine;

namespace JANOARG.Shared.Script.Data.ChartInfo
{
    public class ExternalChart : ScriptableObject
    {
        public Chart Data;
    }

    [System.Serializable]
    public class ExternalChartMeta
    {
        public string Target;

        public string DifficultyName = "Normal";
        public string DifficultyLevel = "6";
        public int DifficultyIndex = 1;
        public float ChartConstant = 6;

        public string CharterName = "";
        public string AltCharterName = "";
    }
}