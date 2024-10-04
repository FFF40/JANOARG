using System.Collections;
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
        >=  940000 => "AAA+",
        >=  920000 => "AAA",
        >=  900000 => "AA+",
        >=  875000 => "AA",
        >=  850000 => "A+",
        >=  800000 => "A",
        >=  700000 => "B",
        >=  600000 => "C",
        >=       1 => "D",
                 _ => "?"
    };
    public static float GetRating(float constant, float score) => score switch
    {
        >= 1000000 => constant + 6,
        >=  990000 => Mathf.Lerp(constant + 4,                constant + 6,                (score - 990000) / 10000),
        >=  950000 => Mathf.Lerp(constant,                    constant + 4,                (score - 950000) / 40000),
        >=  800000 => Mathf.Lerp(Mathf.Max(constant - 10, 0), constant,                    (score - 800000) / 150000),
        >=  600000 => Mathf.Lerp(0,                           Mathf.Max(constant - 10, 0), (score - 600000) / 200000),
                 _ => 0
    };

    public static bool IsHierarchyObject(object obj)
    {
        return obj 
            is PlayableSong
            or Cover or CoverLayer
            or Chart 
            or CameraController or Pallete
            or LaneStyle or HitStyle 
            or LaneGroup or Lane
            || (obj is IList list && IsHierarchyObject(list[0]))
        ;
    }
}