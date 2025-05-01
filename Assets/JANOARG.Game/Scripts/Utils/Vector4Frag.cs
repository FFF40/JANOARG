

using UnityEngine;

public struct Vector4Frag 
{
    public float? x { get; set; }
    public float? y { get; set; }
    public float? z { get; set; }
    public float? w { get; set; }

    public Vector4Frag(float? x = null, float? y = null, float? z = null, float? w = null) 
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static Vector4 operator *(Vector4 left, Vector4Frag right)
    {
        if (right.x.HasValue) left.x = right.x.Value;
        if (right.y.HasValue) left.y = right.y.Value;
        if (right.z.HasValue) left.z = right.z.Value;
        if (right.w.HasValue) left.w = right.w.Value;
        return left;
    }
}