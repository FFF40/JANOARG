using UnityEngine;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

internal struct TouchSample
{
    public int        FingerIndex;
    public double     Time;
    public Vector2    Position;
    public TouchPhase Phase;
}

