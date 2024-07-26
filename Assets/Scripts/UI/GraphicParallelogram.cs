using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways][RequireComponent(typeof(CanvasRenderer))]
public class GraphicParallelogram : MaskableGraphic
{
    [Space]
    [Range(-90, 90)] public float SlantStart = 15;
    [Range(-90, 90)] public float SlantEnd = 15;

    public bool ExpandStart;
    public bool ExpandEnd;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Vector2 min = rectTransform.rect.min;
        Vector2 max = rectTransform.rect.max;

        float tanStart = Mathf.Tan(SlantStart * Mathf.Deg2Rad);
        float offsetStart = tanStart * (max.y - min.y);
        float tanEnd = Mathf.Tan(SlantEnd * Mathf.Deg2Rad);
        float offsetEnd = tanEnd * (max.y - min.y);

        vh.Clear();
        
        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;
        vert.position = new(min.x + (ExpandStart ? Mathf.Min(-offsetStart, 0) : Mathf.Max(-offsetStart, 0)), min.y); 
        vh.AddVert(vert);
        vert.position = new(min.x + (ExpandStart ? Mathf.Min(offsetStart, 0) : Mathf.Max(offsetStart, 0)), max.y); 
        vh.AddVert(vert);
        vert.position = new(max.x + (ExpandEnd ? Mathf.Max(-offsetEnd, 0) : Mathf.Min(-offsetEnd, 0)), min.y);
        vh.AddVert(vert);
        vert.position = new(max.x + (ExpandEnd ? Mathf.Max(offsetEnd, 0) : Mathf.Min(offsetEnd, 0)), max.y); 
        vh.AddVert(vert);
        vh.AddTriangle(0, 2, 3);
        vh.AddTriangle(0, 3, 1);
    }
}
