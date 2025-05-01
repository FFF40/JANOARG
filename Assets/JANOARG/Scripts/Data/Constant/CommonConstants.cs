
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Common Constants", menuName = "JANOARG/Common Constants")]
public class CommonConstants : ScriptableObject
{
    public List<Color> DifficultyColors;
    public Color SpecialDifficultyColor;

    public Color GetDifficultyColor(int index)
    {
        return (index < 0 || index >= DifficultyColors.Count)
            ? SpecialDifficultyColor
            : DifficultyColors[index];
    }
}