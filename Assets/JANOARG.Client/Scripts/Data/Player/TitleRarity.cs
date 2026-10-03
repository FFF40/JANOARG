using UnityEngine;
using UnityEditor;

namespace JANOARG.Client.Data.Player
{
    [CreateAssetMenu(fileName = "Player Title", menuName = "JANOARG/Player/Title Rarities")]
    public class TitleRarity : ScriptableObject
    {
        // In ascending order by difficulty to achieve  
        public string Name;
        public Color Color;
        public int Value;

        #if UNITY_EDITOR

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;

            string newName = $"{Value} - {Name}";

            if (name == newName)
                return;

            string assetPath = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(assetPath))
                return;

            string error = AssetDatabase.RenameAsset(assetPath, newName);

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Failed to rename asset: {error}");
                return;
            }

            name = newName;
            EditorUtility.SetDirty(this);
        }
        #endif
    }

    
}