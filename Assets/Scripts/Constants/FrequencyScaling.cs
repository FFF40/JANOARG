using System;
using UnityEngine;

public static class FrequencyScaling
{
    public static void GetScalingFunctions(FrequencyScale scaling, out Func<float, float> scale, out Func<float, float> unscale)
    {
        switch (scaling)
        {
            case FrequencyScale.Logarithmic:
                scale = (x) => Mathf.Log10(1 + x);
                unscale = (x) => Mathf.Pow(10, x) - 1;
                break;
            case FrequencyScale.Mel:
                scale = (x) => 2595 * Mathf.Log10(1 + x / 700);
                unscale = (x) => 700 * Mathf.Pow(10, x / 2595) - 700;
                break;
            case FrequencyScale.Bark:
                scale = (x) => 6 * (float)Math.Asinh(x / 600);
                unscale = (x) => (float)Math.Sinh(x / 6) * 600;
                break;
            case FrequencyScale.Linear: default:
                scale = (x) => x;
                unscale = (x) => x;
                break;
        }
    }

    public static float AWeight(float freq)
    {
        return Mathf.Pow(12194 * freq * freq, 2)
            / (freq * freq + 424.36f)
            / Mathf.Sqrt((freq * freq + 11599.29f) * (freq * freq + 544496.41f))
            / (freq * freq + 148693636);
    }
}