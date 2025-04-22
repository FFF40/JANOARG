

using UnityEngine;

public struct Vector3Frag 
{
    public float? x { get; set; }
    public float? y { get; set; }
    public float? z { get; set; }

    public Vector3Frag(float? x = null, float? y = null, float? z = null) 
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3 operator *(Vector3 left, Vector3Frag right)
    {
        if (right.x.HasValue) left.x = right.x.Value;
        if (right.y.HasValue) left.y = right.y.Value;
        if (right.z.HasValue) left.z = right.z.Value;
        return left;
    }
}