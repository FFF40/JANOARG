using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways][RequireComponent(typeof(CanvasRenderer))]
public class GraphicParallelogram : MaskableGraphic
{
    [Space][Range(-90, 90)]
    public float Slant = 15;

    public bool ExpandStart;
    public bool ExpandEnd;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Vector2 min = rectTransform.rect.min;
        Vector2 max = rectTransform.rect.max;

        float tan = Mathf.Tan(Slant * Mathf.Deg2Rad);
        float offset = tan * (max.y - min.y);

        vh.Clear();
        
        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;
        vert.position = new(min.x + (ExpandStart ? Mathf.Min(-offset, 0) : Mathf.Max(-offset, 0)), min.y); 
        vh.AddVert(vert);
        vert.position = new(min.x + (ExpandStart ? Mathf.Min(offset, 0) : Mathf.Max(offset, 0)), max.y); 
        vh.AddVert(vert);
        vert.position = new(max.x + (ExpandEnd ? Mathf.Max(-offset, 0) : Mathf.Min(-offset, 0)), min.y);
        vh.AddVert(vert);
        vert.position = new(max.x + (ExpandEnd ? Mathf.Max(offset, 0) : Mathf.Min(offset, 0)), max.y); 
        vh.AddVert(vert);
        vh.AddTriangle(0, 2, 3);
        vh.AddTriangle(0, 3, 1);
    }
}
