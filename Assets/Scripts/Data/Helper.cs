using UnityEngine;

public static class Helper 
{
    public static string GetRank(float score) => score switch
    {
        >= 1000000 => "1",
        >=  995000 => "SSS+",
        >=  990000 => "SSS",
        >=  980000 => "SS+",
        >=  970000 => "SS",
        >=  960000 => "S+",
        >=  950000 => "S",
        >=  920000 => "AAA",
        >=  900000 => "AA",
        >=  850000 => "A",
        >=  800000 => "B",
        >=  700000 => "C",
        >=       1 => "D",
                 _ => "?"
    };
    public static float GetRating(float constant, float score) => score switch
    {
        >= 1000000 => constant + 6,
        >=  990000 => Mathf.Lerp(constant + 4,                constant + 6,                (score - 990000) / 10000),
        >=  950000 => Mathf.Lerp(constant,                    constant + 4,                (score - 950000) / 40000),
        >=  850000 => Mathf.Lerp(Mathf.Max(constant - 10, 0), constant,                    (score - 850000) / 100000),
        >=  700000 => Mathf.Lerp(0,                           Mathf.Max(constant - 10, 0), (score - 700000) / 150000),
                 _ => 0
    };

    public static string FormatDifficulty(string str)
    {
        if (str.EndsWith("*")) str = str[..^1] + "<sub>*</sub>";
        return str;
    }
}