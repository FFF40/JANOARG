

using UnityEngine;

public struct Vector2Frag 
{
    public float? x { get; set; }
    public float? y { get; set; }

    public Vector2Frag(float? x = null, float? y = null) 
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2 operator *(Vector2 left, Vector2Frag right)
    {
        if (right.x.HasValue) left.x = right.x.Value;
        if (right.y.HasValue) left.y = right.y.Value;
        return left;
    }
}