using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cursor", menuName = "Cursor Definition", order = 100)]
public class CursorDefinition : ScriptableObject
{
    public CursorType CursorType;
    public Vector2 Pivot;
    public List<CursorFrame> Frames;
}

[Serializable]
public class CursorFrame 
{
    public Texture2D Texture;
    public float Duration = 0.33f;
}