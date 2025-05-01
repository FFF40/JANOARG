

using UnityEngine;

public struct ColorFrag 
{
    public float? r { get; set; }
    public float? g { get; set; }
    public float? b { get; set; }
    public float? a { get; set; }

    public ColorFrag(float? r = null, float? g = null, float? b = null, float? a = null) 
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public static Color operator *(Color left, ColorFrag right)
    {
        if (right.r.HasValue) left.r = right.r.Value;
        if (right.g.HasValue) left.g = right.g.Value;
        if (right.b.HasValue) left.b = right.b.Value;
        if (right.a.HasValue) left.a = right.a.Value;
        return left;
    }
}